using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.UI;
using Idevgame.GameState;
using Idevgame.GameState.DialogState;
using DG.Tweening;

public class FightState : PersistDialog<FightWnd>
{
    public override string DialogName { get { return "FightWnd"; } }
}

public class FightWnd : Dialog
{
    public override void OnDialogStateEnter(PersistState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Input.multiTouchEnabled = true;//忍刀
        Init();
        BaseDialogState.UICamera.clearFlags = CameraClearFlags.Depth;
        //如果使用鼠标。那么锁定鼠标
#if UNITY_ANDROID
        Cursor.lockState = GameStateMgr.Ins.gameStatus.UseMouse ? CursorLockMode.Locked : CursorLockMode.None;
#endif
    }

    public override void OnDialogStateExit() {
        base.OnDialogStateExit();
        BaseDialogState.UICamera.clearFlags = CameraClearFlags.Color;
        BaseDialogState.UICamera.backgroundColor = Color.black;
        Cursor.lockState = CursorLockMode.None;
    }


    Text timeLabel;
    Image hpBar;
    Image angryBar;
    Text hpLabel;
    Text Position;
    bool showSkill = false;
    public void ShowSkillBar() {
        Control("SkillHotArea").SetActive(false);
        Tweener t = Control("ActionFloat").GetComponent<RectTransform>().DOAnchorPosY(75, 1.0f);
        t.OnComplete(OnSkillBarExpand);
    }

    void OnSkillBarExpand() {
        showSkill = true;
    }

    public void HideSkillBar() {
        Tweener t = Control("ActionFloat").GetComponent<RectTransform>().DOAnchorPosY(-100, 1.0f);
        t.OnComplete(OnSkillBarHide);
    }

    void OnSkillBarHide() {
        Control("SkillHotArea").SetActive(true);
        Control("SkillHotArea").GetComponent<Image>().color = Color.white;
        Control("SkillHotArea").GetComponent<Image>().DOColor(Color.clear, 0.5f);
    }

    public void HideCameraBtn()
    {
        if (Unlock != null)
            Unlock.SetActive(false);
    }

    //显示所有人属性
    public void ShowPlayerInfo() {

    }

    public void ShowCameraBtn()
    {
        if (Unlock != null)
            Unlock.SetActive(true);
    }

