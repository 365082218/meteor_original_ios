using UnityEngine;
using System.Collections;

public class ldaDestroyThisTimed : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

	public void SetDestroyTime(float time) {

		Destroy (gameObject, time);

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
