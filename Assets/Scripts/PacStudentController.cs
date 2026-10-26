using UnityEngine;

public class PacStudentController : MonoBehaviour
{
    // ----- Things you drag in from the Inspector -----
    public Tweener tweener;
    public Animator animator;
    public LevelGenerator level;
    public AudioSource audioSource;
    public AudioClip moveClip;
    public AudioClip eatClip;
    public ParticleSystem dust;
    public float speed = 5f;

    // ----- Directions stored as simple numbers -----
    // x: -1 = left, 1 = right
    // y:  1 = up,  -1 = down
    // 0,0 means "no direction"
    int lastX = 0, lastY = 0;       // the key the player pressed last
    int currentX = 0, currentY = 0; // the way PacStudent is moving now

    void Start()
    {
        animator.SetInteger("Direction", 3); // face right at the start
        StopMoving();
    }

    void Update()
    {
        // 1. Remember the last key pressed
        if (Input.GetKeyDown(KeyCode.W)) { lastX = 0;  lastY = 1;  }
        if (Input.GetKeyDown(KeyCode.S)) { lastX = 0;  lastY = -1; }
        if (Input.GetKeyDown(KeyCode.A)) { lastX = -1; lastY = 0;  }
        if (Input.GetKeyDown(KeyCode.D)) { lastX = 1;  lastY = 0;  }

        // 2. If still sliding to the next tile, wait
        if (tweener.TweenExists(transform))
            return;

        // 3. Try the new key first, otherwise keep going the same way
        if (TryMove(lastX, lastY))
        {
            currentX = lastX;
            currentY = lastY;
        }
        else if (!TryMove(currentX, currentY))
        {
            StopMoving(); // hit a wall
        }
    }

    // Tries to move one tile. Returns true if it worked.
    bool TryMove(int dx, int dy)
    {
        if (dx == 0 && dy == 0)
            return false;

        // Where am I on the grid?
        int col = Mathf.RoundToInt(transform.position.x);
        int row = Mathf.RoundToInt(-transform.position.y);

        // Where do I want to go?
        int newCol = col + dx;
        int newRow = row - dy;

        // Is there a wall?
        if (!level.IsWalkable(newRow, newCol))
            return false;

        // Move there smoothly
        Vector3 target = new Vector3(newCol, -newRow, 0);
        tweener.AddTween(transform, transform.position, target, 1f / speed);

        // Play the right animation
        animator.speed = 1;
        if (dy == 1)       animator.SetInteger("Direction", 0); // up
        else if (dy == -1) animator.SetInteger("Direction", 1); // down
        else if (dx == -1) animator.SetInteger("Direction", 2); // left
        else               animator.SetInteger("Direction", 3); // right

        // Eating sound if there is a pellet (5 or 6), else walking sound
        int tile = level.fullMap[newRow, newCol];
        if (tile == 5 || tile == 6)
            audioSource.clip = eatClip;
        else
            audioSource.clip = moveClip;
        audioSource.Play();

        // Dust effect
        if (!dust.isPlaying)
            dust.Play();

        return true;
    }

    void StopMoving()
    {
        animator.speed = 0; // freeze animation
        audioSource.Stop();
        dust.Stop();
    }
}