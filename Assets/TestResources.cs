using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestResources : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GameObject.Instantiate(Resources.Load("1_material_1"));
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
