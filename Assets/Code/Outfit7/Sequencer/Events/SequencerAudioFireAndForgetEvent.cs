using UnityEngine;
using System.Collections.Generic;
using Outfit7.Audio;

namespace Outfit7.Sequencer {
    public class SequencerAudioFireAndForgetEvent : SequencerTriggerEvent {
        public AudioEventData AudioEventData;
        public float Volume = 1f;
        public float Pitch = 1f;

        public override void OnInit() {
//            ComponentType = typeof(AudioEvent);
        }

        public override void OnTrigger(List<Component> components, float currentTime) {
            if (AudioEventData == null)
                return;

            if (Application.isPlaying) {
                AudioEvent audioEvent = AudioEventManager.Instance.CreateAudioEvent(AudioEventData);
                if (audioEvent != null) {
                    audioEvent.Volume = Volume;
                    audioEvent.Pitch = Pitch;              
                    audioEvent.Play();
                    audioEvent.EventAttributes |= EAudioEventAttributes.AutoDelete; 
                }
            }
        }
    }
}   