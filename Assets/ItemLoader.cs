using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemLoader : MonoBehaviour {

    [SerializeField]
    string model;
    // Use this for initialization
    void Start () {
        SceneItemAgent target = gameObject.AddComponent<SceneItemAgent>();
        target.tag = "SceneItemAgent";
        target.Load(model);
        target.LoadCustom(name, null);//自定义的一些属性，name=damage100
        target.ApplyPost();
        if (target.root != null)
            target.root.gameObject.SetActive(true);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
