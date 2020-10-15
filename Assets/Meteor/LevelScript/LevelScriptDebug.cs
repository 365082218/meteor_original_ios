using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Idevgame.Util;
using System.Linq;

//存储了测试关卡的脚本
//23韩棠-大决战
public class LevelScript_sn23 : LevelScriptBase {
    int RoundTime = 30;
    int PlayerSpawn = 0;
    int PlayerSpawnDir = 84;
    int PlayerWeapon = 57;
    int PlayerWeapon2 = 5;
    int PlayerModel = 0;
    int PlayerHP = 4500;
    string PlayerName = StringUtils.DefaultPlayer;
    public override int GetRoundTime() { return RoundTime; }
    public override int GetPlayerSpawn() { return PlayerSpawn; }
    public override int GetPlayerSpawnDir() { return PlayerSpawnDir; }
    public override int GetPlayerWeapon() { return PlayerWeapon; }
    public override int GetPlayerWeapon2() { return PlayerWeapon2; }
    public override int GetPlayerModel() { return PlayerModel; }
    public override string GetPlayerName() { return PlayerName; }
    public override int GetPlayerMaxHp() { return PlayerHP; }
    public override void OnStart() {
        AddNPC("npc23_05");//韩棠
        AddNPC("npc23_01");//屠大鹏
        AddNPC("npc23_02");//罗金鹏  
        AddNPC("npc23_03");//萧银鹏
        AddNPC("npc23_04");//原怒鹏
        base.OnStart();
    }

