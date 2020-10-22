using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Diagnostics;
using Excel2Json;
using Idevgame.Util;
using System.IO;
using protocol;

public class QueueParam {
    public QueueParam(int a, float f) {
        action = a;
        fade = f;
    }
    public int action;
    public float fade;
}

//管理角色的动画帧，用自己的方式实现动画
[Serializable]
public class ActionManager {
    public static SortedDictionary<int, List<Pose>> ActionList = new SortedDictionary<int, List<Pose>>();
    //加载全部，在游戏过程中尽量不要发生IO
    public static void LoadAll() {
        for (int i = 0; i < 20; i++) {
            if (!ActionList.ContainsKey(i)) {
                ActionList.Add(i, new List<Pose>());
                TextAsset asset = Resources.Load<TextAsset>(string.Format("{0}/P{1}.pos", Main.Ins.AppInfo.MeteorVersion, i));
                string text = System.Text.Encoding.ASCII.GetString(asset.bytes);
                Parse(text, i);
                //检查配置是否存在问题 pose 9内 loopStart比start还小
                //for (int j = 0; j < ActionList[i].Count; j++) {
                //    if (ActionList[i][j].LoopStart == 0 && ActionList[i][j].LoopEnd == 0)
                //        continue;
                //    if (ActionList[i][j].LoopStart < ActionList[i][j].Start || ActionList[i][j].LoopEnd > ActionList[i][j].End) {
                //        UnityEngine.Debug.LogError("file" + i + " pose:" + j + " contains error");
                //    }
                //}
            }
        }
    }

    public void SetAction(int action) {
        if (action != -1 && mActiveAction != null && action != mActiveAction.Idx)
            ChangeAction(action, 0.01f);
    }

    public static bool ActionExist(int unit, int action) {
        if (ActionList[unit] != null && ActionList[unit].Count > action && action >= 0)
            return true;
        return false;
    }

    public static void Clear() {
        ActionList.Clear();
    }

    public static bool IsJumpAction(int idx) {
        return idx >= CommonAction.Jump && idx <= CommonAction.WallLeftJump;
    }

    public static bool IsReadyAction(int idx) {
        if (idx >= CommonAction.DartReady && idx <= CommonAction.HammerReady)
            return true;
        if (idx >= CommonAction.GloveReady && idx <= CommonAction.RendaoReady)
            return true;
        return false;
    }

