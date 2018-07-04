using UnityEngine;
using System.Collections;

public class Demo_LoadScene : MonoBehaviour {

	public string scene;
	
	public void LoadScene()
	{
		if (!string.IsNullOrEmpty(this.scene))
			Application.LoadLevel(this.scene);
	}
}
