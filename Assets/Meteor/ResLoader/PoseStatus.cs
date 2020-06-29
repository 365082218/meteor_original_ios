using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Diagnostics;
//管理角色的动画帧，用自己的方式实现动画
[Serializable]
public class PoseStatus
{
    public static Dictionary<int, List<Pose>> ActionList = new Dictionary<int, List<Pose>>();
    public static bool ActionExist(int unit, int action)
    {
        if (ActionList[unit] != null && ActionList[unit].Count > action && action >= 0)
            return true;
        return false;
    }
    //1一定不要重力，因为招式向上 2一定要重力，因为招式向下，3如果在空中，忽略重力
    //public static Dictionary<int, int> IgnoreGravity = new Dictionary<int, int>();
    public Pose mActiveAction = null;
    public static void Clear()
    {
        ActionList.Clear();
    }
    MeteorUnit _Self;
    public static bool IsReadyAction(int idx)
    {
        if (idx >= CommonAction.DartReady && idx <= CommonAction.HammerReady)
            return true;
        if (idx >= CommonAction.GloveReady && idx <= CommonAction.RendaoReady)
            return true;
        return false;
    }
    int UnitId;
    public int mActiveActionIdx;
    public bool CanMove;
    public bool CanControl;
    public bool CanRotateY
    {
        get
        {
            //如果被锁定
            if (_Self.IsDebugUnit())
                return false;
            if (_Self.controller.InputLocked)
                return false;
            //如果有锁定目标，不许转向(在有锁系统下)
            if (_Self.LockTarget != null && Main.Ins.GameStateMgr.gameStatus.AutoLock)
                return false;
            //攻击动作播放时不许摇杆控制角色转向
            if (IsAttackPose())
                return false;
            //爬墙不许转向
            if (_Self.Climbing)
                return false;
            //等待收回飞轮不许转向
            if (mActiveAction.Idx == CommonAction.WaitWeaponReturn)
                return false;
            //防御不许转向
            if (mActiveAction.Idx >= CommonAction.DartDefence && mActiveAction.Idx <= CommonAction.HammerDefence)
                return false;
            if (mActiveAction.Idx >= CommonAction.ZhihuDefence && mActiveAction.Idx <= CommonAction.RendaoDefence)
                return false;
            //如果是一些特殊动作，比如前滚后滚，爆气，换武器，嘲讽，救人，受击中不许切换转向
            if (mActiveAction.Idx >= CommonAction.DForw1 && mActiveAction.Idx <= CommonAction.DCBack)
                return false;
            if (mActiveAction.Idx >= CommonAction.DForw4 && mActiveAction.Idx <= CommonAction.DBack6)
                return false;
            if (mActiveAction.Idx == CommonAction.Struggle || mActiveAction.Idx == CommonAction.Dead)
                return false;
            if (mActiveAction.Idx == CommonAction.BreakOut || mActiveAction.Idx == CommonAction.ChangeWeapon ||
                mActiveAction.Idx == CommonAction.Taunt || mActiveAction.Idx == CommonAction.AirChangeWeapon)
                return false;
            if (mActiveAction.Idx >= CommonAction.DefenceHitStart && mActiveAction.Idx <= CommonAction.DefenceHitEnd)
                return false;
            if (mActiveAction.Idx >= CommonAction.HitStart && mActiveAction.Idx <= CommonAction.HitEnd)
                return false;
            if (mActiveAction.Idx == CommonAction.HammerMaxFall)
                return false;
            return true;
        }
    }
    
    //public bool CanJump;
    public bool CanChangeWeapon;
    public bool CanDefence
    {
        get
        {
            if (onDefence)
                return false;
            if (onhurt)
                return false;
            if (IsAttackPose())
                return false;
            //只有蹲下，空闲，移动，可以切换为防御
            if (mActiveAction.Idx >= CommonAction.DForw1 && mActiveAction.Idx <= CommonAction.DCBack)
                return false;
            if (mActiveAction.Idx >= CommonAction.DForw4 && mActiveAction.Idx <= CommonAction.DBack6)
                return false;
            if (mActiveAction.Idx == CommonAction.Struggle || mActiveAction.Idx == CommonAction.Dead || mActiveAction.Idx == CommonAction.Struggle0)
                return false;
            if (mActiveAction.Idx == CommonAction.BreakOut || mActiveAction.Idx == CommonAction.ChangeWeapon ||
                mActiveAction.Idx == CommonAction.Taunt || mActiveAction.Idx == CommonAction.AirChangeWeapon)
                return false;
            if (mActiveAction.Idx >= CommonAction.Jump && mActiveAction.Idx <= CommonAction.JumpFallOnGround)
                return false;
            return true;
        }
    }
    public bool CanSkill
    {
        get
        {
            return CanDefence;
        }
    }
    public bool Rotateing = false;
    public bool onDefence
    {
        get
        {
            if (mActiveAction.Idx >= CommonAction.DartDefence && mActiveAction.Idx <= CommonAction.HammerDefence)
                return true;
            if (mActiveAction.Idx >= CommonAction.ZhihuDefence && mActiveAction.Idx <= CommonAction.RendaoDefence)
                return true;
            if (mActiveAction.Idx >= CommonAction.DefenceHitStart && mActiveAction.Idx <= CommonAction.DefenceHitEnd)
                return true;
            if (mActiveAction.Idx >= 490 && mActiveAction.Idx <= 500)//指虎乾坤防御
                return true;
            if (mActiveAction.Idx >= 513 && mActiveAction.Idx <= 519)//忍刀防御
                return true;
            return false;
        }
    }

