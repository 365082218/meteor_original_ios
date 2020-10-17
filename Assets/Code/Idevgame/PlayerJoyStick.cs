using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

using protocol;

public class JoyKeyState {
    public KeyCode key;
    public bool PointDown;
    public UnityEvent OnPressing;
    public UnityEvent OnPress;
    public UnityEvent OnRelease;
    public JoyKeyState(KeyCode joykey = KeyCode.None) {
        key = joykey;
        PointDown = false;
        OnPress = new UnityEvent();
        OnRelease = new UnityEvent();
        OnPressing = new UnityEvent();
    }
}

/// <summary>
/// 测试游戏手柄键值
/// </summary>
public class PlayerJoyStick : MonoBehaviour {
    public SortedDictionary<EKeyList, JoyKeyState> keyMapping = new SortedDictionary<EKeyList, JoyKeyState>();//虚拟键映射关系.
    public JoyKeyState wKey;
    public JoyKeyState sKey;
    public JoyKeyState aKey;
    public JoyKeyState dKey;
    public JoyKeyState CameraAxisXL;//摄像机绕Y轴旋转，水平左
    public JoyKeyState CameraAxisXR;//摄像机绕Y轴旋转，水平右
    public JoyKeyState CameraAxisYU;//摄像机绕X轴旋转，俯仰-上
    public JoyKeyState CameraAxisYD;//摄像机绕X轴旋转，俯仰-下
    public JoyKeyState attack;
    public JoyKeyState defence;
    public JoyKeyState jump;
    public JoyKeyState brust;
    public JoyKeyState changeweapon;
    public JoyKeyState dropweapon;
    public JoyKeyState crouch;
    public JoyKeyState unlock;
    public JoyKeyState help;
    public int CameraAxisX { get { if (CameraAxisXL.PointDown) return -1; if (CameraAxisXR.PointDown) return 1; return 0; } }
    public int CameraAxisY { get { if (CameraAxisYD.PointDown) return -1; if (CameraAxisYU.PointDown) return 1; return 0; } }
    private void Awake() {
        wKey = new JoyKeyState(KeyCode.JoystickButton4);
        sKey = new JoyKeyState(KeyCode.JoystickButton6);
        aKey = new JoyKeyState(KeyCode.JoystickButton7);
        dKey = new JoyKeyState(KeyCode.JoystickButton5);
        CameraAxisXL = new JoyKeyState(KeyCode.JoystickButton10);
        CameraAxisXR = new JoyKeyState(KeyCode.JoystickButton11);
        CameraAxisYU = new JoyKeyState(KeyCode.JoystickButton8);
        CameraAxisYD = new JoyKeyState(KeyCode.JoystickButton9);
        attack = new JoyKeyState(KeyCode.JoystickButton15);
        defence = new JoyKeyState(KeyCode.JoystickButton13);
        jump = new JoyKeyState(KeyCode.JoystickButton14);
        brust = new JoyKeyState(KeyCode.JoystickButton12);
        changeweapon = new JoyKeyState(KeyCode.JoystickButton0);
        dropweapon = new JoyKeyState(KeyCode.JoystickButton3);
        crouch = new JoyKeyState(KeyCode.JoystickButton1);
        unlock = new JoyKeyState(KeyCode.JoystickButton2);
        help = new JoyKeyState(KeyCode.JoystickButton16);
        keyMapping.Add(EKeyList.KL_KeyW, wKey);
        keyMapping.Add(EKeyList.KL_KeyS, sKey);
        keyMapping.Add(EKeyList.KL_KeyA, aKey);
        keyMapping.Add(EKeyList.KL_KeyD, dKey);
        keyMapping.Add(EKeyList.KL_CameraAxisXL, CameraAxisXL);
        keyMapping.Add(EKeyList.KL_CameraAxisXR, CameraAxisXR);
        keyMapping.Add(EKeyList.KL_CameraAxisYU, CameraAxisYU);
        keyMapping.Add(EKeyList.KL_CameraAxisYD, CameraAxisYD);
        keyMapping.Add(EKeyList.KL_Attack, attack);
        keyMapping.Add(EKeyList.KL_Defence, defence);
        keyMapping.Add(EKeyList.KL_Jump, jump);
        keyMapping.Add(EKeyList.KL_BreakOut, brust);
        keyMapping.Add(EKeyList.KL_ChangeWeapon, changeweapon);
        keyMapping.Add(EKeyList.KL_DropWeapon, dropweapon);
        keyMapping.Add(EKeyList.KL_Crouch, crouch);
        keyMapping.Add(EKeyList.KL_KeyQ, unlock);
        keyMapping.Add(EKeyList.KL_Help, help);
    }

    void Sync() {
        foreach (var each in GameStateMgr.Ins.gameStatus.KeyMapping) {
            RegisterInternal(each.Key, each.Value);
        }
    }

