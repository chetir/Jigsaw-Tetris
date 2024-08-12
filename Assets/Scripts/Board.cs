using System;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;


public class Board : MonoBehaviour
{
    private Vector2Int boardSize = new(6, 15);
    public Vector2Int boardTopLeft = new(-6, 4);
    private int boardBottom = -11;
    private HashSet<Vector2Int> rectPoints;  // rect area
    private HashSet<Vector2Int> availablePoints;  // where to set tetrominoes
    private Dictionary<int, SortedSet<int>> occupiedPuzzlePoints;  // where puzzles set
    private List<PuzzleItem> puzzleItems;
    private int score = 0;
    private float multiple = 1.0f;
    private static object o = new object();

    [SerializeField]
    private GameObject _retryButton;
    [SerializeField]
    private TextMeshProUGUI _scoreText;
    [SerializeField]
    private Tilemap _tilemap;
    [SerializeField]
    private Tilemap _frameLayer;
    [SerializeField]
    private FrameItems frameItems;
    private Dictionary<int, FrameItem> frameIdxToItem;


    private void Awake(){
        rectPoints = new HashSet<Vector2Int>();
        availablePoints = new HashSet<Vector2Int>();
        occupiedPuzzlePoints = new Dictionary<int, SortedSet<int>>();
        puzzleItems = new List<PuzzleItem>();
        frameIdxToItem = new Dictionary<int, FrameItem>();
        InitFrameIdx();
    }

    private void Start()
    {
        for (int i = 0; i < boardSize.x; i++) {
            for (int j = 0; j < boardSize.y; j++) {
                var point = new Vector2Int(boardTopLeft.x + i, boardTopLeft.y - j);
                rectPoints.Add(point);
                occupiedPuzzlePoints[point.x] = new SortedSet<int>();
            }
        }
        SetScore();
    }

    public void ChangeMultiple(float val) {
        multiple = val;
    }

    private void InitFrameIdx() {
        for (int i = 0; i < frameItems.Items.Count; i++) {
            frameIdxToItem[frameItems.Items[i].frameIdx] = frameItems.Items[i];
        }
    }

    public void GameOver()
    {
        Debug.Log("GameOver");
        _retryButton.SetActive(true);
        _tilemap.ClearAllTiles();
        _frameLayer.ClearAllTiles();
    }

    public void SetTetrominoTile(Tile tile, ShapeInt shape, Vector3 worldPosition) {
        Vector2Int coords = (Vector2Int)_tilemap.WorldToCell(worldPosition);
        for (int i = 0; i < shape.matrix.Count; i++)
        {
            Vector2Int tilePosition = coords + shape.matrix[i];
            _tilemap.SetTile((Vector3Int)tilePosition, tile);
        }
    }

    public void SetPuzzleTile(Tile tile, ShapeInt shape, Vector2Int coords) {
        for (int i = 0; i < shape.matrix.Count; i++)
        {
            Vector3Int tilePosition = (Vector3Int)(coords + shape.matrix[i]);
            _tilemap.SetTile(tilePosition, tile);
            Tile frameTile = frameIdxToItem[shape.values[i]].Tile;
            _frameLayer.SetTile(tilePosition, frameTile);
        }
    }

    public void SetPuzzleItem(PuzzleItem puzzleItem) {
        lock(o) {
        puzzleItem.shape.SortPoints();
        puzzleItems.Add(puzzleItem);
        foreach (var offset in puzzleItem.shape.matrix) {
            var point = puzzleItem.position + offset;
            occupiedPuzzlePoints[point.x].Add(point.y);
            availablePoints.Add(point);
        }
        }
    }

    public void SetTetromino(ShapeInt shape, Vector3 worldPosition) {
        lock(o) {
        Vector2Int coords = (Vector2Int)_tilemap.WorldToCell(worldPosition);
        bool canMove = false;
        for (int i = 0; i < shape.matrix.Count; i++)
        {
            Vector2Int point = coords + shape.matrix[i];
            availablePoints.Remove(point);
            for (int j = 0; j < puzzleItems.Count; j++) {
                if (puzzleItems[j].Contains(point)) {
                    puzzleItems[j].Set(point);
                    if (puzzleItems[j].IsPuzzleFull()) {
                        score += (int)(puzzleItems[j].scoreBase * multiple);
                        SetScore();
                        ClearTile(puzzleItems[j].shape, puzzleItems[j].position);
                        ClearPuzzleItem(j);
                        canMove = true;
                    }
                    break;
                }
            }
        }

        if (canMove) {
            // DebugPositions();
            CheckMove();
            // DebugPositions();
        }
        }
    }