    public bool onhurt
    {
        get
        {
            if (mActiveAction == null)
                return false;
            if (mActiveAction.Idx >= CommonAction.HitStart && mActiveAction.Idx <= CommonAction.HitEnd)
                return true;
            return false;
        }
    }
    public float ClimbFallTick { get; set; }
    public const float ClimbFallLimit = 0.5f;
    public bool CanAdjust { get; set; }
    public bool CheckClimb { get; set; }//检查轻功开始，在按住上键和跳跃后，上键一旦松开就不再检查轻工
    public float JumpTick = 0.2f;//0.2f内算为跳.
    public bool Jump
    {
        get
        {
            if (mActiveAction.Idx == CommonAction.Jump || 
                mActiveAction.Idx == CommonAction.JumpLeft ||
                mActiveAction.Idx == CommonAction.JumpRight ||
                mActiveAction.Idx == CommonAction.JumpBack ||
                mActiveAction.Idx == CommonAction.WallLeftJump ||
                mActiveAction.Idx == CommonAction.WallRightJump)
                return true;
            return false;
        }
    }

    //加载全部，在游戏过程中尽量不要发生IO
    public static void LoadAll()
    {
        for (int i = 0; i < 20; i++)
        {
            if (!ActionList.ContainsKey(i))
            {
                ActionList.Add(i, new List<Pose>());
                TextAsset asset = Resources.Load<TextAsset>(string.Format("{0}/P{1}.pos", Main.Ins.AppInfo.MeteorVersion, i));
                string text = System.Text.Encoding.ASCII.GetString(asset.bytes);
                Parse(text, i);
            }
        }
    }

    public void Init(MeteorUnit owner)
    {
        _Self = owner;
        CheckClimb = false;
        CanAdjust = true;
        playResultAction = false;
        ClimbFallTick = 0.0f;
        load = owner.charLoader;
        UnitId = _Self == null ? 0 : _Self.UnitId;
        CanMove = true;
        if (!ActionList.ContainsKey(UnitId))
        {
            //int TargetIdx = UnitId >= 20 ? 0 : UnitId;
            if (UnitId >= 20)
            {
                //先找到插件里是否包含此Pos文件
                ModelItem m = DlcMng.GetPluginModel(UnitId);
                if (m != null && m.Installed)
                {
                    for (int i = 0; i < m.resPath.Length; i++)
                    {
                        if (m.resPath[i].ToLower().EndsWith(".pos"))
                        {
                            ActionList.Add(UnitId, new List<Pose>());
                            string text = System.IO.File.ReadAllText(m.resPath[i]);
                            Parse(text, UnitId);
                            return;
                        }
                    }
                    if (m.useFemalePos)
                        ActionList.Add(UnitId, ActionList[1]);
                    else
                        ActionList.Add(UnitId, ActionList[0]);
                }
                
            }
            else
            {
                ActionList.Add(UnitId, new List<Pose>());
                TextAsset asset = Resources.Load<TextAsset>(string.Format("{0}/P{1}.pos", Main.Ins.AppInfo.MeteorVersion, UnitId));
                string text = System.Text.Encoding.ASCII.GetString(asset.bytes);
                Parse(text, UnitId);
            }
        }
    }

    public void StopAction()
    {
        if (load != null)
            load.SetPosData(null);
    }

    public Dictionary<int, int> LinkInput = new Dictionary<int, int>();

    public static bool IgnoreActionMove(int idx)
    {
        ActionDatas.ActionDatas act = Main.Ins.DataMgr.GetData<ActionDatas.ActionDatas>(idx);
        if (act == null)
            return false;
        return act.IgnoreMove;
    }

    public static bool IgnoreXZMove(int idx)
    {
        ActionDatas.ActionDatas act = Main.Ins.DataMgr.GetData<ActionDatas.ActionDatas>(idx);
        if (act == null)
            return false;
        return act.IgnoreXZMove;
    }

