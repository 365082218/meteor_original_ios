using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TestUGUI : MonoBehaviour {

    // Use this for initialization
    UnitTopUITest UnitTopUI;

    void Start () {
        for (int i = 0; i < 10; i++)
        {
            UnitTopUI = (GameObject.Instantiate(Resources.Load("UnitTopUITest")) as GameObject).GetComponent<UnitTopUITest>();
            UnitTopUI.Init(string.Format("{0}", i), Vector3.zero);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
