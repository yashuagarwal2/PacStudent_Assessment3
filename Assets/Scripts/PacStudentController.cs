using UnityEngine;

public class PacStudentController : MonoBehaviour
{
    public Tweener tweener;
    public Animator animator;

    void Start()
    {
        if (tweener == null) tweener = FindAnyObjectByType<Tweener>();
        if (animator == null) animator = GetComponent<Animator>();

        animator.SetInteger("Direction", 3); 
        animator.speed = 0;                  
    }

    void Update()
    {
    }
}