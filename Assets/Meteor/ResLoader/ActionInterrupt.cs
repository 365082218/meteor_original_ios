using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class ActionNode
{
    public int ActionIdx;//指明动作号码
    public int KeyMap;//指明多少行
    public List<ActionNode> target = new List<ActionNode>();//指明链接哪些动作
    public void AddTargetAction(ActionNode tar)
    {
        if (tar == null)
        {
            Debug.LogError("AddTargetAction tar = null?" + "  act src:" + ActionIdx);
            return;
        }
        if (!target.Contains(tar))
            target.Add(tar);
    }
}

public class CommonAction
{
    public const int DartReady = 1;
    public const int GunReady = 2;
    public const int GuillotinesReady = 3;
    public const int BrahchthrustReady = 4;
    public const int KnifeReady = 5;
    public const int SwordReady = 6;
    public const int LanceReady = 7;
    public const int BladeReady = 8;
    public const int HammerReady = 9;
    public const int Reborn = 22;
    public const int DefenceHitStart = 40;//防御受击开始-不受伤，只播放动画
    public const int DefenceHitEnd = 88;//防御受击结束
    public const int HitStart = 90;//受击动作开始
    public const int HitEnd = 121;//受击动作结束
    public const int GloveReady = 400;//持拳套-待机
    public const int QK_BADAO_READY = 401;
    public const int QK_CHIQIANG_READY = 402;
    public const int QK_JUHE_READY = 403;
    public const int RendaoReady = 404;
    public const int WaitWeaponReturn = 219;//等待飞轮回来
    public const int AttackActStart = 200;
    public const int BeHurted100 = 100;//受击被击到空中
    public const int BeHurted109 = 109;//受击低着头循环
    public const int BeHurted110 = 110;//受击抱着肚子
    public const int BeHurted111 = 111;//打的地面上翻滚
    public const int BeHurted114 = 114;//躺在地面继续受击。
    public const int BeHurted115 = 115;//趴在地面继续受击。
    public const int Struggle0 = 112;//卧倒地面
    public const int Struggle = 113;//趴倒地面

    public const int DefAttack116 = 116;//防御打击
    public const int Dead = 117;
    public const int IdleFront = 119;//卧倒起身
    public const int IdleBack = 120;//趴倒起身
    public const int WalkForward = 140;//走路 前
    public const int WalkRight = 141;//走路 右
    public const int WalkLeft = 142;//走路 左
    public const int WalkBackward = 143;//走路 后
    public const int Run = 144;//跑步
    public const int Idle = 0;
    public const int GunReload = 212;//装载子弹
    public const int GunIdle = 213;//待发射子弹
    public const int GunNormalShoot = 206;//枪普攻击 KeyMap=5
    public const int GunHeavyShoot = 207;//枪重射击-不破防-上A-焦野 KeyMap = 6
    public const int GunHeavyShootMax = 208;//枪杀射击-破防-下A-融铁 20气 KeyMap = 7
    public const int GunSkillShoot = 215;//大招 60气 KeyMap = 147 下上A
    public const int GunSkillShootMax = 216;//大招100气 KeyMap = 8 下上上A

    public const int Crouch = 10;//蹲着
    //蹲下 前右左后移动
    public const int CrouchForw = 145;
    public const int CrouchRight = 146;
    public const int CrouchLeft = 147;
    public const int CrouchBack = 148;
    
