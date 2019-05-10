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
        //WSLog.Print(string.Format("朝向:{0}", MeteorManager.Instance.UnitInfos[i].name));
        i++;
    }

    [DebugMethod("待机")]
    public void StopPerform()
    {
        if (GameBattleEx.Instance != null)
            GameBattleEx.Instance.StopAction(InstanceId);
        if (Robot != null)
        {
            Robot.StopMove();
            posMng.ChangeAction(0);
            Robot.ChangeState(EAIStatus.Wait);
        }
    }

    [DebugMethod("关闭/打开AI")]
    public void AIEnable()
    {
        if (GameBattleEx.Instance != null)
            GameBattleEx.Instance.StopAction(InstanceId);
        if (Robot != null)
        {
            Robot.StopMove();
            Robot.EnableAI(Robot.stoped);
        }
    }

    [DebugMethod("血量/减少500")]
    public void ReduceHP5000()
    {
        OnDamage(null, 5000);
    }

    [DebugMethod("血量/减少100")]
    public void ReduceHP1000()
    {
        OnDamage(null, 1000);
    }

    [DebugMethod("血量/减少50")]
    public void ReduceHP500()
    {
        OnDamage(null, 500);
    }

    [DebugMethod("血量/减少10")]
    public void ReduceHP100()
    {
        OnDamage(null, 100);
    }

}
