using System.Collections.Generic;
using UnityEngine;

public class Tweener : MonoBehaviour
{
    class Tween
    {
        public Transform target;
        public Vector3 start, end;
        public float startTime, duration;
    }

    List<Tween> tweens = new List<Tween>();

    public void AddTween(Transform target, Vector3 start, Vector3 end, float duration)
    {
        tweens.Add(new Tween
        {
            target = target,
            start = start,
            end = end,
            startTime = Time.time,
            duration = duration
        });
    }

    public bool TweenExists(Transform target)
    {
        return tweens.Exists(t => t.target == target);
    }

    void Update()
    {
        for (int i = tweens.Count - 1; i >= 0; i--)
        {
            Tween tween = tweens[i];
            float t = tween.duration > 0 ? (Time.time - tween.startTime) / tween.duration : 1f;

            tween.target.position = Vector3.Lerp(tween.start, tween.end, t);

            if (t >= 1f)
                tweens.RemoveAt(i);
        }
    }
}