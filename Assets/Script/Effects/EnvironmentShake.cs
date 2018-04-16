using UnityEngine;
using System.Collections;

public class EnvironmentShake : MonoBehaviour {
	
	public float MinDelayTime = 0f;
	public float MaxDelayTime = 1f;
	private string[] mName = {"ENVshake_01","ENVshake_02","ENVshake_03"}; 
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	void PlayAnimation()
	{
		int index = UnityEngine.Random.Range(0,3);
		GetComponent<Animation>().Play(mName[index]);
	}
	public void PlayEnvironmentShake()
	{
		float delayTime = UnityEngine.Random.Range(MinDelayTime,MaxDelayTime);
		Invoke("PlayAnimation", delayTime);
	}
}
