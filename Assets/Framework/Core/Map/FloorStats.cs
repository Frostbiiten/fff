using UnityEngine.Tilemaps;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Floor Stats", order = 1)]
public class FloorStats : ScriptableObject
{
    // Heat
    [field: SerializeField] public Gradient heatGradient { get; private set; }
    
    // base floor tilemap reference
    [field: SerializeField] public GameObject floorTilemap { get; private set; }

    [field: SerializeField] public int roomSize;
    [field: SerializeField] public int floorSize;
    [field: SerializeField] public AnimationCurve tileBounceAnim;
    [field: SerializeField] public TileBase[] floorTiles { get; private set; }
    [field: SerializeField] public GameObject fireParticles { get; private set; }

    [field: SerializeField] public TileBase outsideTile { get; private set; }
    [field: SerializeField] public TileBase floorTile { get; private set; }
    [field: SerializeField] public TileBase wallTile { get; private set; }
}