    //164-175 前右左后 闪 一样3个 指虎(509-512) 忍刀(501-504) 乾坤刀(505-508)
    public const int DForw1 = 164;//前闪 暗器 火枪 双刺 锤子
    public const int DForw2 = 165;//前闪 刀
    public const int DForw3 = 166;//前闪 匕首 枪 飞轮
    public const int DForw4 = 501;//忍刀前闪
    public const int DForw5 = 505;//乾坤刀前闪
    public const int DForw6 = 509;//指虎前闪
    public const int DRight1 = 167;
    public const int DRight2 = 168;
    public const int DRight3 = 169;
    public const int DRight4 = 502;//忍刀右闪
    public const int DRight5 = 506;//乾坤刀右闪
    public const int DRight6 = 510;//指虎右闪
    public const int DLeft1 = 170;
    public const int DLeft2 = 171;
    public const int DLeft3 = 172;
    public const int DLeft4 = 503;//忍刀左
    public const int DLeft5 = 507;//乾坤左闪
    public const int DLeft6 = 511;//指虎左闪
    public const int DBack1 = 173;
    public const int DBack2 = 174;
    public const int DBack3 = 175;
    public const int DBack4 = 504;//忍刀后闪
    public const int DBack5 = 508;//乾坤后闪
    public const int DBack6 = 512;//指虎后闪
    //蹲下 前右左后翻滚
    public const int DCForw = 176;
    public const int DCRight = 177;
    public const int DCLeft = 178;
    public const int DCBack = 179;
    public const int JumpFallOnGround = 180;//落地
    public const int RunOnDrug = 150;//带毒走
    public const int Jump = 151;//前跳
    public const int JumpFall = 152;//前跳回落
    public const int JumpRight = 153;//右跳
    public const int JumpRightFall = 154;//右跳回落
    public const int JumpLeft = 155;//左跳
    public const int JumpLeftFall = 156;//左跳回落
    public const int JumpBack = 157;//后跳
    public const int JumpBackFall = 158;//后跳回落
    public const int WallRightJump = 159;//接触墙壁时按跳右蹬腿
    public const int WallLeftJump = 160;//接触墙壁时按跳左蹬腿
    //public const int FallOnGround = 180;//落到地面时,跳回落动画还未播放完毕则播放撞击效果的落地.
    public const int Defence = 1000;//虚拟动作，因为与武器有关联
    public const int Attack = 1001;//虚拟动作，因为与武器类型有关联，攻击类的不需要自己控制，读character.act，
    //只有攀爬
    public const int BreakOut = 367;
    public const int ChangeWeapon = 24;
    public const int Taunt = 32;//嘲讽.挑衅
    public const int AirChangeWeapon = 36;//空中换武器

    public const int DartDefence = 11;//飞镖防御
    public const int GuillotinesDefence = 12;//血滴子防御
    public const int MarkDefence = 13;//火铳-防御
    public const int BrahchthrustDefence = 14;//双刺防御
    public const int KnifeDefence = 15; //匕首-防御 
    public const int SwordDefence = 16;//剑-防御
    public const int LanceDefence = 17;//长矛-防御.
    public const int BladeDefence = 18;//大刀-防御
    public const int HammerDefence = 19;//锤子-防御
    public const int ZhihuDefence = 480;//指虎防御
    public const int QiankunDefenct = 481;//乾坤刀防御
    public const int RendaoDefence = 482;//忍刀防御
                                         //475-477指虎防御受击
    public const int OnDrugHurt = 90;//挨打后仰
                                     //其他的呢?
    public const int ClimbUp = 161;
    public const int ClimbLeft = 162;
    public const int ClimbRight = 163;

    public const int KnifeWW = 253;//匕首地面上上A
    public const int KnifeSkill = 259;

    public const int KnifeA2Fall = 332;//空中A后落下 算跳跃落地POSE，可以继续爬墙
    public const int HammerMaxFall = 328;//空中大招后落下
    public const int Fall = 118;//受击从空中落下-硬直内
}

public class ActionInterrupt:Singleton<ActionInterrupt>
{
    public ActionNode Root;
    public SortedDictionary<int, ActionNode> Whole = new SortedDictionary<int, ActionNode>();
    public SortedDictionary<int, List<int>> Lines = new SortedDictionary<int, List<int>>();//存储行 与 Pose的关系
    public void Clear() {
        Root = null;
        Whole.Clear();
        Lines.Clear();
    }