    public static bool IgnoreVelocityXZ(int idx)
    {
        ActionDatas.ActionDatas act = Main.Ins.DataMgr.GetData<ActionDatas.ActionDatas>(idx);
        if (act == null)
            return false;
        return act.IgnoreXZVelocity;
    }

    public static bool IgnorePhysical(int idx)
    {
        ActionDatas.ActionDatas act = Main.Ins.DataMgr.GetData<ActionDatas.ActionDatas>(idx);
        if (act == null)
            return false;
        return act.IgnoreCollision;
    }

    public static bool IgnoreGravity(int idx)
    {
        ActionDatas.ActionDatas act = Main.Ins.DataMgr.GetData<ActionDatas.ActionDatas>(idx);
        if (act == null)
        {
            return false;
        }
        return act.IgnoreGravity;
    }

    public bool IsHurtPose()
    {
        return (mActiveAction.Idx >= CommonAction.HitStart && mActiveAction.Idx <= CommonAction.HitEnd);
    }

    //受击或者被击，都无法转变X轴视角，在没有锁定目标状态下才能转变Y视角.
    public bool IsAttackPose()
    {
        return !(mActiveAction.Attack == null || mActiveAction.Attack.Count == 0);
    }

    public bool IsAttackPose(int i)
    {
        if (i < 0 || i > 600)
            return false;
        Pose p = ActionList[UnitId][i];
        return !(p.Attack == null || p.Attack.Count == 0);
    }

    event Action OnActionFinishedEvt;
    public void LinkEvent(Action evt)
    {
        OnActionFinishedEvt += evt;
    }

    public void ClearEvent()
    {
        OnActionFinishedEvt = null;
    }

    //这个动作做完后，链接到其他动作上.
    public void LinkAction(int idx)
    {
        //先把虚拟动作转换为实际动作ID
        if (idx >= (int)600)
        {
            switch (idx)
            {
                case (int)600:idx = 0;break;
            }
        }

        if (mActiveAction != null)
        {
            PosAction act = null;
            int curIndex = load.GetCurrentFrameIndex();
            for (int i = 0; i < mActiveAction.ActionList.Count; i++)
            {
                if (mActiveAction.ActionList[i].Type.Equals("Blend"))
                {
                    act = mActiveAction.ActionList[i];
                    //在可切换帧范围内
                    if (curIndex >= act.Start && curIndex <= act.End)
                    {
                        //如果角色在空中，那么出招会凝滞重力
                        if (!_Self.IsOnGround())
                            _Self.ResetYVelocity();
                        //_Self.IgnoreGravitys(IgnoreGravity(idx));
                        //Debug.LogError("link action");
                        if (mActiveAction.Next != null)
                        {
                            //Debug.LogError("直接切换动作：" + idx + " NextPose:" + mActiveAction.Next.Time);
                            ChangeAction(idx, mActiveAction.Next.Time);
                        }
                        else
                            ChangeAction(idx);
                        return;
                    }
                    else if (curIndex < act.Start)
                    {
                        LinkInput[mActiveAction.Idx] = idx;
                        //Debug.LogError("等待混合动作：" + idx);
                        return;
                    }
                }
            }
            //如果动作无尾部融合之类的，就立即切换动作吧,表明这个动作一开始跑,任意时候都接受输入并转换状态.
            //如果角色在空中或者招式有向上的移动，那么出招会凝滞重力
            //在空中出任意招式，向上的跳跃能力将会被清空
            if (!_Self.IsOnGround())
                _Self.ResetYVelocity();
            //某些时候，XZ轴速度也会发生改变。这个部分比较细致，
            //_Self.IgnoreGravitys(IgnoreGravity(idx));
            //MeteorManager.Instance.PhysicalIgnore(_Self, IgnorePhysical(idx));
            //else if (!_Self.IsOnGround())
            //    _Self.EnableGravity(false);
            if (IsAttackPose())
            {
                LinkInput[mActiveAction.Idx] = idx;
                //Debug.LogError("等待结束后连接动作：" + idx);
            }
            else
            {
                //Debug.LogError("link action 2");
                if (mActiveAction.Next != null)
                    ChangeAction(idx, mActiveAction.Next.Time);
                else
                    ChangeAction(idx);
            }
        }
    }

