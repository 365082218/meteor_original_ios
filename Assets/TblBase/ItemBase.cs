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
    ChangeWeaponPos7 = 485,//第一帧，切换为居合
    ChangeWeaponPos449_0 = 449,
    ChangeWeaponPos450_0 = 450,
    ChangeWeaponPose570_0 = 570,//勾魂锁-拔刀-持枪
    ChangeWeaponPose451_0 = 451,//居合-大招-切换为拔刀
    //ChangeWeaponPose487_2 = 487,//乾坤大招完毕-最后一帧切换为收刀
    ChangeWeaponPose456_0 = 456,//
}

public class ActionEvent : Singleton<ActionEvent>
{
    public static List<int> LastEvents;
    public static List<int> FirstEvents;
    //处理动作第一帧要做的事
    public static void HandlerFirstActionFrame(MeteorUnit owner, int Action)
    {
        if (FirstEvents == null)
            FirstEvents = new List<int>() { (int)FrameEvent.ChangeWeaponPos2, (int)FrameEvent.ChangeWeaponPos3, (int)FrameEvent.ChangeWeaponPos5,(int)FrameEvent.ChangeWeaponPos4,
            (int)FrameEvent.ChangeWeaponPos449_0, (int)FrameEvent.ChangeWeaponPos450_0,(int)FrameEvent.ChangeWeaponPos6, (int)FrameEvent.ChangeWeaponPose570_0,
            (int)FrameEvent.ChangeWeaponPos7 };
        if (!FirstEvents.Contains(Action))
            return;
        switch ((FrameEvent)Action)
        {
            case FrameEvent.ChangeWeaponPos7:
                owner.ChangeWeaponPos(2);
                break;
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
    

    //处理动作最后一帧要做的事
    public static void HandlerFinalActionFrame(MeteorUnit owner, int Action)
    {
        if (LastEvents == null)
        {
            LastEvents = new List<int>() { (int)FrameEvent.ChangeWeapon0, (int)FrameEvent.ChangeWeapon1, (int)FrameEvent.ChangeWeaponPos0,
            (int)FrameEvent.ChangeWeaponPos1, (int)FrameEvent.ChangeWeaponPos5,
                (int)FrameEvent.ChangeWeaponPose570_0, (int)FrameEvent.ChangeWeaponPose456_0, (int)FrameEvent.ChangeWeaponPose451_0};
        }

        if (!LastEvents.Contains(Action))
            return;

        switch ((FrameEvent)Action)
        {
            //0拔刀1长枪2居合
            case FrameEvent.ChangeWeaponPos0:
                owner.ChangeWeaponPos(1);
                //owner.ChangeWeaponPos(2);
                break;
            case FrameEvent.ChangeWeaponPos1:
                owner.ChangeWeaponPos(2);
                break;
                //case FrameEvent.ChangeWeaponPos2:
                //    owner.ChangeWeaponPos(0);
                //    break;
                //case FrameEvent.ChangeWeaponPos3:
                //    owner.ChangeWeaponPos(2);
                //    break;
            case FrameEvent.ChangeWeaponPose456_0:
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