using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StateItemCtrl : MonoBehaviour {
    public Text t;
    public Text level;
    public Text sceneName;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetInfo(string ts, string l, string n)
    {
        t.text = ts;
        level.text = l;
        sceneName.text = n;
    }
}
