using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Outfit7.Util;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Outfit7.Sequencer {
    public class SequencerAnimationClipEvent : SequencerContinuousEvent {
        public EaseManager.Ease EaseType;
        public AnimationClip AnimationClip;
        public AnimationState AnimationState;

        public override void OnInit() {
            ComponentType = typeof(UnityEngine.Animation);
        }

        public override void OnEnter(List<Component> components, float absoluteTime, float normalizedTime) {
            UnityEngine.Animation animation = (UnityEngine.Animation)components[0];
            if (animation == null)
                return;
            if (AnimationClip == null) {
                O7Log.Error("Animation clip missing in {0} sequence at {1}", gameObject.name, absoluteTime);
                return;
            }
            animation.AddClip(AnimationClip, AnimationClip.name);
            AnimationState = animation[AnimationClip.name];
        }

        public override void OnProcess(List<Component> components, float absoluteTime, float normalizedTime) {
            if (AnimationState == null)
                return;
            UnityEngine.Animation animation = (UnityEngine.Animation)components[0];
            if (animation == null)
                return;
            if (AnimationClip == null) {
                O7Log.Error("Animation clip missing in {0} sequence at {1}", gameObject.name, absoluteTime);
                return;
            }
            AnimationState.normalizedTime = normalizedTime;
            AnimationState.clip.SampleAnimation(animation.gameObject, AnimationState.clip.length * normalizedTime);
        }

        public override void OnExit(List<Component> components) {
            UnityEngine.Animation animation = (UnityEngine.Animation)components[0];
            if (animation == null)
                return;
            animation.Stop();
            AnimationState = null;
        }
    }
}   