    private void DebugPositions() {
        Debug.Log("Start Debug");
        string str = "";
        for (int i = 0; i < puzzleItems.Count; i++) {
            str += puzzleItems[i].position + "\n";
            for(int j = 0; j < puzzleItems[i].shape.matrix.Count; j++) {
                var point = puzzleItems[i].shape.matrix[j] + puzzleItems[i].position;
                var val = puzzleItems[i].shape.values[j];
                str += point + ":" + val + ", ";
            }
            str += "\n";
        }
        Debug.Log(str);

        string str2 = "";
        str2 += "aval: ";
        foreach(var point in availablePoints) {
            str2 += point + " ";
        }
        Debug.Log(str2);

        string str3 = "";
        str3 += "occu: ";
        foreach(var x in occupiedPuzzlePoints) {
            foreach(var y in x.Value) {
                str3 += x.Key + "," + y + " ";
            }
        }
        Debug.Log(str3);

        Debug.Log("puzzleItems: " + puzzleItems.Count);
        Debug.Log("End");
    }

    public void ClearTile(ShapeInt shape, Vector2Int coords)
    {
        for (int i = 0; i < shape.matrix.Count; i++)
        {
            Vector3Int tilePosition = (Vector3Int)(coords + shape.matrix[i]);
            _tilemap.SetTile(tilePosition, null);
            _frameLayer.SetTile(tilePosition, null);
        }
    }

    public void ClearPuzzleItem(int idx)
    {
        for (int i = 0; i < puzzleItems[idx].shape.matrix.Count; i++)
        {
            Vector2Int point = puzzleItems[idx].position + puzzleItems[idx].shape.matrix[i];
            occupiedPuzzlePoints[point.x].Remove(point.y);
            availablePoints.Remove(point);
        }
        puzzleItems.RemoveAt(idx);
    }

    private void MovePuzzleItem(int idx, int step) {
        for (int i = 0; i < puzzleItems[idx].shape.matrix.Count; i++)
        {
            Vector2Int point = puzzleItems[idx].position + puzzleItems[idx].shape.matrix[i];
            occupiedPuzzlePoints[point.x].Remove(point.y);
            bool contains = availablePoints.Remove(point);
            var tile = _tilemap.GetTile((Vector3Int)point);
            var frameTile = _frameLayer.GetTile((Vector3Int)point);

            Vector2Int newPoint = new(point.x, point.y - step);
            occupiedPuzzlePoints[newPoint.x].Add(newPoint.y);
            if (contains) availablePoints.Add(newPoint);
            _tilemap.SetTile((Vector3Int)newPoint, tile);
            _tilemap.SetTile((Vector3Int)point, null);
            _frameLayer.SetTile((Vector3Int)newPoint, frameTile);
            _frameLayer.SetTile((Vector3Int)point, null);
        }

        puzzleItems[idx].position = new(puzzleItems[idx].position.x, puzzleItems[idx].position.y - step);
    }

    public bool IsValidRectPosition(ShapeInt shape, Vector3 worldPosition)
    {
        Vector2Int coords = (Vector2Int)_tilemap.WorldToCell(worldPosition);
        for (int i = 0; i < shape.matrix.Count; i++) {
            if (!rectPoints.Contains(coords + shape.matrix[i])) {
                return false;
            }
        }

        return true;
    }

    public bool IsValidTetrominoPosition(ShapeInt shape, Vector3 worldPosition)
    {
        Vector2Int coords = (Vector2Int)_tilemap.WorldToCell(worldPosition);
        for (int i = 0; i < shape.matrix.Count; i++) {
            if (!availablePoints.Contains(coords + shape.matrix[i])) {
                return false;
            }
        }

        return true;
    }

    public bool IsValidPuzzlePosition(ShapeInt shape, Vector2Int coords)
    {
        for (int i = 0; i < shape.matrix.Count; i++) {
            var point = coords + shape.matrix[i];
            if (!rectPoints.Contains(point) || occupiedPuzzlePoints[point.x].Contains(point.y)) {
                return false;
            }
        }

        return true;
    }

    public void CheckMove() {
        for (int i = 0; i < puzzleItems.Count; i++) {  // natural sorted by times order.
            int minStep = 9999;
            for (int x = puzzleItems[i].shape.MinX; x <= puzzleItems[i].shape.MaxX; x++) {
                for (int y = puzzleItems[i].shape.MinY; y <= puzzleItems[i].shape.MaxY; y++) {
                    if (puzzleItems[i].shape.Contains(new Vector2Int(x, y))) {
                        int posY = y + puzzleItems[i].position.y;
                        minStep = Math.Min(minStep, posY - boardBottom);
                        foreach (var yy in occupiedPuzzlePoints[x + puzzleItems[i].position.x]) {
                            if (yy < posY) {
                                minStep = Math.Min(minStep, posY - yy);
                            } else {
                                break;
                            }
                        }
                        break;
                    }
                }
            }
            if (minStep > 1) {
                MovePuzzleItem(i, minStep - 1);
            }
        }
    }

    public void SetScore() {
        string text = "Score:" + score;
        _scoreText.SetText(text);
    }
}
