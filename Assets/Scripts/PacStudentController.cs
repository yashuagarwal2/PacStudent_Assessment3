using UnityEngine;

public class PacStudentController : MonoBehaviour
{
    public Tweener tweener;
    public Animator animator;
    public LevelGenerator level;
    public AudioSource audioSource;
    public AudioClip moveClip, eatClip;
    public ParticleSystem dust;
    public float speed = 5f;

    public GameManager gm;
    public AudioClip cherryClip;
    public AudioClip wallClip;
    public ParticleSystem wallBump;
    public AudioClip deathClip;
    public ParticleSystem deathParticles;

    Vector2Int lastInput = Vector2Int.zero;
    Vector2Int currentInput = Vector2Int.zero;
    bool isMoving = false;
    Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
        animator.SetInteger("Direction", 3);
        StopMoving();
    }

    void Update()
    {
        if (!gm.gameRunning) return;

        if (Input.GetKeyDown(KeyCode.W)) lastInput = Vector2Int.up;
        if (Input.GetKeyDown(KeyCode.A)) lastInput = Vector2Int.left;
        if (Input.GetKeyDown(KeyCode.S)) lastInput = Vector2Int.down;
        if (Input.GetKeyDown(KeyCode.D)) lastInput = Vector2Int.right;

        if (!tweener.TweenExists(transform))
        {
            if (TryMove(lastInput))
            {
                currentInput = lastInput;
            }
            else if (!TryMove(currentInput))
            {
                bool wasMoving = isMoving;
                StopMoving();
                if (wasMoving) WallBump(currentInput);
            }
        }
    }

    bool TryMove(Vector2Int dir)
    {
        if (dir == Vector2Int.zero) return false;

        int col = Mathf.RoundToInt(transform.position.x);
        int row = Mathf.RoundToInt(-transform.position.y);
        int targetCol = col + dir.x;
        int targetRow = row - dir.y;

        int maxCol = level.fullMap.GetLength(1) - 1;
        if (targetCol < 0 || targetCol > maxCol)
        {
            int newCol = targetCol < 0 ? maxCol : 0;
            transform.position = new Vector3(newCol, -row, 0f);
            targetCol = newCol + dir.x;
        }

        if (!level.IsWalkable(targetRow, targetCol)) return false;

        Vector3 end = new Vector3(targetCol, -targetRow, 0f);
        tweener.AddTween(transform, transform.position, end, 1f / speed);

        animator.speed = 1f;
        animator.SetInteger("Direction", DirectionIndex(dir));

        int tile = level.fullMap[targetRow, targetCol];
        audioSource.clip = (tile == 5 || tile == 6) ? eatClip : moveClip;
        audioSource.Play();

        if (!dust.isPlaying) dust.Play();
        isMoving = true;
        return true;
    }

    int DirectionIndex(Vector2Int dir)
    {
        if (dir == Vector2Int.up) return 0;
        if (dir == Vector2Int.down) return 1;
        if (dir == Vector2Int.left) return 2;
        return 3;
    }

    void StopMoving()
    {
        isMoving = false;
        animator.speed = 0f;
        if (audioSource.isPlaying) audioSource.Stop();
        if (dust.isPlaying) dust.Stop();
    }

    void WallBump(Vector2Int dir)
    {
        audioSource.PlayOneShot(wallClip);
        wallBump.transform.position = transform.position + new Vector3(dir.x, dir.y, 0f) * 0.5f;
        wallBump.Play();
    }

    public void Die()
    {
        tweener.RemoveTween(transform);
        StopMoving();
        animator.speed = 1f;
        animator.SetInteger("Direction", 4);
        audioSource.PlayOneShot(deathClip);
        deathParticles.Play();
    }

    public void ResetToStart()
    {
        tweener.RemoveTween(transform);
        transform.position = startPos;
        lastInput = Vector2Int.zero;
        currentInput = Vector2Int.zero;
        animator.SetInteger("Direction", 3);
        StopMoving();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!gm.gameRunning) return;

        int c = Mathf.RoundToInt(other.transform.position.x);
        int r = Mathf.RoundToInt(-other.transform.position.y);

        if (other.CompareTag("Pellet"))
        {
            gm.AddScore(10);
            level.fullMap[r, c] = 0;
            Destroy(other.gameObject);
            gm.PelletEaten();
        }
        else if (other.CompareTag("PowerPellet"))
        {
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
        else if (other.CompareTag("Ghost"))
        {
            int i = gm.GhostIndex(other.transform);
            if (i < 0) return;
            int s = gm.ghostStates[i];
            if (s == GameManager.NORMAL) gm.PacDied();
            else if (s == GameManager.SCARED || s == GameManager.RECOVERING) gm.EatGhost(i);
        }
    }
}