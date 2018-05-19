using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using CoClass;
using System.IO;
using ProtoBuf;
using System;
using protocol;

public class GunShootUI:Window<GunShootUI>
{
    public override string PrefabName
    {
        get
        {
            return "GunShootUI";
        }
    }
}

public class MainLobby : Window<MainLobby>
{
    public override string PrefabName { get { return "MainLobby"; } }
    GameObject RoomRoot;
    List<GameObject> rooms = new List<GameObject>();
    int SelectRoomId = -1;
    Button selectedBtn;
    protected override bool OnOpen()
    {
        Init();
        return base.OnOpen();
    }

    protected override bool OnClose()
    {
        return base.OnClose();
    }

    public void OnGetRoom(GetRoomRsp rsp)
    {
        SelectRoomId = -1;
        int cnt = rsp.RoomInLobby.Count;
        for (int i = 0; i < rooms.Count; i++)
            GameObject.DestroyImmediate(rooms[i]);
        rooms.Clear();
        GameObject prefab = Resources.Load<GameObject>("RoomInfoItem");
        for (int i = 0; i < cnt; i++)
            InsertRoomItem(rsp.RoomInLobby[i], prefab);
    }
    string[] ruleS = new string[3] { "盟主", "死斗", "暗杀" };
    public void InsertRoomItem(RoomInfo room, GameObject prefab)
    {
        GameObject roomObj = GameObject.Instantiate(prefab, RoomRoot.transform);
        roomObj.layer = RoomRoot.layer;
        roomObj.transform.localPosition = Vector3.zero;
        roomObj.transform.localScale = Vector3.one;
        roomObj.transform.rotation = Quaternion.identity;
        roomObj.transform.SetParent(RoomRoot.transform);
        Control("Name", roomObj).GetComponent<Text>().text = room.roomName;
        Control("Pattern", roomObj).GetComponent<Text>().text = "";
        Control("Password", roomObj).GetComponent<Text>().text = "无";
        Control("Rule", roomObj).GetComponent<Text>().text = ruleS[(int)room.rule - 1];//盟主，死斗，暗杀
        Control("LevelName", roomObj).GetComponent<Text>().text = LevelMng.Instance.GetItem((int)room.levelIdx).Name;
        Control("Version", roomObj).GetComponent<Text>().text = AppInfo.MeteorVersion;
        Control("Ping", roomObj).GetComponent<Text>().text = "???";
        Control("Group1", roomObj).GetComponent<Text>().text = room.Group1.ToString();
        Control("Group2", roomObj).GetComponent<Text>().text = room.Group2.ToString();
        Control("PlayerCount", roomObj).GetComponent<Text>().text = room.playerCount.ToString() + "/" + room.maxPlayer;
        Button btn = roomObj.GetComponent<Button>();
        btn.onClick.AddListener(()=> { OnSelectRoom(room.roomId, btn); });
        rooms.Add(roomObj);
    }

    void OnSelectRoom(uint id, Button btn)
    {
        if (selectedBtn != null)
        {
            selectedBtn.image.color = new Color(1, 1, 1, 0);
            selectedBtn = null;
        }
        btn.image.color = new Color(144.0f / 255.0f, 104.0f / 255.0f, 104.0f / 255.0f, 104.0f / 255.0f);
        selectedBtn = btn;
        SelectRoomId = (int)id;
    }

    void Init()
    {
        Control("JoinRoom").GetComponent<Button>().onClick.AddListener(() =>
        {
            OnJoinRoom();
        });
        Control("CreateRoom").GetComponent<Button>().onClick.AddListener(() =>
        {
            OnCreateRoom();
        });
        Control("Refresh").GetComponent<Button>().onClick.AddListener(() =>
        {
            OnRefresh();
        });
        Control("Close").GetComponent<Button>().onClick.AddListener(() =>
        {
            OnGoback();
        });
        RoomRoot = Control("RoomRoot");
        ClientProxy.Init();
    }



    void OnGoback()
    {
        MainWnd.Instance.Open();
        ClientProxy.Exit();
        Close();
    }

    void OnRefresh()
    {

    }

    void OnCreateRoom()
    {

    }

    void OnJoinRoom()
    {
        if (SelectRoomId != -1)
        {
            ClientProxy.JoinRoom(SelectRoomId);
        }
    }
}

public class NickName : Window<NickName>
{
    public InputField Nick;
    public override string PrefabName { get { return "NickName"; } }
    protected override bool OnOpen()
    {
        Init();
        return base.OnOpen();
    }
    protected override bool FullStretch()
    {
        return false;
    }
    protected override bool OnClose()
    {
        return base.OnClose();
    }

