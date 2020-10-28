using protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

/// <summary>
/// 在帧同步上传阶段采集本地输入，在帧同步下载阶段转发输入到本地输入
/// </summary>
public class MeteorNetInput:MeteorInput
{
    public MeteorNetInput(MeteorUnit owner, MeteorController controller):base(owner, controller)
    {

    }
}

public class MeteorInput
{
    public KeyState[] KeyStates = new KeyState[(int)EKeyList.KL_Max];
    public static float DoubleClickTime = 0.4f;
    public static float LongPressedTime = 0.2f;
    public static float ShortPressTime = 0.12f;//按住的时间大于这个时间前就松开，视为按键抬起响应，触发轻键，类似小跳，当按住时间大于这个时间，若仍没松开，视为按键按下响应，触发重键行为，类似大跳
    public static float SuperJumpTime = 0.35f;//按住跳跃持续0.35S还未松开
    MeteorUnit mOwner;
    ActionManager posMng;
    InputModule InputCore;
    MeteorController mController = null;
    public Vector2 mInputVector = Vector2.zero;
    public int OffX;
    public int OffZ;
    public void AIMove(int x, int z)
    {
        OffX = x;
        OffZ = z;
    }
    
    public bool OnInputMoving()
    {
        return (mInputVector.x != 0 || mInputVector.y != 0);
    }

    public void OnAxisKeyPressing(EKeyList k) {
        OnKeyPressingProxy(k);
    }

    public void OnAxisKeyPress(EKeyList k)
    {
        OnKeyDownProxy(k);
    }

    public void OnAxisKeyRelease(EKeyList k)
    {
        OnKeyUpProxy(k);
    }
    
    //键盘控制模块
    public MeteorInput(MeteorUnit owner, MeteorController controller)
    {
        mOwner = owner;
        mController = controller;
        posMng = mOwner.ActionMgr;
        InputCore = new InputModule(owner);
        for (int i = 0; i < KeyStates.Length; i++)
            KeyStates[i] = new KeyState();
        KeyStates[(int)EKeyList.KL_Jump].AxisName = "Jump";
        KeyStates[(int)EKeyList.KL_Jump].Key = EKeyList.KL_Jump;
        KeyStates[(int)EKeyList.KL_Attack].AxisName = "Attack";
        KeyStates[(int)EKeyList.KL_Attack].Key = EKeyList.KL_Attack;
        KeyStates[(int)EKeyList.KL_Defence].AxisName = "Defence";
        KeyStates[(int)EKeyList.KL_Defence].Key = EKeyList.KL_Defence;
        KeyStates[(int)EKeyList.KL_BreakOut].AxisName = "BreakOut";
        KeyStates[(int)EKeyList.KL_BreakOut].Key = EKeyList.KL_BreakOut;
        KeyStates[(int)EKeyList.KL_ChangeWeapon].AxisName = "ChangeWeapon";
        KeyStates[(int)EKeyList.KL_ChangeWeapon].Key = EKeyList.KL_ChangeWeapon;
        KeyStates[(int)EKeyList.KL_DropWeapon].AxisName = "DropWeapon";
        KeyStates[(int)EKeyList.KL_DropWeapon].Key = EKeyList.KL_DropWeapon;
        KeyStates[(int)EKeyList.KL_KeyW].AxisName = "W";
        KeyStates[(int)EKeyList.KL_KeyW].Key = EKeyList.KL_KeyW;
        KeyStates[(int)EKeyList.KL_KeyS].AxisName = "S";
        KeyStates[(int)EKeyList.KL_KeyS].Key = EKeyList.KL_KeyS;
        KeyStates[(int)EKeyList.KL_KeyA].AxisName = "A";
        KeyStates[(int)EKeyList.KL_KeyA].Key = EKeyList.KL_KeyA;
        KeyStates[(int)EKeyList.KL_KeyD].AxisName = "D";
        KeyStates[(int)EKeyList.KL_KeyD].Key = EKeyList.KL_KeyD;

        KeyStates[(int)EKeyList.KL_KeyQ].AxisName = "Unlock";
        KeyStates[(int)EKeyList.KL_KeyQ].Key = EKeyList.KL_KeyQ;

        KeyStates[(int)EKeyList.KL_Crouch].AxisName = "Crouch";
        KeyStates[(int)EKeyList.KL_Crouch].Key = EKeyList.KL_Crouch;

        KeyStates[(int)EKeyList.KL_Help].AxisName = "Help";
        KeyStates[(int)EKeyList.KL_Help].Key = EKeyList.KL_Help;

        KeyStates[(int)EKeyList.KL_PretendDead].AxisName = "PretendDead";
        KeyStates[(int)EKeyList.KL_PretendDead].Key = EKeyList.KL_PretendDead;

        KeyStates[(int)EKeyList.KL_Taunt].AxisName = "Taunt";
        KeyStates[(int)EKeyList.KL_Taunt].Key = EKeyList.KL_Taunt;

        KeyStates[(int)EKeyList.KL_CameraAxisXL].Key = EKeyList.KL_CameraAxisXL;
        KeyStates[(int)EKeyList.KL_CameraAxisXR].Key = EKeyList.KL_CameraAxisXR;
        KeyStates[(int)EKeyList.KL_CameraAxisYU].Key = EKeyList.KL_CameraAxisYU;
        KeyStates[(int)EKeyList.KL_CameraAxisYD].Key = EKeyList.KL_CameraAxisYD;
    }

    public void ResetVector()
    {
        mInputVector = Vector2.zero;
    }

