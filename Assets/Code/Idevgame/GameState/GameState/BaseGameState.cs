
using System;
using Idevgame.StateManagement;

namespace Idevgame.GameState {
    public abstract class BaseGameState : StateManager<BaseGameState, GameAction>.State {
        public MainGameStateManager GameStateManager { get; private set; }

        protected BaseGameState(MainGameStateManager stateManager) : base(stateManager) {
            GameStateManager = stateManager;
        }

        public override void OnEnter(BaseGameState previousState, object data) {
        }

        public override void OnExit(BaseGameState nextState, object data) {
        }

        public override void OnAction(GameAction gameAction, object data) {
            switch (gameAction) {
                default:
                    break;
            }
        }
    }
}
