using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using SLua;
//用于跨语言交互，由其他语言，反向调用此类的函数.
//using LuaInterface;
using System.Collections.Generic;
using System;
using System.IO;
using CoClass;
using UnityEngine.Profiling;

[CustomLuaClassAttribute]
public class U3D : MonoBehaviour {
    public static U3D ins = null;
    void Awake()
    {
        if (ins == null)
            ins = this;
    }
    // Use this for initialization
    void Start() {
        if (ins != this)
            DestroyImmediate(this);
    }

    void GameStart(string msg)
    {
        Startup.ins.GameStart();
    }


    //控制场景上的文本的函数
    public static void TextAppend(string str)
    {
        if (string.IsNullOrEmpty(str))
            return;
        Say(-1, str);
    }

    public static void TextClear()
    {
        //if (SceneControl.ins != null)
        //	SceneControl.ins.TextClear ();
    }

    //清除所有在此场景物品实例化的按钮,即场景实物，并不清除实务本身.
    //不清除 Menu
    public static void ItemsObjClear()
    {
        //if (SceneControl.ins != null)
        //	SceneControl.ins.ItemsClear();
    }

    public static void ReloadTable()
    {
        TblCore.Instance.Reload();
        //底下这种架构不好，要一个个调用，非常麻烦的，暂时这样
        LevelMng.Instance.ReLoad();
        LoadingTipsManager.Instance.ReLoad();
        UnitMng.Instance.ReLoad();
        WeaponMng.Instance.ReLoad();
        for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
            MeteorManager.Instance.UnitInfos[i].Attr.UpdateAttr();
    }