    public void NetUpdate()
    {
        if (!CombatData.Ins.PauseAll)
        {
            Vector2 vec = Vector2.zero;
            bool RotateY = false;
            if (!mOwner.meteorController.InputLocked)
            {
                if (mOwner.Attr.IsPlayer)
                {
                    //手柄不为空,且在使用的情况下.
                    if (Main.Ins.JoyStick.isActiveAndEnabled) {
                        if (Main.Ins.JoyStick.mJoyPressed) {
                            //如果左侧摇杆在操作。虚拟摇杆
                            vec = new Vector2(Main.Ins.JoyStick.Delta.x, Main.Ins.JoyStick.Delta.y);
                            RotateY = true;
                        } else if (Main.Ins.JoyStick.ArrowPressed) {//虚拟按键
                            vec = new Vector2(Main.Ins.JoyStick.Delta.x, Main.Ins.JoyStick.Delta.y);
                        } else {
                            //硬件摇杆
                            RotateY = true;
                            vec = new Vector2(Input.GetAxisRaw("X axis"), -Input.GetAxisRaw("Y axis"));
                            vec.x = Mathf.Abs(vec.x) > 0.02f ? vec.x : 0;
                            vec.y = Mathf.Abs(vec.y) > 0.02f ? vec.y : 0;
                        }
                    } else {
                        //使用触摸屏UI操作
                        if (UGUIJoystick.Ins != null) {
                            //如果方向键按下了
                            if (UGUIJoystick.Ins.mJoyPressed) {
                                vec = new Vector2(UGUIJoystick.Ins.Delta.x, UGUIJoystick.Ins.Delta.y);
                                RotateY = true;
                            } else if (UGUIJoystick.Ins.ArrowPressed)
                                vec = new Vector2(UGUIJoystick.Ins.Delta.x, UGUIJoystick.Ins.Delta.y);
                            else {
                                RotateY = true;
                                vec = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
                            }
                        }
                    }
                }
                else
                    vec = new Vector2(OffX, OffZ);//AIMove 自动战斗输入
            }
            else
                vec = Vector2.zero;

            //if (CombatData.Ins.Replay) {
            //    if (mOwner.Attr.IsPlayer)
            //        mInputVector = mOwner.joy;
            //    else {
            //        mInputVector = mOwner.meteorController.MoveLocked ? Vector2.zero : vec;
            //        mInputVector.x = Mathf.FloorToInt(mInputVector.x * 1000) / 1000.0f;
            //        mInputVector.y = Mathf.FloorToInt(mInputVector.y * 1000) / 1000.0f;
            //    }
            //} else if (U3D.IsMultiplyPlayer()) {
            //    //if (mOwner.Attr.IsPlayer)
            //    //    FrameSyncServer.Ins.PushJoyDelta(mOwner.InstanceId, mInputVector.x, mInputVector.y);
            //} else {
                mInputVector = mOwner.meteorController.MoveLocked ? Vector2.zero : vec;
            //if (mOwner.Attr.IsPlayer)
            //    FrameSyncLocal.Ins.PushDelta(MeteorMsg.Command.Joy, mOwner.InstanceId, x, y);
            //}
            if (RotateY && GameStateMgr.Ins.gameStatus.JoyRotateOnly)
                mInputVector = Vector2.zero;
            if (RotateY && vec != Vector2.zero) {
                if (mOwner.ActionMgr.CanRotateY) {
                    float yAngle = Mathf.Atan2(vec.x, vec.y) * Mathf.Rad2Deg;
                    yAngle *= FrameReplay.deltaTime;
                    mOwner.OnPlayerMouseDelta(0,  yAngle);
                }
            }
        }

        if (!mOwner.meteorController.InputLocked)
        {
            InputCore.Update();
            //主角色，扫描硬件信息
            UpdateKeyStatus();
            UpdateMoveInput();
        }
    }

    //使用摇杆的时候不要读取硬件信息。
    void UpdateKeyStatus()
    {
        for (int idx = 0; idx < KeyStates.Length; idx++)
        {
            KeyState keyStatus = KeyStates[idx];
            keyStatus.PressedTime += FrameReplay.deltaTime;
            keyStatus.ReleasedTime += FrameReplay.deltaTime;

            if (Main.Ins.JoyStick.isActiveAndEnabled) {
                if (mOwner.Attr.IsPlayer) {
                    Main.Ins.JoyStick.ProcessKey(keyStatus);
                }
            } else {
#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)
                //底下做的是与键盘同步，多输入设备会让其他设备的输出,被键盘状态刷新掉
                if (string.IsNullOrEmpty(keyStatus.AxisName))
                    continue;
                if (mOwner.Attr.IsPlayer)//主角才读取键盘输入
                {
                    float kValue = Input.GetAxisRaw(keyStatus.AxisName);
                    bool pressed = kValue > 0;
                    if (pressed && keyStatus.Pressed == 0)
                        OnKeyDownProxy(keyStatus.Key);
                    else if (!pressed && keyStatus.Pressed != 0)
                        OnKeyUpProxy(keyStatus.Key);
                    else if (pressed && keyStatus.Pressed != 0) {
                        OnKeyPressingProxy(keyStatus.Key);
                    }
                }
#endif
            }
        }
    }

/*
 * ;匕首
248 太岁 A
249 九尾 AA 
250 夜叉 上A
251 旱魃 上A上A
253 曹沫举顶 上上A 
254 荆轲现匕 左A 
255 聂政屠犬 右A 
257 豫让三伏 下上A 
258 豫让三伏（三插）下上A
343 专诸鱼肠 下A 
259 阎罗 下上上A 
245 姑获 A(空中) 
246 奔云 A A(空中)
247 鹏翼 A A A(空中)
252 罗刹 下A（空）
256 鬼弹 上下A（空） 
558 暗犽 下下A
559 降神 下下A（空）

    */

