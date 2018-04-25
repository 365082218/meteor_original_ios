using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Meteor_KeyBoard_Input
[Serializable]
public class MeteorInput
{
    public KeyState[] KeyStates = new KeyState[(int)EKeyList.KL_Max];
    public const float DoubleClickTime = 0.4f;
    public const float LongPressedTime = 0.2f;
    public const float ShortPressTime = 0.12f;
    MeteorUnit mOwner;
    PoseStatus posMng;
    InputModule InputCore;
    MeteorController mController = null;
    public Vector2 mInputVector = Vector2.zero;
    Vector2 mLastInputVector = Vector2.one;
    public float OffX;
    public float OffZ;
    public List<InputRecord> Record = new List<InputRecord>();
    //检查某个被激活的按键，是否处于激活未重置状态,这个状态下，是不允许再次激活按键的，需要先重置再激活，来触发连续按键
    //轴往一个方向上拖动到 一定的范围(60-100像素区域内)，就会压住这个摇杆侧的按键，模拟街机摇杆
    //摇杆控制接口
    List<EKeyList> keyStateOnActive = new List<EKeyList>();
    public bool IsKeyOnActive(EKeyList k)
    {
        if (keyStateOnActive.Contains(k))
            return true;
        return false;
    }

    public void OnAxisKeyPress(EKeyList k)
    {
        if (!keyStateOnActive.Contains(k))
        {
            keyStateOnActive.Add(k);
            OnKeyDown(k);
        }
        else
            OnKeyPressing(k);
        
    }
    
    public bool OnInputMoving()
    {
        return (mInputVector.x != 0 || mInputVector.y != 0);
    }

    public void OnAxisKeyRelease(EKeyList k)
    {
        if (keyStateOnActive.Contains(k))
            keyStateOnActive.Remove(k);
        OnKeyUp(k);
    }
    
    //键盘控制模块
    public MeteorInput(MeteorUnit owner, MeteorController controller)
    {
        mOwner = owner;
        mController = controller;
        posMng = mOwner.posMng;
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
    }

    void SetController(MeteorController controller)
    {
        mController = controller;
    }

    public void Update(float deltaTime)
    {
        if (!NGUIJoystick.instance.ArrowPressed) mLastInputVector = Vector2.zero;
        if (mOwner.Attr.IsPlayer)
        {
            //如果方向键按下了
            if (NGUIJoystick.instance.mJoyPressed)
                mInputVector = new Vector2(NGUIJoystick.instance.Delta.x, NGUIJoystick.instance.Delta.y);
            else if (NGUIJoystick.instance.ArrowPressed)
                mInputVector = new Vector2(NGUIJoystick.instance.Delta.x, NGUIJoystick.instance.Delta.y);
            else
                mInputVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }
        else
            mInputVector = new Vector2(OffX, OffZ);//AIMove 自动战斗输入

        //如果正在旋转角色，且动作ID不是前跑，那么是无法移动的，原地旋转角色.
        //if (mOwner.posMng.Rotateing && mOwner.posMng.mActiveAction.Idx != CommonAction.Run)
        //    mInputVector = Vector2.zero;

        InputCore.Update();
        //主角色，扫描硬件信息
        if (mOwner.Attr.IsPlayer)
            UpdateKeyStatus(deltaTime);
        UpdateMoveInput(deltaTime);
    }

