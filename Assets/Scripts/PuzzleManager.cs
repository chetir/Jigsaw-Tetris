using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;


public class PuzzleItem
{
    public Vector2Int position;
    public ShapeInt shape;
    public float scoreBase;
    private HashSet<Vector2Int> availablePoints; // InLocal

    public PuzzleItem(Vector2Int pos, ShapeInt sha) {
        position = pos;
        shape = sha;
    }

    public void InitPoints() {
        availablePoints = new HashSet<Vector2Int>();
        for (int i = 0; i < shape.matrix.Count; i++) {
            availablePoints.Add(shape.matrix[i]);
        }
    }

    public void InitScoreBase(float val = 1.0f) {
        scoreBase = val * shape.matrix.Count;
    }

    public void Set(Vector2Int worldPoint) {
        availablePoints.Remove(worldPoint - position);
    }

    public void Set(ShapeInt tetromino, Vector2Int worldPoint) {
        for (int i = 0; i < tetromino.matrix.Count; i++) {
            availablePoints.Remove(worldPoint - position + tetromino.matrix[i]);
        }
    }

    public bool Contains(Vector2Int worldPoint) {
        return availablePoints.Contains(worldPoint - position);
    }

    public bool IsPuzzleFull() {
        return availablePoints.Count == 0;
    }
}

public class PuzzleManager : MonoBehaviour
{
    private readonly int Size = 3;
    private readonly List<int> IdxProbs = new List<int>{0, 1, 1};
    private readonly int[,] dirs = {{-1, 0}, {0, -1}, {1, 0}, {0, 1}};

    [SerializeField]
    private Board board;
    [SerializeField]
    private Tile puzzleTile;
    [SerializeField]
    private PreviewLayer _previewLayer;

    private PuzzleItem activePuzzle;
    private ShapeInt nextShape;
    public float stepDelay = 3f;
    // public float moveDelay = 0.1f;
    public float lockDelay = 0.5f;

    private float stepTime;
    // private float moveTime;
    private float lockTime;
    

    void Start() {
        GenerateNextPuzzle();
        SetActivePuzzle();
        GenerateNextPuzzle();
    }

    void OnEnable() {
        GameInput.InputControl.Instance.GamePlay.RotateClockwise.performed += OnRotateClockwiseActionPerformed;
        GameInput.InputControl.Instance.GamePlay.RotateCounterClockwise.performed += OnRotateCounterClockwiseActionPerformed;
        GameInput.InputControl.Instance.GamePlay.HardDrop.performed += OnHardDropPerformed;
        GameInput.InputControl.Instance.GamePlay.MoveActions.performed += OnMoveActionsPerformed;
    }

    void OnDisable() {
        GameInput.InputControl.Instance.GamePlay.RotateClockwise.performed -= OnRotateClockwiseActionPerformed;
        GameInput.InputControl.Instance.GamePlay.RotateCounterClockwise.performed -= OnRotateCounterClockwiseActionPerformed;
        GameInput.InputControl.Instance.GamePlay.HardDrop.performed -= OnHardDropPerformed;
        GameInput.InputControl.Instance.GamePlay.MoveActions.performed -= OnMoveActionsPerformed;
    }

    private void OnRotateClockwiseActionPerformed(InputAction.CallbackContext context)
    {
        if (activePuzzle.shape == null) return;
        var shape = activePuzzle.shape.Clone();
        shape.Rotate(RotateAngle.Clockwise90);
        Rotate(shape);
    }

    private void OnRotateCounterClockwiseActionPerformed(InputAction.CallbackContext context)
    {
        if (activePuzzle.shape == null) return;
        var shape = activePuzzle.shape.Clone();
        shape.Rotate(RotateAngle.CounterClockwise90);
        Rotate(shape);
    }

    private void OnHardDropPerformed(InputAction.CallbackContext context)
    {
        HardDrop();
    }

    private void OnMoveActionsPerformed(InputAction.CallbackContext context)
    {
        var dir = context.ReadValue<Vector2>();
        if (dir.y < 0) {
            if (Move(dir)) {
                // Update the step time to prevent double movement
                stepTime = Time.time + stepDelay;
            }
        } else {
            Move(dir);
        }
    }

    private void Update()
    {
        // board.ClearTile(activePuzzle.shape, activePuzzle.position);
        // // We use a timer to allow the player to make adjustments to the piece
        // // before it locks in place
        lockTime += Time.deltaTime;

        // // Allow the player to hold movement keys but only after a move delay
        // // so it does not move too fast
        // // if (Time.time > moveTime) {
        //     // HandleMoveInputs();
        // // }

        // // Advance the piece to the next row every x seconds
        if (Time.time > stepTime) {
            Step();
        }
        // board.SetTile(puzzleTile, activePuzzle.shape, activePuzzle.position);
    }

