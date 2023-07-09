using EZCameraShake;
using TMPro;
using UnityEngine;

public class GameMan : MonoBehaviour
{
    public static GameMan inst;

    [field: SerializeField] public PlayerCore playerCore { get; private set; }
    [field: SerializeField] public FloorStats stats { get; private set; }
    [field: SerializeField] public CameraShaker camShaker { get; private set; }
    public Map map { get; private set; }
    
    [field: SerializeField] public GameObject plume { get; private set; }

    private int score = 0;
    private int scoreTarget;
    [SerializeField] private TextMeshProUGUI scoreText;

    public void Awake()
    {
        // not singleton ?
        if (inst != null) Debug.Break();
        inst = this;

        map = new Map();
        map.AddFloor(stats);
        
        // place player on first floor
        Floor baseFloor = map.GetFloor(0);
        Room spawnRoom = baseFloor.GetClosestRoom(stats.floorSize * new Vector2(0.5f, 0.5f));
        playerCore.transform.position = spawnRoom.bounds.center;
    }

    public void AddScore(int delta)
    {
        score += delta;
        scoreText.text = score.ToString();
    }
    
    public void SetMaxScore(int score)
    {
        scoreTarget = score;
    }
    
    public void Update()
    {
        map.Update(Time.deltaTime);
    }
}
