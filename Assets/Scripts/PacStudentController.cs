using UnityEngine;
#if UNITY_EDITOR
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
#endif

public class PacStudentController : MonoBehaviour
{
    private const string DirectionParam = "Direction";

    [SerializeField] private Tweener tweener;
    [SerializeField] private Animator animator;
    [SerializeField] private float speed = 4f;   // tiles per second

    // Clockwise loop around the block (world units)
    private Vector3[] waypoints = new Vector3[]
    {
        new Vector3(1f, -1f, 0f),
        new Vector3(6f, -1f, 0f),
        new Vector3(6f, -5f, 0f),
        new Vector3(1f, -5f, 0f)
    };

    private int currentIndex;
    private bool canSetDirection;   // true only if the Animator has an Int parameter called "Direction"

    void Awake()
    {
        // Fill any empty Inspector slots automatically so the script always runs
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        if (tweener == null)
        {
#if UNITY_2023_1_OR_NEWER
            tweener = FindFirstObjectByType<Tweener>();
#else
            tweener = FindObjectOfType<Tweener>();
#endif
        }

        if (tweener == null)
        {
            Debug.LogWarning("No Tweener found in the scene, adding one to PacStudent.", this);
            tweener = gameObject.AddComponent<Tweener>();
        }

        CheckDirectionParameter();
    }

    void Start()
    {
        // Put PacStudent on the first waypoint, then start the first leg
        currentIndex = 0;
        transform.position = waypoints[currentIndex];
        StartNextLeg();
    }

    void Update()
    {
        // If PacStudent isn't tweening right now, the last leg has finished
        if (!tweener.TweenExists(transform))
        {
            StartNextLeg();
        }
    }

    // Looks once for the "Direction" Int parameter so we never call SetInteger on a missing one
    private void CheckDirectionParameter()
    {
        canSetDirection = false;

        if (animator == null || animator.runtimeAnimatorController == null)
        {
            Debug.LogWarning("PacStudent has no Animator Controller assigned. Movement will still work.", this);
            return;
        }

        foreach (AnimatorControllerParameter p in animator.parameters)
        {
            if (p.name == DirectionParam && p.type == AnimatorControllerParameterType.Int)
            {
                canSetDirection = true;
                break;
            }
        }

        if (!canSetDirection)
        {
            Debug.LogWarning("The Animator has no Int parameter named 'Direction'. Movement will still work, but the animation won't change direction. " +
                             "Fix: click the ⋮ menu on this component and choose 'Set Up Direction Parameter'.", this);
        }
    }

    private void StartNextLeg()
    {
        // Start = current waypoint, end = next waypoint (% wraps back to 0 at the end)
        int nextIndex = (currentIndex + 1) % waypoints.Length;
        Vector3 start = waypoints[currentIndex];
        Vector3 end = waypoints[nextIndex];

        // Distance / speed keeps the speed the same on every leg
        float duration = Vector3.Distance(start, end) / Mathf.Max(speed, 0.01f);

        // Work out which way this leg is heading:
        // up = 0, down = 1, left = 2, right = 3
        float dx = end.x - start.x;
        float dy = end.y - start.y;
        int direction;

        if (Mathf.Abs(dx) > Mathf.Abs(dy))
        {
            direction = dx > 0 ? 3 : 2;   // right : left
        }
        else
        {
            direction = dy > 0 ? 0 : 1;   // up : down
        }

        // Set the animation direction at the moment the leg begins so turning is instant
        if (canSetDirection)
        {
            animator.SetInteger(DirectionParam, direction);
        }

        tweener.AddTween(transform, start, end, duration);

        currentIndex = nextIndex;
    }

#if UNITY_EDITOR
    // One-click setup (Editor only): click the ⋮ menu on this component in the Inspector and choose
    // "Set Up Direction Parameter". Adds the Int parameter "Direction" to the Animator Controller and
    // adds Any State transitions for states named Up / Down / Left / Right (0 / 1 / 2 / 3).
    [ContextMenu("Set Up Direction Parameter")]
    private void SetUpDirectionParameter()
    {
        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("Stop Play mode", "Stop Play mode first, then run this again.", "OK");
            return;
        }

