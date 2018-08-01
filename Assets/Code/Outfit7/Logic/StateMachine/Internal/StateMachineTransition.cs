using System;
using System.Collections.Generic;
using UnityEngine;

namespace Outfit7.Logic.StateMachineInternal {

    [Serializable]
    public class Transition : Base {
        public int DestinationStateIndex = -1;

        public State DestinationState { get; set; }

        public bool Enabled = true;
        public bool Atomic = true;
        public bool UpdateTime = true;
        public float Duration;
        public AnimationCurve AnimationCurve;
        public Condition[] Conditions;
        public InvokeEvent[] PreEnterEvents;
        public InvokeEvent[] PostEnterEvents;

        public void ResetTriggers() {
            for (int i = 0; Conditions != null && i < Conditions.Length; i++) {
                Conditions[i].ResetTrigger();
            }
        }
    }

}