    //得到一个招式可连接的普通招式
    public ActionNode GetNormalNode(MeteorUnit owner, ActionNode source)
    {
        //普通攻击映射输入的行号
        int weapon = owner.GetWeaponType();
        //"1,5,9,13,25,35,48,61,73,91,95,101,108,123,"    "85,22,32,46,59,107,122,105,"   J 攻击  59较特殊，估计所有重武器的空中A都是338动作
        int[] GroundKeyMap = new int[] { 1, 5, 9, 13, 25, 35, 48, 61, 73, 91, 95, 101, 108, 123 };
        int[] AirKeyMap = new int[] { 85, 22, 32, 46, 59, 107, 122, 105 };

        if (owner.IsOnGround())
        {
            for (int i = 0; i < source.target.Count; i++)
            {
                for (int j = 0; j < GroundKeyMap.Length; j++)
                {
                    if (GroundKeyMap[j] == source.target[i].KeyMap && owner.meteorController.Input.CheckPos(source.target[i].KeyMap, source.target[i].ActionIdx))
                        return source.target[i];
                }
            }
        }
        else
        {
            for (int i = 0; i < source.target.Count; i++)
            {
                for (int j = 0; j < AirKeyMap.Length; j++)
                {
                    if (AirKeyMap[j] == source.target[i].KeyMap && owner.meteorController.Input.CheckPos(source.target[i].KeyMap, source.target[i].ActionIdx))
                        return source.target[i];
                }
            }
        }

        return null;
    }

    /*
    259 //匕首大
    203 //标大
    216 //火枪大
    244 //双刺
    293 //长枪大
    310 //刀大
    368 //剑大
    421 //拳套大
    451 //乾坤刀
    474 //忍刀
    325 //锤
    224 //飞轮
    */

    int[] skill = new int[] { 259, 203, 216, 244, 293, 310, 368, 421, 451, 474, 325, 224 };
    bool IsSkill(int action)
    {
        for (int i = 0; i < skill.Length; i++)
        {
            if (action == skill[i])
                return true;
        }
        return false;
    }

    //得到一个招式可连接的非普通招式,不包含大绝招，可以有小绝招
    void Scan(MeteorUnit unit, ref List<ActionNode> dst, ref List<ActionNode> target, ref int [] scan)
    {
        for (int i = 0; i < target.Count; i++)
        {
            for (int j = 0; j < scan.Length; j++)
            {
                if (scan[j] == target[i].KeyMap && !IsSkill(target[i].ActionIdx) && unit.meteorController.Input.CheckPos(target[i].KeyMap, target[i].ActionIdx))
                {
                    dst.Add(target[i]);
                }
            }
        }
    }

