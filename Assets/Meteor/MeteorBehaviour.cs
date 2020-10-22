using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

//不BB了，硬编码，很难抽象出层次优化，第一个版本就硬写代码
public class MeteorBehaviour:Singleton<MeteorBehaviour> {
    //防御，换武器，爆气3连处理.
    public bool ProcessNormalAction(MeteorUnit Owner) {
        MeteorInput Input = Owner.meteorController.Input;
        if (Input.HasInput((int)EKeyList.KL_Defence, (int)EInputType.EIT_Pressing)) {
            Owner.Defence();
            return true;
        } else
        if (Input.HasInput((int)EKeyList.KL_ChangeWeapon, (int)EInputType.EIT_Click)) {
            Owner.ChangeWeapon();
            return true;
        }
        return false;
    }

    public void MoveOnCrouch(MeteorUnit Owner, Vector2 moveVec) {
        if (moveVec.y != 0) {
            if (moveVec.y > 0 && Owner.ActionMgr.mActiveAction.Idx != CommonAction.CrouchForw)
                Owner.ActionMgr.ChangeAction(CommonAction.CrouchForw, 0.1f);
            else if (moveVec.y < 0 && Owner.ActionMgr.mActiveAction.Idx != CommonAction.CrouchBack)
                Owner.ActionMgr.ChangeAction(CommonAction.CrouchBack, 0.1f);
            return;
        }
        if (moveVec.x != 0) {
            if (moveVec.x > 0 && Owner.ActionMgr.mActiveAction.Idx != CommonAction.CrouchRight)
                Owner.ActionMgr.ChangeAction(CommonAction.CrouchRight, 0.1f);
            else if (moveVec.x < 0 && Owner.ActionMgr.mActiveAction.Idx != CommonAction.CrouchLeft)
                Owner.ActionMgr.ChangeAction(CommonAction.CrouchLeft, 0.1f);
        }
    }

    public void Move(MeteorUnit Owner, Vector2 moveVec) {
        if (moveVec.y != 0) {
            if (Owner.HasBuff((int)EBUFF_ID.DrugEx) || Owner.HasBuff((int)EBUFF_ID.Drug)) {
                if (Owner.ActionMgr.mActiveAction.Idx != CommonAction.RunOnDrug)
                    Owner.ActionMgr.ChangeAction(CommonAction.RunOnDrug, 0.1f);
            } else {
                if (moveVec.y > 0 && Owner.ActionMgr.mActiveAction.Idx != CommonAction.Run) {
                    //UnityEngine.Debug.Log("Owner.posMng.mActiveAction.Idx:" + Owner.posMng.mActiveAction.Idx);
                    Owner.ActionMgr.ChangeAction(CommonAction.Run, 0.1f);
                } else if (moveVec.y < 0 && Owner.ActionMgr.mActiveAction.Idx != CommonAction.WalkBackward)
                    Owner.ActionMgr.ChangeAction(CommonAction.WalkBackward, 0.1f);
            }
            return;
        }
        if (moveVec.x != 0) {
            if (Owner.HasBuff((int)EBUFF_ID.DrugEx) || Owner.HasBuff((int)EBUFF_ID.Drug)) {
                if (Owner.ActionMgr.mActiveAction.Idx != CommonAction.RunOnDrug)
                    Owner.ActionMgr.ChangeAction(CommonAction.RunOnDrug, 0.1f);
            } else {
                if (moveVec.x > 0 && Owner.ActionMgr.mActiveAction.Idx != CommonAction.WalkRight)
                    Owner.ActionMgr.ChangeAction(CommonAction.WalkRight, 0.1f);
                else if (moveVec.x < 0 && Owner.ActionMgr.mActiveAction.Idx != CommonAction.WalkLeft)
                    Owner.ActionMgr.ChangeAction(CommonAction.WalkLeft, 0.1f);
            }
        }
    }

    public void Jump(MeteorUnit Owner, Vector2 jumpVec) {
        Jump(Owner, jumpVec, 1.0f);
    }

