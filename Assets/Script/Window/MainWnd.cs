using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using CoClass;
using System.IO;
using ProtoBuf;
using System;
using protocol;
using Idevgame.Util;

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

public class WorldTemplateWnd : Window<WorldTemplateWnd>
{
    public override string PrefabName
    {
        get
        {
            return "WorldTemplate";
        }
    }

    protected override bool OnOpen()
    {
        Init();
        return base.OnOpen();
    }

    void Init()
    {
        Control("CreateWorld").GetComponent<Button>().onClick.AddListener(() =>
        {
            OnCreateWorld();
        });
        //Control("CreateRoom").GetComponent<Button>().onClick.AddListener(() =>
        //{
        //    OnCreateRoom();
        //});
        //Control("Refresh").GetComponent<Button>().onClick.AddListener(() =>
        //{
        //    OnRefresh();
        //});
        //Control("Close").GetComponent<Button>().onClick.AddListener(() =>
        //{
        //    OnGoback();
        //});
        //RoomRoot = Control("RoomRoot");
        //ClientProxy.Init();
    }

    void OnCreateWorld()
    {

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
        WorldTemplateWnd.Instance.Open();
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
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
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

//角色信息面板去掉.以后再加
//这种界面，一般是全屏界面。
public class PlayerWnd:Window<PlayerWnd>
{
    public override string PrefabName  {  get {  return "PlayerWnd"; }  }
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
        d.Init(MeteorManager.Instance.LocalPlayer.UnitId, LayerMask.NameToLayer("3DUIPlayer"));
        WeaponBase weaponProperty = WeaponMng.Instance.GetItem(MeteorManager.Instance.LocalPlayer.weaponLoader.GetCurrentWeapon().Info().UnitId);
        d.weaponLoader.StrWeaponR = weaponProperty.WeaponR;
        d.weaponLoader.StrWeaponL = weaponProperty.WeaponL;
        //d.weaponLoader.EquipWeapon();
        d.transform.SetParent(obj.transform);
        d.transform.localScale = 8 * Vector3.one;
        d.transform.localPosition = new Vector3(0, 0, -300);
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

public class WeaponWnd : Window<WeaponWnd>
{
    public GameObject CameraForWeapon;
    public GameObject WeaponModelParent;
    public GameObject WeaponRoot;
    //public 
    public override string PrefabName
    {
        get
        {
            return "WeaponWnd";
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
        if (CameraForWeapon != null)
            GameObject.Destroy(CameraForWeapon);
        return base.OnClose();
    }

    Coroutine load;
    EquipWeaponType weaponSubType;
    WeaponLoader wload;
    int selectWeapon;
    void Init()
    {
        weaponSubType = EquipWeaponType.Sword;
        if (CameraForWeapon == null)
        {
            CameraForWeapon = GameObject.Instantiate(ResMng.LoadPrefab("CameraForWeapon")) as GameObject;
            CameraForWeapon.Identity(null);
            WeaponModelParent = Control("WeaponParent", CameraForWeapon);
            wload = WeaponModelParent.GetComponent<WeaponLoader>();
            wload.Init();
        }
        WeaponRoot = Control("WeaponRoot");
        Control("Equip").GetComponent<Button>().onClick.AddListener(()=> { ChangeWeaponCode(); });
        Control("Close").GetComponent<Button>().onClick.AddListener(Close);
        for (int i = 0; i < 12; i++)
        {
            string control = string.Format("Tab{0}", i);
            Control(control).GetComponent<UITab>().onValueChanged.AddListener(ChangeWeaponType);
        }
        if (load == null)
            load = GameBattleEx.Instance.StartCoroutine(AddWeapon());
    }

    IEnumerator AddWeapon()
    {
        List<ItemBase> we = GameData.itemMng.GetFullRow();
        int offset = 0;
        for (int i = 0; i < we.Count; i++)
        {
            if (we[i].MainType == 1)
            {
                if (we[i].SubType == (int)weaponSubType)
                {
                    AddWeaponItem(we[i], offset++);
                    yield return 0;
                }
            }
        }
        ShowWeapon();
    }

    List<GameObject> GridWeapon = new List<GameObject>();
    void AddWeaponItem(ItemBase it, int idx)
    {
        if (GridWeapon.Count > idx)
        {
            GridWeapon[idx].SetActive(true);
            UIFunCtrl ctrl = GridWeapon[idx].GetComponent<UIFunCtrl>();
            ctrl.SetEvent(ShowWeapon, it.Idx);
            ctrl.SetText(it.Name);
        }
        else
        {
            GameObject weapon = GameObject.Instantiate(Resources.Load("GridItemBtn")) as GameObject;
            UIFunCtrl obj = weapon.AddComponent<UIFunCtrl>();
            obj.SetEvent(ShowWeapon, it.Idx);
            obj.SetText(it.Name);
            obj.transform.SetParent(WeaponRoot.transform);
            obj.gameObject.layer = WeaponRoot.layer;
            obj.transform.localScale = Vector3.one;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            GridWeapon.Add(weapon);
        }
    }

    void ShowWeapon()
    {
        List<ItemBase> we = GameData.itemMng.GetFullRow();
        for (int i = 0; i < we.Count; i++)
        {
            if (we[i].MainType == 1)
            {
                if (we[i].SubType == (int)weaponSubType)
                {
                    selectWeapon = we[i].Idx;
                    break;
                }
            }
        }
        ShowWeapon(selectWeapon);
    }

    void ShowWeapon(int idx)
    {
        selectWeapon = idx;
        wload.EquipWeapon(selectWeapon);
    }

    void ChangeWeaponCode()
    {
        MeteorManager.Instance.LocalPlayer.ChangeWeaponCode(selectWeapon);
    }

    void ChangeWeaponType(bool change)
    {
        if (!change)
            return;
        for (int i = 0; i < 12; i++)
        {
            string control = string.Format("Tab{0}", i);
            if (Control(control).GetComponent<UITab>().isOn)
            {
                ChangeWeaponType(i);
                break;
            }
        }
        
    }

    void ChangeWeaponType(int subType)
    {
        weaponSubType = (EquipWeaponType)subType;
        for (int i = 0; i < GridWeapon.Count; i++)
            GridWeapon[i].SetActive(false);
        if (load != null)
            GameBattleEx.Instance.StopCoroutine(load);
        load = GameBattleEx.Instance.StartCoroutine(AddWeapon());
    }
}

public class RobotWnd : Window<RobotWnd>
{
    public GameObject RobotRoot;
    public override string PrefabName
    {
        get
        {
            return "RobotWnd";
        }
    }
    protected override bool OnOpen()
    {
        Init();
        return base.OnOpen();
    }

    protected override bool OnClose()
    {
        if (refresh != null && GameBattleEx.Instance)
            GameBattleEx.Instance.StopCoroutine(refresh);
        return base.OnClose();
    }

    int pageIndex = 0;
    private const int PageCount = 20;
    int pageMax = 0;
    int pageMin = 0;
    void Init()
    {
        pageMax = 2;
        RobotRoot = Control("Page");
        Control("Close").GetComponent<Button>().onClick.AddListener(Close);
        Control("PagePrev").GetComponent<Button>().onClick.AddListener(PrevPage);
        Control("PageNext").GetComponent<Button>().onClick.AddListener(NextPage);
        NextPage();
    }

    void NextPage()
    {
        if (pageIndex == pageMax)
            pageIndex = 1;
        else
            pageIndex += 1;
        if (refresh != null)
            GameBattleEx.Instance.StopCoroutine(refresh);
        refresh = GameBattleEx.Instance.StartCoroutine(RefreshRobot(pageIndex));
        Control("PageText").GetComponent<Text>().text = string.Format("{0:d2}/{1:d2}", pageIndex, pageMax);
    }

    void PrevPage()
    {
        if (pageIndex == 1)
            pageIndex = pageMax;
        else
            pageIndex -= 1;
        if (refresh != null)
            GameBattleEx.Instance.StopCoroutine(refresh);
        refresh = GameBattleEx.Instance.StartCoroutine(RefreshRobot(pageIndex));
        Control("PageText").GetComponent<Text>().text = string.Format("{0:d2}/{1:d2}", pageIndex, pageMax);
    }

    Coroutine refresh = null;
    IEnumerator RefreshRobot(int page)
    {
        for (int i = 0; i < PageCount; i++)
        {
            AddRobot(i, page);
            yield return 0;
        }
    }

    Dictionary<int, GameObject> RobotList = new Dictionary<int, GameObject>();
    void AddRobot(int Idx, int pageIdx)
    {
        if (RobotList.ContainsKey(Idx))
        {
            RobotList[Idx].GetComponent<Button>().onClick.RemoveAllListeners();
            RobotList[Idx].GetComponent<Button>().onClick.AddListener(() => { SpawnRobot(Idx, pageIdx == 1 ? EUnitCamp.EUC_ENEMY : EUnitCamp.EUC_FRIEND); });
            RobotList[Idx].GetComponentInChildren<Text>().text = string.Format("{0}{1}", Global.model[Idx], pageIdx == 1 ? "[敌]" : "[友]");
        }
        else
        {
            GameObject obj = GameObject.Instantiate(Resources.Load("GridItemBtn")) as GameObject;
            obj.GetComponent<Button>().onClick.AddListener(() => { SpawnRobot(Idx, pageIdx == 1 ? EUnitCamp.EUC_ENEMY : EUnitCamp.EUC_FRIEND); });
            obj.GetComponentInChildren<Text>().text = string.Format("{0}{1}", Global.model[Idx], pageIdx == 1 ? "[敌]" : "[友]");
            obj.transform.SetParent(RobotRoot.transform);
            obj.gameObject.layer = RobotRoot.layer;
            obj.transform.localScale = Vector3.one;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            RobotList.Add(Idx, obj);
        }
    }

    void SpawnRobot(int idx, EUnitCamp camp)
    {
        U3D.SpawnRobot(idx, camp);
    }
}

public class SfxWnd: Window<SfxWnd>
{
    public GameObject SfxRoot;
    public override string PrefabName
    {
        get
        {
            return "SfxWnd";
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
    int pageIndex = 32;//1-max
    private const int PageCount = 20;
    int pageMax = 0;
    int pageMin = 0;
    void Init()
    {
        pageMax = (SFXLoader.Instance.Eff.Length / PageCount) + ((SFXLoader.Instance.Eff.Length % PageCount) != 0 ? 1 : 0);
        SfxRoot = Control("Page");
        Control("Close").GetComponent<Button>().onClick.AddListener(Close);
        Control("PagePrev").GetComponent<Button>().onClick.AddListener(PrevPage);
        Control("PageNext").GetComponent<Button>().onClick.AddListener(NextPage);
        NextPage();
    }

    void NextPage()
    {
        if (pageIndex == pageMax)
            pageIndex = 1;
        else
            pageIndex += 1;
        if (refresh != null)
            GameBattleEx.Instance.StopCoroutine(refresh);
        refresh = GameBattleEx.Instance.StartCoroutine(RefreshSfx(pageIndex));
        Control("PageText").GetComponent<Text>().text = string.Format("{0:d2}/{1:d2}", pageIndex, pageMax);
    }

    void PrevPage()
    {
        if (pageIndex == 1)
            pageIndex = pageMax;
        else
            pageIndex -= 1;
        if (refresh != null)
            GameBattleEx.Instance.StopCoroutine(refresh);
        refresh = GameBattleEx.Instance.StartCoroutine(RefreshSfx(pageIndex));
        Control("PageText").GetComponent<Text>().text = string.Format("{0:d2}/{1:d2}", pageIndex, pageMax);
    }

    Coroutine refresh = null;
    IEnumerator RefreshSfx(int page)
    {
        for (int i = Mathf.Min((page - 1) * PageCount, (page) * PageCount); i < (page) * PageCount; i++)
        {
            int j = i - (page - 1) * PageCount;
            if (SFXList.Count > j)
                SFXList[j].SetActive(false);
        }

        for (int i = (page - 1)* PageCount; i < Mathf.Min((page)* PageCount, SFXLoader.Instance.TotalSfx); i++)
        {
            AddSFX(i, (page - 1) * PageCount);
            yield return 0;
        }
    }

    List<GameObject> SFXList = new List<GameObject>();
    void AddSFX(int Idx, int startIdx)
    {
        int j = Idx - startIdx;
        if (SFXList.Count > j)
        {
            SFXList[j].SetActive(true);
            SFXList[j].GetComponent<Button>().onClick.RemoveAllListeners();
            SFXList[j].GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            SFXList[j].GetComponent<Button>().onClick.AddListener(() => { PlaySfx(Idx); });
            SFXList[j].GetComponentInChildren<Text>().text = SFXLoader.Instance.Eff[Idx];
        }
        else
        {
            GameObject obj = GameObject.Instantiate(Resources.Load("GridItemBtn")) as GameObject;
            obj.GetComponent<Button>().onClick.AddListener(() => { PlaySfx(Idx); });
            obj.GetComponentInChildren<Text>().text = SFXLoader.Instance.Eff[Idx];
            obj.transform.SetParent(SfxRoot.transform);
            obj.gameObject.layer = SfxRoot.layer;
            obj.transform.localScale = Vector3.one;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            SFXList.Add(obj);
        }
    }

    void PlaySfx(int idx)
    {
        SFXLoader.Instance.PlayEffect(idx, MeteorManager.Instance.LocalPlayer.gameObject, true);
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
        yield return new WaitForSeconds(1.0f);
        if (result == 1)
        {
            
            for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
            {
                if (MeteorManager.Instance.UnitInfos[i].robot != null)
                    MeteorManager.Instance.UnitInfos[i].robot.Stop();
                MeteorManager.Instance.UnitInfos[i].controller.Input.ResetVector();
                if (MeteorManager.Instance.UnitInfos[i].Camp == EUnitCamp.EUC_FRIEND)
                    MeteorManager.Instance.UnitInfos[i].posMng.ChangeAction(CommonAction.Taunt);
            }
        }
        yield return new WaitForSeconds(2.0f);
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