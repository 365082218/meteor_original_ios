using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerManager {
    public static int PlayerMask;//怪物和玩家一起
    public static int Player;//能控制的角色
    public static int MonsterMask;//
    public static int Scene;//场景碰撞
    public static int SceneMask;
    public static int Default;//场景碰撞
    public static int AllSceneMask;//场景+默认
    public static int Flight;//飞行道具-飞镖
    public static int Bone;//受击盒
    public static int Weapon;//攻击盒
    public static int RenderModel;//UI渲染3D层-用来显示装备选择界面的预览图
    public static int Trigger;//机关层-场景道具等
    public static int ShootMask;//枪械射击时，会被bone,scene,trigger挡住
    public static int DetectAll;//既能攻击又能受击，但是只处理攻击部分的.
    public static int WayPoint;//路点层
    public static int WayPointMask;
    public static int DeadMask;
    public static void Init() {
        Player = LayerMask.NameToLayer("Player");
        PlayerMask = 1 << Player;
        AllSceneMask = 1 << LayerMask.NameToLayer("Scene") | 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("Trigger");
        Flight = LayerMask.NameToLayer("Flight"); ;
        Scene = LayerMask.NameToLayer("Scene");
        SceneMask = 1 << Scene;
        Bone = LayerMask.NameToLayer("Bone");
        Weapon = LayerMask.NameToLayer("Weapon");
        Trigger = LayerMask.NameToLayer("Trigger");
        Default = LayerMask.NameToLayer("Default");
        RenderModel = LayerMask.NameToLayer("RenderModel");
        ShootMask = 1 << LayerMask.NameToLayer("Scene") | 1 << LayerMask.NameToLayer("Bone") | 1 << LayerMask.NameToLayer("Trigger") | 1 << LayerMask.NameToLayer("Weapon");
        DetectAll = LayerMask.NameToLayer("DetectAll");
        WayPoint = LayerMask.NameToLayer("WayPoint");
        WayPointMask = 1 << WayPoint;
    }
}
