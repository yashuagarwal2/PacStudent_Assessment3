using UnityEngine;

public class PacStudentController : MonoBehaviour
{
    public Tweener tweener;
    public Animator animator;
    public float speed = 4f;

    Vector3[] points =
    {
        new Vector3(1, -1, 0),
        new Vector3(6, -1, 0),
        new Vector3(6, -5, 0),
        new Vector3(1, -5, 0)
    };

    int current = 0;

    void Start()
    {
        
        if (tweener == null)
        {
            tweener = FindAnyObjectByType<Tweener>();
        }

   
        if (tweener == null)
        {
            tweener = gameObject.AddComponent<Tweener>();
        }

       
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        transform.position = points[0];
        MoveToNextPoint();
    }

    void Update()
    {
        
        if (!tweener.TweenExists(transform))
        {
            MoveToNextPoint();
        }
    }

    void MoveToNextPoint()
    {
        int next = (current + 1) % points.Length;
        Vector3 start = points[current];
        Vector3 end = points[next];
        float duration = Vector3.Distance(start, end) / speed;

        
        int direction;
        if (end.x > start.x) direction = 3;
        else if (end.x < start.x) direction = 2;
        else if (end.y > start.y) direction = 0;
        else direction = 1;

        if (animator != null)
        {
            animator.SetInteger("Direction", direction);
        }

        tweener.AddTween(transform, start, end, duration);
        current = next;
    }
}