    //使用摇杆的时候不要读取硬件信息。
    void UpdateKeyStatus(float deltaTime)
    {
        //不能在循环中处理按键的抬起和按下，必须先缓存一份干净的数据
        //然后扫描输入，处理一些非法输入，再用处理后的合法输入，做最后状态的刷新。
        //否则相反键问题无法解决，比如同时按下键盘AD，则后输入的会覆盖前输入的。
        for (int idx = 0; idx < KeyStates.Length; idx++)
        {
            KeyState keyStatus = KeyStates[idx];
            keyStatus.PressedTime += deltaTime;
            keyStatus.ReleasedTime += deltaTime;

            //底下做的是与键盘同步，多输入设备会让其他设备的输出,被键盘状态刷新掉
            if (string.IsNullOrEmpty(keyStatus.AxisName))
                continue;
            //怪物或者AI，都不读取硬件消息。
            // 假设当前用的是鼠标，摇杆，那么摇杆触发的键位会被键盘重置掉。
            //只有使用键盘的时候，键的状态才从与键盘状态同步。
            //return;
            //发布为PC的时候，才会从硬件扫描
#if BUILD_PC
            float kValue = Input.GetAxisRaw(keyStatus.AxisName);
            bool pressed = kValue > 0;
            if (pressed && keyStatus.Pressed == 0)
                OnKeyDown(keyStatus, false);
            else if (!pressed && keyStatus.Pressed != 0)
                OnKeyUp(keyStatus);
            else if (pressed && keyStatus.Pressed != 0)
                OnKeyPressing(keyStatus);
#endif            
        }

        //if (!Application.isMobilePlatform || Startup.ins.state.useJoystickOrKeyBoard)
        //{
        //    //处理同轴2个按键抵消的问题
        //    //没有同时按下相反方向键的时候
        //    bool w = Input.GetKey(KeyCode.W);
        //    bool s = Input.GetKey(KeyCode.S);
        //    bool a = Input.GetKey(KeyCode.A);
        //    bool d = Input.GetKey(KeyCode.D);

        //    Vector2 vec = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        //    //Debug.Log("w s a d" + w + " " + s + " " + a + " " + d);
        //    if (vec.x != 0)
        //    {
        //        if (vec.x > 0)
        //        {
        //            if (KeyStates[(int)EKeyList.KL_KeyD].Pressed == 0)
        //                OnKeyDown(KeyStates[(int)EKeyList.KL_KeyD], false);
        //            else
        //                OnKeyPressing(KeyStates[(int)EKeyList.KL_KeyD]);
        //        }

        //        if (vec.x > 0 && KeyStates[(int)EKeyList.KL_KeyA].Pressed != 0)
        //            OnKeyUp(KeyStates[(int)EKeyList.KL_KeyA]);

        //        if (vec.x < 0)
        //        {
        //            if (KeyStates[(int)EKeyList.KL_KeyA].Pressed == 0)
        //                OnKeyDown(KeyStates[(int)EKeyList.KL_KeyA], false);
        //            else
        //                OnKeyPressing(KeyStates[(int)EKeyList.KL_KeyA]);
        //        }

        //        if (vec.x < 0 && KeyStates[(int)EKeyList.KL_KeyD].Pressed != 0)
        //            OnKeyUp(KeyStates[(int)EKeyList.KL_KeyD]);
        //    }
        //    else
        //    {
        //        //2个都没有按下，或者2个都按下。
        //        if (KeyStates[(int)EKeyList.KL_KeyD].Pressed != 0 && !d)
        //            OnKeyUp(KeyStates[(int)EKeyList.KL_KeyD]);
        //        else if (KeyStates[(int)EKeyList.KL_KeyD].Pressed == 0 && d)
        //            OnKeyDown(KeyStates[(int)EKeyList.KL_KeyD], false);

        //        if (KeyStates[(int)EKeyList.KL_KeyA].Pressed != 0 && !a)
        //            OnKeyUp(KeyStates[(int)EKeyList.KL_KeyA]);
        //        else if (KeyStates[(int)EKeyList.KL_KeyA].Pressed == 0 && a)
        //            OnKeyDown(KeyStates[(int)EKeyList.KL_KeyA], false);
        //    }

        //    if (vec.y != 0)
        //    {
        //        //同向键按下，
        //        if (vec.y > 0)
        //        {
        //            if (KeyStates[(int)EKeyList.KL_KeyW].Pressed == 0)
        //                OnKeyDown(KeyStates[(int)EKeyList.KL_KeyW], false);
        //            else
        //                OnKeyPressing(KeyStates[(int)EKeyList.KL_KeyW]);
        //        }

        //        if (vec.y < 0 && KeyStates[(int)EKeyList.KL_KeyW].Pressed != 0)
        //            OnKeyUp(KeyStates[(int)EKeyList.KL_KeyW]);

        //        //反向键抬起,
        //        if (vec.y > 0 && KeyStates[(int)EKeyList.KL_KeyS].Pressed != 0)
        //            OnKeyUp(KeyStates[(int)EKeyList.KL_KeyS]);
        //        if (vec.y < 0)
        //        {
        //            if (KeyStates[(int)EKeyList.KL_KeyS].Pressed == 0)
        //                OnKeyDown(KeyStates[(int)EKeyList.KL_KeyS], false);
        //            else
        //                OnKeyPressing(KeyStates[(int)EKeyList.KL_KeyS]);
        //        }

        //    }
        //    else
        //    {
        //        //2个都没有按下，或者2个都按下。
        //        if (KeyStates[(int)EKeyList.KL_KeyW].Pressed != 0 && !w)
        //            OnKeyUp(KeyStates[(int)EKeyList.KL_KeyW]);
        //        else if (KeyStates[(int)EKeyList.KL_KeyW].Pressed == 0 && w)
        //        {
        //            OnKeyDown(KeyStates[(int)EKeyList.KL_KeyW], false);
        //        }
        //        if (KeyStates[(int)EKeyList.KL_KeyS].Pressed != 0 && !s)
        //            OnKeyUp(KeyStates[(int)EKeyList.KL_KeyS]);
        //        else if (KeyStates[(int)EKeyList.KL_KeyS].Pressed == 0 && s)
        //        {
        //            OnKeyDown(KeyStates[(int)EKeyList.KL_KeyS], false);
        //        }
        //    }
        //}
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
//从缓冲区的前多少,取得某个键的按下次数
//int GetDownCount(EKeyList key)
//{
//    int cnt = 0;
//    for (int i = 0; i < KeyLink.Count; i++)
//    {
//        if (KeyLink[i].Key == key && KeyLink[i].ActiveType == ActiveTp.ActivePress)
//            cnt++;
//    }
//    return cnt;
//}

//int GetUpCount(EKeyList key)
//{
//    int cnt = 0;
//    for (int i = 0; i < KeyLink.Count; i++)
//    {
//        if (KeyLink[i].Key == key && KeyLink[i].ActiveType == ActiveTp.ActiveRelease)
//            cnt++;
//    }
//    return cnt;
//}

    //返回当前动作是否在可接受输入帧内
    public bool AcceptInput()
    {
        Pose p = PoseStatus.ActionList[mOwner.UnitId][posMng.mActiveAction.Idx];
        if (p.Next != null)
        {
            int inputEnd = p.Next.End;
            int curIndex = mOwner.charLoader.GetCurrentFrameIndex();
            if (p.Next.Start <= curIndex && curIndex <= inputEnd)
            {
                //if (posMng.mActiveAction.Idx >= 295 && posMng.mActiveAction.Idx <= 300)
                //    Debug.LogError("pos:" + p.Idx + " p.next start:" + p.Next.Start + "  cur index = " + curIndex + " input end:" + inputEnd);
                return true;
            }
            else
                return false;
        }
        return true;
    }

    public bool CheckPos(int KeyMap, int targetAct)
    {
        bool result = false;
        //首先要判断是否在可输入范围内,只是限定输入，切换动作在其他地方
        if (!AcceptInput())
            return false;
        int weapon = mOwner.GetWeaponType();
        //状态要抵消,必须方向相反.比如匕首后前前A,如果在按住后的时候,只需要输入上上A就可以凑招了
        //当解析方向相反的招式起始按键时,必须考虑凑招问题
        //单独按键需要判断 跳,爬墙,等状态
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
                        if (GameData.gameStatus.EnableInfiniteAngry)
                            result = true;
                        else
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
                        if (GameData.gameStatus.EnableInfiniteAngry)
                            result = true;
                        else
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                    case 150://空中下下A
                        if (GameData.gameStatus.EnableInfiniteAngry)
                            result = true;
                        else
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
                        if (GameData.gameStatus.EnableInfiniteAngry)
                            result = true;
                        else
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
                        if (GameData.gameStatus.EnableInfiniteAngry)
                            result = true;
                        else
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                    case 156://雷电斩
                        if (GameData.gameStatus.EnableInfiniteAngry)
                            result = true;
                        else
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
                        if (GameData.gameStatus.EnableInfiniteAngry)
                            result = true;
                        else
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
                        if (GameData.gameStatus.EnableInfiniteAngry)
                            result = true;
                        else
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
                        if (GameData.gameStatus.EnableInfiniteAngry)
                            result = true;
                        else
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
                        if (GameData.gameStatus.EnableInfiniteAngry)
                            result = true;
                        else
                        if (mOwner.AngryValue < 100)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 100;
                            result = true;
                        }
                        break;
                    case 147://小绝
                        if (GameData.gameStatus.EnableInfiniteAngry)
                            result = true;
                        else
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
                    case 12:
                    case 145:
                    case 146:
                        result = true;
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
                        if (GameData.gameStatus.EnableInfiniteAngry)
                            result = true;
                        else
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
                        if (GameData.gameStatus.EnableInfiniteAngry)
                            result = true;
                        else
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                    case 149://加速BUFF
                        if (GameData.gameStatus.EnableInfiniteAngry)
                            result = true;
                        else
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
                        if (GameData.gameStatus.EnableInfiniteAngry)
                            result = true;
                        else
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
                        if (GameData.gameStatus.EnableInfiniteAngry)
                            result = true;
                        else
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            result = true;
                            mOwner.AngryValue -= 60;
                        }
                        break;
                    case 152://右左A小绝旋转
                        if (GameData.gameStatus.EnableInfiniteAngry)
                            result = true;
                        else
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
                        if (GameData.gameStatus.EnableInfiniteAngry)
                            result = true;
                        else
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
                        if (GameData.gameStatus.EnableInfiniteAngry)
                            result = true;
                        else
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                    case 154://左右下A 小绝招
                        if (GameData.gameStatus.EnableInfiniteAngry)
                            result = true;
                        else
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
                        if (GameData.gameStatus.EnableInfiniteAngry)
                            result = true;
                        else
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
                        if (GameData.gameStatus.EnableInfiniteAngry)
                            result = true;
                        else
                        if (mOwner.AngryValue < 60)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 60;
                            result = true;
                        }
                        break;
                    case 158://大绝
                        if (GameData.gameStatus.EnableInfiniteAngry)
                            result = true;
                        else
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

                    case 97://444,拔刀小绝
                        result = mOwner.GetWeaponSubType() == 1;
                        if (result)
                        {
                            if (GameData.gameStatus.EnableInfiniteAngry)
                                result = true;
                            else
                            if (mOwner.AngryValue < 60)
                                result = false;
                            else
                            {
                                mOwner.AngryValue -= 60;
                                result = true;
                            }
                        }
                        break;
                    case 98://445
                        result = mOwner.GetWeaponSubType() == 1;
                        break;
                    case 99://右A 居合转换拔刀447
                    case 100://左A 居合转换持枪 448 
                    case 101://520,521,522,523
                        result = mOwner.GetWeaponSubType() == 2;
                        break;
                    case 102://449-持枪小绝
                        result = mOwner.GetWeaponSubType() == 2;
                        if (result)
                        {
                            if (GameData.gameStatus.EnableInfiniteAngry)
                                result = true;
                            else
                            if (mOwner.AngryValue < 60)
                                result = false;
                            else
                            {
                                mOwner.AngryValue -= 60;
                                result = true;
                            }
                        }
                        break;
                    case 103://450
                        result = mOwner.GetWeaponSubType() == 2;
                        break;
                    case 104://451//大绝招
                        result = mOwner.GetWeaponSubType() == 2;
                        if (result)
                        {
                            if (GameData.gameStatus.EnableInfiniteAngry)
                                result = true;
                            else
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
                    case 159:
                        result = mOwner.GetWeaponSubType() == 1;
                        break;
                    case 160:
                        result = mOwner.GetWeaponSubType() == 0;
                        break;
                }
                break;
            case (int)EquipWeaponType.Gloves://拳套
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
                        if (GameData.gameStatus.EnableInfiniteAngry)
                            result = true;
                        else
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
                        if (GameData.gameStatus.EnableInfiniteAngry)
                            result = true;
                        else
                        if (mOwner.AngryValue < 100)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 100;
                            result = true;
                        }
                        break;
                    case 121://嗜血
                        if (GameData.gameStatus.EnableInfiniteAngry)
                            result = true;
                        else
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
                        if (GameData.gameStatus.EnableInfiniteAngry)
                            result = true;
                        else
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
                        if (GameData.gameStatus.EnableInfiniteAngry)
                            result = true;
                        else
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
                        if (GameData.gameStatus.EnableInfiniteAngry)
                            result = true;
                        else
                        if (mOwner.AngryValue < 50)
                            result = false;
                        else
                        {
                            mOwner.AngryValue -= 50;
                            result = true;
                        }
                        break;
                    case 142://左右A 10气
                        if (GameData.gameStatus.EnableInfiniteAngry)
                            result = true;
                        else
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

    public void ResetJoy()
    {
        OnKeyUp(KeyStates[(int)EKeyList.KL_KeyA]);
        OnKeyUp(KeyStates[(int)EKeyList.KL_KeyD]);
        OnKeyUp(KeyStates[(int)EKeyList.KL_KeyW]);
        OnKeyUp(KeyStates[(int)EKeyList.KL_KeyS]);
    }

    public void ResetInput()
    {
        foreach (KeyState keyState in KeyStates)
        {
            if (keyState.Pressed != 0)
                OnKeyUp(keyState);
        }

        InputCore.Reset();
    }

    public void OnKeyDown(EKeyList key, bool isAI = false)
    {
        OnKeyDown(KeyStates[(int)key], isAI);
    }

    public void OnKeyPressing(EKeyList key)
    {
        OnKeyPressing(KeyStates[(int)key]);
    }

    Dictionary<EKeyList, int> genFreq = new Dictionary<EKeyList, int>();
    public void OnKeyPressing(KeyState keyStatus)
    {
        if (mOwner.controller.InputLocked)
            return;
        if (genFreq.ContainsKey(keyStatus.Key))
        {
            genFreq[keyStatus.Key]--;
            if (genFreq[keyStatus.Key] == 0)
            {
                InputCore.OnKeyPressing(keyStatus);
                genFreq[keyStatus.Key] = Global.waitForNextInput + 1;
            }
        }
        else
            genFreq.Add(keyStatus.Key, Global.waitForNextInput + 1);
    }

    public void OnKeyUp(EKeyList key)
    {
        if (mOwner.controller.InputLocked)
            return;
        //Log.LogInfo("OnKeyUp key:" + key);
        OnKeyUp(KeyStates[(int)key]);
    }

    
    public void OnKeyDown(KeyState keyStatus, bool isAI)
    {
        if (mOwner.controller.InputLocked)
            return;
        InputRecord rec = new InputRecord();
        rec.key = keyStatus.Key;
        rec.pressed = true;
        Record.Add(rec);
        if (Record.Count >= 5)
            Record.RemoveAt(0);
        //if (!PoseInputLocked())
        InputCore.OnKeyDown(keyStatus);
        //任意的按下一个按键，会让重复扫描功能暂停
        EKeyList[] keys = new EKeyList[genFreq.Keys.Count];
        genFreq.Keys.CopyTo(keys, 0);
        for (int i = 0; i < keys.Length; i++)
            genFreq[keys[i]] = Global.waitForNextInput + 1;
        keyStatus.Pressed = keyStatus.PressedTime < DoubleClickTime ? 2 : 1;
        keyStatus.PressedTime = 0.0f;
        keyStatus.IsAI = isAI;
        //if (FightWnd.Exist)
        //    FightWnd.Instance.UpdateInputInfo();
    }

    public void OnKeyUp(KeyState keyStatus)
    {
        if (mOwner.controller.InputLocked)
            return;
        //InputRecord rec = new InputRecord();
        //rec.key = keyStatus.Key;
        //rec.pressed = false;
        //Record.Add(rec);
        //if (Record.Count >= 5)
        //    Record.RemoveAt(0);
        //if (!PoseInputLocked())
        //    InputModel.Instance.OnKeyUp(keyStatus);
        //if (genFreq.ContainsKey(keyStatus.Key))
        //    genFreq.Remove(keyStatus.Key);
        keyStatus.Pressed = 0;
        keyStatus.ReleasedTime = 0.0f;
        //if (FightWnd.Exist)
        //    FightWnd.Instance.UpdateInputInfo();
    }

    public bool IsKeyDown(EKeyList key)
    {
        foreach (KeyState keyStatus in KeyStates)
        {
            if (keyStatus.Key == key && keyStatus.Pressed != 0 && keyStatus.IsAI == false)
                return true;
        }
        return false;
    }

    public bool HasKeyDown()
    {
        foreach (KeyState keyStatus in KeyStates)
        {
            if (string.IsNullOrEmpty(keyStatus.AxisName))
                continue;
            if (keyStatus.Pressed != 0 && keyStatus.IsAI == false)
                return true;
        }
        return false;
    }

    void UpdateMoveInput(float deltaTime)
    {
        Vector2 direction = mInputVector;//摇杆
        if (direction == Vector2.zero)
            return;
        //如果摇杆按着边缘的方向键，触发任意方向，则移动，否则，旋转目标。
        //跳跃的时候，方向轴受一定控制，但是强度没那么大.
        //只有在爬墙的时候，才能手动控制方向.只有按住上，才会继续给水平冲量，这个冲量方向顺着爬墙的方向
        if (mOwner.Climbing)
        {
            direction.Normalize();
            Vector2 runTrans = direction * mOwner.Speed;
            float y = runTrans.y * 0.145f;//1.5倍
            if (y > 0 && mOwner.ClimbingTime < Global.ClimbLimit)
            {
                float climbScale = (Global.ClimbLimit - mOwner.ClimbingTime) / (Global.ClimbLimit);
                mOwner.SetWorldVelocity(-y * mOwner.transform.forward);//给角色面向方向一个冲量，让角色顺着墙壁跑，当Y达到一个限制时，就会从墙壁落下
                mOwner.AddYVelocity(y  * climbScale);
            }
        }
        else if (mOwner.IsOnGround())
        {
            if (posMng.CanMove)
            {
                direction.Normalize();
                //跑的速度 1000 = 145M/S 按原来游戏计算
                Vector2 runTrans = direction * mOwner.Speed * (mOwner.Crouching ? 0.25f : 1);//蹲下是跑步的4/1
                float x = runTrans.x * (mOwner.Crouching ? 0.065f : 0.036f), y = runTrans.y * (mOwner.Crouching ? 0.065f : (runTrans.y >= 0 ? 0.145f: 0.036f));//前走速度145 后走速度36,左右走速度是36 模型Z轴与角色面朝相反
                mOwner.SetVelocity(y, x);
            }
            else
            {
                //无法移动
            }
        }
        else if (mOwner.posMng.mActiveAction.Idx == CommonAction.Jump && mOwner.ImpluseVec.x == 0.0f && mOwner.ImpluseVec.z == 0.0f)//跳跃后按下方向，微量的跳跃速度
        {
            direction.Normalize();
            Vector2 runTrans = direction * mOwner.Speed;
            float x = runTrans.x * 0.036f, y = runTrans.y * 0.036f;
            mOwner.SetVelocity(y, x);
        }
    }

    bool checkInputType(EKeyList inpuKey, EInputType inputType, float deltaTime)
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
                ret = (pressed != 0 && (pressedTime < LongPressedTime && pressedTime + deltaTime >= LongPressedTime));
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
                ret = (pressed != 0 && pressedTime <= ShortPressTime && pressedTime + deltaTime >= ShortPressTime);
                break;
        }
        return ret;
    }

    public bool HasInput(int key, int tp, float time)
    {
        return checkInputType((EKeyList)key, (EInputType)tp, time);
    }
}