    public List<ActionNode> GetSlashNode(MeteorUnit owner, ActionNode source)
    {
        List<ActionNode> action = new List<ActionNode>();
        int weapon = owner.GetWeaponType();
        //下A: "3,7,11,19,37,84,49,63,75,92,98,113,125,24" "24,33,47,60,72,83,106,109,126,"    SJ 下攻击 所有的基础动作，都不由连招系统负责，而由基本按键识别系统识别
        //左A: "16,28,38,64,89,100,"       AJ 左攻击 这里面只会缺少86爆气行
        //右A: "15,29,39,65,93,99,"        DJ 右攻击
        //上A: "2,6,10,14,26,36,50,62,74,96,124,"  "87,23,"    WJ 上攻击
        //下下A: "88,40,52,68,78,94,114,140,90," "150,133,"  SSJ 下下攻击
        //上上A: "20,27,43,54,66,77,97,117,137,160," "130,"  WWJ 上上攻击
        //左左A: "139,111,"  "132,"  AAJ 左左攻击
        //右右A: "138,112,"  "131,"  DDJ 右右攻击
        //下上A: "143,147,145,18,30,41,53,67,76,102,159,116,134,"    "144,146,"  SWJ 下上攻击
        //上下A: "42,103,118,127,"   "34,"   WSJ 上下攻击
        //左右A: "17,55,151,69,79,110,142,"      ADJ 左右攻击

        //右左A: "152,51,155,80,"        DAJ 右左攻击

        //左右下A: "71,81,120,128,154,"        ADSJ 左右下攻击
        //下左右A: "21,"       SADJ 下左右攻击   双刺大招

        //左右上A: "70,115,148,45,57,157,135"      ADWJ 左右上攻击
        
        //下下上A: "121,56,104,129,156,82,149"     SSWJ 下下上攻击
        //下上上A: "158,58,119,4,8,12,31,44,141,"      SWWJ 下上上攻击
        //上上上A: "153,"      WWWJ 上上上攻击
        //左右上下A: "136,"      ADWSJ 左右上下攻击
        int[] slash0Ground = new int[] { 3, 7, 11, 19, 37, 84, 49, 63, 75, 92, 98, 113, 125, 24 };//下A地面
        int[] slash0Air = new int[] { 24, 33, 47, 60, 72, 83, 106, 109, 126 };//下A空中
        int[] slash1Ground = new int[] { 16, 28, 38, 64, 89, 100 };//左攻击
        //int[] slash1Air = new int[] { };//左攻击无空中招式
        int[] slash2Ground = new int[] { 15, 29, 39, 65, 93, 99 };//右攻击
        //int[] slash2Air = new int[] { };//右攻击无空中招式
        int[] slash3Ground = new int[] { 2, 6, 10, 14, 26, 36, 50, 62, 74, 96, 124 };//上攻击
        int[] slash3Air = new int[] { 87, 23 };//上攻击空中招式
        int[] slash4Ground = new int[] { 88, 40, 52, 68, 78, 94, 114, 140, 90 };//下下攻击
        int[] slash4Air = new int[] { 150, 133 };//下下攻击空中
        int[] slash5Ground = new int[] { 20, 27, 43, 54, 66, 77, 97, 117, 137, 160 };//上上攻击
        int[] slash5Air = new int[] { 130 };//上上攻击空中
        int[] slash6Ground = new int[] { 139, 111 };//左左攻击
        int[] slash6Air = new int[] { 132 };//左左攻击空中
        int[] slash7Ground = new int[] { 138, 112 };//右右攻击
        int[] slash7Air = new int[] { 131 };//右右攻击空中
        int[] slash8Ground = new int[] { 143, 147, 145, 18, 30, 41, 53, 67, 76, 102, 159, 116, 134 };//下上攻击
        int[] slash8Air = new int[] { 144, 146 };//下上攻击空中
        int[] slash9Ground = new int[] { 42, 103, 118, 127 };//上下攻击
        int[] slash9Air = new int[] { 34 };//上下攻击空中
        int[] slash10Ground = new int[] { 17, 55, 151, 69, 79, 110, 142 };//左右攻击
        //int[] slash10Air = new int[] {  };//左右攻击空中
        int[] slash11Ground = new int[] { 152, 51, 155, 80 };//右左攻击
        //int[] slash11Air = new int[] { };//右左攻击空中
        int[] slash12Ground = new int[] { 71, 81, 120, 128, 154 };//左右下攻击
        int[] slash13Ground = new int[] { 21 };//下左右A
        int[] slash14Ground = new int[] { 70, 115, 148, 45, 57, 157, 135 };//左右上
        int[] slash15Ground = new int[] { 121, 56, 104, 129, 156, 82, 149 };//下下上
        int[] slash16Ground = new int[] { 158, 58, 119, 4, 8, 12, 31, 44, 141 };//下上上
        int[] slash17Ground = new int[] { 153 };//上上上-枪绝招
        int[] slash18Ground = new int[] { 136 };//左右下上-忍刀绝招
        
        if (owner.IsOnGround())
        {
            Scan(owner, ref action, ref source.target, ref slash0Ground);
            Scan(owner, ref action, ref source.target, ref slash1Ground);
            Scan(owner, ref action, ref source.target, ref slash2Ground);
            Scan(owner, ref action, ref source.target, ref slash3Ground);
            Scan(owner, ref action, ref source.target, ref slash4Ground);
            Scan(owner, ref action, ref source.target, ref slash5Ground);
            Scan(owner, ref action, ref source.target, ref slash6Ground);
            Scan(owner, ref action, ref source.target, ref slash7Ground);
            Scan(owner, ref action, ref source.target, ref slash8Ground);
            Scan(owner, ref action, ref source.target, ref slash9Ground);
            Scan(owner, ref action, ref source.target, ref slash10Ground);
            Scan(owner, ref action, ref source.target, ref slash11Ground);
            Scan(owner, ref action, ref source.target, ref slash12Ground);
            Scan(owner, ref action, ref source.target, ref slash13Ground);
            Scan(owner, ref action, ref source.target, ref slash14Ground);
            Scan(owner, ref action, ref source.target, ref slash15Ground);
            Scan(owner, ref action, ref source.target, ref slash16Ground);
            Scan(owner, ref action, ref source.target, ref slash17Ground);
            Scan(owner, ref action, ref source.target, ref slash18Ground);
            
        }
        else
        {
            Scan(owner, ref action, ref source.target, ref slash0Air);
            Scan(owner, ref action, ref source.target, ref slash3Air);
            Scan(owner, ref action, ref source.target, ref slash4Air);
            Scan(owner, ref action, ref source.target, ref slash5Air);
            Scan(owner, ref action, ref source.target, ref slash6Air);
            Scan(owner, ref action, ref source.target, ref slash7Air);
            Scan(owner, ref action, ref source.target, ref slash8Air);
            Scan(owner, ref action, ref source.target, ref slash9Air);
        }
        return action;
    }