    private void OnEnable() {
        Sync();
    }

    public void ResetAll() {
        wKey.OnPress.RemoveAllListeners();
        wKey.OnRelease.RemoveAllListeners();
        wKey.OnPressing.RemoveAllListeners();
        sKey.OnPress.RemoveAllListeners();
        sKey.OnRelease.RemoveAllListeners();
        sKey.OnPressing.RemoveAllListeners();
        aKey.OnPress.RemoveAllListeners();
        aKey.OnRelease.RemoveAllListeners();
        aKey.OnPressing.RemoveAllListeners();
        dKey.OnPress.RemoveAllListeners();
        dKey.OnRelease.RemoveAllListeners();
        dKey.OnPressing.RemoveAllListeners();
        attack.OnPress.RemoveAllListeners();
        attack.OnRelease.RemoveAllListeners();
        defence.OnPress.RemoveAllListeners();
        defence.OnRelease.RemoveAllListeners();
        jump.OnPress.RemoveAllListeners();
        jump.OnRelease.RemoveAllListeners();
        changeweapon.OnPress.RemoveAllListeners();
        changeweapon.OnRelease.RemoveAllListeners();
        brust.OnPress.RemoveAllListeners();
        crouch.OnPress.RemoveAllListeners();
        crouch.OnRelease.RemoveAllListeners();
        dropweapon.OnPress.RemoveAllListeners();
        unlock.OnPress.RemoveAllListeners();
        help.OnPress.RemoveAllListeners();
    }

