using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class StartSceneManager : MonoBehaviour
{
    public TMP_Text l1Score;
    public TMP_Text l1Time;
    public TMP_Text l2Score;
    public TMP_Text l2Time;
    public MusicManager music;

    void Start()
    {
        ShowScore(l1Score, l1Time, "L1");
        ShowScore(l2Score, l2Time, "L2");

        if (music != null)
        {
            music.PlayStartScene();
        }
    }

    void ShowScore(TMP_Text scoreText, TMP_Text timeText, string key)
    {
        int bestScore = PlayerPrefs.GetInt(key + "Score", 0);
        float bestTime = PlayerPrefs.GetFloat(key + "Time", 0f);

        if (scoreText != null)
        {
            scoreText.text = "High Score: " + bestScore;
        }

        if (timeText != null)
        {
            timeText.text = "Time: " + GameManager.FormatTime(bestTime);
        }
    }

    public void LoadLevel1()
    {
        SceneManager.LoadScene("Level01Scene");
    }

    public void LoadLevel2()
    {
        SceneManager.LoadScene("InnovationScene");
    }

    public void LoadStart()
    {
        SceneManager.LoadScene("StartScene");
    }
}