    public Pose mActiveAction = null;//正在播放的
    MeteorUnit mOwner;
    public Vector3 moveDelta;//上一帧的位移
    int UnitId;
    public bool AirAttacked { get; set; }//是否在空中发出招式.落地/切换到待机/准备时重置
    public int mActiveActionIdx;
    public bool CanControl;
    public bool CanRotateY {
        get {
            //如果被锁定
            if (mOwner.IsDebugUnit())
                return false;
            //如果空中发出过攻击招式
            if (!mOwner.IsOnGround() && AirAttacked) {
                return false;
            }
            if (mOwner.meteorController.InputLocked)
                return false;
            //如果有锁定目标，不许转向(在有锁系统下)
            if (mOwner.LockTarget != null && GameStateMgr.Ins.gameStatus.AutoLock)
                return false;
            //攻击动作播放时不许摇杆控制角色转向
            if (IsAttackPose())
                return false;
            //爬墙不许转向
            if (mOwner.Climbing)
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
    public bool CanDefence {
        get {
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
    public bool Rotateing = false;
    public bool onDefence {
        get {
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

    public bool onhurt {
        get {
            if (mActiveAction == null)
                return false;
            if (mActiveAction.Idx >= CommonAction.HitStart && mActiveAction.Idx <= CommonAction.HitEnd)
                return true;
            return false;
        }
    }
    public float ClimbFallTick { get; set; }//毫秒
    
    public bool CanAdjust { get; set; }
    public bool CheckClimb { get; set; }//检查轻功开始，在按住上键和跳跃后，上键一旦松开就不再检查轻工
    public float JumpTick = 0.2f;//0.2f内算为跳.
    public bool Jump {
        get {
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
    public void Init(MeteorUnit owner) {
        activeStatus.InitWith(30, 6);
        fadeOutStatus.InitWith(30, 6);
        mOwner = owner;
        CheckClimb = false;
        CanAdjust = true;
        playResultAction = false;
        ClimbFallTick = 0.0f;
        Characterloader = owner.characterLoader;
        UnitId = mOwner == null ? 0 : mOwner.ModelId;
        if (!ActionList.ContainsKey(UnitId)) {
            //int TargetIdx = UnitId >= 20 ? 0 : UnitId;
            //最先从安装的资料片里找-正在用的
            if (CombatData.Ins.Chapter != null) {
                SortedDictionary<int, string> models = CombatData.Ins.GScript.GetModel();
                if (models != null && models.ContainsKey(UnitId)) {
                    string path = CombatData.Ins.Chapter.GetResPath(FileExt.Pos, models[UnitId]);
                    if (!string.IsNullOrEmpty(path)) {
                        if (File.Exists(path)) {
                            ActionList.Add(UnitId, new List<Pose>());
                            string context = File.ReadAllText(path);
                            Parse(context, UnitId);
                            return;
                        }
                    }
                }
            }
            if (UnitId >= 20) {
                //先找到插件里是否包含此Pos文件
                ModelItem m = DlcMng.GetPluginModel(UnitId);
                if (m != null && m.Installed) {
                    for (int i = 0; i < m.resPath.Count; i++) {
                        if (m.resPath[i].ToLower().EndsWith(".pos")) {
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
            } else {
                ActionList.Add(UnitId, new List<Pose>());
                TextAsset asset = Resources.Load<TextAsset>(string.Format("{0}/P{1}.pos", Main.Ins.AppInfo.MeteorVersion, UnitId));
                string text = System.Text.Encoding.ASCII.GetString(asset.bytes);
                Parse(text, UnitId);
            }
        }
    }

    public void StopAction() {
        ChangeActionCore(null, 0);
    }

    public SortedDictionary<int, int> LinkInput = new SortedDictionary<int, int>();
    public SortedDictionary<int, int> QueuedAction = new SortedDictionary<int, int>();
    public Dictionary<int, QueueParam> QueuedAction2 = new Dictionary<int, QueueParam>();
    public static bool IgnoreActionMove(int idx) {
        ActionData act = DataMgr.Ins.GetActionData(idx);
        if (act == null)
            return false;
        //if (idx == 152)
        //    return true;
        //if (idx == 100)
        //    return false;
        return act.IgnoreMove == 1;
    }

    public static bool IgnoreVelocityXZ(int idx) {
        ActionData act = DataMgr.Ins.GetActionData(idx);
        if (act == null)
            return false;
        return act.IgnoreXZVelocity == 1;
    }

    public static bool IgnorePhysical(int idx) {
        ActionData act = DataMgr.Ins.GetActionData(idx);
        if (act == null)
            return false;
        return act.IgnoreCollision == 1;
    }

    public static bool IgnoreGravity(int idx) {
        ActionData act = DataMgr.Ins.GetActionData(idx);
        //if (idx == 151)
        //    return true;
        //if (idx == 100)
        //    return false;
        if (act == null) {
            return false;
        }
        return act.IgnoreGravity == 1;
    }

    public bool IsHurtPose() {
        return (mActiveAction.Idx >= CommonAction.HitStart && mActiveAction.Idx <= CommonAction.HitEnd);
    }

    //受击或者被击，都无法转变X轴视角，在没有锁定目标状态下才能转变Y视角.
    public bool IsAttackPose() {
        return !(mActiveAction.Attack == null || mActiveAction.Attack.Count == 0);
    }

    public bool IsAttackPose(int i) {
        if (i < 0 || i > 600)
            return false;
        Pose p = ActionList[UnitId][i];
        return !(p.Attack == null || p.Attack.Count == 0);
    }

    public void QueueAction2(int idx, int action, float crossfade) {
        if (!QueuedAction2.ContainsKey(idx)) {
            QueuedAction2.Add(idx, new QueueParam(action, crossfade));
        }
    }

    //在循环动作中添加一个链接动作，如果循环动作准备播放下一次了，那么先检查连接动作，有的话就切换动作.
    public void QueueAction(int idx, PoseEvt evt = PoseEvt.None) {
        if (!QueuedAction.ContainsValue(idx)) {
            QueuedAction.Add(mActiveAction.Idx, idx);
            if (evt != PoseEvt.None)
                LinkEvent(idx, evt);
        }
    }

    //这个动作做完后，链接到其他动作上.
    public void LinkAction(int idx) {
        //先把虚拟动作转换为实际动作ID
        if (idx >= (int)600) {
            switch (idx) {
                case (int)600: idx = 0; break;
            }
        }
        mOwner.UpdateNinjaState(NinjaState.None);
        if (mActiveAction != null) {
            PosAction act = null;
            for (int i = 0; i < mActiveAction.ActionList.Count; i++) {
                if (mActiveAction.ActionList[i].Type.Equals("Blend")) {
                    act = mActiveAction.ActionList[i];
                    //在可切换帧范围内
                    if (curIndex >= act.Start && curIndex <= act.End) {
                        //如果角色在空中，那么出招会凝滞重力
                        if (!mOwner.IsOnGround())
                            mOwner.ResetYVelocity();
                        //_Self.IgnoreGravitys(IgnoreGravity(idx));
                        //Debug.LogError("link action");
                        if (mActiveAction.Next != null) {
                            //Debug.LogError("直接切换动作：" + idx + " NextPose:" + mActiveAction.Next.Time);
                            ChangeAction(idx, mActiveAction.Next.Time);
                        } else
                            ChangeAction(idx, 0);
                        return;
                    } else if (curIndex < act.Start) {
                        LinkInput[mActiveAction.Idx] = idx;
                        //Debug.LogError("等待混合动作：" + idx);
                        return;
                    }
                }
            }
            //如果动作无尾部融合之类的，就立即切换动作吧,表明这个动作一开始跑,任意时候都接受输入并转换状态.
            //如果角色在空中或者招式有向上的移动，那么出招会凝滞重力
            //在空中出任意招式，向上的跳跃能力将会被清空
            if (!mOwner.IsOnGround())
                mOwner.ResetYVelocity();
            //某些时候，XZ轴速度也会发生改变。这个部分比较细致，
            //_Self.IgnoreGravitys(IgnoreGravity(idx));
            //MeteorManager.Instance.PhysicalIgnore(_Self, IgnorePhysical(idx));
            //else if (!_Self.IsOnGround())
            //    _Self.EnableGravity(false);
            if (IsAttackPose()) {
                LinkInput[mActiveAction.Idx] = idx;
                //Debug.LogError("等待结束后连接动作：" + idx);
            } else {
                //Debug.LogError("link action 2");
                if (mActiveAction.Next != null) {
                    QueueAction2(mActiveAction.Idx, idx, mActiveAction.Next.Time);
                } else
                    ChangeAction(idx, 0);
            }
        }
    }

    //动作播放完毕，切换下一个可连接动作.
    public bool playResultAction = false;
    //public System.Action OnDebugActionFinished;
    public void OnActionFinished() {
        if (mOwner.Dead) {
            if (mOwner.charController.enabled) {
                mOwner.charController.enabled = false;
            }
            return;
        }

        if (mActiveAction.Idx == CommonAction.Dead)
            return;

        if (mActiveAction.Idx == CommonAction.Struggle0 || mActiveAction.Idx == CommonAction.Struggle)
            return;

        if (QueuedAction2.ContainsKey(mActiveAction.Idx)) {
            int key = mActiveAction.Idx;
            QueueParam param = QueuedAction2[key];
            ChangeAction(param.action, param.fade);
            QueuedAction2.Remove(key);
        }

        if (QueuedAction.ContainsKey(mActiveAction.Idx)) {
            int idx = mActiveAction.Idx;
            ChangeAction(QueuedAction[idx], 0);
            QueuedAction.Remove(idx);
            return;
        }
        //使用火枪，状态机与普通状态机不一致
        if (mOwner.GetWeaponType() == (int)EquipWeaponType.Gun) {
            //212=>213
            if (mActiveAction.Idx == CommonAction.GunReload) {
                ChangeAction(CommonAction.GunIdle, 0.1f);
            } else {
                if (mOwner.IsOnGround()) {
                    //213=>213
                    if (mActiveAction.Idx == CommonAction.GunIdle) {
                        ChangeAction(CommonAction.GunIdle, 0);
                    } else {
                        if (LinkInput.ContainsKey(mActiveAction.Idx)) {
                            //拿着火枪在空中踢人.
                            //int TargetActionIdx = mActiveAction.Idx;
                            if (mActiveAction.Next != null)
                                ChangeAction(LinkInput[mActiveAction.Idx], mActiveAction.Next.Time);//
                            else
                                ChangeAction(LinkInput[mActiveAction.Idx], 0.1f);//ok
                            LinkInput.Clear();
                        } else
                        if (mActiveAction.Link != 0) {
                            if (mActiveAction.Next != null)
                                ChangeAction(mActiveAction.Link, mActiveAction.Next.Time);
                            else
                                ChangeAction(mActiveAction.Link, 0.1f);
                        } else {
                            if (!mOwner.GunReady) {
                                if (mActiveAction.Next != null)
                                    ChangeAction(CommonAction.Idle, mActiveAction.Next.Time);
                                else
                                    ChangeAction(CommonAction.Idle, 0.1f);
                            } else {
                                //没有重装子弹的进入213
                                if (mActiveAction.Next != null)
                                    ChangeAction(CommonAction.GunIdle, mActiveAction.Next.Time);
                                else
                                    ChangeAction(CommonAction.GunIdle, 0.1f);
                            }
                        }
                    }
                } else if (mActiveAction.Link != 0) {
                    if (mActiveAction.Next != null)
                        ChangeAction(mActiveAction.Link, mActiveAction.Next.Time);
                    else
                        ChangeAction(mActiveAction.Link, 0.1f);
                } else
                    ChangeAction(CommonAction.JumpFall, 0.1f);//拿着枪从空中落下.
            }
        } else {
            if (LinkInput.ContainsKey(mActiveAction.Idx)) {
                //int TargetActionIdx = mActiveAction.Idx;
                if (mActiveAction.Next != null)
                    ChangeAction(LinkInput[mActiveAction.Idx], mActiveAction.Next.Time);//
                else
                    ChangeAction(LinkInput[mActiveAction.Idx], 0.1f);//ok
                LinkInput.Clear();
            } else {
                //忍刀无限飞,招式结束阶段，若
                if (!mOwner.IsOnGround()) {
                    if (mOwner.SuperJump())
                        return;
                }
                        
                if (mActiveAction.Link != 0) {
                    if (mActiveAction.Next != null)
                        ChangeAction(mActiveAction.Link, mActiveAction.Next.Time);
                    else
                        ChangeAction(mActiveAction.Link, 0.1f);
                } else {
                    if (mOwner.IsOnGround()) {
                        //如果处于防御-受击状态中,恢复为防御pose
                        if (onDefence) {
                            mOwner.Defence();
                        } else
                        if (mOwner.LockTarget != null && GameStateMgr.Ins.gameStatus.AutoLock) {
                            int ReadyAction = 0;
                            switch ((EquipWeaponType)mOwner.GetWeaponType()) {
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
                                    switch (mOwner.GetWeaponSubType()) {
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
                        } else {
                            if (mActiveAction.Next != null)
                                ChangeAction(CommonAction.Idle, mActiveAction.Next.Time);
                            else
                                ChangeAction(CommonAction.Idle, 0.1f);
                        }
                    } else if (mOwner.ActionMgr.onhurt) {

                    } else
                        ChangeAction(CommonAction.JumpFall, 0.1f);
                }
            }
        }
    }

    public void OnReborn() {
        CanControl = true;
        PoseStartEvent.Clear();
        ChangeAction(CommonAction.Idle, 0);
    }

    public void OnDead() {
        CanControl = false;
    }

    //生成骨架节点名称
    public static string GetHierarchyPath(Transform root, Transform son) {
        string s = "";
        List<string> hierarchy = new List<string>();
        Transform t = son;
        while (t != root) {
            hierarchy.Insert(0, t.name);
            t = t.parent;
        }
        s = hierarchy[0];
        for (int i = 1; i < hierarchy.Count; i++) {
            s += "/" + hierarchy[i];
        }
        return s;
    }
    //使用Pose生成AnimationClip,可转换动画模式为此，就不用自己做动画混合那块了.
    //SortedDictionary<int, AnimationClip> ClipMap = new SortedDictionary<int, AnimationClip>();
    //Animation Bake;
    //public void BakePose(int pose) {
    //    if (ClipMap.ContainsKey(pose)) {
    //        return;
    //    }
    //    Pose po = ActionList[UnitId][pose];
    //    AnimationClip clip = new AnimationClip();
    //    clip.name = pose.ToString();
    //    clip.frameRate = Pose.FPS;
    //    clip.legacy = true;

    //    for (int i = 0; i < Characterloader.dummy.Count; i++) {
    //        //每一帧,每个骨骼的状态变化
    //        AnimationCurve curveRotX = new AnimationCurve();
    //        AnimationCurve curveRotY = new AnimationCurve();
    //        AnimationCurve curveRotZ = new AnimationCurve();
    //        AnimationCurve curveRotW = new AnimationCurve();
    //        AmbLoader.Ins.BakeIntoCurve(UnitId, po, curveRotX, i, BakeInto.BakeLocalDummyRotationX);
    //        AmbLoader.Ins.BakeIntoCurve(UnitId, po, curveRotY, i, BakeInto.BakeLocalDummyRotationY);
    //        AmbLoader.Ins.BakeIntoCurve(UnitId, po, curveRotZ, i, BakeInto.BakeLocalDummyRotationZ);
    //        AmbLoader.Ins.BakeIntoCurve(UnitId, po, curveRotW, i, BakeInto.BakeLocalDummyRotationW);
    //        string strHierarchy = GetHierarchyPath(Characterloader.mOwner.gameObject.transform, Characterloader.dummy[i]);
    //        clip.SetCurve(strHierarchy, typeof(Transform), "localRotation.x", curveRotX);
    //        clip.SetCurve(strHierarchy, typeof(Transform), "localRotation.y", curveRotY);
    //        clip.SetCurve(strHierarchy, typeof(Transform), "localRotation.z", curveRotZ);
    //        clip.SetCurve(strHierarchy, typeof(Transform), "localRotation.w", curveRotW);
    //        if (i == 0) {
    //            AnimationCurve curvePosX = new AnimationCurve();
    //            AnimationCurve curvePosY = new AnimationCurve();
    //            AnimationCurve curvePosZ = new AnimationCurve();
    //            //读取帧数据，得到关键帧，设置到曲线上
    //            AmbLoader.Ins.BakeIntoCurve(UnitId, po, curvePosX, i, BakeInto.BakeLocalDummyPosX);
    //            AmbLoader.Ins.BakeIntoCurve(UnitId, po, curvePosY, i, BakeInto.BakeLocalDummyPosY);
    //            AmbLoader.Ins.BakeIntoCurve(UnitId, po, curvePosZ, i, BakeInto.BakeLocalDummyPosZ);
    //            clip.SetCurve(strHierarchy, typeof(Transform), "localPosition.x", curvePosX);
    //            clip.SetCurve(strHierarchy, typeof(Transform), "localPosition.y", curvePosY);
    //            clip.SetCurve(strHierarchy, typeof(Transform), "localPosition.z", curvePosZ);
    //        }
    //    }
    //    for (int i = 0; i < Characterloader.bo.Count; i++) {
    //        //每一帧,每个骨骼的状态变化
    //        AnimationCurve curveRotX = new AnimationCurve();
    //        AnimationCurve curveRotY = new AnimationCurve();
    //        AnimationCurve curveRotZ = new AnimationCurve();
    //        AnimationCurve curveRotW = new AnimationCurve();
    //        AmbLoader.Ins.BakeIntoCurve(UnitId, po, curveRotX, i, BakeInto.BakeLocalRotationX);
    //        AmbLoader.Ins.BakeIntoCurve(UnitId, po, curveRotY, i, BakeInto.BakeLocalRotationY);
    //        AmbLoader.Ins.BakeIntoCurve(UnitId, po, curveRotZ, i, BakeInto.BakeLocalRotationZ);
    //        AmbLoader.Ins.BakeIntoCurve(UnitId, po, curveRotW, i, BakeInto.BakeLocalRotationW);
    //        string strHierarchy = GetHierarchyPath(Characterloader.mOwner.gameObject.transform, Characterloader.bo[i]);
    //        clip.SetCurve(strHierarchy, typeof(Transform), "localRotation.x", curveRotX);
    //        clip.SetCurve(strHierarchy, typeof(Transform), "localRotation.y", curveRotY);
    //        clip.SetCurve(strHierarchy, typeof(Transform), "localRotation.z", curveRotZ);
    //        clip.SetCurve(strHierarchy, typeof(Transform), "localRotation.w", curveRotW);
    //        //每个骨骼
    //        //第一个骨骼有位置的
    //        if (i == 0) {
    //            AnimationCurve curvePosX = new AnimationCurve();
    //            AnimationCurve curvePosY = new AnimationCurve();
    //            AnimationCurve curvePosZ = new AnimationCurve();
    //            //读取帧数据，得到关键帧，设置到曲线上
    //            AmbLoader.Ins.BakeIntoCurve(UnitId, po, curvePosX, i, BakeInto.BakeLocalPosX);
    //            AmbLoader.Ins.BakeIntoCurve(UnitId, po, curvePosY, i, BakeInto.BakeLocalPosY);
    //            AmbLoader.Ins.BakeIntoCurve(UnitId, po, curvePosZ, i, BakeInto.BakeLocalPosZ);
    //            clip.SetCurve(strHierarchy, typeof(Transform), "localPosition.x", curvePosX);
    //            clip.SetCurve(strHierarchy, typeof(Transform), "localPosition.y", curvePosY);
    //            clip.SetCurve(strHierarchy, typeof(Transform), "localPosition.z", curvePosZ);
    //        }
    //    }

    //    ClipMap.Add(pose, clip);
    //    if (Bake == null) {
    //        Bake = mOwner.GetComponent<Animation>();
    //        if (Bake == null) {
    //            Bake = mOwner.gameObject.AddComponent<Animation>();
    //        }
    //    }
    //    Bake.AddClip(clip, clip.name);
    //    Bake.clip = clip;
    //}

    //根据动作号开始动画.
    CharacterLoader Characterloader;
    //被动情况下播放动画，类似，受到攻击，或者防御住对方的攻击
    public void OnChangeAction(int idx) {
        //如果是一些倒地动作，动作播放完之后还需要固定长时间才能起身
        ChangeAction(idx, 0.1f);
    }

    bool IsSkillStartPose(int pose) {
        if (pose == 244)//合太极起始
            return true;
        return false;
    }

    bool IsSkillEndPose(int pose) {
        if (pose == 364)//合太极收尾
            return true;
        return false;
    }

    public void ChangeAction(int idx = CommonAction.Idle, float time = 0.01f) {
        if ((idx < 0 || idx > 573) && idx != 1000){
            Log.WriteError("can not change to action:" + idx);
            return;
        }
        //
        CanAdjust = false;
        if (CombatData.Ins.GameFinished && !playResultAction && (idx == CommonAction.Idle || idx == CommonAction.GunIdle)) {
            playResultAction = true;
            mOwner.meteorController.LockMove(true);
            if (mOwner.Camp == EUnitCamp.EUC_ENEMY && Main.Ins.GameBattleEx.BattleLose()) {
                ChangeAction(CommonAction.Taunt, 0.1f);
                return;
            } else if (mOwner.Camp == EUnitCamp.EUC_FRIEND && Main.Ins.GameBattleEx.BattleWin()) {
                ChangeAction(CommonAction.Taunt, 0.1f);
                return;
            } else if (Main.Ins.GameBattleEx.BattleTimeup()) {
                ChangeAction(CommonAction.Taunt, 0.1f);
                return;
            }
        }

        if (Characterloader != null) {
            int weapon = mOwner.GetWeaponType();
            if (idx == CommonAction.Defence) {
                mOwner.meteorController.LockMove(true);
                switch ((EquipWeaponType)weapon) {
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
            } else
            if (idx == CommonAction.Jump) {
                CanAdjust = true;//跳跃后，可以微量移动
                mOwner.meteorController.LockMove(false);
            } else if ((idx >= CommonAction.WalkForward && idx <= CommonAction.RunOnDrug) || idx == CommonAction.Crouch) {
                mOwner.meteorController.LockMove(false);
            } else if (idx == CommonAction.Idle) {
                mOwner.meteorController.LockMove(false);
            } else if (idx == CommonAction.ClimbLeft || idx == CommonAction.ClimbRight || idx == CommonAction.ClimbUp) {
                mOwner.meteorController.LockMove(false);//爬墙也需要按住方向
            }
            else if (IsReadyAction(idx)) {
                mOwner.meteorController.LockMove(false);
            }
            else if (idx == CommonAction.GunIdle) {
                mOwner.meteorController.LockMove(false);
            }
            else
                mOwner.meteorController.LockMove(true);
            //蹲下左右旋转-蹲下前后左右移动，只要之前处于火枪预备则都可以瞬间出火枪的攻击
            if (weapon == (int)EquipWeaponType.Gun) {
                if ((idx >= CommonAction.CrouchForw && idx <= CommonAction.CrouchBack) || idx == CommonAction.GunIdle) {
                    if (idx == CommonAction.GunIdle && !mOwner.GunReady)
                        mOwner.SetGunReady(true);
                    if (mOwner.Attr.IsPlayer) {
                        if (mOwner.GunReady) {
                            GunShootDialogState.State.Open();
                        } else {
                            GunShootDialogState.State.Close();
                        }
                    }
                } else {
                    if (mOwner.Attr.IsPlayer) {
                        GunShootDialogState.State.Close();
                    }
                }
            } else if (mOwner.Attr.IsPlayer) {
                GunShootDialogState.State.Close();
            }

            mOwner.IgnoreGravitys(ActionManager.IgnoreGravity(idx));//设置招式重力
            bool ignorePhy = IgnorePhysical(idx);
            if (ignorePhy != mOwner.IgnorePhysical)
                MeteorManager.Ins.PhysicalIgnore(mOwner, ignorePhy);//设置招式是否忽略角色障碍

            //看是否是大绝的起始招式/结束招式，大绝起始和结束招式之间的招式，不许响应输入切换招式.大绝不可取消.
            if (IsSkillStartPose(idx) && !mOwner.IsPlaySkill)
                mOwner.IsPlaySkill = true;
            else if ((IsSkillEndPose(idx) || onhurt) && mOwner.IsPlaySkill)
                mOwner.IsPlaySkill = false;

            //设置招式是否冻结世界轴XZ上得速度.
            mOwner.ResetWorldVelocity(IgnoreVelocityXZ(idx));

            //除了受击，防御，其他动作在有锁定目标下，都要转向锁定目标.
            if (mOwner.LockTarget != null && !onDefence && !onhurt) {
                //是否旋转面向目标.
                if (mOwner.StateMachine != null && mOwner.StateMachine.IsFighting()) {
                    //NPC.
                    //远程武器无需转向.
                    if (mOwner.GetWeaponType() != (int)EquipWeaponType.Guillotines &&
                        mOwner.GetWeaponType() != (int)EquipWeaponType.Gun &&
                        mOwner.GetWeaponType() != (int)EquipWeaponType.Dart) {
                        mOwner.FaceToTarget(mOwner.LockTarget);
                    }
                } else if (mOwner.StateMachine == null && GameStateMgr.Ins.gameStatus.AutoLock && idx != CommonAction.Idle) {
                    //主角.
                    if (mOwner.GetWeaponType() != (int)EquipWeaponType.Guillotines &&
                        mOwner.GetWeaponType() != (int)EquipWeaponType.Gun &&
                        mOwner.GetWeaponType() != (int)EquipWeaponType.Dart) {
                        //if (idx == 297) {
                        //    UnityEngine.Debug.LogError("true");
                        //}
                        mOwner.FaceToTarget(mOwner.LockTarget);
                    }
                }
            }

            if (mActiveAction != null && mActiveAction.Idx == idx)
                time = 0.0f;

            if (ActionList[UnitId].Count < idx) {
                UnityEngine.Debug.LogError("change action error animation count:" + ActionList[UnitId].Count + " unit:" + UnitId + " change action:" + idx);
                return;
            }
            ChangeActionCore(ActionList[UnitId][idx], time);
            mActiveActionIdx = mActiveAction.Idx;
            LinkInput.Clear();
        }
    }

    public void NetUpdate() {
        moveDelta = Vector3.zero;
        if (mActiveAction != null) {
            
            //UpdateAnimation(FrameReplay.deltaTime);
            UpdateAnimation2();
        }
    }

    void PlayNextKeyFrame() {
        //Debug.Log("PlayKeyFrame");
        TryPlayEffect();
        ChangeAttack();
        ChangeWeaponTrail();
        ActionEvent.HandlerActionEvent(mOwner, mActiveAction.Idx, curIndex);
        if (PoseStartEvent.ContainsKey(mActiveAction.Idx)) {
            //当218发射飞轮，很快返回，还未到219动作时，218播放完接着播放219，就得立即取消循环，221 223
            int evt = PoseStartEvent[mActiveAction.Idx];
            PoseStartEvent.Remove(mActiveAction.Idx);
            switch (evt) {
                case (int)PoseEvt.WeaponIsReturned:
                    loop = false;
                    break;
                case (int)PoseEvt.Fall:
                    loop = false;
                    mOwner.ProcessFall(true, false);
                    break;
            }
        }

        //有连招.
        if (TestInputLink())
            return;

        if (loop) {
            if (LockCurrentFrame) {
                if (PoseStraight > 0.0f)
                    PoseStraight -= FrameReplay.deltaTime;
                if (PoseStraight <= 0.0f) {
                    loop = false;
                    //curIndex = mActiveAction.LoopEnd + 1;
                    playedTimeFadeIn = mActiveAction.KeyFrames[mActiveAction.LoopEnd - mActiveAction.Start];
                    LockCurrentFrame = false;
                    return;
                }
            }
            if (playedTimeFadeIn >= mActiveAction.KeyFrames[mActiveAction.LoopEnd - mActiveAction.Start]) {
                if (playedTimeFadeIn >= mActiveAction.KeyFrames[mActiveAction.LoopStart - mActiveAction.Start]) {
                    bool actionchanged = PlayPosEvent();
                    if (actionchanged)
                        return;
                    if (loop)
                        playedTimeFadeIn = mActiveAction.KeyFrames[mActiveAction.LoopStart - mActiveAction.Start];
                    else
                        playedTimeFadeIn = mActiveAction.KeyFrames[mActiveAction.LoopEnd - mActiveAction.Start];
                    return;
                }
                playedTimeFadeIn = mActiveAction.KeyFrames[mActiveAction.LoopStart - mActiveAction.Start];
            }
        } else {
            if (playedTimeFadeIn >= mActiveAction.KeyFrames[mActiveAction.End -  mActiveAction.Start]) {
                OnActionFinished();
                return;
            }

            if (0 <= playedTimeFadeIn && TheFirstFrame != -1) {
                ActionEvent.HandlerFirstActionFrame(mOwner, mActiveAction.Idx);
                TheFirstFrame = -1;
            }
            if (TheLastFrame != -1 && mActiveAction.KeyFrames[TheLastFrame - mActiveAction.Start] <= playedTimeFadeIn) {
                ActionEvent.HandlerFinalActionFrame(mOwner, mActiveAction.Idx);
                TheLastFrame = -1;
            }
        }
        //Debug.Log("PlayKeyFrame:" + Time.frameCount);
        //BoneStatus status = null;
        //if (mActiveAction.SourceIdx == 0)
        //    status = AmbLoader.Ins.CharCommon[curIndex];
        //else if (mActiveAction.SourceIdx == 1)
        //    status = AmbLoader.Ins.PlayerAnimation[mOwner.UnitId][curIndex];

        ////Debug.LogError("play keyframe " + " idx:" + curIndex);
        //if (Characterloader.bo.Count != 0) {
        //    Characterloader.bo[0].localRotation = status.BoneQuat[0];
        //    Characterloader.bo[0].localPosition = status.BonePos;
        //}

        //for (int i = 1; i < Characterloader.bo.Count; i++)
        //    Characterloader.bo[i].localRotation = status.BoneQuat[i];

        //bool IgnoreActionMoves = ActionManager.IgnoreActionMove(mActiveAction.Idx);
        //if (lastPosIdx == mActiveAction.Idx) {
        //    Vector3 targetPos = status.DummyPos[0];
        //    Vector3 vec = Characterloader.Target.rotation * (targetPos - lastDBasePos) * moveScale;
        //    //如果忽略位移，或者在动作的循环帧中，即第一次从循环头开始播放后，不再计算位移.
        //    if (IgnoreActionMoves) {
        //        vec.x = 0;
        //        vec.z = 0;
        //        vec.y = 0;
        //    }
        //    moveDelta += vec;
        //    //if (po.Idx == 151)
        //    //    Debug.LogError(string.Format("pose:{0} frame:{1} move: x ={2}, y ={3} z = {4}", po.Idx, curIndex, moveDelta.x, moveDelta.y, moveDelta.z));
        //    lastDBasePos = targetPos;
        //}

        //for (int i = 1; i < Characterloader.dummy.Count; i++) {
        //    Characterloader.dummy[i].localRotation = status.DummyQuat[i];
        //    Characterloader.dummy[i].localPosition = status.DummyPos[i];
        //}

        //lastFrameIndex = curIndex;
        
        lastPosIdx = mActiveAction.Idx;
    }

    BoneStatus lastFrameStatus;
    public void GetCurrentSnapShot() {
        if (lastFrameStatus == null) {
            lastFrameStatus = new BoneStatus();
            lastFrameStatus.Init();
            for (int i = 0; i < Characterloader.bo.Count; i++) {
                lastFrameStatus.BoneQuat.Add(new MyQuaternion());
                if (i == 0)
                    lastFrameStatus.BonePos = new MyVector();
            }
            for (int i = 0; i < Characterloader.dummy.Count; i++) {
                lastFrameStatus.DummyQuat.Add(new MyQuaternion());
                lastFrameStatus.DummyPos.Add(new MyVector());
            }
        }
        for (int i = 0; i < Characterloader.bo.Count; i++) {
            lastFrameStatus.BoneQuat[i] = Characterloader.bo[i].localRotation;
            if (i == 0)
                lastFrameStatus.BonePos = Characterloader.bo[i].localPosition;
        }
        for (int i = 0; i < Characterloader.dummy.Count; i++) {
            lastFrameStatus.DummyQuat[i] = Characterloader.dummy[i].localRotation;
            lastFrameStatus.DummyPos[i] = Characterloader.dummy[i].localPosition;
        }
    }
    
    void UpdateAnimation2() {
        if (mActiveAction != null) {
            if (blendTime == 0.0f) {
                playedTimeFadeInWrapped = WrapperTime(mActiveAction, playedTimeFadeIn, loop);
                playedTimeFadeIn += FrameReplay.deltaTime;
                //lastFrameIndex = curIndex;
                bool runCycle = false;
                if (loop) {
                    if (playedTimeFadeIn >= mActiveAction.LoopCache.Key) {
                        runCycle = true;
                    }
                }
                curIndex = PoseState.EvaluatePoseClamp(playedTimeFadeInWrapped, mActiveAction, ref activeStatus, UnitId, runCycle);
                ApplyPose(activeStatus, true);
                PlayNextKeyFrame();
            } else {
                ChangeWeaponTrail();
                BoneStatus status = null;
                if (mActiveAction.SourceIdx == 0 && AmbLoader.Ins.CharCommon.Count > blendStart && blendStart >= 0)
                    status = AmbLoader.Ins.CharCommon[blendStart];
                else if (mActiveAction.SourceIdx == 1 && AmbLoader.Ins.PlayerAnimation.ContainsKey(mOwner.ModelId) && AmbLoader.Ins.PlayerAnimation[mOwner.ModelId].Count > blendStart && blendStart >= 0)
                    status = AmbLoader.Ins.PlayerAnimation[mOwner.ModelId][blendStart];

                if (blendingTime < blendTime && blendTime != 0.0f) {
                    //取得目标动作的左右关键帧
                    blendingTime += FrameReplay.deltaTime;
                    float ratio = (blendingTime / blendTime);
                    Characterloader.bo[0].localRotation = Quaternion.Slerp(lastFrameStatus.BoneQuat[0], status.BoneQuat[0], ratio);
                    Characterloader.bo[0].localPosition = Vector3.Lerp(lastFrameStatus.BonePos, status.BonePos, ratio);
                    for (int i = 1; i < Characterloader.bo.Count; i++) {
                        Characterloader.bo[i].localRotation = Quaternion.Slerp(lastFrameStatus.BoneQuat[i], status.BoneQuat[i], ratio);
                    }
                    lastDBasePos = status.DummyPos[0];
                    for (int i = 1; i < Characterloader.dummy.Count; i++) {
                        Characterloader.dummy[i].localRotation = Quaternion.Slerp(lastFrameStatus.DummyQuat[i], status.DummyQuat[i], ratio);
                        Characterloader.dummy[i].localPosition = Vector3.Lerp(lastFrameStatus.DummyPos[i], status.DummyPos[i], ratio);
                    }
                    
                    //weight_fadein += FrameReplay.deltaTime * weight_fadein_delta;
                    //weight_fadeout += FrameReplay.deltaTime * weight_fadeout_delta;
                    //if (weight_fadein > weight_fadein_target)
                    //    weight_fadein = weight_fadein_target;
                    //if (weight_fadeout < weight_fadeout_target)
                    //    weight_fadeout = weight_fadeout_target;
                    //float weightSum = weight_fadein + weight_fadeout;
                    ////取得blendStart的帧姿势，


                    //playedTimeFadeIn += FrameReplay.deltaTime;
                    //playedTimeFadeInWrapped = WrapperTime(mActiveAction, playedTimeFadeIn, loop);
                    ////取得当前动画姿势-继续往前播放中.
                    //playedTimeFadeOut += FrameReplay.deltaTime;
                    //bool loopFadeOut = mFadeOutAction.LoopEnd != 0 && mFadeOutAction.LoopStart != 0;
                    //playedTimeFadeOutWrapped = WrapperTime(mFadeOutAction, playedTimeFadeOut, loopFadeOut);
                    ////取得当前动作的左右关键帧
                    //bool runCycle = false;
                    //if (loop) {
                    //    if (playedTimeFadeIn > mActiveAction.LoopCache.Key) {
                    //        runCycle = true;
                    //    }
                    //}
                    //PoseState.EvaluatePoseClamp(playedTimeFadeInWrapped, mActiveAction, ref activeStatus, UnitId, runCycle);
                    //if (weight_fadeout != 0.0f)
                    //    PoseState.EvaluatePoseClamp(playedTimeFadeOutWrapped, mFadeOutAction, ref fadeOutStatus, UnitId, false);
                    //BoneStatus sampleStatus = new BoneStatus();
                    //sampleStatus.InitWith(30, 6);
                    //if (weight_fadeout != 0.0f)
                    //    Process(sampleStatus, fadeOutStatus, weight_fadeout / weightSum);
                    //Process(sampleStatus, activeStatus, weight_fadein / weightSum);
                    //ApplyPose(sampleStatus, false);
                } else {
                    blendTime = 0.0f;
                    curIndex = blendStart;
                    blendingTime = 0;
                    //playedTimeFadeOut = 0;
                    //weight_fadein = 1;
                    //weight_fadein_delta = 0;
                    //weight_fadein_target = 0;
                    //weight_fadeout = 1;
                    //weight_fadeout_delta = 0;
                    //weight_fadeout_target = 0;
                }
            }
        }
    }

    //取得时间
    float WrapperTime(Pose pose, float t, bool useloop = true) {
        float start = pose.Cache.Key;
        float end = pose.Cache.Value;
        bool loop = pose.LoopStart != 0 && pose.LoopEnd != 0;
        if (useloop && loop) {
            start = pose.LoopCache.Key;
            end = pose.LoopCache.Value;

            if (t > end) {
                float left = t - end;
                if (start == end) {
                    return start;
                } else {
                    int times = Mathf.FloorToInt(left / (end - start));
                    return start + left - times * (end - start);
                }
            }
            else {
                return t;
            }
        }
        return Mathf.Clamp(t, start, end);
    }


    //合并已经计算出的结果，和下一个动画的采样
    //主要是由于关节的已存在四元数如果和当前四元数的DOT值为负，则权重要取反
    public static void Process(BoneStatus source, BoneStatus sample, float weights) {
        float weight = weights;
        for (int i = 0; i < source.BoneQuat.Count; i++) {
            weight = weights;
            if (Quaternion.Dot(source.BoneQuat[i], sample.BoneQuat[i]) < 0.0f) {
                weight = -weights;
            }
            source.BoneQuat[i] += sample.BoneQuat[i].Scale(weight);
        }

        for (int i = 0; i < source.DummyQuat.Count; i++) {
            weight = weights;
            if (Quaternion.Dot(source.DummyQuat[i], sample.DummyQuat[i]) < 0.0f) {
                weight = -weights;
            }
            source.DummyQuat[i] += sample.DummyQuat[i].Scale(weight);
        }
        weight = weights;
        for (int i = 0; i < source.DummyPos.Count; i++) {
            source.DummyPos[i] += sample.DummyPos[i].Scale(weight);
        }
        source.BonePos += sample.BonePos.Scale(weight);
    }

    //应用骨架姿势
    void ApplyPose(BoneStatus status, bool rootMotion) {
        for (int i = 0; i < Characterloader.bo.Count; i++) {
            Characterloader.bo[i].localRotation = status.BoneQuat[i];
            if (i == 0)
                Characterloader.bo[i].localPosition = status.BonePos;
        }
        bool IgnoreActionMoves = IgnoreActionMove(mActiveAction.Idx);
        if (lastPosIdx == mActiveAction.Idx && rootMotion) {
            Vector3 targetPos = status.DummyPos[0];
            Vector3 diff = targetPos - lastDBasePos;
            diff = diff * moveScale;
            Vector3 vec = Characterloader.Target.rotation * diff;
            //如果忽略位移，或者在动作的循环帧中，即第一次从循环头开始播放后，不再计算位移.
            if (IgnoreActionMoves) {
                vec.x = 0;
                vec.z = 0;
                vec.y = 0;
            }
            moveDelta += vec;
            //if (po.Idx == 151)
            //    Debug.LogError(string.Format("pose:{0} frame:{1} move: x ={2}, y ={3} z = {4}", po.Idx, curIndex, moveDelta.x, moveDelta.y, moveDelta.z));
            lastDBasePos = targetPos;
        }
        for (int i = 1; i < Characterloader.dummy.Count; i++) {
            Characterloader.dummy[i].localRotation = status.DummyQuat[i];
            Characterloader.dummy[i].localPosition = status.DummyPos[i];
        }
    }

    public float FPS = 1.0f / 30.0f;
    int lastFrameIndex = 1;
    //int lastSource = 1;
    int lastPosIdx = 0;
    Vector3 lastDBasePos = Vector3.zero;//上一帧的d_base骨骼坐标.没应用到骨骼上，只是记录下来

    //设置动作位移的根骨骼移动比例,比如完整动作，会让角色Y轴移动10，那么比例为2时，这个动作就会让角色移动20，而帧数不变
    [SerializeField]
    float moveScale = 1.0f;
    public void SetActionScale(float scale) {
        moveScale = scale;
    }

    bool LockCurrentFrame = false;
    public void LockTime(float t) {
        PoseStraight = t;
    }

    public bool IsInStraight() {
        return PoseStraight > 0;
    }

    public bool InTransition() {
        return blendTime != 0.0;
    }

    public float PoseStraight = 0.0f;
    //循环动作锁定，（一直在2个帧段之间播放，待特定条件打成就退出该帧段）
    public bool PlayPosEvent() {
        //有硬直时间存在，但是还没开始消耗硬直时间时，先消耗硬直时间
        if (IsInStraight()) {
            if (LockCurrentFrame)
                return false;
            LockCurrentFrame = true;//开始计算硬直流逝，一直到硬直时间结束.
            return false;
        } else {
            if (mActiveAction.Idx == CommonAction.JumpFallOnGround || mActiveAction.Idx == CommonAction.KnifeA2Fall) {
                if (mOwner.IsOnGround()) {
                    //ChangeAction(0, 0);
                    loop = false;
                    return false;
                }
            } else if (mActiveAction.Idx == CommonAction.ChangeWeapon) {
                mOwner.ChangeNextWeapon();
                loop = false;
                return false;
            } else if (mActiveAction.Idx == CommonAction.AirChangeWeapon) {
                mOwner.ChangeNextWeapon();
                loop = false;
                return false;
            } else if ((mActiveAction.Idx >= CommonAction.Idle && mActiveAction.Idx <= 21) || (mActiveAction.Idx >= CommonAction.WalkForward && mActiveAction.Idx <= CommonAction.RunOnDrug)) {
                if (CombatData.Ins.GameFinished && !playResultAction)
                    loop = false;
                return false;
            }
            else if (mActiveAction.Idx == 219 || mActiveAction.Idx == 221 || mActiveAction.Idx == 223)//飞轮出击后等待接回飞轮
            {
                return false;
                //等着收回武器
            } else if (QueuedAction.ContainsKey(mActiveAction.Idx)) {
                int idx = mActiveAction.Idx;
                ChangeAction(QueuedAction[idx], 0.1f);
                QueuedAction.Remove(idx);
                return true;
            }
            else {
                //锁帧，等着结束.
                if (mOwner.IsOnGround()) {
                    //ChangeAction(CommonAction.Idle, 0.1f);
                    loop = false;
                    return false;
                }
            }
            return false;
        }
    }

    float GetTimePlayed(int frame) {
        float frameCost = 0.0f;
        for (int i = mActiveAction.Start; i < frame; i++) {
            float speedScale = 1.0f;
            for (int j = 0; j < mActiveAction.ActionList.Count; j++) {
                if (i >= mActiveAction.ActionList[j].Start && i <= mActiveAction.ActionList[j].End) {
                    speedScale = (mActiveAction.ActionList[j].Speed == 0.0f ? 1.0f : mActiveAction.ActionList[j].Speed);
                    break;
                }
            }
            frameCost += (Pose.FCS / speedScale);
        }
        return frameCost;
    }

    void PlayEffect() {
        if (!string.IsNullOrEmpty(mActiveAction.EffectID) && !string.Equals(mActiveAction.EffectID, "0")) {
            Characterloader.sfxEffect = SFXLoader.Ins.PlayEffect(string.Format("{0}.ef", mActiveAction.EffectID), mOwner, playedTimeFadeInWrapped);
            //表明特效是由动作触发的,不在该动作中关闭特效的攻击盒时,特效攻击盒仍存在
            //这种一般是特效出来后，在角色受到攻击前打开了特效的攻击盒，但角色受到攻击打断了动作，会立刻关闭攻击特效的攻击属性，这种应该是不对的.
            //类似雷电斩，特效出来后只要攻击盒被打开过，一旦动作被打断，那么攻击特效会一直到特效完毕.
        }
        effectPlayed = true;
    }

    public float blendTime = 0.3f;
    float blendingTime = 0;
    public int curIndex = 0;//帧编号
    int blendStart = 0;//接到哪一帧
    public void SetTime(float t) { playedTimeFadeIn = t; }
    public int GetCurrentFrameIndex() { return curIndex; }

    //僵直清空/飞轮回收，等一些情况时，取消循环,立即切换到动作结束
    public void SetLoop(bool looped) {
        loop = looped;
        curIndex = mActiveAction.LoopEnd;
        playedTimeFadeIn = mActiveAction.KeyFrames[mActiveAction.LoopEnd - mActiveAction.Start];
    }

    public bool GetLoop() {
        return loop;
    }

    //动作开始播放时触发的事件

    Dictionary<int, int> PoseStartEvent = new Dictionary<int, int>();
    public void LinkEvent(int pose, PoseEvt evt) {
        if (PoseStartEvent.ContainsKey(pose))
            PoseStartEvent[pose] = (int)evt;
        else
            PoseStartEvent.Add(pose, (int)evt);
    }

    bool loop = false;
    //这2个用来实现一些技能
    int TheFirstFrame = -1;//第一个Action的第一帧，0则无
    int TheLastFrame = -1;//最后一个Action的最后一帧，0则无
    bool effectPlayed = false;

    
    //float weight_fadein;//混入权重
    //float weight_fadein_target;//混入权重目标
    //float weight_fadein_delta;
    //float weight_fadeout;//混出权重
    //float weight_fadeout_target;//混出权重目标
    //float weight_fadeout_delta;
    bool blending;//是否处于混合中
    BoneStatus activeStatus = new BoneStatus();
    BoneStatus fadeOutStatus = new BoneStatus();
    public void ChangeActionCore(Pose pos, float BlendTime = 0.0f) {
        //没有提取动作的信息前先烘一遍.
        if (!pos.baked)
            pos.Bake();
        //UnityEngine.Debug.Log("change action:" + pos.Idx);
        //一些招式，需要把尾部事件执行完才能切换武器.
        //if (pos.Idx == mActiveActionIdx && pos.Idx == 0)
        //    UnityEngine.Debug.LogError("error");
        LockCurrentFrame = false;
        if (TheLastFrame != -1 && mActiveAction != null) {
            ActionEvent.HandlerFinalActionFrame(mOwner, mActiveAction.Idx);
            TheLastFrame = -1;
        } else {
            if (mActiveAction != null && mActiveAction.Link != 0)
                //一些动作，默认连接其他动作，类似,486第一帧会收刀，收刀会切换武器为2
                ActionEvent.HandlerPoseAction(mOwner, mActiveAction.Link);
        }
        //一些招式，动作结束会给使用者加上BUFF，另外一些招式，会让受击方得到BUFF
        moveScale = 1.0f;
        //重置速度
        bool isAttackPos = false;
        if (mActiveAction == null)
            isAttackPos = false;
        else
            isAttackPos = IsAttackPose(mActiveAction.Idx);
        //保存当前帧的姿势，用于和下个动作融合
        //当前状态下有姿势，且帧存在状态缓存
        if (mActiveAction != null)
            lastPosIdx = mActiveAction.Idx;
        else
            lastPosIdx = pos.Idx;
        mActiveAction = pos;
        loop = (pos.LoopStart != 0 && pos.LoopEnd != 0);//2帧都不为0

        //查看第一个blend的最后一帧，如果有，切换目标帧设置为这个,若第一个是act则目标帧为起始帧
        //PosAction blend = null;
        PosAction act = null;
        if (pos.ActionList.Count != 0) {
            if (isAttackPos) {
                for (int i = 0; i < pos.ActionList.Count; i++) {
                    //过滤掉565，刀雷电斩的头一个 第一个混合段与整个动画一致.
                    if (pos.ActionList[i].Start == pos.Start && pos.ActionList[i].End == pos.End)
                        continue;
                    act = pos.ActionList[i];
                    break;
                }
            } else
                act = pos.ActionList[0];
        }

        TheLastFrame = pos.End - 1;
        TheFirstFrame = pos.Start;

        //算第一个过渡帧条件很多，有切换目的帧是否设定，第一个过渡帧是否存在，上一个动作是否攻击动作，锤绝325BUG，其他招式接325，还要在地面等，应该不需要在地面等
        //curIndex = targetFrame != 0 ? targetFrame : (act != null ? (act.Type == "Action" ? act.Start: (isAttackPos ? act.End : pos.Start)): pos.Start);
        curIndex = act != null ? (act.Type == "Action" ? (isAttackPos ? act.Start : pos.Start) : (isAttackPos ? act.End : act.Start)) : pos.Start;
        //部分动作混合帧比开始帧还靠前
        if (curIndex < pos.Start)
            curIndex = pos.Start;
        blendStart = curIndex;
        effectPlayed = false;
        Characterloader.sfxEffect = null;
        //下一个动作的第一帧所有虚拟物体，因为2个动作的d_base骨骼不能跨动作算位置移动，就是把d_base归位，同一个动作的d_base的移动，才能计算角色移动.
        if (mActiveAction.SourceIdx == 0)
            lastDBasePos = AmbLoader.Ins.CharCommon[curIndex].DummyPos[0];
        else if (mActiveAction.SourceIdx == 1)
            lastDBasePos = AmbLoader.Ins.PlayerAnimation[mOwner.ModelId][curIndex].DummyPos[0];

        blendTime = 0.0f;
        //blendTime = BlendTime;
        if (BlendTime > 0.01f) {
            GetCurrentSnapShot();
            blendTime = BlendTime;
            //playedTimeFadeOut = playedTimeFadeIn;
            playedTimeFadeIn = pos.KeyFrames[blendStart - pos.Start];//淡入动作进入时刻
            //weight_fadein = 0.0f;
            //weight_fadein_target = 1.0f;
            //weight_fadein_delta = 1.0f / blendTime;
            //weight_fadeout = 1.0f;
            //weight_fadeout_target = 0.0f;
            //weight_fadeout_delta = -1.0f / blendTime;
        } else {
            playedTimeFadeIn = 0;
            //playedTimeFadeOut = 0;
        }

        //部分动作第一帧就比循环锁定帧要大
        if (loop) {
            float t = pos.KeyFrames[pos.LoopStart - pos.Start];
            if (playedTimeFadeIn > t) {
                playedTimeFadeIn = t;
                //UnityEngine.Debug.LogError("error");
                blendStart = pos.LoopStart;
                curIndex = pos.LoopStart;
            }
        }

        if (lastPosIdx != 0) {
            Option poseInfo = MenuResLoader.Ins.GetPoseInfo(lastPosIdx);
            if (poseInfo.first.Length != 0 && poseInfo.first[0].flag[0] == 18 && lastPosIdx != 468)//18为，使用招式后获取物品ID 468-469都会调用微尘，hack掉468pose的
                mOwner.GetItem(poseInfo.first[0].flag[1]);//忍刀小绝，同归于尽，会获得微尘物品，会即刻死亡
        }

        //部分事件会发生在动作之前，要在播放指定动作时，处理
        if (PoseStartEvent.ContainsKey(mActiveAction.Idx)) {
            int evt = PoseStartEvent[mActiveAction.Idx];
            switch (evt) {
                case (int)PoseEvt.WeaponIsReturned:
                    loop = false;
                    PoseStartEvent.Remove(mActiveAction.Idx);
                    //blendTime = 0.0f;
                    break;
                case (int)PoseEvt.Fall:
                    break;
            }
        }

        if (pos.Idx == CommonAction.Struggle || pos.Idx == CommonAction.Struggle0) {
            if (PoseStraight <= 0.32f) {
                LockTime(0.32f);
            }
        }
    }

    public float playedTimeFadeIn;//要过渡到的目的动画播放
    public float playedTimeFadeInWrapped;
    void TryPlayEffect() {
        if (!effectPlayed) {
            try {
                //在其他调试场景里屏蔽掉特效
                if (Main.Ins.GameBattleEx != null)
                    PlayEffect();
            } catch (System.Exception exp) {
                UnityEngine.Debug.LogError("Effect: [" + mActiveAction.EffectID + " ] Contains Error" + exp.StackTrace);
                effectPlayed = true;
            }
            effectPlayed = true;
        }
    }

    //在某一帧之前
    bool IsBeforeFrame(int frame) {
        int f = Mathf.Clamp(frame, mActiveAction.Start, mActiveAction.End);
        return playedTimeFadeInWrapped <= mActiveAction.KeyFrames[f - mActiveAction.Start];
    }

    //是否在指定的2帧之间
    public bool IsInKeyFrame(int min, int max) {
        int start = Mathf.Clamp(min, mActiveAction.Start, mActiveAction.End);
        int end = Mathf.Clamp(max, mActiveAction.Start, mActiveAction.End); 
        return (playedTimeFadeInWrapped >= mActiveAction.KeyFrames[start-mActiveAction.Start] && playedTimeFadeInWrapped <= mActiveAction.KeyFrames[end-mActiveAction.Start]);
    }

    void ChangeAttack() {
        bool open = false;
        for (int i = 0; i < mActiveAction.Attack.Count; i++) {
            if (IsInKeyFrame(mActiveAction.Attack[i].Start, mActiveAction.Attack[i].End)) {
                mOwner.ChangeAttack(mActiveAction.Attack[i]);
                open = true;
                break;
            }
        }
        if (!open) {
            mOwner.ChangeAttack(null);
        }
    }

    void ChangeWeaponTrail() {
        if (U3D.IsSpecialWeapon(mOwner.weaponLoader.WeaponSubType()))
            return;
        //开启武器拖尾
        if (mActiveAction.Drag != null) {
            if (IsInKeyFrame(mActiveAction.Drag.Start, mActiveAction.Drag.End))
                mOwner.ChangeWeaponTrail(mActiveAction.Drag);
            else
                mOwner.ChangeWeaponTrail(null);
        } else
            mOwner.ChangeWeaponTrail(null);
    }

    bool TestInputLink() {
        //有连招等待播放
        if (LinkInput.ContainsKey(mActiveAction.Idx)) {
            //当前正处于融合帧中，可以立即切换动画
            for (int i = 0; i < mActiveAction.ActionList.Count; i++) {
                if (mActiveAction.ActionList[i].Type == "Blend") {
                    if (curIndex >= mActiveAction.ActionList[i].Start && curIndex <= mActiveAction.ActionList[i].End) {
                        //Debug.LogError("TestInputLink");
                        int targetIdx = mActiveAction.Idx;
                        if (mActiveAction.Next != null)
                            ChangeAction(LinkInput[targetIdx], mActiveAction.Next.Time);
                        else
                            ChangeAction(LinkInput[targetIdx], 0);
                        LinkInput.Clear();
                        return true;
                    }
                }
            }
        }
        return false;
    }

    static void Parse(string text, int id) {
        if (ActionList.ContainsKey(id) && ActionList[id].Count != 0) {
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
        for (int i = 0; i < pos.Length; i++) {
            string line = pos[i].Replace("\t", "");
            string[] lineObject = line.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (lineObject.Length == 0) {
                //if (i == 7574)
                //    UnityEngine.Debug.Log("line i:" + i);
                //空行跳过
                continue;
            } else if (lineObject[0].StartsWith("#"))
                continue;
            else
              if (lineObject[0] == "Pose" && left == 0 && leftAct == 0) {
                Pose insert = new Pose();
                ActionList[id].Add(insert);
                int idx = int.Parse(lineObject[1]);
                insert.Idx = idx;
                current = insert;
            } else if (lineObject[0] == "{") {
                if (nex != null)
                    leftNex++;
                else
                if (dra != null)
                    leftDra++;
                else
                if (att != null) {
                    leftAtt++;
                } else
                    if (curAct != null)
                    leftAct++;
                else
                    left++;
            } else if (lineObject[0] == "}") {
                if (nex != null) {
                    leftNex--;
                    if (leftNex == 0)
                        nex = null;
                } else
                if (dra != null) {
                    leftDra--;
                    if (leftDra == 0)
                        dra = null;
                } else
                if (att != null) {
                    leftAtt--;
                    if (leftAtt == 0)
                        att = null;
                } else
                if (curAct != null) {
                    leftAct--;
                    if (leftAct == 0)
                        curAct = null;
                } else {
                    left--;
                    if (left == 0)
                        current = null;
                }

            } else if (lineObject[0] == "link" || lineObject[0] == "Link" || lineObject[0] == "Link\t" || lineObject[0] == "link\t") {
                current.Link = int.Parse(lineObject[1]);
            } else if (lineObject[0] == "source" || lineObject[0] == "Source") {
                current.SourceIdx = int.Parse(lineObject[1]);
            } else if (lineObject[0] == "Start" || lineObject[0] == "start") {
                //UnityEngine.Debug.Log(lineObject[1]);
                //if (lineObject[1].StartsWith("0"))
                //    lineObject[i] = lineObject[1].Substring(1);
                if (nex != null) {
                    nex.Start = int.Parse(lineObject[1]);
                } else
                if (dra != null) {
                    dra.Start = int.Parse(lineObject[1]);
                } else
                if (att != null) {
                    att.Start = int.Parse(lineObject[1]);
                } else
                if (curAct != null)
                    curAct.Start = int.Parse(lineObject[1]);
                else
                    current.Start = int.Parse(lineObject[1]);
            } else if (lineObject[0] == "End" || lineObject[0] == "end") {
                //if (pose295) {
                //    UnityEngine.Debug.LogError("295");
                //}
                //if (lineObject[1].StartsWith("0"))
                //    lineObject[i] = lineObject[1].Substring(1);
                if (nex != null) {
                    nex.End = int.Parse(lineObject[1]);
                } else
                if (dra != null) {
                    dra.End = int.Parse(lineObject[1]);
                } else
                if (att != null) {
                    att.End = int.Parse(lineObject[1]);
                } else
                if (curAct != null)
                    curAct.End = int.Parse(lineObject[1]);
                else
                    current.End = int.Parse(lineObject[1]);
            } else if (lineObject[0] == "Speed" || lineObject[0] == "speed") {
                if (curAct != null)
                    curAct.Speed = float.Parse(lineObject[1]);
            } else if (lineObject[0] == "LoopStart") {
                current.LoopStart = int.Parse(lineObject[1]);
                //部分pos改的乱了，pose 09 锤子的预备姿势
                if (current.LoopStart < current.Start)
                    current.LoopStart = current.Start;
            } else if (lineObject[0] == "LoopEnd") {
                current.LoopEnd = int.Parse(lineObject[1]);
            } else if (lineObject[0] == "EffectType") {
                current.EffectType = int.Parse(lineObject[1]);
            } else if (lineObject[0] == "EffectID") {
                current.EffectID = lineObject[1];
            } else if (lineObject[0] == "Blend") {
                PosAction act = new PosAction();
                act.Type = "Blend";
                current.ActionList.Add(act);
                curAct = act;
            } else if (lineObject[0] == "Action") {
                PosAction act = new PosAction();
                act.Type = "Action";
                current.ActionList.Add(act);
                curAct = act;
            } else if (lineObject[0] == "Attack") {
                att = new AttackDes();
                att.PoseIdx = current.Idx;
                //if (att.PoseIdx == 295) {
                //    pose295 = true;
                //}
                current.Attack.Add(att);
            } else if (lineObject[0] == "bone") {
                //重新分割，=号分割，右边的,号分割
                lineObject = line.Split(new char[] { '=' }, System.StringSplitOptions.RemoveEmptyEntries);
                string bones = lineObject[1];
                bool restore = false;
                while (bones.EndsWith(",")) {
                    i++;
                    lineObject = new string[1];
                    lineObject[0] = pos[i];
                    
                    //部分招式，在受击列表最后一行后加了,导致读到下一行去了. 907的pose 295存在这个问题
                    if (lineObject[0].Contains("Start") || lineObject[0].Contains("start"))
                        restore = true;
                    else
                        bones += lineObject[0];
                }
                if (restore)
                    i--;
                //bones = bones.Replace(' ', '_');
                string[] bonesstr = bones.Split(new char[] { ',' });
                for (int j = 0; j < bonesstr.Length; j++) {
                    string b = bonesstr[j].TrimStart(new char[] { ' ', '\"' });
                    b = b.TrimEnd(new char[] { '\"', ' ' });
                    b = b.Replace(' ', '_');
                    att.bones.Add(b);
                }
            } else if (lineObject[0] == "AttackType") {
                att._AttackType = int.Parse(lineObject[1]);
            } else if (lineObject[0] == "CheckFriend") {
                att.CheckFriend = int.Parse(lineObject[1]);
            } else if (lineObject[0] == "DefenseValue") {
                att.DefenseValue = float.Parse(lineObject[1]);
            } else if (lineObject[0] == "DefenseMove") {
                att.DefenseMove = float.Parse(lineObject[1]);
            } else if (lineObject[0] == "TargetValue") {
                att.TargetValue = float.Parse(lineObject[1]);
            } else if (lineObject[0] == "TargetMove") {
                att.TargetMove = float.Parse(lineObject[1]);
            } else if (lineObject[0] == "TargetPose") {
                att.TargetPose = int.Parse(lineObject[1]);
            } else if (lineObject[0] == "TargetPoseFront") {
                att.TargetPoseFront = int.Parse(lineObject[1]);
            } else if (lineObject[0] == "TargetPoseBack") {
                att.TargetPoseBack = int.Parse(lineObject[1]);
            } else if (lineObject[0] == "TargetPoseLeft") {
                att.TargetPoseLeft = int.Parse(lineObject[1]);
            } else if (lineObject[0] == "TargetPoseRight") {
                att.TargetPoseRight = int.Parse(lineObject[1]);
            } else if (lineObject[0] == "Drag") {
                dra = new DragDes();
                current.Drag = dra;
            } else if (lineObject[0] == "Time") {
                if (nex != null)
                    nex.Time = float.Parse(lineObject[1]);
                else
                    dra.Time = float.Parse(lineObject[1]);
            } else if (lineObject[0] == "Color") {
                string[] rgb = lineObject[1].Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                dra.Color.x = int.Parse(rgb[0]);
                dra.Color.y = int.Parse(rgb[1]);
                dra.Color.z = int.Parse(rgb[2]);
            } else if (lineObject[0] == "NextPose") {
                current.Next = new NextPose();
                nex = current.Next;
            } else if (lineObject[0] == "{}") {
                current = null;
                continue;
            } else {
                UnityEngine.Debug.Log("line :" + i + " can t understand：" + pos[i]);
                break;
            }
        }

        //if (id >= 20) {
        //    //额外的模型检查一下动画数据，有些动画片段切割有问题.
        //    for (int i = 0; i < ActionList[id].Count; i++) {
        //        ActionList[id][i].Check(id);
        //    }
        //}
        if (ActionList[id].Count < 573)
            UnityEngine.Debug.LogError("action parse error");
        //for (int i = 0; i < ActionList[id].Count; i++) {
        //    ActionList[id][i].Bake();
        //}
    }
}

public class Pose {
    public static float FCS;//1.0f/30.0f
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
    //检查每个动作的开始帧和结束帧，是否都有对应的数据
    //public void Check(int unitid) {
    //    BoneStatus bstart = AmbLoader.Ins.GetBoneStatus(SourceIdx, unitid, Start);
    //    BoneStatus bend = AmbLoader.Ins.GetBoneStatus(SourceIdx, unitid, End);
    //    if (bstart == null || bend == null) {
    //        UnityEngine.Debug.LogError("pos:" + Idx + " on unit:" + unitid + " contains error");
    //    }
    //}
    public bool baked = false;
    public void Bake() {
        float speedScale = 1.0f;
        int Prev = Start;
        float time = 0.0f;
        for (int i = Start; i <= End; i++) {
            speedScale = 1.0f;
            for (int j = 0; j < ActionList.Count; j++) {
                if (i >= ActionList[j].Start && i <= ActionList[j].End) {
                    speedScale = (ActionList[j].Speed == 0.0f ? 1.0f : ActionList[j].Speed);
                    break;
                }
            }
            time += (i - Prev) * FCS / speedScale;
            Prev = i;
            KeyFrames.Add(time);
        }
        Cache = new KeyValuePair<float, float>(KeyFrames[0], KeyFrames[KeyFrames.Count - 1]);
        if (LoopStart != 0 && LoopEnd != 0)
            LoopCache = new KeyValuePair<float, float>(KeyFrames[LoopStart - Start], KeyFrames[LoopEnd - Start]);
        baked = true;
    }
    //每个pose烘培一个自己的.
    public List<float> KeyFrames = new List<float>();
    public KeyValuePair<float, float> Cache;//非循环起始时间
    public KeyValuePair<float, float> LoopCache;//循环起始时间
    public int GetFrame(float t) {
        int ret = 0;
        for (int i = 0; i < KeyFrames.Count; i++) {
            if (KeyFrames[i] < t)
                ret = i;
            else
                break;
        }
        return ret;
    }
}

//动作状态,记录播放某个动作的混合相关的数据
public class PoseState {
    //找到对应时间左右的2个关键帧，计算插值，得到临时骨架的姿势
    public static int EvaluatePoseClamp(float time, Pose clip, ref BoneStatus pose, int playerId, bool useloop = true) {
        int keyframe = clip.GetFrame(time);
        int lhs = 0;
        int rhs = 0;
        int Start = clip.Start;
        int End = clip.End;
        //float playedTimes = 0.0f;
        //float duration = 0.0f;
        bool clipLoop = clip.LoopEnd != 0 && clip.LoopStart != 0;
        if (clipLoop && useloop) {
            if (Start + keyframe >= clip.LoopEnd) {
                lhs = clip.LoopEnd;
                rhs = clip.Start;
            } else {
                lhs = Start + keyframe;
                rhs = Start + keyframe + 1;
            }
        } else {
            if (Start + keyframe >= End) {
                lhs = End;
                rhs = Start;
            } else {
                lhs = Start + keyframe;
                rhs = Start + keyframe + 1;
            }
        }

        //UnityEngine.Debug.Log("action:" + clip.Idx + "lhs:" + lhs + " rhs:" + rhs);
        //返回较大的帧编号
        int ret = Mathf.Max(lhs, rhs);
        if (lhs != rhs) {
            float p = 0.0f;
            if (lhs < rhs) {
                p = (time - clip.KeyFrames[lhs - clip.Start]) / (clip.KeyFrames[rhs - clip.Start] - clip.KeyFrames[lhs - clip.Start]);
            } else {
                p = (time - clip.KeyFrames[lhs - clip.Start]) / (clip.KeyFrames[lhs - clip.Start] - clip.KeyFrames[rhs - clip.Start]);
            }
            BoneStatus prevPose = AmbLoader.Ins.GetBoneStatus(clip.SourceIdx, playerId, lhs);
            BoneStatus nextPose = AmbLoader.Ins.GetBoneStatus(clip.SourceIdx, playerId, rhs);
            if (prevPose == null || nextPose == null) {
                //UnityEngine.Debug.LogError("动作:" + clip.Idx + " 左侧帧:" + lhs + " 右侧帧:" + rhs + " 找不到对应的关键帧数据，动画缺失，返回");
                return lhs;
            }
            Utility.HermiteInterpolatePose(prevPose, nextPose, ref pose, p);
        }
        else {
            BoneStatus nextPose = AmbLoader.Ins.GetBoneStatus(clip.SourceIdx, playerId, rhs);
            nextPose.Clone(ref pose);
        }

        return ret;
    }
}

//[System.Serializable]
public class AttackDes {
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
public class DragDes {
    public int Start;
    public int End;
    public float Time;
    public Vector3 Color;
}

//[System.Serializable]
public class NextPose {
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
public class PosAction {
    public string Type;//"Blend/Action"
    public int Start;
    public int End;
    public float Speed;//出招速度.

}
