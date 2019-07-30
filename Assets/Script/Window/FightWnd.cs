using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.UI;
public class FightWnd : Window<FightWnd>
{
    public override string PrefabName { get { return "FightWnd"; } }
    protected override bool OnOpen()
    {
        ObjectInit();
        Init();
        return true;
    }
    protected override bool OnShow()
    {
        int size = UIMoveControl.mUIMoveControlList.Count;
        for (int i = 0; i < size; i++)
        {
            UIMoveControl.mUIMoveControlList[i].ShowAction();
        }

        return base.OnShow();
    }

    public override void Show()
    {
        base.Show();
    }

    void ObjectInit()
    {

    }

    Text timeLabel;
    Image hpBar;
    Image angryBar;
    Text hpLabel;

    //角色面板，暂时取消
    void OnPlayerInfo()
    {
        if (PlayerWnd.Exist)
            PlayerWnd.Instance.Close();
        else
            PlayerWnd.Instance.Open();
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
    Animation actionStatusBarCtrl;
    GameObject FloatOpen;
    GameObject Unlock;
    Image LockSprite;
    GameObject TargetBlood;
    Image TargetHp;
    Text TargetHPLabel;
    Text TargetName;
    void Init()
    {
        LevelTalkRoot = Global.ldaControlX("LevelTalk", WndObject).transform;
        ctrl = LevelTalkRoot.GetComponent<AutoMsgCtrl>();
        ctrl.SetConfig(2.0f, 1.5f);
        FloatOpen = Control("FloatOpen");
        FloatOpen.GetComponent<Button>().onClick.AddListener(OnChangeActionBarStatus);
        actionStatusBarCtrl = Control("Slots").GetComponent<Animation>();
        //联机不需要剧情对白面板，而使用房间聊天面板单独代替.
        if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
        {
            GameObject.Destroy(Control("BattleInfo").gameObject);
        }
        else
        {
            Control("BattleInfo").GetComponent<RectTransform>().anchoredPosition = new Vector2(GameData.Instance.gameStatus.ShowSysMenu2 ? 145 : -20, -175);
        }
        Global.ldaControlX("Attack", WndObject).GetComponent<GameButton>().OnPress.AddListener(OnAttackPress);
        Global.ldaControlX("Attack", WndObject).GetComponent<GameButton>().OnRelease.AddListener(OnAttackRelease);
        Global.ldaControlX("Defence", WndObject).GetComponentInChildren<GameButton>().OnPress.AddListener(OnDefencePress);
        Global.ldaControlX("Defence", WndObject).GetComponentInChildren<GameButton>().OnRelease.AddListener(OnDefenceRelease);
        Global.ldaControlX("Jump", WndObject).GetComponentInChildren<GameButton>().OnPress.AddListener(OnJumpPress);
        Global.ldaControlX("Jump", WndObject).GetComponentInChildren<GameButton>().OnRelease.AddListener(OnJumpRelease);
        Global.ldaControlX("ChangeWeapon", WndObject).GetComponentInChildren<GameButton>().OnPress.AddListener(OnChangeWeaponPress);
        Global.ldaControlX("ChangeWeapon", WndObject).GetComponentInChildren<GameButton>().OnRelease.AddListener(OnChangeWeaponRelease);
        Global.ldaControlX("BreakOut", WndObject).GetComponentInChildren<GameButton>().OnPress.AddListener(OnBreakOut);
        Global.ldaControlX("WeaponSelect", WndObject).GetComponentInChildren<Button>().onClick.AddListener(() => { U3D.OpenWeaponWnd(); });
        Global.ldaControlX("SceneName", WndObject).GetComponent<Button>().onClick.AddListener(() => { OpenMiniMap(); });
        Global.ldaControlX("SceneName", WndObject).GetComponentInChildren<Text>().text = Global.Instance.GLevelItem.Name;
        Global.ldaControlX("System", WndObject).GetComponentInChildren<Button>().onClick.AddListener(() => { U3D.OpenSystemWnd(); });
        Global.ldaControlX("Crouch", WndObject).GetComponent<GameButton>().OnPress.AddListener(OnCrouchPress);
        Global.ldaControlX("Crouch", WndObject).GetComponent<GameButton>().OnRelease.AddListener(OnCrouchRelease);
        Global.ldaControlX("Drop", WndObject).GetComponent<Button>().onClick.AddListener(OnClickDrop);
        Unlock = Global.ldaControlX("Unlock", WndObject);
        Unlock.GetComponentInChildren<Button>().onClick.AddListener(OnClickChangeLock);
        LockSprite = Global.ldaControlX("LockSprite", Unlock).GetComponent<Image>();
        Global.ldaControlX("SfxMenu", WndObject).GetComponentInChildren<Button>().onClick.AddListener(() => { U3D.OpenSfxWnd(); });
        Global.ldaControlX("Robot", WndObject).GetComponentInChildren<Button>().onClick.AddListener(() => { U3D.OpenRobotWnd(); });
        timeLabel = Global.ldaControlX("GameTime", WndObject).GetComponent<Text>();
        hpBar = ldaControl("HPBar", WndObject).gameObject.GetComponent<Image>();
        angryBar = ldaControl("AngryBar", WndObject).gameObject.GetComponent<Image>();
        hpLabel = ldaControl("HPLabel", WndObject).gameObject.GetComponent<Text>();
        Global.ldaControlX("Status", WndObject).GetComponentInChildren<GameButton>().OnPress.AddListener(OnStatusPress);
        Global.ldaControlX("Status", WndObject).GetComponentInChildren<GameButton>().OnRelease.AddListener(OnStatusRelease);
        Global.ldaControlX("Chat", WndObject).GetComponentInChildren<Button>().onClick.AddListener(OnChatClick);
        Global.ldaControlX("SysMenu2", WndObject).SetActive(
            (Global.Instance.GLevelMode == LevelMode.CreateWorld && GameData.Instance.gameStatus.ShowSysMenu2) ||
            (Global.Instance.GLevelMode == LevelMode.SinglePlayerTask &&  GameData.Instance.gameStatus.ShowSysMenu2) ||
            (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer));
        Global.ldaControlX("Reborn", WndObject).GetComponentInChildren<Button>().onClick.AddListener(OnRebornClick);
        //联机屏蔽按键-多人游戏
        if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
        {
            Global.ldaControlX("Reborn", WndObject).SetActive(false);
        }
        else
        {
            //非联机屏蔽按键-单人游戏
            Global.ldaControlX("Chat", WndObject).SetActive(true);
        }
        if (MeteorManager.Instance.LocalPlayer != null)
        {
            angryBar.fillAmount = 0.0f;
            UpdatePlayerInfo();
        }
        TargetBlood = Control("TargetBlood");
        TargetBlood.SetActive(false);
        TargetHp = ldaControl("HPBar", TargetBlood).GetComponent<Image>();
        TargetHPLabel = ldaControl("TargetHPLabel", TargetBlood).GetComponent<Text>();
        TargetName = ldaControl("TargetName", TargetBlood).GetComponent<Text>();
        UpdateUIButton();
        

        GameBattleEx.Instance.OnUpdates += Update;
        CanvasGroup[] c = WndObject.GetComponentsInChildren<CanvasGroup>();
        for (int i = 0; i < c.Length; i++)
            c[i].alpha = GameData.Instance.gameStatus.UIAlpha;
#if (UNITY_EDITOR || UNITY_STANDALONE_WIN) && !STRIP_KEYBOARD
        Control("ClickPanel").SetActive(false);
        Control("JoyArrow").SetActive(false);
        Control("ActionFloat").SetActive(false);
        Control("FloatOpen").SetActive(false);
#endif
    }

    public override void OnRefresh(int message, object param)
    {
        base.OnRefresh(message, param);
        UpdateUIButton();
    }

    void OnChatClick()
    {
        if (!ChatWnd.Exist)
            ChatWnd.Instance.Open();
        else
            ChatWnd.Instance.Close();
    }

    void OnStatusPress()
    {
        if (!BattleStatusWnd.Exist)
            BattleStatusWnd.Instance.Open();
    }

    void OnStatusRelease()
    {
        if (BattleStatusWnd.Exist)
        {
            BattleStatusWnd.Instance.Close();
        }
    }

    void OnRebornClick()
    {
        if (Global.Instance.GLevelMode == LevelMode.SinglePlayerTask)
        {
            if (MeteorManager.Instance.LocalPlayer.posMng.mActiveAction.Idx == CommonAction.Idle ||
                MeteorManager.Instance.LocalPlayer.posMng.mActiveAction.Idx == CommonAction.Run ||
                MeteorManager.Instance.LocalPlayer.posMng.mActiveAction.Idx == CommonAction.RunOnDrug)
                MeteorManager.Instance.LocalPlayer.posMng.ChangeAction(CommonAction.Reborn);
        }
        else if (Global.Instance.GLevelMode == LevelMode.CreateWorld)
        {
            if (Global.Instance.GGameMode == GameMode.ANSHA)
            {
                if (MeteorManager.Instance.LocalPlayer.posMng.mActiveAction.Idx == CommonAction.Idle || 
                    MeteorManager.Instance.LocalPlayer.posMng.mActiveAction.Idx == CommonAction.Run ||
                    MeteorManager.Instance.LocalPlayer.posMng.mActiveAction.Idx == CommonAction.RunOnDrug)
                    MeteorManager.Instance.LocalPlayer.posMng.ChangeAction(CommonAction.Reborn);
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

    bool actionBarStatus = false;
    void OnChangeActionBarStatus()
    {
        FloatOpen.GetComponent<Button>().interactable = false;
        Startup.ins.StartCoroutine(actionStatusBarCtrl.PlayAnimation(actionBarStatus ? "HideActionBar" : "ShowActionBar", false, ()=> {
            FloatOpen.GetComponent<Button>().interactable = true;
            actionBarStatus = !actionBarStatus;
            FloatOpen.transform.rotation = Quaternion.Euler(0, 0, actionBarStatus ? 90 : 270);
        }));
    }

    bool openMiniMap = false;
    void OpenMiniMap()
    {
        openMiniMap = !openMiniMap;
        Global.ldaControlX("MiniMapFrame", WndObject).SetActive(openMiniMap);
    }

    public void UpdateUIButton()
    {
        Global.ldaControlX("WeaponSelect", WndObject).SetActive(GameData.Instance.gameStatus.EnableWeaponChoose);
        Global.ldaControlX("SfxMenu", WndObject).SetActive(GameData.Instance.gameStatus.EnableDebugSFX);
        Global.ldaControlX("Robot", WndObject).SetActive(GameData.Instance.gameStatus.EnableDebugRobot);
        Global.ldaControlX("MiniMap", WndObject).SetActive(true);

        if (NGUIJoystick.instance != null)
        {
            if (GameData.Instance.gameStatus.DisableJoystick)
                NGUIJoystick.instance.OnDisabled();
            else
                NGUIJoystick.instance.OnEnabled();
        }

        if (GameData.Instance.gameStatus.LevelDebug)
            GamePool.Instance.ShowDbg();
        else
            GamePool.Instance.CloseDbg();
        if (GameData.Instance.gameStatus.EnableLog)
            WSDebug.Ins.OpenLogView();
        else
            WSDebug.Ins.CloseLogView();

        GameObject clickPanel = Control("ClickPanel").gameObject;
        if (NGUIJoystick.instance != null)
            NGUIJoystick.instance.SetAnchor(GameData.Instance.gameStatus.JoyAnchor);
        int j = 0;
        for (int i = 0; i < clickPanel.transform.childCount; i++)
        {
            Transform tri = clickPanel.transform.GetChild(i);
            if (tri.name == "Direction")
                continue;
            RectTransform r = tri.GetComponent<RectTransform>();
            if (GameData.Instance.gameStatus.HasUIAnchor[j])
                r.anchoredPosition = GameData.Instance.gameStatus.UIAnchor[j];
            j++;
        }
    }

    void OnAttackPress()
    {
        if (Global.Instance.GMeteorInput == null || Global.Instance.PauseAll) return;
        Global.Instance.GMeteorInput.OnKeyDownProxy(EKeyList.KL_Attack, false);//也可看作普攻
    }

    public void OnChangeLock(bool locked)
    {
        LockSprite.enabled = locked;
    }

    void OnClickChangeLock()
    {
        if (Global.Instance.GMeteorInput == null || Global.Instance.PauseAll) return;
        //远程武器禁止切换锁定状态
        int weaponEquiped = MeteorManager.Instance.LocalPlayer.GetWeaponType();
        if (weaponEquiped == (int)EquipWeaponType.Gun || weaponEquiped == (int)EquipWeaponType.Dart || weaponEquiped == (int)EquipWeaponType.Guillotines)
            return;

        if (GameBattleEx.Instance.bLocked)
            GameBattleEx.Instance.Unlock();
        else
            GameBattleEx.Instance.Lock();
    }

    void OnClickDrop()
    {
        MeteorManager.Instance.LocalPlayer.DropWeapon();
    }

    void OnCrouchPress()
    {
        if (MeteorManager.Instance.LocalPlayer.Dead)
            return;

        if (Global.Instance.GMeteorInput == null || Global.Instance.PauseAll) return;
            Global.Instance.GMeteorInput.OnKeyDownProxy(EKeyList.KL_Crouch, false);
    }

    void OnCrouchRelease()
    {
        if (Global.Instance.GMeteorInput == null || Global.Instance.PauseAll) return;
            Global.Instance.GMeteorInput.OnKeyUpProxy(EKeyList.KL_Crouch);
    }

    void OnChangeWeaponPress()
    {
        if (MeteorManager.Instance.LocalPlayer.Dead)
            return;

        if (Global.Instance.GMeteorInput == null || Global.Instance.PauseAll) return;
            Global.Instance.GMeteorInput.OnKeyDownProxy(EKeyList.KL_ChangeWeapon, false);
    }

    void OnChangeWeaponRelease()
    {
        if (MeteorManager.Instance.LocalPlayer.Dead)
            return;

        if (Global.Instance.GMeteorInput == null || Global.Instance.PauseAll) return;
            Global.Instance.GMeteorInput.OnKeyUpProxy(EKeyList.KL_ChangeWeapon);
    }

    void OnAttackRelease()
    {
        if (Global.Instance.GMeteorInput == null || Global.Instance.PauseAll) return;
        Global.Instance.GMeteorInput.OnKeyUpProxy(EKeyList.KL_Attack);
    }

    void OnDefencePress()
    {
        if (MeteorManager.Instance.LocalPlayer.Dead)
            return;
        if (Global.Instance.GMeteorInput == null || Global.Instance.PauseAll) return;
            Global.Instance.GMeteorInput.OnKeyDownProxy(EKeyList.KL_Defence, true);//不要被键盘状态同步，否则按下马上就抬起，那么防御姿势就消失了
        
    }

    void OnDefenceRelease()
    {
        if (MeteorManager.Instance.LocalPlayer.Dead)
            return;
        if (Global.Instance.GMeteorInput == null || Global.Instance.PauseAll) return;
            Global.Instance.GMeteorInput.OnKeyUpProxy(EKeyList.KL_Defence);
    }

    void OnJumpPress()
    {
        //if (!MeteorManager.Instance.LocalPlayer.posMng.CanJump)
        //    return;

        if (Global.Instance.GMeteorInput == null || Global.Instance.PauseAll) return;
        Global.Instance.GMeteorInput.OnKeyDownProxy(EKeyList.KL_Jump, false);//
    }

    void OnJumpRelease()
    {
        if (Global.Instance.GMeteorInput == null || Global.Instance.PauseAll) return;
        Global.Instance.GMeteorInput.OnKeyUpProxy(EKeyList.KL_Jump);
    }

    //按爆气.
    public void OnBreakOut()
    {
        //Debug.Log("OnBreakOut");
        if (Global.Instance.GMeteorInput == null || Global.Instance.PauseAll)
            return;
        if (MeteorManager.Instance.LocalPlayer.AngryValue >= 60 || GameData.Instance.gameStatus.EnableInfiniteAngry)
        {
            Global.Instance.GMeteorInput.OnKeyDownProxy(EKeyList.KL_BreakOut, false);
            //Debug.Log("OnKeyDown");
        }
    }

    //int lastAngry = 0;
    public void UpdateAngryBar()
    {
        if (MeteorManager.Instance.LocalPlayer != null && !MeteorManager.Instance.LocalPlayer.Dead)
        {
            angryBar.fillAmount = (float)MeteorManager.Instance.LocalPlayer.AngryValue /(float)Global.ANGRYMAX;
        }
    }

    public void UpdateTime(string label)
    {
        timeLabel.text = label;
    }

    protected override bool OnClose()
    {
        if (GameBattleEx.Instance != null)
            GameBattleEx.Instance.OnUpdates -= Update;
        return base.OnClose();
    }

    //弹出剧情.
    public void InsertFightMessage(string text)
    {
        if (LevelTalkRoot != null && ctrl != null)
            ctrl.PushMessage(text);
    }

    public void OnBattleStart()
    {
        currentHP = nextHp = MeteorManager.Instance.LocalPlayer.Attr.hpCur;
        hpBar.fillAmount = currentHP / (float)MeteorManager.Instance.LocalPlayer.Attr.HpMax;
    }

    public void OnBattleEnd()
    {
        Global.ldaControlX("Status", WndObject).SetActive(false);
        Global.ldaControlX("Chat", WndObject).SetActive(false);
        //hpBar.fillAmount = (float)MeteorManager.Instance.LocalPlayer.Attr.hpCur / (float)MeteorManager.Instance.LocalPlayer.Attr.HpMax;
    }

    //：生命值    112155/ 129373
    //Coroutine hideTargetInfo;
    //Dictionary<Buff, GameObject> enemyBuffList = new Dictionary<Buff, GameObject>();
    MeteorUnit CurrentMonster;
    public void UpdateMonsterInfo(MeteorUnit mon)
    {
        if (!GameData.Instance.gameStatus.ShowBlood)
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
            hpBar.fillAmount = currentHP / (float)MeteorManager.Instance.LocalPlayer.Attr.TotalHp;
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
        if (MeteorManager.Instance.LocalPlayer != null && MeteorManager.Instance.LocalPlayer.Attr.hpCur >= 0)
        {
            hpLabel.text = ((int)(MeteorManager.Instance.LocalPlayer.Attr.hpCur / 10.0f)).ToString() + "/" + ((int)(MeteorManager.Instance.LocalPlayer.Attr.HpMax / 10.0f)).ToString();
            nextHp = MeteorManager.Instance.LocalPlayer.Attr.hpCur;
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