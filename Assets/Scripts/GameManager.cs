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
    public TMP_Text powerUpText;

    public PacStudentController pac;
    public Animator[] ghostAnimators;
    public MusicManager music;
    public AudioSource sfx;
    public AudioClip ghostEatClip;

    public string saveKey = "L1";

    public const int NORMAL = 0;
    public const int SCARED = 1;
    public const int RECOVERING = 2;
    public const int DEAD = 3;

    public int score = 0;
    public int lives = 3;
    public bool gameRunning = false;
    public bool GhostsFrozen = false;
    public int[] ghostStates = new int[4];

    float gameTime = 0f;
    float scaredTimer = 0f;
    float freezeTimer = 0f;
    int pelletsLeft = -1;
    bool gameOver = false;

    void Start()
    {
        scoreText.text = score.ToString("D6");
        timerText.text = FormatTime(0f);
        ghostTimerText.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(false);

        if (powerUpText != null)
        {
            powerUpText.text = "";
        }

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

        UpdateScaredTimer();
        UpdateFreezeTimer();
    }

    void UpdateScaredTimer()
    {
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

    void UpdateFreezeTimer()
    {
        if (freezeTimer <= 0f)
        {
            return;
        }

        freezeTimer = freezeTimer - Time.deltaTime;

        if (freezeTimer <= 0f)
        {
            freezeTimer = 0f;
            GhostsFrozen = false;
            SetGhostTint(Color.white);
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

    public void ActivateFreeze(float seconds)
    {
        freezeTimer = seconds;
        GhostsFrozen = true;
        SetGhostTint(new Color(0.5f, 0.9f, 1f));
    }

    void SetGhostTint(Color c)
    {
        for (int i = 0; i < 4; i++)
        {
            SpriteRenderer sr = ghostAnimators[i].GetComponent<SpriteRenderer>();

            if (sr != null)
            {
                sr.color = c;
            }
        }
    }

    public void ShowPowerUpText(string message)
    {
        if (powerUpText != null)
        {
            powerUpText.text = message;
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
    }

    public void GhostReachedHome(int i)
    {
        if (ghostStates[i] != DEAD)
        {
            return;
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
            GhostController ghost = ghostAnimators[i].GetComponent<GhostController>();

            if (ghost != null)
            {
                ghost.ResetGhost();
            }

            SetGhostState(i, NORMAL);
        }

        scaredTimer = 0f;
        freezeTimer = 0f;
        GhostsFrozen = false;
        SetGhostTint(Color.white);
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

        if (exitButton != null)
        {
            exitButton.interactable = false;
        }

        int bestScore = PlayerPrefs.GetInt(saveKey + "Score", 0);
        float bestTime = PlayerPrefs.GetFloat(saveKey + "Time", 0f);

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
            PlayerPrefs.SetInt(saveKey + "Score", score);
            PlayerPrefs.SetFloat(saveKey + "Time", gameTime);
            PlayerPrefs.Save();
        }

        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("StartScene");
    }
}