    //动作播放完毕，切换下一个可连接动作.
    public bool playResultAction = false;
    public System.Action OnDebugActionFinished;
    public void OnActionFinished()
    {
        if (OnDebugActionFinished != null)
            OnDebugActionFinished();
        if (waitPause)
        {
            //死亡后接事件
            if (OnActionFinishedEvt != null)
            {
                OnActionFinishedEvt();//关闭碰撞盒
                OnActionFinishedEvt = null;
            }
        }
        else
        {
            if (mActiveAction.Idx == CommonAction.Struggle0 || mActiveAction.Idx == CommonAction.Struggle)
                return;

            //使用火枪，状态机与普通状态机不一致
            if (_Self.GetWeaponType() == (int)EquipWeaponType.Gun)
            {
                //212=>213
                if (mActiveAction.Idx == CommonAction.GunReload)
                {
                    ChangeAction(CommonAction.GunIdle, 0.1f);
                }
                else
                {
                    if (_Self.IsOnGround())
                    {
                        //213=>213
                        if (mActiveAction.Idx == CommonAction.GunIdle)
                        {
                            ChangeAction(CommonAction.GunIdle);
                        }
                        else
                        {
                            if (LinkInput.ContainsKey(mActiveAction.Idx))
                            {
                                //拿着火枪在空中踢人.
                                //int TargetActionIdx = mActiveAction.Idx;
                                if (mActiveAction.Next != null)
                                    ChangeAction(LinkInput[mActiveAction.Idx], mActiveAction.Next.Time);//
                                else
                                    ChangeAction(LinkInput[mActiveAction.Idx], 0.1f);//ok
                                LinkInput.Clear();
                            }
                            else
                            if (mActiveAction.Link != 0)
                            {
                                if (mActiveAction.Next != null)
                                    ChangeAction(mActiveAction.Link, mActiveAction.Next.Time);
                                else
                                    ChangeAction(mActiveAction.Link, 0.1f);
                            }
                            else
                            {
                                if (!_Self.GunReady)
                                {
                                    if (mActiveAction.Next != null)
                                        ChangeAction(CommonAction.Idle, mActiveAction.Next.Time);
                                    else
                                        ChangeAction(CommonAction.Idle, 0.1f);
                                }
                                else
                                {
                                    //没有重装子弹的进入213
                                    if (mActiveAction.Next != null)
                                        ChangeAction(CommonAction.GunIdle, mActiveAction.Next.Time);
                                    else
                                        ChangeAction(CommonAction.GunIdle, 0.1f);
                                }
                            }
                        }
                    }
                    else if (_Self.posMng.onhurt)
                    {
                        //浮空受击.
                    }
                    else
                        ChangeAction(CommonAction.JumpFall);//拿着枪从空中落下.
                }
            }
            else
            {
                if (LinkInput.ContainsKey(mActiveAction.Idx))
                {
                    //int TargetActionIdx = mActiveAction.Idx;
                    if (mActiveAction.Next != null)
                        ChangeAction(LinkInput[mActiveAction.Idx], mActiveAction.Next.Time);//
                    else
                        ChangeAction(LinkInput[mActiveAction.Idx], 0.1f);//ok
                    LinkInput.Clear();
                }
                else
                {
                    if (mActiveAction.Link != 0)
                    {
                        if (mActiveAction.Next != null)
                            ChangeAction(mActiveAction.Link, mActiveAction.Next.Time);
                        else
                            ChangeAction(mActiveAction.Link, 0.01f);//给一个微弱得过渡时间,因为一些动作过渡需要把d_base得偏移对上去(针对152接180导致得角色轻微位移)
                    }
                    else
                    {
                        if (_Self.IsOnGround())
                        {
                            //如果处于防御-受击状态中,恢复为防御pose
                            if (onDefence)
                            {
                                _Self.Defence();
                            }
                            else
                            if (_Self.LockTarget != null && Main.Ins.GameStateMgr.gameStatus.AutoLock)
                            {
                                int ReadyAction = 0;
                                switch ((EquipWeaponType)_Self.GetWeaponType())
                                {
                                    case EquipWeaponType.Knife: ReadyAction = CommonAction.KnifeReady; break;
                                    case EquipWeaponType.Sword: ReadyAction = CommonAction.SwordReady; break;
                                    case EquipWeaponType.Blade: ReadyAction = CommonAction.BladeReady; break;
                                    case EquipWeaponType.Lance: ReadyAction = CommonAction.LanceReady; break;
                                    case EquipWeaponType.Brahchthrust: ReadyAction = CommonAction.BrahchthrustReady; break;
                                    case EquipWeaponType.Dart: ReadyAction = CommonAction.DartReady; break;
                                    case EquipWeaponType.Gloves: ReadyAction = CommonAction.GloveReady; break;
                                    case EquipWeaponType.Guillotines: ReadyAction = CommonAction.GuillotinesReady; break;
                                    case EquipWeaponType.Hammer: ReadyAction = CommonAction.HammerReady; break;
                                    case EquipWeaponType.NinjaSword: ReadyAction = CommonAction.RendaoReady; break;
                                    case EquipWeaponType.HeavenLance:
                                        switch (_Self.GetWeaponSubType())
                                        {
                                            case 0: ReadyAction = CommonAction.QK_BADAO_READY; break;
                                            case 1: ReadyAction = CommonAction.QK_CHIQIANG_READY; break;
                                            case 2: ReadyAction = CommonAction.QK_JUHE_READY; break;
                                        }
                                        break;
                                    case EquipWeaponType.Gun: ReadyAction = CommonAction.GunReady; break;
                                }
                                if (mActiveAction.Next != null)
                                    ChangeAction(ReadyAction, mActiveAction.Next.Time);
                                else
                                    ChangeAction(ReadyAction, 0.1f);
                            }
                            else
                            {
                                if (mActiveAction.Next != null)
                                    ChangeAction(CommonAction.Idle, mActiveAction.Next.Time);
                                else
                                    ChangeAction(CommonAction.Idle, 0.1f);
                            }
                        }
                        else if (_Self.posMng.onhurt)
                        {
                            //Debug.Log("目标受到伤害浮空");
                        }
                        else
                            ChangeAction(CommonAction.JumpFall);
                    }
                }
            }
        }
    }

