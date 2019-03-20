using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DlcHelper : MonoBehaviour
{
    AsyncOperation mAsync;
    public struct LevelParam
    {
        public int id;
        public int gate;
    }

    public void Load(int id)
    {
        StartCoroutine(LoadLevelAsync(id));
    }

    IEnumerator LoadLevelAsync(int levelId)
    {
        int displayProgress = 0;
        int toProgress = 0;
        yield return 0;
        Level lev = DlcMng.Instance.GetLevelItem(levelId);
        if (lev == null)
            yield break;
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
        for (int i = 0; i < 5; i++)
            yield return 0;
        if (LoadingWnd.Exist)
            LoadingWnd.Instance.Close();
        Destroy(this);
    }

    LevelScriptBase GetLevelScript(string sn)
    {
        Type type = Type.GetType(string.Format("LevelScript_{0}", sn));
        if (type == null)
            return null;
        Global.Instance.GScriptType = type;
        return System.Activator.CreateInstance(type) as LevelScriptBase;
    }

    void SpawnAllRobot()
    {
        if (Global.Instance.GGameMode == GameMode.MENGZHU)
        {
            for (int i = 1; i < Global.Instance.MaxPlayer; i++)
            {
                U3D.SpawnRobot(U3D.GetRandomUnitIdx(), EUnitCamp.EUC_KILLALL, GameData.Instance.gameStatus.DisallowSpecialWeapon ? U3D.GetNormalWeaponType() : U3D.GetRandomWeaponType(), Global.Instance.PlayerLife);
            }
        }
        else if (Global.Instance.GGameMode == GameMode.ANSHA || Global.Instance.GGameMode == GameMode.SIDOU)
        {
            int FriendCount = Global.Instance.MaxPlayer / 2 - 1;
            for (int i = 0; i < FriendCount; i++)
            {
                U3D.SpawnRobot(U3D.GetRandomUnitIdx(), MeteorManager.Instance.LocalPlayer.Camp, GameData.Instance.gameStatus.DisallowSpecialWeapon ? U3D.GetNormalWeaponType() : U3D.GetRandomWeaponType(), Global.Instance.PlayerLife);
            }

            for (int i = FriendCount + 1; i < Global.Instance.MaxPlayer; i++)
            {
                U3D.SpawnRobot(U3D.GetRandomUnitIdx(), U3D.GetAnotherCamp(MeteorManager.Instance.LocalPlayer.Camp), GameData.Instance.gameStatus.DisallowSpecialWeapon ? U3D.GetNormalWeaponType() : U3D.GetRandomWeaponType(), Global.Instance.PlayerLife);
            }
        }
    }

    void OnLoadFinishedEx(Level lev)
    {
        SoundManager.Instance.Enable(true);
        LevelScriptBase script = GetLevelScript(lev.LevelScript);
        if (script == null)
        {
            UnityEngine.Debug.LogError(string.Format("level script is null levId:{0}, levScript:{1}", lev.FuBenID, lev.LevelScript));
            return;
        }

        Global.Instance.GScript = script;
        SceneMng.OnLoad();//
                          //加载场景配置数据
        SceneMng.OnEnterLevel(script, lev.ID);//原版功能不加载其他存档数据.

        //设置主角属性
        U3D.InitPlayer(script);
        if (GameData.Instance.gameStatus.PetOn && lev.DisableFindWay == 0)
            U3D.InitPet();
        //把音频侦听移到角色
        Startup.ins.listener.enabled = false;
        Startup.ins.playerListener = MeteorManager.Instance.LocalPlayer.gameObject.AddComponent<AudioListener>();

        //创建房间模式的时候，创建随机NPC
        if (Global.Instance.GLevelMode == LevelMode.CreateWorld)
            SpawnAllRobot();

        //等脚本设置好物件的状态后，根据状态决定是否生成受击盒，攻击盒等.
        GameBattleEx.Instance.Init(lev, script);

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