        Animator a = animator != null ? animator : GetComponentInChildren<Animator>();
        if (a == null || a.runtimeAnimatorController == null)
        {
            EditorUtility.DisplayDialog("No Animator Controller",
                "PacStudent has no Animator with a Controller assigned.\nAssign one in the Animator component first.", "OK");
            return;
        }

        // Handle an Animator Override Controller by using the controller it is based on
        RuntimeAnimatorController rc = a.runtimeAnimatorController;
        AnimatorOverrideController overrideController = rc as AnimatorOverrideController;
        if (overrideController != null)
        {
            rc = overrideController.runtimeAnimatorController;
        }

        AnimatorController controller = rc as AnimatorController;
        if (controller == null)
        {
            EditorUtility.DisplayDialog("Unsupported controller", "Could not open that Animator Controller for editing.", "OK");
            return;
        }

        // 1. Make sure the Int parameter "Direction" exists
        bool hasParam = false;
        foreach (AnimatorControllerParameter p in controller.parameters)
        {
            if (p.name == DirectionParam)
            {
                if (p.type == AnimatorControllerParameterType.Int)
                {
                    hasParam = true;
                }
                else
                {
                    EditorUtility.DisplayDialog("Wrong parameter type",
                        "A parameter called 'Direction' already exists but it is not an Int.\nDelete it in the Animator window's Parameters tab and run this again.", "OK");
                    return;
                }
            }
        }

        if (!hasParam)
        {
            controller.AddParameter(DirectionParam, AnimatorControllerParameterType.Int);
        }

        // 2. Add Any State transitions for states named Up / Down / Left / Right
        AnimatorStateMachine sm = controller.layers[0].stateMachine;
        int wired = 0;

        foreach (ChildAnimatorState child in sm.states)
        {
            AnimatorState state = child.state;
            int value = DirectionFromName(state.name);

            if (value < 0 || HasDirectionTransition(sm, state))
            {
                continue;
            }

            AnimatorStateTransition t = sm.AddAnyStateTransition(state);
            t.AddCondition(AnimatorConditionMode.Equals, value, DirectionParam);
            t.hasExitTime = false;          // turn instantly
            t.duration = 0f;
            t.canTransitionToSelf = false;  // don't restart the animation every leg
            wired++;

            Debug.Log("Wired state '" + state.name + "' to Direction = " + value);
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        string message = "Parameter 'Direction' is ready.\nDirection transitions added: " + wired;
        if (wired == 0)
        {
            message += "\n\nNo new transitions were needed, or no states are named Up/Down/Left/Right. " +
                       "If your walk states have other names, add the transitions by hand in the Animator window.";
        }

        EditorUtility.DisplayDialog("Done", message, "OK");
    }

    // "WalkUp", "walk_up", "Walk Up" -> 0. Only whole words count, so "Setup" is ignored.
    private static int DirectionFromName(string stateName)
    {
        string spaced = Regex.Replace(stateName, "([a-z])([A-Z])", "$1 $2").ToLowerInvariant();
        string[] tokens = spaced.Split(new char[] { ' ', '_', '-', '.' }, System.StringSplitOptions.RemoveEmptyEntries);

        foreach (string token in tokens)
        {
            if (token == "up") return 0;
            if (token == "down") return 1;
            if (token == "left") return 2;
            if (token == "right") return 3;
        }
        return -1;
    }

    private static bool HasDirectionTransition(AnimatorStateMachine sm, AnimatorState state)
    {
        foreach (AnimatorStateTransition t in sm.anyStateTransitions)
        {
            if (t.destinationState != state)
            {
                continue;
            }

            foreach (AnimatorCondition c in t.conditions)
            {
                if (c.parameter == DirectionParam)
                {
                    return true;
                }
            }
        }
        return false;
    }
#endif
}