    bool waitPause = false;
    public void WaitPause(bool wait = true)
    {
        waitPause = wait;//等待
    }

    public void OnReborn()
    {
        CanControl = CanMove = true;
        ClearEvent();
    }

    public void OnDead()
    {
        CanControl = CanMove = false;
    }
    //根据动作号开始动画.
    public CharacterLoader AnimalCtrlEx { get { return load; } }
    CharacterLoader load;
    //动作在地面还是空中
    //动作是移动 防守 还是攻击 受伤 待机 
    public void ChangeActionSingle(int idx = CommonAction.Idle)
    {
        ChangeAction(idx, 0);
    }

    //被动情况下播放动画，类似，受到攻击，或者防御住对方的攻击
    public void OnChangeAction(int idx)
    {
        //如果是一些倒地动作，动作播放完之后还需要固定长时间才能起身
        ChangeAction(idx, 0.1f);
    }

    bool IsSkillStartPose(int pose)
    {
        if (pose == 244)//合太极起始
            return true;
        return false;
    }

    bool IsSkillEndPose(int pose)
    {
        if (pose == 364)//合太极收尾
            return true;
        return false;
    }

    public void ChangeAction(int idx = CommonAction.Idle, float time = 0.01f)
    {
        //if (idx == 151 || idx == 152)
        //{
        //    string stackInfo = new StackTrace().ToString();
        //    UnityEngine.Debug.Log(stackInfo);
        //}
        CanAdjust = false;
        if (_Self.GameFinished && !playResultAction && (idx == CommonAction.Idle || idx == CommonAction.GunIdle))
        {
            playResultAction = true;
            if (_Self.Camp == EUnitCamp.EUC_ENEMY && Main.Ins.GameBattleEx.BattleLose())
            {
                ChangeAction(CommonAction.Taunt, 0.1f);
                return;
            }
            else if (_Self.Camp == EUnitCamp.EUC_FRIEND && Main.Ins.GameBattleEx.BattleWin())
            {
                ChangeAction(CommonAction.Taunt, 0.1f);
                return;
            }
        }

        _Self.IgnoreGravitys(PoseStatus.IgnoreGravity(idx));//设置招式重力
        bool ignorePhy = IgnorePhysical(idx);
        if (ignorePhy != _Self.IgnorePhysical)
            Main.Ins.MeteorManager.PhysicalIgnore(_Self, ignorePhy);//设置招式是否忽略角色障碍

        //看是否是大绝的起始招式/结束招式，大绝起始和结束招式之间的招式，不许响应输入切换招式.大绝不可取消.
        if (IsSkillStartPose(idx) && !_Self.IsPlaySkill)
            _Self.IsPlaySkill = true;
        else if ((IsSkillEndPose(idx) || onhurt) && _Self.IsPlaySkill)
            _Self.IsPlaySkill = false;

        //设置招式是否冻结世界轴XZ上得速度.
        _Self.ResetWorldVelocity(IgnoreVelocityXZ(idx));
        if (load != null)
        {
            int weapon = _Self.GetWeaponType();
            CanMove = false;
            if (idx == CommonAction.Defence)
            {
                switch ((EquipWeaponType)weapon)
                {
                    case EquipWeaponType.Knife: idx = CommonAction.KnifeDefence; break;
                    case EquipWeaponType.Sword: idx = CommonAction.SwordDefence; break;
                    case EquipWeaponType.Blade: idx = CommonAction.BladeDefence; break;
                    case EquipWeaponType.Lance: idx = CommonAction.LanceDefence; break;
                    case EquipWeaponType.Brahchthrust: idx = CommonAction.BrahchthrustDefence; break;
                    //case EquipWeaponType.Dart: idx = CommonAction.DartDefence;break;
                    case EquipWeaponType.Gloves: idx = CommonAction.ZhihuDefence; break;//没找到
                    //case EquipWeaponType.Guillotines: idx = CommonAction.GuillotinesDefence;break;
                    case EquipWeaponType.Hammer: idx = CommonAction.HammerDefence; break;
                    case EquipWeaponType.NinjaSword: idx = CommonAction.RendaoDefence; break;
                    case EquipWeaponType.HeavenLance: idx = CommonAction.QiankunDefenct; break;//
                                                                                               //case EquipWeaponType.Gun: return;//
                }
            }
            else
            if (idx == CommonAction.Jump)
            {
                CanAdjust = true;//跳跃后，可以微量移动
                CanMove = true;
            }
            else if ((idx >= CommonAction.WalkForward && idx <= CommonAction.RunOnDrug) || idx == CommonAction.Crouch)
            {
                CanMove = true;
            }
            else if (idx == CommonAction.Idle)
            {
                CanMove = true;
            }

            //蹲下左右旋转-蹲下前后左右移动，只要之前处于火枪预备则都可以瞬间出火枪的攻击
            if (weapon == (int)EquipWeaponType.Gun)
            {
                if ((idx >= CommonAction.CrouchForw && idx <= CommonAction.CrouchBack) || idx == CommonAction.GunIdle)
                {
                    if (idx == CommonAction.GunIdle && !_Self.GunReady)
                        _Self.SetGunReady(true);
                    if (_Self.Attr.IsPlayer)
                    {
                        if (_Self.GunReady)
                        {
                            Main.Ins.EnterState(Main.Ins.GunShootDialogStatus);
                        }
                        else
                        {
                            Main.Ins.ExitState(Main.Ins.GunShootDialogStatus);
                        }
                    }
                }
                else
                {
                    if (_Self.Attr.IsPlayer)
                    {
                        Main.Ins.ExitState(Main.Ins.GunShootDialogStatus);
                    }
                }
            }
            else if (_Self.Attr.IsPlayer)
            {
                Main.Ins.ExitState(Main.Ins.GunShootDialogStatus);
            }

            //除了受击，防御，其他动作在有锁定目标下，都要转向锁定目标.
            if (_Self.LockTarget != null && !onDefence && !onhurt)
            {
                //是否旋转面向目标.
                if (_Self.StateMachine != null && _Self.StateMachine.IsFighting())
                {
                    //NPC.
                    //远程武器无需转向.
                    if (_Self.GetWeaponType() != (int)EquipWeaponType.Guillotines &&
                        _Self.GetWeaponType() != (int)EquipWeaponType.Gun &&
                        _Self.GetWeaponType() != (int)EquipWeaponType.Dart)
                    {
                        _Self.FaceToTarget(_Self.LockTarget);
                    }
                }
                else if (_Self.StateMachine == null && Main.Ins.GameStateMgr.gameStatus.AutoLock && idx != CommonAction.Idle)
                {
                    //主角.
                    if (_Self.GetWeaponType() != (int)EquipWeaponType.Guillotines &&
                        _Self.GetWeaponType() != (int)EquipWeaponType.Gun &&
                        _Self.GetWeaponType() != (int)EquipWeaponType.Dart)
                        _Self.FaceToTarget(_Self.LockTarget);
                }
            }
            load.SetPosData(ActionList[UnitId][idx], time);
            mActiveAction = ActionList[UnitId][idx];
            mActiveActionIdx = mActiveAction.Idx;
            LinkInput.Clear();
        }
    }

