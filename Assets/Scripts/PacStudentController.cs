#if UNITY_EDITOR
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// One-click setup: adds the Int parameter "Direction" to the selected object's
// Animator Controller, and adds Any State transitions for states named
// Up / Down / Left / Right (up = 0, down = 1, left = 2, right = 3).
public static class AddDirectionParameter
{
    private const string ParamName = "Direction";

    [MenuItem("Tools/Set Up Direction Parameter (select PacStudent first)")]
    private static void Run()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("Stop Play mode", "Stop Play mode first, then run this again.", "OK");
            return;
        }

        GameObject go = Selection.activeGameObject;
        if (go == null)
        {
            EditorUtility.DisplayDialog("Nothing selected", "Click PacStudent in the Hierarchy first, then run this again.", "OK");
            return;
        }

        Animator animator = go.GetComponent<Animator>();
        if (animator == null)
        {
            animator = go.GetComponentInChildren<Animator>();
        }

        if (animator == null || animator.runtimeAnimatorController == null)
        {
            EditorUtility.DisplayDialog("No Animator Controller",
                "The selected object has no Animator with a Controller assigned.\nAssign one in the Animator component first.", "OK");
            return;
        }

        // Handle an Animator Override Controller by using the controller it is based on
        RuntimeAnimatorController rc = animator.runtimeAnimatorController;
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
            if (p.name == ParamName)
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
            controller.AddParameter(ParamName, AnimatorControllerParameterType.Int);
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
            t.AddCondition(AnimatorConditionMode.Equals, value, ParamName);
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
            message += "\n\nNo new transitions were needed or no states are named Up/Down/Left/Right. " +
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
                if (c.parameter == ParamName)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
#endif