using UnityEngine;
using System.Collections;

public class EffectTime : MonoBehaviour
{
    public float DestroyTime = 1.0f;

    [HideInInspector]
    public ObjectPool Pool;

    void OnEnable()
    {
        Invoke("OnEffectEnd", DestroyTime);
    }

    void OnEffectEnd()
    {
        if (Pool)
            Pool.Recycle(gameObject);
        else
            Destroy(gameObject);
    }
}
