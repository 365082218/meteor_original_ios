using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PluginCtrl : MonoBehaviour {

    public Image Progress;
    public Image Preview;
    public Text Desc;
    public Button Install;
    public Text Title;
    private void Awake()
    {
        if (Install != null)
            Install.onClick.AddListener(OnInstall); 
    }
    // Update is called once per frame
    void Update () {
		
	}

    void OnInstall()
    {

    }
}
