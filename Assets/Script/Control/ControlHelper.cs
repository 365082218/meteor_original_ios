using UnityEngine;
using System.Collections;

public class ControlHelper : MonoBehaviour
{
	public delegate void UpdateControl();
	public UpdateControl ControlHelpUpdate;
	
	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(ControlHelpUpdate != null){
			ControlHelpUpdate();
		}
	}
}

