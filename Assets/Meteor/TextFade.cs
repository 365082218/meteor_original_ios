using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class TextFade : MonoBehaviour {
	public Text[] target;
	public Text[] target2;
	Text[] cur = null;
	int idx = 0;
	float duration = 3.0f;
	float wait = 1.0f;//在每段话渐变结束后多久开始下一段话渐变
	float firstWait = 1.0f;//第一段话在多久后开始渐变
	float alllast = 2.0f;//整个渐变结束后持续显示多久
	float allfadeout = 2.0f;
	// Use this for initialization
	void Start () {
		cur = target;
		Invoke ("Fade", firstWait);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void Fade()
	{
		if (cur.Length > idx && cur [idx] != null) {
			cur [idx].CrossFadeAlpha (0, 0.0f, false);
			cur [idx].gameObject.SetActive (true);
			cur [idx].CrossFadeAlpha (1, duration, false);
			Invoke ("OnFadeComplete", duration);
		} 
		else 
		{
			Invoke ("FadeOut", alllast);
		}
	}

	void OnFadeComplete()
	{
		idx++;
		Invoke ("Fade", wait);
	}

	void FadeOut()
	{
		for (int i = 0; i < cur.Length; i++) {
			cur [i].CrossFadeAlpha (0, allfadeout, false);
		}

		Invoke ("GotoNextPage", allfadeout);
	}

	void GotoNextPage()
	{
		//if (cur == target) {
		//	cur = target2;
		//	idx = 0;
		//	Fade ();
		//} else {
			gameObject.SetActive (false);
            Startup.ins.OnGameStart();
            //Camera.main.GetComponent<Startup> ().EntryNextTitle ();
		//}
	}
}
