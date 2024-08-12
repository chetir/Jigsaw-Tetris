using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[CreateAssetMenu(menuName = "Frame/Frame Items", fileName = "New Frame Items")]
public class FrameItems: ScriptableObject
{
    public List<FrameItem> Items;
}

[System.Serializable]
public class FrameItem
{
    public int frameIdx;
    [field: SerializeField]
    public Tile Tile {get; private set;}
    [field: SerializeField]
    public Vector3 TileOffset {get; private set;}
}
