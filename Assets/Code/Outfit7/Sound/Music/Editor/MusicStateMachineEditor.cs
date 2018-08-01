using UnityEngine;
using System.Collections;
using UnityEditor;
using Outfit7.Logic;
using Outfit7.Logic.StateMachineInternal;
using System;
using Outfit7.Audio.MusicInternal;

namespace Outfit7.Audio {

    [CustomEditor(typeof(MusicStateMachine), true)]
    public partial class MusicStateMachineEditor : StateMachineEditor {

        public override string TitleName { get { return "Music State Machine"; } }

        protected override Layer AddLayerInternal() {
            MusicStateMachine stateMachine = target as MusicStateMachine;
            return AddGeneric(ref stateMachine.InternalLayers);
        }

        protected override State AddStateInternal() {
            MusicStateMachine stateMachine = target as MusicStateMachine;
            return AddGeneric(ref stateMachine.InternalStates);
        }

        protected override void RemoveLayerInternal(Layer layer) {
            MusicStateMachine stateMachine = target as MusicStateMachine;
            RemoveGeneric(ref stateMachine.InternalLayers, layer as MusicStateMachineLayer);
        }

        protected override void RemoveStateInternal(State state) {
            MusicStateMachine stateMachine = target as MusicStateMachine;
            RemoveGeneric(ref stateMachine.InternalStates, state as MusicStateMachineState);
        }
    }
}