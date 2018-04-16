using UnityEngine;
using System.Collections;

public class ChapterTitleFaceBorad : MonoBehaviour
{
	Transform cameraTarget;
	// Use this for initialization
	void Start()
	{
		cameraTarget = GameObject.Find("WorldmapRoot").transform.Find("CameraPos0");
	}

    void LateUpdate() 
    {
		if (cameraTarget!=null)
        	transform.rotation = cameraTarget.rotation;
    }
	
	void OnBecameVisible() 
	{
		Debug.Log("BecameVisible");
   		this.enabled = true;
	}
	void OnBecameInvisible()
	{
		Debug.Log("BecameInvisible");
		this.enabled = false;
	}
}

