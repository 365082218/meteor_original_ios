using protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CheatCode {
    //使用作弊码
    public static bool UseCheatCode(string cheatcode) {
        bool ret = false;
        int param1 = -1;
        int param2 = -1;
        string stringparam = "";
        Vector3 pos = Vector3.zero;
        if (CheatOK(cheatcode, "check")) {
            U3D.ShowTargetBlood();
            return true;
        }
        else if (CheatOK(cheatcode, "skill")) {
            if (U3D.IsMultiplyPlayer())
                return false;
            if (FightState.Exist()) {
                FightState.Instance.ShowSkillBar();
            }
            return true;
        }
        else if (CheatParam2(cheatcode, "follow", ref param1, ref param2)) {
            MeteorUnit unit = U3D.GetUnit(param1 - 1);
            if (unit != null && unit.StateMachine != null) {
                MeteorUnit target = U3D.GetUnit(param2 - 1);
                U3D.InsertSystemMsg(string.Format("{0}开始跟随{1}", unit.name, target.name));
                unit.FollowTarget = target;
            }
            return true;
        }
        else if (CheatParam2(cheatcode, "chase", ref param1, ref param2)) {
            MeteorUnit unit = U3D.GetUnit(param1 - 1);
            if (unit != null) {
                MeteorUnit target = U3D.GetUnit(param2 - 1);
                if (target != null && !target.SameCamp(unit)) {
                    U3D.InsertSystemMsg(string.Format("{0}开始追杀{1}", unit.name, target.name));
                    unit.Kill(target);
                }
            }
            return true;
        }
        else if (CheatParam(cheatcode, "wait", ref param1)){
            MeteorUnit unit = U3D.GetUnit(param1 - 1);
            if (unit != null && unit.StateMachine != null) {
                U3D.InsertSystemMsg(string.Format("{0}空闲下来了", unit.name));
                unit.StateMachine.ChangeState(unit.StateMachine.WaitState);
            }
            return true;
        }
        else if (CheatOK(cheatcode, "god")) {
            U3D.GodLike();
            ret = true;
        } else if (CheatOK(cheatcode, "box")) {
            U3D.Box();
            ret = true;
        } else if (CheatParam(cheatcode, "kill", ref param1)) {
            if (U3D.IsMultiplyPlayer()) {
                if (FrameReplay.Ins.Started) {
                    MeteorUnit unit = U3D.GetUnit(param1 - 1);
                    if (unit == null) {
                        return false;
                    }
                    OperateMsg msg = new OperateMsg();
                    msg.Operate = (int)OperateType.Kill;
                    msg.KillTarget = (uint)unit.InstanceId;
                    FrameSyncServer.Ins.NetEvent(MeteorMsg.Command.Kill, msg);
                }
            } else {
                U3D.Kill(param1 - 1);
                MeteorUnit u = U3D.GetUnit(param1 - 1);
                if (u != null) {
                    U3D.InsertSystemMsg(string.Format("{0}遭遇击杀", u.name));
                }
            }
            ret = true;
        } else if (CheatParam(cheatcode, "pose", ref param1)) {
            if (U3D.IsMultiplyPlayer())
                return false;
            if (Main.Ins.LocalPlayer != null && Main.Ins.LocalPlayer.ActionMgr != null)
                Main.Ins.LocalPlayer.ActionMgr.ChangeAction(param1, 0.1f);
            ret = true;
        } else if (CheatParam(cheatcode, "pause", ref param1)) {
            MeteorUnit unit = U3D.GetUnit(param1 - 1);
            if (unit != null && unit.StateMachine != null) {
                unit.AIPause(true, float.MaxValue);
                U3D.InsertSystemMsg(string.Format("{0}无法动弹", unit.name));
            }
            ret = true;
        } else if (CheatParam(cheatcode, "resume", ref param1)) {
            MeteorUnit unit = U3D.GetUnit(param1 - 1);
            if (unit != null && unit.StateMachine != null) {
                unit.AIPause(false, 0);
                U3D.InsertSystemMsg(string.Format("{0}恢复了", unit.name));
            }
            ret = true;
        } else if (CheatParam(cheatcode, "kick", ref param1)) {
            if (U3D.IsMultiplyPlayer()) {
                //检查自己是否房间主人，是则向主机发送指令
                if (FrameReplay.Ins.Started) {
                    MeteorUnit unit = U3D.GetUnit(param1 - 1);
                    if (unit == null) {
                        return false;
                    }
                    OperateMsg msg = new OperateMsg();
                    msg.Operate = (int)OperateType.Kick;
                    msg.KillTarget = (uint)unit.InstanceId;
                    FrameSyncServer.Ins.NetEvent(MeteorMsg.Command.Kick, msg);
                }
            }
            ret = true;
        } else if (CheatParam(cheatcode, "skick", ref param1)) {
            if (U3D.IsMultiplyPlayer()) {
                //检查自己是否房间主人，是则向主机发送指令
                if (FrameReplay.Ins.Started) {
                    MeteorUnit unit = U3D.GetUnit(param1 - 1);
                    if (unit == null) {
                        return false;
                    }
                    OperateMsg msg = new OperateMsg();
                    msg.Operate = (int)OperateType.Skick;
                    msg.KillTarget = (uint)unit.InstanceId;
                    FrameSyncServer.Ins.NetEvent(MeteorMsg.Command.Skick, msg);
                }
            }
            ret = true;
        } else if (CheatParamString(cheatcode, "ai", ref stringparam)) {
            if (U3D.IsMultiplyPlayer()) {
                return false;
            }
            if (Main.Ins.GameBattleEx != null) {
                int r = U3D.AddNPC(stringparam);
                ret = r > 0;
            }
            return true;
        } else if (CheatOK(cheatcode, "fps")) {
            bool show = GameStateMgr.Ins.gameStatus.ShowFPS;
            show = !show;
            Main.Ins.ShowFps(show);
            return true;
        } else if (CheatOK(cheatcode, "position")) {
            if (FightState.Exist()) {
                FightState.Instance.ShowPosition();
            }
            return true;
        } else if (CheatOK(cheatcode, "mhp")) {
            //不支持这个指令了.
            //if (FightState.Exist()) {
            //    FightState.Instance.ShowPlayerInfo();
            //}
            return true;
        } else if (CheatOK(cheatcode, "mnet") || CheatOK("cheatcode", "serverinfo")) {
            //显示网络数据传送情况
            return true;
        } else if (CheatOK(cheatcode, "quit")) {
            if (U3D.IsMultiplyPlayer())
                return false;
            Application.Quit();
            return true;
        } else if (CheatParamVector3(cheatcode, "moveto", ref pos)) {
            if (Main.Ins.LocalPlayer != null && !Main.Ins.LocalPlayer.Dead) {
                Main.Ins.LocalPlayer.SetPosition(pos);
            }
            return true;
        } else if (CheatParam(cheatcode, "ang", ref param1)) {
            if (U3D.IsMultiplyPlayer()) {
                return false;
            }
            MeteorUnit target = U3D.GetUnit(param1 - 1);
            if (target != null) {
                target.angryMax = !target.angryMax;
                U3D.InsertSystemMsg(string.Format("{0}真气{1}", target.name, target.angryMax ? "满":"空"));
            }
            return true;
        } else if (CheatParam(cheatcode, "weapon", ref param1)) {
            if (Main.Ins.LocalPlayer != null) {
                InventoryItem w = GameStateMgr.Ins.MakeEquip(param1);
                if (w == null) {
                    U3D.InsertSystemMsg("找不到编号为:" + param1 + "的武器");
                    return true;
                }
                Main.Ins.LocalPlayer.ChangeWeaponCode(param1);
            }
            return true;
        } else if (CheatParam(cheatcode, "use", ref param1)) {
            if (U3D.IsMultiplyPlayer()) {
                return false;
            }
            if (Main.Ins.LocalPlayer != null) {
                Option it = MenuResLoader.Ins.GetItemInfo(param1);
                if (it == null) {
                    U3D.InsertSystemMsg("找不到编号为:" + param1 + "的物品");
                    return true;
                }
                Main.Ins.LocalPlayer.GetItem(param1);
            }
            return true;
        } else if (CheatParam(cheatcode, "drop", ref param1)) {
            if (U3D.IsMultiplyPlayer()) {
                if (Main.Ins.LocalPlayer != null) {
                    MeteorUnit player = Main.Ins.LocalPlayer;
                    DropMsg msg = new DropMsg();
                    msg.forward = new _Vector3();
                    msg.forward.x = Mathf.FloorToInt(player.transform.forward.x * 1000);
                    msg.forward.y = Mathf.FloorToInt(player.transform.forward.y * 1000);
                    msg.forward.z = Mathf.FloorToInt(player.transform.forward.z * 1000);
                    msg.position = new _Vector3();
                    msg.position.x = Mathf.FloorToInt(player.transform.position.x * 1000);
                    msg.position.y = Mathf.FloorToInt(player.transform.position.y * 1000);
                    msg.position.z = Mathf.FloorToInt(player.transform.position.z * 1000);
                    msg.item = (uint)param1;
                    FrameSyncServer.Ins.NetEvent(MeteorMsg.Command.Drop, msg);
                }
                return true;
            }
            if (Main.Ins.LocalPlayer != null) {
                Option it = MenuResLoader.Ins.GetItemInfo(param1);
                if (it == null) {
                    U3D.InsertSystemMsg("找不到编号为:" + param1 + "的物品");
                    return true;
                }
                DropMng.Ins.DropItem(Main.Ins.LocalPlayer, it);
            }
            return true;
        } else if (CheatOK(cheatcode, "win")) {
            if (U3D.IsMultiplyPlayer()) {
                return false;
            }
            if (Main.Ins.GameBattleEx != null) {
                Main.Ins.GameBattleEx.GameOver(1);
            }
            return true;
        } else if (CheatOK(cheatcode, "lose")) {
            if (U3D.IsMultiplyPlayer()) {
                return false;
            }
            if (Main.Ins.GameBattleEx != null) {
                Main.Ins.GameBattleEx.GameOver(-1);
            }
        }
        return ret;
    }

    static bool CheatOK(string cheat, string code) {
        if (cheat.ToUpper() == code || cheat.ToLower() == code)
            return true;
        return false;
    }

    static bool CheatParam(string cheat, string code, ref int p) {
        string c = cheat.ToLower();
        if (c.StartsWith(code)) {
            string[] cmd = c.Split(' ');
            if (cmd.Length >= 2) {
                if (cmd[0] == code) {
                    if (int.TryParse(cmd[1], out p))
                        return true;
                }
            }
        }
        return false;
    }

    static bool CheatParam2(string cheat, string code, ref int p, ref int q) {
        string c = cheat.ToLower();
        if (c.StartsWith(code)) {
            string[] cmd = c.Split(' ');
            if (cmd.Length >= 3) {
                if (cmd[0] == code) {
                    if (int.TryParse(cmd[1], out p) && int.TryParse(cmd[2], out q))
                        return true;
                }
            }
        }
        return false;
    }

    static bool CheatParamString(string cheat, string code, ref string p) {
        string c = cheat.ToLower();
        if (c.StartsWith(code)) {
            string[] cmd = c.Split(' ');
            if (cmd.Length >= 2) {
                if (cmd[0] == code) {
                    p = cmd[1];
                    return true;
                }
            }
        }
        return false;
    }

    static bool CheatParamVector3(string cheat, string code, ref Vector3 pos) {
        string c = cheat.ToLower();
        float x, y, z = 0;
        if (c.StartsWith(code)) {
            string[] cmd = c.Split(' ');
            if (cmd[0] == code && cmd.Length == 4) {
                if (float.TryParse(cmd[1], out x) && float.TryParse(cmd[2], out y) && float.TryParse(cmd[3], out z)) {
                    pos.x = x;
                    pos.y = y;
                    pos.z = z;
                    return true;
                }
            }
        }
        return false;
    }
}
