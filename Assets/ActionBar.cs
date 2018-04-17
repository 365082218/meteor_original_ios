using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *      匕首大绝-259;
        镖大绝-203;
        火枪大绝-216;
        双刺大绝-244";
        枪大绝-293";
        匕首大绝-259";
        刀大绝-310";
        爆气-367";
        剑大绝-368;
        拳套大绝-421";
        乾坤刀大绝-451";
        忍天地同寿-468";
        忍刀隐身-535";
        锤大绝-325";
        忍刀大绝-474";
 */

public class ActionBar : MonoBehaviour {

    //0丢弃，1丢弃 2援助 3挑衅 4装死 5怒气
    public void OnAction(int id)
    {
        switch (id)
        {
            case 1:
                MeteorManager.Instance.LocalPlayer.PlaySkill();
                break;
            case 2:
                //援助,要看状态机
                if (MeteorManager.Instance.LocalPlayer.posMng.mActiveAction.Idx == CommonAction.Idle)
                    MeteorManager.Instance.LocalPlayer.posMng.ChangeAction(CommonAction.Reborn);
                break;
            case 3:
                //挑衅，要看状态机
                if (MeteorManager.Instance.LocalPlayer.posMng.mActiveAction.Idx == CommonAction.Idle)
                    MeteorManager.Instance.LocalPlayer.posMng.ChangeAction(CommonAction.Taunt);
                break;
            case 4:
                //装死，要看状态机
                if (MeteorManager.Instance.LocalPlayer.posMng.mActiveAction.Idx == CommonAction.Idle)
                    MeteorManager.Instance.LocalPlayer.posMng.ChangeAction(CommonAction.Dead);
                break;
            case 5:
                //加速
                MeteorManager.Instance.LocalPlayer.Attr.AddSpeed(150);
                //MeteorManager.Instance.LocalPlayer.GetItem(5);
                break;
        }
    }
}
