using System.Collections.Generic;
using UnityEngine;

public class Tweener : MonoBehaviour
{
    private class Tween
    {
        public Transform Target;
        public Vector3 StartPos;
        public Vector3 EndPos;
        public float StartTime;
        public float Duration;
    }

    private List<Tween> activeTweens = new List<Tween>();

    public void AddTween(Transform target, Vector3 start, Vector3 end, float duration)
    {
        Tween tween = new Tween();
        tween.Target = target;
        tween.StartPos = start;
        tween.EndPos = end;
        tween.StartTime = Time.time;
        tween.Duration = duration;

        activeTweens.Add(tween);
    }

    public bool TweenExists(Transform target)
    {
        foreach (Tween tween in activeTweens)
        {
            if (tween.Target == target)
            {
                return true;
            }
        }
        return false;
    }

    void Update()
    {
        
        for (int i = activeTweens.Count - 1; i >= 0; i--)
        {
            Tween tween = activeTweens[i];

            
            float t = tween.Duration > 0f
                ? (Time.time - tween.StartTime) / tween.Duration
                : 1f;

            if (t >= 1f)
            {
               
                tween.Target.position = tween.EndPos;
                activeTweens.RemoveAt(i);
            }
            else
            {
                
                tween.Target.position = tween.StartPos + (tween.EndPos - tween.StartPos) * t;
            }
        }
    }
}