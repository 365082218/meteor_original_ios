using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UITalkBubble : MonoBehaviour {
	float begintime;
	public float showtime;
	public string strText;
	Transform TheCamera;
	// Use this for initialization
	GameObject Panel;
	GameObject bgSprite;
	Text label;
	// Update is called once per frame
	void Update () {
		if (Panel != null)
		{
			Panel.transform.rotation = TheCamera.transform.rotation;
//			if (bgSprite != null && label != null)
//			{
//				Bounds bound = NGUIMath.CalculateAbsoluteWidgetBounds(label.transform);
//				bgSprite.transform.localScale = new Vector3(bound.size.x, bound.size.y, 1);
//			}
		}
		if (Time.time - begintime >= showtime)
		{
			GameObject.DestroyImmediate(Panel);
			GameObject.DestroyImmediate(gameObject);
		}
	}

	public void SetText()
	{
		begintime = Time.time;
        MeteorUnit unit = gameObject.GetComponent<MeteorUnit>();
		Vector3 vecPos = new Vector3(transform.position.x, transform.position.y + 15.0f, transform.position.z);
		if (Panel == null)
		{
			Panel = GameObject.Instantiate(Resources.Load("BubbleTalk"), vecPos, Quaternion.identity) as GameObject;
			vecPos.y += 15.0f;
			iTween.MoveTo(Panel, iTween.Hash(
				"position", vecPos,
				"time", 0.5f,
				"easetype", iTween.EaseType.linear,
				"delay", 0.0f));
			Panel.transform.parent = unit.transform;
            Panel.transform.localScale = new Vector3(0.4f, 0.4f, 1.0f);
        }
		TheCamera = Camera.main.transform;
		//bgSprite = Global.ldaControlX("SpriteBg", Panel);
		label = Panel.GetComponentInChildren<Text>();
        if (label != null)
            label.text = strText;
		Transform t = label.transform;
		Bounds b = NGUIMath.CalculateRelativeWidgetBounds(t);

		Vector3 scale = t.localScale;
		b.min = Vector3.Scale(b.min, scale);
		b.max = Vector3.Scale(b.max, scale);
		//bgSprite.transform.localScale = new Vector3(b.size.x,b.size.y,1f);
	}

	public void SetDebugText()
	{
		begintime = Time.time;
        MeteorUnit unit = gameObject.GetComponent<MeteorUnit>();
		if (Panel == null)
		{
			Vector3 vecPos = transform.position;
			Panel = GameObject.Instantiate(Resources.Load("BubbleTalk"), vecPos, Quaternion.identity) as GameObject;
			vecPos.y += 15.0f;
			iTween.MoveTo(Panel, iTween.Hash(
				"position", vecPos,
				"time", 0.5f,
				"easetype", iTween.EaseType.linear,
				"delay", 0.0f));
			Panel.transform.SetParent(gameObject.transform);			
		}
		TheCamera = Camera.main.transform;
		label = Panel.GetComponentInChildren<Text>();
        if (label != null)
            label.text = strText;
		Transform t = label.transform;
		Bounds b = NGUIMath.CalculateRelativeWidgetBounds(t);
		
		Vector3 scale = t.localScale;
		b.min = Vector3.Scale(b.min, scale);
		b.max = Vector3.Scale(b.max, scale);
		//bgSprite.transform.localScale = new Vector3(b.size.x,b.size.y,1f);
	}
}
