using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class WorldTemplateDialogState : CommonDialogState<WorldTemplateDialog>
{
    public override string DialogName { get { return "WorldTemplate"; } }
    public WorldTemplateDialogState(MainDialogStateManager stateMgr) : base(stateMgr)
    {

    }
}

//单机创建房间面板
public class WorldTemplateDialog : Dialog
{
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
    }

    GameObject TemplateRoot;

    int[] ConstRoundTime = { 15, 30, 60 };
    int[] ConstPlayer = { 2, 4, 8, 12, 16 };
    void Init()
    {
        Control("CreateWorld").GetComponent<Button>().onClick.AddListener(() =>
        {
            OnEnterLevel();
        });
        GameObject RuleGroup = Control("RuleGroup", WndObject);
        Toggle rule0 = Control("0", RuleGroup).GetComponent<Toggle>();
        Toggle rule1 = Control("1", RuleGroup).GetComponent<Toggle>();
        Toggle rule2 = Control("2", RuleGroup).GetComponent<Toggle>();
        rule0.isOn = Main.Ins.GameStateMgr.gameStatus.Single.Mode == (int)GameMode.MENGZHU;
        rule1.isOn = Main.Ins.GameStateMgr.gameStatus.Single.Mode == (int)GameMode.ANSHA;
        rule2.isOn = Main.Ins.GameStateMgr.gameStatus.Single.Mode == (int)GameMode.SIDOU;

        rule0.onValueChanged.AddListener((bool select) => { if (select) Main.Ins.GameStateMgr.gameStatus.Single.Mode = (int)GameMode.MENGZHU; });
        rule1.onValueChanged.AddListener((bool select) => { if (select) Main.Ins.GameStateMgr.gameStatus.Single.Mode = (int)GameMode.ANSHA; });
        rule2.onValueChanged.AddListener((bool select) => { if (select) Main.Ins.GameStateMgr.gameStatus.Single.Mode = (int)GameMode.SIDOU; });

        GameObject LifeGroup = Control("LifeGroup", WndObject);
        Toggle Life0 = Control("0", LifeGroup).GetComponent<Toggle>();
        Toggle Life1 = Control("1", LifeGroup).GetComponent<Toggle>();
        Toggle Life2 = Control("2", LifeGroup).GetComponent<Toggle>();

        Life0.isOn = Main.Ins.GameStateMgr.gameStatus.Single.Life == 500;
        Life1.isOn = Main.Ins.GameStateMgr.gameStatus.Single.Life == 200;
        Life2.isOn = Main.Ins.GameStateMgr.gameStatus.Single.Life == 100;
        Life0.onValueChanged.AddListener((bool select) => { if (select) Main.Ins.GameStateMgr.gameStatus.Single.Life = 500; });
        Life1.onValueChanged.AddListener((bool select) => { if (select) Main.Ins.GameStateMgr.gameStatus.Single.Life = 200; });
        Life2.onValueChanged.AddListener((bool select) => { if (select) Main.Ins.GameStateMgr.gameStatus.Single.Life = 100; });

        GameObject MainWeaponGroup = Control("FirstWeapon", WndObject);
        GameObject WeaponGroup = Control("WeaponGroup", MainWeaponGroup);
        for (int i = 0; i <= 11; i++)
        {
            Toggle MainWeapon = Control(string.Format("{0}", i), WeaponGroup).GetComponent<Toggle>();
            MainWeapon.isOn = Main.Ins.GameStateMgr.gameStatus.Single.Weapon0 == i;
            MainWeapon.onValueChanged.AddListener(OnMainWeaponSelected);
        }

        GameObject SubWeaponGroup = Control("SubWeapon", WndObject);
        WeaponGroup = Control("WeaponGroup", SubWeaponGroup);
        for (int i = 0; i <= 11; i++)
        {
            Toggle subWeapon = Control(string.Format("{0}", i), WeaponGroup).GetComponent<Toggle>();
            subWeapon.isOn = Main.Ins.GameStateMgr.gameStatus.Single.Weapon1 == i;
            subWeapon.onValueChanged.AddListener(OnSubWeaponSelected);
        }

        Control("Return").GetComponent<Button>().onClick.AddListener(() =>
        {
            Main.Ins.GameStateMgr.SaveState();
            OnPreviousPress();
        });

        //地图模板，应该从所有地图表里获取，包括外部载入的地图.
        TemplateRoot = Control("WorldRoot", WndObject);
        LevelDatas.LevelDatas[] allLevel = Main.Ins.CombatData.GetAllLevel();
        for (int i = 0; i < allLevel.Length; i++)
        {
            LevelDatas.LevelDatas lev = allLevel[i];
            if (lev == null)
                continue;
            Idevgame.Util.LevelUtils.AddGridItem(lev, TemplateRoot.transform, OnSelectLevel);
        }
        select = Main.Ins.CombatData.GetLevel(Main.Ins.GameStateMgr.gameStatus.ChapterTemplate, Main.Ins.GameStateMgr.gameStatus.Single.LevelTemplate);
        OnSelectLevel(select);

        GameObject ModelGroup = Control("ModelGroup");
        for (int i = 0; i < 20; i++)
        {
            Toggle modelTog = Control(string.Format("{0}", i), ModelGroup).GetComponent<Toggle>();
            Text t = modelTog.GetComponentInChildren<Text>();
            t.text = Main.Ins.DataMgr.GetDatasArray<ModelDatas.ModelDatas>()[i].Name;
            var k = i;
            modelTog.isOn = Main.Ins.GameStateMgr.gameStatus.Single.Model == i;
            modelTog.onValueChanged.AddListener((bool select) => { if (select) Main.Ins.GameStateMgr.gameStatus.Single.Model = k; });
        }

        GameObject TimeGroup = Control("GameTime", WndObject);
        for (int i = 0; i < 3; i++)
        {
            Toggle TimeToggle = Control(string.Format("{0}", i), TimeGroup).GetComponent<Toggle>();
            var k = i;
            TimeToggle.isOn = Main.Ins.GameStateMgr.gameStatus.Single.RoundTime == ConstRoundTime[k];
            TimeToggle.onValueChanged.AddListener((bool selected) => { if (selected) Main.Ins.GameStateMgr.gameStatus.Single.RoundTime = ConstRoundTime[k]; });
        }

        GameObject PlayerGroup = Control("PlayerGroup", WndObject);
        for (int i = 0; i <= 4; i++)
        {
            Toggle PlayerToggle = Control(string.Format("{0}", i), PlayerGroup).GetComponent<Toggle>();
            var k = i;
            PlayerToggle.isOn = Main.Ins.GameStateMgr.gameStatus.Single.MaxPlayer == ConstPlayer[k];
            PlayerToggle.onValueChanged.AddListener((bool selected) =>
            {
                if (selected)
                    Main.Ins.GameStateMgr.gameStatus.Single.MaxPlayer = ConstPlayer[k];
            });
        }

        GameObject DisallowGroup = Control("DisallowGroup", WndObject);
        Toggle DisallowToggle = Control("0", DisallowGroup).GetComponent<Toggle>();
        DisallowToggle.isOn = Main.Ins.GameStateMgr.gameStatus.Single.DisallowSpecialWeapon;
        DisallowToggle.onValueChanged.AddListener((bool selected) => { Main.Ins.GameStateMgr.gameStatus.Single.DisallowSpecialWeapon = selected; });
    }

    void OnMainWeaponSelected(bool select)
    {
        if (select)
        {
            GameObject MainWeaponGroup = Control("FirstWeapon", WndObject);
            GameObject WeaponGroup = Control("WeaponGroup", MainWeaponGroup);
            for (int i = 0; i <= 11; i++)
            {
                Toggle MainWeapon = Control(string.Format("{0}", i), WeaponGroup).GetComponent<Toggle>();
                if (MainWeapon.isOn)
                {
                    Main.Ins.GameStateMgr.gameStatus.Single.Weapon0 = i;
                    break;
                }

            }
        }
    }

    void OnSubWeaponSelected(bool select)
    {
        if (select)
        {
            GameObject MainWeaponGroup = Control("SubWeapon", WndObject);
            GameObject WeaponGroup = Control("WeaponGroup", MainWeaponGroup);
            for (int i = 0; i <= 11; i++)
            {
                Toggle subWeapon = Control(string.Format("{0}", i), WeaponGroup).GetComponent<Toggle>();
                if (subWeapon.isOn)
                {
                    Main.Ins.GameStateMgr.gameStatus.Single.Weapon1 = i;
                    break;
                }
            }
        }
    }

    LevelDatas.LevelDatas select;
    void OnSelectLevel(LevelDatas.LevelDatas lev)
    {
        select = lev;
        Main.Ins.GameStateMgr.gameStatus.Single.LevelTemplate = lev.ID;
        Control("Task").GetComponent<Text>().text = select.Name;
    }

    void OnEnterLevel()
    {
        if (select != null)
        {
            Main.Ins.CombatData.MainWeapon = Main.Ins.GameStateMgr.gameStatus.Single.Weapon0;
            Main.Ins.CombatData.SubWeapon = Main.Ins.GameStateMgr.gameStatus.Single.Weapon1;
            Main.Ins.CombatData.PlayerLife = Main.Ins.GameStateMgr.gameStatus.Single.Life;
            Main.Ins.CombatData.PlayerModel = Main.Ins.GameStateMgr.gameStatus.Single.Model;
            Main.Ins.CombatData.RoundTime = Main.Ins.GameStateMgr.gameStatus.Single.RoundTime;
            Main.Ins.CombatData.MaxPlayer = Main.Ins.GameStateMgr.gameStatus.Single.MaxPlayer;
            bool isPluginLevel = true;
            List<LevelDatas.LevelDatas> all = Main.Ins.DataMgr.GetDatasArray<LevelDatas.LevelDatas>();
            for (var i = 0; i < all.Count; i++)
            {
                if (all[i] == select)
                {
                    isPluginLevel = false;
                    break;
                }
            }
            if (isPluginLevel)
            {
                Main.Ins.CombatData.Chapter = Main.Ins.DlcMng.FindChapterByLevel(select);
                Main.Ins.GameStateMgr.gameStatus.ChapterTemplate = Main.Ins.CombatData.Chapter.ChapterId;
                Main.Ins.GameStateMgr.gameStatus.Single.LevelTemplate = select.ID;
            }
            else
            {
                Main.Ins.GameStateMgr.gameStatus.ChapterTemplate = 0;
                Main.Ins.GameStateMgr.gameStatus.Single.LevelTemplate = select.ID;
                Main.Ins.CombatData.Chapter = null;
            }
            LevelScriptBase script = LevelHelper.GetLevelScript(select.LevelScript);
            if (script == null)
            {
                U3D.PopupTip(string.Format("关卡脚本为空 关卡ID:{0}, 关卡脚本:{1}", select.ID, select.LevelScript));
                return;
            }
            U3D.LoadLevel(select, LevelMode.CreateWorld, (GameMode)Main.Ins.GameStateMgr.gameStatus.Single.Mode);
        }
    }
}