    //返回当前动作是否在可接受输入帧内,就是
    public bool AcceptInput()
    {
        Pose p = ActionManager.ActionList[mOwner.ModelId][posMng.mActiveAction.Idx];
        if (p == null)
            return false;
        //飞轮.
        if (p.Idx >= 218 && p.Idx <= 225)
            return false;

        //防御受击中.
        if (p.Idx >= CommonAction.DefenceHitStart && p.Idx <= CommonAction.DefenceHitEnd)
            return false;

        //倒地挣扎，可以在硬直结束后响应按键
        if (p.Idx >= CommonAction.Struggle0 && p.Idx <= CommonAction.Struggle)
            return true;

        //受击动作中
        if (p.Idx >= CommonAction.HitStart && p.Idx <= CommonAction.HitEnd)
            return false;

        if (p.Idx == CommonAction.Idle || p.Idx == CommonAction.GunIdle)
            return true;

        if (ActionManager.IsReadyAction(p.Idx))
            return true;

        if (ActionManager.IsJumpAction(p.Idx))
            return true;

        if (p.Idx == CommonAction.Run || p.Idx == CommonAction.RunOnDrug)
            return true;

        if (p.Idx >= CommonAction.DCForw && p.Idx <= CommonAction.DCBack)
            return true;

        if (p.Next != null)
        {
            int inputEnd = p.Next.End;
            int curIndex = mOwner.ActionMgr.GetCurrentFrameIndex();
            //边界条件会导致部分招式衔接过快
            //if (posMng.IsInKeyFrame(p.Next.Start, inputEnd)) {
            //    return true;
            //} else
            //    return false;
            if (p.Next.Start < curIndex && curIndex < inputEnd) {
                //if (posMng.mActiveAction.Idx >= 295 && posMng.mActiveAction.Idx <= 300)
                //    Debug.LogError("pos:" + p.Idx + " p.next start:" + p.Next.Start + "  cur index = " + curIndex + " input end:" + inputEnd);
                return true;
            } else
                return false;
        }
        else
        {
            if (p.Idx == CommonAction.GunIdle)
                return true;
            ActionNode act = ActionInterrupt.Ins.GetActions(p.Idx);
            if (act == null)
                return false;
            if (act.target == null || act.target.Count == 0)
                return false;
        }

        //切换武器，不接受连招输入.
        if (p.Idx == CommonAction.ChangeWeapon || p.Idx == CommonAction.AirChangeWeapon)
            return false;

        return true;
    }

