using Excel2Json;
using Idevgame.GameState.DialogState;
using protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomDialogState : CommonDialogState<CreateRoomDialog>
{
    public override string DialogName { get { return "CreateRoom"; } }
    public CreateRoomDialogState(MainDialogMgr stateMgr) : base(stateMgr)
    {

    }
}

public class CreateRoomDialog : Dialog
{
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
    }

    GameObject TemplateRoot;

    int[] ConstRoundTime = { 15, 30, 60 };
    int[] ConstPlayer = { 2, 4, 8, 12, 16 };
    protocol.RoomInfo.RoomPattern[] ConstPattern = { protocol.RoomInfo.RoomPattern._Normal, RoomInfo.RoomPattern._Replay };
    InputField roomName;
    InputField roomSecret;
    Dictionary<string, Button> levelBtns = new Dictionary<string, Button>();
    Button selectedBtn;
    void Init()
    {
        roomName = Control("RoomNameInput").GetComponent<UnityEngine.UI.InputField>();
        roomName.text = string.Format("{0}{1}", GameStateMgr.Ins.gameStatus.NickName, "的房间");
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
        rule0.isOn = GameStateMgr.Ins.gameStatus.NetWork.Mode == (int)GameMode.MENGZHU;
        rule1.isOn = GameStateMgr.Ins.gameStatus.NetWork.Mode == (int)GameMode.ANSHA;
        rule2.isOn = GameStateMgr.Ins.gameStatus.NetWork.Mode == (int)GameMode.SIDOU;

        rule0.onValueChanged.AddListener((bool select) => { if (select) GameStateMgr.Ins.gameStatus.NetWork.Mode = (int)GameMode.MENGZHU; });
        rule1.onValueChanged.AddListener((bool select) => { if (select) GameStateMgr.Ins.gameStatus.NetWork.Mode = (int)GameMode.ANSHA; });
        rule2.onValueChanged.AddListener((bool select) => { if (select) GameStateMgr.Ins.gameStatus.NetWork.Mode = (int)GameMode.SIDOU; });

        GameObject LifeGroup = Control("LifeGroup", WndObject);
        Toggle Life0 = Control("0", LifeGroup).GetComponent<Toggle>();
        Toggle Life1 = Control("1", LifeGroup).GetComponent<Toggle>();
        Toggle Life2 = Control("2", LifeGroup).GetComponent<Toggle>();

        Life0.isOn = GameStateMgr.Ins.gameStatus.NetWork.Life == 500;
        Life1.isOn = GameStateMgr.Ins.gameStatus.NetWork.Life == 200;
        Life2.isOn = GameStateMgr.Ins.gameStatus.NetWork.Life == 100;
        Life0.onValueChanged.AddListener((bool select) => { if (select) GameStateMgr.Ins.gameStatus.NetWork.Life = 500; });
        Life1.onValueChanged.AddListener((bool select) => { if (select) GameStateMgr.Ins.gameStatus.NetWork.Life = 200; });
        Life2.onValueChanged.AddListener((bool select) => { if (select) GameStateMgr.Ins.gameStatus.NetWork.Life = 100; });

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
        select = CombatData.Ins.GetLevel(GameStateMgr.Ins.gameStatus.ChapterTemplate, GameStateMgr.Ins.gameStatus.NetWork.LevelTemplate);
        for (int i = 0; i < TemplateRoot.transform.childCount; i++) {
            Transform tri = TemplateRoot.transform.GetChild(i);
            Button btn = tri.GetComponent<Button>();
            levelBtns.Add(tri.name, btn);
        }
        OnSelectLevel(select);
        GameObject TimeGroup = Control("GameTime", WndObject);
        for (int i = 0; i < 3; i++)
        {
            Toggle TimeToggle = Control(string.Format("{0}", i), TimeGroup).GetComponent<Toggle>();
            var k = i;
            TimeToggle.isOn = GameStateMgr.Ins.gameStatus.NetWork.RoundTime == ConstRoundTime[k];
            TimeToggle.onValueChanged.AddListener((bool selected) => { if (selected) GameStateMgr.Ins.gameStatus.NetWork.RoundTime = ConstRoundTime[k]; });
        }

        GameObject PlayerGroup = Control("PlayerGroup", WndObject);
        for (int i = 0; i <= 4; i++)
        {
            Toggle PlayerToggle = Control(string.Format("{0}", i), PlayerGroup).GetComponent<Toggle>();
            var k = i;
            PlayerToggle.isOn = GameStateMgr.Ins.gameStatus.NetWork.MaxPlayer == ConstPlayer[k];
            PlayerToggle.onValueChanged.AddListener((bool selected) =>
            {
                if (selected)
                    GameStateMgr.Ins.gameStatus.NetWork.MaxPlayer = ConstPlayer[k];
            });
        }

        GameObject PatternGroup = Control("PatternGroup", WndObject);
        for (int i = 0; i <= 1; i++) {
            Toggle PatternToggle = Control(string.Format("{0}", i), PatternGroup).GetComponent<Toggle>();
            var k = i;
            PatternToggle.isOn = GameStateMgr.Ins.gameStatus.NetWork.Pattern == (int)ConstPattern[k];
            PatternToggle.onValueChanged.AddListener((bool selected) => {
                if (selected)
                    GameStateMgr.Ins.gameStatus.NetWork.Pattern = (int)ConstPattern[k];
            });
        }

        GameRecord r = GameStateMgr.Ins.gameStatus.NetWork.record;
        if (r != null) {
            Control("Screen").GetComponent<Image>().sprite = r.LoadTexture();
            Control("RecordName").GetComponent<Text>().text = r.Name;
        }

        Control("Screen").GetComponent<Button>().onClick.AddListener(() => {
            U3D.InsertSystemMsg("录像转播已被屏蔽");
            //if (!RecordSelectState.Exist()) {
            //    U3D.InsertSystemMsg("选择要转播的录像");
            //    Main.Ins.EnterState(Main.Ins.RecordSelectState);
            //}
        });
    }

    LevelData select;
    void OnSelectLevel(LevelData lev)
    {
        if (selectedBtn != null) {
            selectedBtn.image.color = new Color(1, 1, 1, 0);
            selectedBtn = null;
        }
        selectedBtn = levelBtns[lev.Name];
        selectedBtn.image.color = new Color(144.0f / 255.0f, 104.0f / 255.0f, 104.0f / 255.0f, 104.0f / 255.0f);

        select = lev;
        GameStateMgr.Ins.gameStatus.NetWork.LevelTemplate = lev.Id;
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

            if (GameStateMgr.Ins.gameStatus.NetWork.Pattern == (int)RoomInfo.RoomPattern._Replay) {
                U3D.PopupTip("暂不支持");
                return;
            }

            //如果是-录像模式-弹出二级界面-选择已存在的录像
            if (GameStateMgr.Ins.gameStatus.NetWork.Pattern == (int)RoomInfo.RoomPattern._Replay && GameStateMgr.Ins.gameStatus.NetWork.record == null) {
                U3D.InsertSystemMsg("选择要转播的录像");
                RecordSelectState.State.Open();
                return;
            } 
            else
            {

            }
            //如果是-
            GameStateMgr.Ins.gameStatus.NetWork.RoomName = roomName.text;
            TcpClientProxy.Ins.CreateRoom(roomName.text, roomSecret.text);
            //U3D.LoadLevel(select.ID, LevelMode.CreateWorld, (GameMode)GameData.Instance.gameStatus.NetWork.Mode);
        }
    }

    public override void OnRefresh(int message, object param = null) {
        if (message == 1) {
            GameRecord r = param as GameRecord;
            Control("Screen").GetComponent<Image>().sprite = r.LoadTexture();
            Control("RecordName").GetComponent<Text>().text = r.Name;
            OnCreateRoom();
        }
    }
}