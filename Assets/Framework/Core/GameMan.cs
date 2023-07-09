using EZCameraShake;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameMan : MonoBehaviour
{
    public static GameMan inst;

    [field: SerializeField] public PlayerCore playerCore { get; private set; }
    [field: SerializeField] public FloorStats stats { get; private set; }
    [field: SerializeField] public CameraShaker camShaker { get; private set; }
    public Map map { get; private set; }
    
    [field: SerializeField] public GameObject plume { get; private set; }

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI floorText;
    [SerializeField] private Animator niceAnim;
    [SerializeField] private Image timeBar;
    [SerializeField] private AudioSource burningSound;
    private float currentTime;
    private float maxTime;
    private int scoreTarget;
    private int score = 0;
    
    public GameObject enemy;

    [SerializeField] private Progressor progressPrefab;
    public float completeTimer;
    public bool won;

    public void Awake()
    {
        // not singleton ?
        if (inst != null) Debug.Break();
        inst = this;

        if (Progressor.instance == null) Instantiate(progressPrefab);
        ++Progressor.instance.currentLevel;
        maxTime = currentTime = (int)(100 * Mathf.Pow(Progressor.instance.currentLevel, -0.4f));
        timeText.text = ((int)currentTime).ToString();
        Time.timeScale = 1f;
        floorText.text = "FLOOR " + Progressor.instance.currentLevel;

        // Update high score
        int highScore = Mathf.Max(Progressor.instance.currentLevel, PlayerPrefs.GetInt("Highscore", 0));
        PlayerPrefs.SetInt("Highscore", highScore);
        PlayerPrefs.SetInt("Latest", Progressor.instance.currentLevel);
        PlayerPrefs.Save();

        map = new Map();
        map.AddFloor(stats);
        
        // place player on first floor
        Floor baseFloor = map.GetFloor(0);
        Room spawnRoom = baseFloor.GetClosestRoom(stats.floorSize * new Vector2(0.5f, 0.5f));
        playerCore.transform.position = spawnRoom.bounds.center;
        
        AddScore(0);
    }

    public void NextLevel()
    {
        SceneLoader.instance.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void Menu()
    {
        SceneLoader.instance.LoadScene("Menu");
    }

    public void AddScore(int delta)
    {
        score += delta;
        if (score == scoreTarget && !won)
        {
            niceAnim.Play("Nice");
            completeTimer = 2f;
            won = true;
        }
        scoreText.text = score + "/" + scoreTarget;
    }
    
    public void SetMaxScore(int score)
    {
        scoreTarget = score;
    }

    public void AddTime(int delta)
    {
        currentTime += delta;
        timeText.text = ((int)Mathf.Ceil(currentTime)).ToString();
    }
    
    public void Update()
    {
        map.Update(Time.deltaTime);
        currentTime -= Time.deltaTime;

        burningSound.volume = (float)score / scoreTarget;
        
        if (completeTimer > 0f)
        {
            completeTimer -= Time.unscaledDeltaTime;
            Time.timeScale = Mathf.MoveTowards(Time.timeScale, 0.1f, Time.unscaledDeltaTime * 0.5f);
            if (completeTimer <= 0 && !SceneLoader.instance.loading)
            {
                if (won) NextLevel();
                else Menu();
            }
        }
        else
        {
            timeBar.fillAmount = currentTime / maxTime;
            if ((int)currentTime != (int)(currentTime + Time.deltaTime))
            {
                timeText.text = ((int)Mathf.Ceil(currentTime)).ToString();
            }
            
            if (currentTime < 0f && !won)
            {
                playerCore.ChangeHP(-999);
                timeBar.fillAmount = 0;
                timeText.text = "0";
            }
        }
    }
}
