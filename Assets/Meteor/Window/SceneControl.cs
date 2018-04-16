//using UnityEngine;
//using System.Collections;
//using UnityEngine.UI;
//using System.Collections.Generic;
//using DG.Tweening;
//using SLua;
//public class SceneControl : MonoBehaviour {
//	public GameObject Map;
//	public GameObject debugPanel;
//    public GameObject debugMenu;
//	public GameObject fixedMap;
//	public Text text;
//	public Text title;
//	public GameObject MenuRoot;
//	public GameObject TextRoot;
//	public GameObject MonsterCardRoot;
//	public GameObject TextscrollView;
//	public SceneUnit curMapData;
//    EntryPoint entrypoint;
//	public Button[] MapBtn;
//	public GameObject[] lineArray;
//	public Dictionary<GameObject, int> BtnMap = new Dictionary<GameObject, int> ();
//	int curMapBtnIndex = 4;
//    int randMonsterGroupIndex = -1;
//    string strCity;
//    // Use this for initialization
//    public static SceneControl ins = null;
//    void Awake()
//    {
//        ins = this;
//    }
//	void Start () {
//        BtnMap.Clear();
//	    for (int i = 0; i < MapBtn.Length; i++)
//            BtnMap.Add(MapBtn[i].gameObject, i);
//        debugPanel.GetComponent<Button>().onClick.AddListener(() => { debugMenu.SetActive(!debugMenu.activeSelf); });
//    }
	
//    void OnDestroy()
//    {
//        ins = null;
//    }
//	// Update is called once per frame
//	void Update () {
//	}

//	public void OnMapBtnClick(GameObject obj)
//	{
//        if (BtnMap.ContainsKey(obj))
//        {
//            if (BtnMap[obj] < 4)
//            {
//                if (curMapData.Route.ContainsKey(BtnMap[obj]) && curMapData.Route[BtnMap[obj]] == 1)
//                {
//                    EntryPoint ept = new EntryPoint();
//                    ept.cityIdx = entrypoint.cityIdx;
//                    ept.cellIdx = new Cell();
//                    switch (BtnMap[obj])
//                    {
//                        case 0:
//                            ept.cellIdx.cellY = entrypoint.cellIdx.cellY - 1;
//                            ept.cellIdx.cellX = entrypoint.cellIdx.cellX - 1;
//                        break;
//                        case 1:
//                            ept.cellIdx.cellY = entrypoint.cellIdx.cellY;
//                            ept.cellIdx.cellX = entrypoint.cellIdx.cellX - 1;
//                        break;
//                        case 2:
//                            ept.cellIdx.cellY = entrypoint.cellIdx.cellY + 1;
//                            ept.cellIdx.cellX = entrypoint.cellIdx.cellX - 1;
//                        break;
//                        case 3:
//                            ept.cellIdx.cellY = entrypoint.cellIdx.cellY - 1;
//                            ept.cellIdx.cellX = entrypoint.cellIdx.cellX;
//                        break;
//                    }
                    
//                    if (ept.cellIdx.cellX >= 0 && ept.cellIdx.cellY >= 0)
//                        SceneMng.EntryMap(ept);
//                }
//            }
//            else if (BtnMap[obj] > 4)
//            {
//                if (curMapData.Route.ContainsKey(BtnMap[obj] - 1) && curMapData.Route[BtnMap[obj] - 1] == 1)
//                {
//                    EntryPoint ept = new EntryPoint();
//                    ept.cityIdx = entrypoint.cityIdx;
//                    ept.cellIdx = new Cell();
//                    switch (BtnMap[obj])
//                    {
//                        case 5:
//                            ept.cellIdx.cellY = entrypoint.cellIdx.cellY + 1;
//                            ept.cellIdx.cellX = entrypoint.cellIdx.cellX;
//                            break;
//                        case 6:
//                            ept.cellIdx.cellY = entrypoint.cellIdx.cellY - 1;
//                            ept.cellIdx.cellX = entrypoint.cellIdx.cellX + 1;
//                            break;
//                        case 7:
//                            ept.cellIdx.cellY = entrypoint.cellIdx.cellY;
//                            ept.cellIdx.cellX = entrypoint.cellIdx.cellX + 1;
//                            break;
//                        case 8:
//                            ept.cellIdx.cellY = entrypoint.cellIdx.cellY + 1;
//                            ept.cellIdx.cellX = entrypoint.cellIdx.cellX + 1;
//                            break;
//                    }