    static void Parse(string text, int id)
    {
        if (ActionList.ContainsKey(id) && ActionList[id].Count != 0)
        {
            UnityEngine.Debug.LogError("重复解析某个角色的动画配置文件");
            return;
        }
        Pose current = null;
        PosAction curAct = null;
        AttackDes att = null;
        DragDes dra = null;
        NextPose nex = null;
        int left = 0;
        int leftAct = 0;
        int leftAtt = 0;
        int leftDra = 0;
        int leftNex = 0;
        string[] pos = text.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < pos.Length; i++)
        {
            string line = pos[i];
            string[] lineObject = line.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (lineObject.Length == 0)
            {
                //Debug.Log("line i:" + i);
                //空行跳过
                continue;
            }
            else if (lineObject[0].StartsWith("#"))
                continue;
            else
            if (lineObject[0] == "Pose" && left == 0 && leftAct == 0)
            {
                Pose insert = new Pose();
                ActionList[id].Add(insert);
                int idx = int.Parse(lineObject[1]);
                insert.Idx = idx;
                current = insert;
            }
            else if (lineObject[0] == "{")
            {
                if (nex != null)
                    leftNex++;
                else
                if (dra != null)
                    leftDra++;
                else
                if (att != null)
                {
                    leftAtt++;
                }
                else
                    if (curAct != null)
                    leftAct++;
                else
                    left++;
            }
            else if (lineObject[0] == "}")
            {
                if (nex != null)
                {
                    leftNex--;
                    if (leftNex == 0)
                        nex = null;
                }
                else
                if (dra != null)
                {
                    leftDra--;
                    if (leftDra == 0)
                        dra = null;
                }
                else
                if (att != null)
                {
                    leftAtt--;
                    if (leftAtt == 0)
                        att = null;
                }
                else
                if (curAct != null)
                {
                    leftAct--;
                    if (leftAct == 0)
                        curAct = null;
                }
                else
                {
                    left--;
                    if (left == 0)
                        current = null;
                }

            }
            else if (lineObject[0] == "link" || lineObject[0] == "Link" || lineObject[0] == "Link\t" || lineObject[0] == "link\t")
            {
                current.Link = int.Parse(lineObject[1]);
            }
            else if (lineObject[0] == "source" || lineObject[0] == "Source")
            {
                current.SourceIdx = int.Parse(lineObject[1]);
            }
            else if (lineObject[0] == "Start" || lineObject[0] == "start")
            {
                if (nex != null)
                {
                    nex.Start = int.Parse(lineObject[1]);
                }
                else
                if (dra != null)
                {
                    dra.Start = int.Parse(lineObject[1]);
                }
                else
                if (att != null)
                {
                    att.Start = int.Parse(lineObject[1]);
                }
                else
                if (curAct != null)
                    curAct.Start = int.Parse(lineObject[1]);
                else
                    current.Start = int.Parse(lineObject[1]);
            }
            else if (lineObject[0] == "End" || lineObject[0] == "end")
            {
                if (nex != null)
                {
                    nex.End = int.Parse(lineObject[1]);
                }
                else
                if (dra != null)
                {
                    dra.End = int.Parse(lineObject[1]);
                }
                else
                if (att != null)
                {
                    att.End = int.Parse(lineObject[1]);
                }
                else
                if (curAct != null)
                    curAct.End = int.Parse(lineObject[1]);
                else
                    current.End = int.Parse(lineObject[1]);
            }
            else if (lineObject[0] == "Speed" || lineObject[0] == "speed")
            {
                if (curAct != null)
                    curAct.Speed = float.Parse(lineObject[1]);
            }
            else if (lineObject[0] == "LoopStart")
            {
                current.LoopStart = int.Parse(lineObject[1]);
            }
            else if (lineObject[0] == "LoopEnd")
            {
                current.LoopEnd = int.Parse(lineObject[1]);
            }
            else if (lineObject[0] == "EffectType")
            {
                current.EffectType = int.Parse(lineObject[1]);
            }
            else if (lineObject[0] == "EffectID")
            {
                current.EffectID = lineObject[1];
            }
            else if (lineObject[0] == "Blend")
            {
                PosAction act = new PosAction();
                act.Type = "Blend";
                current.ActionList.Add(act);
                curAct = act;
            }
            else if (lineObject[0] == "Action")
            {
                PosAction act = new PosAction();
                act.Type = "Action";
                current.ActionList.Add(act);
                curAct = act;
            }
            else if (lineObject[0] == "Attack")
            {
                att = new AttackDes();
                att.PoseIdx = current.Idx;
                current.Attack.Add(att);
            }
            else if (lineObject[0] == "bone")
            {
                //重新分割，=号分割，右边的,号分割
                lineObject = line.Split(new char[] { '=' }, System.StringSplitOptions.RemoveEmptyEntries);
                string bones = lineObject[1];
                while (bones.EndsWith(","))
                {
                    i++;
                    lineObject = new string[1];
                    lineObject[0] = pos[i];
                    bones += lineObject[0];
                }
                //bones = bones.Replace(' ', '_');
                string[] bonesstr = bones.Split(new char[] { ',' });
                for (int j = 0; j < bonesstr.Length; j++)
                {
                    string b = bonesstr[j].TrimStart(new char[] { ' ', '\"' });
                    b = b.TrimEnd(new char[] { '\"', ' ' });
                    b = b.Replace(' ', '_');
                    att.bones.Add(b);
                }
            }
            else if (lineObject[0] == "AttackType")
            {
                att._AttackType = int.Parse(lineObject[1]);
            }
            else if (lineObject[0] == "CheckFriend")
            {
                att.CheckFriend = int.Parse(lineObject[1]);
            }
            else if (lineObject[0] == "DefenseValue")
            {
                att.DefenseValue = float.Parse(lineObject[1]);
            }
            else if (lineObject[0] == "DefenseMove")
            {
                att.DefenseMove = float.Parse(lineObject[1]);
            }
            else if (lineObject[0] == "TargetValue")
            {
                att.TargetValue = float.Parse(lineObject[1]);
            }
            else if (lineObject[0] == "TargetMove")
            {
                att.TargetMove = float.Parse(lineObject[1]);
            }
            else if (lineObject[0] == "TargetPose")
            {
                att.TargetPose = int.Parse(lineObject[1]);
            }
            else if (lineObject[0] == "TargetPoseFront")
            {
                att.TargetPoseFront = int.Parse(lineObject[1]);
            }
            else if (lineObject[0] == "TargetPoseBack")
            {
                att.TargetPoseBack = int.Parse(lineObject[1]);
            }
            else if (lineObject[0] == "TargetPoseLeft")
            {
                att.TargetPoseLeft = int.Parse(lineObject[1]);
            }
            else if (lineObject[0] == "TargetPoseRight")
            {
                att.TargetPoseRight = int.Parse(lineObject[1]);
            }
            else if (lineObject[0] == "Drag")
            {
                dra = new DragDes();
                current.Drag = dra;
            }
            else if (lineObject[0] == "Time")
            {
                if (nex != null)
                    nex.Time = float.Parse(lineObject[1]);
                else
                    dra.Time = float.Parse(lineObject[1]);
            }
            else if (lineObject[0] == "Color")
            {
                string[] rgb = lineObject[1].Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                dra.Color.x = int.Parse(rgb[0]);
                dra.Color.y = int.Parse(rgb[1]);
                dra.Color.z = int.Parse(rgb[2]);
            }
            else if (lineObject[0] == "NextPose")
            {
                current.Next = new NextPose();
                nex = current.Next;
            }
            else if (lineObject[0] == "{}")
            {
                current = null;
                continue;
            }
            else
            {
                UnityEngine.Debug.Log("line :" + i + " can t understand：" + pos[i]);
                break;
            }
        }
    }
}

