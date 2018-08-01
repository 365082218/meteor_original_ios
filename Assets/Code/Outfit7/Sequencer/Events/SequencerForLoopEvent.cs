using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Outfit7.Logic.StateMachineInternal;


namespace Outfit7.Sequencer {
    public class SequencerForLoopEvent : SequencerTriggerEvent {
        //public Parameter GoToParam;
        //public float GoToTime = 0f;
        public ParameterIntField Cycle = new ParameterIntField(3);
        public ParameterFloatField GoToTime = new ParameterFloatField(0.0f);
        public SequencerSequence AffectingSequence;
        private int CurrentCycle = -1;

        public override bool IgnoreObjects() {
            return true;
        }

        public string GetRemainingCycleCount() {
            return CurrentCycle.ToString();
        }

        protected override void OnLiveInit(SequencerSequence sequence) {
            Cycle.LiveInit(sequence.Parameters);
            GoToTime.LiveInit(sequence.Parameters);
            CurrentCycle = -1;
        }

        public override void OnTrigger(List<Component> components, float currentTime) {
            if (AffectingSequence == null)
                return;
            GoToTime.Init(AffectingSequence.GetCurrentTime());
            if (CurrentCycle == -1) {
                CurrentCycle = Cycle.Value;
                Cycle.Init(null);
            }
            if (CurrentCycle == 0) {
                CurrentCycle = -1;
            } else {
                AffectingSequence.MoveToTime(GoToTime.Value, AffectingSequence.GetCurrentTime() - StartTime);
                CurrentCycle--;
            }
        }
    }
}   