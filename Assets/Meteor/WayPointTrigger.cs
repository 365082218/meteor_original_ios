using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPointTrigger : MonoBehaviour {

	// Use this for initialization
	//void Start () {
		
	//}
	
	//// Update is called once per frame
	//void Update () {
		
	//}

    public int WayIndex;
    private void OnTriggerEnter(Collider other)
    {
        MeteorUnit user = other.GetComponentInParent<MeteorUnit>();
        if (user.robot != null)
            user.robot.OnGotoWayPoint(WayIndex);
    }

    private void OnTriggerStay(Collider other)
    {
        MeteorUnit user = other.GetComponentInParent<MeteorUnit>();
        if (user.robot != null)
            user.robot.OnGotoWayPoint(WayIndex);
    }
}