    public void Jump(MeteorUnit Owner, Vector2 jumpVec, float ShortsScale) {
        Owner.SetJumpVelocity(jumpVec);
        if (jumpVec.y != 0) {
            if (jumpVec.y > 0)
                Owner.Jump(ShortsScale, CommonAction.Jump);
            else
                Owner.Jump(ShortsScale, CommonAction.JumpBack);
        } else
        if (jumpVec.x != 0) {
            if (jumpVec.x > 0)
                Owner.Jump(ShortsScale, CommonAction.JumpRight);
            else
                Owner.Jump(ShortsScale, CommonAction.JumpLeft);
        } else
        if (jumpVec == Vector2.zero)
            Owner.Jump(ShortsScale);
    }

    public void ProcessBehaviour(MeteorUnit target) {
        if (CombatData.Ins.GLevelMode == LevelMode.MultiplyPlayer && !target.Attr.IsPlayer)
            return;
        ActionManager posMng = target.ActionMgr;
        MeteorInput Input = target.meteorController.Input;
        MeteorUnit Owner = target;
        //后面改为状态机 遵循 当前动作-遍历每一个中断动作-扫描切换所需状态.符合则切换（关键是这个状态表不好生成）
        //除了idle以外还有其他预备动作，都可以随意切换
        if (posMng.mActiveAction.Idx == CommonAction.Idle ||
            ActionManager.IsReadyAction(posMng.mActiveAction.Idx))//动作是假死，角色没有挂
        {
            if (ProcessNormalAction(Owner)) {
                return;
            } else if (Input.HasInput((int)EKeyList.KL_Help, (int)EInputType.EIT_Click)) {
                //复活队友
                Owner.ActionMgr.ChangeAction(CommonAction.Reborn, 0.1f);
            } else if (Input.HasInput((int)EKeyList.KL_Defence, (int)EInputType.EIT_Pressing)) {
                Owner.Defence();
                return;
            } else
             if (Input.HasInput((int)EKeyList.KL_Crouch, (int)EInputType.EIT_Pressing)) {
                Owner.OnCrouch();
                return;
            } else if (Input.HasInput((int)EKeyList.KL_Jump, (int)EInputType.EIT_ShortRelease) && Owner.IsOnGround()) {
                //Debug.Log("EIT_ShortRelease");
                Jump(Owner, Input.mInputVector, Input.KeyStates[(int)EKeyList.KL_Jump].PressedTime * 0.32f/ MeteorInput.ShortPressTime);
            } else if (Input.HasInput((int)EKeyList.KL_Jump, (int)EInputType.EIT_FullPress) && Owner.IsOnGround()) {
                Jump(Owner, Input.mInputVector);
            } else
              if (Input.HasInput((int)EKeyList.KL_KeyW, (int)EInputType.EIT_DoubleClick)) {
                //这里要判断武器
                Owner.IdleRush();
            } else
              if (Input.HasInput((int)EKeyList.KL_KeyS, (int)EInputType.EIT_DoubleClick)) {
                //这里要判断武器
                Owner.IdleRush(RushDirection.Back);
            } else
              if (Input.HasInput((int)EKeyList.KL_KeyA, (int)EInputType.EIT_DoubleClick)) {
                //这里要判断武器
                Owner.IdleRush(RushDirection.Left);
            } else
              if (Input.HasInput((int)EKeyList.KL_KeyD, (int)EInputType.EIT_DoubleClick)) {
                //这里要判断武器
                Owner.IdleRush(RushDirection.Right);
            } else
              if (Input.mInputVector != Vector2.zero)
                Move(Owner, Input.mInputVector);
        } else if (posMng.mActiveAction.Idx == CommonAction.Crouch || (posMng.mActiveAction.Idx >= CommonAction.CrouchForw && posMng.mActiveAction.Idx <= CommonAction.CrouchBack)) {
            if (Owner.GetWeaponType() == (int)EquipWeaponType.Gun && Owner.GunReady) {
                ProcessGunAction(Owner);
                return;
            }

            //除了不能防御
            if (ProcessNormalAction(Owner)) {
                return;
            } else
            if (Input.HasInput((int)EKeyList.KL_Crouch, (int)EInputType.EIT_Releasing)) {
                posMng.ChangeAction(CommonAction.Idle, 0.1f);
            } else if (Input.HasInput((int)EKeyList.KL_Jump, (int)EInputType.EIT_ShortRelease) && Owner.IsOnGround()) {
                Jump(Owner, Input.mInputVector, Input.KeyStates[(int)EKeyList.KL_Jump].PressedTime * 0.32f / MeteorInput.ShortPressTime);
            } else if (Input.HasInput((int)EKeyList.KL_Jump, (int)EInputType.EIT_FullPress) && Owner.IsOnGround()) {
                Jump(Owner, Input.mInputVector);
            } else
              if (Input.HasInput((int)EKeyList.KL_KeyW, (int)EInputType.EIT_DoubleClick)) {
                //这里要判断武器
                Owner.CrouchRush();
            } else
              if (Input.HasInput((int)EKeyList.KL_KeyS, (int)EInputType.EIT_DoubleClick)) {
                //这里要判断武器
                Owner.CrouchRush(RushDirection.Back);
            } else
              if (Input.HasInput((int)EKeyList.KL_KeyA, (int)EInputType.EIT_DoubleClick)) {
                //这里要判断武器
                Owner.CrouchRush(RushDirection.Left);
            } else
              if (Input.HasInput((int)EKeyList.KL_KeyD, (int)EInputType.EIT_DoubleClick)) {
                //这里要判断武器
                Owner.CrouchRush(RushDirection.Right);
            } else if (Input.mInputVector != Vector2.zero) {
                MoveOnCrouch(Owner, Input.mInputVector);
            } else if (Input.mInputVector == Vector2.zero && !posMng.Rotateing) {
                if (posMng.mActiveAction.Idx != CommonAction.Crouch)
                    posMng.ChangeAction(CommonAction.Crouch, 0.1f);
            }
        } else if (posMng.mActiveAction.Idx == CommonAction.Run || posMng.mActiveAction.Idx == CommonAction.RunOnDrug) {
            if (Input.HasInput((int)EKeyList.KL_Help, (int)EInputType.EIT_Click)) {
                //复活队友
                Owner.ActionMgr.ChangeAction(CommonAction.Reborn, 0.1f);
            } else if (Input.HasInput((int)EKeyList.KL_Crouch, (int)EInputType.EIT_Pressing)) {
                Owner.OnCrouch();
            } else
            if (Input.HasInput((int)EKeyList.KL_KeyW, (int)EInputType.EIT_DoubleClick)) {
                Owner.IdleRush();
            } else
            if (Input.HasInput((int)EKeyList.KL_KeyS, (int)EInputType.EIT_DoubleClick)) {
                //这里要判断武器
                Owner.IdleRush(RushDirection.Back);
            } else
            if (Input.HasInput((int)EKeyList.KL_KeyA, (int)EInputType.EIT_DoubleClick)) {
                //这里要判断武器
                Owner.IdleRush(RushDirection.Left);
            } else
            if (Input.HasInput((int)EKeyList.KL_KeyD, (int)EInputType.EIT_DoubleClick)) {
                //这里要判断武器
                Owner.IdleRush(RushDirection.Right);
            } else
            if (Input.mInputVector == Vector2.zero)
                posMng.ChangeAction(0, 0.1f);
            else
            if (Input.HasInput((int)EKeyList.KL_Jump, (int)EInputType.EIT_ShortRelease) && Owner.IsOnGround()) {
                Jump(Owner, Input.mInputVector, Input.KeyStates[(int)EKeyList.KL_Jump].PressedTime * 0.32f / MeteorInput.ShortPressTime);
            } else if (Input.HasInput((int)EKeyList.KL_Jump, (int)EInputType.EIT_FullPress) && Owner.IsOnGround()) {
                Jump(Owner, Input.mInputVector);
            } else
              if (ProcessNormalAction(Owner)) {
                return;
            } else if (Input.mInputVector != Vector2.zero)
                Move(Owner, Input.mInputVector);

        } else if (posMng.mActiveAction.Idx == CommonAction.WalkLeft) {
            if (Input.HasInput((int)EKeyList.KL_Crouch, (int)EInputType.EIT_Pressing)) {
                Owner.OnCrouch();
            } else
            if (Input.HasInput((int)EKeyList.KL_Jump, (int)EInputType.EIT_ShortRelease) && Owner.IsOnGround()) {
                Jump(Owner, Input.mInputVector, Input.KeyStates[(int)EKeyList.KL_Jump].PressedTime * 0.32f/ MeteorInput.ShortPressTime);
            } else if (Input.HasInput((int)EKeyList.KL_Jump, (int)EInputType.EIT_FullPress) && Owner.IsOnGround()) {
                Jump(Owner, Input.mInputVector);
            } else
              if (Input.HasInput((int)EKeyList.KL_KeyW, (int)EInputType.EIT_DoubleClick)) {
                Owner.IdleRush();
            } else
              if (Input.HasInput((int)EKeyList.KL_KeyS, (int)EInputType.EIT_DoubleClick)) {
                //这里要判断武器
                Owner.IdleRush(RushDirection.Back);
            } else
              if (Input.HasInput((int)EKeyList.KL_KeyA, (int)EInputType.EIT_DoubleClick)) {
                //这里要判断武器
                Owner.IdleRush(RushDirection.Left);
            } else
              if (Input.HasInput((int)EKeyList.KL_KeyD, (int)EInputType.EIT_DoubleClick)) {
                //这里要判断武器
                Owner.IdleRush(RushDirection.Right);
            } else if (!posMng.Rotateing && Input.mInputVector == Vector2.zero)
                posMng.ChangeAction(CommonAction.Idle, 0.1f);
            else if (ProcessNormalAction(Owner)) {
                return;
            } else if (Input.mInputVector != Vector2.zero)
                Move(Owner, Input.mInputVector);
        } else if (posMng.mActiveAction.Idx == CommonAction.WalkRight) {
            if (Input.HasInput((int)EKeyList.KL_Crouch, (int)EInputType.EIT_Pressing)) {
                Owner.OnCrouch();
            } else
            if (Input.HasInput((int)EKeyList.KL_Jump, (int)EInputType.EIT_ShortRelease) && Owner.IsOnGround()) {
                Jump(Owner, Input.mInputVector, Input.KeyStates[(int)EKeyList.KL_Jump].PressedTime * 0.32f/ MeteorInput.ShortPressTime);
            } else
            if (Input.HasInput((int)EKeyList.KL_Jump, (int)EInputType.EIT_FullPress) && Owner.IsOnGround()) {
                Jump(Owner, Input.mInputVector);
            } else
            if (Input.HasInput((int)EKeyList.KL_KeyW, (int)EInputType.EIT_DoubleClick)) {
                Owner.IdleRush();
            } else
            if (Input.HasInput((int)EKeyList.KL_KeyS, (int)EInputType.EIT_DoubleClick)) {
                //这里要判断武器
                Owner.IdleRush(RushDirection.Back);
            } else
            if (Input.HasInput((int)EKeyList.KL_KeyA, (int)EInputType.EIT_DoubleClick)) {
                //这里要判断武器
                Owner.IdleRush(RushDirection.Left);
            } else
            if (Input.HasInput((int)EKeyList.KL_KeyD, (int)EInputType.EIT_DoubleClick)) {
                //这里要判断武器
                Owner.IdleRush(RushDirection.Right);
            } else if (!posMng.Rotateing && Input.mInputVector == Vector2.zero)
                posMng.ChangeAction(CommonAction.Idle, 0.1f);
            else if (ProcessNormalAction(Owner)) {
                return;
            } else if (Input.mInputVector != Vector2.zero)
                Move(Owner, Input.mInputVector);
        } else if (posMng.mActiveAction.Idx == CommonAction.WalkBackward) {
            if (Input.HasInput((int)EKeyList.KL_Crouch, (int)EInputType.EIT_Pressing)) {
                Owner.OnCrouch();
            } else
            if (Input.HasInput((int)EKeyList.KL_Jump, (int)EInputType.EIT_ShortRelease) && Owner.IsOnGround()) {
                Jump(Owner, Input.mInputVector, Input.KeyStates[(int)EKeyList.KL_Jump].PressedTime * 0.32f / MeteorInput.ShortPressTime);
                //Debug.LogError("jumpback");
            } else
            if (Input.HasInput((int)EKeyList.KL_Jump, (int)EInputType.EIT_FullPress) && Owner.IsOnGround()) {
                Jump(Owner, Input.mInputVector);
            } else
            if (Input.HasInput((int)EKeyList.KL_KeyW, (int)EInputType.EIT_DoubleClick)) {
                Owner.IdleRush();
            } else
            if (Input.HasInput((int)EKeyList.KL_KeyS, (int)EInputType.EIT_DoubleClick)) {
                //这里要判断武器
                Owner.IdleRush(RushDirection.Back);
            } else
            if (Input.HasInput((int)EKeyList.KL_KeyA, (int)EInputType.EIT_DoubleClick)) {
                //这里要判断武器
                Owner.IdleRush(RushDirection.Left);
            } else
            if (Input.HasInput((int)EKeyList.KL_KeyD, (int)EInputType.EIT_DoubleClick)) {
                //这里要判断武器
                Owner.IdleRush(RushDirection.Right);
            } else if (!posMng.Rotateing && Input.mInputVector == Vector2.zero)
                posMng.ChangeAction(CommonAction.Idle, 0.1f);
            else if (ProcessNormalAction(Owner)) {
                return;
            } else if (Input.mInputVector != Vector2.zero)
                Move(Owner, Input.mInputVector);
        } else if ((posMng.mActiveAction.Idx >= CommonAction.BrahchthrustDefence
              && posMng.mActiveAction.Idx <= CommonAction.HammerDefence) ||
              (posMng.mActiveAction.Idx >= CommonAction.ZhihuDefence
              && posMng.mActiveAction.Idx <= CommonAction.RendaoDefence)) {
            //还有乾坤刀的其他2种姿势没处理
            if (Input.HasInput((int)EKeyList.KL_Defence, (int)EInputType.EIT_Releasing)) {
                Owner.ReleaseDefence();
            }
        } else if (posMng.mActiveAction.Idx == CommonAction.Struggle || posMng.mActiveAction.Idx == CommonAction.Struggle0 || posMng.mActiveAction.Idx == CommonAction.Dead)//地面挣扎.僵直中无法输入.
          {
            if (Owner.IsOnGround()) {
                if (Input.HasInput((int)EKeyList.KL_KeyW, (int)EInputType.EIT_Release)) {
                    posMng.ChangeAction(CommonAction.DCForw, 0.1f);
                } else if (Input.HasInput((int)EKeyList.KL_KeyS, (int)EInputType.EIT_Release)) {
                    posMng.ChangeAction(CommonAction.DCBack, 0.1f);
                } else if (Input.HasInput((int)EKeyList.KL_KeyA, (int)EInputType.EIT_Release)) {
                    posMng.ChangeAction(CommonAction.DCLeft, 0.1f);
                } else if (Input.HasInput((int)EKeyList.KL_KeyD, (int)EInputType.EIT_Release)) {
                    posMng.ChangeAction(CommonAction.DCRight, 0.1f);
                } else if (Input.HasInput((int)EKeyList.KL_Jump, (int)EInputType.EIT_ShortRelease)) {
                    Jump(Owner, Input.mInputVector, Input.KeyStates[(int)EKeyList.KL_Jump].PressedTime * 0.32f / MeteorInput.ShortPressTime);
                } else if (Input.HasInput((int)EKeyList.KL_Jump, (int)EInputType.EIT_FullPress)) {
                    Jump(Owner, Input.mInputVector);
                } else if (Input.HasInput((int)EKeyList.KL_Defence, (int)EInputType.EIT_Click)) {
                    if (posMng.mActiveAction.Idx == CommonAction.Struggle)
                        Owner.ActionMgr.ChangeAction(CommonAction.IdleBack, 0.1f);
                    else if (posMng.mActiveAction.Idx == CommonAction.Struggle0)
                        Owner.ActionMgr.ChangeAction(CommonAction.IdleFront, 0.1f);
                }
            }
        } else if (Owner.Climbing) {
            if (Input.HasInput((int)EKeyList.KL_KeyW, (int)EInputType.EIT_Release)) {
                //Owner.ProcessFall();
                //Owner.meteorController.LockMove(true);
                Owner.ActionMgr.QueueAction(CommonAction.JumpFall, PoseEvt.Fall);
            } else if (Owner.ImpluseVec.y > 0 && Owner.OnTouchWall) {
                if (Input.HasInput((int)EKeyList.KL_Jump, (int)EInputType.EIT_Click)) {
                    //Debug.LogError("ProcessTouchWallJump");
                    Owner.ProcessTouchWallJump(true);
                }
            } else if (Input.HasInput((int)EKeyList.KL_Jump, (int)EInputType.EIT_Click)) {
                //Debug.LogError("ProcessJump2");
                Owner.ProcessJump2();
            }
        } else if (posMng.mActiveAction.Idx >= CommonAction.Jump && posMng.mActiveAction.Idx <= CommonAction.WallLeftJump) {
            if (Input.HasInput((int)EKeyList.KL_ChangeWeapon, (int)EInputType.EIT_Click)) {
                Owner.ChangeWeapon();
            }  else if (Owner.ImpluseVec.y > 0) {
                if (Input.HasInput((int)EKeyList.KL_Jump, (int)EInputType.EIT_Click)) {
                    Owner.ProcessJump2();
                }
            }
        } else if (posMng.mActiveAction.Idx == CommonAction.GunIdle) {
            ProcessGunAction(Owner);
        } else if (posMng.IsHurtPose() &&
              Input.HasInput((int)EKeyList.KL_BreakOut, (int)EInputType.EIT_Click)) {
            Owner.DoBreakOut();
        }
    }

