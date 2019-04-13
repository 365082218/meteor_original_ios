using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class NpcFunCtrl : MonoBehaviour {
    public Text FunName;
    NpcFunction Fun;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnClick()
    {
        if (Fun != null)
            ScriptMng.Instance.CallScript(Fun.Script);
    }

    public void Attach(NpcFunction fun)
    {
        Fun = fun;
        FunName.text = Fun.Name;
    }
}
