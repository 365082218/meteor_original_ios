using UnityEngine;
using System.Collections;

public class LookAtCamara: NetBehaviour 
{
    protected new void Awake()
    {
        base.Awake();
    }

    protected new void OnDestroy()
    {
        base.OnDestroy();
    }

    public override void NetLateUpdate()
    {
        if (CameraFollow.Ins != null)
        {
            transform.LookAt(CameraFollow.Ins.transform);
            transform.Rotate(new Vector3(-90, 0, 0));
        }
    }
}
