using UnityEngine;
using System.Collections;
using SLua;
using UnityEngine.UI;

public class NpcMenuCallCtrl : MonoBehaviour {
    LuaFunction fun;
    public Text menuLabel;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Attach(string menu, LuaFunction function)
    {
        fun = function;
        menuLabel.text = menu;
    }

    public void OnClick()
    {
        if (fun != null)
            fun.call();
    }
}
