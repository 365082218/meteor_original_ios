using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class JiguanyuanCtrl : Window<JiguanyuanCtrl>
{

    //   List<Button> citylst;
    //   List<Button> gatelst;
    //   public GameObject cityRoot;
    //   public GameObject gateRoot;
    //   public static JiguanyuanCtrl Ins;
    //   int curClickCity = 0;
    //   void Awake()
    //   {
    //       Ins = this;
    //       gate = new Dictionary<int, List<Cell>>();
    //       citylst = new List<Button>();
    //       gatelst = new List<Button>();
    //   }

    //   void OnDestroy()
    //   {
    //       Ins = null;
    //   }

    //   //Dictionary<int, List<Cell>> gate;
    //// Use this for initialization
    //void Start () {
    //       //读取所有已经探索过的城镇.
    //       //gate.Clear();
    //       //for (int i = 0; i < GameData.save.Gatelst.Count; i++)
    //       //{
    //       //    if (!gate.ContainsKey(GameData.save.Gatelst[i].cityIdx))
    //       //    {
    //       //        gate.Add(GameData.save.Gatelst[i].cityIdx, new List<Cell>() { GameData.save.Gatelst[i].cellIdx});
    //       //        InsertCity(GameData.save.Gatelst[i].cityIdx);
    //       //    }
    //       //    else if (!gate[GameData.save.Gatelst[i].cityIdx].Contains(GameData.save.Gatelst[i].cellIdx))
    //       //        gate[GameData.save.Gatelst[i].cityIdx].Add(GameData.save.Gatelst[i].cellIdx);

    //       //}
    //}

    //// Update is called once per frame
    //void Update () {

    //}

    //   public void OnClose()
    //   {
    //       WsWindow.Close(WsWindow.Jiguanyuan);
    //   }

    //   void InsertCity(int idx)
    //   {
    //       GameObject MenuItem = GameObject.Instantiate(Resources.Load("CityMenuItem", typeof(GameObject))) as GameObject;
    //       MenuItem.transform.SetParent(cityRoot.transform);
    //       MenuItem.transform.localPosition = Vector3.zero;
    //       MenuItem.transform.localScale = Vector3.one;
    //       MenuItem.transform.localRotation = Quaternion.identity;
    //       Button ctrl = MenuItem.GetComponent<Button>();
    //       ctrl.onClick.AddListener(delegate ()
    //       {
    //           OnClickCity(idx);
    //       });

    //       //ctrl.GetComponentInChildren<Text>().text = GameData.FindMapByCityIdx(idx).Name;
    //       citylst.Add(ctrl);
    //   }

    //   public void OnClickCity(int idx)
    //   {
    //       curClickCity = idx;
    //       ClearCell();
    //       //for (int i = 0; i < gate[idx].Count; i++)
    //       //    InsertCell(gate[idx][i]);
    //   }

    //   public void InsertCell(Cell ce)
    //   {
    //       GameObject MenuItem = GameObject.Instantiate(Resources.Load("GateMenuItem", typeof(GameObject))) as GameObject;
    //       MenuItem.transform.SetParent(gateRoot.transform);
    //       MenuItem.transform.localPosition = Vector3.zero;
    //       MenuItem.transform.localScale = Vector3.one;
    //       MenuItem.transform.localRotation = Quaternion.identity;
    //       Button ctrl = MenuItem.GetComponent<Button>();
    //       ctrl.onClick.AddListener(delegate ()
    //       {
    //           OnClickGate(ce);
    //       });
    //       //Map map = GameData.FindMapByCityIdx(curClickCity);
    //       //ctrl.GetComponentInChildren<Text>().text = GameData.FindMapByCityIdx(curClickCity).Info[ce].Title;
    //       gatelst.Add(ctrl);
    //   }

    //   public void ClearCell()
    //   {
    //       for (int i = 0; i < gatelst.Count; i++)
    //           DestroyImmediate(gatelst[i].gameObject);
    //       gatelst.Clear();
    //   }

    //   public void OnClickGate(Cell ce)
    //   {
    //       if (curClickCity != 0)
    //       {
    //           WsWindow.Close(WsWindow.Jiguanyuan);
    //           if (!AccidentHappens())
    //           {
    //               U3D.EnterMap(curClickCity, ce.cellX, ce.cellY);
    //               if (InventoryCtrl.ins != null)
    //                   WsWindow.Close(WsWindow.Inventory);
    //           }
    //           else
    //           {
    //               //去一个秘密地图.
    //               //int nRand = Random.Range(0, GameData.save.SecretMap.Count);
    //               //U3D.AddGate(GameData.save.SecretMap[nRand]);
    //               //EntryPoint ept = GameData.save.SecretMap[nRand];
    //               //GameData.save.SecretMap.RemoveAt(nRand);
    //               //U3D.EnterMap(ept.cityIdx, ept.cellIdx.cellX, ept.cellIdx.cellY);
    //               //if (InventoryCtrl.ins != null)
    //               //    WsWindow.Close(WsWindow.Inventory);
    //               //U3D.TextAppend(LangItem.GetLangString(StringIden.Accident));
    //           }
    //       }
    //   }

    //   bool AccidentHappens()
    //   {
    //       bool ret = false;
    //       //if (GameData.save.SecretMap.Count != 0)
    //       //{
    //       //    //检查是否还存在未进入过的秘密入口，
    //       //    int rand = Random.Range(0, 10000);
    //       //    if (rand < GameData.save.useMoveFunCnt)
    //       //    {
    //       //        ret = true;
    //       //        GameData.save.useMoveFunCnt = 0;
    //       //    }
    //       //}
    //       return ret;
    //   }


    public override string PrefabName
    {
        get
        {
            return "Jiguanyuan";
        }
    }
}
