using Idevgame.Debugs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[DebugClass("MeteorUnit")]
public partial class GameBattleEx:DebugInstance
{
    public string Name
    {
        get
        {
            return "战斗";
        }
    }

    [DebugMethod("胜利")]
    public void GameWin()
    {
        GameOver(1);
    }

    [DebugMethod("失败")]
    public void GameLose()
    {
        GameOver(0);
    }

    [DebugMethod("平")]
    public void GameTimeOut()
    {
        GameOver(2);
    }
}