    void Init()
    {
        Control("Yes").GetComponent<Button>().onClick.AddListener(() => {
            OnApply();
        });
        Control("Cancel").GetComponent<Button>().onClick.AddListener(() => {
            OnCancel();
        });
        Nick = Control("Nick").GetComponent<InputField>();
        if (string.IsNullOrEmpty(GameData.gameStatus.NickName))
            Nick.text = "流星杀手";
        else
            Nick.text = GameData.gameStatus.NickName;
    }

    void OnApply()
    {
        if (!string.IsNullOrEmpty(Nick.text))
        {
            GameData.gameStatus.NickName = Nick.text;
            Close();
        }
        else
            U3D.PopupTip("昵称不能为空");
        
    }

    void OnCancel()
    {
        Close();
    }
}

public class MainWnd : Window<MainWnd>
{
    MeteorUnit Unit;
    public int[] HeroId = { 0, 1 };
    public override string PrefabName { get { return "MainWnd"; } }
    protected override bool OnOpen()
    {
        Init();
        return base.OnOpen();
    }

    protected override bool OnClose()
    {
        return base.OnClose();
    }

	void Init()
	{
        Control("Version").GetComponent<Text>().text = AppInfo.MeteorVersion;
        Control("SinglePlayer").GetComponent<Button>().onClick.AddListener(()=> {
            OnSinglePlayer();
        });
        Control("MultiplePlayer").GetComponent<Button>().onClick.AddListener(() => {
            OnlineGame();
        });
        Control("PlayerSetting").GetComponent<Button>().onClick.AddListener(() => {
            OnSetting();
        });
        Control("QuitGame").GetComponent<Button>().onClick.AddListener(()=> {
            Application.Quit();
        });
        Control("UploadLog").GetComponent<Button>().onClick.AddListener(() => { FtpLog.UploadStart(); });
        Global.timeScale = 1;
        //Cursor.SetCursor(Resources.Load<Texture2D>("mCursor"), new Vector2(0, 0), CursorMode.Auto);
        //Cursor.visible = true;
    }

    void OnSinglePlayer()
    {
        MainMenu.Instance.Open();
    }

    void OnlineGame()
    {
        MainLobby.Instance.Open();
        Close();
    }

    void OnSetting()
    {
        NickName.Instance.Open();
    }
}

public class PlayerWnd:Window<PlayerWnd>
{
    public override string PrefabName  {  get {  return "PlayerWnd"; }  }
    protected override bool FullStretch(){return false;}
    protected override bool OnOpen()
    {
        Init();
        return base.OnOpen();
    }

    protected override int GetZ() { return 200; }
    protected override bool Use3DCanvas()
    {
        return true;
    }
    protected override bool OnClose()
    {
        return base.OnClose();
    }

    void Init()
    {
        RectTransform rectTran = WndObject.GetComponent<RectTransform>();
        if (rectTran != null)
            rectTran.anchoredPosition = new Vector2((1920f - rectTran.sizeDelta.x) / 2.0f, -(1080f - rectTran.sizeDelta.y) / 2.0f);
        GameObject obj = Global.ldaControlX("3DParent", WndObject);
        GameObject objPlayer = GameObject.Instantiate(Resources.Load("3DUIPlayer")) as GameObject;
        MeteorUnitDebug d = objPlayer.GetComponent<MeteorUnitDebug>();
        objPlayer.transform.position = Vector3.zero;
        objPlayer.transform.rotation = Quaternion.identity;
        objPlayer.transform.localScale = Vector3.one;
        d.gameObject.layer = obj.gameObject.layer;
        d.Init(0);
        WeaponBase weaponProperty = WeaponMng.Instance.GetItem(MeteorManager.Instance.LocalPlayer.weaponLoader.GetCurrentWeapon().Info().UnitId);
        d.weaponLoader.StrWeaponR = weaponProperty.WeaponR;
        d.weaponLoader.StrWeaponL = weaponProperty.WeaponL;
        d.weaponLoader.EquipWeapon();
        d.transform.SetParent(obj.transform);
        d.transform.localScale = 8 * Vector3.one;
        d.transform.localPosition = new Vector3(0, 0, -45);
        d.transform.localRotation = Quaternion.identity;
        Global.ldaControlX("Close Button", WndObject).GetComponent<Button>().onClick.AddListener(Close);

        SetStat("Stat Label 1", MeteorManager.Instance.LocalPlayer.Attr.hpCur + "/" + MeteorManager.Instance.LocalPlayer.Attr.HpMax);
        SetStat("Stat Label 2", MeteorManager.Instance.LocalPlayer.AngryValue.ToString());
        SetStat("Stat Label 3", MeteorManager.Instance.LocalPlayer.CalcDamage().ToString());
        SetStat("Stat Label 4", MeteorManager.Instance.LocalPlayer.CalcDef().ToString());
        SetStat("Stat Label 5", MeteorManager.Instance.LocalPlayer.Speed.ToString());
        SetStat("Stat Label 6", string.Format("{0:f2}", MeteorManager.Instance.LocalPlayer.Speed / 1000.0f));

        //处理背包的点击
        UIItemSlot [] slots = Global.ldaControlX("Slots Grid", WndObject).GetComponentsInChildren<UIItemSlot>();
        for (int i = 0; i < slots.Length; i++)
            slots[i].onClick.AddListener(OnClickItem);
    }

