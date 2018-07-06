using Idevgame.Debug;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DebugClass("MeteorUnit")]
public partial class MeteorUnit: DebugInstance
{
    public string Name { get { return name; } }
    int i = 0;
    [DebugMethod("面向下一个目标")]
    public void FacetoNextTarget()
    {
        i %= MeteorManager.Instance.UnitInfos.Count;
        if (MeteorManager.Instance.UnitInfos[i] == this)
        {
            i++;
            i %= MeteorManager.Instance.UnitInfos.Count;
        }
        FaceToTarget(MeteorManager.Instance.UnitInfos[i]);
        WSLog.Print(string.Format("朝向:{0}", MeteorManager.Instance.UnitInfos[i].name));
        i++;
    }

    [DebugMethod("待机")]
    public void StopPerform()
    {
        if (GameBattleEx.Instance != null)
            GameBattleEx.Instance.StopAction(InstanceId);
        if (robot != null)
        {
            robot.ResetAIVelocity();
            posMng.ChangeAction(0);
            robot.ChangeState(EAIStatus.Wait);
        }
    }

    [DebugMethod("关闭/打开AI")]
    public void AIPause()
    {
        if (GameBattleEx.Instance != null)
            GameBattleEx.Instance.StopAction(InstanceId);
        if (robot != null)
        {
            robot.ResetAIVelocity();
            robot.EnableAI(robot.stoped);
        }
    }
}
