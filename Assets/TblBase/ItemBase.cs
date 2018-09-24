using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemBase : TblBase
{
    public override string TableName { get { return "Item"; } }
    public int Idx;
    public string Name;
    public int MainType;
    public int SubType;
    public int EquipIdx;
    public int UnitId;
    public string Icon;
    public string Desc;
    public int Quality;
    public int HP;
    public int MP;
    public int Damage;
    public int Def;
    public int Speed;
    public int Crit;
    public uint Coin;
    public uint Stack;
    public int CNY;
    public int ArmyHP;
    public int ArmyDamage;
    public int ArmyDef;
    public float Size;
    public string Script;
    public int Skill;
    public int LevReq;
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
    ChangeWeaponPos7 = 485,//最后一帧，切换为居合
    ChangeWeaponPos8 = 487,//最后一帧，切换为居合
    ChangeWeaponPos449_0 = 449,//拔刀术，前
    ChangeWeaponPos450_0 = 450,//拔刀术，后
    ChangeWeaponPose570_0 = 570,//勾魂锁-拔刀-持枪
    ChangeWeaponPose451_0 = 451,//居合-大招-切换为拔刀
    ChangeWeaponPose487_2 = 487,//乾坤大招完毕-最后一帧切换为收刀
}

public class ActionEvent : Singleton<ActionEvent>
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
            (int)FrameEvent.ChangeWeaponPos449_0, (int)FrameEvent.ChangeWeaponPos450_0,(int)FrameEvent.ChangeWeaponPos6, (int)FrameEvent.ChangeWeaponPose570_0,
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
                owner.ChangeWeaponPos(0);
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
        }
    }
    
    //部分动作事件
    public static void HandlerPoseAction(MeteorUnit owner, int action)
    {
        switch (action)
        {
            case (int)FrameEvent.ChangeWeaponPos7://收刀切换为居合最后一帧
            case (int)FrameEvent.ChangeWeaponPos8://收刀切换为居合
                owner.ChangeWeaponPos(2);
                break;
        }
    }

    //处理动作最后一帧要做的事
    public static void HandlerFinalActionFrame(MeteorUnit owner, int Action)
    {
        if (LastEvents == null)
        {
            LastEvents = new List<int>() { (int)FrameEvent.ChangeWeapon0, (int)FrameEvent.ChangeWeapon1, (int)FrameEvent.ChangeWeaponPos0, (int)FrameEvent.ChangeWeaponPos5,
                (int)FrameEvent.ChangeWeaponPose570_0, (int)FrameEvent.ChangeWeaponPose451_0, (int)FrameEvent.ChangeWeaponPos7};
        }

        if (!LastEvents.Contains(Action))
            return;

        switch ((FrameEvent)Action)
        {
            case FrameEvent.ChangeWeaponPos7://收刀切换为居合最后一帧
                owner.ChangeWeaponPos(2);
                break;
            //0拔刀1长枪2居合
            case FrameEvent.ChangeWeaponPos0:
                owner.ChangeWeaponPos(1);
                //owner.ChangeWeaponPos(2);
                break;

                //case FrameEvent.ChangeWeaponPos2:
                //    owner.ChangeWeaponPos(0);
                //    break;
                //case FrameEvent.ChangeWeaponPos3:
                //    owner.ChangeWeaponPos(2);
                //    break;
            case FrameEvent.ChangeWeaponPos8:
                owner.ChangeWeaponPos(2);
                break;
            case FrameEvent.ChangeWeaponPos5:
                owner.ChangeWeaponPos(1);
                break;
            case FrameEvent.ChangeWeapon0://左打虎
            case FrameEvent.ChangeWeapon1://右打虎
                owner.ChangeNextWeapon();
            break;
            case FrameEvent.ChangeWeaponPose570_0:
                owner.ChangeWeaponPos(1);
                break;
            case FrameEvent.ChangeWeaponPose451_0:
                owner.ChangeWeaponPos(0);
                break;
        }
    }
}