    void OnClickItem(UIItemSlot slot)
    {
        if (ItemInfoWnd.Exist)
            ItemInfoWnd.Instance.Close();
        ItemInfoWnd.Instance.Open();
        ItemInfoWnd.Instance.AssignItem(slot.GetItemInfo());
    }

    void SetStat(string label, string value)
    {
        GameObject objStat = Global.ldaControlX(label, WndObject);
        GameObject objStatValue = Control("Stat Value", objStat);
        objStatValue.GetComponent<Text>().text = value;
    }
}

public class ItemInfoWnd:Window<ItemInfoWnd>
{
    public override string PrefabName { get { return "ItemInfoWnd"; } }
    public static UIItemInfo Item;
    public void AssignItem(UIItemInfo item)
    {
        Item = item;
    }
    protected override bool FullStretch() { return false; }
    protected override bool OnOpen()
    {
        Init();
        return base.OnOpen();
    }

    protected override bool OnClose()
    {
        return base.OnClose();
    }

    void Init()
    {

    }
}

public class DebugWnd:Window<DebugWnd>
{
    public GameObject FunctionRoot;
    public GameObject WeaponRoot;
    //public 
    public override string PrefabName
    {
        get
        {
            return "DebugPanel";
        }
    }
    protected override bool OnOpen()
    {
        Init();
        return base.OnOpen();
    }

    protected override bool OnClose()
    {
        if (load != null && GameBattleEx.Instance)
            GameBattleEx.Instance.StopCoroutine(load);
        return base.OnClose();
    }

    Coroutine load;
    void Init()
    {
        FunctionRoot = Control("FunctionRoot");
        WeaponRoot = Control("WeaponRoot");
        Control("Close").GetComponent<Button>().onClick.AddListener(Close);
        if (load == null)
            load = GameBattleEx.Instance.StartCoroutine(AddRobotAndWeapon());
    }

    IEnumerator AddRobotAndWeapon()
    {
        for (int i = 0; i < Global.model.Length; i++)
        {
            AddSpawnItem(i);
            yield return 0;
        }
        List<ItemBase> we = GameData.itemMng.GetFullRow();
        for (int i = 0; i < we.Count; i++)
        {
            if (we[i].MainType == 1)
            {
                AddWeaponItem(we[i]);
                yield return 0;
            }
        }
    }

    void AddSpawnItem(int Idx)
    {
        UIFunCtrl obj = (GameObject.Instantiate(Resources.Load("UIFuncItem")) as GameObject).GetComponent<UIFunCtrl>();
        obj.SetEvent(SpawnMonster, Idx);
        obj.SetText(Global.model[Idx]);
        obj.transform.SetParent(FunctionRoot.transform);
        obj.gameObject.layer = FunctionRoot.layer;
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
    }

    void AddWeaponItem(ItemBase it)
    {
        UIFunCtrl obj = (GameObject.Instantiate(Resources.Load("UIFuncItem")) as GameObject).GetComponent<UIFunCtrl>();
        obj.SetEvent(ChangeWeaponCode, it.Idx);
        obj.SetText(it.Name);
        obj.transform.SetParent(WeaponRoot.transform);
        obj.gameObject.layer = WeaponRoot.layer;
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
    }

    void ChangeWeaponCode(int code)
    {
        MeteorManager.Instance.LocalPlayer.ChangeWeaponCode(code);
    }
    void SpawnMonster(int code)
    {
        U3D.SpawnRobot(code);
    }
}
public class BattleResultWnd : Window<BattleResultWnd>
{
    public override string PrefabName
    {
        get
        {
            return "BattleResultWnd";
        }
    }

    protected override bool OnOpen()
    {
        Init();
        return base.OnOpen();
    }

    protected override bool OnClose()
    {
        return base.OnClose();
    }

    GameObject BattleResult;
    GameObject BattleTitle;
    Transform MeteorResult;
    Transform ButterflyResult;