    int trg0 = 0;
    int trg1 = 0;
    int trg2 = 0;
    int trg3 = 0;
    int trg4 = 0;
    int trg5 = 0;
    int trg6 = 0;//叶翔
    int trg7 = 0;//石群
    int time1 = 0;
    int time2 = 0;
    int time3 = 0;
    int timeSkill = 0;
    int skillgroup = 7;
    public override int OnUpdate() {
        int player = GetChar("player");
        if (player < 0) {
            return 0;
        }

        int c0;
        int c1;
        int c2;
        int c3;
        int c4;
        int c5;

        if (trg0 == 0) {
            c1 = GetChar("屠城");
            c2 = GetChar("罗江");
            c3 = GetChar("萧安");
            c4 = GetChar("原冲");
            c5 = GetChar("韩棠");
            if (c1 >= 0 && c2 >= 0 && c3 >= 0 && c4 >= 0 && c5 >= 0) {
                Perform(c1, "faceto", player);
                Perform(c2, "faceto", player);
                Perform(c3, "faceto", player);
                Perform(c4, "faceto", player);

                ChangeBehavior(c1, "kill", c5);
                ChangeBehavior(c2, "kill", player);
                ChangeBehavior(c3, "kill", c5);
                ChangeBehavior(c4, "kill", player);

                Perform(c1, "say", "所以你只管放心死吧孙玉伯一定很快就会到地狱去陪你。");
                Perform(c1, "pause", 3);
                Perform(c1, "say", "根若已烂了这棵树很快就会烂光的。");
                Perform(c1, "pause", 3);
                Perform(c1, "say", "孙玉伯一向认为他的属下都对他极忠诚，但现在连他最信任的人也出卖了他这就好像一棵树的根已经烂了。");
                Perform(c1, "pause", 3);
                Perform(c1, "say", "这人当然是很得孙玉伯的信任，所以才会知道你们的关系……");
                Perform(c1, "pause", 3);
                Perform(c1, "say", "你一定会奇怪我们怎么知道你和孙玉伯的关系？这当然是有人告诉我们的只可惜你这一辈子也猜不出这个人是谁。");
                Perform(c1, "pause", 3);
                Perform(c1, "say", "因为你是孙玉伯的死党，‘十二飞鹏帮’现在己和孙玉伯势不两立。");
                Perform(c1, "pause", 3);
                Perform(c1, "say", "你知不知道我们为什么要杀你？");
                Perform(c1, "pause", 3);
                Perform(c1, "say", "韩棠你该觉得骄傲才是，杀孙剑的时候，我们连手都没有动，但杀你我们却动用了全力。");
                Perform(c1, "pause", 3);
                Perform(c1, "block", 0);
                Perform(c1, "say", "屠城屠大鹏");
                Perform(c1, "pause", 4);
                Perform(c1, "aggress");
                Perform(c1, "say", "你怎么知道那五人全是幌子，我才是真正来杀你的？");
                Perform(c1, "pause", 1);
                Perform(c1, "guard", 6);
                Perform(c1, "block", 1);

                //韩棠对白
                Perform(c5, "block", 0);
                Perform(c5, "pause", 3);
                Perform(c5, "say", "你们都是‘十二飞鹏帮’的人?");
                Perform(c5, "pause", 3);
                Perform(c5, "faceto", c1);
                Perform(c5, "guard", 6);
                Perform(c5, "block", 1);
                //罗江
                Perform(c2, "block", 0);
                Perform(c2, "say", "罗江罗金鹏。");
                Perform(c2, "pause", 5);
                Perform(c2, "guard", 10);
                Perform(c2, "block", 1);

                //萧安
                Perform(c3, "block", 0);
                Perform(c3, "say", "萧安萧银鹏。");
                Perform(c3, "pause", 6);
                Perform(c3, "guard", 10);
                Perform(c3, "block", 1);

                //原冲
                Perform(c4, "block", 0);
                Perform(c4, "say", "原冲原怒鹏。");
                Perform(c4, "pause", 7);
                Perform(c4, "guard", 10);
                Perform(c4, "block", 1);

                //主角
                PlayerPerform("block", 0);
                PlayerPerform("pause", 10);
                PlayerPerform("say", "...");
                PlayerPerform("faceto", c1);
                PlayerPerform("guard", 10);
                PlayerPerform("block", 1);
                trg0 = 1;
            }
        }

        //蝴蝶阵营获胜
        if (trg1 == 0) {
            c5 = GetChar("韩棠");
            if (GetHP(c5) <= 0) {
                c1 = GetChar("屠城");
                Perform(c1, "say", "出卖你的人是律香川，他不但出卖你还出卖了孙玉伯");
                Perform(c1, "pause", 2);
                Perform(c1, "say", "我知道你死不暝目，死后一定变为厉鬼．但你的鬼魂却不该来找我们，你应该去找那出卖你的人。");
                Perform(c1, "pause", 2);

                c3 = GetChar("萧安");
                Perform(c3, "say", "走，快走——");
                Perform(c3, "pause", 6);
                trg1 = 1;
                time1 = GetGameTime() + 8;
            }
        }

        if (trg1 == 1 && GetGameTime() > time1) {
            GameOver(-1);
        }

        //流星阵营，特殊事件触发
        if (trg2 == 0) {
            if (GetHP(player) > 0 && GetHP(player) <= (GetMaxHP(player) / 3)) {
                PlayerPerform("say", "....");
                PlayerPerform("pause", 3);
                PlayerPerform("say", "算你们厉害，不过我也有兄弟！");
                trg2 = 1;
                time2 = GetGameTime() + 5;
            }
        }

        if (trg2 == 1 && GetGameTime() > time2) {
            if (GetHP(player) > 0) {
                AddNPC("npc23_06");
                AddNPC("npc23_07");
            }
            trg2 = 2;
        }

        if (trg5 == 0) {
            c5 = GetChar("韩棠");
            if (GetHP(c5) > 0 && GetHP(c5) <= (GetMaxHP(c5) / 3)) {
                Perform(c5, "say", "....");
                Perform(c5, "pause", 3);
                Perform(c5, "say", "好久没有这种感觉了！");
                trg5 = 1;
                time3 = GetGameTime() + 5;
            }
        }

        if (trg5 == 1 && GetGameTime() > time3) {
            c5 = GetChar("韩棠");
            if (GetHP(c5) > 0) {
                Perform(c5, "use", 36);
                Perform(c5, "use", 20);
                Perform(c5, "use", 3);
                Perform(c5, "use", 4);
                Perform(c5, "use", 5);
                Perform(c5, "use", 8);
                Perform(c5, "use", 19);
                Perform(c5, "use", 34);
                Perform(c5, "use", 40);
                Perform(c5, "use", 41);
                Perform(c5, "use", 15);
                Perform(c5, "use", 15);
                Perform(c5, "use", 15);
            }
            trg5 = 2;
        }

        //生成了援军后，援军对白.
        if (trg2 == 2) {
            c4 = GetChar("叶翔");
            c5 = GetChar("石群");
            c1 = GetChar("罗江");
            c0 = GetChar("屠城");
            if (GetHP(c1) > 0)
                ChangeBehavior(c4, "kill", c1);
            else if (GetHP(c0) > 0)
                ChangeBehavior(c4, "kill", c0);
            Perform(c4, "say", "上吧");
            Perform(c4, "pause", 10);
            Perform(c4, "say", "十二飞鹏帮少有的高手竟然都在，可够热闹");
            Perform(c4, "pause", 2);

            c3 = GetChar("萧安");
            c2 = GetChar("原冲");
            if (GetHP(c3) > 0) {
                ChangeBehavior(c5, "kill", c3);
                Perform(c5, "say", "我缠住萧安,你去解决另几个高手吧！");
            } else if (GetHP(c2) > 0) {
                ChangeBehavior(c5, "kill", c2);
                Perform(c5, "say", "我缠住原冲,你去解决剩下那几个高手吧！");
            } else {
                ChangeBehavior(c5, "follow", player);
                Perform(c5, "say", "我去看看星魂");
            }
            Perform(c5, "pause", 3);
            Perform(c5, "say", "这次的马蜂窝可捅的不小啊，回去好歹也该请我喝个痛快！");
            Perform(c5, "pause", 5);
            trg2 = 3;
        }

        //一起技能释放硬切换
        if (trg3 == 0 && skillgroup > 0 && GetGameTime() > timeSkill) {
            c1 = GetChar("屠城");
            c2 = GetChar("罗江");
            c3 = GetChar("萧安");
            c4 = GetChar("原冲");
            if (c1 >= 0 && c2 >= 0 && c3 >= 0 && c4 >= 0) {
                //4人都离角色300码以内
                float maxDistance = 300.0f;
                if (PlayerDistance(c1, 0) <= maxDistance && PlayerDistance(c2, 0) <= maxDistance && PlayerDistance(c3, 0) <= maxDistance && PlayerDistance(c4, 0) <= maxDistance) {
                    //任意一个人得到满怒气，则全体满怒气.一起开大.
                    if (GetAngry(c1) == 100 || GetAngry(c2) == 100 || GetAngry(c3) == 100 || GetAngry(c4) == 100) {
                        int maxCount = 0;
                        if (GetHP(c1) > 0) {
                            Perform(c1, "faceto", 0);
                            Perform(c1, "use", 34);//嗜血+攻
                            Perform(c1, "use", 8);//怒气MAX
                            maxCount++;
                        }

                        if (GetHP(c2) > 0) {
                            Perform(c2, "faceto", 0);
                            Perform(c2, "use", 34);//嗜血+攻
                            Perform(c2, "use", 8);//怒气MAX
                            maxCount++;
                        }

                        if (GetHP(c3) > 0) {
                            Perform(c3, "faceto", 0);
                            Perform(c3, "use", 34);//嗜血+攻
                            Perform(c3, "use", 8);//怒气MAX
                            maxCount++;
                        }

                        if (GetHP(c4) > 0) {
                            Perform(c4, "faceto", 0);
                            Perform(c4, "use", 34);//嗜血+攻
                            Perform(c4, "use", 8);//怒气MAX
                            maxCount++;
                        }

                        int talked = 0;
                        int delay = 1;
                        if (GetHP(c1) > 0 && CanUseSkill(c1)) {
                            Perform(c1, "skill");
                            Perform(c1, "pause", 1);
                            talked = 1;
                            Perform(c1, "say", "看招");
                            delay += 1;
                        }

                        if (GetHP(c2) > 0 && CanUseSkill(c2)) {
                            Perform(c2, "skill");
                            if (talked == 0) {
                                Perform(c2, "say", "这下可激怒我了");
                                talked = 1;
                            }
                            Perform(c2, "pause", delay);
                            delay += 1;
                        }

                        if (GetHP(c3) > 0 && CanUseSkill(c3)) {
                            Perform(c3, "skill");
                            if (talked == 0) {
                                Perform(c3, "say", "去死吧");
                                talked = 1;
                            }
                            Perform(c3, "pause", delay);
                            delay += 1;
                        }

                        if (GetHP(c4) > 0 && CanUseSkill(c4)) {
                            Perform(c4, "skill");
                            if (talked == 0) {
                                Perform(c4, "say", "这次一定要分出个胜负");
                                talked = 1;
                            }
                            Perform(c4, "pause", delay);
                        }
                        skillgroup -= 1;
                        timeSkill = GetGameTime() + 45;//45秒触发间隔
                    }
                }
            }
        }
        return 0;
    }

