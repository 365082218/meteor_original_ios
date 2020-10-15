using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CalcDistance : MonoBehaviour {
    public GameObject target0;
    public GameObject target1;
    public float dis;
    public float angle;
    public void CalcAngle() {
        float rdot = Utility.GetAngleBetween(target0.transform.forward, (target1.transform.position - target0.transform.position).normalized);
        float r = Mathf.Acos(rdot);
        angle = Mathf.Rad2Deg * r;
    }

    public void CalcDis() {
        dis = Vector3.Distance(target0.transform.position, target1.transform.position);
    }

    //令target0朝向target1
    public void LookAt() {
        target0.transform.LookAt(target1.transform);
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
