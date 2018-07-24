using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetTrigger : MonoBehaviour {

    public int TargetIndex;
    public string style;//类型 char/waypoint
    public int param;//序号（人物/路点）
    private void OnTriggerEnter(Collider other)
    {
       MeteorUnit unit = other.GetComponentInParent<MeteorUnit>();
       if (unit != null)
        {
            if (unit.robot != null)
                unit.robot.OnGotoTargetPoint(TargetIndex);
        }
    }
}
