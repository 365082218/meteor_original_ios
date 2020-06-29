using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.UI;
using Idevgame.GameState;
using Idevgame.GameState.DialogState;

public class ReplayState : PersistDialog<ReplayUiConroller>
{
    public override string DialogName { get { return "ReplayWnd"; } }
    public ReplayState() : base()
    {

    }
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

    public override void OnDialogStateEnter(PersistState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
    }

    void Init()
    {
        clickPanel = Control("ClickPanel");
        LevelTalkRoot = Control("LevelTalk", WndObject).transform;
        ctrl = LevelTalkRoot.GetComponent<AutoMsgCtrl>();
        ctrl.SetConfig(2.0f, 1.5f);
        Control("BattleInfo").GetComponent<RectTransform>().anchoredPosition = new Vector2(Main.Ins.GameStateMgr.gameStatus.ShowSysMenu2 ? 145 : -20, -175);
        NodeHelper.Find("SceneName", WndObject).GetComponent<Button>().onClick.AddListener(() => { OpenMiniMap(); });
        NodeHelper.Find("SceneName", WndObject).GetComponentInChildren<Text>().text = Main.Ins.CombatData.GLevelItem.Name;
        NodeHelper.Find("System", WndObject).GetComponentInChildren<Button>().onClick.AddListener(() => { U3D.OpenSystemWnd(); });
        timeLabel = NodeHelper.Find("GameTime", WndObject).GetComponent<Text>();
        hpBar = Control("HPBar", WndObject).gameObject.GetComponent<Image>();
        angryBar = Control("AngryBar", WndObject).gameObject.GetComponent<Image>();
        hpLabel = Control("HPLabel", WndObject).gameObject.GetComponent<Text>();
        NodeHelper.Find("Status", WndObject).GetComponentInChildren<GameButton>().OnPress.AddListener(OnStatusPress);
        NodeHelper.Find("Status", WndObject).GetComponentInChildren<GameButton>().OnRelease.AddListener(OnStatusRelease);
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
        UpdateUIButton();
        CanvasGroup[] c = WndObject.GetComponentsInChildren<CanvasGroup>();
        for (int i = 0; i < c.Length; i++)
            c[i].alpha = Main.Ins.GameStateMgr.gameStatus.UIAlpha;
    }

    void OnStatusPress()
    {
        if (!BattleStatusDialogState.Exist())
            Main.Ins.EnterState(Main.Ins.BattleStatusDialogState);
    }

    void OnStatusRelease()
    {
        if (BattleStatusDialogState.Exist())
            Main.Ins.ExitState(Main.Ins.BattleStatusDialogState);
    }

    bool openMiniMap = false;
    void OpenMiniMap()
    {
        openMiniMap = !openMiniMap;
        NodeHelper.Find("MiniMapFrame", WndObject).SetActive(openMiniMap);
    }
    public GameObject clickPanel;
    public void UpdateUIButton()
    {
        NodeHelper.Find("MiniMap", gameObject).SetActive(true);

        if (CameraController.Ins != null)
            CameraController.Ins.SetAnchor(Main.Ins.GameStateMgr.gameStatus.JoyAnchor);
    }

    public void UpdateTime(string label)
    {
        timeLabel.text = label;
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
        NodeHelper.Find("Status", WndObject).SetActive(false);
        NodeHelper.Find("Chat", WndObject).SetActive(false);
    }

    MeteorUnit CurrentMonster;
    public void UpdateMonsterInfo(MeteorUnit mon)
    {
        if (!Main.Ins.GameStateMgr.gameStatus.ShowBlood)
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
        if (currentHP != nextHp)
        {
            currentHP = Mathf.MoveTowards(currentHP, nextHp, 1000f * Time.deltaTime);
            hpBar.fillAmount = currentHP / (float)Main.Ins.LocalPlayer.Attr.TotalHp;
        }

        if (currentTargetHp != nextTargetHp)
        {
            currentTargetHp = Mathf.MoveTowards(currentTargetHp, nextTargetHp, 1000f * Time.deltaTime);
            TargetHp.fillAmount = currentTargetHp / CurrentMonster.Attr.TotalHp;
        }

        if (CheckHideTarget)
        {
            TargetInfoLast -= Time.deltaTime;
            if (TargetInfoLast <= 0.0f)
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

    public void UpdateAngryBar()
    {
        if (Main.Ins.LocalPlayer != null && !Main.Ins.LocalPlayer.Dead)
            angryBar.fillAmount = (float)Main.Ins.LocalPlayer.AngryValue / (float)CombatData.ANGRYMAX;
    }
}