using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectDebug : MonoBehaviour {

    // Use this for initialization
    void Start () {
        Main.Instance.SFXLoader.InitSync();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void PlayEffect(string sfx)
    {
        sfx += ".ef";
        Main.Instance.SFXLoader.PlayEffect(sfx, gameObject);
    }

    public void PlayEffect(int idx)
    {
        Main.Instance.SFXLoader.PlayEffect(idx, gameObject);
    }
}
