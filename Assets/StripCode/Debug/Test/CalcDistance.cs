using UnityEngine;
using System.Collections;

public class CalcDistance : MonoBehaviour {

    public GameObject targetA;
    public GameObject targetB;
    public float distance;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Calc()
    {
        if (targetA != null && targetB != null)
        {
            distance = (targetA.transform.position - targetB.transform.position).magnitude;
        }
    }
}
