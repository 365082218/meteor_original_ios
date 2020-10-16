using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Excel2Json;


public class InputItem
{
    public int state;
    public int totalState;
    public int frame;
    public char[] keyInput;
    public EKeyList[] KeyInput;
    public Dictionary<int, List<int>> lines = new Dictionary<int, List<int>>();//存储 行-行里的Pose集合
    public Dictionary<int, List<int>> linesAir = new Dictionary<int, List<int>>();
    public int Idx;
    public MeteorUnit mOwner;//输入控制者
    EKeyList last;
    string to;
    public override string ToString()
    {
        if (string.IsNullOrEmpty(to))
            to = new string(keyInput);
        return to;
    }

    public InputItem(MeteorUnit owner)
    {
        mOwner = owner;
    }

    public EKeyList Pressing = EKeyList.KL_None;
    public void OnKeyPressing(KeyState k) {
        if (state == 0) {
            if (k.Key == KeyInput[state]) {
                state++;
                Pressing = k.Key;//等待蓄力招式后续输入
                frame = Main.Ins.AppInfo.LinkDelay();
            }
        }
    }

    public bool OnKeyDown(KeyState k)
    {
        if (KeyInput.Length <= state || state < 0)
            return false;
        if (k.Key == KeyInput[state])
        {
            Pressing = EKeyList.KL_None;
            state++;
            if (state == totalState)
            {
                int target = -1;
                if (Check(out target))
                {
                    mOwner.ActionMgr.LinkAction(target);
                    return true;
                }
                else
                {
                    Reset();
                }
            }
            else
            {
                frame = Main.Ins.AppInfo.LinkDelay();
            }
        }
        return false;
    }