//                    if (ept.cellIdx.cellX >= 0 && ept.cellIdx.cellY >= 0)
//                        SceneMng.EntryMap(ept);
//                }
//            }
//        }
//    }

//	public void OnTitleMaskDone()
//	{
//		text.CrossFadeAlpha (0, 0.0f, false);
//		text.gameObject.SetActive (true);
//		text.CrossFadeAlpha (1, 1.0f, false);
//        //if (ExistBattle())
//        //    Invoke("ShowMapPanel", 2.0f);
//        //else
//            Invoke("ShowMapPanel", 0.0f);
//    }

//	public void MaskBegin()
//	{
//	}


//	void ShowMapPanel()
//	{
//		if (Map != null)
//			Map.SetActive (true);
//		if (ExistBattle ()) {
//			ShowMonsterName ();
//		}
//		else
//			ScriptMng.ins.CallScript (curMapData.Script);
//        LoadRandMonster();

//        if (ExistBattle())
//        {
//            BattleUnit bat = GameData.FindBattleByIdx(curMapData.BattleId);
//            //怪物主动攻击主角.
//            if (bat != null && bat.Monster != null && bat.Monster.Count != 0)
//                ScriptMng.ins.CallScript(bat.strScript);
//        }
//	}

//    void LoadRandMonster()
//    {
//        //if (curMapData.MonsterPool != null)
//        //    SceneMng.GenRandMonster(curMapData.Idx, curMapData.MonsterPool);
//        //遍历在这个场景的所有怪物组.
//        //if (SceneMng.randomBattle.ContainsKey(curMapData.Idx))
//        //{
//        //    for (int i = 0; i < SceneMng.randomBattle[curMapData.Idx].Count; i++)
//        //    {
//        //        RandMonsterCtrl ctrl = WsWindow.AddMenu<RandMonsterCtrl>("MapRandomMonsterItem", MenuRoot.transform);
//        //        ctrl.Attach(SceneMng.randomBattle[curMapData.Idx][i]);
//        //    }
//        //}
//    }

//    void LoadItems()
//    {
//        if (curMapData.MapObjects != null)
//        {
//            //string[] strObjects = curMapData.MapObjects.Split(new char[] { '#' });
//            //查看该地图的物品曾经被加载过没有，如果没有，就加载，否则直接就在内存里，不用再加载了.直接显示到场景里就好了.
//            if (!SceneMng.hasLoaded(GameData.MapId(entrypoint)))
//            {
//                for (int i = 0; i < curMapData.MapObjects.Count; i++)
//                {
//                    SceneMng.MakeInstanceInMap(entrypoint, curMapData.MapObjects[i]);
//                }
//                SceneMng.LoadDone(GameData.MapId(entrypoint));
//            }
//        }

//        //待加载物件，是此场景未初始化前，其他场景把一些物品放到此场景来了。
//        if (GameData.save.mapObjectsWaitLoad.ContainsKey(entrypoint) && GameData.save.mapObjectsWaitLoad[entrypoint] != null)
//        {
//            if (!GameData.save.mapObjects.ContainsKey(entrypoint))
//                GameData.save.mapObjects.Add(entrypoint, new List<MapObject>());
//            for (int i = 0; i < GameData.save.mapObjectsWaitLoad[entrypoint].Count; i++)
//                GameData.save.mapObjects[entrypoint].Add(GameData.save.mapObjectsWaitLoad[entrypoint][i]);
//            GameData.save.mapObjectsWaitLoad[entrypoint].Clear();
//        }
//        //加载之前存储在内存中实例化过的物品.
//        if (GameData.save.mapObjects.ContainsKey(entrypoint) && GameData.save.mapObjects[entrypoint] != null)
//        {
//            for (int i = 0; i < GameData.save.mapObjects[entrypoint].Count; i++)
//            {
//                //宝箱不关心怪物属性数据.只关心自身的物品.
//                if (!GameData.save.mapObjects[entrypoint][i].Initialized)
//                    GameData.save.mapObjects[entrypoint][i].Init();
//                if (GameData.save.mapObjects[entrypoint][i].visible)
//                    LoadItemInstance(GameData.save.mapObjects[entrypoint][i]);
//            }

