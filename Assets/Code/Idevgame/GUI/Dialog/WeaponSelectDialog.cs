using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSelectDialogState : CommonDialogState<WeaponSelectDialog>
{
    public override string DialogName { get { return "WeaponSelectDialog"; } }
    public WeaponSelectDialogState(MainDialogStateManager stateMgr) : base(stateMgr)
    {

    }
}

public class WeaponSelectDialog : Dialog
{
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
    }
    Image weaponImg;
    int weaponIdx = 0;

    void Init()
    {
        weaponImg = Control("Image").GetComponent<Image>();
        weaponImg.material = ResMng.Load("Weapon_0") as Material;
        Control("Next").GetComponent<Button>().onClick.AddListener(() =>
        {
            OnNextWeapon();
            U3D.PlayBtnAudio();
        });
        Control("Prev").GetComponent<Button>().onClick.AddListener(() =>
        {
            OnPrevWeapon();
            U3D.PlayBtnAudio();
        });
        Control("Select").GetComponent<UIButtonExtended>().onClick.AddListener(() =>
        {
            OnSelectWeapon();
            U3D.PlayBtnAudio();
        });
    }

    void OnNextWeapon()
    {
        weaponIdx += 1;
        if (weaponIdx >= 12)
            weaponIdx = 0;
        weaponImg.material = ResMng.Load(string.Format("Weapon_{0}", weaponIdx)) as Material;
    }

    void OnPrevWeapon()
    {
        weaponIdx -= 1;
        if (weaponIdx < 0)
            weaponIdx = 11;
        weaponImg.material = ResMng.Load(string.Format("Weapon_{0}", weaponIdx)) as Material;
    }

    void OnSelectWeapon()
    {
        Main.Ins.NetWorkBattle.weaponIdx = U3D.GetWeaponByCode(weaponIdx);
        Main.Ins.NetWorkBattle.EnterLevel();
        OnBackPress();
    }
}
