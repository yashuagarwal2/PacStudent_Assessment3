using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public enum Type { Speed, Freeze }
    public Type type;
    public float lifetime = 8f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}