//            //查看是否有NPC带有传送功能，若有，那么把此地图放到可传送列表里.
//            //如果此地图不在已知传送列表里。
//            if (!GameData.save.Gatelst.Contains(entrypoint))
//            {
//                for (int i = 0; i < GameData.save.mapObjects[entrypoint].Count; i++)
//                {
//                    if (GameData.save.Gatelst.Contains(entrypoint))
//                        break;
//                    if (GameData.save.mapObjects[entrypoint][i].Unit.FunctionList != null)
//                    {
//                        for (int j = 0; j < GameData.save.mapObjects[entrypoint][i].Unit.FunctionList.Count; j++)
//                        {
//                            NpcFunction ta = GameData.FindFunByIdx(GameData.save.mapObjects[entrypoint][i].Unit.FunctionList[j]);
//                            if (ta != null && ta.FunType == (int)FunctionType.Move)
//                            {
//                                GameData.save.Gatelst.Add(entrypoint);
//                                break;
//                            }
//                        }
//                    }
//                }
//            }
//        }
//    }

//    //核心
//    public void LoadItemInstance(MapObject item)
//    {
//        //这里要决定1，用什么预设，比如物品，就挂MenuItem
//        //要是场景固定 怪物/NPC 类型，就挂MapItem。
//        GameObject insert = null;
//        if (item.Unit.type != (int)MapObjectType.Npc)
//        {
//            insert = WsWindow.AddMenu("MenuItem", MenuRoot.transform);
//        }
//        else
//        {
//            //拥有血量 体力 魔法属性的.虽然这里不能战斗
//            insert = WsWindow.AddMenu("MapFixMonsterItem", MenuRoot.transform);
//        }
//        insert.GetComponent<MapUnitCtrl>().Attach(item);
//        //if (item.Unit.type == (int)MapObjectType.Building)
//        //    insert.GetComponent<MenuProgressCtrl>().BindBuilding(WorkFactory.Ins.Factory[item.nBuildingId]);//建筑有工作进度和升级进度
//        if (item.Unit.type == (int)MapObjectType.Chest || item.Unit.type == (int)MapObjectType.Furniture)
//        {
//            MenuProgressCtrl ctrl = insert.GetComponent<MenuProgressCtrl>();
//            DestroyImmediate(ctrl);
//        }
//    }

//    public void RemoveNpc(int idx)
//    {
//        MapUnitCtrl[] npcs = MenuRoot.GetComponentsInChildren<MapUnitCtrl>();
//        for (int i = 0; i < npcs.Length; i++)
//        {
//            if (npcs[i].Item != null && npcs[i].Item.Unit.Idx == idx)
//            {
//                DestroyImmediate(npcs[i].gameObject);
//                return;
//            }
//        }
//    }

