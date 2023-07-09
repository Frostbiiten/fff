using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{
    public TextMeshProUGUI highest;
    public TextMeshProUGUI recent;

    private void Awake()
    {
        Time.timeScale = 1f;
        highest.text = "HIGHEST FLOOR: " + PlayerPrefs.GetInt("Highscore", 0);
        recent.text = "LATEST ATTEMPT: " + PlayerPrefs.GetInt("Latest", 0);
    }

    public void Go()
    {
        SceneManager.LoadScene("Game");
    }
}