public class Pose
{
    public static int FPS;
    public int Idx;
    public int SourceIdx;
    public int Start;
    public int End;
    public int LoopStart;
    public int LoopEnd;
    public int EffectType;//这个链接到特效事件表里.
    public string EffectID;
    public List<PosAction> ActionList = new List<PosAction>();
    public int Link;
    public List<AttackDes> Attack = new List<AttackDes>();
    public DragDes Drag;
    public NextPose Next;
}

//[System.Serializable]
public class AttackDes
{
    public List<string> bones = new List<string>();//攻击伤害盒
    public int PoseIdx;//伤害由哪个动作赋予，由动作可以反向查找技能，以此算伤害
    public int Start;
    public int End;
    public int _AttackType;//0普攻1破防
    public int CheckFriend;
    public float DefenseValue;//防御僵硬
    public float DefenseMove;//防御时移动.
    public float TargetValue;//攻击僵硬
    public float TargetMove;//攻击时移动
    public int TargetPose;//受击时播放动作
    public int TargetPoseFront;  //挨打倒地096
    public int TargetPoseBack;//倒地前翻   099
    public int TargetPoseLeft;//倒地右翻   098
    public int TargetPoseRight;//倒地左翻  097
}

//[System.Serializable]
public class DragDes
{
    public int Start;
    public int End;
    public float Time;
    public Vector3 Color;
}