    public override void Scene_OnInit() {
        InitBoxes(g_iNumBoxes);
        InitBBoxes(g_iNumBBoxes);
        InitChairs(g_iNumChairs);
        InitDeskes(g_iNumDeskes);
        InitJugs(g_iNumJugs);
    }
}

//所有的新关卡，不读取,通用模板，无剧情，仅用来测试
public class LevelScript_sn1000 : LevelScriptBase
{
    public override int GetRoundTime() {  return 100; }
    public override int GetPlayerSpawn() { return 0; }
    public override int GetPlayerSpawnDir() { return 0; }
    public override int GetPlayerWeapon() { return _GetPlayerWeapon(); }
    public override int GetPlayerWeapon2() { return _GetPlayerWeapon2(); }
    public override int GetPlayerMaxHp() { return _GetPlayerMaxHp(); }
    public override int GetPlayerModel() { return _GetPlayerModel(); }
    public override string GetPlayerName() { return _GetPlayerName(); }

    public override void OnStart()
    {
        base.OnStart();
    }
}

//威震八方-教学关卡
public class LevelScript_sn31 : LevelScript_sn22
{
    int RoundTime = 60;
    int PlayerSpawn = 14;
    int PlayerSpawnDir = 90;
    int PlayerWeapon = 6;
    int PlayerWeapon2 = 0;
    int PlayerHP = 3000;
    //"flat_roofR65" 巽 xun
    //"flat_roofR65" 坎 kan
    //"flat_roofR69" 艮 gen
    //"flat_roofR69" 坤 kun
    //"flat_roofR71" 兑 dui
    //"flat_roofR72" 乾 qian
    //"flat_roofR73" 离 li
    //"flat_roofR74" 震 zhen
    public override int GetRoundTime() { return RoundTime; }
    public override int GetPlayerSpawn() { return PlayerSpawn; }
    public override int GetPlayerSpawnDir() { return PlayerSpawnDir; }
    public override int GetPlayerWeapon() { return PlayerWeapon; }
    public override int GetPlayerWeapon2() { return PlayerWeapon2; }
    public override int GetPlayerMaxHp() { return PlayerHP; }

