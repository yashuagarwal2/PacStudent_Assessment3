using UnityEngine;
using TMPro;

public class PacStudentController : MonoBehaviour
{
    public Tweener tweener;
    public Animator animator;
    public LevelGenerator level;
    public AudioSource audioSource;
    public AudioClip moveClip;
    public AudioClip eatClip;
    public ParticleSystem dust;
    public float speed = 5f;

    public GameManager gm;
    public AudioClip cherryClip;
    public AudioClip wallClip;
    public ParticleSystem wallBump;
    public AudioClip deathClip;
    public ParticleSystem deathParticles;

    public bool abilitiesEnabled = false;
    public TMP_Text dashText;
    public AudioClip powerUpClip;
    public float dashCooldown = 5f;

    int lastX = 0;
    int lastY = 0;
    int currentX = 0;
    int currentY = 0;
    bool isMoving = false;
    Vector3 startPos;

    float speedBoostTimer = 0f;
    float dashTimer = 0f;
    SpriteRenderer sr;

    void Start()
    {
        startPos = transform.position;
        sr = GetComponent<SpriteRenderer>();
        animator.SetInteger("Direction", 3);
        StopMoving();
        UpdateDashText();
    }

    void Update()
    {
        if (gm.gameRunning == false)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            lastX = 0;
            lastY = 1;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            lastX = 0;
            lastY = -1;
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            lastX = -1;
            lastY = 0;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            lastX = 1;
            lastY = 0;
        }

        if (abilitiesEnabled)
        {
            UpdateAbilities();
        }

        if (tweener.TweenExists(transform))
        {
            return;
        }

        if (abilitiesEnabled && Input.GetKey(KeyCode.Space) && dashTimer <= 0f)
        {
            if (TryDash())
            {
                return;
            }
        }

