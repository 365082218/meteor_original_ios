using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public enum WeaponPos {
    Pos = 0,//太刀-l,r 左手柄，右手刀
    PosA = 1,//持枪-a-左手
    PosB = 2,//居合-b-左手
}
public enum FrameEvent
{
    ChangeWeaponPos0 = 430,//430拔刀切换持枪
    ChangeWeaponPos1 = 431,//431拔刀切换居合
    ChangeWeaponPos2 = 440,//440持枪切换拔刀
    ChangeWeaponPos3 = 441,//441持枪切换居合
    ChangeWeaponPos4 = 447,//447居合切换拔刀
    ChangeWeaponPos5 = 448,//448居合切换持枪
    ChangeWeapon0 = 412,//左打虎
    ChangeWeapon1 = 413,//右打虎
    ChangeWeaponPos6 = 523,//在第一个动作第一帧切换为拔刀
    ChangeWeaponPos7 = 486,//最后一帧，切换为居合
    ChangeWeaponPos8 = 487,//最后一帧，切换为居合
    ChangeWeaponPos449_0 = 449,//拔刀术，前
    ChangeWeaponPos450_0 = 450,//拔刀术，后
    ChangeWeaponPose570_0 = 570,//勾魂锁-拔刀-持枪
    ChangeWeaponPose451_0 = 451,//居合-大招-切换为拔刀
    ChangeWeaponPose487_2 = 487,//乾坤大招完毕-最后一帧切换为收刀
    RebornSelectFriend = 22,//运气出招第一帧播放复活特效.
    RebornFriend = 34,//运气收招最后一帧复活同伴.
}

//处理一些动作得特殊事件，类似乾坤刀切换姿态，拳套快速切换武器等.
//还处理空中下A时，从空中某个帧处开始开启重力，但是在动作播放前一段时忽略重力.
//大刀下A POSE 298忽略重力一直到，5975帧开启重力
public class ActionEvent
{
    public static List<int> LastEvents;
    public static List<int> FirstEvents;
    public static List<int> TryEvents;
    public static int TryHandlerLastActionFrame(MeteorUnit owner, int Action)
    {
        if (TryEvents == null)
        {
            TryEvents = new List<int>() { (int)FrameEvent.ChangeWeapon0, (int)FrameEvent.ChangeWeapon1, (int)FrameEvent.ChangeWeaponPos0,
            (int)FrameEvent.ChangeWeaponPos1, (int)FrameEvent.ChangeWeaponPos5,
                (int)FrameEvent.ChangeWeaponPose570_0, (int)FrameEvent.ChangeWeaponPose487_2, (int)FrameEvent.ChangeWeaponPose451_0, (int)FrameEvent.ChangeWeaponPos449_0, (int)FrameEvent.ChangeWeaponPos450_0,
            (int)FrameEvent.ChangeWeaponPos6, (int)FrameEvent.ChangeWeaponPos7, (int)FrameEvent.ChangeWeaponPos8};
        }

        if (!TryEvents.Contains(Action))
            return -1;

        switch ((FrameEvent)Action)
        {
            //0拔刀1长枪2居合
            case FrameEvent.ChangeWeaponPos0:
                return 1;
            //case FrameEvent.ChangeWeaponPos2:
            //    owner.ChangeWeaponPos(0);
            //    break;
            //case FrameEvent.ChangeWeaponPos3:
            //    owner.ChangeWeaponPos(2);
            //    break;
            case FrameEvent.ChangeWeaponPos5:
                return 1;
            case FrameEvent.ChangeWeapon0://左侧换兵
            case FrameEvent.ChangeWeapon1://右侧换兵
                return owner.GetNextWeaponType();//这个连不连的上还不清楚.
            case FrameEvent.ChangeWeaponPose570_0:
                return 1;
            case FrameEvent.ChangeWeaponPose451_0:
                return 0;
            case FrameEvent.ChangeWeaponPos1://第一帧切换为拔刀，最后接485，在485最后一帧切换为居合
            case FrameEvent.ChangeWeaponPos449_0://第一帧切换为拔刀，最后接485，在485最后一帧切换为居合
            case FrameEvent.ChangeWeaponPos450_0://第一帧切换为拔刀，最后接485，在485最后一帧切换为居合
            case FrameEvent.ChangeWeaponPos6://第一帧切换为拔刀，最后接485，在485最后一帧切换为居合
            case FrameEvent.ChangeWeaponPos7://523->485 449->485 450->485
            case FrameEvent.ChangeWeaponPos8://456->487 居合
                return 2;//前拔刀，后拔刀，中断时
        }
        return -1;
    }