//控制输入，以及摄像机的调整.
public class MeteorController : MonoBehaviour {
    public MeteorInput Input;
    MeteorUnit mOwner;
    PoseStatus posMng;
    float mCameraModify = 0.0f;
    bool mInputLocked = false;
    Vector3 mCameraPosCache = Vector3.zero;
    Vector3 mCameraOffset = Vector3.zero;

    public bool InputLocked { get { return mInputLocked; } set { mInputLocked = value; } }

    public Transform CameraTag;
    public static MeteorController currentController = null;

    public Transform CameraTarget;
    public bool doCameraView = false;
    public Vector3 FeatureCameraPos = Vector3.zero;
    public Vector3 FeatureIncCameraPos = Vector3.zero;
    public Vector3 FeatureNowCameraPos = Vector3.zero;

    public Vector3 CameraPos = new Vector3(3, 2, 0);
    public Vector3 CameraLookAtOffset = new Vector3(0, 1, 0);
    Vector3 LookAtPos;
    public Vector2 BossCamera = new Vector2(30, 20);
    public MeteorUnit Owner { get { return mOwner; } }

   


    void Start()
    {
    }

    public void Init()
    {
        mOwner = GetComponent<MeteorUnit>();
        if (mOwner != null)
            posMng = mOwner.posMng;
        else
            return;
        OnEnable();
        BossCamera.x = 18;
    }

