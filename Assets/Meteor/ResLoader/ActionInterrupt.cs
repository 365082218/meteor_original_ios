using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

//每一个动作，都有默认的几个中断动作，
//中断动作，代表当某一条件成立时，可以由当前动作，切换到中断动作,条件一般是，按键输入
public class ActionChangeDef
{
    public VK_Pose ActionIdx;//虚拟状态-一一映射到实际动作.
    //负责单按键切换状态
    public int InputKey;//输入键位
    public int InputType;//输入状态 压下 弹起等
}

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
//虚拟招式表
public enum VK_Pose
{
    VK_Idle = 600,//虚拟动作从600开始，实际动作从0开始
    //双击 上下左右
    VK_Attack,
    VK_Defence,
    VK_Couch,//蹲着
    VK_Taunt,//嘲讽
    VK_PlayedDead,//装死
    VK_Help,//救活同伴
    VK_Break,//爆气
    //模仿赵云传 3武器切换，一种轻武器，一种重武器，一种暗器
    VK_ChangeWeapon1,
    VK_ChangeWeapon2,
    //单击
    VK_Run,//跑步
    VK_WalkForward,//中毒是走150，不中毒是跑 144
    VK_WalkLeft,
    VK_WalkRight,
    VK_WalkBackward,
    VK_RunOnDrug,
    //双击上下左右，根据BUFF状态（毒），武器，使用不同的动作
    VK_DForw,
    VK_DBack,
    VK_DLeft,
    VK_DRight,
    //方向+跳
    VK_JumpForw,
    VK_JumpBack,
    VK_JumpLeft,
    VK_JumpRight,
    VK_Jump,//原地跳
    VK_JumpFall,//跳跃转空中落地
    //爬墙
    VK_ClimbForw,//往上爬墙
    VK_ClimeLeft,//往左爬墙
    VK_ClimeRight,//往右爬墙
    VK_Fall,//从天空掉落
    //爬墙蹬，就是爬墙到某个点，按跳跃
    VK_Rebound,
    VK_ReboundLeft,
    VK_ReboundRight,
    //连招虚拟映射, W S A D, J 
    VK_WJ,//上手
    VK_SJ,//下手
    VK_WWJ,//上上手
    VK_SSJ,//下下手
    VK_SWJ,//下上手
    VK_SWWJ,//下上上手
    VK_SSWJ,//下下上手
    VK_AJ,//左A
    VK_DJ,//右A
    VK_ADJ,//左右A
    VK_DAJ,//右左A
    VK_ADWJ,//左右上A
    VK_ADSJ,//左右下A
    VK_WWWJ,//上上上A
    VK_AAJ,//指虎左打虎切换武器
    VK_DDJ,//指虎右打虎切换武器

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
    public const int ZhihuReady = 400;
    public const int QK_BADAO_READY = 401;
    public const int QK_CHIQIANG_READY = 402;
    public const int QK_JUHE_READY = 403;
    public const int RendaoReady = 404;
    public const int WaitWeaponReturn = 219;//等待飞轮回来
    public const int AttackActStart = 200;
    public const int BeHurted109 = 109;//受击低着头循环
    public const int BeHurted110 = 110;//受击抱着肚子
    public const int BeHurted111 = 111;//打的地面上翻滚
    public const int BeHurted114 = 114;//躺在地面继续受击。
    public const int BeHurted115 = 115;//趴在地面继续受击。
    public const int Struggle0 = 112;
    public const int Struggle = 113;

    public const int DefAttack116 = 116;//防御打击
    public const int Dead = 117;
    public const int WalkForward = 140;//走路 前
    public const int WalkRight = 141;//走路 右
    public const int WalkLeft = 142;//走路 左
    public const int WalkBackward = 143;//走路 后
    public const int Run = 144;//跑步
    public const int Idle = 0;
    public const int GunReload = 212;//装载子弹
    public const int GunIdle = 213;//待发射子弹
    public const int Crouch = 10;//蹲着
    //蹲下 前右左后移动
    public const int CrouchForw = 145;
    public const int CrouchRight = 146;
    public const int CrouchLeft = 147;
    public const int CrouchBack = 148;
    
    //164-175 前右左后 闪 一样3个 指虎(509-512) 忍刀(501-504) 乾坤刀(505-508)
    public const int DForw1 = 164;//前闪 暗器 火枪 双刺 锤子
    public const int DForw2 = 165;//前闪 剑 刀
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
    public const int FallOnGround = 180;//落到地面时,跳回落动画还未播放完毕则播放撞击效果的落地.
    public const int Defence = 1000;//虚拟动作，因为与武器有关联
    public const int Attack = 1001;//虚拟动作，因为与武器类型有关联，攻击类的不需要自己控制，读character.act，
    //只有攀爬，
    public const int SwordAttack = 0;//剑A
    public const int BladeAttack = 0;//刀A
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

public class ActionInterrupt: Singleton<ActionInterrupt> {
    public ActionNode Root;
    public Dictionary<int, ActionNode> Whole = new Dictionary<int, ActionNode>();
    public Dictionary<int, List<int>> Lines = new Dictionary<int, List<int>>();//存储行 与 Pose的关系
    public void Init()
    {
        if (Root != null)
            return;

        TextAsset act = Resources.Load<TextAsset>(Global.MeteorVersion + "/characteract");
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
        for (int i = CommonAction.ZhihuReady; i <= CommonAction.RendaoReady; i++)
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
        for (int i = 150; i <= 180; i++)
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
        //for (int i = CommonAction.DCForw; i <= CommonAction.DCBack; i++)
        //{
        //    ActionNode n = new ActionNode();
        //    n.ActionIdx = i;
        //    n.KeyMap = 0;
        //    level0.Add(n);
        //    Whole.Add(i, n);
        //}

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
            Log.LogInfo("Input:" + first[i].KeyMap + "targetAction = " + first[i].ActionIdx);
        }
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