    AutoMsgCtrl ctrl;
    Transform LevelTalkRoot;
    GameObject Unlock;
    Image LockSprite;
    GameObject TargetBlood;
    Image TargetHp;
    Text TargetHPLabel;
    Text TargetName;
    GameObject Menu2HotArea;
    GameObject Prev;
    GameObject Next;
    void Init()
    {
        Prev = Control("Prev");
        Next = Control("Next");
        Button PrevBtn = Control("PrevPanel").GetComponent<Button>();
        Button NextBtn = Control("NextPanel").GetComponent<Button>();
        PrevBtn.onClick.AddListener(U3D.WatchPrevRobot);
        NextBtn.onClick.AddListener(U3D.WatchNextRobot);
        Position = Control("Position").GetComponent<Text>();
        clickPanel = Control("ClickPanel");
        LevelTalkRoot = Control("LevelTalk", WndObject).transform;
        ctrl = LevelTalkRoot.GetComponent<AutoMsgCtrl>();
        ctrl.SetConfig(2.0f, 1.5f);
        //联机不需要剧情对白面板，而使用房间聊天面板单独代替.
        if (U3D.IsMultiplyPlayer()) {
            GameObject.Destroy(Control("BattleInfo").gameObject);
        } else {
            Control("BattleInfo").GetComponent<RectTransform>().anchoredPosition = new Vector2(145, -175);
        }
        NodeHelper.Find("Attack", WndObject).GetComponent<GameButton>().OnPress.AddListener(OnAttackPress);
        NodeHelper.Find("Attack", WndObject).GetComponent<GameButton>().OnRelease.AddListener(OnAttackRelease);
        NodeHelper.Find("Defence", WndObject).GetComponentInChildren<GameButton>().OnPress.AddListener(OnDefencePress);
        NodeHelper.Find("Defence", WndObject).GetComponentInChildren<GameButton>().OnRelease.AddListener(OnDefenceRelease);
        NodeHelper.Find("Jump", WndObject).GetComponentInChildren<GameButton>().OnPress.AddListener(OnJumpPress);
        NodeHelper.Find("Jump", WndObject).GetComponentInChildren<GameButton>().OnRelease.AddListener(OnJumpRelease);
        NodeHelper.Find("ChangeWeapon", WndObject).GetComponentInChildren<GameButton>().OnPress.AddListener(OnChangeWeaponPress);
        NodeHelper.Find("ChangeWeapon", WndObject).GetComponentInChildren<GameButton>().OnRelease.AddListener(OnChangeWeaponRelease);
        NodeHelper.Find("BreakOut", WndObject).GetComponentInChildren<GameButton>().OnPress.AddListener(OnBreakOut);
        NodeHelper.Find("WeaponSelect", WndObject).GetComponentInChildren<Button>().onClick.AddListener(() => { U3D.OpenWeaponWnd(); });
        NodeHelper.Find("SceneName", WndObject).GetComponent<Button>().onClick.AddListener(() => { OpenMiniMap(); });
        NodeHelper.Find("SceneName", WndObject).GetComponentInChildren<Text>().text = CombatData.Ins.GLevelItem.Name;
        NodeHelper.Find("System", WndObject).GetComponentInChildren<Button>().onClick.AddListener(() => { U3D.OpenSystemWnd(); });
        NodeHelper.Find("Crouch", WndObject).GetComponent<GameButton>().OnPress.AddListener(OnCrouchPress);
        NodeHelper.Find("Crouch", WndObject).GetComponent<GameButton>().OnRelease.AddListener(OnCrouchRelease);
        NodeHelper.Find("Drop", WndObject).GetComponent<Button>().onClick.AddListener(OnClickDrop);
        Unlock = NodeHelper.Find("Unlock", WndObject);
        Unlock.GetComponentInChildren<Button>().onClick.AddListener(OnClickChangeLock);
        LockSprite = NodeHelper.Find("LockSprite", Unlock).GetComponent<Image>();
        NodeHelper.Find("SfxMenu", WndObject).GetComponentInChildren<Button>().onClick.AddListener(() => { U3D.OpenSfxWnd(); });
        NodeHelper.Find("Robot", WndObject).GetComponentInChildren<Button>().onClick.AddListener(() => { U3D.OpenRobotWnd(); });
        timeLabel = NodeHelper.Find("GameTime", WndObject).GetComponent<Text>();
        hpBar = Control("HPBar", WndObject).gameObject.GetComponent<Image>();
        angryBar = Control("AngryBar", WndObject).gameObject.GetComponent<Image>();
        hpLabel = Control("HPLabel", WndObject).gameObject.GetComponent<Text>();
        NodeHelper.Find("Status", WndObject).GetComponentInChildren<GameButton>().OnPress.AddListener(OnStatusPress);
        NodeHelper.Find("Status", WndObject).GetComponentInChildren<GameButton>().OnRelease.AddListener(OnStatusRelease);
        NodeHelper.Find("Chat", WndObject).GetComponentInChildren<Button>().onClick.AddListener(OnChatClick);
        Control("SkillHotArea").GetComponent<Button>().onClick.AddListener(ShowSkillBar);
        NodeHelper.Find("Reborn", WndObject).GetComponentInChildren<Button>().onClick.AddListener(OnRebornClick);
        NodeHelper.Find("SysMenu2", WndObject).SetActive(true);
        //单机-金华城-只有这一关能复活冷燕
        if (CombatData.Ins.GLevelMode == LevelMode.SinglePlayerTask && CombatData.Ins.GLevelItem.Id == 4)
            NodeHelper.Find("Reborn", WndObject).SetActive(true);
        else
        {
            //创建关卡，非暗杀，都不允许复活
            if (CombatData.Ins.GGameMode != GameMode.ANSHA)
                NodeHelper.Find("Reborn", WndObject).SetActive(false);
        }

        //联机屏蔽按键-多人游戏
        if (U3D.IsMultiplyPlayer())
        {
            //联机还无法复活队友.
            NodeHelper.Find("Reborn", WndObject).SetActive(false);
        }
        else
        {
            //非联机屏蔽按键-单人游戏
            NodeHelper.Find("Chat", WndObject).SetActive(false);
        }

        if (Main.Ins.LocalPlayer != null)
        {
            angryBar.fillAmount = 0.0f;
            UpdatePlayerInfo();
        }
        TargetBlood = Control("TargetBlood");
        TargetBlood.SetActive(false);
        TargetHp = Control("HPBar", TargetBlood).GetComponent<Image>();
        TargetHPLabel = Control("TargetHPLabel", TargetBlood).GetComponent<Text>();
        TargetName = Control("TargetName", TargetBlood).GetComponent<Text>();
        UpdateUIButton();
        CanvasGroup[] c = WndObject.GetComponentsInChildren<CanvasGroup>();
        for (int i = 0; i < c.Length; i++)
            c[i].alpha = GameStateMgr.Ins.gameStatus.UIAlpha;
        //使用手柄时，不再显示右下侧按键和方向盘.
        if (GameStateMgr.Ins.gameStatus.UseGamePad) {
            Control("ClickPanel").SetActive(false);
            Control("JoyArrow").SetActive(false);
        }

        OnBattleStart();
        Menu2HotArea = Control("Menu2HotArea", WndObject);
        Menu2HotArea.GetComponent<Button>().onClick.AddListener(ShowSysMenu2);
        Menu2HotArea.SetActive(false);
        Control("HideBtn", WndObject).GetComponent<Button>().onClick.AddListener(SysMenu2Hide);
    }

