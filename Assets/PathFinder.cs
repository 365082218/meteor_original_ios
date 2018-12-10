using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinder : MonoBehaviour {

    // Use this for initialization
    public Transform Begin;
    public Transform End;
    public MeteorUnit owner;
    public WayPoint[] way;
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void FindPath()
    {
        List<WayPoint> wp = new List<WayPoint>();
        PathMng.Instance.FindPath(owner, Begin.transform.position, End.transform.position, ref wp);
        way = wp.ToArray();
    }
}