    public void OnBattleStart() {
        ResetAll();
        wKey.OnPress.AddListener(() => { CombatData.Ins.GMeteorInput.OnAxisKeyPress(EKeyList.KL_KeyW); });
        sKey.OnPress.AddListener(() => { CombatData.Ins.GMeteorInput.OnAxisKeyPress(EKeyList.KL_KeyS); });
        aKey.OnPress.AddListener(() => { CombatData.Ins.GMeteorInput.OnAxisKeyPress(EKeyList.KL_KeyA); });
        dKey.OnPress.AddListener(() => { CombatData.Ins.GMeteorInput.OnAxisKeyPress(EKeyList.KL_KeyD); });

        wKey.OnPressing.AddListener(() => { CombatData.Ins.GMeteorInput.OnKeyPressingProxy(EKeyList.KL_KeyW); });
        sKey.OnPressing.AddListener(() => { CombatData.Ins.GMeteorInput.OnKeyPressingProxy(EKeyList.KL_KeyS); });
        aKey.OnPressing.AddListener(() => { CombatData.Ins.GMeteorInput.OnKeyPressingProxy(EKeyList.KL_KeyA); });
        dKey.OnPressing.AddListener(() => { CombatData.Ins.GMeteorInput.OnKeyPressingProxy(EKeyList.KL_KeyD); });

        wKey.OnRelease.AddListener(() => { CombatData.Ins.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyW); });
        sKey.OnRelease.AddListener(() => { CombatData.Ins.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyS); });
        aKey.OnRelease.AddListener(() => { CombatData.Ins.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyA); });
        dKey.OnRelease.AddListener(() => { CombatData.Ins.GMeteorInput.OnAxisKeyRelease(EKeyList.KL_KeyD); });

        attack.OnPress.AddListener(() => { if (FightState.Exist()) FightState.Instance.OnAttackPress(); });
        attack.OnRelease.AddListener(()=> { if (FightState.Exist()) FightState.Instance.OnAttackRelease(); });

        defence.OnPress.AddListener(() => { if (FightState.Exist()) FightState.Instance.OnDefencePress(); });
        defence.OnRelease.AddListener(() => { if (FightState.Exist()) FightState.Instance.OnDefenceRelease(); });

        jump.OnPress.AddListener(() => { if (FightState.Exist()) FightState.Instance.OnJumpPress(); });
        jump.OnRelease.AddListener(() => { if (FightState.Exist()) FightState.Instance.OnJumpRelease(); });

        changeweapon.OnPress.AddListener(() => { if (FightState.Exist()) FightState.Instance.OnChangeWeaponPress(); });
        changeweapon.OnRelease.AddListener(() => { if (FightState.Exist()) FightState.Instance.OnChangeWeaponRelease(); });

        brust.OnPress.AddListener(() => { if (FightState.Exist()) FightState.Instance.OnBreakOut(); });

        crouch.OnPress.AddListener(() => { if (FightState.Exist()) FightState.Instance.OnCrouchPress(); });
        crouch.OnRelease.AddListener(() => { if (FightState.Exist()) FightState.Instance.OnCrouchRelease(); });

        dropweapon.OnPress.AddListener(() => { if (FightState.Exist()) FightState.Instance.OnClickDrop(); });
        unlock.OnPress.AddListener(() => { if (FightState.Exist()) FightState.Instance.OnClickChangeLock(); });
        help.OnPress.AddListener(() => { if (FightState.Exist()) FightState.Instance.OnRebornClick(); });
    }

    void RegisterInternal(EKeyList ek, KeyCode k) {
        if (keyMapping.ContainsKey(ek)) {
            keyMapping[ek].key = k;
        } else {
            keyMapping.Add(ek, new JoyKeyState(k));
        }
    }

    public void Register(EKeyList ek, KeyCode k) {
        GameStateMgr.Ins.gameStatus.UseGamePad = true;
        List<EKeyList> Keys = new List<EKeyList>();
        foreach (var each in GameStateMgr.Ins.gameStatus.KeyMapping) {
            Keys.Add(each.Key);
        }
        for (int i = 0; i < Keys.Count; i++) {
            if (GameStateMgr.Ins.gameStatus.KeyMapping[Keys[i]] == k) {
                GameStateMgr.Ins.gameStatus.KeyMapping[Keys[i]] = KeyCode.None;
            }
        }

        GameStateMgr.Ins.gameStatus.KeyMapping[ek] = k;
        if (!enabled) {
            enabled = true;//触发onenable
        } else {
            Sync();//主动同步.
        }
    }

    [HideInInspector]
    public bool mJoyPressed;//左侧摇杆是否推/拉
    public bool ArrowPressed { get { return wKey.PointDown || sKey.PointDown || aKey.PointDown || dKey.PointDown; } }
    Vector2 mDelta = Vector2.zero;
    public MyVector2 Delta {
        get {
            if (mJoyPressed) {

            } else if (ArrowPressed) {
                if (wKey.PointDown)
                    mDelta = Vector2.up;
                else if (sKey.PointDown)
                    mDelta = Vector2.down;
                else if (aKey.PointDown)
                    mDelta = Vector2.left;
                else if (dKey.PointDown)
                    mDelta = Vector2.right;
            }
            return mDelta;
        }
    }

    //private string currentButton;//当前按下的按键
    //private string currentAxis;//当前移动的轴向
    //private float axisInput;//当前轴向的值
    MeteorController controller;
    MeteorUnit localPlayer;
    //public void Update() {
        //if (Main.Ins.GameBattleEx != null && Main.Ins.GameBattleEx.BattleFinished())
        //    return;
        //if (CombatData.Ins.Replay && FrameReplay.Instance.Started) {

        //} else {
        //    if (Main.Ins.LocalPlayer != null) {
        //        if (Main.Ins.LocalPlayer.meteorController.InputLocked)
        //            return;
        //    }
        //    getButtons();
        //}
        //getAxis();
    //}

    public void ProcessKey(KeyState keyState) {
        Pressed.Clear();
        Released.Clear();
        Pressing.Clear();
        if (keyMapping.ContainsKey(keyState.Key)) {
            JoyKeyState js = keyMapping[keyState.Key];
            bool old = js.PointDown;
            js.PointDown = Input.GetKey(js.key);
            if (old && !js.PointDown)
                Released.Add(js);
            if (!old && js.PointDown)
                Pressed.Add(js);
            if (old && js.PointDown)
                Pressing.Add(js);
            //先按下
            for (int i = 0; i < Pressed.Count; i++) {
                if (Pressed[i].OnPress != null) {
                    Pressed[i].OnPress.Invoke();
                }
            }
            //再蓄力
            for (int i = 0; i < Pressing.Count; i++) {
                if (Pressing[i].OnPressing != null) {
                    Pressing[i].OnPressing.Invoke();
                }
            }
            //再弹起
            for (int i = 0; i < Released.Count; i++) {
                if (Released[i].OnRelease != null) {
                    Released[i].OnRelease.Invoke();
                }
            }
        }
    }
    /// <summary>
    /// Get Button data of the joysick
    /// </summary>
    List<JoyKeyState> Pressed = new List<JoyKeyState>();
    List<JoyKeyState> Released = new List<JoyKeyState>();
    List<JoyKeyState> Pressing = new List<JoyKeyState>();
    void getButtons() {
        Pressed.Clear();
        Released.Clear();
        Pressing.Clear();
        foreach (var each in keyMapping) {
            //识别抬起，按下基础事件
            bool old = each.Value.PointDown;
            each.Value.PointDown = Input.GetKey(each.Value.key);
            if (old && !each.Value.PointDown) {
                Released.Add(each.Value);
            }
            if (!old && each.Value.PointDown) {
                Pressed.Add(each.Value);
            }

            if (old && each.Value.PointDown) {
                Pressing.Add(each.Value);
            }
        }

        //先按下
        for (int i = 0; i < Pressed.Count; i++) {
            if (Pressed[i].OnPress != null) {
                Pressed[i].OnPress.Invoke();
            }
        }
        //再蓄力
        for (int i = 0; i < Pressing.Count; i++) {
            if (Pressing[i].OnPressing != null) {
                Pressing[i].OnPressing.Invoke();
            }
        }
        //再弹起
        for (int i = 0; i < Released.Count; i++) {
            if (Released[i].OnRelease != null) {
                Released[i].OnRelease.Invoke();
            }
        }
    }

    /// <summary>
    /// Get Axis data of the joysick
    /// </summary>
    //void getAxis() {
    //    if (Input.GetAxisRaw("X axis") >= 0 || Input.GetAxisRaw("X axis") <= 0) {
    //        currentAxis = "X axis";
    //        axisInput = Input.GetAxisRaw("X axis");
    //    }

    //    if (Input.GetAxisRaw("Y axis") >= 0 || Input.GetAxisRaw("Y axis") <= 0) {
    //        currentAxis = "Y axis";
    //        axisInput = Input.GetAxisRaw("Y axis");
    //    }


    //    if (Input.GetAxisRaw("3rd axis") > 0.3 || Input.GetAxisRaw("3rd axis") < -0.3) {
    //        currentAxis = "3rd axis";
    //        axisInput = Input.GetAxisRaw("3rd axis");
    //    }

    //    if (Input.GetAxisRaw("4th axis") > 0.3 || Input.GetAxisRaw("4th axis") < -0.3) {
    //        currentAxis = "4th axis";
    //        axisInput = Input.GetAxisRaw("4th axis");
    //    }

    //    if (Input.GetAxisRaw("5th axis") > 0.3 || Input.GetAxisRaw("5th axis") < -0.3) {
    //        currentAxis = "5th axis";
    //        axisInput = Input.GetAxisRaw("5th axis");
    //    }

    //    if (Input.GetAxisRaw("6th axis") > 0.3 || Input.GetAxisRaw("6th axis") < -0.3) {
    //        currentAxis = "6th axis";
    //        axisInput = Input.GetAxisRaw("6th axis");
    //    }

    //    if (Input.GetAxisRaw("7th axis") > 0.3 || Input.GetAxisRaw("7th axis") < -0.3) {
    //        currentAxis = "7th axis";
    //        axisInput = Input.GetAxisRaw("7th axis");
    //    }

    //    if (Input.GetAxisRaw("8th axis") > 0.3 || Input.GetAxisRaw("8th axis") < -0.3) {
    //        currentAxis = "8th axis";
    //        axisInput = Input.GetAxisRaw("8th axis");
    //    }

    //    if (Input.GetAxisRaw("9th axis") > 0.3 || Input.GetAxisRaw("9th axis") < -0.3) {
    //        currentAxis = "9th axis";
    //        axisInput = Input.GetAxisRaw("9th axis");
    //    }

    //    if (Input.GetAxisRaw("10th axis") > 0.3 || Input.GetAxisRaw("10th axis") < -0.3) {
    //        currentAxis = "10th axis";
    //        axisInput = Input.GetAxisRaw("10th axis");
    //    }

    //    if (Input.GetAxisRaw("11th axis") > 0.3 || Input.GetAxisRaw("11th axis") < -0.3) {
    //        currentAxis = "11th axis";
    //        axisInput = Input.GetAxisRaw("11th axis");
    //    }

    //    if (Input.GetAxisRaw("12th axis") > 0.3 || Input.GetAxisRaw("12th axis") < -0.3) {
    //        currentAxis = "12th axis";
    //        axisInput = Input.GetAxisRaw("12th axis");
    //    }

    //    if (Input.GetAxisRaw("13th axis") > 0.3 || Input.GetAxisRaw("13th axis") < -0.3) {
    //        currentAxis = "13th axis";
    //        axisInput = Input.GetAxisRaw("13th axis");
    //    }

    //    if (Input.GetAxisRaw("14th axis") > 0.3 || Input.GetAxisRaw("14th axis") < -0.3) {
    //        currentAxis = "14th axis";
    //        axisInput = Input.GetAxisRaw("14th axis");
    //    }
    //}

    /// <summary>
    /// show the data onGUI
    /// </summary>
    //void OnGUI() {
    //    //    GUI.TextArea(new Rect(0, 0, 250, 50), "Current Button : " + currentButton);//使用GUI在屏幕上面实时打印当前按下的按键
    //    GUI.TextArea(new Rect(0, 100, 250, 50), "Current Axis : " + currentAxis);//使用GUI在屏幕上面实时打印当前按下的轴
    //    GUI.TextArea(new Rect(0, 200, 250, 50), "Axis Value : " + axisInput);//使用GUI在屏幕上面实时打印当前按下的轴的量
    //}
}