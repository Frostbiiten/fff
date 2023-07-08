using UnityEngine;

public class GameMan : MonoBehaviour
{
    public static GameMan inst;
    
    [field: SerializeField] public FloorStats stats { get; private set; }
    private Map _map = new Map();

    public void Awake()
    {
        if (inst != null) Debug.Break();
        inst = this;
        _map.AddFloor(stats);
    }
}
