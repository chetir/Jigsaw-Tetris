using System.Collections;
using System.Collections.Generic;
using GameInput;
using UnityEngine;
using UnityEngine.InputSystem;

public class TetrisManager : MonoBehaviour
{
    [SerializeField]
    private TetrisItems tetrisItems;

    private int nextIdx = -1;
    private int activeIdx = -1;
    private ShapeInt shape;

    [SerializeField]
    private MouseUser _mouseUser;

    [SerializeField]
    private PreviewLayer _previewLayer;

    [SerializeField]
    private Board board;

    private void OnEnable() {
        FirstInitRandomTetromino();
        GameInput.InputControl.Instance.GamePlay.TetrominoRotate.performed += OnRotatePerformed;
        GameInput.InputControl.Instance.GamePlay.FlipAction.performed += OnFlipActionPerformed;
    }

    private void OnDisable() {
        GameInput.InputControl.Instance.GamePlay.TetrominoRotate.performed -= OnRotatePerformed;
        GameInput.InputControl.Instance.GamePlay.FlipAction.performed -= OnFlipActionPerformed;
    }

    private void OnRotatePerformed(InputAction.CallbackContext context)
    {
        var dir = context.ReadValue<float>();
        if (dir > 0) {
            shape?.Rotate(RotateAngle.Clockwise90);
        } else if (dir < 0) {
            shape?.Rotate(RotateAngle.CounterClockwise90);
        }
    }

    private void OnFlipActionPerformed(InputAction.CallbackContext ctx) {
        shape?.Filp(0);
    }

    public void FirstInitRandomTetromino() {
        int randIdx = Random.Range(0, ShapePrefabs.Length);
        SetNextIdx(randIdx);
        SetActiveIdx();
        int randIdx2 = Random.Range(0, ShapePrefabs.Length);
        SetNextIdx(randIdx2);
    }

    public void GetRandomTetromino() {
        int randIdx = Random.Range(0, ShapePrefabs.Length);
        SetActiveIdx();
        SetNextIdx(randIdx);
    }

    public void SetActiveIdx() {
        activeIdx = nextIdx;
        int shapeIdx = tetrisItems.Items[activeIdx].shapeIdx;
        Shape _shape = ShapePrefabs.shapes[shapeIdx];
        shape = new ShapeInt(_shape);
        _previewLayer.SetPreviewItem(tetrisItems.Items[activeIdx]);
        _previewLayer.ShowStaticPreview();
    }

    public void SetNextIdx(int index) {
        nextIdx = index;
        _previewLayer.SetNextPreviewItem(tetrisItems.Items[index]);
        _previewLayer.ShowNextPreview();
    }

    void Update() {
        if (activeIdx == -1) {
            _previewLayer.ClearGhostPreview();
            return;
        }

        var mousePos = (Vector3)_mouseUser.MouseInWorldPosition;
        
        if (!board.IsValidRectPosition(shape, mousePos)) {
            _previewLayer.ClearGhostPreview();
            return;
        }

        _previewLayer.ShowGhostPreview(shape, mousePos);
        
        if (board.IsValidTetrominoPosition(shape, mousePos) && _mouseUser.IsMouseButtonPressed(MouseUser.MouseButton.Left)) {
            board.SetTetrominoTile(tetrisItems.Items[activeIdx].Tile, shape, mousePos);
            board.SetTetromino(shape, mousePos);
            GetRandomTetromino();
            _previewLayer.ClearGhostPreview();
        }
    }
}