    bool Check(out int targetPose)
    {
        if (mOwner != null)
        {
            //在硬直中
            if (mOwner.ActionMgr.IsInStraight())
            {
                //Debug.LogError("in straight:" + mOwner.charLoader.PoseStraight);
                targetPose = -1;
                return false;
            }

            //
            //技能中的动作，不许接输入
            if (mOwner.IsPlaySkill)
            {
                targetPose = -1;
                return false;
            }
            if (ActionInterrupt.Ins.Whole.ContainsKey(mOwner.ActionMgr.mActiveAction.Idx) || mOwner.GetWeaponType() == (int)EquipWeaponType.Gun)
            {
                int targetIdx = mOwner.ActionMgr.mActiveAction.Idx;

                //滚动或者闪动。可以连任意Pose与Idle一样
                if (targetIdx >= 164 && targetIdx <= 179)
                    targetIdx = 0;
                if (targetIdx >= 112 && targetIdx <= 113)
                    targetIdx = 0;

                //任意准备动作，可以接任意Pose与Idle一样
                if (ActionManager.IsReadyAction(targetIdx))
                    targetIdx = 0;

                //部分火枪动作，翻滚，攻击，都有后摇，不允许立即切换，必须等待动作完成
                if (mOwner.GunReady && !mOwner.ActionMgr.IsAttackPose() && 
                    !(mOwner.ActionMgr.mActiveAction.Idx >= CommonAction.DCForw && mOwner.ActionMgr.mActiveAction.Idx <= CommonAction.DCBack))
                {
                    targetIdx = CommonAction.Idle;//模拟普通IDLE就可以发其他火枪招式了
                    //Debug.LogError("useGun");
                }

                if (!ActionInterrupt.Ins.Whole.ContainsKey(targetIdx))
                {
                    //Debug.LogError(string.Format("not contains:{0}", targetIdx));
                    targetPose = -1;
                    return false;
                }

                //枪射击大招
                if (mOwner.ActionMgr.mActiveAction.Idx == CommonAction.GunSkillShootMax) {
                    targetPose = -1;
                    return false;
                }

                ActionNode no = ActionInterrupt.Ins.Whole[targetIdx];
                //if (targetIdx == 200)
                //{
                //    Debug.LogError("200--");
                //}
                if (mOwner.IsOnGround())
                {
                    //拦截火枪的状态机，只要不处于213-使用火枪就进入212POSE
                    if (!mOwner.GunReady && mOwner.GetWeaponType() == (int)EquipWeaponType.Gun)
                    {
                        targetPose = CommonAction.GunReload;
                        //Debug.LogError("targetPose = 212");
                        return true;
                    }

                    foreach (var each in lines)
                    {
                        for (int i = 0; i < no.target.Count; i++)
                        {
                            
                            if (each.Value.Contains(no.target[i].ActionIdx))
                            {
                                //if (no.target[i].ActionIdx == 407)
                                //    Debug.LogError("hit");
                                bool ret = mOwner.meteorController.Input.CheckPos(each.Key, no.target[i].ActionIdx);
                                if (ret)
                                {
                                    //if (no.target[i].ActionIdx == 407)
                                    //    Debug.LogError("hit ok");
                                    targetPose = no.target[i].ActionIdx;
                                    //if (targetPose == 200 && targetIdx == 200)
                                    //{
                                    //    Debug.DebugBreak();
                                    //    Debug.LogError("link 200");
                                    //}
                                    return ret;
                                } else {
                                    //if (no.target[i].ActionIdx == 407)
                                    //    Debug.LogError("hit error");
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (var each in linesAir)
                    {
                        for (int i = 0; i < no.target.Count; i++)
                        {
                            if (each.Value.Contains(no.target[i].ActionIdx))
                            {
                                bool ret = mOwner.meteorController.Input.CheckPos(each.Key, no.target[i].ActionIdx);
                                if (ret)
                                {
                                    targetPose = no.target[i].ActionIdx;
                                    return ret;
                                }
                            }
                        }
                    }
                }
            }
        }
        targetPose = -1;
        return false;
    }

    public void Reset()
    {
        state = 0;
        frame = Main.Ins.AppInfo.LinkDelay();
        Pressing = EKeyList.KL_None;
    }

    public bool Wait()
    {
        if (state != 0 && state != totalState)
            return true;
        return false;
    }

    public void Update()
    {
        frame -= 1;
        if (frame < 0)
            Reset();
    }
}

[Serializable]
public class KeyState
{
    public string AxisName;
    public float PressedTime = 10.0f;
    public float ReleasedTime = 10.0f;
    public int Pressed = 0;
    public EKeyList Key;
};

public class InputModule
{
    MeteorUnit mOwner;
    public List<InputItem> inputs = new List<InputItem>();
    //SortedList input = new SortedList();//KeyMap与招式的对应关系.
    public InputModule(MeteorUnit owner)
    {
        if (inputs.Count != 0)
            return;
        mOwner = owner;
        List<InputData> ipts = DataMgr.Ins.GetInputDatas();
        for (int i = 0; i < ipts.Count; i++)
        {
            InputItem it = new InputItem(owner);
            it.Idx = ipts[i].Key;
            it.keyInput = ipts[i].InputString.ToCharArray();
            it.KeyInput = new EKeyList[it.keyInput.Length];
            for (int j = 0; j < it.keyInput.Length; j++)
            {
                if (it.keyInput[j].Equals('W'))
                    it.KeyInput[j] = EKeyList.KL_KeyW;
                else
                if (it.keyInput[j].Equals('S'))
                    it.KeyInput[j] = EKeyList.KL_KeyS;
                else
                if (it.keyInput[j].Equals('A'))
                    it.KeyInput[j] = EKeyList.KL_KeyA;
                else
                if (it.keyInput[j].Equals('D'))
                    it.KeyInput[j] = EKeyList.KL_KeyD;
                else
                if (it.keyInput[j].Equals('J'))
                    it.KeyInput[j] = EKeyList.KL_Attack;
                //else
                //if (it.keyInput[j] == 'K')
                //    it.KeyInput[j] = EKeyList.KL_Jump;
            }

            string[] str = ipts[i].Lines.Trim(new char[] { '\"', '\t' }).Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int j = 0; j < str.Length; j++)
            {
                it.lines.Add(int.Parse(str[j]), new List<int>());
            }
            str = ipts[i].LinesAir.Trim(new char[] { '\"', '\t' }).Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int j = 0; j < str.Length; j++)
            {
                it.linesAir.Add(int.Parse(str[j]), new List<int>());
            }
            it.frame = Main.Ins.AppInfo.LinkDelay();
            it.state = 0;
            it.totalState = it.keyInput.Length;
            int[] lines = new int[it.lines.Keys.Count];
            it.lines.Keys.CopyTo(lines, 0);
            int[] linesAir = new int[it.linesAir.Keys.Count];
            it.linesAir.Keys.CopyTo(linesAir, 0);

            for (int j = 0; j < lines.Length; j++)
            {
                if (ActionInterrupt.Ins.Lines.ContainsKey(lines[j]))
                {
                    for (int k = 0; k < ActionInterrupt.Ins.Lines[lines[j]].Count; k++)
                    {
                        it.lines[lines[j]].Add(ActionInterrupt.Ins.Lines[lines[j]][k]);
                    }
                }
                else
                    Debug.Log(string.Format("line:{0} miss", lines[j]));
            }
            for (int j = 0; j < linesAir.Length; j++)
            {
                if (ActionInterrupt.Ins.Lines.ContainsKey(linesAir[j]))
                {
                    for (int k = 0; k < ActionInterrupt.Ins.Lines[linesAir[j]].Count; k++)
                    {
                        it.linesAir[linesAir[j]].Add(ActionInterrupt.Ins.Lines[linesAir[j]][k]);
                    }
                }
                else
                    Debug.Log(string.Format("line:{0} miss", linesAir[j]));
            }
            inputs.Add(it);
            //input.Add(i, it.);
        }
    }

    public void Update()
    {
        for (int i = 0; i < inputs.Count; i++)
        {
            //等待下一个指令输入的，需要更新
            if (inputs[i].Wait())
                inputs[i].Update();
            else 
            {
                //重置了状态的，需要响应角色的蓄力键
            }
        }
    }

    public void Reset()
    {
        for (int i = 0; i < inputs.Count; i++)
            inputs[i].Reset();
    }

    public bool OnKeyDown(KeyState keyStatus)
    {
        for (int i = inputs.Count - 1; i >= 0; i--)
        {
            //if (inputs[i].ToString() == "SSWJ" && keyStatus.Key == EKeyList.KL_Attack && mOwner.Attr.IsPlayer && (mOwner.posMng.mActiveAction.Idx == 431 || mOwner.posMng.mActiveAction.Idx == 485))//乾坤刀绝招?
            //    Debug.DebugBreak();
            if (inputs[i].OnKeyDown(keyStatus))
            {
                return true;
            }
        }
        return false;
    }

    public void OnKeyPressing(KeyState keyStatus) {
        for (int i = inputs.Count - 1; i >= 0; i--) {
            inputs[i].OnKeyPressing(keyStatus);
        }
    }

    public void RemovePressing(EKeyList key) {
        for (int i = 0; i < inputs.Count; i++) {
            if (inputs[i].Pressing == key) {
                inputs[i].Reset(); 
            }
        }
    }
}