using UnityEngine;
using System.Collections;

public class AutoTipsControl : MonoBehaviour {
	UILabel label;
	// Use this for initialization
	void Start () {
		label = transform.Find("Label").GetComponent<UILabel>() as UILabel;
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	public void StartAction(Transform parent, string tips, Vector3 pos, Vector3 scale, string color = "[ffffff]")
	{
		label = transform.Find("Label").GetComponent<UILabel>() as UILabel;
		label.transform.localPosition = new Vector3(label.transform.localPosition.x, label.transform.localPosition.y, -510);
		label.transform.localPosition = new Vector3(0.001f, 0.001f, 1);
		transform.parent = parent;
		label.text = color + tips + "[-]";
		transform.localPosition = pos;
		transform.localScale = scale;

        iTween.ValueTo(gameObject, iTween.Hash(new object[] { "from", 0.001f, "to", 1, "time", 0.5f, "onupdate", "OnScaleUpdate", "oncomplete", "OnScaleComplete", "ignoretimescale",true }));
	}

	public void OnScaleUpdate(float value)
	{
		transform.localScale = new Vector3(value, value, 1);
		transform.localPosition = new Vector3 (transform.localPosition.x, transform.localPosition.y, -510);
	}

	public void OnScaleComplete()
	{
        iTween.ValueTo(gameObject, iTween.Hash(new object[] { "from", 0, "to", 100, "time", 0.6f, "onupdate", "OnMoveUpdate", "oncomplete", "OnMoveComplete", "ignoretimescale", true }));
	}

	public void OnMoveUpdate(float value)
	{
		transform.localPosition = new Vector3(0, value, -510);
	}

	public void OnMoveComplete()
	{
		GameObject.Destroy(gameObject);
	}
}
