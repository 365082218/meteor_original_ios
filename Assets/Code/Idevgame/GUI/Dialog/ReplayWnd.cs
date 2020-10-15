using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.UI;
using Idevgame.GameState;
using Idevgame.GameState.DialogState;
using DG.Tweening;


public class ReplayState : PersistDialog<ReplayUiConroller>
{
    public override string DialogName { get { return "ReplayWnd"; } }
}

public class ReplayUiConroller : Dialog
{
    UILoadingBar LoadingBar;
    Text timeLabel;
    Image hpBar;
    Image angryBar;
    Text hpLabel;
    AutoMsgCtrl ctrl;
    Transform LevelTalkRoot;
    GameObject Unlock;
    Image LockSprite;
    GameObject TargetBlood;
    Image TargetHp;
    Text TargetHPLabel;
    Text TargetName;
    Image Fill;
    Text ProgressText;
    public override void OnDialogStateEnter(PersistState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
        BaseDialogState.UICamera.clearFlags = CameraClearFlags.Depth;
    }

    void Init()
    {
        Fill = Control("Fill", WndObject).GetComponent<Image>();
        ProgressText = Control("ProgressText", WndObject).GetComponent<Text>();
        clickPanel = Control("ClickPanel");
        LevelTalkRoot = Control("LevelTalk", WndObject).transform;
        ctrl = LevelTalkRoot.GetComponent<AutoMsgCtrl>();
        ctrl.SetConfig(2.0f, 1.5f);
        Control("FreeCamera", WndObject).GetComponent<Button>().onClick.AddListener(UseFreeCamera);
        Control("FollowCamera", WndObject).GetComponent<Button>().onClick.AddListener(UseFollowCamera);
        Control("PrevTarget", WndObject).GetComponent<Button>().onClick.AddListener(OnPrevTarget);
        Control("NextTarget", WndObject).GetComponent<Button>().onClick.AddListener(OnNextTarget);
        Control("BattleInfo").GetComponent<RectTransform>().anchoredPosition = new Vector2(145, -175);
        NodeHelper.Find("SceneName", WndObject).GetComponent<Button>().onClick.AddListener(() => { OpenMiniMap(); });
        NodeHelper.Find("SceneName", WndObject).GetComponentInChildren<Text>().text = CombatData.Ins.GLevelItem.Name;
        NodeHelper.Find("System", WndObject).GetComponentInChildren<Button>().onClick.AddListener(() => { Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.EscConfirmDialogState); });
        timeLabel = NodeHelper.Find("GameTime", WndObject).GetComponent<Text>();
        hpBar = Control("HPBar", WndObject).gameObject.GetComponent<Image>();
        angryBar = Control("AngryBar", WndObject).gameObject.GetComponent<Image>();
        hpLabel = Control("HPLabel", WndObject).gameObject.GetComponent<Text>();
        NodeHelper.Find("Status", WndObject).GetComponentInChildren<GameButton>().OnPress.AddListener(OnStatusPress);
        NodeHelper.Find("Status", WndObject).GetComponentInChildren<GameButton>().OnRelease.AddListener(OnStatusRelease);
        Control("HideBtn", WndObject).GetComponent<Button>().onClick.AddListener(SysMenu2Hide);
        Control("Menu2HotArea", WndObject).GetComponent<Button>().onClick.AddListener(ShowSysMenu2);
        Control("Menu2HotArea", WndObject).SetActive(false);
        NodeHelper.Find("SysMenu2", WndObject).SetActive(true);
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
        NodeHelper.Find("MiniMap", gameObject).SetActive(true);
        CanvasGroup[] c = WndObject.GetComponentsInChildren<CanvasGroup>();
        for (int i = 0; i < c.Length; i++)
            c[i].alpha = GameStateMgr.Ins.gameStatus.UIAlpha;
    }

    void UseFreeCamera() {

    }

    void UseFollowCamera() {

    }

    void OnPrevTarget() {

    }

    void OnNextTarget() {

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
        Control("Menu2HotArea", WndObject).SetActive(false);
    }

    void OnSysMenu2Hide() {
        system2Hide = true;
        Control("Menu2HotArea", WndObject).SetActive(true);
    }

    void OnStatusPress()
    {
        //if (!BattleStatusDialogState.Exist())
        //    Main.Ins.EnterState(Main.Ins.BattleStatusDialogState);
    }

    void OnStatusRelease()
    {
        //if (BattleStatusDialogState.Exist())
        //    Main.Ins.ExitState(Main.Ins.BattleStatusDialogState);
    }

    bool openMiniMap = false;
    void OpenMiniMap()
    {
        openMiniMap = !openMiniMap;
        NodeHelper.Find("MiniMapFrame", WndObject).SetActive(openMiniMap);
    }
    public GameObject clickPanel;

    public void UpdateTime(string label)
    {
        timeLabel.text = label;
        //Fill.fillAmount = (float)(FrameReplay.Instance.LogicFrames) / CombatData.Ins.GRecord.frameCount;
        ProgressText.text = string.Format("{0}%", Mathf.FloorToInt(Fill.fillAmount * 100));
    }

    public void InsertFightMessage(string text)
    {
        if (LevelTalkRoot != null && ctrl != null)
            ctrl.PushMessage(text);
    }

    public void OnBattleStart()
    {
        currentHP = nextHp = Main.Ins.LocalPlayer.Attr.hpCur;
        hpBar.fillAmount = currentHP / (float)Main.Ins.LocalPlayer.Attr.HpMax;
    }

    public void OnBattleEnd()
    {
        U3D.InsertSystemMsg("录像播放完成");
        Main.Ins.GameBattleEx.UpdateTime();
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
            TargetInfoLast = 0.5f;
            return;
        }

        CheckHideTarget = true;
        TargetInfoLast = 0.5f;
        TargetHp.fillAmount = (float)mon.Attr.hpCur / (float)mon.Attr.TotalHp;
        TargetHPLabel.text = ((int)(mon.Attr.hpCur / 10.0f)).ToString() + "/" + ((int)(mon.Attr.TotalHp / 10.0f)).ToString();
        currentTargetHp = mon.Attr.hpCur;
        nextTargetHp = mon.Attr.hpCur;
        TargetName.text = mon.name;
        CurrentMonster = mon;
    }
    
    public void Update()
    {
        if (currentHP != nextHp)
        {
            currentHP = Mathf.FloorToInt(Mathf.MoveTowards(currentHP, nextHp, FrameReplay.deltaTime));
            hpBar.fillAmount = currentHP / (float)Main.Ins.LocalPlayer.Attr.TotalHp;
        }

        if (currentTargetHp != nextTargetHp)
        {
            currentTargetHp = Mathf.FloorToInt(Mathf.MoveTowards(currentTargetHp, nextTargetHp, FrameReplay.deltaTime));
            TargetHp.fillAmount = currentTargetHp / (float)CurrentMonster.Attr.TotalHp;
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
        TargetInfoLast = 0.5f;
    }

    bool CheckHideTarget = false;
    float TargetInfoLast = 0.5f;
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

    public void UpdateAngryBar()
    {
        if (Main.Ins.LocalPlayer != null && !Main.Ins.LocalPlayer.Dead)
            angryBar.fillAmount = Main.Ins.LocalPlayer.AngryValue / CombatData.ANGRYMAX;
    }
}