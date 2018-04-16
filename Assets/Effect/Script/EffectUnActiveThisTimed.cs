using UnityEngine;
using System.Collections;

public class EffectUnActiveThisTimed : MonoBehaviour {

    public float unActiveTime = 0.5f;

    //void Start()
    //{
    //    Invoke("UnActiveFunc", unActiveTime);
    //}

    void OnEnable()
    {
        Invoke("UnActiveFunc", unActiveTime);
    }

    void UnActiveFunc()
    {
        gameObject.SetActive(false);
    }
}
