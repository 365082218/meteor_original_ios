using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Outfit7.Sequencer {
    public class SequencerPropertyEvent : SequencerContinuousEvent {
        public PropertyAnimationData AnimationData;
        public BaseProperty Property;
        public float Multiplier = 1;
        public float Offset = 0;
        public bool DoRemap = false;
        public float Remap0 = 0;
        public float Remap1 = 1;
        public float Repeat = 1;
        public bool Bounce = false;

        public override void OnInit() {
        }

        protected override void OnLiveInit(SequencerSequence sequence) {
            if (AnimationData != null)
                AnimationData.LiveInit(sequence);
        }

        public override void OnPreplay() {
            OnEnter(null, 0, 0);
            OnProcess(null, 0, 0);
        }

        public override void OnEnter(List<Component> components, float absoluteTime, float normalizedTime) {
            if (AnimationData == null)
                return;
            AnimationData.Init(Property);
        }

        public override void OnProcess(List<Component> components, float absoluteTime, float normalizedTime) {
            if (AnimationData == null || Property == null)
                return;
            AnimationData.Process(Property, absoluteTime, Duration, Multiplier, Offset, DoRemap, Remap0, Remap1, Repeat, Bounce);
        }

        public override void OnExit(List<Component> components) {
        }
    }
}