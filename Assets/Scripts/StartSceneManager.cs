using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneManager : MonoBehaviour
{
    public void LoadLevel1()
    {
        SceneManager.LoadScene("Level01Scene");
    }

    public void LoadLevel2()
    {
    }

    public void LoadStart()
    {
        SceneManager.LoadScene("StartScene");
    }
}