    void OnEnable()
    {
        if (posMng != null)
        {
            Input = new MeteorInput(mOwner, this);
            if (Owner.Attr.IsPlayer)
                Global.GMeteorInput = Input;
        }
    }

    void OnDestroy()
    {
        if (Input != null)
            Input.ResetInput();
    }

    private void Update()
    {
        if (Global.PauseAll)
            return;

        bool blockInput = false;
        if (Owner.robot != null)
        {
            Owner.robot.Update();
            blockInput = Owner.robot.InputBlocked();
        }
        if (!mInputLocked && !blockInput)
        {
            CheckActionInput(Time.deltaTime);
            if (Input != null)
                Input.Update(Time.deltaTime);
        }
    }

    // LateUpdate is called after all Update functions have been called
    void LateUpdate()
    {
        if (Global.PauseAll)
            return;
        if (mOwner != null)
            UpdateCamera(Time.deltaTime);
    }

    //普通功能的状态转换，不涉及
    void CheckActionInput(float deltaTime)
    {
        if (Input == null || posMng == null || posMng.mActiveAction == null)
            return;
        if (Owner.Dead)
            return;
        //一些按键不限定当前POSE，类似解锁
        if (Owner.Attr.IsPlayer && Input.HasInput((int)EKeyList.KL_KeyQ, (int)EInputType.EIT_Click, Time.deltaTime))
            GameBattleEx.Instance.ChangeLockStatus();
        //无需考虑当前动作的处理
        if (Input.HasInput((int)EKeyList.KL_DropWeapon, (int)EInputType.EIT_Click, Time.deltaTime))
            Owner.DropWeapon();
        //行为树处理.
        MeteorBehaviour.Instance.ProcessBehaviour(Owner);
    }


