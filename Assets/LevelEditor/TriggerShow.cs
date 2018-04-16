using UnityEngine;
using System.Collections;

public class TriggerShow : MonoBehaviour {

    //x向右扫描特效区
    //z向内扫描特效区
    public static TriggerShow TriggerShowControl;
    public GameObject[] mControlList;
    public GameObject ShowTarget;
    GameObject Terrain;
    //控制陷阱的显示相机和一些.
	// Use this for initialization
	void Start () {
        TriggerShowControl = this;
        GameObject.DontDestroyOnLoad(gameObject);
        foreach (var son in mControlList)
        {
            GameObject.DontDestroyOnLoad(son);
        }
        Terrain = GameObject.Find("Terrain");
        Show(false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Show(bool bShow = true)
    {
        foreach (var son in mControlList)
        {
            son.SetActive(bShow);
        }
        gameObject.SetActive(bShow);
        if (ShowTarget != null && !bShow)
            GameObject.DestroyImmediate(ShowTarget);
    }

    

    
}
