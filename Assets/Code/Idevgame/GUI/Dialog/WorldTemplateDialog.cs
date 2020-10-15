using Excel2Json;
using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class WorldTemplateDialogState : CommonDialogState<WorldTemplateDialog>
{
    public override string DialogName { get { return "WorldTemplate"; } }
    public WorldTemplateDialogState(MainDialogMgr stateMgr) : base(stateMgr)
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
        rule0.isOn = GameStateMgr.Ins.gameStatus.Single.Mode == (int)GameMode.MENGZHU;
        rule1.isOn = GameStateMgr.Ins.gameStatus.Single.Mode == (int)GameMode.ANSHA;
        rule2.isOn = GameStateMgr.Ins.gameStatus.Single.Mode == (int)GameMode.SIDOU;

        rule0.onValueChanged.AddListener((bool select) => { if (select) GameStateMgr.Ins.gameStatus.Single.Mode = (int)GameMode.MENGZHU; });
        rule1.onValueChanged.AddListener((bool select) => { if (select) GameStateMgr.Ins.gameStatus.Single.Mode = (int)GameMode.ANSHA; });
        rule2.onValueChanged.AddListener((bool select) => { if (select) GameStateMgr.Ins.gameStatus.Single.Mode = (int)GameMode.SIDOU; });

        GameObject LifeGroup = Control("LifeGroup", WndObject);
        Toggle Life0 = Control("0", LifeGroup).GetComponent<Toggle>();
        Toggle Life1 = Control("1", LifeGroup).GetComponent<Toggle>();
        Toggle Life2 = Control("2", LifeGroup).GetComponent<Toggle>();

        Life0.isOn = GameStateMgr.Ins.gameStatus.Single.Life == 500;
        Life1.isOn = GameStateMgr.Ins.gameStatus.Single.Life == 200;
        Life2.isOn = GameStateMgr.Ins.gameStatus.Single.Life == 100;
        Life0.onValueChanged.AddListener((bool select) => { if (select) GameStateMgr.Ins.gameStatus.Single.Life = 500; });
        Life1.onValueChanged.AddListener((bool select) => { if (select) GameStateMgr.Ins.gameStatus.Single.Life = 200; });
        Life2.onValueChanged.AddListener((bool select) => { if (select) GameStateMgr.Ins.gameStatus.Single.Life = 100; });

        GameObject MainWeaponGroup = Control("FirstWeapon", WndObject);
        GameObject WeaponGroup = Control("WeaponGroup", MainWeaponGroup);
        for (int i = 0; i <= 11; i++)
        {
            Toggle MainWeapon = Control(string.Format("{0}", i), WeaponGroup).GetComponent<Toggle>();
            MainWeapon.isOn = GameStateMgr.Ins.gameStatus.Single.Weapon0 == i;
            MainWeapon.onValueChanged.AddListener(OnMainWeaponSelected);
        }

        GameObject SubWeaponGroup = Control("SubWeapon", WndObject);
        WeaponGroup = Control("WeaponGroup", SubWeaponGroup);
        for (int i = 0; i <= 11; i++)
        {
            Toggle subWeapon = Control(string.Format("{0}", i), WeaponGroup).GetComponent<Toggle>();
            subWeapon.isOn = GameStateMgr.Ins.gameStatus.Single.Weapon1 == i;
            subWeapon.onValueChanged.AddListener(OnSubWeaponSelected);
        }

        Control("Return").GetComponent<Button>().onClick.AddListener(() =>
        {
            GameStateMgr.Ins.SaveState();
            OnPreviousPress();
        });

        //地图模板，应该从所有地图表里获取，包括外部载入的地图.
        TemplateRoot = Control("WorldRoot", WndObject);
        List<LevelData> baseLevel = DataMgr.Ins.GetLevelDatas();
        LevelData[] allLevel = baseLevel.ToArray();
        for (int i = 0; i < allLevel.Length; i++)
        {
            LevelData lev = allLevel[i];
            if (lev == null)
                continue;
            Idevgame.Util.LevelUtils.AddGridItem(lev, TemplateRoot.transform, OnSelectLevel);
        }
        select = CombatData.Ins.GetLevel(GameStateMgr.Ins.gameStatus.ChapterTemplate, GameStateMgr.Ins.gameStatus.Single.LevelTemplate);
        for (int i = 0; i < TemplateRoot.transform.childCount; i++) {
            Transform tri = TemplateRoot.transform.GetChild(i);
            Button btn = tri.GetComponent<Button>();
            levelBtns.Add(tri.name, btn);
        }
        OnSelectLevel(select);

        GameObject ModelGroup = Control("ModelGroup");
        for (int i = 0; i < 20; i++)
        {
            Toggle modelTog = Control(string.Format("{0}", i), ModelGroup).GetComponent<Toggle>();
            Text t = modelTog.GetComponentInChildren<Text>();
            t.text = DataMgr.Ins.GetModelDatas()[i].Name;
            var k = i;
            modelTog.isOn = GameStateMgr.Ins.gameStatus.Single.Model == i;
            modelTog.onValueChanged.AddListener((bool select) => { if (select) GameStateMgr.Ins.gameStatus.Single.Model = k; });
        }

        GameObject TimeGroup = Control("GameTime", WndObject);
        for (int i = 0; i < 3; i++)
        {
            Toggle TimeToggle = Control(string.Format("{0}", i), TimeGroup).GetComponent<Toggle>();
            var k = i;
            TimeToggle.isOn = GameStateMgr.Ins.gameStatus.Single.RoundTime == ConstRoundTime[k];
            TimeToggle.onValueChanged.AddListener((bool selected) => { if (selected) GameStateMgr.Ins.gameStatus.Single.RoundTime = ConstRoundTime[k]; });
        }

        GameObject PlayerGroup = Control("PlayerGroup", WndObject);
        for (int i = 0; i <= 4; i++)
        {
            Toggle PlayerToggle = Control(string.Format("{0}", i), PlayerGroup).GetComponent<Toggle>();
            var k = i;
            PlayerToggle.isOn = GameStateMgr.Ins.gameStatus.Single.MaxPlayer == ConstPlayer[k];
            PlayerToggle.onValueChanged.AddListener((bool selected) =>
            {
                if (selected)
                    GameStateMgr.Ins.gameStatus.Single.MaxPlayer = ConstPlayer[k];
            });
        }

        GameObject DisallowGroup = Control("DisallowGroup", WndObject);
        Toggle DisallowToggle = Control("0", DisallowGroup).GetComponent<Toggle>();
        DisallowToggle.isOn = GameStateMgr.Ins.gameStatus.Single.DisallowSpecialWeapon;
        DisallowToggle.onValueChanged.AddListener((bool selected) => { GameStateMgr.Ins.gameStatus.Single.DisallowSpecialWeapon = selected; });
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
                    GameStateMgr.Ins.gameStatus.Single.Weapon0 = i;
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
                    GameStateMgr.Ins.gameStatus.Single.Weapon1 = i;
                    break;
                }
            }
        }
    }

    LevelData select;
    Dictionary<string, Button> levelBtns = new Dictionary<string, Button>();
    Button selectedBtn;
    void OnSelectLevel(LevelData lev)
    {
        if (selectedBtn != null) {
            selectedBtn.image.color = new Color(1, 1, 1, 0);
            selectedBtn = null;
        }
        selectedBtn = levelBtns[lev.Name];
        selectedBtn.image.color = new Color(144.0f / 255.0f, 104.0f / 255.0f, 104.0f / 255.0f, 104.0f / 255.0f);

        select = lev;
        GameStateMgr.Ins.gameStatus.Single.LevelTemplate = lev.Id;
        Control("Task").GetComponent<Text>().text = select.Name;
    }

    void OnEnterLevel()
    {
        if (select != null)
        {
            CombatData.Ins.MainWeapon = GameStateMgr.Ins.gameStatus.Single.Weapon0;
            CombatData.Ins.SubWeapon = GameStateMgr.Ins.gameStatus.Single.Weapon1;
            CombatData.Ins.PlayerLife = GameStateMgr.Ins.gameStatus.Single.Life;
            CombatData.Ins.PlayerModel = GameStateMgr.Ins.gameStatus.Single.Model;
            CombatData.Ins.RoundTime = GameStateMgr.Ins.gameStatus.Single.RoundTime;
            CombatData.Ins.MaxPlayer = GameStateMgr.Ins.gameStatus.Single.MaxPlayer;
            bool isPluginLevel = true;
            List<LevelData> all = DataMgr.Ins.GetLevelDatas();
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
                CombatData.Ins.Chapter = DlcMng.Ins.FindChapterByLevel(select);
                GameStateMgr.Ins.gameStatus.ChapterTemplate = CombatData.Ins.Chapter.ChapterId;
                GameStateMgr.Ins.gameStatus.Single.LevelTemplate = select.Id;
            }
            else
            {
                GameStateMgr.Ins.gameStatus.ChapterTemplate = 0;
                GameStateMgr.Ins.gameStatus.Single.LevelTemplate = select.Id;
                CombatData.Ins.Chapter = null;
            }
            LevelScriptBase script = LevelHelper.GetLevelScript(select.LevelScript);
            if (script == null)
            {
                U3D.PopupTip(string.Format("关卡脚本为空 关卡ID:{0}, 关卡脚本:{1}", select.Id, select.LevelScript));
                return;
            }
            U3D.LoadLevel(select, LevelMode.CreateWorld, (GameMode)GameStateMgr.Ins.gameStatus.Single.Mode);
        }
    }
}