    public Vector2 BossDieOffset = new Vector2(5, 0);
    Vector3 mBeginOffset = Vector3.zero;
    Vector3 mAddtiveOffset = Vector3.zero;
    float mAdjustTime = 3.0f;
    float mAdjustLeftTime = 0.0f;
    float preY = float.MaxValue;
    int HitY = 1;
    int curHity = 0;
    Transform trans;
    bool isAttackMove;
    Vector3 attackPos = Vector3.zero;
    Vector3 attackOffset = Vector3.zero;
    bool IsBoss;
    float BossDis;//boss 摄像机距离.
    bool IsBossDeading;

    bool BossDead = false;
    //public CameraController CameraController = null;
    void UpdateCamera(float deltaTime)
    {
        if (CameraTag == null)
            return;
    }

    void ChangeCamera()
    {
    }

    void UpdateFollowBone()
    {
        if (Owner.posMng.FollowBone != null)
        {
            Transform tran = Owner.posMng.FollowBone;
            if (isAttackMove == false)
            {
                attackPos = tran.position;
            }
            else
            {
                CameraLookAtOffset = (attackPos - tran.position) * Owner.posMng.FollowBoneScale + Vector3.up;
                attackOffset = (attackPos - tran.position) * Owner.posMng.FollowBoneScale;
            }
            isAttackMove = true;
        }
        else
        {
            CameraLookAtOffset = Vector3.Lerp(CameraLookAtOffset, Vector3.up, Time.deltaTime * 5);
            attackOffset = Vector3.Lerp(attackOffset, Vector3.zero, Time.deltaTime * 5);
            isAttackMove = false;
        }
    }

    bool IsRotateCameraByBoss = false;

    void RotateCameraByBoss(Vector3 targetPos)
    {
        IsRotateCameraByBoss = true;
    }

    public Vector3 LockPos;
    public bool IsLockPos;
    void OnLockPos()
    {
        Vector3 targetPos = Vector3.zero;
        targetPos = LockPos;
        CameraTag.position = targetPos + CameraPos;
        LookAtPos = targetPos + CameraLookAtOffset;
        CameraTag.LookAt(LookAtPos);
        CameraTag.Translate(mCameraOffset);
        CameraTag.transform.position += CameraTag.transform.forward;
    }

    void UpdateBossCamera(float deltaTime)
    {
        if (mCameraPosCache != CameraPos)
        {
            mCameraModify = Mathf.Atan2(CameraPos.x, CameraPos.z);
            mCameraPosCache = CameraPos;
        }

        Vector3 targetPos = Vector3.zero;
        if (doCameraView && CameraTarget != null)
        {
            targetPos = CameraTarget.position;
        }
        else
        {
        }
    }

    Vector3 pos;

    public void LockInput(bool param)
    {
        mInputLocked = param;
        if (Input != null)
            Input.ResetInput();
        //CheckActionInput(Time.deltaTime);
    }
}
