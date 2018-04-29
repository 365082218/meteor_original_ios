using UnityEngine;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI;



public enum SkillType
{
    Reborn,//复活
    RemoveAnomaly,//解除异常(点穴，沉默，减速) 战斗中使用
    ReleaseToxicity,//解除毒性
    SkillDamage,//指定伤害值
    PhysicalDamage,//以部队数量×物理伤害×倍数
    Status,//状态,防御攻击身法 数回合内提升
    Recovery,//恢复
    Attribute,//属性类.多属于建筑开启技能，例如 骑术，目标一般是骑兵类，我方所有骑兵(1002,重骑兵，1003 轻骑兵)，攻击 或者 防御 属性得到永久提升.
}

public enum SkillAttribute
{
    HP,//气血
    DEF,//防御
    ATT,//攻击
    SPD,//身法
}

public class PlayerCtrl : MonoBehaviour {

    public Camera RenderMaskCamera;
    public Camera Followed;
    public Transform MaskBrush;
    public Transform MaskWall;
    //public Transform Light;
    public Sprite[] sprites;
    int xIdx = -1;//[0,50]
    int yIdx = -1;//[0,50]
    int xStart = -1;
    int yStart = -1;
    int xBase = 60;
    int yBase = -60;
    int width = 120;
    int height = 120;

    //MapIns current;
    public static PlayerCtrl Ins;
    void Awake()
    {
        Ins = this;
    }

