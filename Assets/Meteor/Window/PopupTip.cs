using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PopupTip : MonoBehaviour {

    public Text message;
    public Text title;
    public float duraction = 3.0f;
	// Use this for initialization
	void Start () {
        title.text = "消息";
	}
	
	// Update is called once per frame
	void Update () {
        duraction -= Time.deltaTime;
        if (duraction <= 0.0f)
            DestroyImmediate(gameObject);
    }

    public void Popup(string str)
    {
        message.text = str;
        gameObject.FlyTo(duraction - 1.0f, 100.0f);
    }
}
