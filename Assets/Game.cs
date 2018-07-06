using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour {
    static Game _Instance;
    public static Game Instance { get { return _Instance; } }
    private void Awake()
    {
        _Instance = this;
#if !STRIP_DBG_SETTING
        InitDebugSetting();
#endif
    }
    private void OnDestroy()
    {
        _Instance = null;
    }
    
    public SkcMatMng SkcMng = null;
    public EffectPoolManager EffectMng = null;
    public MeshMng MeshMng = null;

#if !STRIP_DBG_SETTING
    void InitDebugSetting()
    {
        GameObject DebugCanvas = GameObject.Instantiate(Resources.Load<GameObject>("DebugCanvas"));
        DebugCanvas.transform.SetParent(transform);
        DebugCanvas.transform.localScale = Vector3.one;
        DebugCanvas.transform.rotation = Quaternion.identity;
        DebugCanvas.transform.position = Vector3.zero;
    }
#endif
}
