using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using CoClass;
public class ItemTopUI : MonoBehaviour {
    public Text ItemName;
    public Transform Panel;
    Transform target;
    RectTransform rect;
    InventoryItem Item;
    const float ViewLimit = 60.0f;
    // Use this for initialization
    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }
    void Start () {
	    
	}
	
	// Update is called once per frame
	void LateUpdate () {
        if (Global.PauseAll)
            return;
        float unitDis = Vector3.Distance(transform.position, MeteorManager.Instance.LocalPlayer.transform.position);
        if (unitDis <= ViewLimit)
            Panel.gameObject.SetActive(true);
        else
        {
            if (unitDis > ViewLimit)
            {
                if (Panel.gameObject.activeInHierarchy)
                    Panel.gameObject.SetActive(false);
            }
        }
        //1.6f 1.7666f
        //Vector3 vec = Camera.main.WorldToScreenPoint(new Vector3(target.position.x, target.position.y + 40.0f, target.position.z));
        //float scalex = vec.x / Screen.width;
        //float scaley = vec.y / Screen.height;
        //rect.anchoredPosition3D = new Vector3(1920f * scalex, 1080f * scaley, vec.z);
        Vector3 vec = new Vector3(target.position.x, target.position.y + 40.0f, target.position.z);
        transform.position = vec;
        if (GameData.Instance.gameStatus.EnableItemName)
        {
            if (!Panel.gameObject.activeInHierarchy)
                Panel.gameObject.SetActive(true);
        }
    }

    public void Init(InventoryItem it, Transform attach)
    {
        transform.SetParent(null);
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;
        transform.GetComponent<Canvas>().worldCamera = U3D.GetMainCamera();
        //rect.anchoredPosition3D = new Vector3(0, 40.0f, 0);
        //transform.localScale = Vector3.one * 0.1f;//0.07-0.03 0.07是主角与敌人距离最近时，0.03是摄像机与敌人最近时，摄像机与敌人最近时，要看得到敌人，必须距离 44左右
        ItemName.text = it == null ? "" : it.Name();
        target = attach;
     }
}
