using UnityEngine;
using System.Collections;

public class ReadyHero : MonoBehaviour {

	// Use this for initialization
	//public  Vector3 org;
	public 	static GameObject ChosenButton;
	public static string whichone;
	private bool ifcanchange = true;

	public bool takeCanChange { get { return ifcanchange; } set { ifcanchange = value; } }

	void Awake()
	{
		//org = transform.TransformPoint (transform.position);
		gameObject.GetComponent<BoxCollider> ().enabled = false;
	}

	void Start () {
		this.gameObject.transform.position = transform.parent.Find("ButtonHeroF01").position;
	}


	public void compl()
	{
		gameObject.GetComponent<BoxCollider> ().enabled = true;
		transform.parent.Find("ButtonHeroF01").gameObject.GetComponent<BoxCollider> ().enabled = true;
	}

	public void comb()
	{
		transform.parent.Find("ButtonHeroF01").gameObject.GetComponent<BoxCollider> ().enabled = true;

	}
	// Update is called once per frame

}
