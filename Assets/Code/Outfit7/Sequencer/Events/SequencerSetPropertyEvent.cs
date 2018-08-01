using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Outfit7.Sequencer {
    public class SequencerSetPropertyEvent : SequencerTriggerEvent {
        public PropertyAnimationData AnimationData;
        public BaseProperty Property;

        public override void OnInit() {
        }

        protected override void OnLiveInit(SequencerSequence sequence) {
            if (AnimationData != null) {
                AnimationData.LiveInit(sequence);
            }
        }

        public override void OnPreplay() {
            OnTrigger(null, 0);
        }

        public override void OnTrigger(List<Component> components, float currentTime) {
            if (AnimationData == null || Property == null)
                return;
            AnimationData.Init(Property);
            AnimationData.Process(Property, 0, 0, 1, 0, false, 0, 1, 1, false);
        }
    }
}