    public bool CheckPos(int KeyMap, int targetAct)
    {
        //if (targetAct == 407) {
        //    Debug.LogError("Checkpos0");
        //}
        bool result = false;
        //首先要判断是否在可输入范围内,只是限定输入，切换动作在其他地方
        if (!AcceptInput())
            return false;
        //if (targetAct == 407) {
        //    Debug.LogError("Checkpos1");
        //}
        int weapon = mOwner.GetWeaponType();
        switch (weapon)
        {
            case (int)EquipWeaponType.Knife:
                switch (KeyMap)
                {
                    case 25://匕首普攻,需要看输入缓冲是否含有其他方向键,或者含有任意方向键的状态
                        result = true;
                        break;
                    case 32:
                        result = true;//空中A
                        break;
                    case 26://匕首上A,状态,只能按下一次W或者弹起一次W,或者当前W被按下,还要判断反向键的状态
                            result = true;
                        break;
                    //上上A,前3个按键里面只有找到至少2个按下W按键
                    case 27:
                        result = true;
                        break;
                    case 28://左A 370-371
                        result = true;
                        break;
                    case 29://右A
                        result = true;
                        break;
                    case 30://下上A(地面)            
                        result = true;
                        break;
                    case 31://下上上A 阎罗
                        if (mOwner.AngryValue < 100)
                            result = false;
                        else
                        {
                            result = true;
                            mOwner.AngryValue -= 100;
                        }
                        break;
                    case 84://下A 空 地都可以 接空中252不能在地面 接地面343不能在空中
                    case 33:
                            result = true;
                        break;
                    case 34:
                            result = true;
                        break;
                    case 88://下下A 地面小绝 下扎
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                    case 150://空中下下A
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                }
                break;
                //锤子长枪大刀属于重武器，共用输入59编号动作338
            case (int)EquipWeaponType.Blade:
                switch (KeyMap)
                {
                    case 61:
                    case 62:
                    case 63:
                    case 64:
                    case 65:
                    case 66:
                    case 67:
                    case 68:
                    case 69:
                    case 70:
                        result = true;
                        break;
                    case 71://大绝招
                        if (mOwner.AngryValue < 100)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 100;
                            result = true;
                        }
                        break;
                    case 72:
                        result = true;
                        break;
                    case 155://旋风斩
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                    case 156://雷电斩
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                    case 59://一般空踢
                        result = true;
                        break;
                }
                break;
            case (int)EquipWeaponType.Dart:
                switch (KeyMap)
                {
                    case 1:
                        result = true;
                        break;
                    case 143://地面八方绝
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                    case 85:
                        result = true;
                        break;
                    case 4://落樱雪
                        if (mOwner.AngryValue < 100)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 100;
                            result = true;
                        }
                        break;
                    case 2:
                    case 3:
                    case 87:
                        result = true;
                        break;
                    case 144://空中八方绝
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                }
                break;
            case (int)EquipWeaponType.Gun:
                switch (KeyMap)
                {
                    case 5:
                    case 6:
                        result = true;
                        break;
                    case 7:
                        if (mOwner.AngryValue < 20)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 20;
                            result = true;
                        }
                        break;
                    case 8://大绝
                        if (mOwner.AngryValue < 100)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 100;
                            result = true;
                        }
                        break;
                    case 147://小绝
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                    case 59://一般空踢
                        result = true;
                        break;
                }
                break;
            case (int)EquipWeaponType.Guillotines://血滴子
                switch (KeyMap)
                {
                    case 9:
                    case 10:
                    case 11:
                        result = true;
                        break;
                    case 12:
                        if (mOwner.AngryValue < 100)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 100;
                            result = true;
                        }
                        break;
                    case 145:
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                    case 146:
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                    case 59://一般空踢
                        result = true;
                        break;
                }
                break;
            case (int)EquipWeaponType.Brahchthrust://分水刺
                switch (KeyMap)
                {
                    case 13:
                    case 14:
                    case 15:
                    case 16:
                    case 17:
                    case 18:
                    case 19:
                    case 20:
                        result = true;
                        break;
                    case 21:
                        if (mOwner.AngryValue < 100)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 100;
                            result = true;
                        }
                        break;
                    case 22:
                    case 23:
                    case 24:
                        result = true;
                        break;
                    case 148://左右上小绝
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                    case 149://加速BUFF
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            result = true;
                            mOwner.AngryValue -= 60;
                        }
                        break;
                }
                break;
            case (int)EquipWeaponType.Sword://剑
                switch (KeyMap)
                {
                    case 35:
                    case 36:
                    case 37:
                    case 38:
                    case 39:
                    case 40:
                    case 41:
                    case 42:
                    case 43:
                    case 44:
                        result = true;
                        break;
                    case 45://大招
                        if (mOwner.AngryValue < 100)
                            result = false;
                        else
                        {
                            result = true;
                            mOwner.AngryValue -= 100;
                        }
                        break;
                    case 46:
                    case 47:
                        result = true;
                        break;
                    case 151://左右A剑气
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            result = true;
                            mOwner.AngryValue -= 60;
                        }
                        break;
                    case 152://右左A小绝旋转
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                }
                break;
            case (int)EquipWeaponType.Lance://枪
                switch (KeyMap)
                {
                    case 48:
                    case 49:
                    case 50:
                    case 51:
                    case 52:
                    case 53:
                    case 54:
                    case 55:
                    case 56:
                    case 57:
                        result = true;
                        break;
                    case 58://大招
                        if (mOwner.AngryValue < 100)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 100;
                            result = true;
                        }
                        break;
                    case 59:
                    case 60:
                        result = true;
                        break;
                    case 153://强攻 前前前A
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                    case 154://左右下A 小绝招
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                }
                break;
            case (int)EquipWeaponType.Hammer://锤子
                switch (KeyMap)
                {
                    case 73:
                    case 74:
                    case 75:
                    case 76:
                    case 77:
                    case 78:
                    case 79:
                    case 80:
                    case 81:
                        result = true;
                        break;
                    case 82://铜皮
                        if (mOwner.AngryValue < 100)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 100;
                            result = true;
                        }
                        break;
                    case 83:
                        result = true;
                        break;
                    case 157://震荡波
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                    case 158://大绝
                        if (mOwner.AngryValue < 100)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 100;
                            result = true;
                        }
                        break;
                    case 59://一般空踢
                        result = true;
                        break;
                }
                break;
            case (int)EquipWeaponType.HeavenLance://乾坤
                switch (KeyMap)
                {
                    //乾坤分3种姿态，也就是3个POSE组
                    case 89://左A 拔刀切换持枪 430
                        result = mOwner.GetWeaponSubType() == 0;
                        break;
                    case 90://下下A 拔刀切换居合 431
                    case 91://432，433，434
                    case 92://439
                        result = mOwner.GetWeaponSubType() == 0;
                        break;
                    case 93://右A 持枪切换拔刀440
                    case 94://下下A 持枪切换居合441
                    case 95://442
                    case 96://443
                        result = mOwner.GetWeaponSubType() == 1;
                        break;

                    case 97://444,持枪旋风
                        result = mOwner.GetWeaponSubType() == 1;
                        break;
                    case 98://445
                        result = mOwner.GetWeaponSubType() == 1;
                        break;
                    case 99://右A 居合转换拔刀447
                    case 100://左A 居合转换持枪 448 
                    case 101://520,521,522,523
                        result = mOwner.GetWeaponSubType() == 2;
                        break;
                    case 102://449-收刀下上A-拔刀术
                        result = mOwner.GetWeaponSubType() == 2;
                        break;
                    case 103://450
                        result = mOwner.GetWeaponSubType() == 2;
                        break;
                    case 104://451//大绝招
                        result = mOwner.GetWeaponSubType() == 2;
                        if (result)
                        {
                            if (mOwner.AngryValue < 100)
                                result = false;
                            else
                            {
                                mOwner.AngryValue -= 100;
                                result = true;
                            }
                        }
                        break;
                    case 105://458
                    case 106://空中下A 446
                        result = true;
                        break;
                    case 159://持枪小绝-Pos 570
                        result = mOwner.GetWeaponSubType() == 1;
                        if (result)
                        {
                            if (mOwner.AngryValue < 60)
                                result = false;
                            else
                            {
                                mOwner.AngryValue -= 60;
                                result = true;
                            }
                        }
                        break;
                    case 160://拔刀小绝-Pos 568
                        result = mOwner.GetWeaponSubType() == 0;
                        if (result)
                        {
                            if (mOwner.AngryValue < 60)
                                result = false;
                            else
                            {
                                mOwner.AngryValue -= 60;
                                result = true;
                            }
                        }
                        break;
                }

                if (!result)
                {
                    //再次尝试乾坤刀切换pose后速连
                    int nextPose = ActionEvent.TryHandlerLastActionFrame(mOwner, mOwner.ActionMgr.mActiveAction.Idx);
                    result = TryHeavenLance(nextPose, KeyMap);
                    //if (nextPose == 2)
                    //{
                    //    //104，大招被，正拔刀术阻挡了
                    //    Debug.LogError(string.Format("KeyMap:{0} result:{1} targetact:{2}", KeyMap, result, targetAct));
                    //}
                }
                break;
            case (int)EquipWeaponType.Gloves://拳套
                //可能是全套快速出招接其他招式.
                int nextWeapon = ActionEvent.TryHandlerLastActionFrame(mOwner, mOwner.ActionMgr.mActiveAction.Idx);//切换了武器后，新的武器POSE或者SUBTYPE
                if (nextWeapon != weapon && nextWeapon != -1)
                    result = TryOtherWeapon(nextWeapon, KeyMap);
                if (result)
                    return result;
                switch (KeyMap)
                {
                    //乾坤分3种姿态，也就是3个POSE组
                    case 107:
                    case 108:
                    case 109:
                    case 110:
                    case 111:
                    case 112:
                    case 113:
                    case 114:
                        result = true;
                        break;
                    case 115:
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                    case 116:
                    case 117:
                    case 118:
                    case 119:
                        result = true;
                        break;
                    case 120://大绝
                        if (mOwner.AngryValue < 100)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 100;
                            result = true;
                        }
                        break;
                    case 121://嗜血
                        if (mOwner.AngryValue < 100)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 100;
                            result = true;
                        }
                        break;
                }
                break;
            case (int)EquipWeaponType.NinjaSword://忍者刀
                switch (KeyMap)
                {
                    case 122:
                    case 123:
                    case 124:
                    case 125:
                    case 126:
                    case 127:
                        result = true;
                        break;
                    case 128://忍术-隐忍
                        if (mOwner.AngryValue < 50)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 50;
                            result = true;
                        }
                        break;
                    case 129:
                    case 130:
                    case 131:
                    case 132:
                    case 133:
                    case 134:
                    case 135:
                        result = true;
                        break;
                    case 136://天地同寿
                        if (mOwner.AngryValue < 50)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 50;
                            result = true;
                        }
                        break;
                    case 137:
                    case 138:
                    case 139:
                    case 140:
                        result = true;
                        break;
                    case 141://忍爆弹-忍大招
                        if (mOwner.AngryValue < 50)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 50;
                            result = true;
                        }
                        break;
                    case 142://左右A 10气
                        if (mOwner.AngryValue < 10)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 10;
                            result = true;
                        }
                        break;
                }
                break;
        }
        //if (targetAct == 407) {
        //    Debug.LogError("Checkpos2 " + result);
        //}
        return result;
    }

    bool TryHeavenLance(int nextPose, int KeyMap)
    {
        //Debug.LogError(string.Format("nextPose:{0}", nextPose));
        bool result = false;
        switch (KeyMap)
        {
            //乾坤分3种姿态，也就是3个POSE组
            case 89://左A 拔刀切换持枪 430
                result = nextPose == 0;
                break;
            case 90://下下A 拔刀切换居合 431
            case 91://432，433，434
            case 92://439
                result = nextPose == 0;
                break;
            case 93://右A 持枪切换拔刀440
            case 94://下下A 持枪切换居合441
            case 95://442
            case 96://443
                result = nextPose == 1;
                break;

            case 97://444,持枪旋风
                result = nextPose == 1;
                break;
            case 98://445
                result = nextPose == 1;
                break;
            case 99://右A 居合转换拔刀447
            case 100://左A 居合转换持枪 448 
            case 101://520,521,522,523
                result = nextPose == 2;
                break;
            case 102://449-收刀下上A-拔刀术
                result = nextPose == 2;
                break;
            case 103://450
                result = nextPose == 2;
                break;
            case 104://451//大绝招
                result = nextPose == 2;
                if (result)
                {
                    if (mOwner.AngryValue < 100)
                        result = false;
                    else
                    {
                        mOwner.AngryValue -= 100;
                        result = true;
                    }
                }
                break;
            case 105://458
            case 106://空中下A 446
                result = true;
                break;
            case 159://持枪小绝-Pos 570
                result = nextPose == 1;
                if (result)
                {
                    if (mOwner.AngryValue < 60)
                        result = false;
                    else
                    {
                        mOwner.AngryValue -= 60;
                        result = true;
                    }
                }
                break;
            case 160://拔刀小绝-Pos 568
                result = nextPose == 0;
                if (result)
                {
                    if (mOwner.AngryValue < 60)
                        result = false;
                    else
                    {
                        mOwner.AngryValue -= 60;
                        result = true;
                    }
                }
                break;
        }
        return result;
    }

    //指虎速连其他招，只能由上一级拳套武器时
    //nextWeapon，指虎速连之前要切换到的武器
    bool TryOtherWeapon(int nextWeapon, int KeyMap)
    {
        bool result = false;
        //状态要抵消,必须方向相反.比如匕首后前前A,如果在按住后的时候,只需要输入上上A就可以凑招了
        //当解析方向相反的招式起始按键时,必须考虑凑招问题
        //单独按键需要判断 跳,爬墙,等状态
        switch (nextWeapon)
        {
            case (int)EquipWeaponType.Knife:
                switch (KeyMap)
                {
                    case 25://匕首普攻,需要看输入缓冲是否含有其他方向键,或者含有任意方向键的状态
                        result = true;
                        break;
                    case 32:
                        result = true;//空中A
                        break;
                    case 26://匕首上A,状态,只能按下一次W或者弹起一次W,或者当前W被按下,还要判断反向键的状态
                        result = true;
                        break;
                    //上上A,前3个按键里面只有找到至少2个按下W按键
                    case 27:
                        result = true;
                        break;
                    case 28://左A 370-371
                        result = true;
                        break;
                    case 29://右A
                        result = true;
                        break;
                    case 30://下上A(地面)            
                        result = true;
                        break;
                    case 31://下上上A 阎罗
                        if (mOwner.AngryValue < 100)
                            result = false;
                        else
                        {
                            result = true;
                            mOwner.AngryValue -= 100;
                        }
                        break;
                    case 84://下A 空 地都可以 接空中252不能在地面 接地面343不能在空中
                    case 33:
                        result = true;
                        break;
                    case 34:
                        result = true;
                        break;
                    case 88://下下A 地面小绝 下扎
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                    case 150://空中下下A
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                }
                break;
            //锤子长枪大刀属于重武器，共用输入59编号动作338
            case (int)EquipWeaponType.Blade:
                switch (KeyMap)
                {
                    case 61:
                    case 62:
                    case 63:
                    case 64:
                    case 65:
                    case 66:
                    case 67:
                    case 68:
                    case 69:
                    case 70:
                        result = true;
                        break;
                    case 71://大绝招
                        if (mOwner.AngryValue < 100)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 100;
                            result = true;
                        }
                        break;
                    case 72:
                        result = true;
                        break;
                    case 155://旋风斩
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                    case 156://雷电斩
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                    case 59://一般空踢
                        result = true;
                        break;
                }
                break;
            case (int)EquipWeaponType.Dart:
                switch (KeyMap)
                {
                    case 1:
                        result = true;
                        break;
                    case 143://地面八方绝
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                    case 85:
                        result = true;
                        break;
                    case 4://落樱雪
                        if (mOwner.AngryValue < 100)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 100;
                            result = true;
                        }
                        break;
                    case 2:
                    case 3:
                    case 87:
                        result = true;
                        break;
                    case 144://空中八方绝
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                }
                break;
            case (int)EquipWeaponType.Gun:
                switch (KeyMap)
                {
                    case 5:
                    case 6:
                    case 7:
                        result = true;
                        break;
                    case 8://大绝
                        if (mOwner.AngryValue < 100)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 100;
                            result = true;
                        }
                        break;
                    case 147://小绝
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                    case 59://一般空踢
                        result = true;
                        break;
                }
                break;
            case (int)EquipWeaponType.Guillotines://血滴子
                //先判定场景内有无已发射出还未返回到手里的血滴子，有则不许发射新的血滴子
                if (FlyWheel.FindFlyWheel(mOwner))
                    return false;
                switch (KeyMap)
                {
                    case 9:
                    case 10:
                    case 11:
                        result = true;
                        break;
                    case 12:
                        if (mOwner.AngryValue < 100)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 100;
                            result = true;
                        }
                        break;
                    case 145:
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                    case 146:
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                    case 59://一般空踢
                        result = true;
                        break;
                }
                break;
            case (int)EquipWeaponType.Brahchthrust://分水刺
                switch (KeyMap)
                {
                    case 13:
                    case 14:
                    case 15:
                    case 16:
                    case 17:
                    case 18:
                    case 19:
                    case 20:
                        result = true;
                        break;
                    case 21:
                        if (mOwner.AngryValue < 100)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 100;
                            result = true;
                        }
                        break;
                    case 22:
                    case 23:
                    case 24:
                        result = true;
                        break;
                    case 148://左右上小绝
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                    case 149://加速BUFF
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            result = true;
                            mOwner.AngryValue -= 60;
                        }
                        break;
                }
                break;
            case (int)EquipWeaponType.Sword://剑
                switch (KeyMap)
                {
                    case 35:
                    case 36:
                    case 37:
                    case 38:
                    case 39:
                    case 40:
                    case 41:
                    case 42:
                    case 43:
                    case 44:
                        result = true;
                        break;
                    case 45://大招
                        if (mOwner.AngryValue < 100)
                            result = false;
                        else
                        {
                            result = true;
                            mOwner.AngryValue -= 100;
                        }
                        break;
                    case 46:
                    case 47:
                        result = true;
                        break;
                    case 151://左右A剑气
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            result = true;
                            mOwner.AngryValue -= 60;
                        }
                        break;
                    case 152://右左A小绝旋转
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                }
                break;
            case (int)EquipWeaponType.Lance://枪
                switch (KeyMap)
                {
                    case 48:
                    case 49:
                    case 50:
                    case 51:
                    case 52:
                    case 53:
                    case 54:
                    case 55:
                    case 56:
                    case 57:
                        result = true;
                        break;
                    case 58://大招
                        if (mOwner.AngryValue < 100)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 100;
                            result = true;
                        }
                        break;
                    case 59:
                    case 60:
                        result = true;
                        break;
                    case 153://强攻 前前前A
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                    case 154://左右下A 小绝招
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                }
                break;
            case (int)EquipWeaponType.Hammer://锤子
                switch (KeyMap)
                {
                    case 73:
                    case 74:
                    case 75:
                    case 76:
                    case 77:
                    case 78:
                    case 79:
                    case 80:
                    case 81:
                        result = true;
                        break;
                    case 82://铜皮
                        if (mOwner.AngryValue < 100)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 100;
                            result = true;
                        }
                        break;
                    case 83:
                        result = true;
                        break;
                    case 157://震荡波
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                    case 158://大绝
                        if (mOwner.AngryValue < 100)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 100;
                            result = true;
                        }
                        break;
                    case 59://一般空踢
                        result = true;
                        break;
                }
                break;
            case (int)EquipWeaponType.HeavenLance://乾坤
                switch (KeyMap)
                {
                    //乾坤分3种姿态，也就是3个POSE组
                    case 89://左A 拔刀切换持枪 430
                        result = mOwner.GetWeaponSubType() == 0;
                        break;
                    case 90://下下A 拔刀切换居合 431
                    case 91://432，433，434
                    case 92://439
                        result = mOwner.GetWeaponSubType() == 0;
                        break;
                    case 93://右A 持枪切换拔刀440
                    case 94://下下A 持枪切换居合441
                    case 95://442
                    case 96://443
                        result = mOwner.GetWeaponSubType() == 1;
                        break;

                    case 97://444,持枪旋风
                        result = mOwner.GetWeaponSubType() == 1;
                        break;
                    case 98://445
                        result = mOwner.GetWeaponSubType() == 1;
                        break;
                    case 99://右A 居合转换拔刀447
                    case 100://左A 居合转换持枪 448 
                    case 101://520,521,522,523
                        result = mOwner.GetWeaponSubType() == 2;
                        break;
                    case 102://449-收刀下上A-拔刀术
                        result = mOwner.GetWeaponSubType() == 2;
                        break;
                    case 103://450
                        result = mOwner.GetWeaponSubType() == 2;
                        break;
                    case 104://451//大绝招
                        result = mOwner.GetWeaponSubType() == 2;
                        if (result)
                        {
                            if (mOwner.AngryValue < 100)
                                result = false;
                            else
                            {
                                mOwner.AngryValue -= 100;
                                result = true;
                            }
                        }
                        break;
                    case 105://458
                    case 106://空中下A 446
                        result = true;
                        break;
                    case 159://持枪小绝-Pos 570
                        result = mOwner.GetWeaponSubType() == 1;
                        if (result)
                        {
                            if (mOwner.AngryValue < 60)
                                result = false;
                            else
                            {
                                mOwner.AngryValue -= 60;
                                result = true;
                            }
                        }
                        break;
                    case 160://拔刀小绝-Pos 568
                        result = mOwner.GetWeaponSubType() == 0;
                        if (result)
                        {
                            if (mOwner.AngryValue < 60)
                                result = false;
                            else
                            {
                                mOwner.AngryValue -= 60;
                                result = true;
                            }
                        }
                        break;
                }
                break;
            case (int)EquipWeaponType.NinjaSword://忍者刀
                switch (KeyMap)
                {
                    case 122:
                    case 123:
                    case 124:
                    case 125:
                    case 126:
                    case 127:
                        result = true;
                        break;
                    case 128://忍术-隐忍
                        if (mOwner.AngryValue < 50)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 50;
                            result = true;
                        }
                        break;
                    case 129:
                    case 130:
                    case 131:
                    case 132:
                    case 133:
                    case 134:
                    case 135:
                        result = true;
                        break;
                    case 136://天地同寿
                        if (mOwner.AngryValue < 50)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 50;
                            result = true;
                        }
                        break;
                    case 137:
                    case 138:
                    case 139:
                    case 140:
                        result = true;
                        break;
                    case 141://忍爆弹-忍大招
                        if (mOwner.AngryValue < 50)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 50;
                            result = true;
                        }
                        break;
                    case 142://左右A 10气
                        if (mOwner.AngryValue < 10)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 10;
                            result = true;
                        }
                        break;
                }
                break;
        }
        return result;
    }

    public void ResetInput()
    {
        InputCore.Reset();
    }

    public void OnKeyPressing(EKeyList key) {
        if (mOwner.meteorController.InputLocked)
            return;
        OnKeyPressing(KeyStates[(int)key]);
    }

    Dictionary<EKeyList, int> genFreq = new Dictionary<EKeyList, int>();
    void OnKeyPressing(KeyState keyStatus) {
        if (genFreq.ContainsKey(keyStatus.Key)) {
            genFreq[keyStatus.Key]--;
            if (genFreq[keyStatus.Key] == 0) {
                InputCore.OnKeyPressing(keyStatus);
                genFreq[keyStatus.Key] = 1;
            }
        } else
            genFreq.Add(keyStatus.Key, 1);
    }

    public void SyncKeyPressing(EKeyList key) {
        OnKeyPressing(key);
    }

    public void OnKeyPressingProxy(EKeyList key) {
        //if (CombatData.Ins.Replay) {
        //    if (!mOwner.Attr.IsPlayer)
        //        OnKeyPressing(key);
        //    return;
        //} else if (U3D.IsMultiplyPlayer()) {
        //    if (mOwner.Attr.IsPlayer) {
        //        FrameSyncServer.Ins.PushKeyEvent(protocol.MeteorMsg.Command.Key, mOwner.InstanceId, key, VirtualKeyState.Pressing);
        //    }
        //} else {
            OnKeyPressing(key);
            //if (mOwner.Attr.IsPlayer)
            //    FrameSyncLocal.Ins.PushKeyEvent(protocol.MeteorMsg.Command.Key, mOwner.InstanceId, key, VirtualKeyState.Pressing);
        //}
    }

    public void SyncKeyDown(EKeyList key, bool system = false) {
        OnKeyDownProxy(KeyStates[(int)key], system);
    }

    public void SyncKeyUp(EKeyList key, bool system = false) {
        OnKeyUpProxy(KeyStates[(int)key], system);
    }

    public void OnKeyDownProxy(EKeyList key, bool system = false)
    {
        if (mOwner.Dead)
            return;
        //if (CombatData.Ins.Replay) {
        //    if (!mOwner.Attr.IsPlayer)
        //        OnKeyDownProxy(KeyStates[(int)key], system);
        //    return;
        //} else if (U3D.IsMultiplyPlayer()) {
        //    if (mOwner.Attr.IsPlayer)
        //        FrameSyncServer.Ins.PushKeyDown(mOwner.InstanceId, key);
        //} else {
            OnKeyDownProxy(KeyStates[(int)key], system);
            //if (mOwner.Attr.IsPlayer)
            //    FrameSyncLocal.Ins.PushKeyDown(mOwner.InstanceId, key);
        //}
    }

    public void OnKeyUpProxy(EKeyList key, bool system = false)
    {
        if (mOwner.Dead)
            return;
        //if (CombatData.Ins.Replay) {
        //    if (!mOwner.Attr.IsPlayer)
        //        OnKeyUpProxy(KeyStates[(int)key], system);
        //    return;
        //} else if (U3D.IsMultiplyPlayer()) {
        //    if (mOwner.Attr.IsPlayer)
        //        FrameSyncServer.Ins.PushKeyUp(mOwner.InstanceId, key);
        //} else {
        OnKeyUpProxy(KeyStates[(int)key], system);
        //    if (mOwner.Attr.IsPlayer)
        //        FrameSyncLocal.Ins.PushKeyUp(mOwner.InstanceId, key);
        //}
    }

    public void OnKeyDown(KeyState keyStatus)
    {
        if (InputCore.OnKeyDown(keyStatus))
            InputCore.Reset();
    }

    public void OnKeyUp(KeyState keyStatus)
    {
        keyStatus.Pressed = 0;
        keyStatus.ReleasedTime = 0.0f;
        if (keyStatus.Key == EKeyList.KL_Jump) {
            mOwner.UpdateNinjaState(NinjaState.WaitPress);
        }
    }

    public void OnKeyUpProxy(KeyState keyStatus, bool system = false)
    {
        if (mOwner.meteorController.InputLocked && !system)
            return;
        keyStatus.Pressed = 0;
        keyStatus.ReleasedTime = 0.0f;
        if (genFreq.ContainsKey(keyStatus.Key))
            genFreq.Remove(keyStatus.Key);
        InputCore.RemovePressing(keyStatus.Key);
        OnKeyUp(keyStatus);
    }

    public void OnKeyDownProxy(KeyState keyStatus, bool system = false) {
        if (mOwner.meteorController.InputLocked && !system)
            return;
        keyStatus.Pressed = keyStatus.PressedTime < DoubleClickTime ? 2 : 1;
        keyStatus.PressedTime = 0.0f;
        if (!genFreq.ContainsKey(keyStatus.Key))
            genFreq.Add(keyStatus.Key, 1);
        if (keyStatus.Key == EKeyList.KL_Jump)
            mOwner.UpdateNinjaState(NinjaState.Ready);
        OnKeyDown(keyStatus);
    }

    void UpdateMoveInput()
    {
        Vector2 direction = mInputVector.normalized;//摇杆
        if (direction == Vector2.zero)
        {
            posMng.CheckClimb = false;
            return;
        }
        //如果摇杆按着边缘的方向键，触发任意方向，则移动，否则，旋转目标。
        //跳跃的时候，方向轴受一定控制，但是强度没那么大.
        //只有在爬墙的时候，才能手动控制方向.只有按住上，才会继续给水平冲量，这个冲量方向顺着爬墙的方向
        if (mOwner.Climbing)
        {
            //direction.Normalize();
            float y = direction.y;
            //爬墙速度未X时间间隔
            //其他方向的移动，只要在爬行过程都给，
            if (mOwner.ActionMgr.mActiveAction.Idx == CommonAction.ClimbUp)
                mOwner.AddWorldVelocityExcludeY((-(mOwner.ClimbingSpeed / 2.0f) * mOwner.transform.forward * FrameReplay.deltaTime));
            else if (mOwner.ActionMgr.mActiveAction.Idx == CommonAction.ClimbRight)
                mOwner.AddWorldVelocityExcludeY((-mOwner.ClimbingSpeed * FrameReplay.deltaTime * mOwner.transform.right -(mOwner.ClimbingSpeed / 2.0f) * FrameReplay.deltaTime * mOwner.transform.forward));
            else if (mOwner.ActionMgr.mActiveAction.Idx == CommonAction.ClimbLeft)
                mOwner.AddWorldVelocityExcludeY((mOwner.ClimbingSpeed * FrameReplay.deltaTime * mOwner.transform.right - (mOwner.ClimbingSpeed / 2.0f) * FrameReplay.deltaTime * mOwner.transform.forward));

            //能向上爬行的阶段
            if (y > 0 && mOwner.ClimbingTime < CombatData.ClimbLimit)
            {
                mOwner.AddYVelocity(mOwner.ClimbingSpeedY * FrameReplay.deltaTime);
            }
        }
        else if (!mOwner.IsOnGround())
        {
            if (mOwner.ActionMgr.mActiveAction.Idx == CommonAction.Jump && mOwner.ImpluseVec.x == 0.0f && mOwner.ImpluseVec.z == 0.0f)
            {
                //CanAdjust表示，在跳跃开始后，还未被任何状态打断为不可微调状态
                //只要单次起跳过程一旦碰到墙壁-则切换为不可微调
                if (mOwner.ActionMgr.CanAdjust && !mOwner.OnTouchWall && !mOwner.OnTopGround && !mOwner.MoveOnGroundEx && !mOwner.OnGround)
                {
                    //direction.Normalize();
                    Vector2 runTrans = direction * mOwner.MoveSpeed;
                    float x = runTrans.x * 0.035f, y = runTrans.y * 0.035f;
                    mOwner.SetVelocity(y, x);
                }
            }
            else {
                if (mOwner.ClimbJumping) {
                    //Debug.Log("空中无法设置移动距离");
                }
            }
        } else if (mOwner.IsOnGround()) {
            //direction.Normalize();
            //跑的速度 1000 = 145M/S 按原来游戏计算
            Vector2 runTrans = direction * mOwner.MoveSpeed * (mOwner.Crouching ? 0.25f : 1.0f);//蹲下是跑步的4/1,中毒是一半速度
            float x = runTrans.x * (mOwner.Crouching ? 0.085f : 0.045f), y = runTrans.y * (mOwner.Crouching ? 0.085f : (runTrans.y >= 0 ? 0.135f : 0.045f));//前走速度145 后走速度36,左右走速度是36 模型Z轴与角色面朝相反
            mOwner.SetVelocity(y, x);
        }

    }

    bool checkInputType(EKeyList inpuKey, EInputType inputType)
    {
        int pressed = KeyStates[(int)inpuKey].Pressed;
        float pressedTime = KeyStates[(int)inpuKey].PressedTime;
        float releasedTime = KeyStates[(int)inpuKey].ReleasedTime;

        bool ret = false;
        switch (inputType)
        {
            case EInputType.EIT_Click:
                ret = (pressedTime == 0);
                break;
            case EInputType.EIT_DoubleClick:
                ret = (pressed == 2 && pressedTime == 0);
                break;
            case EInputType.EIT_Press:
                ret = (pressed != 0 && pressedTime >= LongPressedTime);
                break;
            case EInputType.EIT_Release:
                ret = (pressed == 0 && releasedTime == 0);
                break;
            case EInputType.EIT_Pressing:
                ret = (pressed != 0);
                break;
            case EInputType.EIT_Releasing:
                ret = (pressed == 0);
                break;
            case EInputType.EIT_ShortRelease://小跳
                ret = (pressed == 0 && releasedTime == 0 && pressedTime < ShortPressTime);
                break;
            case EInputType.EIT_FullPress://大跳
                ret = (pressed != 0 && pressedTime <= ShortPressTime && pressedTime + FrameReplay.deltaTime > ShortPressTime);//后面条件限制只触发一帧
                break;
            case EInputType.EIT_PressingEnough://忍者超级跳，按键蓄力足够久时间
                ret = (pressed != 0) && pressedTime >= SuperJumpTime;
                break;
        }
        return ret;
    }

    public bool HasInput(int key, int tp)
    {
        return checkInputType((EKeyList)key, (EInputType)tp);
    }
}

