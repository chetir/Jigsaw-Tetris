using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[CreateAssetMenu(menuName = "Tetris/Tetris Items", fileName = "New Tetris Items")]
public class TetrisItems: ScriptableObject
{
    public List<TetrisItem> Items;
}

[System.Serializable]
public class TetrisItem
{
    [field: SerializeField]
    public Sprite Sprite;
    [field: SerializeField]
    public int shapeIdx;
    [field: SerializeField]
    public Tile Tile {get; private set;}
    [field: SerializeField]
    public Vector3 TileOffset {get; private set;}
}
