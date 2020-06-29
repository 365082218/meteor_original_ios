using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectDebug : MonoBehaviour {

    // Use this for initialization
    void Start () {
        Main.Ins.SFXLoader.InitSync();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void PlayEffect(string sfx)
    {
        sfx += ".ef";
        Main.Ins.SFXLoader.PlayEffect(sfx, gameObject);
    }

    public void PlayEffect(int idx)
    {
        Main.Ins.SFXLoader.PlayEffect(idx, gameObject);
    }
}