//	List<GameObject> cardList = new List<GameObject>();
//	void ShowMonsterName()
//	{
//        for (int i = 0; i < cardList.Count; i++)
//        {
//            GameObject.DestroyImmediate(cardList[i]);
//        }
//        cardList.Clear();
//        //这里不显示随机怪物的。随机怪物显示在左下NPC和场景物品还有目录里，混合起来.
//        string scripts = "";
//        if (MonsterCardRoot != null)
//        {
//            List<MonsterCount> monster = PlayerEx.Instance.GetMonsterList(GameData.MapId(entrypoint), curMapData.BattleId, ref scripts);
//            if (monster == null)
//                return;
//            for (int i = 0; i < monster.Count; i++)
//            {
//                GameObject cardItem = GameObject.Instantiate(Resources.Load("MonsterCardItem")) as GameObject;
//                if (cardItem && MonsterCardRoot)
//                {
//                    cardItem.transform.SetParent(MonsterCardRoot.transform);
//                    cardItem.transform.localPosition = Vector3.zero;
//                    cardItem.transform.localScale = Vector3.one;
//                    cardItem.transform.rotation = Quaternion.identity;
//                    CardItem cardInfo = cardItem.GetComponent<CardItem>();
//                    //cardInfo.SetCardIdx(monster[i].monsterId, scripts);
//                    cardList.Add(cardItem);
//                }
//            }
//        }
//    }

//    public void RemoveMonsterCard()
//    {
//        for (int i = 0; i < cardList.Count; i++)
//            cardList[i].GetComponent<CardItem>().OnDead();
//    }

//    bool ExistBattle()
//	{
//		bool ret = false;
//		if (curMapData.BattleId != 0 && !PlayerEx.Instance.SceneBattleDone (GameData.MapId(entrypoint), curMapData.BattleId)) {
//			ret = true;
//		} else if (PlayerEx.Instance.ExistTaskBattle (GameData.MapId(entrypoint))) {
//			ret = true;
//		} else
//			ret = false;
//		return ret;
//	}

//	void RefreshText()
//	{
//        for (int i = 0; i < lineArray.Length; i++)
//        {
//            lineArray[i].SetActive(false);
//        }
//        SceneUnit dat = null;
//        if (curMapData != null)
//        {
//            //固定战斗-任务战斗-随机战斗-普通描述
//            //如果是固定战斗场景.且没发生过这个固定战斗.
//            if (curMapData.BattleId != 0 && !PlayerEx.Instance.SceneBattleDone(GameData.MapId(entrypoint), curMapData.BattleId))
//            {
//                AppendText(curMapData.BattleText);
//                //text.text = curMapData.BattleText;
//                BattleUnit bat = GameData.FindBattleByIdx(curMapData.BattleId);
//                if (bat != null && !string.IsNullOrEmpty(bat.Des))
//                    AppendText(" " + bat.Des);//text.text += "  " + bat.Des;
//            }
//            else if (PlayerEx.Instance.ExistTaskBattle(GameData.MapId(entrypoint)))
//            {
//                //查看人物是否包含任务，包含则触发任务战斗.
//                AppendText(curMapData.BattleText);//text.text = curMapData.BattleText;
//                int battle = PlayerEx.Instance.GetTaskBattleId(GameData.MapId(entrypoint));
//                BattleUnit bat = GameData.FindBattleByIdx(battle);
//                if (bat != null && !string.IsNullOrEmpty(bat.Des))
//                    AppendText(" " + bat.Des);//text.text += "  " + bat.Des;
//            } /*else if (SceneMng.ExistRandomBattle (curMapData.Idx)) {
//        		//如果当前场景存在随机战斗.
//        		AppendText(curMapData.BattleText);
//        		//text.text = curMapData.BattleText;
//        	}*/
//            else
//            {
//                //body字段舍弃，改为在脚本中处理普通流程输出.
//                //text.text = curMapData.Body;
//            }

