using UnityEngine;

public class Progressor : MonoBehaviour
{
    public static Progressor instance;
    public int currentLevel;

    public void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
