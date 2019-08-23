using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using SLua;
using System.Collections.Generic;
using System;
using System.IO;

using UnityEngine.Profiling;
using protocol;
using System.Linq;
using System.Net;
using System.ComponentModel;

[CustomLuaClassAttribute]
public class U3D : MonoBehaviour {
    public static U3D Instance = null;
    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public static void ReloadTable()
    {
        TblCore.Instance.Reload();
        LevelMng.Instance.ReLoad();
        WeaponMng.Instance.ReLoad();
    }

    public static List<string> Nicks;
    public static string GetRandomName()
    {
        if (Nicks != null && Nicks.Count != 0)
        {
            int index2 = UnityEngine.Random.Range(0, Nicks.Count);
            string m = Nicks[index2];
            Nicks.RemoveAt(index2);
            return m;
        }
        TextAsset name = Resources.Load<TextAsset>("Name");
        string s = name.text.Replace("\r\n", ",");
        s = s.Replace(" ", ",");
        Nicks = s.Split(new char[] { ','}, StringSplitOptions.RemoveEmptyEntries).ToList();
        int index = UnityEngine.Random.Range(0, Nicks.Count);
        string n = Nicks[index];
        Nicks.RemoveAt(index);
        return n;
    }

    public static int GetPrevUnitId(int unit)
    {
        List<int> all = GetUnitList();
        int index = all.IndexOf(unit);
        index -= 1;
        index = Mathf.Clamp(index, 0, all.Count - 1);
        return all[index];
    }

    public static int GetNextUnitId(int unit)
    {
        List<int> all = GetUnitList();
        int index = all.IndexOf(unit);
        index += 1;
        index = Mathf.Clamp(index, 0, all.Count - 1);
        return all[index];
    }

    //取得某个种类的随机一把武器.
    public static int GetRandomWeaponType()
    {
        int i = UnityEngine.Random.Range((int)EquipWeaponType.Sword, 1 + (int)EquipWeaponType.NinjaSword);
        return i;
    }

    public static int GetNormalWeaponType()
    {
        List<int> s = new List<int>();
        for (int i = 0; i <= 11; i++)
        {
            s.Add(i);
        }
        //去掉远程武器
        s.Remove(2);
        s.Remove(3);
        s.Remove(6);
        int k = UnityEngine.Random.Range(0, s.Count);
        return s[k];
    }

    public static List<int> GetUnitList()
    {
        List<int> all = new List<int>();
        for (int i = 0; i < Global.MaxModel; i++)
        {
            all.Add(i);
        }

        if (GameData.Instance.gameStatus != null)
        {
            for (int i = 0; i < GameData.Instance.gameStatus.pluginModel.Count; i++)
            {
                GameData.Instance.gameStatus.pluginModel[i].Check();
                if (!GameData.Instance.gameStatus.pluginModel[i].Installed)
                    continue;
                all.Add(GameData.Instance.gameStatus.pluginModel[i].ModelId);
            }
        }
        return all;
    }

    //取得随机英雄ID
    public static int GetRandomUnitIdx()
    {
        List<int> all = GetUnitList();
        return all[UnityEngine.Random.Range(0, all.Count)];
    }

    public static EUnitCamp GetAnotherCamp(EUnitCamp camp)
    {
        if (camp == EUnitCamp.EUC_ENEMY)
            return EUnitCamp.EUC_FRIEND;
        if (camp == EUnitCamp.EUC_FRIEND)
            return EUnitCamp.EUC_ENEMY;
        return EUnitCamp.EUC_KILLALL;
    }

    //通过EquipWeaponCode,得到指定类型的武器.用于在联机界面的武器选择时.
    public static int GetWeaponByCode(int i)
    {
        if (i < 0 || i >= weaponCode.Length)
            return 5;//默认是匕首
        return weaponCode[i];
    }

    //sort -x->+x标识当前武器得前/后多少个武器
    public static int GetWeaponBySort(int weapon, int sort)
    {
        List<ItemBase> we = GameData.Instance.itemMng.GetFullRow();
        for (int i = 0; i < we.Count; i++)
        {
            if (we[i].MainType == 1)
            {
                //武器
                if (we[i].Idx == weapon)
                {
                    if (i + sort < we.Count)
                    {
                        if (we[i + sort].MainType == 1)
                            return we[i + sort].Idx;
                        else
                            return we[0].Idx;
                    }
                    else
                        return we[0].Idx;
                }
            }
        }
        return 1;
    }

