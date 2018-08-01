using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Outfit7.Logic.StateMachineInternal;


namespace Outfit7.Sequencer {
    public class SequencerStopSequenceEvent : SequencerTriggerEvent {
        public SequencerSequence AffectingSequence;

        public override bool IgnoreObjects() {
            return true;
        }

        public override void OnTrigger(List<Component> components, float currentTime) {
            if (AffectingSequence == null)
                return;
            AffectingSequence.Stop(StartTime, this);
        }
    }
}   