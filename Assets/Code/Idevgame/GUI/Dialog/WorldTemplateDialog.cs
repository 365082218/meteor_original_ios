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
        rule0.isOn = GameData.Instance.gameStatus.Single.Mode == (int)GameMode.MENGZHU;
        rule1.isOn = GameData.Instance.gameStatus.Single.Mode == (int)GameMode.ANSHA;
        rule2.isOn = GameData.Instance.gameStatus.Single.Mode == (int)GameMode.SIDOU;

        rule0.onValueChanged.AddListener((bool select) => { if (select) GameData.Instance.gameStatus.Single.Mode = (int)GameMode.MENGZHU; });
        rule1.onValueChanged.AddListener((bool select) => { if (select) GameData.Instance.gameStatus.Single.Mode = (int)GameMode.ANSHA; });
        rule2.onValueChanged.AddListener((bool select) => { if (select) GameData.Instance.gameStatus.Single.Mode = (int)GameMode.SIDOU; });

        GameObject LifeGroup = Control("LifeGroup", WndObject);
        Toggle Life0 = Control("0", LifeGroup).GetComponent<Toggle>();
        Toggle Life1 = Control("1", LifeGroup).GetComponent<Toggle>();
        Toggle Life2 = Control("2", LifeGroup).GetComponent<Toggle>();

        Life0.isOn = GameData.Instance.gameStatus.Single.Life == 500;
        Life1.isOn = GameData.Instance.gameStatus.Single.Life == 200;
        Life2.isOn = GameData.Instance.gameStatus.Single.Life == 100;
        Life0.onValueChanged.AddListener((bool select) => { if (select) GameData.Instance.gameStatus.Single.Life = 500; });
        Life1.onValueChanged.AddListener((bool select) => { if (select) GameData.Instance.gameStatus.Single.Life = 200; });
        Life2.onValueChanged.AddListener((bool select) => { if (select) GameData.Instance.gameStatus.Single.Life = 100; });

        GameObject MainWeaponGroup = Control("FirstWeapon", WndObject);
        GameObject WeaponGroup = Control("WeaponGroup", MainWeaponGroup);
        for (int i = 0; i <= 11; i++)
        {
            Toggle MainWeapon = Control(string.Format("{0}", i), WeaponGroup).GetComponent<Toggle>();
            MainWeapon.isOn = GameData.Instance.gameStatus.Single.Weapon0 == i;
            MainWeapon.onValueChanged.AddListener(OnMainWeaponSelected);
        }

        GameObject SubWeaponGroup = Control("SubWeapon", WndObject);
        WeaponGroup = Control("WeaponGroup", SubWeaponGroup);
        for (int i = 0; i <= 11; i++)
        {
            Toggle subWeapon = Control(string.Format("{0}", i), WeaponGroup).GetComponent<Toggle>();
            subWeapon.isOn = GameData.Instance.gameStatus.Single.Weapon1 == i;
            subWeapon.onValueChanged.AddListener(OnSubWeaponSelected);
        }

        Control("Return").GetComponent<Button>().onClick.AddListener(() =>
        {
            GameData.Instance.SaveState();
            OnPreviousPress();
        });

        //地图模板，应该从所有地图表里获取，包括外部载入的地图.
        TemplateRoot = Control("WorldRoot", WndObject);
        Level[] allLevel = Global.Instance.GetAllLevel();
        for (int i = 0; i < allLevel.Length; i++)
        {
            Level lev = allLevel[i];
            if (lev == null)
                continue;
            AddGridItem(lev, TemplateRoot.transform);
        }
        select = Global.Instance.GetLevel(GameData.Instance.gameStatus.ChapterTemplate, GameData.Instance.gameStatus.Single.LevelTemplate);
        OnSelectLevel(select);

        GameObject ModelGroup = Control("ModelGroup");
        for (int i = 0; i < 20; i++)
        {
            Toggle modelTog = Control(string.Format("{0}", i), ModelGroup).GetComponent<Toggle>();
            Text t = modelTog.GetComponentInChildren<Text>();
            t.text = ModelMng.Instance.GetAllItem()[i].Name;
            var k = i;
            modelTog.isOn = GameData.Instance.gameStatus.Single.Model == i;
            modelTog.onValueChanged.AddListener((bool select) => { if (select) GameData.Instance.gameStatus.Single.Model = k; });
        }

        GameObject TimeGroup = Control("GameTime", WndObject);
        for (int i = 0; i < 3; i++)
        {
            Toggle TimeToggle = Control(string.Format("{0}", i), TimeGroup).GetComponent<Toggle>();
            var k = i;
            TimeToggle.isOn = GameData.Instance.gameStatus.Single.RoundTime == ConstRoundTime[k];
            TimeToggle.onValueChanged.AddListener((bool selected) => { if (selected) GameData.Instance.gameStatus.Single.RoundTime = ConstRoundTime[k]; });
        }

        GameObject PlayerGroup = Control("PlayerGroup", WndObject);
        for (int i = 0; i <= 4; i++)
        {
            Toggle PlayerToggle = Control(string.Format("{0}", i), PlayerGroup).GetComponent<Toggle>();
            var k = i;
            PlayerToggle.isOn = GameData.Instance.gameStatus.Single.MaxPlayer == ConstPlayer[k];
            PlayerToggle.onValueChanged.AddListener((bool selected) =>
            {
                if (selected)
                    GameData.Instance.gameStatus.Single.MaxPlayer = ConstPlayer[k];
            });
        }

        GameObject DisallowGroup = Control("DisallowGroup", WndObject);
        Toggle DisallowToggle = Control("0", DisallowGroup).GetComponent<Toggle>();
        DisallowToggle.isOn = GameData.Instance.gameStatus.Single.DisallowSpecialWeapon;
        DisallowToggle.onValueChanged.AddListener((bool selected) => { GameData.Instance.gameStatus.Single.DisallowSpecialWeapon = selected; });
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
                    GameData.Instance.gameStatus.Single.Weapon0 = i;
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
                    GameData.Instance.gameStatus.Single.Weapon1 = i;
                    break;
                }
            }
        }
    }

    Level select;
    void OnSelectLevel(Level lev)
    {
        select = lev;
        GameData.Instance.gameStatus.Single.LevelTemplate = lev.ID;
        Control("Task").GetComponent<Text>().text = select.Name;
    }

    void OnEnterLevel()
    {
        if (select != null)
        {
            Global.Instance.MainWeapon = GameData.Instance.gameStatus.Single.Weapon0;
            Global.Instance.SubWeapon = GameData.Instance.gameStatus.Single.Weapon1;
            Global.Instance.PlayerLife = GameData.Instance.gameStatus.Single.Life;
            Global.Instance.PlayerModel = GameData.Instance.gameStatus.Single.Model;
            Global.Instance.RoundTime = GameData.Instance.gameStatus.Single.RoundTime;
            Global.Instance.MaxPlayer = GameData.Instance.gameStatus.Single.MaxPlayer;
            U3D.LoadLevel(select.ID, LevelMode.CreateWorld, (GameMode)GameData.Instance.gameStatus.Single.Mode);
        }
    }

    void AddGridItem(Level lev, Transform parent)
    {
        GameObject objPrefab = Resources.Load("LevelSelectItem", typeof(GameObject)) as GameObject;
        GameObject obj = GameObject.Instantiate(objPrefab) as GameObject;
        obj.transform.SetParent(parent);
        obj.name = lev.Name;
        obj.GetComponent<Button>().onClick.AddListener(() => { OnSelectLevel(lev); });
        obj.GetComponentInChildren<Text>().text = lev.Name;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;
    }
}