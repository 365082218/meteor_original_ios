using UnityEngine;
using System.Collections;

public class CloseMenuCtrl : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    System.Action onclick;
    public void SetHandler(System.Action handler)
    {
        onclick = handler;
    }

    public void OnClick()
    {
        if (onclick != null)
            onclick.Invoke();
    }
}
