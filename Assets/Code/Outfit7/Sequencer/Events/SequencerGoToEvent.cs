using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Outfit7.Logic.StateMachineInternal;


namespace Outfit7.Sequencer {
    public class SequencerGoToEvent : SequencerTriggerEvent {
        //public Parameter GoToParam;
        //public float GoToTime = 0f;
        public ParameterFloatField GoToTime = new ParameterFloatField(0.0f);
        public SequencerSequence AffectingSequence;

        public override bool IgnoreObjects() {
            return true;
        }

        public override void OnTrigger(List<Component> components, float currentTime) {
            if (AffectingSequence == null)
                return;
            GoToTime.Init(AffectingSequence.GetCurrentTime());
            AffectingSequence.MoveToTime(GoToTime.Value, AffectingSequence.GetCurrentTime() - StartTime);
        }
    }
}   