//            title.text = strCity + "-" + curMapData.Title;
//            if (curMapData.Route.ContainsKey(3) && curMapData.Route[3] == 1)
//            {
//                MapBtn[3].gameObject.SetActive(true);
//                Cell ce = new Cell();
//                ce.cellX = entrypoint.cellIdx.cellX;
//                ce.cellY = entrypoint.cellIdx.cellY - 1;
//                dat = GameData.FindMapByCityIdx(entrypoint.cityIdx).Info[ce];
//                if (dat != null)
//                {
//                    MapBtn[3].GetComponentInChildren<Text>().text = dat.Title;
//                    lineArray[5].SetActive(true);
//                    if (dat.Route.ContainsKey(1) && dat.Route[1] == 1)
//                    {
//                        lineArray[2].SetActive(true);
//                        if (MapBtn[0].GetComponentInChildren<Text>().text == "")
//                        {
//                            Cell ce2 = new Cell();
//                            ce2.cellX = entrypoint.cellIdx.cellX - 1;
//                            ce2.cellY = entrypoint.cellIdx.cellY - 1;
//                            SceneUnit dat2 = GameData.FindMapByCityIdx(entrypoint.cityIdx).Info[ce2];
//                            if (dat2 != null)
//                                MapBtn[0].GetComponentInChildren<Text>().text = dat2.Title;
//                        }
//                    }
//                    if (dat.Route.ContainsKey(6) && dat.Route[6] == 1)
//                    {
//                        lineArray[7].SetActive(true);
//                        if (MapBtn[6].GetComponentInChildren<Text>().text == "")
//                        {
//                            Cell ce2 = new Cell();
//                            ce2.cellY = entrypoint.cellIdx.cellY - 1;
//                            ce2.cellX = entrypoint.cellIdx.cellX + 1;
//                            SceneUnit dat2 = GameData.FindMapByCityIdx(entrypoint.cityIdx).Info[ce2];
//                            if (dat2 != null)
//                                MapBtn[6].GetComponentInChildren<Text>().text = dat2.Title;
//                        }
//                    }
//                }
//            }
//            else
//                MapBtn[3].GetComponentInChildren<Text>().text = "";
            
//            if (curMapData.Route.ContainsKey(1) && curMapData.Route[1] == 1)
//            {
//                MapBtn[1].gameObject.SetActive(true);
//                Cell ce = new Cell();
//                ce.cellY = entrypoint.cellIdx.cellY;
//                ce.cellX = entrypoint.cellIdx.cellX - 1;
//                dat = GameData.FindMapByCityIdx(entrypoint.cityIdx).Info[ce];
//                if (dat != null)
//                {
//                    MapBtn[1].GetComponentInChildren<Text>().text = dat.Title;
//                    lineArray[3].SetActive(true);
//                    if (dat.Route.ContainsKey(3) && dat.Route[3] == 1)
//                    {
//                        lineArray[0].SetActive(true);
//                        if (MapBtn[0].GetComponentInChildren<Text>().text == "")
//                        {
//                            Cell ce2 = new Cell();
//                            ce2.cellY = entrypoint.cellIdx.cellY - 1;
//                            ce2.cellX = entrypoint.cellIdx.cellX - 1;
//                            SceneUnit dat2 = GameData.FindMapByCityIdx(entrypoint.cityIdx).Info[ce2];
//                            if (dat2 != null)
//                                MapBtn[0].GetComponentInChildren<Text>().text = dat2.Title;
//                        }
//                    }
//                    if (dat.Route.ContainsKey(4) && dat.Route[4] == 1)
//                    {
//                        lineArray[1].SetActive(true);
//                        if (MapBtn[2].GetComponentInChildren<Text>().text == "")
//                        {
//                            Cell ce2 = new Cell();
//                            ce2.cellY = entrypoint.cellIdx.cellY + 1;
//                            ce2.cellX = entrypoint.cellIdx.cellX - 1;
//                            SceneUnit dat2 = GameData.FindMapByCityIdx(entrypoint.cityIdx).Info[ce2];
//                            if (dat2 != null)
//                                MapBtn[2].GetComponentInChildren<Text>().text = dat2.Title;
//                        }
//                    }
//                }

//            }
//            else
//                MapBtn[1].GetComponentInChildren<Text>().text = "";
            
