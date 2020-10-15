using Assets.Code.Idevgame.Common.Util;
using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSelectDialogState : CommonDialogState<WeaponSelectDialog>
{
    public override string DialogName { get { return "WeaponSelectDialog"; } }
    public WeaponSelectDialogState(MainDialogMgr stateMgr) : base(stateMgr)
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

    public override void OnClose() {
        if (time != null) {
            time.Stop();
            time = null;
        }
    }
    Image weaponImg;
    int weaponIdx = -1;
    void Init()
    {
        weaponImg = Control("Image").GetComponent<Image>();
        weaponImg.color = Color.white;
        weaponIdx = -1;
        OnNextWeapon();
        Control("Next").GetComponent<Button>().onClick.AddListener(() =>
        {
            OnNextWeapon();
        });
        Control("Prev").GetComponent<Button>().onClick.AddListener(() =>
        {
            OnPrevWeapon();
        });
        Control("Select").GetComponent<UIButtonExtended>().onClick.AddListener(() =>
        {
            OnSelectWeapon();
            U3D.PlayBtnAudio();
        });
        Control("Timeup").SetActive(false);
        Control("Exit").SetActive(false);
        Control("Exit").GetComponent<Button>().onClick.AddListener(() => { NetWorkBattle.Ins.OnDisconnect(); });
    }

    public void ShowBack() {
        Control("Exit").SetActive(true);
        Button btn = Control("Exit").GetComponent<Button>();
        btn.GetComponentInChildren<Text>().text = "重选阵营";
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => {
            Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.CampSelectDialogState);
        });
    }

    Timer time;
    int totalTime;
    public void ShowTimeup(int second) {
        if (time != null)
            time.Stop();
        time = Timer.loop(1, UpdateText);
        totalTime = second + 1;
        Control("Timeup").SetActive(true);
        Control("Exit").SetActive(true);
        UpdateText();
    }

    void UpdateText() {
        totalTime -= 1;
        Control("Timeup").GetComponent<Text>().text = string.Format("还剩{0}秒重开", totalTime);
        if (totalTime <= 0) {
            Control("Timeup").SetActive(false);
            if (time != null) {
                time.Stop();
                time = null;
            }
        }
    }

    void OnNextWeapon()
    {
        weaponIdx += 1;
        if (weaponIdx >= 12)
            weaponIdx = 0;
        LoadWeaponTex();
    }

    void LoadWeaponTex() {
        Texture2D tex = Resources.Load<Texture2D>(string.Format("wp00{0}", weaponIdx + 1));
        if (tex != null) {
            weaponImg.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        }
        Utility.Expand(weaponImg, tex.width, tex.height);
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
        NetWorkBattle.Ins.weaponIdx = U3D.GetWeaponByCode(weaponIdx);
        NetWorkBattle.Ins.EnterLevel();
    }
}
