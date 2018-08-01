using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Outfit7.Logic.StateMachineInternal;


namespace Outfit7.Sequencer {
    public class SequencerTimeScaleEvent : SequencerTriggerEvent {
        //public Parameter GoToParam;
        //public float GoToTime = 0f;
        public ParameterFloatField TimeScale = new ParameterFloatField(1.0f);
        public SequencerSequence AffectingSequence;

        public override bool IgnoreObjects() {
            return true;
        }

        public override void OnTrigger(List<Component> components, float currentTime) {
            if (AffectingSequence == null)
                return;
            TimeScale.Init(AffectingSequence.TimeScale);
            AffectingSequence.TimeScale = TimeScale.Value;
        }
    }
}   