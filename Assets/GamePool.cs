using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePool : MonoBehaviour {
    static GamePool _Instance;
    public static GamePool Instance { get { return _Instance; } }
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
    private GameObject DebugCanvas;
    void InitDebugSetting()
    {
        DebugCanvas = GameObject.Instantiate(ResMng.LoadPrefab("DebugCanvas")) as GameObject;
        DebugCanvas.transform.SetParent(transform);
        DebugCanvas.transform.localScale = Vector3.one;
        DebugCanvas.transform.rotation = Quaternion.identity;
        DebugCanvas.transform.position = Vector3.zero;
        DebugCanvas.SetActive(false);
    }

    public void ShowDbg()
    {
        DebugCanvas.SetActive(true);
    }

    public void CloseDbg()
    {
        DebugCanvas.SetActive(false);
    }
#endif
}
