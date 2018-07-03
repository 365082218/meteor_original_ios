using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour {
    static Game _Instance;
    public static Game Instance { get { return _Instance; } }
    private void Awake()
    {
        _Instance = this;
    }
    private void OnDestroy()
    {
        _Instance = null;
    }
    public SkcMatMng SkcMng = null;
    public EffectPoolManager EffectMng = null;
    //[SerializeField] private 
}