    public override void OnStart()
    {
        for (int i = 1; i < 9; i++)
        {
            string s = string.Format("npc31_0{0}", i);
            AddNPC(s);
        }
        AddNPC("npc31_13");
        Vector3 vec = CombatData.Ins.GLevelSpawn[14];
        vec.x += 375;
        U3D.MovePlayer("高寄萍", vec);
        U3D.RotatePlayer("高寄萍", -90);
        int player = U3D.GetChar("player");
        MeteorUnit uplayer = U3D.GetUnit(player);
        if (uplayer != null) {
            vec = CombatData.Ins.GLevelSpawn[14];
            vec.x += 415;
            vec.y = uplayer.transform.position.y;
            U3D.MovePlayer("player", vec);
            U3D.RotatePlayer("player", 90);
        }
        //强制AI进入各区域
        for (int i = 1; i < 9; i++) {
            base.TeamBTransferToArena(i, i - 1);
        }
        base.OnStart();
    }

    public override void TeamATransferToArena(int characterid, int arena) {
        MeteorUnit unit = U3D.GetUnit(characterid);
        if (unit != null && unit.StateMachine != null) {
            return;
        }
        base.TeamATransferToArena(characterid, arena);
    }

    public override void TeamATransferFromArena(int characterid, int arena) {
        MeteorUnit unit = U3D.GetUnit(characterid);
        if (unit != null && unit.StateMachine != null) {
            return;
        }
        base.TeamATransferFromArena(characterid, arena);
    }

    public override void TeamBTransferToArena(int characterid, int arena) {
        MeteorUnit unit = U3D.GetUnit(characterid);
        if (unit != null && unit.StateMachine != null) {
            return;
        }
        base.TeamBTransferToArena(characterid, arena);
    }

    public override void TeamBTransferFromArena(int characterid, int arena) {
        MeteorUnit unit = U3D.GetUnit(characterid);
        if (unit != null && unit.StateMachine != null) {
            return;
        }
        base.TeamBTransferFromArena(characterid, arena);
    }