//            if (curMapData.Route.ContainsKey(4) && curMapData.Route[4] == 1)
//            {
//                MapBtn[5].gameObject.SetActive(true);
//                Cell ce = new Cell();
//                ce.cellY = entrypoint.cellIdx.cellY + 1;
//                ce.cellX = entrypoint.cellIdx.cellX;
//                dat = GameData.FindMapByCityIdx(entrypoint.cityIdx).Info[ce];
//                if (dat != null)
//                {
//                    MapBtn[5].GetComponentInChildren<Text>().text = dat.Title;
//                    lineArray[6].SetActive(true);
//                    if (dat.Route.ContainsKey(1) && dat.Route[1] == 1)
//                    {
//                        lineArray[4].SetActive(true);
//                        if (MapBtn[2].GetComponentInChildren<Text>().text == "")
//                        {
//                            Cell ce2 = new Cell();
//                            ce2.cellY = entrypoint.cellIdx.cellY + 1;
//                            ce2.cellX = entrypoint.cellIdx.cellX - 1;
//                            SceneUnit dat2 = GameData.FindMapByCityIdx(entrypoint.cityIdx).Info[ce2];
//                            if (dat2 != null)
//                                MapBtn[2].GetComponentInChildren<Text>().text = dat2.Title;
//                        }
//                    }
//                    if (dat.Route.ContainsKey(6) && dat.Route[6] == 1)
//                    {
//                        lineArray[9].SetActive(true);
//                        if (MapBtn[8].GetComponentInChildren<Text>().text == "")
//                        {
//                            Cell ce2 = new Cell();
//                            ce2.cellY = entrypoint.cellIdx.cellY + 1;
//                            ce2.cellX = entrypoint.cellIdx.cellX + 1;
//                            SceneUnit dat2 = GameData.FindMapByCityIdx(entrypoint.cityIdx).Info[ce2];
//                            if (dat2 != null)
//                                MapBtn[8].GetComponentInChildren<Text>().text = dat2.Title;
//                        }
//                    }
//                }
//            }
//            else
//                MapBtn[5].GetComponentInChildren<Text>().text = "";

//            if (curMapData.Route.ContainsKey(6) && curMapData.Route[6] == 1)
//            {
//                MapBtn[7].gameObject.SetActive(true);
//                Cell ce = new Cell();
//                ce.cellY = entrypoint.cellIdx.cellY;
//                ce.cellX = entrypoint.cellIdx.cellX + 1;
//                dat = GameData.FindMapByCityIdx(entrypoint.cityIdx).Info[ce];
//                if (dat != null)
//                {
//                    MapBtn[7].GetComponentInChildren<Text>().text = dat.Title;
//                    lineArray[8].SetActive(true);
//                    if (dat.Route.ContainsKey(3) && dat.Route[3] == 1)
//                    {
//                        lineArray[10].SetActive(true);
//                        if (MapBtn[6].GetComponentInChildren<Text>().text == "")
//                        {
//                            Cell ce2 = new Cell();
//                            ce2.cellY = entrypoint.cellIdx.cellY - 1;
//                            ce2.cellX = entrypoint.cellIdx.cellX + 1;
//                            SceneUnit dat2 = GameData.FindMapByCityIdx(entrypoint.cityIdx).Info[ce2];
//                            if (dat2 != null)
//                                MapBtn[6].GetComponentInChildren<Text>().text = dat2.Title;
//                        }
//                    }
//                    if (dat.Route.ContainsKey(4) && dat.Route[4] == 1)
//                    {
//                        lineArray[11].SetActive(true);
//                        if (MapBtn[8].GetComponentInChildren<Text>().text == "")
//                        {
//                            Cell ce2 = new Cell();
//                            ce2.cellY = entrypoint.cellIdx.cellY + 1;
//                            ce2.cellX = entrypoint.cellIdx.cellX + 1;
//                            SceneUnit dat2 = GameData.FindMapByCityIdx(entrypoint.cityIdx).Info[ce2];
//                            if (dat2 != null)
//                                MapBtn[8].GetComponentInChildren<Text>().text = dat2.Title;
//                        }
//                    }
//                }
//            }
//            else
//                MapBtn[7].GetComponentInChildren<Text>().text = "";
//        }

//        for (int i = 0; i < MapBtn.Length; i++)
//        {
//            if (string.IsNullOrEmpty(MapBtn[i].GetComponentInChildren<Text>().text))
//                MapBtn[i].gameObject.SetActive(false);
//        }
//    }