    void OnDestroy()
    {
        Ins = null;
    }
	// Use this for initialization
	void Start () {
        ////测试用
        //xIdx = 25;
        //yIdx = 25;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Walk(int direction)
    {
        //if (direction < 0 || direction > 3)
        //    return;
        ////1判断能否移动
        ////2移动
        ////3显示UI上的按钮。
        ////4消耗物品
        ////5调用格子上的事件
        //if (xIdx <= 0 && direction == (int)DirectionEx.Left)
        //    return;
        //if (xIdx >= 50 && direction == (int)DirectionEx.Right)
        //    return;
        //if (yIdx >= 50 && direction == (int)DirectionEx.Bottom)
        //    return;
        //if (yIdx <= 0 && direction == (int)DirectionEx.Top)
        //    return;
        //int iIdx = xIdx;
        //int jIdx = yIdx;
        //switch (direction)
        //{
        //    case (int)DirectionEx.Left:iIdx = xIdx - 1;break;
        //    case (int)DirectionEx.Right:iIdx = xIdx + 1;break;
        //    case (int)DirectionEx.Top:jIdx = yIdx - 1;break;
        //    case (int)DirectionEx.Bottom:jIdx = yIdx + 1;break;
        //}
        //MoveTo(iIdx, jIdx);
    }

    void SetUnitTo(GameObject obj, int x, int y)
    {
        int xpos = xBase + x * 120;
        int ypos = yBase - y * 120;
        obj.transform.localPosition = new Vector3(xpos, ypos, obj.transform.localPosition.z);    
    }

    //不要摄像机缓动，作为地图初始化时设置位置.
    void SetTo(int x, int y)
    {
        int xpos = xBase + x * 120;
        int ypos = yBase - y * 120;
        transform.localPosition = new Vector3(xpos, ypos, transform.localPosition.z);
        MaskBrush.localPosition = new Vector3(xpos, ypos, MaskBrush.localPosition.z);
        MaskWall.localPosition = new Vector3(xpos, ypos, MaskWall.localPosition.z);
        Followed.transform.localPosition = new Vector3(xpos, ypos, Followed.transform.position.z);
    }

    void MoveTo(int x, int y)
    {
        //如果此地有触发器，那么不允许快速移动到下一个地点.
        //for (int i = 0; i < current.cell.Count; i++)
        //{
        //    if (current.cell[i].x == x && current.cell[i].y == y)
        //    {
        //        /*WalkBtnCtrl.Ins.OnEnableWalk(false);*/
        //        break;
        //    }
        //}
        //int xpos = xBase + x * 120;
        //int ypos = yBase - y * 120;
        //transform.localPosition = new Vector3(xpos, ypos, transform.localPosition.z);
        //MaskBrush.localPosition = new Vector3(xpos, ypos, MaskBrush.localPosition.z);
        ////Light.localPosition = new Vector3(xpos, ypos, Light.localPosition.z);
        //xIdx = x;
        //yIdx = y;
        //Tweener tw = Followed.transform.DOMove(new Vector3(xpos, ypos, Followed.transform.position.z), 0.4f);
        //tw.SetEase(Ease.Linear);
        //tw.OnComplete(delegate () { OnMoveto(xIdx, yIdx); });
    }

    //void Goto(CellTrigger trigger)
    //{
    //    //如果有多条路，就打开界面，创建该出口通往的所有道路.给选择
    //    //否则直接去唯一的通路.
    //    int cnt = 0;
    //    if (trigger.cellBuildInfo.CityMapEntryList != null)
    //        cnt += trigger.cellBuildInfo.CityMapEntryList.Count;
    //    if (trigger.cellBuildInfo.ExploreMapIdx != null)
    //        cnt += trigger.cellBuildInfo.ExploreMapIdx.Count;
    //    if (cnt == 1)
    //    {
    //        if (trigger.cellBuildInfo.CityMapEntryList != null)
    //        {
    //            U3D.EnterMap(trigger.cellBuildInfo.CityMapEntryList[0].cityIdx,
    //            trigger.cellBuildInfo.CityMapEntryList[0].cellIdx.cellX,
    //            trigger.cellBuildInfo.CityMapEntryList[0].cellIdx.cellY);
    //            //把当前探索摄像机等给关闭掉
    //            OnSave();
    //            TopInfoCtrl.Ins.ResetTitle();
    //            WsWindow.Close(WsWindow.MapPanel);
    //        }
    //        else
    //        {
    //            //打开另一个迷雾地图
    //            U3D.gotoMap(trigger.cellBuildInfo.ExploreMapIdx[0].idx, trigger.cellBuildInfo.ExploreMapIdx[0].doorIdx);
    //        }
    //    }
    //    else
    //    {
    //        //打开一个界面，给选择去哪.
    //    }
    //}

    

    //void OnMoveto(int x, int y)
    //{
    //    //先执行完此触发器上的事件
    //    for (int i = 0; i < current.cell.Count; i++)
    //    {
    //        if (current.cell[i].x == x && current.cell[i].y == y)
    //        {
    //            if (x == xStart && y == yStart)
    //            {
    //                Goto(current.cell[i]);
    //                return;
    //            }
    //            //current.cell[i].cellBuildInfo.name;//标题.
    //            //current.cell[i].cellBuildInfo.description;//描述.
    //            //战斗.
    //            //current.cell[i].bat;
    //            int k = i;
    //            if (current.cell[k].bat != null && current.cell[k].bat.nodes.Count != 0)
    //            {
    //                QuestionCtrl ctrl = WsWindow.Open<QuestionCtrl>(WsWindow.Question);
    //                ctrl.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
    //                ctrl.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
    //                if (ctrl != null)
    //                {
    //                    ctrl.Name.text = current.cell[k].cellBuildInfo.name;
    //                    ctrl.Description.text = current.cell[k].cellBuildInfo.description;
    //                    ctrl.BattleText.text = "";
    //                    ctrl.OnYes.onClick.RemoveAllListeners();
    //                    ctrl.OnYes.onClick.AddListener(() =>
    //                    {
    //                        ctrl.Name.text = current.cell[k].cellBuildInfo.name + "[第" + current.cell[k].bat.nodes[0].Level + "层]";
    //                        ctrl.BattleText.text = current.cell[k].bat.nodes[0].Talk;
    //                        ctrl.OnYes.GetComponentInChildren<Text>().text = "战斗";
    //                        //ctrl.OnCancel.GetComponentInChildren<Text>().text = "离开";
    //                        ctrl.OnYes.onClick.RemoveAllListeners();
    //                        ctrl.OnYes.onClick.AddListener(() => {
    //                            //打开战斗界面，放到最顶层，遮蔽住下层的问答界面.
    //                            //WsWindow.Open<BattleCtrl>(WsWindow.Battle);
    //                            U3D.PopupTip("打开战斗界面");
    //                        });
    //                    })
    //                    ;
    //                    ctrl.OnCancel.onClick.RemoveAllListeners();
    //                    ctrl.OnCancel.onClick.AddListener(() => { WsWindow.Close(WsWindow.Question); /*WalkBtnCtrl.Ins.OnEnableWalk(true);*/ });
    //                }
    //            }
    //            else//没有战斗，或者战斗已经打通，那么显示该地点的物品，左边自己的背包，右边地点的物品列表.
    //            {
    //                //传送门.
    //                //if (current.cell[k].cellBuildInfo.type == 0)
    //                //    Goto(current.cell[k]);
    //                //else
    //                //    ;
    //                    //显示战利品或者空的
    //            }
    //            break;
    //        }
    //    }

    //    PlayerEx.Instance.moveCnt++;
    //    bool cost = PlayerEx.Instance.moveCnt > PlayerEx.Instance.Food;
    //    if (cost)
    //    {
    //        bool eat = U3D.EatFood();
    //        if (eat)
    //            TopInfoCtrl.Ins.UpdateFood();
    //        else
    //        {
    //            //显示队伍解散事件，队伍中所有的士兵都离开了.
    //        }
    //    }
    //    //战斗确认界面
    //    //退出战斗确认界面，就开启移动按钮
    //    //WalkBtnCtrl.Ins.OnEnableWalk(true);
    //}

    //public void OnSaveMapFogTexture()
    //{
    //    int width = RenderMaskCamera.targetTexture.width;
    //    int height = RenderMaskCamera.targetTexture.height;
    //    Texture2D texture = new Texture2D(width, height);
    //    RenderTexture.active = null;
    //    RenderTexture.active = RenderMaskCamera.targetTexture;
    //    texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
    //    texture.Apply();
    //    System.IO.File.WriteAllBytes(Application.persistentDataPath + "/" + GameData.gameStatus.saveSlot + "/" + current.idx + "_mask.png", texture.EncodeToPNG());
    //    RenderTexture.active = null;
    //}

    public void LoadMapInstance(int idx)
    {
        //先查询此地图有无存档，有则从存档加载，无则第一次生成.
       // bool firstload = false;
       // current = GameData.FindMapSaveByIdx(idx);
       // if (current == null)
       // {
       //     firstload = true;
       //     current = GenerateMapIns(idx);
       //     GameData.save.MapStates[idx] = current;
       // }

       // if (!firstload && System.IO.File.Exists(Application.persistentDataPath + "/" + GameData.gameStatus.saveSlot + "/" + current.idx + "_mask.png")) 
       // {
       //     byte[] png = System.IO.File.ReadAllBytes(Application.persistentDataPath + "/" + GameData.gameStatus.saveSlot + "/" + current.idx + "_mask.png");
       //     Texture2D tex = new Texture2D(512, 512);
       //     tex.LoadImage(png);
       //     Graphics.Blit(tex, RenderMaskCamera.targetTexture);
       // }

       //for (int i = 0; i < current.cell.Count; i++)
       // {
       //     if (current.cell[i] != null)
       //     {
       //         GameObject objCell = Instantiate(Resources.Load("CellUnit")) as GameObject;
       //         objCell.transform.SetParent(transform.parent);
       //         SetUnitTo(objCell, current.cell[i].x, current.cell[i].y);
       //         if (sprites.Length > current.cell[i].cellBuildInfo.type)
       //             objCell.GetComponent<SpriteRenderer>().sprite = sprites[current.cell[i].cellBuildInfo.type];
       //     }
       // }
       // MapConfig cfg = GameData.FindMapCfgByIdx(current.idx);
       // TopInfoCtrl.Ins.SetTitle(cfg.name);
    }

    public void OnSave()
    {
        //OnSaveMapFogTexture();
        //GameData.save.MapStates[current.idx] = current;
    }

    //MapIns GenerateMapIns(int idx)
    //{
        //从配置读取某个地图的设定，比如怪物占多少格子，宝物占多少格子
        //MapConfig cfg = GameData.FindMapCfgByIdx(idx);
        //MapIns map = new MapIns();
        //map.cell = new List<CellTrigger>();
        //map.idx = idx;
        //int buildCnt = cfg.build.Count;
        //List<CellBattle> batlst = new List<CellBattle>();
        //for (int i = 0; i < cfg.bat.Count; i++)
        //{
        //    CellBattle ce = GameData.FindCellBattleInfo(cfg.bat[i]);
        //    batlst.Add(ce.Clone());
        //}

        //List<int> x = new List<int>();
        //List<int> y = new List<int>();
        //for (int i = 0; i < 51; i++)
        //{
        //    x.Add(i);
        //    y.Add(i);
        //}
        //int k = 0;
        //while (buildCnt != 0 && x.Count != 0 && y.Count != 0)
        //{
        //    int xRand = x[Random.Range(0, x.Count)];
        //    int yRand = y[Random.Range(0, y.Count)];
        //    CellTrigger trigger = new CellTrigger();
        //    trigger.x = xRand;
        //    trigger.y = yRand;
        //    trigger.cellBuildInfo = cfg.build[k++].Clone();
        //    if (trigger.cellBuildInfo.type == 0)
        //        trigger.bat = null;
        //    else
        //    {
        //        if (batlst.Count != 0)
        //        {
        //            int nrand = Random.Range(0, batlst.Count);
        //            trigger.bat = batlst[nrand];
        //            batlst.RemoveAt(nrand);
        //        }
        //    }
        //    map.cell.Add(trigger);
        //    buildCnt--;
        //    x.Remove(xRand);
        //    y.Remove(yRand);
        //}
        //return map;
    //    return null;
    //}

    //public void SetPlayerPos(int door)
    //{
    //    for (int i = 0; i < current.cell.Count; i++)
    //    {
    //        if (current.cell[i].cellBuildInfo != null)
    //        {
    //            if (current.cell[i].cellBuildInfo.index == door)
    //            {
    //                xIdx = current.cell[i].x;
    //                yIdx = current.cell[i].y;
    //                xStart = xIdx;
    //                yStart = yIdx;
    //                SetTo(xIdx, yIdx);
    //            }
    //        }
    //    }
    //}
}