//控制角色输入
public class MeteorController {
    public MeteorInput Input;
    MeteorUnit mOwner;
    ActionManager posMng;
    bool mInputLocked = false;
    public bool InputLocked { get { return mInputLocked || Owner.Dead; } set { mInputLocked = value; } }
    bool mMoveLocked = false;
    public bool MoveLocked { get { return mMoveLocked; } set { mMoveLocked = value; } }
    public MeteorUnit Owner { get { return mOwner; } }
    public void Init(MeteorUnit Target)
    {
        mOwner = Target;
        if (mOwner != null)
        {
            posMng = mOwner.ActionMgr;
            Input = new MeteorInput(mOwner, this);
            if (Owner.Attr.IsPlayer)
                CombatData.Ins.GMeteorInput = Input;
        }
    }

    public void NetUpdate()
    {
        CheckActionInput(FrameReplay.deltaTime);
        if (Input != null)
            Input.NetUpdate();
    }

    //普通功能的状态转换，不涉及
    void CheckActionInput(float deltaTime)
    {
        if (Input == null || posMng == null || posMng.mActiveAction == null)
            return;
        if (Owner.Dead)
            return;
        //一些按键不限定当前POSE，类似解锁
        if (Owner.Attr.IsPlayer && Input.HasInput((int)EKeyList.KL_KeyQ, (int)EInputType.EIT_Click))
            Main.Ins.GameBattleEx.ChangeLockStatus();
        //无需考虑当前动作的处理,无需考虑硬直
        if (Input.HasInput((int)EKeyList.KL_DropWeapon, (int)EInputType.EIT_Click))
            Owner.DropWeapon();
        else if (Input.HasInput((int)EKeyList.KL_BreakOut, (int)EInputType.EIT_Click))
        {
            Owner.DoBreakOut();
            return;
        }
        //硬直中不允许其他姿势的控制.
        if (Owner.ActionMgr.IsInStraight())
            return;
        //在一定动作时才可响应的动作.
        if (Main.Ins.GameBattleEx != null && !Main.Ins.GameBattleEx.BattleFinished() && !CombatData.Ins.PauseAll)
            MeteorBehaviour.Ins.ProcessBehaviour(Owner);
        else
        {
            //战斗结束，战斗暂停

        }
    }

    //不允许响应方向键
    public void LockMove(bool locked) {
        MoveLocked = locked;
    }

    public void LockInput(bool param)
    {
        mInputLocked = param;
        if (mInputLocked)
        {
            if (Input != null)
            {
                Input.ResetInput();
                Input.mInputVector = Vector3.zero;
            }
        }
    }
}
