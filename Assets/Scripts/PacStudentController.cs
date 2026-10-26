using UnityEngine;

public class PacStudentController : MonoBehaviour
{
    public Tweener tweener;
    public Animator animator;
    public LevelGenerator level;
    public AudioSource audioSource;
    public AudioClip moveClip, eatClip;
    public ParticleSystem dust;
    public float speed = 5f; // tiles per second

    Vector2Int lastInput = Vector2Int.zero;
    Vector2Int currentInput = Vector2Int.zero;

    void Start()
    {
        StopMoving(); // stay still until a key is pressed
    }

    void Update()
    {
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
                StopMoving();
            }
        }
    }

    bool TryMove(Vector2Int dir)
    {
        if (dir == Vector2Int.zero) return false;

        int col = Mathf.RoundToInt(transform.position.x);
        int row = Mathf.RoundToInt(-transform.position.y);

        int targetCol = col + dir.x;
        int targetRow = row - dir.y; // minus, because rows go DOWN

        if (!level.IsWalkable(targetRow, targetCol)) return false;

        Vector3 end = new Vector3(targetCol, -targetRow, 0f);
        tweener.AddTween(transform, transform.position, end, 1f / speed);

        // Animation
        animator.speed = 1f;
        animator.SetInteger("Direction", DirectionIndex(dir));

        // Audio: eating sound on pellet tiles, otherwise movement sound
        int tile = level.fullMap[targetRow, targetCol];
        AudioClip clip = (tile == 5 || tile == 6) ? eatClip : moveClip;
        audioSource.clip = clip;
        audioSource.Play();

        // Dust
        if (!dust.isPlaying) dust.Play();

        return true;
    }

    int DirectionIndex(Vector2Int dir)
    {
        if (dir == Vector2Int.up) return 0;
        if (dir == Vector2Int.down) return 1;
        if (dir == Vector2Int.left) return 2;
        return 3; // right
    }

    void StopMoving()
    {
        animator.speed = 0f;
        if (audioSource.isPlaying) audioSource.Stop();
        if (dust.isPlaying) dust.Stop();
    }
}