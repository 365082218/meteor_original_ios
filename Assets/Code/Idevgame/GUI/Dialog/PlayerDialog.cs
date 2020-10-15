using Excel2Json;
using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDialogState : PersistDialog<PlayerDialog>
{
    public override string DialogName { get { return "PlayerDialog"; } }
    protected override float GetZ() { return 200; }
    protected override bool Use3DCanvas()
    {
        return true;
    }
}

public class PlayerDialog : Dialog
{
    public override void OnDialogStateEnter(PersistState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
    }

    void Init()
    {
        //RectTransform rectTran = WndObject.GetComponent<RectTransform>();
        //if (rectTran != null)
        //    rectTran.anchoredPosition = new Vector2((1920f - rectTran.sizeDelta.x) / 2.0f, -(1080f - rectTran.sizeDelta.y) / 2.0f);
        //GameObject obj = NodeHelper.Find("3DParent", WndObject);
        //GameObject objPlayer = GameObject.Instantiate(Resources.Load("3DUIPlayer")) as GameObject;
        //MeteorUnitDebug d = objPlayer.GetComponent<MeteorUnitDebug>();
        //objPlayer.transform.position = Vector3.zero;
        //objPlayer.transform.rotation = Quaternion.identity;
        //objPlayer.transform.localScale = Vector3.one;
        //d.gameObject.layer = obj.gameObject.layer;
        //d.Init(Main.Ins.LocalPlayer.UnitId, LayerMask.NameToLayer("3DUIPlayer"));
        //WeaponData weaponProperty = U3D.GetWeaponProperty(Main.Ins.LocalPlayer.weaponLoader.GetCurrentWeapon().Info().UnitId);
        //d.weaponLoader.StrWeaponR = weaponProperty.WeaponR;
        //d.weaponLoader.StrWeaponL = weaponProperty.WeaponL;
        ////d.weaponLoader.EquipWeapon();
        //d.transform.SetParent(obj.transform);
        //d.transform.localScale = 8 * Vector3.one;
        //d.transform.localPosition = new Vector3(0, 0, -300);
        //d.transform.localRotation = Quaternion.identity;
        //NodeHelper.Find("Close Button", WndObject).GetComponent<Button>().onClick.AddListener(OnBackPress);

        //SetStat("Stat Label 1", Main.Ins.LocalPlayer.Attr.hpCur + "/" + Main.Ins.LocalPlayer.Attr.HpMax);
        //SetStat("Stat Label 2", Main.Ins.LocalPlayer.AngryValue.ToString());
        //SetStat("Stat Label 3", Main.Ins.LocalPlayer.CalcDamage().ToString());
        //SetStat("Stat Label 4", Main.Ins.LocalPlayer.CalcDef().ToString());
        //SetStat("Stat Label 5", Main.Ins.LocalPlayer.MoveSpeed.ToString());
        //SetStat("Stat Label 6", string.Format("{0:f2}", Main.Ins.LocalPlayer.MoveSpeed / 1000.0f));

        ////处理背包的点击
        //UIItemSlot[] slots = NodeHelper.Find("Slots Grid", WndObject).GetComponentsInChildren<UIItemSlot>();
        //for (int i = 0; i < slots.Length; i++)
        //    slots[i].onClick.AddListener(OnClickItem);
    }

    void OnClickItem(UIItemSlot slot)
    {
        //Main.Ins.ExitState(Main.Ins.ItemInfoDialogState);
        //Main.Ins.EnterState(Main.Ins.ItemInfoDialogState);
        ItemInfoDialogState.Instance.AssignItem(slot.GetItemInfo());
    }

    void SetStat(string label, string value)
    {
        GameObject objStat = NodeHelper.Find(label, WndObject);
        GameObject objStatValue = Control("Stat Value", objStat);
        objStatValue.GetComponent<Text>().text = value;
    }
}