using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Idevgame.GameState;
/// <summary>
/// 最开始进入游戏时，出现开屏图，以及加载进度条
/// </summary>
public class StartupState: BaseGameState{
    StartupUiController controller;
    protected StartupState(MainGameStateManager stateManager) : base(stateManager) {
    }

    public override void OnEnter(BaseGameState previousState, object data)
    {
        GameObject ui = GameObject.Instantiate(ResMng.LoadPrefab("") as GameObject);
        controller = ui.GetComponent<StartupUiController>();
    }

    public override void OnExit(BaseGameState nextState, object data)
    {
    }

    public override void OnAction(GameAction gameAction, object data)
    {
        switch (gameAction)
        {
            default:
                break;
        }
    }
}
