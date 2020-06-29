using Idevgame.GameState.DialogState;
using protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomOptionDialogState : CommonDialogState<RoomOptionDialog>
{
    public override string DialogName { get { return "RoomOption"; } }
    public RoomOptionDialogState(MainDialogStateManager stateMgr) : base(stateMgr)
    {

    }
}

public class RoomOptionDialog : Dialog
{
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
    }

    GameObject TemplateRoot;

    int[] ConstRoundTime = { 15, 30, 60 };
    int[] ConstPlayer = { 2, 4, 8, 12, 16 };
    protocol.RoomInfo.RoomPattern[] ConstPattern = { protocol.RoomInfo.RoomPattern._Normal, protocol.RoomInfo.RoomPattern._Record, RoomInfo.RoomPattern._Replay };
    InputField roomName;
    InputField roomSecret;
    void Init()
    {
        roomName = Control("RoomNameInput").GetComponent<UnityEngine.UI.InputField>();
        roomName.text = string.Format("{0}{1}", Main.Ins.GameStateMgr.gameStatus.NickName, "的房间");
        roomSecret = Control("RoomSecretInput").GetComponent<UnityEngine.UI.InputField>();
        roomSecret.text = "";
        Control("CreateWorld").GetComponent<Button>().onClick.AddListener(() =>
        {
            OnCreateRoom();
        });
        GameObject RuleGroup = Control("RuleGroup", WndObject);
        Toggle rule0 = Control("0", RuleGroup).GetComponent<Toggle>();
        Toggle rule1 = Control("1", RuleGroup).GetComponent<Toggle>();
        Toggle rule2 = Control("2", RuleGroup).GetComponent<Toggle>();
        rule0.isOn = Main.Ins.GameStateMgr.gameStatus.NetWork.Mode == (int)GameMode.MENGZHU;
        rule1.isOn = Main.Ins.GameStateMgr.gameStatus.NetWork.Mode == (int)GameMode.ANSHA;
        rule2.isOn = Main.Ins.GameStateMgr.gameStatus.NetWork.Mode == (int)GameMode.SIDOU;

        rule0.onValueChanged.AddListener((bool select) => { if (select) Main.Ins.GameStateMgr.gameStatus.NetWork.Mode = (int)GameMode.MENGZHU; });
        rule1.onValueChanged.AddListener((bool select) => { if (select) Main.Ins.GameStateMgr.gameStatus.NetWork.Mode = (int)GameMode.ANSHA; });
        rule2.onValueChanged.AddListener((bool select) => { if (select) Main.Ins.GameStateMgr.gameStatus.NetWork.Mode = (int)GameMode.SIDOU; });

        GameObject LifeGroup = Control("LifeGroup", WndObject);
        Toggle Life0 = Control("0", LifeGroup).GetComponent<Toggle>();
        Toggle Life1 = Control("1", LifeGroup).GetComponent<Toggle>();
        Toggle Life2 = Control("2", LifeGroup).GetComponent<Toggle>();

        Life0.isOn = Main.Ins.GameStateMgr.gameStatus.NetWork.Life == 500;
        Life1.isOn = Main.Ins.GameStateMgr.gameStatus.NetWork.Life == 200;
        Life2.isOn = Main.Ins.GameStateMgr.gameStatus.NetWork.Life == 100;
        Life0.onValueChanged.AddListener((bool select) => { if (select) Main.Ins.GameStateMgr.gameStatus.NetWork.Life = 500; });
        Life1.onValueChanged.AddListener((bool select) => { if (select) Main.Ins.GameStateMgr.gameStatus.NetWork.Life = 200; });
        Life2.onValueChanged.AddListener((bool select) => { if (select) Main.Ins.GameStateMgr.gameStatus.NetWork.Life = 100; });

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
        select = Main.Ins.CombatData.GetLevel(Main.Ins.GameStateMgr.gameStatus.ChapterTemplate, Main.Ins.GameStateMgr.gameStatus.NetWork.LevelTemplate);
        OnSelectLevel(select);

        GameObject TimeGroup = Control("GameTime", WndObject);
        for (int i = 0; i < 3; i++)
        {
            Toggle TimeToggle = Control(string.Format("{0}", i), TimeGroup).GetComponent<Toggle>();
            var k = i;
            TimeToggle.isOn = Main.Ins.GameStateMgr.gameStatus.NetWork.RoundTime == ConstRoundTime[k];
            TimeToggle.onValueChanged.AddListener((bool selected) => { if (selected) Main.Ins.GameStateMgr.gameStatus.NetWork.RoundTime = ConstRoundTime[k]; });
        }

        GameObject PlayerGroup = Control("PlayerGroup", WndObject);
        for (int i = 0; i <= 4; i++)
        {
            Toggle PlayerToggle = Control(string.Format("{0}", i), PlayerGroup).GetComponent<Toggle>();
            var k = i;
            PlayerToggle.isOn = Main.Ins.GameStateMgr.gameStatus.NetWork.MaxPlayer == ConstPlayer[k];
            PlayerToggle.onValueChanged.AddListener((bool selected) =>
            {
                if (selected)
                    Main.Ins.GameStateMgr.gameStatus.NetWork.MaxPlayer = ConstPlayer[k];
            });
        }
    }

    LevelDatas.LevelDatas select;
    void OnSelectLevel(LevelDatas.LevelDatas lev)
    {
        select = lev;
        Main.Ins.GameStateMgr.gameStatus.NetWork.LevelTemplate = lev.ID;
        Control("Task").GetComponent<Text>().text = select.Name;
    }

    void OnCreateRoom()
    {
        if (select != null)
        {
            //Global.Instance.PlayerLife = GameData.Instance.gameStatus.NetWork.Life;
            //Global.Instance.RoundTime = GameData.Instance.gameStatus.NetWork.RoundTime;
            //Global.Instance.MaxPlayer = GameData.Instance.gameStatus.NetWork.MaxPlayer;
            if (string.IsNullOrEmpty(roomName.text))
            {
                U3D.PopupTip("需要设置房间名");
                return;
            }
            Main.Ins.GameStateMgr.gameStatus.NetWork.RoomName = roomName.text;
            Common.CreateRoom(roomName.text, roomSecret.text);
            //U3D.LoadLevel(select.ID, LevelMode.CreateWorld, (GameMode)GameData.Instance.gameStatus.NetWork.Mode);
        }
    }
}