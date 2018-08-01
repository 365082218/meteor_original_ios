using Outfit7.Logic;
using Outfit7.Logic.StateMachineInternal;
using System;

namespace Outfit7.Audio.MusicInternal {

    [Serializable]
    public class MusicStateMachineState : State {

        public override bool OnEnter(StateMachine stateMachine, Layer layer, int layerIndex, LayerStateData stateData, bool callOnEnter) {
            base.OnEnter(stateMachine, layer, layerIndex, stateData, callOnEnter);
            return true;
        }

        public override void OnExit(StateMachine stateMachine, Layer layer, int layerIndex, LayerStateData stateData) {
            base.OnExit(stateMachine, layer, layerIndex, stateData);
        }

        public override void OnUpdateReferernces(StateMachine stateMachine, Layer layer) {
            base.OnUpdateReferernces(stateMachine, layer);
        }

        public override void OnUpdate(StateMachine stateMachine, Layer layer, LayerStateData stateData, ref float normalizedSpeed) {
            base.OnUpdate(stateMachine, layer, stateData, ref normalizedSpeed);
        }
    }

}
