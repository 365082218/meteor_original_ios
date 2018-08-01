using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Outfit7.Sequencer {
    public class PlayAnimatorStateEvent : SequencerContinuousEvent {
        public string StateName = "";
        public string SpeedParamName = "";
        private int StateHash = -1;
        private int ParamHash = -1;
        private float ClipLength = -1;

        public override void OnInit() {
            ComponentType = typeof(UnityEngine.Animator);
        }

        public override void OnEnter(List<Component> components, float absoluteTime, float normalizedTime) {
            UnityEngine.Animator animator = (UnityEngine.Animator) components[0];
            if (animator == null)
                return;
            if (StateName == "")
                return;
            StateHash = Animator.StringToHash(StateName);
            animator.Play(StateHash);

            if (SpeedParamName == "")
                return;
            ParamHash = Animator.StringToHash(SpeedParamName);
        }

        public override void OnProcess(List<Component> components, float absoluteTime, float normalizedTime) {
            UnityEngine.Animator animator = (UnityEngine.Animator) components[0];
            if (animator == null)
                return;

            if (ParamHash != -1) {
                if (ClipLength < 0) {
                    if (animator.GetCurrentAnimatorClipInfo(0).Length <= 0)
                        return;
                    AnimatorClipInfo clipInfo = animator.GetCurrentAnimatorClipInfo(0)[0];
                    ClipLength = clipInfo.clip.length;
                    animator.SetFloat(ParamHash, ClipLength / Duration);

                } else {
                    animator.SetFloat(ParamHash, ClipLength / Duration);
                }
            }
                

            #if UNITY_EDITOR
            if (!Application.isPlaying && AnimationMode.InAnimationMode()) {
                /*UnityEngine.Animation animation = (UnityEngine.Animation) components[0];
                AnimationMode.SampleAnimationClip(animation.gameObject, AnimationClip, AnimationClip.length * EaseManager.Evaluate(EaseType, normalizedTime, 1, 1, 1));*/
            }
            #endif
        }

        public override void OnExit(List<Component> components) {
        }
    }
}   