    public IEnumerator SetResult(int result)
    {
        yield return new WaitForSeconds(3.0f);
        
        string mat = "";
        Text txt;
        switch (result)
        {
            case -1:
            case 0:
                mat = "BattleLose";
                txt = Control("ButterflyWin").GetComponent<Text>();
                U3D.InsertSystemMsg("蝴蝶阵营 获胜");
                txt.text = "1";
                break;
            case 1:
                mat = "BattleWin";
                txt = Control("MeteorWin").GetComponent<Text>();
                U3D.InsertSystemMsg("流星阵营 获胜");
                txt.text = "1";
                break;
            case 2:
                mat = "BattleNone";
                U3D.InsertSystemMsg("和局");
                break;

        }
        BattleResult.GetComponent<Image>().material = Resources.Load<Material>(mat);
        BattleResult.SetActive(true);
        BattleTitle.SetActive(true);
    }

    public void Init()
    {
        MeteorResult = Control("MeteorResult").transform;
        ButterflyResult = Control("ButterflyResult").transform;
        BattleResult = Global.ldaControlX("BattleResult", WndObject);
        BattleTitle = Global.ldaControlX("BattleTitle", WndObject);
        for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
        {
            if (GameBattleEx.Instance.BattleResult.ContainsKey(MeteorManager.Instance.UnitInfos[i].name))
            {
                InsertPlayerResult(MeteorManager.Instance.UnitInfos[i].name, GameBattleEx.Instance.BattleResult[MeteorManager.Instance.UnitInfos[i].name]);
                GameBattleEx.Instance.BattleResult.Remove(MeteorManager.Instance.UnitInfos[i].name);
            }
            else if (MeteorManager.Instance.UnitInfos[i].Camp == EUnitCamp.EUC_ENEMY || MeteorManager.Instance.UnitInfos[i].Camp == EUnitCamp.EUC_FRIEND)
                InsertPlayerResult(MeteorManager.Instance.UnitInfos[i].name, MeteorManager.Instance.UnitInfos[i].InstanceId, 0, 0, MeteorManager.Instance.UnitInfos[i].Camp);
        }

        foreach (var each in GameBattleEx.Instance.BattleResult)
            InsertPlayerResult(each.Key, each.Value);
        GameBattleEx.Instance.BattleResult.Clear();
    }

    void InsertPlayerResult(string name_, int id, int killed, int dead, EUnitCamp camp)
    {
        GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>("ResultItem")); ;
        obj.transform.SetParent(camp ==  EUnitCamp.EUC_FRIEND ? MeteorResult : ButterflyResult);
        obj.layer = MeteorResult.gameObject.layer;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = Vector3.zero;

        Text Idx = ldaControl("Idx", obj).GetComponent<Text>();
        Text Name = ldaControl("Name", obj).GetComponent<Text>();
        //Text Camp = ldaControl("Camp", obj).GetComponent<Text>();
        Text Killed = ldaControl("Killed", obj).GetComponent<Text>();
        Text Dead = ldaControl("Dead", obj).GetComponent<Text>();
        Idx.text = (id + 1).ToString();
        Name.text = name_;
        //Camp.text = result.camp == 1 ""
        Killed.text = killed.ToString();
        Dead.text = dead.ToString();
        MeteorUnit u = U3D.GetUnit(id);
        if (u != null)
        {
            if (u.Dead)
            {
                Idx.color = Color.red;
                Name.color = Color.red;
                Killed.color = Color.red;
                Dead.color = Color.red;
            }
        }
        else
        {
            //得不到信息了。说明该NPC被移除掉了
            Idx.color = Color.red;
            Name.color = Color.red;
            Killed.color = Color.red;
            Dead.color = Color.red;
        }
    }

    void InsertPlayerResult(string name_, GameBattleEx.BattleResultItem result)
    {
        GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>("ResultItem")); ;
        obj.transform.SetParent(result.camp == 1 ? MeteorResult : ButterflyResult);
        obj.layer = MeteorResult.gameObject.layer;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = Vector3.zero;

        Text Idx = ldaControl("Idx", obj).GetComponent<Text>();
        Text Name = ldaControl("Name", obj).GetComponent<Text>();
        //Text Camp = ldaControl("Camp", obj).GetComponent<Text>();
        Text Killed = ldaControl("Killed", obj).GetComponent<Text>();
        Text Dead = ldaControl("Dead", obj).GetComponent<Text>();
        Idx.text = (result.id + 1).ToString();
        Name.text = name_;
        //Camp.text = result.camp == 1 ""
        Killed.text = result.killCount.ToString();
        Dead.text = result.deadCount.ToString();
        MeteorUnit u = U3D.GetUnit(result.id);
        if (u != null)
        {
            if (u.Dead)
            {
                Idx.color = Color.red;
                Name.color = Color.red;
                Killed.color = Color.red;
                Dead.color = Color.red;
            }
        }
    }
}