using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LinkLabel : MonoBehaviour {
    public string URL;
	// Use this for initialization
    protected void Awake()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(() => {
            if (!string.IsNullOrEmpty(URL))
                Application.OpenURL(URL);
        });
    }
}
