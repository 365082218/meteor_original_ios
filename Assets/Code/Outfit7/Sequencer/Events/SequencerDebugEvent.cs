using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Outfit7.Logic.StateMachineInternal;


namespace Outfit7.Sequencer {
    public class SequencerDebugEvent : SequencerContinuousEvent {
        public Texture2D test;

        public override void OnInit() {
        }

        public override void OnEnter(List<Component> components, float absoluteTime, float normalizedTime) { 
        }

        public override void OnProcess(List<Component> components, float absoluteTime, float normalizedTime) {
            
        }

        public override void OnExit(List<Component> components) {
        }
    }
}   