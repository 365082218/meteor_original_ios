//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using DG.Tweening;
//using UnityEngine.UI;

//public class BuildListCtrl : MonoBehaviour {
//    public static BuildListCtrl Ins;
//    public Transform BuildListRoot;
//    public GameObject TipPanel;
//    public Text TipText;
//    // Use this for initialization
//    void Awake() { Ins = this; }
//    void OnDestroy() { Ins = null; }
//	void Start () {
	
//	}
	
//	// Update is called once per frame
//	void Update () {
	
//	}
//    bool show = false;
//    public void Open()
//    {
//        transform.SetAsLastSibling();
//        transform.DOLocalMoveX(0.0f, 0.5f);
//        show = true;
//        UpdateUI();
//    }

//    public void OnClose()
//    {
//        transform.DOLocalMoveX(Screen.width, 0.5f);
//        show = false;
//    }
//    Dictionary<int, BuildingItemCtrl> buildingList = new Dictionary<int, BuildingItemCtrl>();
//    public void UpdateUI()
//    {
//        if (!show)
//            return;
//        for (int i = 0; i < GameData.MainRole.EnableBuild.Count; i++)
//        {
//            if (buildingList.ContainsKey(GameData.MainRole.EnableBuild[i]))
//                buildingList[GameData.MainRole.EnableBuild[i]].SetBuilding(GameData.MainRole.EnableBuild[i]);
//            else
//            {
//                GameObject objBuild = Instantiate(Resources.Load("BuildingItem"), BuildListRoot) as GameObject;
//                objBuild.transform.localScale = Vector3.one;
//                objBuild.transform.localRotation = Quaternion.identity;
//                objBuild.transform.localPosition = Vector3.zero;
//                BuildingItemCtrl ctrl = objBuild.GetComponent<BuildingItemCtrl>();
//                ctrl.SetBuilding(GameData.MainRole.EnableBuild[i]);
//                buildingList.Add(GameData.MainRole.EnableBuild[i], ctrl);
//            }
//        }
//    }

//    public void ShowDes(string text)
//    {
//        TipText.text = text;
//        TipPanel.SetActive(true);
//    }

//    public void CloseTip()
//    {
//        TipPanel.SetActive(false);
//    }

//    public void ChangeLang()
//    {
//        //界面上的文字要重新读取其他语言的
//        UpdateUI();
//    }
//}