//	public void SetCurMap(EntryPoint ept)
//	{
//        Map map = GameData.FindMapByCityIdx(ept.cityIdx);
//        strCity = map.Name;
//        curMapData = map.Info[ept.cellIdx];
//        entrypoint = ept;
//        if (curMapData == null) {
//			Debug.LogError ("Map:" + ept + " missing");
//			return;
//		}
//		MapBtn [curMapBtnIndex].GetComponentInChildren<Text> ().text = curMapData.Title;
//		RefreshText ();
//        RefreshScroll();
//        LoadItems();
//    }

//	public void AppendText(string content)
//	{
//        if (text.text.Length >= 4096)
//            text.text = "";
//        if (!string.IsNullOrEmpty(content))
//        {
//            if (text.text == "")
//                text.text += content;
//            else
//                text.text += "\n" + content;
//        }
//        RefreshScroll();
//    }

//    public void RefreshScroll()
//    {
//        StopCoroutine(YiledRefresh());
//        StartCoroutine(YiledRefresh());
//    }

//    IEnumerator YiledRefresh()
//    {
//        yield return new WaitForSeconds(0.5f);
//        //RectTransform textRect = TextRoot.GetComponent<RectTransform>();
//        float contentHight = TextRoot.GetComponent<RectTransform>().sizeDelta.y;
//        float visibleHeight = TextscrollView.GetComponent<RectTransform>().sizeDelta.y;
//        if (contentHight > visibleHeight)
//        {
//            DOTween.To(() => TextscrollView.GetComponent<ScrollRect>().verticalScrollbar.value, x => TextscrollView.GetComponent<ScrollRect>().verticalScrollbar.value = x, 0.0f, 0.5f);
//        }

//    }

//	public void TextClear()
//	{
//		text.text = "";
//	}

//    //记录到新增到场景里的按钮
//    Dictionary<GameObject, string> SceneMenuDict = new Dictionary<GameObject, string>();
//    public void AddMenuObj(string menu, LuaFunction fn)
//	{
//		GameObject objMenu = null;
//		if (MenuRoot != null) {
//            objMenu = WsWindow.AddMenu("MenuItem", MenuRoot.transform);
//		}
//        Text text = objMenu.GetComponentInChildren<Text>();
//        if (text != null)
//            text.text = menu;
//        Button btnEvent = objMenu.GetComponent<Button>();
//        btnEvent.onClick.RemoveAllListeners();
//        btnEvent.onClick.AddListener(delegate ()
//        {
//            fn.call();
//        });
//        SceneMenuDict.Add(objMenu, menu);
//    }

//    public void RemoveMenuObj(string menu)
//    {
//        GameObject objDel = null;
//        foreach (var each in SceneMenuDict)
//        {
//            if (each.Value == menu)
//                objDel = each.Key;
//        }
//        if (SceneMenuDict.ContainsKey(objDel))
//        {
//            SceneMenuDict.Remove(objDel);
//            WsWindow.RemoveMenu(objDel, MenuRoot.transform);
//        }
//    }

//    public void MenuClear()
//    {
//        foreach (var each in SceneMenuDict)
//            WsWindow.RemoveMenu(each.Key, MenuRoot.transform);
//        SceneMenuDict.Clear();
//    }

//    public void ItemsClear()
//	{
//        WsWindow.CleanMenu(MenuRoot.transform);
//    }

//    public void ShowPlayerInfo()
//    {
//        U3D.ShowPlayerInfo();
//    }

//    public void OnClickInventory()
//    {
//        U3D.OpenInVentory();
//    }

//    public void OnClickSystem()
//    {
//        U3D.OpenSystem();
//    }

//    //进入设置工作界面，所有囚犯
//    public void OnGotoTown()
//    {
//        MainTownCtrl.Ins.Open();
//    }

//    //进入主设置界面.显示 建造 商店 制作 训练 商城
//    public void OnGotoCity()
//    {
//        MainCityCtrl.Ins.Open();
//    }

//    public void OnGotoBattle()
//    {
//        WsWindowEx.OpenSinglePanel<AdminDebugPanel>();
//    }
//}