    int trg0 = 0;
    int t0 = 0;
    /*
    乾 qián    gièng、khiân khèn    kin4 Càn 건（geon）	けん（ken）
    坤 kūn kuǒng、khun khûn    kwan1 Khôn    곤（gon）	こん（kon）
    震 zhèn    cīng、chìn chṳ́n   zan3 Chấn    진（jin）	しん（shin）
    巽 xùn sóng、sùn sun seon3 Tốn 손（son）	そん（son）
    坎 kǎn kāng、khá khám    ham2 Khảm    감（gam）	かん（kan）
    离 lí  liê、lî lì  lei4 Ly  이（i）	り（ri）
    艮 gèn góng、kùn ken gan3 Cấn 간（gan）	ごん（gon）
    兑 duì dō̤i、toē tui deoi3 Đoài    태（tae）	だ（da）
    */
    public override int OnUpdate()
    {
        int player = GetChar("player");
        if (player < 0)
        {
            return 0;
        }

        int c;

        if (trg0 == 0)
        {
            c = GetChar("高寄萍");
            if (c >= 0)
            {
                if (U3D.Distance(player, c) <= 65)
                {
                    Perform(c, "say", "如果受伤了可以来我这里恢复,找一把喜欢的武器开始练习吧");
                    Perform(c, "pause", 5);
                    Perform(c, "say", "具体招式可查看系统-出招表");
                    Perform(c, "pause", 5);
                    Perform(c, "say", "或者下上A，空中下上A，下上上A(绝招)");
                    Perform(c, "pause", 5);
                    Perform(c, "say", "只有基本的攻击和技能，一般是方向键+攻击");
                    Perform(c, "pause", 5);
                    Perform(c, "say", "关于[火枪],[飞镖],[飞轮]");
                    Perform(c, "pause", 5);
                    Perform(c, "say", "可以通过卦位外的传送门,进入到该武器的教学");
                    Perform(c, "pause", 5);
                    Perform(c, "say", "当你不清楚什么武器招式如何释放的时候");
                    Perform(c, "pause", 5);
                    Perform(c, "say", "里面的人会训练你各种武器相关的招式");
                    Perform(c, "pause", 5);
                    Perform(c, "say", "[重锤],[拳套],[忍刀],[双刺]");
                    Perform(c, "pause", 5);
                    Perform(c, "say", "分别代表了 [长剑],[大刀],[长枪],[匕首]");
                    Perform(c, "pause", 5);
                    Perform(c, "say", "星,这里有8个卦位！=.=");
                    Perform(c, "pause", 5);
                    Perform(c, "faceto", player);
                    trg0 = 1;
                    PlayerPerform("block", 0);
                    PlayerPerform("pause", 20);
                    PlayerPerform("block", 1);
                    t0 = GetGameTime();
                }
            }
        }

        if (trg0 == 1)
        {
            c = GetChar("高寄萍");
            if (c >= 0)
            {
                if (GetGameTime() - t0 > 30 && GetHP(player) < (GetMaxHP(player) / 2) && U3D.Distance(player, c) <= 65)
                {
                    PlayerPerform("use", 15);
                    PlayerPerform("pause", 3);
                    Perform(c, "help", player);
                    Perform(c, "say", "星,别动我给你疗伤！");
                    Perform(c, "faceto", player);
                    t0 = GetGameTime();
                }
            }

            //如果所有人全部都打败
            if (U3D.AllEnemyDead())
            {
                trg0 = 2;
                GameOver(1);
            }
        }

        //当玩家离开各个卦位时，停止AI

        return 0;
    }
}

//秦皇陵
public class LevelScript_sn02_1:LevelScript_sn02
{
    public override void OnStart()
    {
        AddNPC("npc02_01");
        base.OnStart();
    }

    int tag1 = 0;
    public override int OnUpdate()
    {
        if (tag1 == 0)
        {
            int c1 = GetChar("皇陵使");
            ChangeBehavior(c1, "patrol", 1, 2, 3);
            tag1 = 1;
        }
        if (tag1 == 1) {
            int player = GetChar("player");
            if (player < 0) {
                return 0;
            }
            int c1 = GetChar("皇陵使");
            ChangeBehavior(c1, "kill", player);
            tag1 = 2;
        }
        return 0;
    }

    public override GameResult OnUnitDead(MeteorUnit deadUnit) {
        return GameResult.None;
    }
}

//单人录像测试
public class LevelScript_sn02_2:LevelScript_sn02 {
    public override void OnStart() {
        base.OnStart();
    }

    public override int OnUpdate() {
        return 0;
    }

    public override GameResult OnUnitDead(MeteorUnit deadUnit) {
        return GameResult.None;
    }
}
