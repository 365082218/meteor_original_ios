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
    public const int GunNormalShoot = 206;//枪普攻击
    public const int GunHeavyShoot = 207;//枪重射击-不破防
    public const int GunHeavyShootMax = 208;//枪杀射击-破防

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
    public const int Fall = 118;
}

public class ActionInterrupt
{
    public ActionNode Root;
    public Dictionary<int, ActionNode> Whole = new Dictionary<int, ActionNode>();
    public Dictionary<int, List<int>> Lines = new Dictionary<int, List<int>>();//存储行 与 Pose的关系
    
    bool IsWeaponKey(int weapon, int KeyMap)
    {
        bool result = false;
        switch (weapon)
        {
            case (int)EquipWeaponType.Knife:
                switch (KeyMap)
                {
                    case 25://匕首普攻,需要看输入缓冲是否含有其他方向键,或者含有任意方向键的状态
                    case 32:
                    case 26://匕首上A,状态,只能按下一次W或者弹起一次W,或者当前W被按下,还要判断反向键的状态
                    //上上A,前3个按键里面只有找到至少2个按下W按键
                    case 27:
                    case 28://左A 370-371
                    case 29://右A
                    case 30://下上A(地面)            
                    case 31://下上上A 阎罗
                    case 84://下A 空 地都可以 接空中252不能在地面 接地面343不能在空中
                    case 33:
                    case 34:
                    case 88://下下A 地面小绝 下扎
                    case 150://空中下下A
                        result = true;
                        break;
                }
                break;
            //锤子长枪大刀属于重武器，共用输入59编号动作338
            case (int)EquipWeaponType.Blade:
                switch (KeyMap)
                {
                    case 61:
                    case 62:
                    case 63:
                    case 64:
                    case 65:
                    case 66:
                    case 67:
                    case 68:
                    case 69:
                    case 70:
                    case 71://大绝招
                    case 72:
                    case 155://旋风斩
                    case 156://雷电斩
                    case 59://一般空踢
                        result = true;
                        break;
                }
                break;
            case (int)EquipWeaponType.Dart:
                switch (KeyMap)
                {
                    case 1:
                    case 143://地面八方绝
                    case 85:
                    case 4://落樱雪
                    case 2:
                    case 3:
                    case 87:
                    case 144://空中八方绝
                        result = true;
                        break;
                }
                break;
            case (int)EquipWeaponType.Gun:
                switch (KeyMap)
                {
                    case 5:
                    case 6:
                    case 7://重射击
                    case 8://大绝
                    case 147://小绝
                    case 59://一般空踢
                        result = true;
                        break;
                }
                break;
            case (int)EquipWeaponType.Guillotines://血滴子
                switch (KeyMap)
                {
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                    case 145:
                    case 146:
                    case 59://一般空踢
                        result = true;
                        break;
                }
                break;
            case (int)EquipWeaponType.Brahchthrust://分水刺
                switch (KeyMap)
                {
                    case 13:
                    case 14:
                    case 15:
                    case 16:
                    case 17:
                    case 18:
                    case 19:
                    case 20:
                    case 21:
                    case 22:
                    case 23:
                    case 24:
                    case 148://左右上小绝
                    case 149://加速BUFF
                        result = true;
                        break;
                }
                break;
            case (int)EquipWeaponType.Sword://剑
                switch (KeyMap)
                {
                    case 35:
                    case 36:
                    case 37:
                    case 38:
                    case 39:
                    case 40:
                    case 41:
                    case 42:
                    case 43:
                    case 44:
                    case 45://大招
                    case 46:
                    case 47:
                    case 151://左右A剑气
                    case 152://右左A小绝旋转
                        result = true;
                        break;
                }
                break;
            case (int)EquipWeaponType.Lance://枪
                switch (KeyMap)
                {
                    case 48:
                    case 49:
                    case 50:
                    case 51:
                    case 52:
                    case 53:
                    case 54:
                    case 55:
                    case 56:
                    case 57:
                    case 58://大招
                    case 59:
                    case 60:
                    case 153://强攻 前前前A
                    case 154://左右下A 小绝招
                        result = true;
                        break;
                }
                break;
            case (int)EquipWeaponType.Hammer://锤子
                switch (KeyMap)
                {
                    case 73:
                    case 74:
                    case 75:
                    case 76:
                    case 77:
                    case 78:
                    case 79:
                    case 80:
                    case 81:
                    case 82://铜皮
                    case 83:
                    case 157://震荡波
                    case 158://大绝
                    case 59://一般空踢
                        result = true;
                        break;
                }
                break;
            case (int)EquipWeaponType.HeavenLance://乾坤
                switch (KeyMap)
                {
                    //乾坤分3种姿态，也就是3个POSE组
                    case 89://左A 拔刀切换持枪 430
                    case 90://下下A 拔刀切换居合 431
                    case 91://432，433，434
                    case 92://439
                    case 93://右A 持枪切换拔刀440
                    case 94://下下A 持枪切换居合441
                    case 95://442
                    case 96://443
                    case 97://444,拔刀小绝
                    case 98://445
                    case 99://右A 居合转换拔刀447
                    case 100://左A 居合转换持枪 448 
                    case 101://520,521,522,523
                    case 102://449-持枪小绝
                    case 103://450
                    case 104://451//大绝招
                    case 105://458
                    case 106://空中下A 446
                    case 159:
                    case 160:
                        result = true;
                        break;
                }
                break;
            case (int)EquipWeaponType.Gloves://拳套
                switch (KeyMap)
                {
                    //乾坤分3种姿态，也就是3个POSE组
                    case 107:
                    case 108:
                    case 109:
                    case 110:
                    case 111:
                    case 112:
                    case 113:
                    case 114:
                    case 115:
                    case 116:
                    case 117:
                    case 118:
                    case 119:
                    case 120://大绝
                    case 121://嗜血
                        result = true;
                        break;
                }
                break;
            case (int)EquipWeaponType.NinjaSword://忍者刀
                switch (KeyMap)
                {
                    case 122:
                    case 123:
                    case 124:
                    case 125:
                    case 126:
                    case 127:
                    case 128://忍术-隐忍
                    case 129:
                    case 130:
                    case 131:
                    case 132:
                    case 133:
                    case 134:
                    case 135:
                    case 136://天地同寿
                    case 137:
                    case 138:
                    case 139:
                    case 140:
                    case 141://忍爆弹-忍大招
                    case 142://左右A 10气
                        result = true;
                        break;
                }
                break;
        }
        
        return result;
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
                    if (GroundKeyMap[j] == source.target[i].KeyMap && IsWeaponKey(weapon, source.target[i].KeyMap))
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
                    if (AirKeyMap[j] == source.target[i].KeyMap && IsWeaponKey(weapon, source.target[i].KeyMap))
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
    void Scan(int weapon, ref List<ActionNode> dst, ref List<ActionNode> src, ref int [] scan)
    {
        if (dst == null || dst.Count == 0 || src == null || src.Count == 0 || scan == null || scan.Length == 0)
            Debug.DebugBreak();
        for (int i = 0; i < src.Count; i++)
        {
            for (int j = 0; j < scan.Length; j++)
            {
                if (scan[j] == src[i].KeyMap && !IsSkill(src[i].ActionIdx) && IsWeaponKey(weapon, src[i].KeyMap))
                {
                    dst.Add(src[i]);
                }
            }
        }
    }

    Dictionary<int, List<ActionNode>> CacheGround = new Dictionary<int, List<ActionNode>>();
    Dictionary<int, List<ActionNode>> CacheAir = new Dictionary<int, List<ActionNode>>();
    public List<ActionNode> GetSlashNode(MeteorUnit owner, ActionNode source)
    {
        List<ActionNode> action = new List<ActionNode>();
        int weapon = owner.GetWeaponType();
        if (owner.IsOnGround())
        {
            if (CacheGround.TryGetValue(source.ActionIdx, out action))
                return action;
        }
        else
        {
            if (CacheAir.TryGetValue(source.ActionIdx, out action))
                return action;
        }

        action = new List<ActionNode>();
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
            Scan(weapon, ref action, ref source.target, ref slash0Ground);
            Scan(weapon, ref action, ref source.target, ref slash1Ground);
            Scan(weapon, ref action, ref source.target, ref slash2Ground);
            Scan(weapon, ref action, ref source.target, ref slash3Ground);
            Scan(weapon, ref action, ref source.target, ref slash4Ground);
            Scan(weapon, ref action, ref source.target, ref slash5Ground);
            Scan(weapon, ref action, ref source.target, ref slash6Ground);
            Scan(weapon, ref action, ref source.target, ref slash7Ground);
            Scan(weapon, ref action, ref source.target, ref slash8Ground);
            Scan(weapon, ref action, ref source.target, ref slash9Ground);
            Scan(weapon, ref action, ref source.target, ref slash10Ground);
            Scan(weapon, ref action, ref source.target, ref slash11Ground);
            Scan(weapon, ref action, ref source.target, ref slash12Ground);
            Scan(weapon, ref action, ref source.target, ref slash13Ground);
            Scan(weapon, ref action, ref source.target, ref slash14Ground);
            Scan(weapon, ref action, ref source.target, ref slash15Ground);
            Scan(weapon, ref action, ref source.target, ref slash16Ground);
            Scan(weapon, ref action, ref source.target, ref slash17Ground);
            Scan(weapon, ref action, ref source.target, ref slash18Ground);
            CacheGround.Add(source.ActionIdx, action);
        }
        else
        {
            Scan(weapon, ref action, ref source.target, ref slash0Air);
            Scan(weapon, ref action, ref source.target, ref slash3Air);
            Scan(weapon, ref action, ref source.target, ref slash4Air);
            Scan(weapon, ref action, ref source.target, ref slash5Air);
            Scan(weapon, ref action, ref source.target, ref slash6Air);
            Scan(weapon, ref action, ref source.target, ref slash7Air);
            Scan(weapon, ref action, ref source.target, ref slash8Air);
            Scan(weapon, ref action, ref source.target, ref slash9Air);
            CacheAir.Add(source.ActionIdx, action);
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
    //大绝招
    public ActionNode GetSkillNode(MeteorUnit owner, ActionNode source)
    {
        int weapon = owner.GetWeaponType();
        //刀绝，枪绝，剑绝，匕首绝，锤绝，双刺绝，火枪绝，飞镖绝，忍刀绝，飞轮绝，乾坤刀绝，指虎绝
        for (int i = 0; i < source.target.Count; i++)
        {
            if (IsSkill(source.target[i].ActionIdx) && IsWeaponKey(weapon, source.target[i].KeyMap))
                return source.target[i];
        }
        return null;
    }

    public void Init()
    {
        if (Root != null)
            return;

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

        //
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
        List<ActionNode> first = Whole.Values.ToList();
        for (int i = 0; i < first.Count; i++)
        {
            Log.Write(string.Format("Input:{0} targetAction ={1}", first[i].KeyMap, first[i].ActionIdx));
        }
    }

    public ActionNode GetActions(int actionIdx)
    {
        Dictionary<int, ActionNode>.Enumerator ienum = Whole.GetEnumerator();
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
