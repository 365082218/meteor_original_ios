using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
public class UnitTopUI : MonoBehaviour {
    public Text MonsterName;
    Transform target;
    RectTransform rect;
    MeteorUnit owner;
    UnitDebugInfo Info;
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
        if (owner == null)
            return;
        if (owner == MeteorManager.Instance.LocalPlayer)
            return;
        float unitDis = Vector3.Distance(owner.transform.position, MeteorManager.Instance.LocalPlayer.transform.position);
        if ((unitDis <= ViewLimit) && !MonsterName.gameObject.activeSelf)
            MonsterName.gameObject.SetActive(true);
        else
        {
            if ((unitDis > ViewLimit) && MonsterName.gameObject.activeSelf)
            {
                MonsterName.gameObject.SetActive(false);
                if (Info.gameObject.activeInHierarchy)
                    Info.gameObject.SetActive(false);
            }
        }
        //1.6f 1.7666f
        if (MonsterName.gameObject.activeSelf)
        {
            Vector3 vec = Camera.main.WorldToScreenPoint(new Vector3(target.position.x, target.position.y + 40.0f, target.position.z));
            float scalex = vec.x / Screen.width;
            float scaley = vec.y / Screen.height;
            rect.anchoredPosition3D = new Vector3(1920f * scalex, 1080f * scaley, vec.z);
            if (Startup.ins.state.EnableDebug)
            {
                if (!Info.gameObject.activeInHierarchy)
                    Info.gameObject.SetActive(true);
            }
         }
    }

    public void Init(MonsterEx attr, Transform attach, EUnitCamp camp)
    {
        owner = attach.GetComponent<MeteorUnit>();
        Info = GetComponentInChildren<UnitDebugInfo>(true);
        Info.SetOwner(owner);
        Transform mainCanvas = GameObject.Find("Canvas").transform;
        transform.SetParent(mainCanvas);
        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;
        transform.SetAsLastSibling();
        //rect.anchoredPosition3D = new Vector3(0, 40.0f, 0);
        //transform.localScale = Vector3.one * 0.1f;//0.07-0.03 0.07是主角与敌人距离最近时，0.03是摄像机与敌人最近时，摄像机与敌人最近时，要看得到敌人，必须距离 44左右
        MonsterName.text = attr == null ? "" : attr.Name;
        if (camp == EUnitCamp.EUC_ENEMY)
            MonsterName.color = Color.red;
        else if (camp == EUnitCamp.EUC_NONE)
            MonsterName.color = Color.white;
        target = attach;
        if (owner == MeteorManager.Instance.LocalPlayer)
            MonsterName.gameObject.SetActive(false);
     }
}
