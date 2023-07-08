using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Floor Stats", order = 1)]
public class FloorStats : ScriptableObject
{
    // base floor tilemap reference
    [field: SerializeField] public GameObject floorTilemap { get; private set; }

    [field: SerializeField] public int roomSize;
    [field: SerializeField] public int floorSize;
    
    [field: SerializeField] public TileBase floorTile { get; private set; }
    [field: SerializeField] public TileBase wallTile { get; private set; }
}