    //通过EquipWeaponType,得到指定类型的武器.用于在
    public static int GetWeaponByType(int weaponIndex)
    {
        List<int> weaponList = null;
        if (weaponDict.ContainsKey(weaponIndex))
            weaponList = weaponDict[weaponIndex];
        else
            weaponList = new List<int>();
        if (weaponList.Count == 0)
        {
            List<ItemBase> we = GameData.Instance.itemMng.GetFullRow();
            for (int i = 0; i < we.Count; i++)
            {
                if (we[i].MainType == 1)
                {
                    if (we[i].SubType == (int)weaponIndex)
                    {
                        weaponList.Add(we[i].Idx);
                    }
                }
            }
            if (!weaponDict.ContainsKey(weaponIndex))
                weaponDict.Add(weaponIndex, weaponList);
        }
        int k = Rand(weaponList.Count);
        return weaponList[k];
    }

    //七星，旋风，怒火，峨眉，蛇吻，碧血剑，战戟,斩铁,流星,乾坤刀,指虎,忍刀
    static int[] weaponCode = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 47, 51, 55 };

    static Dictionary<int, List<int>> weaponDict = new Dictionary<int, List<int>>();
    public static void SpawnRobot(int idx, EUnitCamp camp, int weaponIndex = 0, int hpMax = 1000)
    {
        if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
        {
            U3D.PopupTip("联机无法添加机器人");
            return;
        }

        List<int> weaponList = null;
        if (weaponDict.ContainsKey(weaponIndex))
            weaponList = weaponDict[weaponIndex];
        else
            weaponList = new List<int>();
        if (weaponList.Count == 0)
        {
            List<ItemBase> we = GameData.Instance.itemMng.GetFullRow();
            for (int i = 0; i < we.Count; i++)
            {
                if (we[i].MainType == 1)
                {
                    if (we[i].SubType == (int)weaponIndex)
                    {
                        weaponList.Add(we[i].Idx);
                    }
                }
            }
            if (!weaponDict.ContainsKey(weaponIndex))
                weaponDict.Add(weaponIndex, weaponList);
        }

        MonsterEx mon = new MonsterEx(hpMax * 10);
        int k = Rand(weaponList.Count);
        mon.Weapon = weaponList[k];
        mon.Weapon2 = weaponList[(k + 1) % weaponList.Count];
        mon.IsPlayer = false;

        mon.name = U3D.GetRandomName();
        GameObject objPrefab = Resources.Load("MeteorUnit") as GameObject;
        GameObject ins = GameObject.Instantiate(objPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        MeteorUnit unit = ins.GetComponent<MeteorUnit>();
        unit.Camp = camp;
        unit.Init(idx, mon);
        MeteorManager.Instance.OnGenerateUnit(unit);
        unit.SetGround(false);

        if (camp == EUnitCamp.EUC_FRIEND)
        {
            unit.transform.position = Global.Instance.GCampASpawn[Global.Instance.CampASpawnIndex];
            Global.Instance.CampASpawnIndex++;
            Global.Instance.CampASpawnIndex %= 8;
            
        }
        else if (camp == EUnitCamp.EUC_ENEMY)
        {
            unit.transform.position = Global.Instance.GCampBSpawn[Global.Instance.CampBSpawnIndex];
            Global.Instance.CampBSpawnIndex++;
            Global.Instance.CampBSpawnIndex %= 8;
        }
        else
        {
            //16个点
            unit.transform.position = Global.Instance.GLevelSpawn[Global.Instance.SpawnIndex];
            Global.Instance.SpawnIndex++;
            Global.Instance.SpawnIndex %= 16;
        }
        
        //InsertSystemMsg(U3D.GetCampEnterLevelStr(unit));
        //找寻敌人攻击.因为这个并没有脚本模板
        unit.Robot.ChangeState(EAIStatus.Wait);

        unit.Attr.GetItem = 0;
        unit.Attr.View = 5000;//视野给大一点
        if (Global.Instance.GGameMode == GameMode.MENGZHU)
        {
            
        }
        else if (Global.Instance.GGameMode == GameMode.ANSHA)
        {
            if (unit.IsLeader)
                U3D.ChangeBehaviorEx(unit.InstanceId, "follow", new object[] { "enemyvip" });
            else
                U3D.ChangeBehaviorEx(unit.InstanceId, "follow", new object[] { "vip" });
        }
        else if (Global.Instance.GGameMode == GameMode.SIDOU)
        {
            if (unit.IsLeader)
                U3D.ChangeBehaviorEx(unit.InstanceId, "follow", new object[] { "enemyvip" });
            else
                U3D.ChangeBehaviorEx(unit.InstanceId, "follow", new object[] { "vip" });
        }
        return;
    }

    public static void ChangePlayerModel(int model)
    {
        Global.Instance.PauseAll = true;
        MeteorManager.Instance.LocalPlayer.controller.InputLocked = true;
        MeteorManager.Instance.LocalPlayer.Init(model, MeteorManager.Instance.LocalPlayer.Attr, true);
        Global.Instance.PauseAll = false;
        MeteorManager.Instance.LocalPlayer.controller.InputLocked = false;
    }

    public static MeteorUnit InitNetPlayer(PlayerEventData player)
    {
        MonsterEx mon = SceneMng.Instance.InitNetPlayer(player);
        GameObject objPrefab = Resources.Load("MeteorUnit") as GameObject;
        
        GameObject ins = GameObject.Instantiate(objPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        MeteorUnit unit = ins.GetComponent<MeteorUnit>();
        if (mon.IsPlayer)
            MeteorManager.Instance.LocalPlayer = unit;
        unit.Camp = (EUnitCamp)player.camp; 
        unit.Init(mon.Model, mon);
        MeteorManager.Instance.OnGenerateUnit(unit, (int)player.playerId);
        unit.SetGround(false);
        if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
        {
            if (Global.Instance.GGameMode == GameMode.Normal)
            {
                unit.transform.position = Global.Instance.GLevelItem.wayPoint.Count > mon.SpawnPoint ? Global.Instance.GLevelItem.wayPoint[mon.SpawnPoint].pos : Global.Instance.GLevelItem.wayPoint[0].pos;//等关卡脚本实现之后在设置单机出生点.PlayerEx.Instance.SpawnPoint
                unit.transform.eulerAngles = new Vector3(0, mon.SpawnDir, 0);
            }
            else if (Global.Instance.GGameMode == GameMode.MENGZHU)
            {
                //16个点
                unit.transform.position = Global.Instance.GLevelSpawn[mon.SpawnPoint];
                unit.transform.eulerAngles = new Vector3(0, mon.SpawnDir, 0);
            }
            else if (Global.Instance.GGameMode == GameMode.ANSHA || Global.Instance.GGameMode == GameMode.SIDOU)
            {
                //2个队伍8个点.必须带阵营.
                if (unit.Camp == EUnitCamp.EUC_FRIEND)
                {
                    unit.transform.position = Global.Instance.GCampASpawn[mon.SpawnPoint];
                }
                else if (unit.Camp == EUnitCamp.EUC_ENEMY)
                {
                    unit.transform.position = Global.Instance.GCampASpawn[mon.SpawnPoint];
                }
            }
        }
        else
        {
            //Vector3 spawnPos;
            //Quaternion quat;
            //unit.transform.position = spawnPos;
            //unit.transform.rotation = quat;
        }
        return unit;
    }

    public static MeteorUnit InitPlayer(LevelScriptBase script)
    {
        MonsterEx mon = SceneMng.Instance.InitPlayer(script);
        GameObject objPrefab = Resources.Load("MeteorUnit") as GameObject;
        GameObject ins = GameObject.Instantiate(objPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        MeteorUnit unit = ins.GetComponent<MeteorUnit>();
        MeteorManager.Instance.LocalPlayer = unit;
        unit.Camp = EUnitCamp.EUC_FRIEND;//流星阵营
        unit.Init(mon.Model, mon);
        MeteorManager.Instance.OnGenerateUnit(unit);
        unit.SetGround(false);
        if (Global.Instance.GLevelMode <= LevelMode.SinglePlayerTask)
        {
            if (Global.Instance.GScript.DisableFindWay())
            {
                //不许寻路，无寻路点的关卡，使用
                bool setPosition = false;
                if (script != null)
                    setPosition = script.OnPlayerSpawn(unit);
                if (!setPosition)
                {
                    unit.transform.position = Global.Instance.GLevelSpawn[mon.SpawnPoint >= Global.Instance.GLevelSpawn.Length ? 0 : mon.SpawnPoint];
                }
            }
            else
            {
                if (Global.Instance.GLevelItem.wayPoint.Count == 0)
                {
                    unit.transform.position = Global.Instance.GLevelSpawn[mon.SpawnPoint];
                }
                else
                    unit.transform.position = Global.Instance.GLevelItem.wayPoint.Count > mon.SpawnPoint ? Global.Instance.GLevelItem.wayPoint[mon.SpawnPoint].pos : Global.Instance.GLevelItem.wayPoint[0].pos;//等关卡脚本实现之后在设置单机出生点.PlayerEx.Instance.SpawnPoint
            }
        }
        else if (Global.Instance.GLevelMode > LevelMode.SinglePlayerTask && Global.Instance.GLevelMode <= LevelMode.MultiplyPlayer)
        {
            if (Global.Instance.GGameMode == GameMode.Normal)
            {
                if (Global.Instance.GScript.DisableFindWay())
                {
                    //不许寻路，无寻路点的关卡，使用
                    unit.transform.position = Global.Instance.GLevelSpawn[mon.SpawnPoint >= Global.Instance.GLevelSpawn.Length ? 0 : mon.SpawnPoint];
                }
                else
                {
                    unit.transform.position = Global.Instance.GLevelItem.wayPoint.Count > mon.SpawnPoint ? Global.Instance.GLevelItem.wayPoint[mon.SpawnPoint].pos : Global.Instance.GLevelItem.wayPoint[0].pos;//等关卡脚本实现之后在设置单机出生点.PlayerEx.Instance.SpawnPoint
                }
            }
            else if (Global.Instance.GGameMode == GameMode.MENGZHU)
            {
                //16个点
                unit.transform.position = Global.Instance.GLevelSpawn[Global.Instance.SpawnIndex];
                Global.Instance.SpawnIndex++;
                Global.Instance.SpawnIndex %= 16;
                unit.transform.eulerAngles = new Vector3(0, mon.SpawnDir, 0);
            }
            else if (Global.Instance.GGameMode == GameMode.ANSHA || Global.Instance.GGameMode == GameMode.SIDOU)
            {
                //2个队伍8个点.
                if (unit.Camp == EUnitCamp.EUC_FRIEND)
                {
                    unit.transform.position = Global.Instance.GCampASpawn[Global.Instance.CampASpawnIndex];
                    Global.Instance.CampASpawnIndex++;
                    Global.Instance.CampASpawnIndex %= 8;
                }
                else if (unit.Camp == EUnitCamp.EUC_ENEMY)
                {
                    unit.transform.position = Global.Instance.GCampASpawn[Global.Instance.CampBSpawnIndex];
                    Global.Instance.CampBSpawnIndex++;
                    Global.Instance.CampBSpawnIndex %= 8;
                }
            }
        }
        unit.transform.eulerAngles = new Vector3(0, mon.SpawnDir, 0);
        U3D.InsertSystemMsg(U3D.GetCampEnterLevelStr(unit));
        return unit;
    }

    public static string GetDefaultFile(string Path, int type, bool local, bool inzipPath)
    {
        string suffix = "";
        switch (type)
        {
            case 0:
                suffix = ".jpg";
                break;
            case 1:
                suffix = ".dll";
                break;
            case 2:
                suffix = ".txt";
                break;
        }

        string shortDir = "";
        int k = Path.LastIndexOf('/');
        shortDir = Path.Substring(0, k);
        string shortName = Path.Substring(k + 1);
        shortName = shortName.Substring(0, shortName.Length - 4);
        if (local)
            return ResMng.localPath + "/Plugins/" + shortDir + "/" + (inzipPath ? shortName + "/" + shortName: shortName) + suffix;//.zip => .png
        return Path.Substring(0, Path.Length - 4) + suffix;
    }

    public static int AddNPC(string script)
    {
        MeteorUnit target = SceneMng.Instance.Spawn(script);
        return target.InstanceId;
    }

    public static string GetCampStr(EUnitCamp Camp)
    {
        if (Camp == EUnitCamp.EUC_ENEMY)
            return string.Format("{0}","蝴蝶");
        if (Camp == EUnitCamp.EUC_FRIEND)
            return string.Format("{0}", "流星");
        return string.Format("{0}", "无");
    }

    public static string GetCampEnterLevelStr(MeteorUnit unit)
    {
        if (unit.Camp == EUnitCamp.EUC_ENEMY)
            return string.Format("{0} 选择蝴蝶, 进入战场", unit.name);
        if (unit.Camp == EUnitCamp.EUC_FRIEND)
            return string.Format("{0} 选择流星,进入战场", unit.name);
        return string.Format("{0} 进入战场", unit.name);
    }

    public static void InsertSystemMsg(string msg)
    {
        //if (!GameOverlayWnd.Exist)
        //    GameOverlayWnd.Instance.Open();
        //GameOverlayWnd.Instance.InsertSystemMsg(msg);
    }

    //弹出一个简单提示
    public static void PopupTip(string str)
    {
        PopupTipState Tips = new PopupTipState(Main.Instance.PopupStateManager);
        Main.Instance.PopupStateManager.AutoPopup(Tips, str);
    }

    static UnityEngine.AsyncOperation backOp;
    static UnityEngine.AsyncOperation loadMainOp;
    static Coroutine loadMain;
    //返回到主目录
    public static void GoBack(Action t = null)
    {
        Global.Instance.GLevelItem = null;
        if (loadMain != null)
        {
            Instance.StopCoroutine(loadMain);
            loadMain = null;
        }
        loadMain = Instance.StartCoroutine(Instance.LoadMainWnd(t));
    }

    //修改版本号后回到Startup重新加载资源
    public static void ReStart()
    {
        FrameReplay.Instance.OnDisconnected();
        Global.Instance.GLevelItem = null;
        if (loadMain != null)
        {
            Instance.StopCoroutine(loadMain);
            loadMain = null;
        }
        loadMain = Instance.StartCoroutine(Instance.LoadStartup());
    }

    IEnumerator LoadStartup()
    {
        ResMng.LoadScene("Startup");
        backOp = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Startup", UnityEngine.SceneManagement.LoadSceneMode.Single);//.LoadSceneAsync_s (1);
        yield return backOp;
    }

    IEnumerator LoadMainWnd(Action t)
    {
        ResMng.LoadScene("Menu");
        loadMainOp = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Menu", UnityEngine.SceneManagement.LoadSceneMode.Single);//.LoadSceneAsync_s (1);
        yield return loadMainOp;
        OnLoadMainFinished(t);
    }

    void OnLoadMainFinished(Action t)
    {
        //MainWnd.Instance.Open();
        GameData.Instance.SaveState();
        if (t != null)
            t.Invoke();
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

    public static void OpenRobotWnd()
    {
        //if (RobotWnd.Exist)
        //    RobotWnd.Instance.Close();
        //else
        //    RobotWnd.Instance.Open();
    }

    public static void OpenSfxWnd()
    {
        //if (SfxWnd.Exist)
        //    SfxWnd.Instance.Close();
        //else
        //    SfxWnd.Instance.Open();
    }

    //打开武器界面，主角色调试切换主手武器.
    public static void OpenWeaponWnd()
    {
        //if (WeaponWnd.Exist)
        //    WeaponWnd.Instance.Close();
        //else
        //    WeaponWnd.Instance.Open();
    }

    public static void OpenSystemWnd()
    {
        if (Global.Instance.GLevelItem != null)
        {
            //EscWnd.Instance.Open();
            if (NGUIJoystick.instance != null)
                NGUIJoystick.instance.Lock(true);
            return;
        }
    }

    public static void PlayBtnAudio(string audio = "btn")
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound(audio);
    }

    public static void LoadNetLevel()
    {
        NetWorkBattle.Instance.Load();
    }

    public static void LoadScene(string scene, Action OnFinished)
    {
        LevelHelper helper = Instance.gameObject.AddComponent<LevelHelper>();
        helper.LoadScene(scene, OnFinished);
    }

    //加载当前设置的关卡.在打开模组剧本时/联机时
    public static void LoadLevelEx()
    {
        SoundManager.Instance.StopAll();
        SoundManager.Instance.Enable(false);
        ClearLevelData();
        Main.Instance.DialogStateManager.ChangeState(Main.Instance.DialogStateManager.LoadingDialogState);
        //LoadingWnd.Instance.Open();
        Resources.UnloadUnusedAssets();
        GC.Collect();
        LevelHelper helper = Instance.gameObject.AddComponent<LevelHelper>();
        helper.Load();
        Log.Write("helper.load end");
    }

    //走参数指定关卡.
    public static void LoadLevel(int id, LevelMode levelmode, GameMode gamemode)
    {
        Main.Instance.DialogStateManager.ChangeState(Main.Instance.DialogStateManager.LoadingDialogState);
        //暂时不允许使用声音管理器，在切换场景时不允许播放
        SoundManager.Instance.StopAll();
        SoundManager.Instance.Enable(false);
        ClearLevelData();
        Level lev = LevelMng.Instance.GetItem(id);
        Global.Instance.GLevelItem = lev;
        Global.Instance.GLevelMode = levelmode;
        Global.Instance.GGameMode = gamemode;
        Global.Instance.Chapter = null;
        //LoadingWnd.Instance.Open();
        Resources.UnloadUnusedAssets();
        GC.Collect();
        if (!string.IsNullOrEmpty(lev.sceneItems) && !GameData.Instance.gameStatus.SkipVideo && levelmode == LevelMode.SinglePlayerTask && Global.Instance.Chapter == null)
        {
            string num = lev.sceneItems.Substring(2);
            int number = 0;
            if (int.TryParse(num, out number))
            {
                if (id >= 0 && id <= 9)
                {
                    string movie = string.Format(Main.strSFile, Main.strHost, Main.port, Main.strProjectUrl, "mmv/" + "b" + number + ".mv");
                    U3D.PlayMovie(movie);
                }
            }
        }
        LevelHelper helper = Instance.gameObject.AddComponent<LevelHelper>();
        helper.Load();
    }

    public static void ClearLevelData()
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
        Global.Instance.CampASpawnIndex = 0;
        Global.Instance.CampBSpawnIndex = 0;
        Global.Instance.SpawnIndex = 0;
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

    public static void CreateEffect(int id, string effect, bool loop = false)
    {
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
            SFXLoader.Instance.PlayEffect(effect, objEffect.gameObject, loop);
    }

    public static MeteorUnit GetTeamLeader(EUnitCamp camp)
    {
        for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
        {
            if (MeteorManager.Instance.UnitInfos[i].Camp == camp)
            {
                if (MeteorManager.Instance.UnitInfos[i].IsLeader)
                    return MeteorManager.Instance.UnitInfos[i];
            }
        }
        return null;
    }

    //取得敌对方首领.
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
                //object target = null;
                if (value[1].GetType() == typeof(int))
                {
                    GameBattleEx.Instance.PushActionFollow(id, (int)value[1]);
                }
                else
                {
                    //target = value[1] as string;
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
                //不再实现，类似AI意义不大，四处跑，即寻找一个临近路点，跑过去.
            }
            else if (act == "dodge")
            {
                //不再实现，类似AI意义不大，寻找一个目标临近路点，跑过去.
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

        for (int i = 0; i < MeteorManager.Instance.DeadUnits.Count; i++)
        {
            if (MeteorManager.Instance.DeadUnits[i].name == player)
                return MeteorManager.Instance.DeadUnits[i].InstanceId;
        }

        foreach (var each in MeteorManager.Instance.LeavedUnits)
        {
            if (each.Value == player)
                return each.Key;
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
        for (int i = 0; i < MeteorManager.Instance.DeadUnits.Count; i++)
        {
            if (MeteorManager.Instance.DeadUnits[i].InstanceId == id)
                return MeteorManager.Instance.DeadUnits[i].Attr.hpCur;
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
        for (int i = 0; i < MeteorManager.Instance.DeadUnits.Count; i++)
        {
            if (MeteorManager.Instance.DeadUnits[i].InstanceId == id)
                return MeteorManager.Instance.DeadUnits[i].Attr.HpMax;
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

    //场景发生破坏后，对路点的影响.
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
            GameBattleEx.Instance.PushActionUse(0, param);
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

    // pose="say", "pause", "use", "block"
    public static int StopPerform(int id)
    {
        MeteorUnit u = GetUnit(id);
        //停止动作的时候，状态为等待，一般遇见敌人时会调用此接口
        if (u.Robot != null)
            u.Robot.ChangeState(EAIStatus.Wait);
        if (u != null)
        {
            //先清除该角色的聊天动作，其他动画动作不处理。
            GameBattleEx.Instance.StopAction(id);
        }
        return 0;
    }

    // type="char", "waypoint", "flag", "safe"
    //获得2个角色的距离.
    public static float Distance(int idx1, int idx2)
    {
        if (Global.Instance.GLevelItem != null && Global.Instance.GLevelMode <= LevelMode.SinglePlayerTask)
        {
            MeteorUnit a = GetUnit(idx1);
            MeteorUnit b = GetUnit(idx2);
            if (a == null || b == null)
                return 0;
            return Vector3.Distance(a.transform.position, b.transform.position);
        }
        return 0;
    }

    public static int Rand(int n)
    {
        return UnityEngine.Random.Range(0, n);
    }

    //单机下
    public static void RotatePlayer(string name, float yRotate)
    {
        if (Global.Instance.GLevelItem != null && Global.Instance.GLevelMode <= LevelMode.SinglePlayerTask)
        {
            int id = GetChar(name);
            if (id != -1)
            {
                MeteorUnit unit = GetUnit(id);
                if (unit != null)
                    unit.transform.rotation = Quaternion.Euler(0, yRotate, 0);
            }
        }
    }

    public static void MovePlayer(string name, Vector3 position)
    {
        if (Global.Instance.GLevelItem != null && Global.Instance.GLevelMode <= LevelMode.SinglePlayerTask)
        {
            int id = GetChar(name);
            if (id != -1)
            {
                MeteorUnit unit = GetUnit(id);
                if (unit != null)
                    unit.SetPosition(position);
            }
        }
    }

    public static void MoveNpc(string name, int spawnPoint)
    {
        if (Global.Instance.GLevelItem != null && Global.Instance.GLevelMode <= LevelMode.SinglePlayerTask)
        {
            int id = GetChar(name);
            if (id != -1)
            {
                MeteorUnit unit = GetUnit(id);
                if (unit != null)
                    unit.SetPosition(spawnPoint);
            }
        }
    }

    public static void RemoveNPC(int id)
    {
        MeteorUnit unit = GetUnit(id);
        bool dead = unit.Dead;
        string message = unit.name + " 离开战场";
        MeteorManager.Instance.OnRemoveUnit(unit);
        if (!dead)
            InsertSystemMsg(message);
    }

    public static void UpdateAIAttrib(int id)
    {
        MeteorUnit monster = GetUnit(id);
        monster.Attr.UpdateAttr();
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
        return null;
    }

    public static void Say(int id, string param)
    {
        if (GameBattleEx.Instance != null)
            GameBattleEx.Instance.PushActionSay(id, param);//1=say 2=pause
    }

    static WebClient downloadMovie;
    public static List<string> WaitDownload = new List<string>();
    public static void PlayMovie(string movie)
    {
        if (GameData.Instance.gameStatus.LocalMovie.ContainsKey(movie))
        {
            Debug.Log("PlayMovie:" + movie);
            if (downloadMovie == null)
            {
                if (WaitDownload.Count != 0)
                {
                    downloadMovie = new WebClient();
                    string download = WaitDownload[0];
                    int index = download.LastIndexOf("/");
                    string file = movie.Substring(index + 1);
                    string local = Application.persistentDataPath + "/mmv/" + file;//start.mv
                    local = local.Replace(".mv", ".mp4");
                    Debug.Log("file:" + file);
                    downloadMovie.DownloadFileCompleted += (object sender, AsyncCompletedEventArgs e) => {
                        if (e.Error != null)
                        {
                            GameData.Instance.gameStatus.LocalMovie.Add(download, local);
                            WaitDownload.Remove(download);
                        }
                        downloadMovie.CancelAsync();
                        downloadMovie.Dispose();
                        downloadMovie = null;
                    };
                    downloadMovie.DownloadFileAsync(new Uri(download), local);
                    Debug.Log("downloadMovieQueue:" + download + " to" + local);
                }
            }
            Handheld.PlayFullScreenMovie(GameData.Instance.gameStatus.LocalMovie[movie], Color.black, FullScreenMovieControlMode.CancelOnInput, FullScreenMovieScalingMode.AspectFit);
        }
        else
        {
            if (GameData.Instance.gameStatus.SkipVideo)
                return;
            if (!Directory.Exists(Application.persistentDataPath + "/mmv"))
                Directory.CreateDirectory(Application.persistentDataPath + "/mmv");
            if (downloadMovie != null)
            {
                //还在下载其他
                if (!WaitDownload.Contains(movie))
                    WaitDownload.Add(movie);
                return;
            }
            downloadMovie = new WebClient();
            int index = movie.LastIndexOf("/");
            string file = movie.Substring(index + 1);
            string local = Application.persistentDataPath + "/mmv/" + file;
            local = local.Replace(".mv", ".mp4");
            Debug.Log("file:" + file);
            downloadMovie.DownloadFileCompleted += (object sender, AsyncCompletedEventArgs e) => {
                if (e.Error != null)
                    GameData.Instance.gameStatus.LocalMovie.Add(movie, local);
                downloadMovie.CancelAsync();
                downloadMovie.Dispose();
                downloadMovie = null;
            };
            downloadMovie.DownloadFileAsync(new Uri(movie), local);
            Debug.Log("downloadMovie:" + movie + " to" + local);
        }
    }

    public static int GetEnemyCount()
    {
        int total = 0;
        for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
        {
            if (MeteorManager.Instance.UnitInfos[i].Camp != EUnitCamp.EUC_Meteor)
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
            //其他物品，暂时没有其他道具系统.
        }
    }

    public static Camera GetMainCamera()
    {
        GameObject objCamera = GameObject.Find("CameraEx");
        return objCamera == null ? null : objCamera.GetComponent<Camera>();
    }

    public static bool IsSpecialWeapon(EquipWeaponType t)
    {
        return t == EquipWeaponType.Gun || t == EquipWeaponType.Guillotines || t == EquipWeaponType.Dart;
    }

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

    public static int GetMaxLevel()
    {
        //内置关卡的最后一个关卡.
        Level[] level = LevelMng.Instance.GetAllItem();
        if (level == null)
            return 0;
        int max = 0;
        for (int i = 0; i < level.Length; i++)
        {
            if (level[i].ID > max)
                max = level[i].ID;
        }
        return max;
    }

    public static void UnlockLevel()
    {
        GameData.Instance.gameStatus.Level = Global.Instance.LEVELMAX;
    }

    public static bool IsUnitDead(int instanceid)
    {
        MeteorUnit u = GetUnit(instanceid);
        if (u)
            return u.Dead;
        return true;
    }

    public static bool AllFriendDead()
    {
        bool alldead = true;
        for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
        {
            if (MeteorManager.Instance.UnitInfos[i].SameCamp(MeteorManager.Instance.LocalPlayer))
            {
                if (!MeteorManager.Instance.UnitInfos[i].Dead)
                {
                    alldead = false;
                    break;
                }
            }
        }
        return alldead;
    }

    public static bool AllEnemyDead()
    {
        bool alldead = true;
        for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
        {
            if (!MeteorManager.Instance.UnitInfos[i].SameCamp(MeteorManager.Instance.LocalPlayer))
            {
                if (!MeteorManager.Instance.UnitInfos[i].Dead)
                {
                    alldead = false;
                    break;
                }
            }
        }
        return alldead;
    }

    public static void OnPauseAI()
    {
        for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
        {
            MeteorManager.Instance.UnitInfos[i].EnableAI(false);
        }
    }

    public static void OnResumeAI()
    {
        for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
        {
            MeteorManager.Instance.UnitInfos[i].EnableAI(true);
        }
    }

    //以下为脚本系统执行面板里能响应的操作
    public static void GodLike()
    {
        GameData.Instance.gameStatus.GodLike = !GameData.Instance.gameStatus.GodLike;
        U3D.InsertSystemMsg(GameData.Instance.gameStatus.GodLike ? "作弊[开]" : "作弊[关]");
        //if (EscWnd.Exist)
        //    EscWnd.Instance.Close();
        //EscWnd.Instance.Open();
    }

    public static WeaponBase GetWeaponProperty(int weaponIdx)
    {
        WeaponBase w = WeaponMng.Instance.GetItem(weaponIdx);
        if (w == null)
            w = PluginWeaponMng.Instance.GetItem(weaponIdx);
        if (w == null)
            Debug.LogError("can not find weapon:" + weaponIdx);
        return w;
    }

    public static void DeletePlugins()
    {
        GameData.Instance.gameStatus.pluginChapter.Clear();
        GameData.Instance.gameStatus.pluginModel.Clear();
        GameData.Instance.gameStatus.pluginNpc.Clear();
        Global.Instance.PluginUpdated = false;
        GameData.Instance.SaveState();
    }

    /// <summary>
    /// 加载路径内的指令文件-供播放
    /// </summary>
    public static byte[] LoadReplayData()
    {
        return null;
    }

    public static void ShowLeaderSfx()
    {
        MeteorUnit uEnemy = U3D.GetTeamLeader(EUnitCamp.EUC_ENEMY);
        MeteorUnit uPlayer = U3D.GetUnit(0);
        SFXLoader.Instance.PlayEffect("vipblue.ef", uPlayer.gameObject, false);
        if (uEnemy != null)
            SFXLoader.Instance.PlayEffect("vipred.ef", uEnemy.gameObject, false);
    }
}