    public void ChangeStepDelay(float val) {
        stepDelay = 5.0f / val;
    }

    private void Step()
    {
        stepTime = Time.time + stepDelay;

        // Step down to the next row
        Move(Vector2Int.down);

        // Once the piece has been inactive for too long it becomes locked
        if (lockTime >= lockDelay) {
            Lock();
        }
    }

    private void HardDrop()
    {
        while (Move(Vector2Int.down)) {
            continue;
        }

        Lock(2.0f);
    }

    private void Lock(float val = 1.0f)
    {
        Debug.Log("Lock");
        activePuzzle.InitPoints();
        activePuzzle.InitScoreBase(val);
        board.SetPuzzleItem(activePuzzle);
        SetActivePuzzle();
        GenerateNextPuzzle();
    }

    private bool Move(Vector2 translation)
    {
        board.ClearTile(activePuzzle.shape, activePuzzle.position);
        Vector2Int newPosition = activePuzzle.position;
        newPosition.x += (int)translation.x;
        newPosition.y += (int)translation.y;

        bool valid = board.IsValidPuzzlePosition(activePuzzle.shape, newPosition);

        // Only save the movement if the new position is valid
        if (valid)
        {
            activePuzzle.position = newPosition;
            // moveTime = Time.time + moveDelay;
            lockTime = 0f; // reset
        }
        board.SetPuzzleTile(puzzleTile, activePuzzle.shape, activePuzzle.position);

        return valid;
    }

    private bool Rotate(ShapeInt shape)
    {
        board.ClearTile(activePuzzle.shape, activePuzzle.position);

        bool valid = board.IsValidPuzzlePosition(shape, activePuzzle.position);

        // Only save the movement if the new position is valid
        if (valid)
        {
            activePuzzle.shape = shape;
            // moveTime = Time.time + moveDelay;
            lockTime = 0f; // reset
        }
        board.SetPuzzleTile(puzzleTile, activePuzzle.shape, activePuzzle.position);

        return valid;
    }

    private void GenerateNextPuzzle() {
        int[,] points = new int[Size, Size];
        for (int i = 0; i < Size; i++){
            for (int j = 0; j < Size; j++) {
                points[i, j] = -1;
            }
        }
        Generate(Random.Range(0, Size), Random.Range(0, Size), 1, ref points);
        nextShape = new(points);
        _previewLayer.ShowNextPuzzlePreview(nextShape);
    }

    private void SetActivePuzzle() {
        Vector2Int position = new(board.boardTopLeft.x - nextShape.MinX, board.boardTopLeft.y - nextShape.MaxY);
        activePuzzle = new PuzzleItem(position, nextShape);
        stepTime = Time.time + stepDelay;
        // moveTime = Time.time + moveDelay;
        lockTime = 0f;
        if (board.IsValidPuzzlePosition(activePuzzle.shape, activePuzzle.position)) {
            board.SetPuzzleTile(puzzleTile, activePuzzle.shape, position);
        } else {
            board.GameOver();
            // StartCoroutine(CountDown(activePuzzle.shape, activePuzzle.position));
        }
    }

    IEnumerator CountDown(ShapeInt shape, Vector2Int coords) {
        for (int i = 0; i < stepDelay; i++) {
            if (!board.IsValidPuzzlePosition(shape, coords)) {
                yield return new WaitForSeconds(1.0f);
            } else {
                board.SetPuzzleTile(puzzleTile, shape, coords);
                break;
            }

            if (i == 4) {
                board.GameOver();
            }
        }
        yield return null;
    } 

    bool Generate(int x, int y, int val, ref int[,] points) {
        points[x, y] = val;

        if (val == 0) return true;

        int frameIdx = 0;
        for (int i = 0; i < dirs.GetLength(0); i++) {
            int nextX = x + dirs[i, 1];
            int nextY = y + dirs[i, 0];
            if (nextX >= Size || nextX < 0 || nextY >= Size || nextY < 0 || points[nextX, nextY] == 0) {
                frameIdx |= 1 << i;
                continue;
            }
            if (points[nextX, nextY] > 0) {
                continue;
            }

            List<int> validIdxProbs = new List<int>(IdxProbs);
            int nextVal = IdxProbs[Random.Range(0, validIdxProbs.Count)];
            while(!Generate(nextX, nextY, nextVal, ref points)) {
                validIdxProbs.Remove(nextVal);
                if (validIdxProbs.Count == 0) {
                    return false;
                }
                nextVal = IdxProbs[Random.Range(0, validIdxProbs.Count)];
            }
            if (nextVal == 0) {
                frameIdx |= 1 << i;
            }
        }
        if (frameIdx == 0) {
            points[x, y] = 16;
        } else {
            points[x, y] = frameIdx;
        }
        
        return true;
    }
}