    //当前场景立即产生一个npc
    static int nextSpawnPoint = 0;
    static int[] weaponCode = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 47, 51, 55 };
    static int weaponIndex = 0;
    public static void SpawnRobot(int idx, EUnitCamp camp)
    {
        MonsterEx mon = new MonsterEx();
        mon.Weapon = weaponCode[weaponIndex % weaponCode.Length];
        mon.Weapon2 = weaponCode[(weaponIndex + 1) % weaponCode.Length];
        weaponIndex++;
        mon.IsPlayer = false;

        mon.name = Global.model[idx % Global.model.Length];
        GameObject objPrefab = Resources.Load("MeteorUnit") as GameObject;
        GameObject ins = GameObject.Instantiate(objPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        MeteorUnit unit = ins.GetComponent<MeteorUnit>();
        unit.Camp = camp;
        unit.Init(idx % Global.model.Length, mon);
        MeteorManager.Instance.OnGenerateUnit(unit);
        unit.SetGround(false);
        //新模式，只有出生点.
        if (Global.GLevelItem.SceneMode == 1)
        {
            unit.transform.position = GameObject.Find("StartPoint").transform.position;
            unit.transform.eulerAngles = new Vector3(0, mon.SpawnDir, 0);
            InsertSystemMsg(LevelHelper.GetCampStr(unit));
            //找寻敌人攻击.因为这个并没有脚本模板
            unit.robot.ChangeState(EAIStatus.Wait);
            return;
        }

        if (Global.GLevelMode == LevelMode.SinglePlayerTask)
        {
            unit.transform.position = Global.GLevelItem.wayPoint.Count > mon.SpawnPoint ? Global.GLevelItem.wayPoint[mon.SpawnPoint].pos : GameObject.Find("StartPoint").transform.position;//等关卡脚本实现之后在设置单机出生点.PlayerEx.Instance.SpawnPoint
            unit.transform.eulerAngles = new Vector3(0, mon.SpawnDir, 0);
        }
        else if (Global.GLevelMode == LevelMode.MultiplyPlayer)
        {
            if (Global.GGameMode == GameMode.Normal)
            {
                unit.transform.position = Global.GLevelItem.wayPoint.Count > mon.SpawnPoint ? Global.GLevelItem.wayPoint[mon.SpawnPoint].pos : GameObject.Find("StartPoint").transform.position;//等关卡脚本实现之后在设置单机出生点.PlayerEx.Instance.SpawnPoint
                unit.transform.eulerAngles = new Vector3(0, mon.SpawnDir, 0);
            }
            else if (Global.GGameMode == GameMode.MENGZHU)
            {
                //16个点
                unit.transform.position = Global.GLevelSpawn[Global.SpawnIndex];
                Global.SpawnIndex++;
                Global.SpawnIndex %= 16;
                unit.transform.eulerAngles = new Vector3(0, mon.SpawnDir, 0);
            }
            else if (Global.GGameMode == GameMode.ANSHA || Global.GGameMode == GameMode.SIDOU)
            {
                //2个队伍8个点.
                if (unit.Camp == EUnitCamp.EUC_FRIEND)
                {
                    unit.transform.position = Global.GCampASpawn[Global.CampASpawnIndex];
                    Global.CampASpawnIndex++;
                    Global.CampASpawnIndex %= 8;
                }
                else if (unit.Camp == EUnitCamp.EUC_ENEMY)
                {
                    unit.transform.position = Global.GCampASpawn[Global.CampBSpawnIndex];
                    Global.CampBSpawnIndex++;
                    Global.CampBSpawnIndex %= 8;
                }
            }
        }

        InsertSystemMsg(LevelHelper.GetCampStr(unit));
        //找寻敌人攻击.因为这个并没有脚本模板
        unit.robot.ChangeState(EAIStatus.Wait);
        //unit.transform.position = Global.GLevelItem.wayPoint.Count != 0 ? Global.GLevelItem.wayPoint[nextSpawnPoint].pos : GameObject.Find("StartPoint").transform.position;
        //unit.transform.rotation = new Quaternion(0, 0, 0, 1);
    }

    public static void ChangePlayerModel(int model)
    {
        Global.PauseAll = true;
        MeteorManager.Instance.LocalPlayer.controller.InputLocked = true;

        MeteorManager.Instance.LocalPlayer.Init(model, MeteorManager.Instance.LocalPlayer.Attr, true);

        //MeteorUnit unit = MeteorManager.Instance.LocalPlayer;
        //MonsterEx mon = unit.Attr;
        //GameObject objPrefab = Resources.Load("MeteorUnit") as GameObject;
        //GameObject objClone = GameObject.Instantiate(objPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        //MeteorUnit unitClone = objClone.GetComponent<MeteorUnit>();
        //MeteorManager.Instance.LocalPlayer = unitClone;
        //unitClone.Camp = EUnitCamp.EUC_FRIEND;//流星阵营
        //unitClone.Init(model, mon);
        //mon.Model = model;
        //MeteorManager.Instance.OnCloneUnit(unit, unitClone);
        ////LuaFunction onInit = ScriptMng.ins.GetFunc("OnInit");
        ////LuaFunction OnStart = ScriptMng.ins.GetFunc("OnStart");
        ////onInit.call(unit.InstanceId);
        ////unit.SetGround(false);
        //unitClone.transform.position = unit.transform.position;// = Global.GLevelItem.wayPoint.Count > mon.SpawnPoint ? Global.GLevelItem.wayPoint[mon.SpawnPoint].pos : GameObject.Find("StartPoint").transform.position;//等关卡脚本实现之后在设置单机出生点.PlayerEx.Instance.SpawnPoint
        //unitClone.transform.eulerAngles = unit.transform.eulerAngles;// = new Vector3(0, mon.SpawnDir, 0);
        //unitClone.gameObject.AddComponent<AudioListener>();

        //GameObject.Destroy(unit.gameObject);
        Global.PauseAll = false;
        MeteorManager.Instance.LocalPlayer.controller.InputLocked = false;
    }

    public static MeteorUnit InitPlayer(LevelScriptBase script)
    {
        MonsterEx mon = SceneMng.InitPlayer(script);
        GameObject objPrefab = Resources.Load("MeteorUnit") as GameObject;
        GameObject ins = GameObject.Instantiate(objPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        MeteorUnit unit = ins.GetComponent<MeteorUnit>();
        MeteorManager.Instance.LocalPlayer = unit;
        unit.Camp = EUnitCamp.EUC_FRIEND;//流星阵营
        unit.Init(mon.Model, mon);
        MeteorManager.Instance.OnGenerateUnit(unit);
        //LuaFunction onInit = ScriptMng.ins.GetFunc("OnInit");
        //LuaFunction OnStart = ScriptMng.ins.GetFunc("OnStart");
        //onInit.call(unit.InstanceId);
        unit.SetGround(false);
        if (Global.GLevelMode == LevelMode.SinglePlayerTask)
        {
            unit.transform.position = Global.GLevelItem.wayPoint.Count > mon.SpawnPoint ? Global.GLevelItem.wayPoint[mon.SpawnPoint].pos : GameObject.Find("StartPoint").transform.position;//等关卡脚本实现之后在设置单机出生点.PlayerEx.Instance.SpawnPoint
            unit.transform.eulerAngles = new Vector3(0, mon.SpawnDir, 0);
        }
        else if (Global.GLevelMode == LevelMode.MultiplyPlayer)
        {
            if (Global.GGameMode == GameMode.Normal)
            {
                unit.transform.position = Global.GLevelItem.wayPoint.Count > mon.SpawnPoint ? Global.GLevelItem.wayPoint[mon.SpawnPoint].pos : GameObject.Find("StartPoint").transform.position;//等关卡脚本实现之后在设置单机出生点.PlayerEx.Instance.SpawnPoint
                unit.transform.eulerAngles = new Vector3(0, mon.SpawnDir, 0);
            }
            else if (Global.GGameMode == GameMode.MENGZHU)
            {
                //16个点
                unit.transform.position = Global.GLevelSpawn[Global.SpawnIndex];
                Global.SpawnIndex++;
                Global.SpawnIndex %= 16;
                unit.transform.eulerAngles = new Vector3(0, mon.SpawnDir, 0);
            }
            else if (Global.GGameMode == GameMode.ANSHA || Global.GGameMode == GameMode.SIDOU)
            {
                //2个队伍8个点.
                if (unit.Camp == EUnitCamp.EUC_FRIEND)
                {
                    unit.transform.position = Global.GCampASpawn[Global.CampASpawnIndex];
                    Global.CampASpawnIndex++;
                    Global.CampASpawnIndex %= 8;
                }
                else if (unit.Camp == EUnitCamp.EUC_ENEMY)
                {
                    unit.transform.position = Global.GCampASpawn[Global.CampBSpawnIndex];
                    Global.CampBSpawnIndex++;
                    Global.CampBSpawnIndex %= 8;
                }
            }
        }
        //OnStart.call();
        U3D.InsertSystemMsg(LevelHelper.GetCampStr(unit));
        return unit;
    }

    public static int AddNPC(string script)
    {
        MeteorUnit target = SceneMng.Spawn(script);
        return target.InstanceId;
    }

    
    public static void InsertSystemMsg(string msg)
    {
        if (!GameOverlayWnd.Exist)
            GameOverlayWnd.Instance.Open();
        GameOverlayWnd.Instance.InsertSystemMsg(msg);
    }

    //从mapunit.xls读取idx原件，放到当前场景上,只是放到配置上。但是不生成
    public static void AddMapUnit(int mapunitIdx)
    {
        //SceneMng.MakeInstance(mapunitIdx);
    }

    //删除一个指定名称的按钮.若有相同的，则删除找到的最后一个.
    public static void RemoveMenu(string menu)
    {
        if (string.IsNullOrEmpty(menu))
        {
            Debug.LogError("is a hide button");
        }
        //if (SceneControl.ins != null)
        //    SceneControl.ins.RemoveMenuObj(menu);
    }

    
    //增加一个按钮，但按钮是没有状态的，下次进这个场景，这个按钮不存在
    //要添加可以在此场景保存的物品，调用AddMapUnit.这个讲会在重进入场景时，复原.
	public static void AddMenu(string menu, LuaFunction fn)
	{
		if (string.IsNullOrEmpty (menu)) {
			Debug.LogError ("is a hide button");
		}
		if (fn == null) {
			Debug.LogError ("a invalid function");
			return;
		}
		//if (SceneControl.ins != null)
		//	SceneControl.ins.AddMenuObj (menu, fn);
	}

    //清理掉加到场景 物件 部分的 按钮，对NPC,随机怪，场景物件不管
    public static void MenuClear()
    {
        //SceneControl.ins.MenuClear();
    }

    public static void EnterMap(int mapid, int x, int y)
	{
  //      EntryPoint ept = new EntryPoint();
  //      ept.cityIdx = mapid;
  //      ept.cellIdx = new Cell();
  //      ept.cellIdx.cellX = x;
  //      ept.cellIdx.cellY = y;
		//SceneMng.EntryMap (ept);
	}

    //查看物品元数据.基础表格信息
	public static void ViewItem(int unitid)
	{
		//Camera.main.gameObject.GetComponent<Startup> ().ShowItemInfo (unitid);
	}

    //请教NPC
    public static void FightWithNpc(int npcIdx)
    {
        
    }

    //杀死NPC
    public static void KillNpc(int npcIdx)
    {
        
    }

    public static bool FindNpc(int idx, int mapidx, int cellx, int celly)
    {
        //EntryPoint ep = new EntryPoint();
        //ep.cityIdx = mapidx;
        //ep.cellIdx = new Cell(cellx, celly);
        //return SceneMng.FindNpc(idx, ep);
        return false;
    }

    //点击下方的 物品，NPC按钮，弹出一个简单提示
    public static void PopupTip(string str)
    {
        PopupTip tip = WsWindow.OpenMul<PopupTip>(WsWindow.PopupTip);
        tip.Popup(str);
    }
    public static void PopupTip(int strIden)
    {
        PopupTip tip = WsWindow.OpenMul<PopupTip>(WsWindow.PopupTip);
        LangBase langIt = GameData.Instance.langMng.GetRowByIdx((int)strIden) as LangBase;
        string str = "";
        if (Lang == (int)LanguageType.Ch && langIt != null)
        str = langIt.Ch;
        if (Lang == (int)LanguageType.En && langIt != null)
            str = langIt.En;
        tip.Popup(str);
    }

    //为所有的类包装.脚本里只需要在调用这里的函数.
    public static void AddItem(int idx, uint count)
    {
        //Player.Instance.AddItem(name, count);
    }

    public static void OnDeadEnd()
    {
        //SceneMng.OnDeadEnd();
    }

    public static void OnDead(int battleId, LuaFunction fn)
    {
        //SceneMng.OnDead(battleId, fn);
    }

    public static void StartBattle(int battleid)
    {
        //SceneMng.StartBattle(battleid);
    }

    //完成指定任务上带的战斗.
    public static void StartTaskBattle(int task, LuaFunction onSuccessful)
    {
        //TaskUnit ta = GameData.FindTaskByIdx(task);
        //if (ta.TaskType == (int)TaskType.BattleDone)
        //{
        //    if (onSuccessful != null)
        //        OnDead(ta.CompleteBattleId, onSuccessful);//战斗胜利后调用剧情.保留在战斗界面.
        //    SceneMng.StartBattle(ta.CompleteBattleId);

        //}
    }

    public static bool ContainsItem(string item)
    {
        //return SceneMng.ContainsItem(item);
        return false;
    }
    static AsyncOperation backOp;
    static AsyncOperation loadMainOp;
    static Coroutine loadMain;
    //返回到主目录
    public static void GoBack(Action t = null)
    {
        Global.GLevelItem = null;
        if (loadMain != null)
        {
            ins.StopCoroutine(loadMain);
            loadMain = null;
        }
        loadMain = ins.StartCoroutine(ins.LoadMainWnd(t));
    }

    //修改版本号后回到Startup重新加载资源
    public static void ReStart()
    {
        Global.GLevelItem = null;
        if (loadMain != null)
        {
            ins.StopCoroutine(loadMain);
            loadMain = null;
        }
        loadMain = ins.StartCoroutine(ins.LoadStartup());
    }

    IEnumerator LoadStartup()
    {
        ResMng.LoadScene("Startup");
        backOp = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Startup", UnityEngine.SceneManagement.LoadSceneMode.Single);//.LoadSceneAsync_s (1);
        yield return backOp;
    }

    IEnumerator LoadMainWnd(Action t)
    {
        //Debug.LogError("LoadScenes Menu");
        ResMng.LoadScene("Menu");
        loadMainOp = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Menu", UnityEngine.SceneManagement.LoadSceneMode.Single);//.LoadSceneAsync_s (1);
        yield return loadMainOp;
        OnLoadMainFinished(t);
    }

    void OnLoadMainFinished(Action t)
    {
        MainWnd.Instance.Open();
        if (t != null)
            t.Invoke();
    }

    //当前场景里的一个宝箱是否是空的.
    public static bool ItemIsEmpty(string mapObject)
    {
        //return SceneMng.ItemIsEmpty(mapObject);
        return false;
    }

    public static FileStream save;
    //清理存档文件
    public static void SaveClean()
    {
        if (save != null)
            save.Close();
        save = null;
    }

    public static void SaveState(string str, object val)
    {
        if (save == null)
        {
            string path = string.Format("{0}/{1}", Application.persistentDataPath, GameData.Instance.gameStatus.saveSlot);
            string script_path = string.Format("{0}/script_status.txt", path);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            save = File.Open(script_path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            save.SetLength(0);
        }
        byte[] line = new byte[2] { (byte)'\r', (byte)'\n' };
        if (save != null)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(string.Format("{0}={1}", str, val));
            save.Write(buffer, 0, buffer.Length);
            save.Write(line, 0, 2);
            save.Flush();
        }
    }

    public static void SaveDone()
    {
        if (save != null)
        {
            save.Flush();
            save.Close();
        }
        save = null;
    }

    public static void PlaySound(string wav)
    {
        
    }

    public static void NpcTalkClose()
    {
        WsWindow.Close(WsWindow.NpcTalkPanel);    
    }

    //模拟炼铁狱全BUFF
    public static void Fullup()
    {
    }

    public static void OpenRobotWnd()
    {
        if (RobotWnd.Exist)
            RobotWnd.Instance.Close();
        else
            RobotWnd.Instance.Open();
    }

    public static void OpenSfxWnd()
    {
        if (SfxWnd.Exist)
            SfxWnd.Instance.Close();
        else
            SfxWnd.Instance.Open();
    }

    //打开武器界面，主角色调试切换主手武器.
    public static void OpenWeaponWnd()
    {
        if (WeaponWnd.Exist)
            WeaponWnd.Instance.Close();
        else
            WeaponWnd.Instance.Open();
    }

    public static void OpenSystemWnd()
    {
        if (Global.GLevelItem != null)
        {
            NewSystemWnd.Instance.DoModal();
            if (NGUIJoystick.instance != null)
                NGUIJoystick.instance.Lock(true);
            return;
        }

        //WsWindow.Close(WsWindow.System);
        //GameObject ctrl = WsWindow.Open(WsWindow.System);
        //RectTransform rectTran = ctrl.GetComponent<RectTransform>();
        //if (rectTran != null)
        //    rectTran.sizeDelta = new Vector2(0, 0);
    }

    

    //开启一个传送点，可以从此传送点，传送到目的位置
    public static void EnableGate(int sourceDoor)
    {
        //GameData.save.loadDoneList;
    }

    //显示0-4 全部-道术师-步兵-骑兵-弓弩手 供招募的
    public static void ShowEmploy(int type)
    {
        ArmyShopCtrl ctrl =  WsWindow.Open<ArmyShopCtrl>(WsWindow.ArmyShop);
        RectTransform rectTran = ctrl.GetComponent<RectTransform>();
        if (rectTran != null)
        {
            rectTran.sizeDelta = new Vector2(0, 0);
            //rectTran.anchorMin = new Vector2(0, 0);
            //rectTran.anchorMax = new Vector2(1, 1);
        }
        ctrl.BindArmyType(type);
        ctrl.Reset();
        ctrl.UpdateUI();
    }

    public static void EnableUIFunc(int func)
    {
        //UIFunction fun = GameData.FindUIFunc(func);
        //if (fun != null)
        //{
        //    if (!GameData.MainRole.EnableUIFunc.ContainsKey(func))
        //    {
        //        GameData.MainRole.EnableUIFunc.Add(func, false);//false指示这个UI功能还没有点击过.
        //        if (MainCityCtrl.Ins != null)
        //            MainCityCtrl.Ins.UpdateUI();
        //    }
        //}
    }

    //通过官阶来开启可招募士兵.
    public static void EnableArmyLevel(int level)
    {
        //for (int i = 0; i < GameData.Data.MonsterList.Count; i++)
        //{
        //    if (GameData.Data.MonsterList[i].Level == level)
        //        EnableArmy(GameData.Data.MonsterList[i].Idx);
        //}
    }

    public static void EnableBuild(int build)
    {
        //MapObject objBuild = GameData.FindNpcById(npc);
        //foreach (var each in WorkFactory.Ins.Factory)
        //{
        //    if (objBuild.PropertyIdx == each.Value.BuildingPropertyIdx)
        //        return;
        //}
    }

    //这种技能都是兵种大类属性技能，比如骑术，骑兵的护甲
    public static void EnableSkill(int skill)
    {
        
    }

    //显示建筑列表，包括可建造及已建造的
    public static void ShowBuild()
    {
        //BuildListCtrl.Ins.Open();
    }

    public static void ChangeLang(int lang)
    {
        //重新加载数据
        Lang = lang;
        //GameData.LoadData();
        //if (GameData.save != null)
        //    GameData.save.ChangeLanguage(lang);
        LangItem.ChangeLang();
        //MainCityCtrl.Ins.ChangeLang();
        //MainTownCtrl.Ins.ChangeLang();
        //BuildListCtrl.Ins.ChangeLang();
    }

    public static void OpenLangSel()
    {
        LanguageSelectCtrl ctrl = WsWindow.Open<LanguageSelectCtrl>(WsWindow.LanguageSelect);
        RectTransform rectTran = ctrl.GetComponent<RectTransform>();
        if (rectTran != null)
            rectTran.sizeDelta = new Vector2(0, 0);
        ctrl.UpdateUI();
    }

    public static bool EatFood()
    {
        //在作战行囊里找到食物，如果有则吃掉一个，没有返回false;
        //UnitId.Food
        //if (GameData.MainInventory.GetItemCount((int)UnitId.Food) > 0)
        //{
        //    GameData.MainInventory.RemoveItemCnt((int)UnitId.Food, 1);
        //    return true;
        //}
        return false;
    }

    //允许用一些物品合成一件物品.
    public static void EnableMakeItem(int idx)
    {
        ItemBase it = GameData.Instance.FindItemByIdx(idx);
        if (it != null)
        {
            //if (!GameData.MainRole.EnableMakeList.Contains(idx))
            //{
            //    GameData.MainRole.EnableMakeList.Add(idx);
                
            //    //可以新制造一件物品的时候，UI功能制造按钮，会重新设置为没点击过
            //    if (GameData.MainRole.EnableUIFunc.ContainsKey((int)UIFuncType.Produce))
            //        GameData.MainRole.EnableUIFunc[(int)UIFuncType.Produce] = true;
            //    MainCityCtrl.Ins.AppendText(LangItem.GetLangString(StringIden.CanMake) + StringTbl.unitPrefix + it.Name + StringTbl.unitSuffix);
            //    MainCityCtrl.Ins.UpdateUI();
            //}
        }
    }

    static void SaveLastLevelData()
    {
        //if (GameData.MainRole.lastLevel != 0)
        //{
        //    if (SceneMng.hasLoaded(GameData.MainRole.lastLevel))
        //    {
        //        //只要是要存储的都会绑定一个此数据.在剔除一个场景序列化对象时,会先在对应的 mapObject里删除掉那一项数据
        //        MapUnitCtrl[] ctrls = GameObject.FindObjectsOfType<MapUnitCtrl>();
        //        for (int i = 0; i < ctrls.Length; i++)
        //            ctrls[i].Save();
        //    }
        //}
    }

    public static void LoadLevel(int id, LevelMode levelmode, GameMode gamemode)
    {
        LoadLevel(id, LoadingWnd.Instance, levelmode, gamemode);
    }

    public static void LoadLevel(int id, LoadingUI loading, LevelMode levelmode, GameMode gamemode)
    {
        uint profileTotalAllocate = Profiler.GetTotalAllocatedMemory();
        uint profileTotalReserved = Profiler.GetTotalReservedMemory();
        long gcTotal = System.GC.GetTotalMemory(false);
        Log.Write("profile totalAllocate:" + profileTotalAllocate + " profile TotalReserved:" + profileTotalReserved + " gc totalAllocate:" + gcTotal);
        Log.Write("start load level:" + id);
        if (FightWnd.Exist)
            FightWnd.Instance.Close();
        //if (StateWnd.Exist)
        //    StateWnd.Instance.Close();
        WindowMng.CloseAll();
        Log.Write("WindowMng.CloseAll();");
        //暂时不允许使用声音管理器，在切换场景时不允许播放
        SoundManager.Instance.StopAll();
        SoundManager.Instance.Enable(false);
        SaveLastLevelData();
        ClearLevelData();
        Log.Write("ClearLevelData");
        Level lev = LevelMng.Instance.GetItem(id);
        Global.GLevelItem = lev;
        Global.GLevelMode = levelmode;
        Global.GGameMode = gamemode;
        Log.Write("Global.GLevelItem = lev;");
        LoadingWnd.Instance.Open();
        Log.Write("LoadingWnd.Instance.Open();");
        Resources.UnloadUnusedAssets();
        GC.Collect();
        if (!string.IsNullOrEmpty(lev.sceneItems))
        {
            string num = lev.sceneItems.Substring(2);
            int number = 0;
            if (int.TryParse(num, out number))
            {
                //Debug.Log("b" + number);
                PlayMovie("b" + number);
            }
        }
        LevelHelper helper = ins.gameObject.AddComponent<LevelHelper>();
        helper.Load(id, loading);
        Log.Write("helper.load end");
    }

    static void ClearLevelData()
    {
        SFXLoader.Instance.Effect.Clear();
        DesLoader.Instance.Refresh();
        GMCLoader.Instance.Refresh();
        GMBLoader.Instance.Refresh();
        FMCLoader.Instance.Refresh();
        //先清理BUF
        BuffMng.Instance.Clear();
        MeteorManager.Instance.Clear();
        LevelScriptBase.Clear();
#if !STRIP_DBG_SETTING
        WSDebug.Ins.Clear();
#endif
    }

    //对接原版脚本
    static Loader sceneRoot;
    public static void SetSceneItem(string name, string features, int value1)
    {
        if (sceneRoot == null)
            sceneRoot = FindObjectOfType<Loader>();
        GameObject objSelected = Global.Control(name, sceneRoot.gameObject);
        if (objSelected == null)
        {
            //Debug.LogError(name + " can not find");
            return;
        }
        SceneItemAgent agent = objSelected.GetComponent<SceneItemAgent>();
        if (agent != null)
            agent.SetSceneItem(features, value1);
    }

    public static void SetSceneItem(string name, string features, string sub_features, int value)
    {
        if (sceneRoot == null)
            sceneRoot = FindObjectOfType<Loader>();
        GameObject objSelected = Global.Control(name, sceneRoot.gameObject);
        if (objSelected == null)
        {
            //Debug.LogError(name + " can not find");
            return;
        }
        SceneItemAgent agent = objSelected.GetComponent<SceneItemAgent>();
        if (agent != null)
            agent.SetSceneItem(features, sub_features, value);
    }

    //setSceneItem("xx", "pos", posid, loop)
    public static void SetSceneItem(string name, string features, int value1, int value2)
    {
        GameObject objSelected = Global.Control(name, Loader.Instance.gameObject);
        if (objSelected == null)
        {
            //Debug.LogError(name + " can not find");
            return;
        }
        SceneItemAgent agent = objSelected.GetComponent<SceneItemAgent>();
        if (agent != null)
            agent.SetSceneItem(features, value1, value2);
    }

    public static void SetSceneItem(int id, string features, string sub_feature, int value)
    {
        SceneItemAgent objSelected = null;
        SceneItemAgent[] agents = FindObjectsOfType<SceneItemAgent>();
        for (int i = 0; i < agents.Length; i++)
        {
            if (agents[i].InstanceId == id)
            {
                objSelected = agents[i];
                break;
            }
        }
        if (objSelected == null)
        {
            //Debug.LogError("id: " + id + " can not find");
            return;
        }
        if (objSelected != null)
            objSelected.SetSceneItem(features, sub_feature, value);
    }

    public static void SetSceneItem(int id, string features, int value1, int value2)
    {
        SceneItemAgent objSelected = null;
        SceneItemAgent[] agents = FindObjectsOfType<SceneItemAgent>();
        for (int i = 0; i < agents.Length; i++)
        {
            if (agents[i].InstanceId == id)
            {
                objSelected = agents[i];
                break;
            }
        }
        if (objSelected == null)
        {
            //Debug.LogError("id: " + id + " can not find");
            return;
        }
        if (objSelected != null)
            objSelected.SetSceneItem(features, value1, value2);
    }

    public static void SetSceneItem(int id, string feature, string value)
    {
        SceneItemAgent objSelected = null;
        SceneItemAgent[] agents = FindObjectsOfType<SceneItemAgent>();
        for (int i = 0; i < agents.Length; i++)
        {
            if (agents[i].InstanceId == id)
            {
                objSelected = agents[i];
                break;
            }
        }
        if (objSelected == null)
        {
            //Debug.LogError("id: " + id + " can not find");
            return;
        }
        if (objSelected != null)
            objSelected.SetSceneItem(feature, value);
    }
    public static void SetSceneItem(string name, string feature, string value)
    {
        GameObject objSelected = Global.Control(name, Loader.Instance.gameObject);
        if (objSelected == null)
        {
            //Debug.LogError(name + " can not find");
            return;
        }
        SceneItemAgent agent = objSelected.GetComponent<SceneItemAgent>();
        if (agent != null)
            agent.SetSceneItem(feature, value);
    }

    public static SceneItemAgent GetSceneFlag()
    {
        for (int i = 0; i < MeteorManager.Instance.SceneItems.Count; i++)
        {
            if (MeteorManager.Instance.SceneItems[i].ItemInfo != null && MeteorManager.Instance.SceneItems[i].ItemInfo.IsFlag())
                return MeteorManager.Instance.SceneItems[i];
        }
        return null;
    }

    public static SceneItemAgent GetSceneItem(string name)
    {
        for (int i = 0; i < MeteorManager.Instance.SceneItems.Count; i++)
        {
            if (MeteorManager.Instance.SceneItems[i].name == name)
                return MeteorManager.Instance.SceneItems[i];
        }
        return null;
    }

    public static int GetSceneItem(string name, string feature)
    {
        for (int i = 0; i < MeteorManager.Instance.SceneItems.Count; i++)
        {
            if (MeteorManager.Instance.SceneItems[i].name == name)
                return MeteorManager.Instance.SceneItems[i].GetSceneItem(feature);
        }
        return -1;
    }

    public static SceneItemAgent GetSceneItem(int id)
    {
        for (int i = 0; i < MeteorManager.Instance.SceneItems.Count; i++)
        {
            if (MeteorManager.Instance.SceneItems[i].InstanceId == id)
                return MeteorManager.Instance.SceneItems[i];
        }
        return null;
    }
    public static int GetSceneItem(int id, string feature)
    {
        for (int i = 0; i < MeteorManager.Instance.SceneItems.Count; i++)
        {
            if (MeteorManager.Instance.SceneItems[i].InstanceId == id)
                return MeteorManager.Instance.SceneItems[i].GetSceneItem(feature);
        }
        return -1;
    }

    public static int GetTeam(int characterId)
    {
        for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
        {
            if (MeteorManager.Instance.UnitInfos[i].InstanceId == characterId)
                return (int)MeteorManager.Instance.UnitInfos[i].Camp;
        }
        return -1;
    }

    public static void CreateEffect(string target, string effect, bool loop = false)
    {
        if (effect == "GiMaHIT")
            Debug.LogError("find");
        GameObject objEffect = null;
        for (int i = 0; i < Loader.Instance.transform.childCount; i++)
        {
            GameObject obj = Loader.Instance.transform.GetChild(i).gameObject;
            if (obj.name == target)
            {
                objEffect = obj;
                break;
            }
        }
        if (SFXLoader.Instance != null && objEffect != null)
            SFXLoader.Instance.PlayEffect(effect, objEffect, !loop);
    }
    public static void CreateEffect(int id, string effect)
    {
        //if (effect == "GiMaHIT")
            //Debug.LogError("find");
        SceneItemAgent[] agents = FindObjectsOfType<SceneItemAgent>();
        SceneItemAgent objEffect = null;
        for (int i = 0; i < agents.Length; i++)
        {
            if (agents[i].InstanceId == id)
            {
                objEffect = agents[i];
                break;
            }
        }
        if (SFXLoader.Instance != null && objEffect != null)
            SFXLoader.Instance.PlayEffect(effect, objEffect.gameObject, true);
    }

    //开启同步和关闭同步，这里先放着，等联机时实现
    public static void NetEvent(int status)
    {

    }

    public static MeteorUnit GetTeamLeader(EUnitCamp camp)
    {
        if (camp == EUnitCamp.EUC_FRIEND)
            return MeteorManager.Instance.LocalPlayer;
        else if (camp == EUnitCamp.EUC_ENEMY)
        {
            int hpMax = 0;
            MeteorUnit ret = null;
            for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
            {
                if (MeteorManager.Instance.UnitInfos[i].Camp == camp)
                {
                    if (ret != null)
                    {
                        if (hpMax < MeteorManager.Instance.UnitInfos[i].Attr.HpMax)
                        {
                            hpMax = MeteorManager.Instance.UnitInfos[i].Attr.HpMax;
                            ret = MeteorManager.Instance.UnitInfos[i];
                        }
                    }
                    return ret;
                }
            }
        }
        else
        {
            Debug.LogError("other camp has no vip");
        }
        return null;
    }

    static MeteorUnit GetEnemyTeamLeader(EUnitCamp camp)
    {
        if (camp == EUnitCamp.EUC_ENEMY)
            return GetTeamLeader(EUnitCamp.EUC_FRIEND);
        else if (camp == EUnitCamp.EUC_FRIEND)
            return GetTeamLeader(EUnitCamp.EUC_ENEMY);
        return null;
    }

    //提供给外部脚本调的
    public static void ChangeBehavior(int id, params object[] value)
    {
        if (value != null && value.Length != 0)
        {
            string act = value[0] as string;
            if (act == "wait")
            {
                GameBattleEx.Instance.PushActionWait(id);
            }
            else if (act == "follow")
            {
                object target = null;
                if (value[1].GetType() == typeof(int))
                {
                    GameBattleEx.Instance.PushActionFollow(id, (int)value[1]);
                }
                else
                {
                    target = value[1] as string;
                    if (value[1] as string == "vip")
                    {
                        //跟随己方首领.
                        MeteorUnit u = GetUnit(id);
                        if (u != null)
                        {
                            MeteorUnit t = GetTeamLeader(u.Camp);
                            if (t != null)
                                GameBattleEx.Instance.PushActionFollow(id, t.InstanceId);
                        }
                    }
                    else if (value[1] as string == "enemyvip")
                    {
                        //跟随敌方首领.
                        MeteorUnit u = GetUnit(id);
                        if (u != null)
                        {
                            MeteorUnit t = GetEnemyTeamLeader(u.Camp);
                            if (t != null)
                                GameBattleEx.Instance.PushActionFollow(id, t.InstanceId);
                        }
                    }
                    else if (value[1] as string == "player")
                    {
                        GameBattleEx.Instance.PushActionFollow(id, MeteorManager.Instance.LocalPlayer.InstanceId);
                    }
                    else
                    {
                        int flag = U3D.GetChar("flag");//跟随镖物跑，A，镖物被人取得，B，镖物在地图某处.
                        if (flag >= 0)
                        {
                            GameBattleEx.Instance.PushActionFollow(id, flag);
                        }
                        else
                        {
                            Debug.LogError(string.Format("follow flag:{0} can not find", flag));
                        }
                    }
                }
            }
            else if (act == "patrol")
            {
                List<int> Path = new List<int>();
                for (int i = 1; i < value.Length; i++)
                {
                    if (value[i] == null)
                        continue;
                    if (value[i].GetType() == typeof(double))
                        Path.Add((int)(double)value[i]);
                    else if (value[i].GetType() == typeof(int))
                        Path.Add((int)value[i]);
                }
                GameBattleEx.Instance.PushActionPatrol(id, Path);
            }
            else if (act == "faceto")
            {
                GameBattleEx.Instance.PushActionFaceTo(id, (int)value[1]);
            }
            else if (act == "kill")
            {
                //Debug.LogError("gamebattleex kill");
                GameBattleEx.Instance.PushActionKill(id, (int)value[1]);
            }
            else if (act == "idle")
            {
                //原地不动.
            }
            else if (act == "attacktarget")
            {
                if (value.Length == 2)
                {
                    //UnityEngine.Debug.LogError(string.Format("attacktarget:{0}", (int)value[1]));
                    GameBattleEx.Instance.PushActionAttackTarget(id, (int)value[1]);
                }
                else if (value.Length == 3)
                {
                    //UnityEngine.Debug.LogError(string.Format("attacktarget:{0}, {1}", (int)value[1], (int)value[2]));
                    GameBattleEx.Instance.PushActionAttackTarget(id, (int)value[1], (int)value[2]);
                }
            }
            else if (act == "run")
            {
                //乱跑.可能是一段时间在主角附近找一个4层路点，然后跑到该路点去，到达之后，重复重复再重复.
                MeteorUnit un = GetUnit(id);
                Debug.Log(string.Format("level:{0} player:{1} run", Global.GLevelItem.ID, un.name));
            }
        }
    }
    // behavior="wait", "idle", "run", "follow", "patrol", "attacktarget", "kill"
    //只有string 和 int类型，后者
    public static void ChangeBehaviorEx(int id, string act, params object[] value)
    {
        object[] param = new object[value == null ? 1 : value.Length + 1];
        param[0] = act;
        if (value != null)
        {
            for (int i = 0; i < value.Length; i++)
                param[i + 1] = value[i];
        }
        ChangeBehavior(id, param);
    }

    public static MeteorUnit GetFlag()
    {
        for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
        {
            if (MeteorManager.Instance.UnitInfos[i].GetFlag)
                return MeteorManager.Instance.UnitInfos[i];
        }
        return null;
    }

    public static int GetChar(string player)
    {
        if (player == "player")
            return MeteorManager.Instance.LocalPlayer.InstanceId;
        //取得谁获得了任务物品
        if (player == "flag")
        {
            for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
            {
                if (MeteorManager.Instance.UnitInfos[i].GetFlag)
                    return MeteorManager.Instance.UnitInfos[i].InstanceId;
            }
            return -1;
        }
        for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
        {
            if (MeteorManager.Instance.UnitInfos[i].name == player)
                return MeteorManager.Instance.UnitInfos[i].InstanceId;
        }
        return -1;
    }

    // name="player", "vip", "enemyvip", "flag", "xxx"
    public static int GetAnyChar(string name)
    {
        if (name == "player")
            return 0;
        for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
        {
            if (MeteorManager.Instance.UnitInfos[i].name == name)
                return MeteorManager.Instance.UnitInfos[i].InstanceId;
        }
        for (int i = 0; i < MeteorManager.Instance.DeadUnits.Count; i++)
        {
            if (MeteorManager.Instance.DeadUnits[i].name == name)
                return MeteorManager.Instance.DeadUnits[i].InstanceId;
        }
        return -1;
    }	// including dead char
    public static int GetSelf(int self)
    {
        return self;
    }

    public static int GetAngry(int id)
    {
        MeteorUnit unit = GetUnit(id);
        if (unit != null)
            return unit.AngryValue;
        return 0;
    }

    public static int GetHP(int id)
    {
        for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
        {
            if (MeteorManager.Instance.UnitInfos[i].InstanceId == id)
                return MeteorManager.Instance.UnitInfos[i].Attr.hpCur;
        }
        return 0;
    }
    public static int GetMaxHP(int id)
    {
        for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
        {
            if (MeteorManager.Instance.UnitInfos[i].InstanceId == id)
                return MeteorManager.Instance.UnitInfos[i].Attr.HpMax;
        }
        return 0;
    }

    public static int GetEnemy(int id)
    {
        MeteorUnit unit = GetUnit(id);
        if (unit == null)
            return -1;
        MeteorUnit enemy = unit.GetLockedTarget();
        if (enemy != null)
            return enemy.InstanceId;
        return -1;
    }
    public static int GetLeader(int id)
    {
        return 0;
    }
    public static int GetGameTime()
    {
        return 0;
    }
    public static int EnableWaypoints(int a, int b, params int[] way)
    {
        return 0;
    }
    public static int DisableWaypoints(int a, int b, params int[] way)
    {
        return 0;
    }

    public static int Perform(int id, string pose, int last)
    {
        return Perform(id, pose, new int[] { last });
    }

    public static int Perform(int id, string pose, params int[] fun)
    {
        MeteorUnit unit = GetUnit(id);
        if (unit != null)
        {
            if (pose == "pause" && fun != null && fun.Length == 1)
            {
                GameBattleEx.Instance.PushActionPause(id, fun[0]);
            }
            //unit.PauseAI(fun[0]);
            else if (pose == "faceto" && fun != null && fun.Length == 1)
            {
                MeteorUnit target = GetUnit(fun[0]);
                if (target != null)
                    unit.FaceToTarget(target);
            }
            else if (pose == "guard" && fun != null && fun.Length == 1)
                GameBattleEx.Instance.PushActionGuard(id, fun[0]);
            else if (pose == "aggress")
                GameBattleEx.Instance.PushActionAggress(id);
            else if (pose == "attack")
            {
                //攻击。？？
            }
            else if (pose == "use")
            {
                unit.GetItem(fun[0]);
            }
            else if (pose == "skill")
            {
                GameBattleEx.Instance.PushActionSkill(id); //unit.PlaySkill();
            }
            else if (pose == "crouch")
                GameBattleEx.Instance.PushActionCrouch(id, fun[0]);//1是指令状态，1代表应用状态， 0代表取消状态
            else if (pose == "block")
                GameBattleEx.Instance.PushActionBlock(id, fun[0]);//阻止输入/取消阻止输入.与硬直应该差不多.

        }
        return 0;
    }

    public static int Perform(int id, string pose, string param)
    {
        if (pose == "say")
            Say(id, param);
        return 0;
    }
    // pose="faceto", "say", "pause", "aggress", "jump", "attack", "guard", "crouch", "use"
    public static int PlayerPerform(string act, int param)
    {
        if (act == "pause")
            GameBattleEx.Instance.PushActionPause(0, param);//0代表主角色.
        else if (act == "use")
        {
            MeteorManager.Instance.LocalPlayer.GetItem(param);
            return 1;
        }
        else if (act == "crouch")
            GameBattleEx.Instance.PushActionCrouch(0, param);//1是指令状态，1代表应用状态， 0代表取消状态
        else if (act == "block")
            GameBattleEx.Instance.PushActionBlock(0, param);//阻止输入/取消阻止输入.与硬直应该差不多.
        else if (act == "guard")
            GameBattleEx.Instance.PushActionGuard(0, param);
        else if (act == "faceto")
        {
            MeteorUnit target = GetUnit(param);
            if (target != null)
                MeteorManager.Instance.LocalPlayer.FaceToTarget(target);
        }
        //Debug.Log("PlayerPerform(string act, int param):" + "act:" + act + " param:" + param);
        return 0;
    }
    public static int PlayerPerform(string act, string content)
    {
        if (act == "say")
            Say(0, content);
        return 0;
    }
    public static int PlayerPerform(int id, string pose, params int[] fun)
    {
        return 0;
    }
    // pose="say", "pause", "use", "block"
    public static int StopPerform(int id)
    {
        MeteorUnit u = GetUnit(id);
        //停止动作的时候，状态为等待，一般遇见敌人时会调用此接口
        if (u.robot != null)
            u.robot.ChangeState(EAIStatus.Wait);
        if (u != null)
        {
            //先清除该角色的聊天动作，其他动画动作不处理。
            GameBattleEx.Instance.StopAction(id);
        }
        return 0;
    }
    public static int IsPerforming(int player)
    {
        if (GameBattleEx.Instance != null)
            return GameBattleEx.Instance.IsPerforming(player) ? 1: 0;
        return 0;
    }

    public static int SetTarget(int idx, string type, params int[] fun)
    {
        return 0;
    }// type="char", "waypoint", "flag", "safe"
    public static int Distance(int idx1, int idx2)
    {
        return 0;
    }

    public static int Rand(int n)
    {
        return 0;
    }
    public static void RemoveNPC(int id)
    {
        MeteorUnit unit = GetUnit(id);
        string message = unit.name + " 离开战场";
        MeteorManager.Instance.OnRemoveUnit(unit);
        InsertSystemMsg(message);
    }

    public static void UpdateAIAttrib(int id)
    {
        MeteorUnit monster = GetUnit(id);
        monster.Attr.UpdateAttr();
    }

    public static int Call(int id, string functionName, params object[] param)
    {
        return 0;
    }

    public static MeteorUnit GetUnit(int id)
    {
        for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
        {
            if (MeteorManager.Instance.UnitInfos[i].InstanceId == id)
                return MeteorManager.Instance.UnitInfos[i];
        }

        for (int i = 0; i < MeteorManager.Instance.DeadUnits.Count; i++)
        {
            if (MeteorManager.Instance.DeadUnits[i].InstanceId == id)
                return MeteorManager.Instance.DeadUnits[i];
        }

        for (int i = 0; i < MeteorManager.Instance.LeavedUnits.Count; i++)
        {
            if (MeteorManager.Instance.LeavedUnits[i].InstanceId == id)
                return MeteorManager.Instance.LeavedUnits[i];
        }

        return null;
    }

    public static void Say(int id, string param)
    {
        if (GameBattleEx.Instance != null)
            GameBattleEx.Instance.PushActionSay(id, param);//1=say 2=pause
    }

    public static int Print(int a, string b, string c, params object[] param)
    {
        return 0;
    }

    public static void PlayMovie(string movie)
    {
        //Handheld.PlayFullScreenMovie(movie, Color.black, FullScreenMovieControlMode.CancelOnInput, FullScreenMovieScalingMode.AspectFit);
    }

    public static int GetEnemyCount()
    {
        int total = 0;
        for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
        {
            if (MeteorManager.Instance.UnitInfos[i].Camp != EUnitCamp.EUC_FRIEND)
                total++;
        }
        return total;
    }

    //脚本输入框支持
    //角色获得物品-可拾取类
    public static void GetItem(int player, int item)
    {
        MeteorUnit u = GetUnit(player);
        if (u != null)
            u.GetItem(item);
    }

    //角色获得BUFF
    public static void AddBuff(int player, int buff)
    {
        GetItem(player, buff);
    }

    //角色获得怒气
    public static void AddAngry(int player, int angry)
    {
        MeteorUnit u = GetUnit(player);
        if (u != null)
            u.AddAngry(angry);
    }

    //角色放大招
    public static void PlaySkill(int player)
    {
        MeteorUnit u = GetUnit(player);
        if (u != null)
            u.PlaySkill();
    }

    //665->剔骨 W11_4
    public static void Drop(int itemIdx)
    {
        MakeItem(itemIdx);
    }
    //仍物品到前方地面上
    public static void MakeItem(int itemIdx)
    {
        ItemBase it = GameData.Instance.FindItemByIdx(itemIdx);
        if (it.MainType == 1)//武器
            DropMng.Instance.DropWeapon2(it.UnitId);
        else if (it.MainType == 2)//
        {

        }
    }

    public static int Lang
    {
        get
        {
            if (GameData.Instance.gameStatus == null)
                return (int)LanguageType.En;
            else
                return GameData.Instance.gameStatus.Language;
        }
        set
        {
            if (GameData.Instance.gameStatus == null)
                return;
            else
                GameData.Instance.gameStatus.Language = value;
        }
    }

    public static Camera GetMainCamera()
    {
        GameObject objCamera = GameObject.Find("CameraEx");
        return objCamera == null ? null : objCamera.GetComponent<Camera>();
    }

    //是否是远程武器
    [DoNotToLua]
    public static bool IsSpecialWeapon(int itemIdx)
    {
        ItemBase it0 = GameData.Instance.FindItemByIdx(itemIdx);
        if (it0 == null)
            return false;
        if (it0.SubType == (int)EquipWeaponType.Gun || 
            it0.SubType == (int)EquipWeaponType.Guillotines || 
            it0.SubType == (int)EquipWeaponType.Dart)
        {
            return true;
        }
        return false;
    }

    public static void UnlockLevel()
    {
        Level[] level = LevelMng.Instance.GetAllItem();
        GameData.Instance.gameStatus.Level = level[level.Length - 1].ID;
    }

    
}