    bool showPosition = false;
    public void ShowPosition() {
        showPosition = !showPosition;
        Position.gameObject.SetActive(showPosition);
    }

    bool system2Hide = false;
    void SysMenu2Hide() {
        if (system2Hide)
            return;
        GameObject sysMenu2 = NodeHelper.Find("SysMenu2", WndObject);
        Tweener t = sysMenu2.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-180, 0), 0.5f);
        t.OnComplete(OnSysMenu2Hide);
    }

    void ShowSysMenu2() {
        if (!system2Hide)
            return;
        GameObject sysMenu2 = NodeHelper.Find("SysMenu2", WndObject);
        Tweener t = sysMenu2.GetComponent<RectTransform>().DOAnchorPos(new Vector2(0, 0), 0.5f);
        t.OnComplete(OnSysMenu2Show);
    }

    void OnSysMenu2Show() {
        system2Hide = false;
        Menu2HotArea.SetActive(false);
    }

    void OnSysMenu2Hide() {
        system2Hide = true;
        Menu2HotArea.SetActive(true);
        Menu2HotArea.GetComponent<Image>().color = Color.white;
        Menu2HotArea.GetComponent<Image>().DOColor(Color.clear, 0.5f);
    }

    public override void OnRefresh(int message, object param)
    {
        base.OnRefresh(message, param);
        UpdateUIButton();
    }

    void OnChatClick()
    {
        if (!ChatDialogState.Exist())
            ChatDialogState.State.Open();
        else
            ChatDialogState.State.Close();
    }

    void OnStatusPress()
    {
        if (!BattleStatusDialogState.Exist())
            BattleStatusDialogState.State.Open();
    }

    void OnStatusRelease()
    {
        if (BattleStatusDialogState.Exist())
            BattleStatusDialogState.State.Close();
    }

    public void OnRebornClick()
    {
        if (CombatData.Ins.GLevelMode == LevelMode.SinglePlayerTask)
        {
            if (Main.Ins.LocalPlayer.Dead)
                return;

            if (CombatData.Ins.GMeteorInput == null || CombatData.Ins.PauseAll) return;
                CombatData.Ins.GMeteorInput.OnKeyDownProxy(EKeyList.KL_Help);
        }
        else if (CombatData.Ins.GLevelMode == LevelMode.CreateWorld)
        {
            if (CombatData.Ins.GGameMode == GameMode.ANSHA)
            {
                if (CombatData.Ins.GMeteorInput == null || CombatData.Ins.PauseAll) return;
                    CombatData.Ins.GMeteorInput.OnKeyDownProxy(EKeyList.KL_Help);
            }
        }
    }
    //int currentPosIdx;
    //public void UpdatePoseStatus(int idx, int frame = 0)
    //{
    //    if (currentPos != null)
    //    {
    //        currentPosIdx = idx;
    //        currentPos.GetComponentInChildren<Text>().text = "Pose " + idx + " Frames " + frame;
    //    }

    //    MeteorManager.Instance.LocalPlayer.Action = idx;
    //    MeteorManager.Instance.LocalPlayer.Frame = frame;
    //}

    //IEnumerator ShowHPWarning()
    //{
    //    while (true)
    //    {
    //        if (MeteorManager.Instance.LocalPlayer.Dead)
    //            break;
    //        if ((float)MeteorManager.Instance.LocalPlayer.Attr.HpMax * 0.3f > (float)MeteorManager.Instance.LocalPlayer.Attr.hpCur)
    //            hpWarning.enabled = !hpWarning.enabled;
    //        yield return new WaitForSeconds(0.5f);//半秒切换一次状态
    //    }
    //    hpWarning.enabled = false;
    //}

    //IEnumerator ShowAngryWarning()
    //{
    //    while (true)
    //    {
    //        if (MeteorManager.Instance.LocalPlayer.Dead)
    //            break;
    //        if (MeteorManager.Instance.LocalPlayer.AngryValue == Global.ANGRYMAX)
    //            angryWarning.enabled = !angryWarning.enabled;
    //        yield return new WaitForSeconds(0.5f);//半秒切换一次状态
    //    }
    //    angryWarning.enabled = false;
    //}

    bool openMiniMap = false;
    void OpenMiniMap()
    {
        openMiniMap = !openMiniMap;
        NodeHelper.Find("MiniMapFrame", WndObject).SetActive(openMiniMap);
    }
    public GameObject clickPanel;
    public void UpdateUIButton()
    {
        NodeHelper.Find("WeaponSelect", gameObject).SetActive(GameStateMgr.Ins.gameStatus.EnableWeaponChoose);
        NodeHelper.Find("SfxMenu", gameObject).SetActive(GameStateMgr.Ins.gameStatus.EnableDebugSFX);
        NodeHelper.Find("Robot", gameObject).SetActive(GameStateMgr.Ins.gameStatus.EnableDebugRobot && !U3D.IsMultiplyPlayer());
        NodeHelper.Find("MiniMap", gameObject).SetActive(true);

        if (UGUIJoystick.Ins != null) {
            UGUIJoystick.Ins.SetJoyEnable(GameStateMgr.Ins.gameStatus.JoyEnable);
        }

        if (UGUIJoystick.Ins != null)
            UGUIJoystick.Ins.SetAnchor(GameStateMgr.Ins.gameStatus.JoyAnchor);

        Prev.gameObject.SetActive(U3D.WatchAi);
        Next.gameObject.SetActive(U3D.WatchAi);
        int j = 0;
        for (int i = 0; i < clickPanel.transform.childCount; i++)
        {
            Transform tri = clickPanel.transform.GetChild(i);
            if (tri.name == "Direction")
                continue;
            RectTransform r = tri.GetComponent<RectTransform>();
            if (GameStateMgr.Ins.gameStatus.HasUIAnchor[j])
                r.anchoredPosition = GameStateMgr.Ins.gameStatus.UIAnchor[j];
            float scale = GameStateMgr.Ins.gameStatus.UIScale[j];
            r.localScale = new Vector3(scale, scale, 1);
            j++;
        }
    }

    public void OnAttackPress()
    {
        if (CombatData.Ins.GMeteorInput == null || CombatData.Ins.PauseAll) return;
        CombatData.Ins.GMeteorInput.OnKeyDownProxy(EKeyList.KL_Attack);//也可看作普攻
    }

    public void OnChangeLock(bool locked)
    {
        LockSprite.enabled = locked;
    }

    public void OnClickChangeLock()
    {
        if (CombatData.Ins.GMeteorInput == null || CombatData.Ins.PauseAll) return;
        //远程武器禁止切换锁定状态
        int weaponEquiped = Main.Ins.LocalPlayer.GetWeaponType();
        if (weaponEquiped == (int)EquipWeaponType.Gun || weaponEquiped == (int)EquipWeaponType.Dart || weaponEquiped == (int)EquipWeaponType.Guillotines)
            return;

        if (Main.Ins.GameBattleEx.bLocked)
            Main.Ins.GameBattleEx.Unlock();
        else
            Main.Ins.GameBattleEx.Lock();
    }

    public void OnClickDrop()
    {
        if (Main.Ins.LocalPlayer.Dead)
            return;

        if (CombatData.Ins.GMeteorInput == null || CombatData.Ins.PauseAll) return;
        CombatData.Ins.GMeteorInput.OnKeyDownProxy(EKeyList.KL_DropWeapon);
    }

    public void OnCrouchPress()
    {
        if (Main.Ins.LocalPlayer.Dead)
            return;

        if (CombatData.Ins.GMeteorInput == null || CombatData.Ins.PauseAll) return;
        CombatData.Ins.GMeteorInput.OnKeyDownProxy(EKeyList.KL_Crouch);
    }

    public void OnCrouchRelease()
    {
        if (CombatData.Ins.GMeteorInput == null || CombatData.Ins.PauseAll) return;
        CombatData.Ins.GMeteorInput.OnKeyUpProxy(EKeyList.KL_Crouch);
    }

    public void OnChangeWeaponPress()
    {
        if (Main.Ins.LocalPlayer.Dead)
            return;

        if (CombatData.Ins.GMeteorInput == null || CombatData.Ins.PauseAll) return;
        CombatData.Ins.GMeteorInput.OnKeyDownProxy(EKeyList.KL_ChangeWeapon);
    }

    public void OnChangeWeaponRelease()
    {
        if (Main.Ins.LocalPlayer.Dead)
            return;

        if (CombatData.Ins.GMeteorInput == null || CombatData.Ins.PauseAll) return;
        CombatData.Ins.GMeteorInput.OnKeyUpProxy(EKeyList.KL_ChangeWeapon);
    }

    public void OnAttackRelease()
    {
        if (CombatData.Ins.GMeteorInput == null || CombatData.Ins.PauseAll) return;
        CombatData.Ins.GMeteorInput.OnKeyUpProxy(EKeyList.KL_Attack);
    }

    public void OnDefencePress()
    {
        if (Main.Ins.LocalPlayer.Dead)
            return;
        if (CombatData.Ins.GMeteorInput == null || CombatData.Ins.PauseAll) return;
        CombatData.Ins.GMeteorInput.OnKeyDownProxy(EKeyList.KL_Defence);//不要被键盘状态同步，否则按下马上就抬起，那么防御姿势就消失了

    }

    public void OnDefenceRelease()
    {
        if (Main.Ins.LocalPlayer.Dead)
            return;
        if (CombatData.Ins.GMeteorInput == null || CombatData.Ins.PauseAll) return;
        CombatData.Ins.GMeteorInput.OnKeyUpProxy(EKeyList.KL_Defence);
    }

    public void OnJumpPress()
    {
        //if (!MeteorManager.Instance.LocalPlayer.posMng.CanJump)
        //    return;

        if (CombatData.Ins.GMeteorInput == null || CombatData.Ins.PauseAll) return;
        CombatData.Ins.GMeteorInput.OnKeyDownProxy(EKeyList.KL_Jump);//
    }

    public void OnJumpRelease()
    {
        if (CombatData.Ins.GMeteorInput == null || CombatData.Ins.PauseAll) return;
        CombatData.Ins.GMeteorInput.OnKeyUpProxy(EKeyList.KL_Jump);
    }

    //按爆气.
    public void OnBreakOut()
    {
        //Debug.Log("OnBreakOut");
        if (CombatData.Ins.GMeteorInput == null || CombatData.Ins.PauseAll)
            return;
        if (Main.Ins.LocalPlayer.AngryValue >= 60)
        {
            CombatData.Ins.GMeteorInput.OnKeyDownProxy(EKeyList.KL_BreakOut);
            //Debug.Log("OnKeyDown");
        }
    }

    //int lastAngry = 0;
    public void UpdateAngryBar()
    {
        if (Main.Ins.LocalPlayer != null && !Main.Ins.LocalPlayer.Dead)
        {
            angryBar.fillAmount = (float)Main.Ins.LocalPlayer.AngryValue / (float)CombatData.ANGRYMAX;
        }
    }

    public void UpdateTime(string label)
    {
        timeLabel.text = label;
    }

    //弹出剧情.
    public void InsertFightMessage(string text)
    {
        if (LevelTalkRoot != null && ctrl != null)
            ctrl.PushMessage(text);
    }

    public void OnBattleStart()
    {
        //Debug.Log("OnBattleStart");
        if (Main.Ins.LocalPlayer != null) {
            currentHP = nextHp = Main.Ins.LocalPlayer.Attr.hpCur;
            hpBar.fillAmount = currentHP / (float)Main.Ins.LocalPlayer.Attr.HpMax;
        }
    }

    public void OnBattleEnd()
    {
        NodeHelper.Find("Status", WndObject).SetActive(false);
        NodeHelper.Find("Chat", WndObject).SetActive(false);
    }

    MeteorUnit CurrentMonster;
    public void UpdateMonsterInfo(MeteorUnit mon)
    {
        if (!GameStateMgr.Ins.gameStatus.ShowBlood)
            return;

        if (!TargetBlood.activeInHierarchy)
            TargetBlood.SetActive(true);

        if (CurrentMonster == mon)
        {
            nextTargetHp = mon.Attr.hpCur;
            TargetHPLabel.text = ((int)(mon.Attr.hpCur / 10.0f)).ToString() + "/" + ((int)(mon.Attr.TotalHp / 10.0f)).ToString();
            CheckHideTarget = true;
            TargetInfoLast = 5.0f;
            return;
        }

        CheckHideTarget = true;
        TargetInfoLast = 5.0f;
        TargetHp.fillAmount = (float)mon.Attr.hpCur / (float)mon.Attr.TotalHp;
        TargetHPLabel.text = ((int)(mon.Attr.hpCur / 10.0f)).ToString() + "/" + ((int)(mon.Attr.TotalHp / 10.0f)).ToString();
        currentTargetHp = mon.Attr.hpCur;
        nextTargetHp = mon.Attr.hpCur;
        TargetName.text = mon.name;
        CurrentMonster = mon;
    }

    public void Update()
    {
        if (showPosition) {
            if (Main.Ins.LocalPlayer != null) {
                Transform trans = Main.Ins.LocalPlayer.transform;
                Position.text = string.Format("Position:{0} {1} {2}", Mathf.FloorToInt(trans.position.x), Mathf.FloorToInt(trans.position.y), Mathf.FloorToInt(trans.position.z));
            }
        }
        if (currentHP != nextHp)
        {
            currentHP = Mathf.MoveTowards(currentHP, nextHp, 1000f * FrameReplay.deltaTime);
            hpBar.fillAmount = currentHP / (float)Main.Ins.LocalPlayer.Attr.TotalHp;
        }

        if (currentTargetHp != nextTargetHp)
        {
            currentTargetHp = Mathf.MoveTowards(currentTargetHp, nextTargetHp, 1000f * FrameReplay.deltaTime);
            TargetHp.fillAmount = currentTargetHp / CurrentMonster.Attr.TotalHp;
        }

        if (CheckHideTarget)
        {
            TargetInfoLast -= FrameReplay.deltaTime;
            if (TargetInfoLast <= 0)
                HideTargetInfo();
        }
    }

    void HideTargetInfo()
    {
        TargetBlood.SetActive(false);
        CheckHideTarget = false;
        CurrentMonster = null;
        TargetInfoLast = 5.0f;
    }

    bool CheckHideTarget = false;
    float TargetInfoLast = 10;
    float nextTargetHp = 0;
    float currentTargetHp = 0;
    float nextHp = 0;
    float currentHP = 0;
    public void UpdatePlayerInfo()
    {
        if (Main.Ins.LocalPlayer != null && Main.Ins.LocalPlayer.Attr.hpCur >= 0)
        {
            hpLabel.text = ((int)(Main.Ins.LocalPlayer.Attr.hpCur / 10.0f)).ToString() + "/" + ((int)(Main.Ins.LocalPlayer.Attr.HpMax / 10.0f)).ToString();
            nextHp = Main.Ins.LocalPlayer.Attr.hpCur;
            UpdateAngryBar();
        }
    }
}