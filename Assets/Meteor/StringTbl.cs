using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public enum StringIden
{
    MeteorDream = 1,
    DreamAgain = 2,
    NotInPlaying = 3,//还未进入游戏无法存档
    Restart = 100,
    Save = 101,
    Load = 102,
    CombatMode = 103,
    Help = 104,
    Guide = 105,
    WantedList = 106,
    CustomSer = 107,
    Language = 108,
    GoBack = 201,
    System = 202,
    Cancel = 203,
    Chinese = 204,
    English = 205,
    SelectLangTip = 206,
    ConstructionUpgrade = 207,
    ConstructionComplete = 208,
    NoMorePrisoner = 209,
    Upgrade = 300,
    Build = 301,
    Role = 302,
    Item = 303,
    Battle = 304,
    Tael = 305,
    Backpack = 306,
    Return = 307,
    Food = 308,
    Location = 309,
    Accident = 310,
    Prison = 311,
    CanMake = 312,
    CanRecruit = 313,
    GraspSkill = 314,
    Head = 315,
    Armor = 316,//护甲
    Hand = 317,
    Leg = 318,
    Accessories = 319,
    Weapon = 320,
    Armors = 321,//防具
    Damage = 322,//攻击力
    Defence = 323,//防御
    Mana = 324,//内力
    Speed = 325,//速度
    Equipment = 326,//装备
    Skill = 327,//技能
    Close = 328,//关闭
    Level = 329,//水平
    Exp = 330,//经验
    NeedExp = 331,//升级所需经验
    LeaderShip = 332,//领导力
    SkillExp = 333,//历练
    Famous = 334,//声望
    PrevPage = 335,
    NextPage = 336,
    Use = 337,
    Job = 338,
    Takeoff = 339,
    Puton = 340,
    Life = 341,
    Armylife = 342,
    ArmyDamage = 343,
    ArmyDef = 344,
    SaveWitchState = 345,//使用哪个存档开始新游戏
    LoadWitchState = 346,//加载哪个存档

    Open = 401,//开启活动平台-
    Enter = 402,//进入那个地图
    Update = 403,//更新
}

public static class TextHelper
{
    public static void ChangeLang(this Text txt)
    {
        LangItem ctrl = txt.GetComponent<LangItem>();
        if (ctrl != null)
        {
            string text = ctrl.GetLangStr();
            //如果找不到字串，那么不改变.
            if (text != null && !string.IsNullOrEmpty(text))
                txt.text = text;
        }
    }
}

public static class StringTbl {

    //描述闪避的随机字符串
    public static string[] shenfa = { "身法矫健", "轻灵洒脱"};
    //描述暴击的随机字符串
    public static string[] baoji =  { "间不容发", "千钧一发"};
    //描述人物实力较弱小
    public static string[] lv = { "土鸡瓦狗", "微不足道", "不值一提", "不足挂齿" };
    //描述人物实力较强
    public static string[] lv0 = { "盖世无双", "一代宗师", "万人之敌"};
    public static string money = "银两";
    public static string exp = "经验";
    public static string[] unit = { "", "件", "把", "份", "", "本", "个" };
    public static string unitPrefix = "「";
    public static string unitSuffix = "」";


}