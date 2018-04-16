using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ldaFreezeControl : MonoBehaviour {

    public float FreezeTime_ = 0.3f;

    public GameObject BottomObject = null;

    //public GameObject MiddleObject = null;

    public List<GameObject> needRemoveFreezeObjects = null;

    public Material iceMaterial;

	// Use this for initialization
	void Start () {

        FreezeBehaviour fb;
        if(BottomObject!=null)
        {
            fb = BottomObject.GetComponent<FreezeBehaviour>();
            if(fb!=null)
            {
                fb.FreezeTime = FreezeTime_;
                fb.isFrozen = true;
            }
        }
	}

    public void AddRemoveFreezeObject(GameObject go)
    {
        if(needRemoveFreezeObjects==null)
        {
            needRemoveFreezeObjects = new List<GameObject>();
        }
        needRemoveFreezeObjects.Add(go);
    }

    void OnDisable()
    {
        FreezeBehaviour fb;
        for(int i=0; i<needRemoveFreezeObjects.Count; i++)
        {
            //移除组件
            fb = needRemoveFreezeObjects[i].GetComponent<FreezeBehaviour>();
            if (fb != null)
                Destroy(fb);
        }
    }
	
	// Update is called once per frame
    //void Update () {
	
    //}
}