        if (TryMove(lastX, lastY))
        {
            currentX = lastX;
            currentY = lastY;
        }
        else if (TryMove(currentX, currentY) == false)
        {
            bool wasMoving = isMoving;
            StopMoving();

            if (wasMoving)
            {
                WallBump(currentX, currentY);
            }
        }
    }

    void UpdateAbilities()
    {
        if (speedBoostTimer > 0f)
        {
            speedBoostTimer = speedBoostTimer - Time.deltaTime;

            if (speedBoostTimer <= 0f)
            {
                speedBoostTimer = 0f;

                if (sr != null)
                {
                    sr.color = Color.white;
                }

                gm.ShowPowerUpText("");
            }
        }

        if (dashTimer > 0f)
        {
            dashTimer = dashTimer - Time.deltaTime;

            if (dashTimer < 0f)
            {
                dashTimer = 0f;
            }

            UpdateDashText();
        }
    }

    void UpdateDashText()
    {
        if (dashText == null)
        {
            return;
        }

        if (abilitiesEnabled == false)
        {
            dashText.text = "";
        }
        else if (dashTimer > 0f)
        {
            dashText.text = "DASH: " + Mathf.CeilToInt(dashTimer);
        }
        else
        {
            dashText.text = "DASH: READY [SPACE]";
        }
    }

    bool TryDash()
    {
        int dx = currentX;
        int dy = currentY;

        if (dx == 0 && dy == 0)
        {
            dx = lastX;
            dy = lastY;
        }

        if (dx == 0 && dy == 0)
        {
            return false;
        }

        int col = Mathf.RoundToInt(transform.position.x);
        int row = Mathf.RoundToInt(-transform.position.y);
        int steps = 0;

        for (int i = 1; i <= 3; i++)
        {
            int c = col + dx * i;
            int r = row - dy * i;

            if (level.IsWalkable(r, c) == false)
            {
                break;
            }

            steps = i;
        }

        if (steps == 0)
        {
            return false;
        }

        int endCol = col + dx * steps;
        int endRow = row - dy * steps;
        Vector3 end = new Vector3(endCol, -endRow, 0);
        tweener.AddTween(transform, transform.position, end, 0.12f);

        if (powerUpClip != null)
        {
            audioSource.PlayOneShot(powerUpClip);
        }
        else
        {
            audioSource.PlayOneShot(moveClip);
        }

        wallBump.transform.position = transform.position;
        wallBump.Play();

        currentX = dx;
        currentY = dy;
        isMoving = true;
        dashTimer = dashCooldown;
        UpdateDashText();
        return true;
    }

    float CurrentSpeed()
    {
        if (speedBoostTimer > 0f)
        {
            return speed * 2f;
        }

        return speed;
    }

    bool TryMove(int dx, int dy)
    {
        if (dx == 0 && dy == 0)
        {
            return false;
        }

        int col = Mathf.RoundToInt(transform.position.x);
        int row = Mathf.RoundToInt(-transform.position.y);
        int targetCol = col + dx;
        int targetRow = row - dy;

        int maxCol = level.fullMap.GetLength(1) - 1;

        if (targetCol < 0)
        {
            transform.position = new Vector3(maxCol, -row, 0);
            targetCol = maxCol + dx;
        }
        else if (targetCol > maxCol)
        {
            transform.position = new Vector3(0, -row, 0);
            targetCol = 0 + dx;
        }

        if (level.IsWalkable(targetRow, targetCol) == false)
        {
            return false;
        }

        Vector3 end = new Vector3(targetCol, -targetRow, 0);
        tweener.AddTween(transform, transform.position, end, 1f / CurrentSpeed());

        animator.speed = 1;

        if (dy == 1)
        {
            animator.SetInteger("Direction", 0);
        }
        else if (dy == -1)
        {
            animator.SetInteger("Direction", 1);
        }
        else if (dx == -1)
        {
            animator.SetInteger("Direction", 2);
        }
        else
        {
            animator.SetInteger("Direction", 3);
        }

        int tile = level.fullMap[targetRow, targetCol];

        if (tile == 5 || tile == 6)
        {
            audioSource.clip = eatClip;
        }
        else
        {
            audioSource.clip = moveClip;
        }
        audioSource.Play();

        if (dust.isPlaying == false)
        {
            dust.Play();
        }

        isMoving = true;
        return true;
    }

    void StopMoving()
    {
        isMoving = false;
        animator.speed = 0;

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        if (dust.isPlaying)
        {
            dust.Stop();
        }
    }

    void WallBump(int dx, int dy)
    {
        audioSource.PlayOneShot(wallClip);

        float bumpX = transform.position.x + dx * 0.5f;
        float bumpY = transform.position.y + dy * 0.5f;
        wallBump.transform.position = new Vector3(bumpX, bumpY, 0);
        wallBump.Play();
    }

    public void Die()
    {
        tweener.RemoveTween(transform);
        StopMoving();
        animator.speed = 1;
        animator.SetInteger("Direction", 4);
        audioSource.PlayOneShot(deathClip);
        deathParticles.Play();
    }

    public void ResetToStart()
    {
        tweener.RemoveTween(transform);
        transform.position = startPos;
        lastX = 0;
        lastY = 0;
        currentX = 0;
        currentY = 0;
        speedBoostTimer = 0f;

        if (sr != null)
        {
            sr.color = Color.white;
        }

        gm.ShowPowerUpText("");
        animator.SetInteger("Direction", 3);
        StopMoving();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (gm.gameRunning == false)
        {
            return;
        }

        if (other.CompareTag("Pellet"))
        {
            int c = Mathf.RoundToInt(other.transform.position.x);
            int r = Mathf.RoundToInt(-other.transform.position.y);

            gm.AddScore(10);
            level.fullMap[r, c] = 0;
            Destroy(other.gameObject);
            gm.PelletEaten();
        }
        else if (other.CompareTag("PowerPellet"))
        {
            int c = Mathf.RoundToInt(other.transform.position.x);
            int r = Mathf.RoundToInt(-other.transform.position.y);

            gm.AddScore(50);
            level.fullMap[r, c] = 0;
            Destroy(other.gameObject);
            gm.StartScared();
            gm.PelletEaten();
        }
        else if (other.CompareTag("Cherry"))
        {
            gm.AddScore(100);
            audioSource.PlayOneShot(cherryClip);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("PowerUp"))
        {
            EatPowerUp(other);
        }
        else if (other.CompareTag("Ghost"))
        {
            HitGhost(other);
        }
    }

    void EatPowerUp(Collider2D other)
    {
        PowerUp p = other.GetComponent<PowerUp>();

        if (p != null && p.type == PowerUp.Type.Speed)
        {
            speedBoostTimer = 5f;

            if (sr != null)
            {
                sr.color = new Color(1f, 0.8f, 0.3f);
            }

            gm.ShowPowerUpText("SPEED BOOST!");
        }
        else
        {
            gm.ActivateFreeze(4f);
            gm.ShowPowerUpText("GHOSTS FROZEN!");
        }

        if (powerUpClip != null)
        {
            audioSource.PlayOneShot(powerUpClip);
        }
        else
        {
            audioSource.PlayOneShot(cherryClip);
        }

        gm.AddScore(25);
        Destroy(other.gameObject);
    }

    void HitGhost(Collider2D other)
    {
        int i = gm.GhostIndex(other.transform);

        if (i < 0)
        {
            return;
        }

        int state = gm.ghostStates[i];

        if (state == GameManager.NORMAL)
        {
            gm.PacDied();
        }
        else if (state == GameManager.SCARED || state == GameManager.RECOVERING)
        {
            gm.EatGhost(i);
        }
    }
}