using UnityEngine;
using System.Collections;

public class LookAtCamara : LockBehaviour 
{
    protected override void LockUpdate()
    {
        base.LockUpdate();
        if (CameraFollow.Ins != null)
        {
            transform.LookAt(CameraFollow.Ins.transform);
            transform.Rotate(new Vector3(-90, 0, 0));
        }
    }
}
