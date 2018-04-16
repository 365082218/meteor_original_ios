using UnityEngine;
using System.Collections;

public class PlayAllAnimation : MonoBehaviour {

	public float RotateSpeed = 50f;

	// Use this for initialization
	private ArrayList aniList=new ArrayList();
	int p=0;
	void Start () {
		if(GetComponent<Animation>() == null)return;
		foreach (AnimationState ast in GetComponent<Animation>()){
			aniList.Add(ast.name);
		}
		PlayNext();
	}
	public void PlayNext(){
		if(aniList.Count>0 && GetComponent<Animation>()!=null ){
			if(p==aniList.Count)p=0;
			string n=(string)aniList[p];
			float l=GetComponent<Animation>()[n].clip.length+0.1f;
			GetComponent<Animation>().CrossFade(n,0.1f);
			p++;
			Invoke("PlayNext",l);
		}
	}

	void Update () {

		if (Input.GetMouseButton(0))
		{
			float m_movX = Input.GetAxis("Mouse X") * RotateSpeed * Time.deltaTime;
			float m_movY = Input.GetAxis("Mouse Y") * RotateSpeed * Time.deltaTime;

			if(m_movX != 0)
			{
				int dfsff =9999;
			}

			//target.localRotation = Quaternion.Euler(0, -0.5f * delta.x * speed, 0f) * target.localRotation;
			this.transform.Rotate (new Vector3 (0f, -0.5f * m_movX * RotateSpeed, 0f) , Space.Self);
			//this.transform.Rotate (new Vector3 (-0.5f * m_movY * RotateSpeed, -0.5f * m_movX * RotateSpeed, 0f) , Space.Self);

		}
	}
}

