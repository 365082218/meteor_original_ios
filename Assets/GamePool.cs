using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePool : MonoBehaviour {
    static GamePool _Instance;
    public static GamePool Instance { get { return _Instance; } }
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
    public MeshMng MeshMng = null;
}
