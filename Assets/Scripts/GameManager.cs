using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public TMP_Text scoreText;
    public TMP_Text timerText;
    public TMP_Text ghostTimerText;
    public TMP_Text countdownText;
    public TMP_Text gameOverText;
    public GameObject blocker;
    public GameObject[] lifeIcons;
    public Button exitButton;

    public PacStudentController pac;
    public Animator[] ghostAnimators;
    public MusicManager music;
    public AudioSource sfx;
    public AudioClip ghostEatClip;

    public const int NORMAL = 0;
    public const int SCARED = 1;
    public const int RECOVERING = 2;
    public const int DEAD = 3;

    public int score = 0;
    public int lives = 3;
    public bool gameRunning = false;
    public int[] ghostStates = new int[4];

    float gameTime = 0f;
    float scaredTimer = 0f;
    int pelletsLeft = -1;
    bool gameOver = false;
    Vector3[] ghostStarts = new Vector3[4];

    void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            ghostStarts[i] = ghostAnimators[i].transform.position;
        }

        scoreText.text = score.ToString("D6");
        timerText.text = FormatTime(0f);
        ghostTimerText.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(false);

        StartCoroutine(Countdown());
    }

    IEnumerator Countdown()
    {
        gameRunning = false;
        blocker.SetActive(true);
        countdownText.gameObject.SetActive(true);
        music.PlayIntro();

        countdownText.text = "3";
        yield return new WaitForSeconds(1f);
        countdownText.text = "2";
        yield return new WaitForSeconds(1f);
        countdownText.text = "1";
        yield return new WaitForSeconds(1f);
        countdownText.text = "GO!";
        yield return new WaitForSeconds(1f);

        countdownText.gameObject.SetActive(false);
        blocker.SetActive(false);

        int pellets = GameObject.FindGameObjectsWithTag("Pellet").Length;
        int powerPellets = GameObject.FindGameObjectsWithTag("PowerPellet").Length;
        pelletsLeft = pellets + powerPellets;

        music.PlayNormal();
        gameRunning = true;
    }

    void Update()
    {
        if (gameRunning == false)
        {
            return;
        }

        gameTime = gameTime + Time.deltaTime;
        timerText.text = FormatTime(gameTime);

        if (scaredTimer <= 0f)
        {
            return;
        }

        scaredTimer = scaredTimer - Time.deltaTime;

        if (scaredTimer < 0f)
        {
            scaredTimer = 0f;
        }

        ghostTimerText.text = Mathf.CeilToInt(scaredTimer).ToString();

        int newState = GlobalState();

        for (int i = 0; i < 4; i++)
        {
            if (ghostStates[i] != DEAD && ghostStates[i] != newState)
            {
                SetGhostState(i, newState);
            }
        }

        if (scaredTimer == 0f)
        {
            ghostTimerText.gameObject.SetActive(false);

            if (AnyGhostDead() == false)
            {
                music.PlayNormal();
            }
        }
    }

    public static string FormatTime(float t)
    {
        int minutes = (int)(t / 60f);
        int seconds = (int)(t % 60f);
        int hundredths = (int)((t * 100f) % 100f);

        return minutes.ToString("00") + ":" + seconds.ToString("00") + ":" + hundredths.ToString("00");
    }

    public void AddScore(int amount)
    {
        score = score + amount;
        scoreText.text = score.ToString("D6");
    }

    public void PelletEaten()
    {
        pelletsLeft = pelletsLeft - 1;

        if (pelletsLeft == 0)
        {
            StartCoroutine(GameOver());
        }
    }

    public void StartScared()
    {
        scaredTimer = 10f;
        ghostTimerText.gameObject.SetActive(true);

        for (int i = 0; i < 4; i++)
        {
            if (ghostStates[i] != DEAD)
            {
                SetGhostState(i, SCARED);
            }
        }

        if (AnyGhostDead() == false)
        {
            music.PlayScared();
        }
    }

    public int GlobalState()
    {
        if (scaredTimer <= 0f)
        {
            return NORMAL;
        }

        if (scaredTimer <= 3f)
        {
            return RECOVERING;
        }

        return SCARED;
    }

    public void SetGhostState(int i, int state)
    {
        ghostStates[i] = state;
        ghostAnimators[i].SetInteger("State", state);
    }

    public bool AnyGhostDead()
    {
        for (int i = 0; i < 4; i++)
        {
            if (ghostStates[i] == DEAD)
            {
                return true;
            }
        }

        return false;
    }

    public int GhostIndex(Transform t)
    {
        for (int i = 0; i < 4; i++)
        {
            if (ghostAnimators[i].transform == t)
            {
                return i;
            }
        }

        return -1;
    }

    public void EatGhost(int i)
    {
        SetGhostState(i, DEAD);
        AddScore(300);
        sfx.PlayOneShot(ghostEatClip);
        music.PlayDead();
        StartCoroutine(ReviveGhost(i));
    }

    IEnumerator ReviveGhost(int i)
    {
        yield return new WaitForSeconds(3f);

        if (ghostStates[i] != DEAD)
        {
            yield break;
        }

        SetGhostState(i, GlobalState());

        if (AnyGhostDead() == false)
        {
            if (GlobalState() == NORMAL)
            {
                music.PlayNormal();
            }
            else
            {
                music.PlayScared();
            }
        }
    }

    public void PacDied()
    {
        if (gameRunning == false)
        {
            return;
        }

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        gameRunning = false;
        lives = lives - 1;

        if (lives >= 0 && lives < lifeIcons.Length)
        {
            lifeIcons[lives].SetActive(false);
        }

        pac.Die();
        yield return new WaitForSeconds(2f);

        if (lives <= 0)
        {
            StartCoroutine(GameOver());
            yield break;
        }

        pac.ResetToStart();

        for (int i = 0; i < 4; i++)
        {
            ghostAnimators[i].transform.position = ghostStarts[i];
            SetGhostState(i, NORMAL);
        }

        scaredTimer = 0f;
        ghostTimerText.gameObject.SetActive(false);
        music.PlayNormal();
        gameRunning = true;
    }

    IEnumerator GameOver()
    {
        if (gameOver)
        {
            yield break;
        }

        gameOver = true;
        gameRunning = false;

        blocker.SetActive(true);
        gameOverText.gameObject.SetActive(true);
        exitButton.interactable = false;

        int bestScore = PlayerPrefs.GetInt("L1Score", 0);
        float bestTime = PlayerPrefs.GetFloat("L1Time", 0f);

        bool newBest = false;

        if (score > bestScore)
        {
            newBest = true;
        }
        else if (score == bestScore && gameTime < bestTime)
        {
            newBest = true;
        }

        if (newBest)
        {
            PlayerPrefs.SetInt("L1Score", score);
            PlayerPrefs.SetFloat("L1Time", gameTime);
            PlayerPrefs.Save();
        }

        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("StartScene");
    }
}