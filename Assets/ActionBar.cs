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
                MeteorManager.Instance.LocalPlayer.DropWeapon();
                break;
            case 2:
                //援助,要看状态机
                break;
            case 3:
                //挑衅，要看状态机
                break;
            case 4:
                //装死，要看状态机
                break;
            case 5:
                //怒绝，要看状态机
                //int target = -1;
                //int keyMap = 0;
                ////得到所有绝招的KeyMap,尝试释放。
                //if (Global.GMeteorInput.CheckPos(keyMap, target))
                //{
                //    MeteorManager.Instance.LocalPlayer.posMng.LinkAction(target);
                //    MeteorManager.Instance.LocalPlayer.AngryValue = 0;
                //    FightWnd.Instance.UpdatePlayerInfo();
                //}
                break;
        }
    }
}
