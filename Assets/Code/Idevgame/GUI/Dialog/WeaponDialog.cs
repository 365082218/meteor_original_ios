using Idevgame.GameState.DialogState;
using Idevgame.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponDialogState : CommonDialogState<WeaponDialog>
{
    public override string DialogName { get { return "WeaponDialog"; } }
    public WeaponDialogState(MainDialogStateManager stateMgr) : base(stateMgr)
    {

    }
}

public class WeaponDialog : Dialog
{
    public GameObject CameraForWeapon;
    public GameObject WeaponModelParent;
    public GameObject WeaponRoot;
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
    }

    public override void OnClose()
    {
        if (load != null)
        {
            Main.Ins.StopCoroutine(load);
            load = null;
        }
        if (CameraForWeapon != null)
            GameObject.Destroy(CameraForWeapon);
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
        Control("Equip").GetComponent<Button>().onClick.AddListener(() => { ChangeWeaponCode(); });
        Control("Close").GetComponent<Button>().onClick.AddListener(OnBackPress);
        for (int i = 0; i < 12; i++)
        {
            string control = string.Format("Tab{0}", i);
            Control(control).GetComponent<UITab>().onValueChanged.AddListener(ChangeWeaponType);
        }
        if (load == null)
            load = Main.Ins.StartCoroutine(AddWeapon());
    }

    IEnumerator AddWeapon()
    {
        List<ItemDatas.ItemDatas> we = Main.Ins.DataMgr.GetDatasArray<ItemDatas.ItemDatas>();
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
    void AddWeaponItem(ItemDatas.ItemDatas it, int idx)
    {
        if (GridWeapon.Count > idx)
        {
            GridWeapon[idx].SetActive(true);
            UIFunCtrl ctrl = GridWeapon[idx].GetComponent<UIFunCtrl>();
            ctrl.SetEvent(ShowWeapon, it.ID);
            ctrl.SetText(it.Name);
        }
        else
        {
            GameObject weapon = GameObject.Instantiate(Resources.Load("GridItemBtn")) as GameObject;
            UIFunCtrl obj = weapon.AddComponent<UIFunCtrl>();
            obj.SetEvent(ShowWeapon, it.ID);
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
        List<ItemDatas.ItemDatas> we = Main.Ins.DataMgr.GetDatasArray<ItemDatas.ItemDatas>();
        for (int i = 0; i < we.Count; i++)
        {
            if (we[i].MainType == 1)
            {
                if (we[i].SubType == (int)weaponSubType)
                {
                    selectWeapon = we[i].ID;
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
        Main.Ins.LocalPlayer.ChangeWeaponCode(selectWeapon);
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
            Main.Ins.StopCoroutine(load);
        load = Main.Ins.StartCoroutine(AddWeapon());
    }
}