    public int GetSkillPose(MeteorUnit owner)
    {
        int result = 0;
        switch (owner.GetWeaponType())
        {
            case (int)EquipWeaponType.Knife:
                result = 259;
                break;
            case (int)EquipWeaponType.Dart:
                result = 203;
                break;
            case (int)EquipWeaponType.Gun:
                result = 216;
                break;
            case (int)EquipWeaponType.Brahchthrust:
                result = 244;
                break;
            case (int)EquipWeaponType.Lance:
                result = 293;
                break;
            case (int)EquipWeaponType.Blade:
                result = 310;
                break;
            case (int)EquipWeaponType.Sword:
                result = 368;
                break;
            case (int)EquipWeaponType.Gloves:
                result = 421;
                break;
            case (int)EquipWeaponType.HeavenLance:
                result = 451;
                break;
            case (int)EquipWeaponType.NinjaSword:
                result = 474;
                break;
            case (int)EquipWeaponType.Hammer:
                result = 325;
                break;
            case (int)EquipWeaponType.Guillotines:
                result = 224;
                break;
            default:
                break;
        }
        return result;
    }

    //可接上的大小绝招
    public ActionNode GetSkillNode(MeteorUnit owner, ActionNode source)
    {
        int weapon = owner.GetWeaponType();
        //刀绝，枪绝，剑绝，匕首绝，锤绝，双刺绝，火枪绝，飞镖绝，忍刀绝，飞轮绝，乾坤刀绝，指虎绝
        for (int i = 0; i < source.target.Count; i++)
        {
            if (IsSkill(source.target[i].ActionIdx) && owner.meteorController.Input.CheckPos(source.target[i].KeyMap, source.target[i].ActionIdx))
                return source.target[i];
        }
        return null;
    }

