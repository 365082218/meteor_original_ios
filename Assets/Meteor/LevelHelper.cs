using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Reflection;
using UnityEngine.UI;

//for loadingui updateui
public interface LoadingUI
{
    void UpdateProgress(float percent);
}

public class LevelHelper : MonoBehaviour
{
	AsyncOperation mAsync;
    public struct LevelParam
    {
        public int id;
        public int gate;
    }

    public void Load()
    {
        StartCoroutine(LoadLevelAsync());
    }

    IEnumerator LoadLevelAsync()
    {
        int displayProgress = 0;
        int toProgress = 0;
        yield return 0;
        Level lev = Global.Instance.GLevelItem;
        ResMng.LoadScene(lev.Scene);
        mAsync = SceneManager.LoadSceneAsync(lev.Scene);
        mAsync.allowSceneActivation = false;
        while (mAsync.progress < 0.9f)
        {
            toProgress = (int)mAsync.progress * 100;
            while (displayProgress < toProgress)
            {
                ++displayProgress;
                if (LoadingWnd.Exist)
                    LoadingWnd.Instance.UpdateProgress(displayProgress / 100.0f);
                yield return 0;
            }
            yield return 0;
        }
        toProgress = 100;
        //WSLog.LogInfo("displayProgress < toProgress");
        while (displayProgress < toProgress)
        {
            ++displayProgress;
            if (LoadingWnd.Exist)
                LoadingWnd.Instance.UpdateProgress(displayProgress / 100.0f);
            yield return 0;
        }
        //WSLog.LogInfo("displayProgress < toProgress");
        mAsync.allowSceneActivation = true;
        yield return 0;
        //WSLog.LogInfo("OnLoadFinishedEx");
        OnLoadFinishedEx(lev);
        if (LoadingWnd.Exist)
            LoadingWnd.Instance.Close();
        Destroy(this);
    }

    LevelScriptBase GetLevelScript(string sn)
    {
        string typeIden = string.Format("LevelScript_{0}", sn);
        Type type = Type.GetType(typeIden);
        if (type == null)
        {
            //尝试在chapter的dll里加载
            if (System.IO.File.Exists(Global.Instance.Chapter.Dll))
            {
                Assembly ass = Assembly.Load(System.IO.File.ReadAllBytes(Global.Instance.Chapter.Dll));
                Type[] t = ass.GetTypes();
                for (int i = 0; i < t.Length; i++)
                {
                    if (t[i].Name == typeIden)
                    {
                        //LevelScriptBase l = System.Activator.CreateInstance(t[i]) as LevelScriptBase;
                        type = t[i];
                        break;
                    }
                }
            }
            if (type == null)
            {
                Log.WriteError(string.Format("Load sn failed {0}, Meteor Version:{1}", sn, AppInfo.Instance.AppVersion()));
                return null;
            }
        }
        Global.Instance.GScriptType = type;
        return System.Activator.CreateInstance(type) as LevelScriptBase;
    }

    void SpawnAllRobot()
    {
        if (Global.Instance.GGameMode == GameMode.MENGZHU)
        {
            for (int i = 1; i < Global.Instance.MaxPlayer; i++)
            {
                U3D.SpawnRobot(U3D.GetRandomUnitIdx(), EUnitCamp.EUC_KILLALL, GameData.Instance.gameStatus.Single.DisallowSpecialWeapon ? U3D.GetNormalWeaponType() : U3D.GetRandomWeaponType(), Global.Instance.PlayerLife);
            }
        }
        else if (Global.Instance.GGameMode == GameMode.ANSHA || Global.Instance.GGameMode == GameMode.SIDOU)
        {
            int FriendCount = Global.Instance.MaxPlayer / 2 - 1;
            for (int i = 0; i < FriendCount; i++)
            {
                U3D.SpawnRobot(U3D.GetRandomUnitIdx(), MeteorManager.Instance.LocalPlayer.Camp, GameData.Instance.gameStatus.Single.DisallowSpecialWeapon ? U3D.GetNormalWeaponType() : U3D.GetRandomWeaponType(), Global.Instance.PlayerLife);
            }

            for (int i = FriendCount + 1; i < Global.Instance.MaxPlayer; i++)
            {
                U3D.SpawnRobot(U3D.GetRandomUnitIdx(), U3D.GetAnotherCamp(MeteorManager.Instance.LocalPlayer.Camp), GameData.Instance.gameStatus.Single.DisallowSpecialWeapon ? U3D.GetNormalWeaponType() : U3D.GetRandomWeaponType(), Global.Instance.PlayerLife);
            }
        }
    }

    //单机
    void OnLoadFinishedEx(Level lev)
    {
        SoundManager.Instance.Enable(true);
        LevelScriptBase script = GetLevelScript(lev.LevelScript);
        if (script == null)
        {
            UnityEngine.Debug.LogError(string.Format("level script is null levId:{0}, levScript:{1}", lev.ID, lev.LevelScript));
            return;
        }

        Global.Instance.GScript = script;
        SceneMng.OnLoad();//
        //加载场景配置数据
        SceneMng.OnEnterLevel(script, lev.ID);

        //设置主角属性
        U3D.InitPlayer(script);
        if (GameData.Instance.gameStatus.PetOn && lev.DisableFindWay == 0)
            U3D.InitPet();
        //把音频侦听移到角色
        Startup.ins.listener.enabled = false;
        Startup.ins.playerListener = MeteorManager.Instance.LocalPlayer.gameObject.AddComponent<AudioListener>();

        if (Global.Instance.GLevelMode == LevelMode.CreateWorld)
            SpawnAllRobot();
        //等脚本设置好物件的状态后，根据状态决定是否生成受击盒，攻击盒等.
        GameBattleEx.Instance.Init(script);

        //先创建一个相机
        GameObject camera = GameObject.Instantiate(Resources.Load("CameraEx")) as GameObject;
        camera.name = "CameraEx";

        //角色摄像机跟随者着角色.
        CameraFollow followCamera = GameObject.Find("CameraEx").GetComponent<CameraFollow>();
        followCamera.Init();
        GameBattleEx.Instance.m_CameraControl = followCamera;
        //摄像机完毕后
        FightWnd.Instance.Open();
        if (!string.IsNullOrEmpty(lev.BgmName))
            SoundManager.Instance.PlayMusic(lev.BgmName);

        //除了主角的所有角色,开始输出,选择阵营, 进入战场
        for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
        {
            if (MeteorManager.Instance.UnitInfos[i] == MeteorManager.Instance.LocalPlayer)
                continue;
            MeteorUnit unitLog = MeteorManager.Instance.UnitInfos[i];
            U3D.InsertSystemMsg(U3D.GetCampEnterLevelStr(unitLog));
        }

        U3D.InsertSystemMsg("新回合开始计时");
        if (FightWnd.Exist)
            FightWnd.Instance.OnBattleStart();
    }
}
