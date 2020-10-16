using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

public class NetBehaviour:MonoBehaviour {
    protected void Awake()
    {
        if (FrameReplay.Ins != null)
            FrameReplay.Ins.RegisterObject(this);
    }

    protected void OnDestroy()
    {
        if (FrameReplay.Ins != null)
            FrameReplay.Ins.UnRegisterObject(this);
    }

    public virtual void NetUpdate()
    {

    }

    public virtual void NetLateUpdate()
    {

    }

    public virtual void Write(BinaryWriter writer) {

    }

    public virtual void Read(BinaryReader reader) {

    }
}
