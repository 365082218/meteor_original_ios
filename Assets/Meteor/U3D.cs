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
#if !STRIP_DBG_SETTING
        InitDebugSetting();
#endif
    }

    public static void ReloadTable()
    {
        //重新加载表格
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
        for (int i = 0; i < Main.Ins.CombatData.MaxModel; i++)
        {
            all.Add(i);
        }

        if (Main.Ins.GameStateMgr.gameStatus != null)
        {
            for (int i = 0; i < Main.Ins.GameStateMgr.gameStatus.pluginModel.Count; i++)
            {
                Main.Ins.GameStateMgr.gameStatus.pluginModel[i].Check();
                if (!Main.Ins.GameStateMgr.gameStatus.pluginModel[i].Installed)
                    continue;
                all.Add(Main.Ins.GameStateMgr.gameStatus.pluginModel[i].ModelId);
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
        List<ItemDatas.ItemDatas> we = Main.Ins.DataMgr.GetDatasArray<ItemDatas.ItemDatas>();
        for (int i = 0; i < we.Count; i++)
        {
            if (we[i].MainType == 1)
            {
                //武器
                if (we[i].ID == weapon)
                {
                    if (i + sort < we.Count)
                    {
                        if (we[i + sort].MainType == 1)
                            return we[i + sort].ID;
                        else
                            return we[0].ID;
                    }
                    else
                        return we[0].ID;
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
            List<ItemDatas.ItemDatas> we = Main.Ins.DataMgr.GetDatasArray<ItemDatas.ItemDatas>();
            for (int i = 0; i < we.Count; i++)
            {
                if (we[i].MainType == 1)
                {
                    if (we[i].SubType == (int)weaponIndex)
                    {
                        weaponList.Add(we[i].ID);
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
        if (Main.Ins.CombatData.GLevelMode == LevelMode.MultiplyPlayer)
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
            List<ItemDatas.ItemDatas> we = Main.Ins.DataMgr.GetDatasArray<ItemDatas.ItemDatas>();
            for (int i = 0; i < we.Count; i++)
            {
                if (we[i].MainType == 1)
                {
                    if (we[i].SubType == (int)weaponIndex)
                    {
                        weaponList.Add(we[i].ID);
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
        Main.Ins.MeteorManager.OnGenerateUnit(unit);
        unit.SetGround(false);

        if (camp == EUnitCamp.EUC_FRIEND)
        {
            unit.transform.position = Main.Ins.CombatData.GCampASpawn[Main.Ins.CombatData.CampASpawnIndex];
            Main.Ins.CombatData.CampASpawnIndex++;
            Main.Ins.CombatData.CampASpawnIndex %= 8;
        }
        else if (camp == EUnitCamp.EUC_ENEMY)
        {
            unit.transform.position = Main.Ins.CombatData.GCampBSpawn[Main.Ins.CombatData.CampBSpawnIndex];
            Main.Ins.CombatData.CampBSpawnIndex++;
            Main.Ins.CombatData.CampBSpawnIndex %= 8;
        }
        else
        {
            //16个点
            unit.transform.position = Main.Ins.CombatData.GLevelSpawn[Main.Ins.CombatData.SpawnIndex];
            Main.Ins.CombatData.SpawnIndex++;
            Main.Ins.CombatData.SpawnIndex %= 16;
        }
        
        InsertSystemMsg(U3D.GetCampEnterLevelStr(unit));
        //找寻敌人攻击.因为这个并没有脚本模板
        unit.StateMachine.ChangeState(unit.StateMachine.IdleState);

        unit.Attr.GetItem = 0;
        unit.Attr.View = 5000;//视野给大一点
        if (Main.Ins.CombatData.GGameMode == GameMode.MENGZHU)
        {
            
        }
        else if (Main.Ins.CombatData.GGameMode == GameMode.ANSHA)
        {
            if (unit.IsLeader)
                U3D.ChangeBehaviorEx(unit.InstanceId, "follow", new object[] { "enemyvip" });
            else
                U3D.ChangeBehaviorEx(unit.InstanceId, "follow", new object[] { "vip" });
        }
        else if (Main.Ins.CombatData.GGameMode == GameMode.SIDOU)
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
        Main.Ins.CombatData.PauseAll = true;
        Main.Ins.LocalPlayer.controller.InputLocked = true;
        Main.Ins.LocalPlayer.Init(model, Main.Ins.LocalPlayer.Attr, true);
        Main.Ins.CombatData.PauseAll = false;
        Main.Ins.LocalPlayer.controller.InputLocked = false;
    }

    public static MeteorUnit InitNetPlayer(PlayerEventData player)
    {
        MonsterEx mon = Main.Ins.SceneMng.InitNetPlayer(player);
        GameObject objPrefab = Resources.Load("MeteorUnit") as GameObject;
        
        GameObject ins = GameObject.Instantiate(objPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        MeteorUnit unit = ins.GetComponent<MeteorUnit>();
        if (mon.IsPlayer)
            Main.Ins.LocalPlayer = unit;
        unit.Camp = (EUnitCamp)player.camp; 
        unit.Init(mon.Model, mon);
        Main.Ins.MeteorManager.OnGenerateUnit(unit, (int)player.playerId);
        unit.SetGround(false);
        if (Main.Ins.CombatData.GLevelMode == LevelMode.MultiplyPlayer)
        {
            if (Main.Ins.CombatData.GGameMode == GameMode.Normal)
            {
                unit.transform.position = Main.Ins.CombatData.wayPoints.Count > mon.SpawnPoint ? Main.Ins.CombatData.wayPoints[mon.SpawnPoint].pos : Main.Ins.CombatData.wayPoints[0].pos;//等关卡脚本实现之后在设置单机出生点.PlayerEx.Instance.SpawnPoint
                unit.transform.eulerAngles = new Vector3(0, mon.SpawnDir, 0);
            }
            else if (Main.Ins.CombatData.GGameMode == GameMode.MENGZHU)
            {
                //16个点
                unit.transform.position = Main.Ins.CombatData.GLevelSpawn[mon.SpawnPoint];
                unit.transform.eulerAngles = new Vector3(0, mon.SpawnDir, 0);
            }
            else if (Main.Ins.CombatData.GGameMode == GameMode.ANSHA || Main.Ins.CombatData.GGameMode == GameMode.SIDOU)
            {
                //2个队伍8个点.必须带阵营.
                if (unit.Camp == EUnitCamp.EUC_FRIEND)
                {
                    unit.transform.position = Main.Ins.CombatData.GCampASpawn[mon.SpawnPoint];
                }
                else if (unit.Camp == EUnitCamp.EUC_ENEMY)
                {
                    unit.transform.position = Main.Ins.CombatData.GCampASpawn[mon.SpawnPoint];
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
        MonsterEx mon = Main.Ins.SceneMng.InitPlayer(script);
        GameObject objPrefab = Resources.Load("MeteorUnit") as GameObject;
        GameObject ins = GameObject.Instantiate(objPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        MeteorUnit unit = ins.GetComponent<MeteorUnit>();
        Main.Ins.LocalPlayer = unit;
        unit.Camp = EUnitCamp.EUC_FRIEND;//流星阵营
        unit.Init(mon.Model, mon);
        Main.Ins.MeteorManager.OnGenerateUnit(unit);
        unit.SetGround(false);
        if (Main.Ins.CombatData.GLevelMode <= LevelMode.SinglePlayerTask)
        {
            if (Main.Ins.CombatData.wayPoints.Count == 0)
            {
                unit.transform.position = Main.Ins.CombatData.GLevelSpawn[mon.SpawnPoint];
            }
            else
                unit.transform.position = Main.Ins.CombatData.wayPoints.Count > mon.SpawnPoint ? Main.Ins.CombatData.wayPoints[mon.SpawnPoint].pos : Main.Ins.CombatData.wayPoints[0].pos;//等关卡脚本实现之后在设置单机出生点.PlayerEx.Instance.SpawnPoint
        }
        else if (Main.Ins.CombatData.GLevelMode > LevelMode.SinglePlayerTask && Main.Ins.CombatData.GLevelMode <= LevelMode.MultiplyPlayer)
        {
            if (Main.Ins.CombatData.GGameMode == GameMode.Normal)
            {
                unit.transform.position = Main.Ins.CombatData.wayPoints.Count > mon.SpawnPoint ? Main.Ins.CombatData.wayPoints[mon.SpawnPoint].pos : Main.Ins.CombatData.wayPoints[0].pos;//等关卡脚本实现之后在设置单机出生点.PlayerEx.Instance.SpawnPoint
            }
            else if (Main.Ins.CombatData.GGameMode == GameMode.MENGZHU)
            {
                //16个点
                unit.transform.position = Main.Ins.CombatData.GLevelSpawn[Main.Ins.CombatData.SpawnIndex];
                Main.Ins.CombatData.SpawnIndex++;
                Main.Ins.CombatData.SpawnIndex %= 16;
                unit.transform.eulerAngles = new Vector3(0, mon.SpawnDir, 0);
            }
            else if (Main.Ins.CombatData.GGameMode == GameMode.ANSHA || Main.Ins.CombatData.GGameMode == GameMode.SIDOU)
            {
                //2个队伍8个点.
                if (unit.Camp == EUnitCamp.EUC_FRIEND)
                {
                    unit.transform.position = Main.Ins.CombatData.GCampASpawn[Main.Ins.CombatData.CampASpawnIndex];
                    Main.Ins.CombatData.CampASpawnIndex++;
                    Main.Ins.CombatData.CampASpawnIndex %= 8;
                }
                else if (unit.Camp == EUnitCamp.EUC_ENEMY)
                {
                    unit.transform.position = Main.Ins.CombatData.GCampASpawn[Main.Ins.CombatData.CampBSpawnIndex];
                    Main.Ins.CombatData.CampBSpawnIndex++;
                    Main.Ins.CombatData.CampBSpawnIndex %= 8;
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
        MeteorUnit target = Main.Ins.SceneMng.Spawn(script);
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
        if (GameOverlayDialogState.Exist())
            GameOverlayDialogState.Instance.InsertSystemMsg(msg);
    }

    //弹出一个简单提示
    public static void PopupTip(string str)
    {
        PopupTipController Tips = new PopupTipController();
        Tips.Popup(str);
    }

    static UnityEngine.AsyncOperation backOp;
    static UnityEngine.AsyncOperation loadMainOp;

    //回到关卡选择面板，继续剧本
    public static void GoToLevelMenu()
    {
        U3D.LoadScene("Startup", () => {
            Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.LevelDialogState, Main.Ins.CombatData.Chapter == null);
        });
    }
    //返回到主目录
    public static void GoBack()
    {
        Main.Ins.CombatData.GLevelItem = null;
        Main.Ins.CombatData.wayPoints = null;
        U3D.LoadScene("Startup", () => {
            Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.MainMenuState);
        });
    }

    //修改版本号后回到Startup重新加载资源
    public static void ReStart()
    {
        FrameReplay.Instance.OnDisconnected();
        Main.Ins.CombatData.GLevelItem = null;
        Main.Ins.CombatData.wayPoints = null;
        U3D.LoadScene("Startup", () => {
            Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.StartupDialogState);
        });
    }

    void OnLoadMainFinished(Action t)
    {
        //MainWnd.Instance.Open();
        Main.Ins.GameStateMgr.SaveState();
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
            string path = string.Format("{0}/{1}", Application.persistentDataPath, Main.Ins.GameStateMgr.gameStatus.saveSlot);
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
        if (RobotDialogState.Exist)
            RobotDialogState.Instance.OnBackPress();
        else
            Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.RobotDialogState);
    }

    public static void OpenSfxWnd()
    {
        if (SfxDialogState.Exist)
            SfxDialogState.Instance.OnBackPress();
        else
            Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.SfxDialogState);
    }

    //打开武器界面，主角色调试切换主手武器.
    public static void OpenWeaponWnd()
    {
        if (WeaponDialogState.Exist)
            WeaponDialogState.Instance.OnBackPress();
        else
            Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.WeaponDialogState);
    }

#if !STRIP_DBG_SETTING
    private GameObject DebugCanvas;
    void InitDebugSetting()
    {
        DebugCanvas = GameObject.Instantiate(ResMng.LoadPrefab("DebugCanvas")) as GameObject;
        DebugCanvas.transform.SetParent(transform);
        DebugCanvas.transform.localScale = Vector3.one;
        DebugCanvas.transform.rotation = Quaternion.identity;
        DebugCanvas.transform.position = Vector3.zero;
        DebugCanvas.SetActive(false);
    }

    public void ShowDbg()
    {
        DebugCanvas.SetActive(true);
        WSDebug.Ins.OpenGUIDebug();
    }

    public void CloseDbg()
    {
        DebugCanvas.SetActive(false);
    }
#endif

    public static void OpenSystemWnd()
    {
        if (Main.Ins.CombatData.GLevelItem != null)
        {
            Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.EscDialogState);
            if (NGUIJoystick.instance != null)
                NGUIJoystick.instance.Lock(true);
            return;
        }
    }

    public static void PlayBtnAudio(string audio = "btn")
    {
        if (Main.Ins.SoundManager != null)
            Main.Ins.SoundManager.PlaySound(audio);
    }

    public static void LoadNetLevel()
    {
        Main.Ins.NetWorkBattle.Load();
    }

    public static void LoadScene(string scene, Action OnFinished)
    {
        LevelHelper helper = Instance.gameObject.AddComponent<LevelHelper>();
        helper.LoadScene(scene, OnFinished);
    }

    //加载当前设置的关卡.在打开模组剧本时/联机时
    public static void LoadLevelEx()
    {
        Main.Ins.SoundManager.StopAll();
        Main.Ins.SoundManager.Enable(false);
        ClearLevelData();
        Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.LoadingDialogState);
        //LoadingWnd.Instance.Open();
        Resources.UnloadUnusedAssets();
        GC.Collect();
        LevelHelper helper = Instance.gameObject.AddComponent<LevelHelper>();
        helper.Load();
        Log.Write("helper.load end");
    }

    //播放路线
    public static void PlayRecord(GameRecord rec)
    {
        Main.Ins.CombatData.GRecord = rec;
        Main.Ins.CombatData.GLevelItem = Main.Ins.CombatData.GetLevel(rec.Chapter, rec.Id);
        Main.Ins.CombatData.Chapter = DlcMng.GetPluginChapter(rec.Chapter);
        Main.Ins.CombatData.GLevelMode = LevelMode.SinglePlayerTask;
        Main.Ins.CombatData.GGameMode = GameMode.Normal;
        Main.Ins.CombatData.wayPoints = CombatData.GetWayPoint(Main.Ins.CombatData.GLevelItem);
        LoadLevelEx();
    }

    //走参数指定关卡.非剧本&非回放
    public static void LoadLevel(LevelDatas.LevelDatas lev, LevelMode levelmode, GameMode gamemode)
    {
        Main.Ins.CombatData.GRecord = null;
        Main.Ins.CombatData.Chapter = null;
        Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.LoadingDialogState);
        //暂时不允许使用声音管理器，在切换场景时不允许播放
        Main.Ins.SoundManager.StopAll();
        Main.Ins.SoundManager.Enable(false);
        ClearLevelData();
        Main.Ins.CombatData.GLevelItem = lev;
        Main.Ins.CombatData.GLevelMode = levelmode;
        Main.Ins.CombatData.GGameMode = gamemode;
        Main.Ins.CombatData.wayPoints = CombatData.GetWayPoint(lev);
        Resources.UnloadUnusedAssets();
        GC.Collect();
        if (!string.IsNullOrEmpty(lev.sceneItems) && !Main.Ins.GameStateMgr.gameStatus.SkipVideo && levelmode == LevelMode.SinglePlayerTask && Main.Ins.CombatData.Chapter == null)
        {
            string num = lev.sceneItems.Substring(2);
            int number = 0;
            if (int.TryParse(num, out number))
            {
                if (lev.ID >= 0 && lev.ID <= 9)
                {
                    string movie = string.Format(Main.strFile, Main.strHost, Main.port, Main.strProjectUrl, "mmv/" + "b" + number + ".mv");
                    U3D.PlayMovie(movie);
                }
            }
        }
        LevelHelper helper = Instance.gameObject.AddComponent<LevelHelper>();
        helper.Load();
    }

    public static void ClearLevelData()
    {
        Main.Ins.SFXLoader.Effect.Clear();
        Main.Ins.DesLoader.Refresh();
        Main.Ins.GMCLoader.Refresh();
        Main.Ins.GMBLoader.Refresh();
        Main.Ins.FMCLoader.Refresh();
        //先清理BUF
        Main.Ins.BuffMng.Clear();
        Main.Ins.MeteorManager.Clear();
        LevelScriptBase.Clear();
        Main.Ins.CombatData.CampASpawnIndex = 0;
        Main.Ins.CombatData.CampBSpawnIndex = 0;
        Main.Ins.CombatData.SpawnIndex = 0;
#if !STRIP_DBG_SETTING
        U3D.Instance.CloseDbg();
#endif
    }

    //对接原版脚本
    static Loader sceneRoot;
    public static void SetSceneItem(string name, string features, int value1)
    {
        if (sceneRoot == null)
            sceneRoot = FindObjectOfType<Loader>();
        GameObject objSelected = NodeHelper.Find(name, sceneRoot.gameObject);
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
        GameObject objSelected = NodeHelper.Find(name, sceneRoot.gameObject);
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
        GameObject objSelected = NodeHelper.Find(name, Loader.Instance.gameObject);
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
        GameObject objSelected = NodeHelper.Find(name, Loader.Instance.gameObject);
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
        for (int i = 0; i < Main.Ins.MeteorManager.SceneItems.Count; i++)
        {
            if (Main.Ins.MeteorManager.SceneItems[i].name == name)
                return Main.Ins.MeteorManager.SceneItems[i];
        }
        return null;
    }

    public static int GetSceneItem(string name, string feature)
    {
        for (int i = 0; i < Main.Ins.MeteorManager.SceneItems.Count; i++)
        {
            if (Main.Ins.MeteorManager.SceneItems[i].name == name)
                return Main.Ins.MeteorManager.SceneItems[i].GetSceneItem(feature);
        }
        return -1;
    }

    public static SceneItemAgent GetSceneItem(int id)
    {
        for (int i = 0; i < Main.Ins.MeteorManager.SceneItems.Count; i++)
        {
            if (Main.Ins.MeteorManager.SceneItems[i].InstanceId == id)
                return Main.Ins.MeteorManager.SceneItems[i];
        }
        return null;
    }
    public static int GetSceneItem(int id, string feature)
    {
        for (int i = 0; i < Main.Ins.MeteorManager.SceneItems.Count; i++)
        {
            if (Main.Ins.MeteorManager.SceneItems[i].InstanceId == id)
                return Main.Ins.MeteorManager.SceneItems[i].GetSceneItem(feature);
        }
        return -1;
    }

    public static int GetTeam(int characterId)
    {
        for (int i = 0; i < Main.Ins.MeteorManager.UnitInfos.Count; i++)
        {
            if (Main.Ins.MeteorManager.UnitInfos[i].InstanceId == characterId)
                return (int)Main.Ins.MeteorManager.UnitInfos[i].Camp;
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
        if (Main.Ins.SFXLoader != null && objEffect != null)
            Main.Ins.SFXLoader.PlayEffect(effect, objEffect, !loop);
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
        if (Main.Ins.SFXLoader != null && objEffect != null)
            Main.Ins.SFXLoader.PlayEffect(effect, objEffect.gameObject, !loop);
    }

    public static MeteorUnit GetTeamLeader(EUnitCamp camp)
    {
        for (int i = 0; i < Main.Ins.MeteorManager.UnitInfos.Count; i++)
        {
            if (Main.Ins.MeteorManager.UnitInfos[i].Camp == camp)
            {
                if (Main.Ins.MeteorManager.UnitInfos[i].IsLeader)
                    return Main.Ins.MeteorManager.UnitInfos[i];
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
                Main.Ins.GameBattleEx.PushActionWait(id);
            }
            else if (act == "follow")
            {
                //object target = null;
                if (value[1].GetType() == typeof(int))
                {
                    Main.Ins.GameBattleEx.PushActionFollow(id, (int)value[1]);
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
                                Main.Ins.GameBattleEx.PushActionFollow(id, t.InstanceId);
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
                                Main.Ins.GameBattleEx.PushActionFollow(id, t.InstanceId);
                        }
                    }
                    else if (value[1] as string == "player")
                    {
                        Main.Ins.GameBattleEx.PushActionFollow(id, Main.Ins.LocalPlayer.InstanceId);
                    }
                    else
                    {
                        int flag = U3D.GetChar("flag");//跟随镖物跑，A，镖物被人取得，B，镖物在地图某处.
                        if (flag >= 0)
                        {
                            Main.Ins.GameBattleEx.PushActionFollow(id, flag);
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
                Main.Ins.GameBattleEx.PushActionPatrol(id, Path);
            }
            else if (act == "faceto")
            {
                Main.Ins.GameBattleEx.PushActionFaceTo(id, (int)value[1]);
            }
            else if (act == "kill")
            {
                //Debug.LogError("gamebattleex kill");
                Main.Ins.GameBattleEx.PushActionKill(id, (int)value[1]);
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
                    Main.Ins.GameBattleEx.PushActionAttackTarget(id, (int)value[1]);
                }
                else if (value.Length == 3)
                {
                    //UnityEngine.Debug.LogError(string.Format("attacktarget:{0}, {1}", (int)value[1], (int)value[2]));
                    Main.Ins.GameBattleEx.PushActionAttackTarget(id, (int)value[1], (int)value[2]);
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
        for (int i = 0; i < Main.Ins.MeteorManager.UnitInfos.Count; i++)
        {
            if (Main.Ins.MeteorManager.UnitInfos[i].GetFlag)
                return Main.Ins.MeteorManager.UnitInfos[i];
        }
        return null;
    }

    public static int GetChar(string player)
    {
        if (player == "player")
            return Main.Ins.LocalPlayer.InstanceId;
        //取得谁获得了任务物品
        if (player == "flag")
        {
            for (int i = 0; i < Main.Ins.MeteorManager.UnitInfos.Count; i++)
            {
                if (Main.Ins.MeteorManager.UnitInfos[i].GetFlag)
                    return Main.Ins.MeteorManager.UnitInfos[i].InstanceId;
            }
            return -1;
        }
        for (int i = 0; i < Main.Ins.MeteorManager.UnitInfos.Count; i++)
        {
            if (Main.Ins.MeteorManager.UnitInfos[i].name == player)
                return Main.Ins.MeteorManager.UnitInfos[i].InstanceId;
        }

        for (int i = 0; i < Main.Ins.MeteorManager.DeadUnits.Count; i++)
        {
            if (Main.Ins.MeteorManager.DeadUnits[i].name == player)
                return Main.Ins.MeteorManager.DeadUnits[i].InstanceId;
        }

        foreach (var each in Main.Ins.MeteorManager.LeavedUnits)
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
        for (int i = 0; i < Main.Ins.MeteorManager.UnitInfos.Count; i++)
        {
            if (Main.Ins.MeteorManager.UnitInfos[i].name == name)
                return Main.Ins.MeteorManager.UnitInfos[i].InstanceId;
        }
        for (int i = 0; i < Main.Ins.MeteorManager.DeadUnits.Count; i++)
        {
            if (Main.Ins.MeteorManager.DeadUnits[i].name == name)
                return Main.Ins.MeteorManager.DeadUnits[i].InstanceId;
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
        for (int i = 0; i < Main.Ins.MeteorManager.UnitInfos.Count; i++)
        {
            if (Main.Ins.MeteorManager.UnitInfos[i].InstanceId == id)
                return Main.Ins.MeteorManager.UnitInfos[i].Attr.hpCur;
        }
        for (int i = 0; i < Main.Ins.MeteorManager.DeadUnits.Count; i++)
        {
            if (Main.Ins.MeteorManager.DeadUnits[i].InstanceId == id)
                return Main.Ins.MeteorManager.DeadUnits[i].Attr.hpCur;
        }
        return 0;
    }
    public static int GetMaxHP(int id)
    {
        for (int i = 0; i < Main.Ins.MeteorManager.UnitInfos.Count; i++)
        {
            if (Main.Ins.MeteorManager.UnitInfos[i].InstanceId == id)
                return Main.Ins.MeteorManager.UnitInfos[i].Attr.HpMax;
        }
        for (int i = 0; i < Main.Ins.MeteorManager.DeadUnits.Count; i++)
        {
            if (Main.Ins.MeteorManager.DeadUnits[i].InstanceId == id)
                return Main.Ins.MeteorManager.DeadUnits[i].Attr.HpMax;
        }
        return 0;
    }

    public static int GetEnemy(int id)
    {
        MeteorUnit unit = GetUnit(id);
        if (unit == null)
            return -1;
        MeteorUnit enemy = unit.LockTarget;
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
                Main.Ins.GameBattleEx.PushActionPause(id, fun[0]);
            }
            //unit.PauseAI(fun[0]);
            else if (pose == "faceto" && fun != null && fun.Length == 1)
            {
                MeteorUnit target = GetUnit(fun[0]);
                if (target != null)
                    unit.FaceToTarget(target);
            }
            else if (pose == "guard" && fun != null && fun.Length == 1)
                Main.Ins.GameBattleEx.PushActionGuard(id, fun[0]);
            else if (pose == "aggress")
                Main.Ins.GameBattleEx.PushActionAggress(id);
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
                Main.Ins.GameBattleEx.PushActionSkill(id); //unit.PlaySkill();
            }
            else if (pose == "crouch")
                Main.Ins.GameBattleEx.PushActionCrouch(id, fun[0]);//1是指令状态，1代表应用状态， 0代表取消状态
            else if (pose == "block")
                Main.Ins.GameBattleEx.PushActionBlock(id, fun[0]);//阻止输入/取消阻止输入.与硬直应该差不多.
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
            Main.Ins.GameBattleEx.PushActionPause(0, param);//0代表主角色.
        else if (act == "use")
        {
            Main.Ins.GameBattleEx.PushActionUse(0, param);
        }
        else if (act == "crouch")
            Main.Ins.GameBattleEx.PushActionCrouch(0, param);//1是指令状态，1代表应用状态， 0代表取消状态
        else if (act == "block")
            Main.Ins.GameBattleEx.PushActionBlock(0, param);//阻止输入/取消阻止输入.与硬直应该差不多.
        else if (act == "guard")
            Main.Ins.GameBattleEx.PushActionGuard(0, param);
        else if (act == "faceto")
        {
            MeteorUnit target = GetUnit(param);
            if (target != null)
                Main.Ins.LocalPlayer.FaceToTarget(target);
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
        if (u.StateMachine != null)
            u.StateMachine.ChangeState(u.StateMachine.IdleState);
        if (u != null)
        {
            //先清除该角色的聊天动作，其他动画动作不处理。
            Main.Ins.GameBattleEx.StopAction(id);
        }
        return 0;
    }

    // type="char", "waypoint", "flag", "safe"
    //获得2个角色的距离.
    public static float Distance(int idx1, int idx2)
    {
        if (Main.Ins.CombatData.GLevelItem != null && Main.Ins.CombatData.GLevelMode <= LevelMode.SinglePlayerTask)
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
        if (Main.Ins.CombatData.GLevelItem != null && Main.Ins.CombatData.GLevelMode <= LevelMode.SinglePlayerTask)
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
        if (Main.Ins.CombatData.GLevelItem != null && Main.Ins.CombatData.GLevelMode <= LevelMode.SinglePlayerTask)
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
        if (Main.Ins.CombatData.GLevelItem != null && Main.Ins.CombatData.GLevelMode <= LevelMode.SinglePlayerTask)
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
        Main.Ins.MeteorManager.OnRemoveUnit(unit);
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
        for (int i = 0; i < Main.Ins.MeteorManager.UnitInfos.Count; i++)
        {
            if (Main.Ins.MeteorManager.UnitInfos[i].InstanceId == id)
                return Main.Ins.MeteorManager.UnitInfos[i];
        }

        for (int i = 0; i < Main.Ins.MeteorManager.DeadUnits.Count; i++)
        {
            if (Main.Ins.MeteorManager.DeadUnits[i].InstanceId == id)
                return Main.Ins.MeteorManager.DeadUnits[i];
        }
        return null;
    }

    public static void Say(int id, string param)
    {
        if (Main.Ins.GameBattleEx != null)
            Main.Ins.GameBattleEx.PushActionSay(id, param);//1=say 2=pause
    }

    static WebClient downloadMovie;
    public static List<string> WaitDownload = new List<string>();
    public static void PlayMovie(string movie)
    {
        if (Main.Ins.GameStateMgr.gameStatus.LocalMovie.ContainsKey(movie))
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
                            Main.Ins.GameStateMgr.gameStatus.LocalMovie.Add(download, local);
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
            Handheld.PlayFullScreenMovie(Main.Ins.GameStateMgr.gameStatus.LocalMovie[movie], Color.black, FullScreenMovieControlMode.CancelOnInput, FullScreenMovieScalingMode.AspectFit);
        }
        else
        {
            if (Main.Ins.GameStateMgr.gameStatus.SkipVideo)
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
                if (e.Error == null)
                {
                    Debug.Log("download completed");
                    Main.Ins.GameStateMgr.gameStatus.LocalMovie.Add(movie, local);
                    Main.Ins.GameStateMgr.SaveState();
                }
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
        for (int i = 0; i < Main.Ins.MeteorManager.UnitInfos.Count; i++)
        {
            if (Main.Ins.MeteorManager.UnitInfos[i].Camp != EUnitCamp.EUC_Meteor)
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
        ItemDatas.ItemDatas it = Main.Ins.GameStateMgr.FindItemByIdx(itemIdx);
        if (it.MainType == 1)//武器
            Main.Ins.DropMng.DropWeapon2(it.UnitId);
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
        ItemDatas.ItemDatas it0 = Main.Ins.GameStateMgr.FindItemByIdx(itemIdx);
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
        List< LevelDatas.LevelDatas> level = Main.Ins.DataMgr.GetDatasArray<LevelDatas.LevelDatas>();
        if (level == null)
            return 0;
        int max = 0;
        for (int i = 0; i < level.Count; i++)
        {
            if (level[i].ID > max)
                max = level[i].ID;
        }
        return max;
    }

    public static void UnlockLevel()
    {
        Main.Ins.GameStateMgr.gameStatus.Level = Main.Ins.CombatData.LEVELMAX;
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
        for (int i = 0; i < Main.Ins.MeteorManager.UnitInfos.Count; i++)
        {
            if (Main.Ins.MeteorManager.UnitInfos[i].SameCamp(Main.Ins.LocalPlayer))
            {
                if (!Main.Ins.MeteorManager.UnitInfos[i].Dead)
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
        for (int i = 0; i < Main.Ins.MeteorManager.UnitInfos.Count; i++)
        {
            if (!Main.Ins.MeteorManager.UnitInfos[i].SameCamp(Main.Ins.LocalPlayer))
            {
                if (!Main.Ins.MeteorManager.UnitInfos[i].Dead)
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
        for (int i = 0; i < Main.Ins.MeteorManager.UnitInfos.Count; i++)
        {
            Main.Ins.MeteorManager.UnitInfos[i].EnableAI(false);
        }
    }

    public static void OnResumeAI()
    {
        for (int i = 0; i < Main.Ins.MeteorManager.UnitInfos.Count; i++)
        {
            Main.Ins.MeteorManager.UnitInfos[i].EnableAI(true);
        }
    }

    //以下为脚本系统执行面板里能响应的操作
    public static void GodLike()
    {
        Main.Ins.GameStateMgr.gameStatus.GodLike = !Main.Ins.GameStateMgr.gameStatus.GodLike;
        U3D.InsertSystemMsg(Main.Ins.GameStateMgr.gameStatus.GodLike ? "作弊[开]" : "作弊[关]");
        //if (EscWnd.Exist)
        //    EscWnd.Instance.Close();
        //EscWnd.Instance.Open();
    }

    public static WeaponDatas.WeaponDatas GetWeaponProperty(int weaponIdx)
    {
        WeaponDatas.WeaponDatas w = Main.Ins.DataMgr.GetData<WeaponDatas.WeaponDatas>(weaponIdx);
        //if (w == null)
        //    w = PluginWeaponMng.Instance.GetItem(weaponIdx);
        if (w == null)
            Debug.LogError("can not find weapon:" + weaponIdx);
        return w;
    }

    public static void DeletePlugins()
    {
        Main.Ins.GameStateMgr.gameStatus.pluginChapter.Clear();
        Main.Ins.GameStateMgr.gameStatus.pluginModel.Clear();
        Main.Ins.GameStateMgr.gameStatus.pluginNpc.Clear();
        Main.Ins.CombatData.PluginUpdated = false;
        Main.Ins.GameStateMgr.SaveState();
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
        Main.Ins.SFXLoader.PlayEffect("vipblue.ef", uPlayer.gameObject, false);
        if (uEnemy != null)
            Main.Ins.SFXLoader.PlayEffect("vipred.ef", uEnemy.gameObject, false);
    }
}
