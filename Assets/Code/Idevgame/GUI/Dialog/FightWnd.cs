using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.UI;
using Idevgame.GameState;
using Idevgame.GameState.DialogState;

public class FightDialogState : PersistDialog<FightUiConroller>
{
    public override string DialogName { get { return "FightWnd"; } }
    public FightDialogState() : base()
    {

    }
}

public class FightUiConroller : Dialog
{
    public override void OnDialogStateEnter(PersistState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
    }


    Text timeLabel;
    Image hpBar;
    Image angryBar;
    Text hpLabel;

    //角色面板，暂时取消
    void OnPlayerInfo()
    {
        if (PlayerDialogState.Exist())
            Main.Instance.ExitState(Main.Instance.PlayerDialogState);
        else
            Main.Instance.EnterState(Main.Instance.PlayerDialogState);
    }

    public void HideCameraBtn()
    {
        if (Unlock != null)
            Unlock.SetActive(false);
    }

    public void EnableJoyStick()
    {
        if (NGUIJoystick.instance != null)
            NGUIJoystick.instance.JoyCollider.enabled = true;
    }

    public void DisableJoyStick()
    {
        if (NGUIJoystick.instance != null)
            NGUIJoystick.instance.JoyCollider.enabled = false;
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
    void Init()
    {
        clickPanel = Control("ClickPanel");
        LevelTalkRoot = Control("LevelTalk", WndObject).transform;
        ctrl = LevelTalkRoot.GetComponent<AutoMsgCtrl>();
        ctrl.SetConfig(2.0f, 1.5f);
        //联机不需要剧情对白面板，而使用房间聊天面板单独代替.
        if (Main.Instance.CombatData.GLevelMode == LevelMode.MultiplyPlayer)
        {
            GameObject.Destroy(Control("BattleInfo").gameObject);
        }
        else
        {
            Control("BattleInfo").GetComponent<RectTransform>().anchoredPosition = new Vector2(Main.Instance.GameStateMgr.gameStatus.ShowSysMenu2 ? 145 : -20, -175);
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
        NodeHelper.Find("SceneName", WndObject).GetComponentInChildren<Text>().text = Main.Instance.CombatData.GLevelItem.Name;
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
        NodeHelper.Find("SysMenu2", WndObject).SetActive(
            (Main.Instance.CombatData.GLevelMode == LevelMode.CreateWorld && Main.Instance.GameStateMgr.gameStatus.ShowSysMenu2) ||
            (Main.Instance.CombatData.GLevelMode == LevelMode.SinglePlayerTask && Main.Instance.GameStateMgr.gameStatus.ShowSysMenu2) ||
            (Main.Instance.CombatData.GLevelMode == LevelMode.MultiplyPlayer));
        NodeHelper.Find("Reborn", WndObject).GetComponentInChildren<Button>().onClick.AddListener(OnRebornClick);

        //单机
        if (Main.Instance.CombatData.GLevelMode == LevelMode.SinglePlayerTask && Main.Instance.CombatData.GLevelItem.ID == 4)
            NodeHelper.Find("Reborn", WndObject).SetActive(true);
        else
        {
            //创建关卡，非暗杀，都不允许复活
            if (Main.Instance.CombatData.GGameMode != GameMode.ANSHA)
                NodeHelper.Find("Reborn", WndObject).SetActive(false);
        }

        //联机屏蔽按键-多人游戏
        if (Main.Instance.CombatData.GLevelMode == LevelMode.MultiplyPlayer)
        {
            //联机还无法复活队友.
            NodeHelper.Find("Reborn", WndObject).SetActive(false);
        }
        else
        {
            //非联机屏蔽按键-单人游戏
            NodeHelper.Find("Chat", WndObject).SetActive(false);
        }
#if !STRIP_DBG_SETTING
        NodeHelper.Find("DBG", WndObject).GetComponent<Button>().onClick.AddListener(OnDebugCanvas);
#else
        NodeHelper.Find("DBG", WndObject).SetActive(false);
#endif
        if (Main.Instance.MeteorManager.LocalPlayer != null)
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
            c[i].alpha = Main.Instance.GameStateMgr.gameStatus.UIAlpha;
#if (UNITY_EDITOR || UNITY_STANDALONE_WIN) && !STRIP_KEYBOARD
        Control("ClickPanel").SetActive(false);
        Control("JoyArrow").SetActive(false);
#endif
    }

    public override void OnRefresh(int message, object param)
    {
        base.OnRefresh(message, param);
        UpdateUIButton();
    }

    void OnChatClick()
    {
        if (!ChatDialogState.Exist())
            Main.Instance.EnterState(Main.Instance.ChatDialogState);
        else
            Main.Instance.ExitState(Main.Instance.ChatDialogState);
    }

#if !STRIP_DBG_SETTING
    void OnDebugCanvas()
    {
        U3D.Instance.ShowDbg();
    }
#endif

    void OnStatusPress()
    {
        if (!BattleStatusDialogState.Exist())
            Main.Instance.EnterState(Main.Instance.BattleStatusDialogState);
    }

    void OnStatusRelease()
    {
        if (BattleStatusDialogState.Exist())
        {
            Main.Instance.ExitState(Main.Instance.BattleStatusDialogState);
        }
    }

    void OnRebornClick()
    {
        if (Main.Instance.CombatData.GLevelMode == LevelMode.SinglePlayerTask)
        {
            if (Main.Instance.MeteorManager.LocalPlayer.posMng.mActiveAction.Idx == CommonAction.Idle ||
                Main.Instance.MeteorManager.LocalPlayer.posMng.mActiveAction.Idx == CommonAction.Run ||
                Main.Instance.MeteorManager.LocalPlayer.posMng.mActiveAction.Idx == CommonAction.RunOnDrug)
                Main.Instance.MeteorManager.LocalPlayer.posMng.ChangeAction(CommonAction.Reborn);
        }
        else if (Main.Instance.CombatData.GLevelMode == LevelMode.CreateWorld)
        {
            if (Main.Instance.CombatData.GGameMode == GameMode.ANSHA)
            {
                if (Main.Instance.MeteorManager.LocalPlayer.posMng.mActiveAction.Idx == CommonAction.Idle ||
                    Main.Instance.MeteorManager.LocalPlayer.posMng.mActiveAction.Idx == CommonAction.Run ||
                    Main.Instance.MeteorManager.LocalPlayer.posMng.mActiveAction.Idx == CommonAction.RunOnDrug)
                    Main.Instance.MeteorManager.LocalPlayer.posMng.ChangeAction(CommonAction.Reborn);
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
        NodeHelper.Find("WeaponSelect", gameObject).SetActive(Main.Instance.GameStateMgr.gameStatus.EnableWeaponChoose);
        NodeHelper.Find("SfxMenu", gameObject).SetActive(Main.Instance.GameStateMgr.gameStatus.EnableDebugSFX);
        NodeHelper.Find("Robot", gameObject).SetActive(Main.Instance.GameStateMgr.gameStatus.EnableDebugRobot);
#if !STRIP_DBG_SETTING
        NodeHelper.Find("DBG", gameObject).SetActive(Main.Instance.GameStateMgr.gameStatus.LevelDebug);
#endif
        NodeHelper.Find("MiniMap", gameObject).SetActive(true);

        if (NGUIJoystick.instance != null)
        {
            if (Main.Instance.GameStateMgr.gameStatus.DisableJoystick)
                NGUIJoystick.instance.OnDisabled();
            else
                NGUIJoystick.instance.OnEnabled();
        }
#if !STRIP_DBG_SETTING
        if (Main.Instance.GameStateMgr.gameStatus.EnableLog)
            WSDebug.Ins.OpenLogView();
        else
            WSDebug.Ins.CloseLogView();
#endif
        if (NGUIJoystick.instance != null)
            NGUIJoystick.instance.SetAnchor(Main.Instance.GameStateMgr.gameStatus.JoyAnchor);
        int j = 0;
        for (int i = 0; i < clickPanel.transform.childCount; i++)
        {
            Transform tri = clickPanel.transform.GetChild(i);
            if (tri.name == "Direction")
                continue;
            RectTransform r = tri.GetComponent<RectTransform>();
            if (Main.Instance.GameStateMgr.gameStatus.HasUIAnchor[j])
                r.anchoredPosition = Main.Instance.GameStateMgr.gameStatus.UIAnchor[j];
            j++;
        }
    }

    void OnAttackPress()
    {
        if (Main.Instance.CombatData.GMeteorInput == null || Main.Instance.CombatData.PauseAll) return;
        Main.Instance.CombatData.GMeteorInput.OnKeyDownProxy(EKeyList.KL_Attack, false);//也可看作普攻
    }

    public void OnChangeLock(bool locked)
    {
        LockSprite.enabled = locked;
    }

    void OnClickChangeLock()
    {
        if (Main.Instance.CombatData.GMeteorInput == null || Main.Instance.CombatData.PauseAll) return;
        //远程武器禁止切换锁定状态
        int weaponEquiped = Main.Instance.MeteorManager.LocalPlayer.GetWeaponType();
        if (weaponEquiped == (int)EquipWeaponType.Gun || weaponEquiped == (int)EquipWeaponType.Dart || weaponEquiped == (int)EquipWeaponType.Guillotines)
            return;

        if (Main.Instance.GameBattleEx.bLocked)
            Main.Instance.GameBattleEx.Unlock();
        else
            Main.Instance.GameBattleEx.Lock();
    }

    void OnClickDrop()
    {
        Main.Instance.MeteorManager.LocalPlayer.DropWeapon();
    }

    void OnCrouchPress()
    {
        if (Main.Instance.MeteorManager.LocalPlayer.Dead)
            return;

        if (Main.Instance.CombatData.GMeteorInput == null || Main.Instance.CombatData.PauseAll) return;
        Main.Instance.CombatData.GMeteorInput.OnKeyDownProxy(EKeyList.KL_Crouch, false);
    }

    void OnCrouchRelease()
    {
        if (Main.Instance.CombatData.GMeteorInput == null || Main.Instance.CombatData.PauseAll) return;
        Main.Instance.CombatData.GMeteorInput.OnKeyUpProxy(EKeyList.KL_Crouch);
    }

    void OnChangeWeaponPress()
    {
        if (Main.Instance.MeteorManager.LocalPlayer.Dead)
            return;

        if (Main.Instance.CombatData.GMeteorInput == null || Main.Instance.CombatData.PauseAll) return;
        Main.Instance.CombatData.GMeteorInput.OnKeyDownProxy(EKeyList.KL_ChangeWeapon, false);
    }

    void OnChangeWeaponRelease()
    {
        if (Main.Instance.MeteorManager.LocalPlayer.Dead)
            return;

        if (Main.Instance.CombatData.GMeteorInput == null || Main.Instance.CombatData.PauseAll) return;
        Main.Instance.CombatData.GMeteorInput.OnKeyUpProxy(EKeyList.KL_ChangeWeapon);
    }

    void OnAttackRelease()
    {
        if (Main.Instance.CombatData.GMeteorInput == null || Main.Instance.CombatData.PauseAll) return;
        Main.Instance.CombatData.GMeteorInput.OnKeyUpProxy(EKeyList.KL_Attack);
    }

    void OnDefencePress()
    {
        if (Main.Instance.MeteorManager.LocalPlayer.Dead)
            return;
        if (Main.Instance.CombatData.GMeteorInput == null || Main.Instance.CombatData.PauseAll) return;
        Main.Instance.CombatData.GMeteorInput.OnKeyDownProxy(EKeyList.KL_Defence, true);//不要被键盘状态同步，否则按下马上就抬起，那么防御姿势就消失了

    }

    void OnDefenceRelease()
    {
        if (Main.Instance.MeteorManager.LocalPlayer.Dead)
            return;
        if (Main.Instance.CombatData.GMeteorInput == null || Main.Instance.CombatData.PauseAll) return;
        Main.Instance.CombatData.GMeteorInput.OnKeyUpProxy(EKeyList.KL_Defence);
    }

    void OnJumpPress()
    {
        //if (!MeteorManager.Instance.LocalPlayer.posMng.CanJump)
        //    return;

        if (Main.Instance.CombatData.GMeteorInput == null || Main.Instance.CombatData.PauseAll) return;
        Main.Instance.CombatData.GMeteorInput.OnKeyDownProxy(EKeyList.KL_Jump, false);//
    }

    void OnJumpRelease()
    {
        if (Main.Instance.CombatData.GMeteorInput == null || Main.Instance.CombatData.PauseAll) return;
        Main.Instance.CombatData.GMeteorInput.OnKeyUpProxy(EKeyList.KL_Jump);
    }

    //按爆气.
    public void OnBreakOut()
    {
        //Debug.Log("OnBreakOut");
        if (Main.Instance.CombatData.GMeteorInput == null || Main.Instance.CombatData.PauseAll)
            return;
        if (Main.Instance.MeteorManager.LocalPlayer.AngryValue >= 60 || Main.Instance.GameStateMgr.gameStatus.EnableInfiniteAngry)
        {
            Main.Instance.CombatData.GMeteorInput.OnKeyDownProxy(EKeyList.KL_BreakOut, false);
            //Debug.Log("OnKeyDown");
        }
    }

    //int lastAngry = 0;
    public void UpdateAngryBar()
    {
        if (Main.Instance.MeteorManager.LocalPlayer != null && !Main.Instance.MeteorManager.LocalPlayer.Dead)
        {
            angryBar.fillAmount = (float)Main.Instance.MeteorManager.LocalPlayer.AngryValue / (float)CombatData.ANGRYMAX;
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
        currentHP = nextHp = Main.Instance.MeteorManager.LocalPlayer.Attr.hpCur;
        hpBar.fillAmount = currentHP / (float)Main.Instance.MeteorManager.LocalPlayer.Attr.HpMax;
    }

    public void OnBattleEnd()
    {
        NodeHelper.Find("Status", WndObject).SetActive(false);
        NodeHelper.Find("Chat", WndObject).SetActive(false);
        //hpBar.fillAmount = (float)MeteorManager.Instance.LocalPlayer.Attr.hpCur / (float)MeteorManager.Instance.LocalPlayer.Attr.HpMax;
    }

    Dictionary<Buff, GameObject> enemyBuffList = new Dictionary<Buff, GameObject>();
    MeteorUnit CurrentMonster;
    public void UpdateMonsterInfo(MeteorUnit mon)
    {
        if (!Main.Instance.GameStateMgr.gameStatus.ShowBlood)
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
        //targetTitleInfo.text = mon.name;
        currentTargetHp = mon.Attr.hpCur;
        nextTargetHp = mon.Attr.hpCur;
        TargetName.text = mon.name;
        //if (updateEnemyBuff != null)
        //{
        //    GameBattleEx.Instance.StopCoroutine(updateEnemyBuff);
        //    updateEnemyBuff = null;
        //}
        //foreach (var each in enemyBuffList)
        //    GameObject.Destroy(each.Value);
        //enemyBuffList.Clear();

        //if (!mon.Dead)
        //{
        //    foreach (var each in BuffMng.Instance.BufDict)
        //    {
        //        if (!enemyBuffList.ContainsKey(each.Value) && each.Value.Units.ContainsKey(mon))
        //        {
        //            GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>("BuffItem"));// new GameObject(buf.Iden);
        //            obj.name = each.Value.Iden;
        //            obj.transform.SetParent(TargetBuffPanel.transform);
        //            obj.transform.localScale = Vector3.one;
        //            obj.transform.localPosition = Vector3.zero;
        //            obj.transform.localRotation = Quaternion.identity;
        //            obj.layer = TargetBuffPanel.layer;
        //            enemyBuffList.Add(each.Value, obj);

        //            GameObject BuffImg = Control("BuffImg", enemyBuffList[each.Value]);
        //            BuffImg.GetComponent<Image>().fillAmount = each.Value.Units[mon].refresh_tick / (each.Value.last_time / 10);
        //            GameObject BuffLength = Control("BuffLength", enemyBuffList[each.Value]);
        //            BuffLength.GetComponent<Text>().text = string.Format("{0:F1}", each.Value.Units[mon].refresh_tick);

        //            GameObject BuffName = Control("BuffName", enemyBuffList[each.Value]);
        //            BuffName.GetComponent<Text>().text = each.Value.Iden;
        //        }
        //    }
        //    if (updateEnemyBuff == null)
        //        updateEnemyBuff = GameBattleEx.Instance.StartCoroutine(UpdateEnemyBuff());
        //}
        CurrentMonster = mon;
        //if (hideTargetInfo != null)
        //    GameBattleEx.Instance.StopCoroutine(hideTargetInfo);
        //hideTargetInfo = GameBattleEx.Instance.StartCoroutine(HideTargetInfo());
    }

    //Coroutine updateEnemyBuff;
    //IEnumerator UpdateEnemyBuff()
    //{
    //    while (true)
    //    {
    //        try
    //        {
    //            foreach (var each in enemyBuffList)
    //            {
    //                if (currentMonster != null && each.Key.Units.ContainsKey(currentMonster))
    //                {
    //                    GameObject BuffImg = Control("BuffImg", each.Value);
    //                    BuffImg.GetComponent<Image>().fillAmount = each.Key.Units[currentMonster].refresh_tick / (each.Key.last_time / 10);
    //                    GameObject BuffLength = Control("BuffLength", each.Value);
    //                    BuffLength.GetComponent<Text>().text = string.Format("{0:F1}", each.Key.Units[currentMonster].refresh_tick);
    //                }
    //            }
    //        }
    //        catch (Exception exp)
    //        {
    //            Debug.LogError(exp.Message + exp.StackTrace);
    //        }
    //        yield return 0;
    //    }
    //}

    public void Update()
    {
        if (currentHP != nextHp)
        {
            currentHP = Mathf.MoveTowards(currentHP, nextHp, 1000f * Time.deltaTime);
            hpBar.fillAmount = currentHP / (float)Main.Instance.MeteorManager.LocalPlayer.Attr.TotalHp;
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
        if (Main.Instance.MeteorManager.LocalPlayer != null && Main.Instance.MeteorManager.LocalPlayer.Attr.hpCur >= 0)
        {
            hpLabel.text = ((int)(Main.Instance.MeteorManager.LocalPlayer.Attr.hpCur / 10.0f)).ToString() + "/" + ((int)(Main.Instance.MeteorManager.LocalPlayer.Attr.HpMax / 10.0f)).ToString();
            nextHp = Main.Instance.MeteorManager.LocalPlayer.Attr.hpCur;
            UpdateAngryBar();
        }
    }

    //Dictionary<Buff, GameObject> buffList = new Dictionary<Buff, GameObject>();
    //Coroutine updateBuff;
    //public void AddBuff(Buff buf)
    //{
    //    if (updateBuff == null)
    //        updateBuff = GameBattleEx.Instance.StartCoroutine(UpdateBuff());
    //    if (buffList.ContainsKey(buf))
    //    {
    //        GameObject BuffName = Control("BuffName", buffList[buf]);
    //        BuffName.GetComponent<Text>().text = buf.Iden;
    //        GameObject BuffImg = Control("BuffImg", buffList[buf]);
    //        BuffImg.GetComponent<Image>().fillAmount = 1;
    //        GameObject BuffLength = Control("BuffLength", buffList[buf]);
    //        BuffLength.GetComponent<Text>().text = string.Format("{0:F2}", buf.last_time / 10);
    //    }
    //    else
    //    {
    //        GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>("BuffItem"));// new GameObject(buf.Iden);
    //        obj.name = buf.Iden;
    //        obj.transform.SetParent(BuffRoot.transform);
    //        obj.transform.localScale = Vector3.one;
    //        obj.transform.localPosition = Vector3.zero;
    //        obj.transform.localRotation = Quaternion.identity;
    //        obj.layer = BuffRoot.layer;
    //        buffList.Add(buf, obj);
    //        GameObject BuffName = Control("BuffName", buffList[buf]);
    //        BuffName.GetComponent<Text>().text = buf.Iden;
    //        GameObject BuffImg = Control("BuffImg", buffList[buf]);
    //        BuffImg.GetComponent<Image>().fillAmount = 1;
    //        GameObject BuffLength = Control("BuffLength", buffList[buf]);
    //        BuffLength.GetComponent<Text>().text = string.Format("{0:F1}", buf.last_time / 10);
    //    }
    //}

    //public IEnumerator UpdateBuff()
    //{
    //    while (true)
    //    {
    //        try
    //        {
    //            foreach (var each in buffList)
    //            {
    //                if (each.Key.Units.ContainsKey(MeteorManager.Instance.LocalPlayer))
    //                {
    //                    GameObject BuffImg = Control("BuffImg", each.Value);
    //                    BuffImg.GetComponent<Image>().fillAmount = each.Key.Units[MeteorManager.Instance.LocalPlayer].refresh_tick / (each.Key.last_time / 10);
    //                    GameObject BuffLength = Control("BuffLength", each.Value);
    //                    BuffLength.GetComponent<Text>().text = string.Format("{0:F1}", each.Key.Units[MeteorManager.Instance.LocalPlayer].refresh_tick);
    //                }
    //            }
    //        }
    //        catch (Exception exp)
    //        {
    //            Debug.LogError(exp.Message + exp.StackTrace);
    //        }
    //        yield return 0;
    //    }
    //}

    //public void RemoveBuff(Buff buf, MeteorUnit unit = null)
    //{
    //    if (unit == null)
    //    {
    //        if (buffList.ContainsKey(buf))
    //        {
    //            GameObject.Destroy(buffList[buf]);
    //            buffList.Remove(buf);
    //        }
    //    }
    //    else
    //    {
    //        if (enemyBuffList.ContainsKey(buf))
    //        {
    //            GameObject.Destroy(enemyBuffList[buf]);
    //            enemyBuffList.Remove(buf);
    //        }
    //    }
    //}
}