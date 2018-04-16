//using UnityEngine;
//using System.Collections;
//using UnityEngine.UI;
//using CoClass;
//public class BuildingItemCtrl : MonoBehaviour {

//    public Image created;
//    public Image newBuild;
//    public Button OnCreate;
//    public Button Tip;
//    public Text buildName;
//    public Text Item1;
//    public Text Item2;
//    public Text Item3;
//    public Button Level;

//    // Use this for initialization
//    void Start () {
	
//	}
	
//	// Update is called once per frame
//	void Update () {
	
//	}

//    public void SetBuilding(int Idx)
//    {
//        foreach (var each in WorkFactory.Ins.Factory)
//        {
//            if (each.Value.BuildingPropertyIdx == Idx)
//            {
//                AttachBuilding(each.Value);
//                return;
//            }
//        }
//        //没找到这个建筑，说明还没建筑
//        ShowBuildProperty(Idx);
//    }

//    public void AttachBuilding(Building build)
//    {
//        BuildingInfo info = GameData.FindBuildingInfo(build.BuildingPropertyIdx);
//        buildName.text = info.Name;
//        //如果是满级了那么显示这个，且不显示所需
//        if (build.Level == info.improve[info.improve.Count - 1].Level && info.improve.Count == 1)
//        {
//            created.gameObject.SetActive(true);
//            Item1.text = "";
//            Item2.text = "";
//            Item3.text = "";
//            Level.gameObject.SetActive(false);
//            OnCreate.onClick.RemoveAllListeners();
//        }
//        else
//        {
//            created.gameObject.SetActive(false);
//            Level.gameObject.SetActive(true);
//            Level.GetComponentInChildren<Text>().text = build.Level.ToString();
//            ShowNeedItem(build.BuildingPropertyIdx, build.Level);
//            OnCreate.onClick.RemoveAllListeners();
//            OnCreate.onClick.AddListener(()=> { OnImproveBuild(build); });
//        }

//        newBuild.gameObject.SetActive(false);
//        Tip.onClick.RemoveAllListeners();
//        Tip.onClick.AddListener(()=> { ShowBuildDes(build.BuildingPropertyIdx); });
        
//    }

//    void ShowBuildDes(int idx)
//    {
//        BuildingInfo info = GameData.FindBuildingInfo(idx);
//        BuildListCtrl.Ins.ShowDes(info.Des);
//        //info.Des;
//    }

//    void ShowBuildProperty(int idx)
//    {
//        //待建造的建筑.
//        Level.gameObject.SetActive(false);
//        BuildingInfo info = GameData.FindBuildingInfo(idx);
//        buildName.text = info.Name;
//        created.gameObject.SetActive(false);
//        newBuild.gameObject.SetActive(true);
//        Tip.onClick.RemoveAllListeners();
//        Tip.onClick.AddListener(() => { ShowBuildDes(idx); });
//        OnCreate.onClick.RemoveAllListeners();
//        OnCreate.onClick.AddListener(()=> { OnCreateBuild(idx); });
//        ShowNeedItem(idx, 0);
//    }

//    void ShowNeedItem(int idx, int level)
//    {
//        Building build = null;
//        Item1.text = "";
//        Item2.text = "";
//        Item3.text = "";
//        foreach (var each in WorkFactory.Ins.Factory)
//        {
//            if (each.Value.BuildingPropertyIdx == idx)
//            {
//                build = each.Value;
//                break;
//            }
//        }

//        BuildingInfo info = GameData.FindBuildingInfo(idx);
//        if (info.improve.Count >= level + 1)
//        {
//            if (info.improve[level].CostMoney != 0)
//            {
//                Item1.text = LangItem.GetLangString(StringIden.Tael) + ":" + info.improve[level].CostMoney;
//                if (PlayerEx.TotalMoney >= info.improve[level].CostMoney)
//                    Item1.color = Color.white;
//                else
//                    Item1.color = Color.gray;
//            }
//            if (info.improve[level].ItemsToIprove.Count != 0)
//            {
//                foreach (var each in info.improve[level].ItemsToIprove)
//                {
//                    ItemBase it = GameData.FindItemByIdx(each.Key);
//                    if (Item1.text == "")
//                    {
//                        Item1.text = StringTbl.unitPrefix + it.Name + StringTbl.unitSuffix + " X " + each.Value;
//                        if (GameData.MainInventory.GetItemCount(each.Key) >= each.Value)
//                            Item1.color = Color.white;
//                        else
//                            Item1.color = Color.gray;
//                        continue;
//                    }
//                    if (Item2.text == "")
//                    {
//                        Item2.text = StringTbl.unitPrefix + it.Name + StringTbl.unitSuffix + " X " + each.Value;
//                        if (GameData.MainInventory.GetItemCount(each.Key) >= each.Value)
//                            Item2.color = Color.white;
//                        else
//                            Item2.color = Color.gray;
//                        continue;
//                    }
//                    if (Item3.text == "")
//                    {
//                        Item3.text = StringTbl.unitPrefix + it.Name + StringTbl.unitSuffix + " X " + each.Value;
//                        if (GameData.MainInventory.GetItemCount(each.Key) >= each.Value)
//                            Item3.color = Color.white;
//                        else
//                            Item3.color = Color.gray;
//                        break;
//                    }
//                }

//            }
//        }
//    }

//    void OnCreateBuild(int idx)
//    {
//        CloseTip();
//        //如果物品不足提示
//        BuildingInfo info = GameData.FindBuildingInfo(idx);
//        //如果物品不足则提示
//        U3D.PopupTip(LangItem.GetLangString(StringIden.Build) + info.Name);

//        //扣除掉物品数量和金钱数量.
//        WorkFactory.Ins.InitBuild(idx);
//        SetBuilding(idx);
//    }

//    void OnImproveBuild(Building build)
//    {
//        CloseTip();
//        BuildingInfo info = GameData.FindBuildingInfo(build.BuildingPropertyIdx);
//        //如果物品不足则提示
//        U3D.PopupTip(LangItem.GetLangString(StringIden.Upgrade) + info.Name);
//    }

//    public void CloseTip()
//    {
//        BuildListCtrl.Ins.CloseTip();
//    }
//}