    public void Init()
    {
        TextAsset act = Resources.Load<TextAsset>(string.Format("{0}/characteract", Main.Ins.AppInfo.MeteorVersion));
        MemoryStream ms = new MemoryStream(act.bytes);
        StreamReader text = new StreamReader(ms);

        //添加基本输入支持
        Root = new ActionNode();
        Root.ActionIdx = 0;
        Root.KeyMap = 0;//也就是什么都不输入，直接可以切换到Idle。
        Whole.Add(0, Root);

        //预备动作与IDLE类似

        //与IDLE并级别的.跑,左移,右移,后移,跳,左跳, 右跳, 后跳,(新增 怒气满 且防御时)
        //这些招式都可以往后接任意出招
        List<ActionNode> level0 = new List<ActionNode>();
        for (int i = 10; i < 11; i++)
        {
            ActionNode n = new ActionNode();
            n.ActionIdx = i;
            n.KeyMap = 0;
            level0.Add(n);
            Whole.Add(i, n);
        }

        //准备动作
        for (int i = CommonAction.DartReady; i <= CommonAction.HammerReady; i++)
        {
            ActionNode n = new ActionNode();
            n.ActionIdx = i;
            n.KeyMap = 0;
            level0.Add(n);
            Whole.Add(i, n);
        }

        //准备动作
        for (int i = CommonAction.GloveReady; i <= CommonAction.RendaoReady; i++)
        {
            ActionNode n = new ActionNode();
            n.ActionIdx = i;
            n.KeyMap = 0;
            level0.Add(n);
            Whole.Add(i, n);
        }

        //受击动作
        for (int i = 40; i <= 88; i++)
        {
            ActionNode n = new ActionNode();
            n.ActionIdx = i;
            n.KeyMap = 0;
            level0.Add(n);
            Whole.Add(i, n);
        }

        //倒地挣扎
        for (int i = 112; i <= 113; i++) {
            ActionNode n = new ActionNode();
            n.ActionIdx = i;
            n.KeyMap = 0;
            level0.Add(n);
            Whole.Add(i, n);
        }

        for (int i = 117; i < 118; i++) {
            ActionNode n = new ActionNode();
            n.ActionIdx = i;
            n.KeyMap = 0;
            level0.Add(n);
            Whole.Add(i, n);
        }

        for (int i = 140; i <= 148; i++)
        {
            ActionNode n = new ActionNode();
            n.ActionIdx = i;
            n.KeyMap = 0;
            level0.Add(n);
            Whole.Add(i, n);
        }

        //跳跃
        for (int i = 150; i < CommonAction.DCForw; i++)
        {
            ActionNode n = new ActionNode();
            n.ActionIdx = i;
            n.KeyMap = 0;
            level0.Add(n);
            Whole.Add(i, n);
        }

        //普通武器双击方向可以接任意招式
        for (int i = CommonAction.DForw4; i <= CommonAction.DBack6; i++)
        {
            ActionNode n = new ActionNode();
            n.ActionIdx = i;
            n.KeyMap = 0;
            level0.Add(n);
            Whole.Add(i, n);
        }

        //远程武器双击方向滚动可以接任何招式-(处理远程武器大招-滚动完毕后再出招BUG)
        for (int i = CommonAction.DCForw; i <= CommonAction.DCBack; i++)
        {
            ActionNode n = new ActionNode();
            n.ActionIdx = i;
            n.KeyMap = 0;
            level0.Add(n);
            Whole.Add(i, n);
        }

        int line = 1;
        string sV = "";
        while (!text.EndOfStream)
        {
            string s = text.ReadLine();
            if (string.IsNullOrEmpty(s))
                continue;
            if (s.StartsWith("#"))
                continue;
            int subIndex = s.IndexOf('#');
            if (subIndex != -1)
                s = s.Substring(0, subIndex);
            sV += s.Trim().ToLower();
            //有的是会跨行的
            if (sV.EndsWith(","))
                continue;

            if (sV.StartsWith("group"))
            {
                string[] actArray = sV.Split(new char[] { '(','-', ')'}, System.StringSplitOptions.RemoveEmptyEntries);
                if (actArray.Length == 2)
                {
                    int Action = -1;
                    if (int.TryParse(actArray[1], out Action))
                    {
                        ActionNode no = new ActionNode();
                        no.ActionIdx = Action;
                        no.KeyMap = line;
                        if (!Lines.ContainsKey(line))
                            Lines.Add(line, new List<int> { Action });
                        Whole.Add(no.ActionIdx, no);
                        //如果是第一个动作，那么可以从IDLE切换过去
                        Root.AddTargetAction(no);
                        //还可以从顶级非攻击动作进入
                        for (int j = 0; j < level0.Count; j++)
                            level0[j].AddTargetAction(no);
                    }
                }
                else if (actArray.Length == 3)
                {
                    int ActionStart = -1;
                    int ActionEnd = -1;
                    if (int.TryParse(actArray[1], out ActionStart) && int.TryParse(actArray[2], out ActionEnd))
                    {
                        for (int i = 0; i <= ActionEnd - ActionStart; i++)
                        {
                            ActionNode no = new ActionNode();
                            no.ActionIdx = i + ActionStart;
                            no.KeyMap = line;
                            if (!Lines.ContainsKey(line))
                                Lines.Add(line, new List<int> { no.ActionIdx });
                            else
                                Lines[line].Add(no.ActionIdx);
                            Whole.Add(no.ActionIdx, no);
                            //如果是第一个动作，那么可以从IDLE切换过去
                            if (i == 0)
                            {
                                Root.AddTargetAction(no);
                                for (int j = 0; j < level0.Count; j++)
                                    level0[j].AddTargetAction(no);
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError("!!!");
                }
                line++;
            }
            else
            {
                string[] actArray = sV.Split(new char[] { '(', ',', ')', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (actArray.Length > 1)
                {
                    int Action = -1;
                    ActionNode src = null;
                    if (int.TryParse(actArray[0], out Action))
                    {
                        if (Whole.ContainsKey(Action))
                            src = Whole[Action];
                        else
                        {
                            //部分招式不接受任何输入。只能从上一个POSE自动切换，类似573 刀小绝2段，是没有任何招式可以后面接他的，而且他也不接受输入
                            //Debug.LogError("action:" + Action + " prev failed");
                            ActionNode no = new ActionNode();
                            no.ActionIdx = Action;
                            no.KeyMap = -1;//不接受输入
                            Whole.Add(no.ActionIdx, no);
                            src = no;
                        }
                    }
                    if (src != null)
                    {
                        for (int i = 1; i < actArray.Length; i++)
                        {
                            ActionNode target = null;
                            Action = -1;
                            if (int.TryParse(actArray[i], out Action))
                                target = Whole[Action];
                            src.AddTargetAction(target);
                        }
                    }
                }
            }
            sV = "";
        }
        ms.Close();
        text.Close();

        //打印一下输入输出对应表
        //List<ActionNode> first = Whole.Values.ToList();
        //for (int i = 0; i < first.Count; i++)
        //{
        //    Log.Write(string.Format("Input:{0} targetAction ={1}", first[i].KeyMap, first[i].ActionIdx));
        //}
    }

    public ActionNode GetActions(int actionIdx)
    {
        SortedDictionary<int, ActionNode>.Enumerator ienum = Whole.GetEnumerator();
        while (ienum.MoveNext())
        {
            if (ienum.Current.Key == actionIdx)
                return ienum.Current.Value;
        }
        return null;
    }

    //只走一层节点,网状结构，可以无限递归
    public ActionNode FindAct(List<ActionNode> lst, int idx)
    {
        if (lst == null)
            return null;
        for (int i = 0; i < lst.Count; i++)
        {
            if (lst[i].ActionIdx == idx)
                return lst[i];
        }
        return null;
    }

    //key = EO_OP
    //普通功能按键
    //void AddInterrupt(int posCurrent, VK_Pose posTarget, int key, int inputType)
    //{
    //    ActionChangeDef def = new ActionChangeDef();
    //    def.ActionIdx = posTarget;
    //    def.InputKey = key;
    //    def.InputType = inputType;
    //    if (interrupt.ContainsKey(posCurrent))
    //        interrupt[posCurrent].Add(def);
    //    else
    //        interrupt.Add(posCurrent, new List<ActionChangeDef>() { def});
    //}

    //连招
    //void AddInterrupt(int posCurrent, VK_Pose posTarget, List<int> key)
    //{
    //    ActionChangeDef def = new ActionChangeDef();
    //    def.ActionIdx = posTarget;
    //    def.KeyLink = key;
    //    def.InputKey = 0;//单键为0时，判断连招
    //    def.InputType = 0;
    //    if (interrupt.ContainsKey(posCurrent))
    //        interrupt[posCurrent].Add(def);
    //    else
    //        interrupt.Add(posCurrent, new List<ActionChangeDef>() { def });
    //}
}