//[System.Serializable]
public class NextPose
{
    public int Start;
    public int End;
    public float Time;
}

//攻击动作，即哪几帧使用攻击盒算伤害.
/*
 * Pose 572     #荡~臂锣ぇ~）９＋\辽~><~
{
  source     0
  Start      16621
  End        16732
  EffectType 0
  EffectID   Pos572
  link       573
  Blend
  {
    Start 16621
    End   16641
    Speed 1.5
  }
  Action
  {
    start 16642
    end   16690
    speed 1.5
  }
  Action
  {
    start 16691
    end   16725
    speed 1
  }
  Blend
  {
    start 16726
    end   16726
    speed 1
  }
  Action
  {
    start 16727
    end   16732
    speed 1
  }
  Attack
  {
    bone = "weapon","effect","bau Spine1","bau Spine",
           "bau R UpperArm","bau R Forearm","bau R Hand",
           "bau L UpperArm","bau L Forearm","bau L Hand",
           "bad R Thigh","bad R Calf","bad R Foot",
           "bad L Thigh","bad L Calf","bad L Foot"
    Start       16693
    End         16724
    AttackType  0
    CheckFriend 0
    DefenseValue     0.3
    DefenseMove      5
    TargetValue      0.3
    TargetMove       5
    TargetPose       098   
    TargetPoseFront  098
    TargetPoseBack   097
    TargetPoseLeft   099
    TargetPoseRight  095
  }
  Drag
  {
    Start  16685
    End    16732
    Time   0.1
    Color  255,255,198
  }
  NextPose
  {
    Start 16643
    End   16731
    Time  0.1
  }
  }
 */
[System.Serializable]
public class PosAction
{
    public string Type;//"Blend/Action"
    public int Start;
    public int End;
    public float Speed;//出招速度.

}
