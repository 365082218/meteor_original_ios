using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Outfit7.Audio {
    public class AudioEventBehaviour : MonoBehaviour {

        public AudioEventData AudioEventData = null;
        public float Volume = 1.0f;
        public float Pitch = 1.0f;
        [HideInInspector]
        public AudioEvent AudioEvent = null;

        public void OnEnable() {
            if (AudioEventData != null) {
                if (AudioEvent == null) {
                    AudioEvent = AudioEventManager.Instance.CreateAudioEvent(AudioEventData.GetAudioEventData(), transform, EAudioEventAttributes.AutoDelete);
                    if (AudioEvent != null) {
                        AudioEvent.OnEventDestroy += OnAudioEventDelete;
                    }
                }
            }
            if (AudioEvent != null) {
                AudioEvent.Volume = Volume;
                AudioEvent.Pitch = Pitch;
                if (!AudioEvent.IsPlaying) {
                    AudioEvent.Play();
                }
            }
        }

        void OnAudioEventDelete(AudioEvent ae) {
            AudioEvent.OnEventDestroy -= OnAudioEventDelete;
            AudioEvent = null;
        }

        public void OnDisable() {
            if (AudioEvent != null) {
                AudioEvent.OnEventDestroy -= OnAudioEventDelete;
                AudioEvent.Stop();
                AudioEvent.EventAttributes |= EAudioEventAttributes.AutoDelete;
                AudioEvent = null;
            }
        }
    }
}
