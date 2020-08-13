using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetBehaviour:MonoBehaviour {
    protected void Awake()
    {
        FrameReplay.Instance.RegisterObject(this);
    }

    protected void OnDestroy()
    {
        ////Debug.Log(this);
        FrameReplay.Instance.UnRegisterObject(this);
    }


    public virtual void NetUpdate()
    {

    }

    public virtual void NetLateUpdate()
    {

    }
}