    void ProcessGunAction(MeteorUnit target) {
        ActionManager posMng = target.ActionMgr;
        MeteorInput Input = target.meteorController.Input;
        MeteorUnit Owner = target;
        //响应
        if (Input.HasInput((int)EKeyList.KL_BreakOut, (int)EInputType.EIT_Click)) {
            Owner.SetGunReady(false);
            Owner.DoBreakOut();
        } else//除了跳/受击/爆气 清了枪蹲下的状态，其他全部不能清理这个状态
        if (Input.HasInput((int)EKeyList.KL_Jump, (int)EInputType.EIT_ShortRelease) && Owner.IsOnGround()) {
            Owner.SetGunReady(false);
            Owner.SetGround(false);
            Jump(Owner, Input.mInputVector, Input.KeyStates[(int)EKeyList.KL_Jump].PressedTime * 0.32f / MeteorInput.ShortPressTime);
        } else if (Input.HasInput((int)EKeyList.KL_Jump, (int)EInputType.EIT_FullPress) && Owner.IsOnGround()) {
            Owner.SetGunReady(false);
            Owner.SetGround(false);
            Jump(Owner, Input.mInputVector);
        } else
          if (Input.HasInput((int)EKeyList.KL_KeyW, (int)EInputType.EIT_DoubleClick)) {
            //这里要判断武器
            Owner.CrouchRush();
        } else
          if (Input.HasInput((int)EKeyList.KL_KeyS, (int)EInputType.EIT_DoubleClick)) {
            //这里要判断武器
            Owner.CrouchRush(RushDirection.Back);
        } else
          if (Input.HasInput((int)EKeyList.KL_KeyA, (int)EInputType.EIT_DoubleClick)) {
            //这里要判断武器
            Owner.CrouchRush(RushDirection.Left);
        } else
          if (Input.HasInput((int)EKeyList.KL_KeyD, (int)EInputType.EIT_DoubleClick)) {
            //这里要判断武器
            Owner.CrouchRush(RushDirection.Right);
        } else if (Input.mInputVector != Vector2.zero) {
            MoveOnCrouch(Owner, Input.mInputVector);
        } else if (Input.mInputVector == Vector2.zero && !posMng.Rotateing) {
            if (posMng.mActiveAction.Idx != CommonAction.GunIdle)
                posMng.ChangeAction(CommonAction.GunIdle, 0.1f);
        }
    }
}