    //处理动作第一帧要做的事,一般都是切换武器POSE，乾坤刀的.
    public static void HandlerFirstActionFrame(MeteorUnit owner, int Action)
    {
        if (FirstEvents == null)
            FirstEvents = new List<int>() { (int)FrameEvent.ChangeWeaponPos2, (int)FrameEvent.ChangeWeaponPos3, (int)FrameEvent.ChangeWeaponPos5,(int)FrameEvent.ChangeWeaponPos4,
            (int)FrameEvent.ChangeWeaponPos449_0, (int)FrameEvent.ChangeWeaponPos450_0,(int)FrameEvent.ChangeWeaponPos6, (int)FrameEvent.ChangeWeaponPose570_0,(int)FrameEvent.RebornSelectFriend
             };
        if (!FirstEvents.Contains(Action))
            return;
        switch ((FrameEvent)Action)
        {
            case FrameEvent.ChangeWeaponPos6:
                owner.ChangeWeaponPos(0);
                break;
            case FrameEvent.ChangeWeaponPos2:
                owner.ChangeWeaponPos(0);
                break;
            case FrameEvent.ChangeWeaponPos3:
                owner.ChangeWeaponPos(0);
                break;
            case FrameEvent.ChangeWeaponPos5:
                owner.ChangeWeaponPos(WeaponPos.Pos);
                break;
            case FrameEvent.ChangeWeaponPos449_0:
                owner.ChangeWeaponPos(0);
                break;
            case FrameEvent.ChangeWeaponPos450_0:
                owner.ChangeWeaponPos(0);
                break;
            case FrameEvent.ChangeWeaponPos4:
                owner.ChangeWeaponPos(0);
                break;
            case FrameEvent.ChangeWeaponPose570_0:
                owner.ChangeWeaponPos(0);
                break;
            case FrameEvent.RebornSelectFriend:
                owner.SelectRebornTarget();
                break;
        }
    }
    
    //部分动作事件
    public static void HandlerPoseAction(MeteorUnit owner, int action)
    {
        switch (action)
        {
            case (int)FrameEvent.ChangeWeaponPos7://收刀切换为居合最后一帧
            case (int)FrameEvent.ChangeWeaponPos8://收刀切换为居合
                owner.ChangeWeaponPos(WeaponPos.PosB);
                break;
        }
    }

    //处理动作最后一帧要做的事
    public static void HandlerFinalActionFrame(MeteorUnit owner, int Action)
    {
        if (LastEvents == null)
        {
            LastEvents = new List<int>() { (int)FrameEvent.ChangeWeapon0, (int)FrameEvent.ChangeWeapon1, (int)FrameEvent.ChangeWeaponPos0, (int)FrameEvent.ChangeWeaponPos5,
                (int)FrameEvent.ChangeWeaponPose570_0, (int)FrameEvent.ChangeWeaponPose451_0, (int)FrameEvent.ChangeWeaponPos7, (int) FrameEvent.RebornFriend };
        }

        if (!LastEvents.Contains(Action))
            return;

        switch ((FrameEvent)Action)
        {
            case FrameEvent.ChangeWeaponPos7://收刀切换为居合最后一帧
                owner.ChangeWeaponPos(WeaponPos.PosB);
                break;
            //0拔刀1长枪2居合
            case FrameEvent.ChangeWeaponPos0:
                owner.ChangeWeaponPos(WeaponPos.PosA);
                //owner.ChangeWeaponPos(2);
                break;

                //case FrameEvent.ChangeWeaponPos2:
                //    owner.ChangeWeaponPos(0);
                //    break;
                //case FrameEvent.ChangeWeaponPos3:
                //    owner.ChangeWeaponPos(2);
                //    break;
            case FrameEvent.ChangeWeaponPos8:
                owner.ChangeWeaponPos(WeaponPos.PosB);
                break;
            case FrameEvent.ChangeWeaponPos5:
                owner.ChangeWeaponPos(WeaponPos.PosA);
                break;
            case FrameEvent.ChangeWeapon0://左打虎
            case FrameEvent.ChangeWeapon1://右打虎
                owner.ChangeNextWeapon();
            break;
            case FrameEvent.ChangeWeaponPose570_0:
                owner.ChangeWeaponPos(WeaponPos.PosA);
                break;
            case FrameEvent.ChangeWeaponPose451_0:
                owner.ChangeWeaponPos(WeaponPos.Pos);
                break;
            case FrameEvent.RebornFriend:
                owner.RebornFriend();
                break;
        }
    }

    //处理类似大刀空中下A，在固定帧5975开启重力
    public static void HandlerActionEvent(MeteorUnit owner, int pos, int frame)
    {
        if (pos == 298 && frame == 5975)
        {
            //大刀空中下A
            owner.IgnoreGravitys(false);
        }
        else if (pos == 235 && frame == 10973)
        {
            //双刺空中下A
            owner.IgnoreGravitys(false);
        }
        else if (pos == 465 && frame == 14904)
        {
            //忍者刀空中下A
            owner.IgnoreGravitys(false);
        }
    }
}