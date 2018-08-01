using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Outfit7.Audio;
using Outfit7.Util;

namespace Outfit7.Sequencer {
    public class SequencerAudioEvent : SequencerContinuousEvent {
        public AudioEventData AudioEventData;
        public AnimationCurve VolumeCurve = new AnimationCurve(new Keyframe[]{ new Keyframe(0, 1), new Keyframe(1, 1) });
        public AnimationCurve PitchCurve = new AnimationCurve(new Keyframe[]{ new Keyframe(0, 1), new Keyframe(1, 1) });
        private AudioEvent AudioEvent;

        public override void OnInit() {
            //ComponentType = typeof(UnityEngine.Animation);
        }

        public AudioEvent GetAudioEvent() {
            return AudioEvent;
        }

        public void OnAudioEventDestroy(AudioEvent ae) {
            Assert.State(ae == AudioEvent, "Wrong AudioEvent object: ae:{0}, AudioEvent: {1}", ae == null ? "null" : ae.gameObject.name, AudioEvent == null ? "null" : AudioEvent.gameObject.name);
            AudioEvent.OnEventDestroy -= OnAudioEventDestroy;
            AudioEvent = null;
        }

        public override void OnEnter(List<Component> components, float absoluteTime, float normalizedTime) {
            if (AudioEventData == null)
                return;

            if (Application.isPlaying) {
                AudioEvent = AudioEventManager.Instance.CreateAudioEvent(AudioEventData.GetAudioEventData(), null, EAudioEventAttributes.AutoDelete);
                if (AudioEvent != null) {
                    AudioEvent.OnEventDestroy += OnAudioEventDestroy;
                    AudioEvent.Play();
                }
            } else {
#if UNITY_EDITOR
                AudioEvent = AudioEventManager.CreateAudioEventForEditor();
                if (AudioEvent != null) {
                    AudioEvent.Initialize(AudioEventData);
                    AudioEvent.Play();
                }
#endif
            }
        }

        public override void OnProcess(List<Component> components, float absoluteTime, float normalizedTime) {
            if (AudioEvent == null)
                return;

            AudioEvent.Volume = VolumeCurve.Evaluate(normalizedTime);
            AudioEvent.Pitch = PitchCurve.Evaluate(normalizedTime);
        }

        public override void OnExit(List<Component> components) {
            if (AudioEvent == null) {
                return;
            }

            AudioEvent.Stop(true);
            if (Application.isPlaying) {
                AudioEventManager.Instance.DeleteAudioEvent(AudioEvent);
            } else {
                GameObject.DestroyImmediate(AudioEvent.gameObject, true);
            }
        }
    }
}   