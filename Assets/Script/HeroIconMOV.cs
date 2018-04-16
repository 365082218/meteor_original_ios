using UnityEngine;
using System.Collections;

public class HeroIconMOV : MonoBehaviour {

	public bool ifstarticon = false;

	public void Reach()
	{
		ifstarticon = true;
		this.gameObject.GetComponent<BoxCollider>().enabled = true;
		//LineUpWnd.Instance.mlock--;
	}
	public void ComeBack()
	{
		ifstarticon = false;
		this.gameObject.GetComponent<BoxCollider>().enabled = true;
		//LineUpWnd.Instance.mlock--;
	}
}
