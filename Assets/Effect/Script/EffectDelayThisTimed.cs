using UnityEngine;
using System.Collections;

public class EffectDelayThisTimed : MonoBehaviour {
	
	public float delayTime = 0.5f;
	
	// Use this for initialization
	void Start () {		
		gameObject.SetActiveRecursively(false);
		Invoke("DelayFunc", delayTime);
	}
	
	void DelayFunc()
	{
		gameObject.SetActiveRecursively(true);
	}
	
}
