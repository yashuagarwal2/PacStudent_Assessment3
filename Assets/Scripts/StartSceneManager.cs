using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class StartSceneManager : MonoBehaviour
{
    public TMP_Text l1Score;
    public TMP_Text l1Time;

    void Start()
    {
        if (l1Score != null) l1Score.text = "High Score: " + PlayerPrefs.GetInt("L1Score", 0);
        if (l1Time != null) l1Time.text = "Time: " + GameManager.FormatTime(PlayerPrefs.GetFloat("L1Time", 0f));
    }

    public void LoadLevel1() { SceneManager.LoadScene("Level01Scene"); }
    public void LoadLevel2() { }
    public void LoadStart() { SceneManager.LoadScene("StartScene"); }
}