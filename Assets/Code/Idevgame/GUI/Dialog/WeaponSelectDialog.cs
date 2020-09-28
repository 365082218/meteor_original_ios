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
    int weaponIdx = -1;
    void Init()
    {
        weaponImg = Control("Image").GetComponent<Image>();
        weaponImg.color = Color.black;
        weaponIdx = -1;
        OnNextWeapon();
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
        LoadWeaponTex();
    }

    void LoadWeaponTex() {
        Texture2D tex = Resources.Load<Texture2D>(string.Format("wp00{0}", weaponIdx));
        if (tex != null) {
            weaponImg.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
            Utility.Expand(weaponImg, tex.width, tex.height);
            weaponImg.color = Color.white;
        }
    }

    void OnPrevWeapon()
    {
        weaponIdx -= 1;
        if (weaponIdx < 0)
            weaponIdx = 11;
        LoadWeaponTex();
    }

    void OnSelectWeapon()
    {
        Main.Ins.NetWorkBattle.weaponIdx = U3D.GetWeaponByCode(weaponIdx);
        Main.Ins.NetWorkBattle.EnterLevel();
        OnBackPress();
    }
}
