using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {

	// Use this for initialization
	void Start () {
        if (!MainMenu.Exist)
            MainWnd.Instance.Open();
        //音频侦听器移动
        if (Startup.ins.gameObject.GetComponent<AudioListener>() == null)
            Startup.ins.gameObject.AddComponent<AudioListener>();
	}
}
