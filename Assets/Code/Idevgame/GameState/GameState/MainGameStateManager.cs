using System;
using System.Collections.Generic;
using Idevgame.StateManagement;
using Idevgame.StateManagement.GameStateManagement;
using Idevgame.Util;

namespace Idevgame.GameState {
    public class MainGameStateManager : GameStateManager<BaseGameState,GameAction> {

        public static bool BlockUpdatesOnStart = true;
        public MainGameStateManager() {
            //SettingsState = new SettingsState(this);
        }

        public override void Init() {
            ApplyProperties();
            base.Init();
        }

        public void ApplyProperties() {
           
        }

        protected override Pair<BaseGameState,Object> GetEntryStateAndData() {
            return new Pair<BaseGameState,Object>(null, null);
        }

        public BaseGameState GetEntryState() {
            return GetEntryStateAndData().First;
        }

        public override void OnUpdate() {
            if (BlockUpdatesOnStart)
                return;
            base.OnUpdate();
        }

        protected override void OnStateChanged() {
            base.OnStateChanged();
        }

        protected override bool BlockStateChange(BaseGameState newState) {
            return base.BlockStateChange(newState);
        }


        public override bool FireAction(GameAction gameAction, object data) {
            bool triggered = base.FireAction(gameAction, data);
            return triggered;
        }

        protected override void OnStateExitEvent(BaseGameState currentState, BaseGameState state, object data) {
            base.OnStateExitEvent(currentState, state, data);
        }

    }
}
