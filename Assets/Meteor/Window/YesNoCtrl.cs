using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class YesNoCtrl : MonoBehaviour {

    public Text yes;
    public Text no;
    public Text title;
    System.Action onYes;
    public void SetYesNo(string y, string n)
    {
        yes.text = y == null ? "" : y;
        no.text = n == null ? "" : n;
    }

    public void SetTitle(string t, System.Action del)
    {
        title.text = t == null ? "" : t;
        onYes = del;
    }
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnClickYes()
    {
        if (onYes != null)
            onYes.Invoke();
    }

    public void OnClickNo()
    {
        WsWindow.Close(WsWindow.YesNo);
    }
}
