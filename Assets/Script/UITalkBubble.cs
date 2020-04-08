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
    //RectTransform rect;
    public bool AutoHide;
    private void Awake()
    {
        //rect = GetComponent<RectTransform>();
        TheCamera = Camera.main.transform;
    }
    // Update is called once per frame
    void Update () {
		if (Panel != null)
			Panel.transform.rotation = TheCamera.transform.rotation;
        if (Time.time - begintime >= showtime)
        {
            GameObject.DestroyImmediate(Panel);
            GameObject.DestroyImmediate(gameObject);
        }
        else
        {
            if (AutoHide && Main.Ins.LocalPlayer != null)
            {
                if (gameObject.activeInHierarchy)
                {
                    if (Vector3.Distance(transform.position, Main.Ins.LocalPlayer.transform.position) >= 50.0f)
                    {
                        gameObject.SetActive(false);
                    }
                }
                else
                {
                    if (Vector3.Distance(transform.position, Main.Ins.LocalPlayer.transform.position) < 50.0f)
                    {
                        gameObject.SetActive(true);
                    }
                }
            }
        }
	}

	public void SetText()
	{
        begintime = Time.time;
        MeteorUnit unit = gameObject.GetComponent<MeteorUnit>();

        //Vector3 vec = Camera.main.WorldToScreenPoint(new Vector3(Panel.transform.position.x, Panel.transform.position.y + 40.0f, Panel.transform.position.z));
        //float scalex = vec.x / Screen.width;
        //float scaley = vec.y / Screen.height;
        //rect.anchoredPosition3D = new Vector3(1920f * scalex, 1080f * scaley, vec.z);

        Vector3 vecPos = new Vector3(transform.position.x, transform.position.y + 15.0f, transform.position.z);
		if (Panel == null)
		{
			Panel = GameObject.Instantiate(Resources.Load("BubbleTalk"), vecPos, Quaternion.identity) as GameObject;
            vecPos.y += 15.0f;
			//iTween.MoveTo(Panel, iTween.Hash(
			//	"position", vecPos,
			//	"time", 0.5f,
			//	"easetype", iTween.EaseType.linear,
			//	"delay", 0.0f));
			Panel.transform.parent = unit.transform;
            GetComponentInChildren<Canvas>().worldCamera = GameObject.Find("CameraEx").GetComponent<Camera>();
            Panel.transform.localScale = new Vector3(8.0f / 425.0f, 2.0f / 80.0f, 1.0f);
        }
		
		label = Panel.GetComponentInChildren<Text>();
        if (label != null)
            label.text = strText;
		Transform t = label.transform;
		Bounds b = NGUIMath.CalculateRelativeWidgetBounds(t);

		Vector3 scale = t.localScale;
		b.min = Vector3.Scale(b.min, scale);
		b.max = Vector3.Scale(b.max, scale);
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
			//iTween.MoveTo(Panel, iTween.Hash(
			//	"position", vecPos,
			//	"time", 0.5f,
			//	"easetype", iTween.EaseType.linear,
			//	"delay", 0.0f));
			Panel.transform.SetParent(gameObject.transform);
            GetComponentInChildren<Canvas>().worldCamera = GameObject.Find("CameraEx").GetComponent<Camera>();
        }
		TheCamera = Camera.main.transform;
		label = Panel.GetComponentInChildren<Text>();
        if (label != null)
            label.text = strText;
        Panel.transform.localScale = new Vector3(25.0f / (label.preferredWidth + 20), 10.0f / (label.preferredHeight + 10), 1.0f);
        //Transform t = label.transform;
        //Bounds b = NGUIMath.CalculateRelativeWidgetBounds(t);

        //Vector3 scale = t.localScale;
        //b.min = Vector3.Scale(b.min, scale);
        //b.max = Vector3.Scale(b.max, scale);
    }
}
