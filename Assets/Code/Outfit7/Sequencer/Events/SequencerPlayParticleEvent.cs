using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Outfit7.Logic.StateMachineInternal;


namespace Outfit7.Sequencer {
    public class SequencerPlayParticleEvent : SequencerTriggerEvent {
        public ParticleSystem ParticleSystem;
        public bool Simulate = false;

        public override bool IgnoreObjects() {
            return true;
        }

        public override void OnTrigger(List<Component> components, float currentTime) {
            if (ParticleSystem == null)
                return;
            if (Simulate && (ParticleSystem.duration + ParticleSystem.startLifetime) > currentTime - StartTime) {
                if (!ParticleSystem.isPlaying)
                    ParticleSystem.Simulate(currentTime - StartTime);
                ParticleSystem.Play();
            } else {
                if (!Simulate)
                    ParticleSystem.Play();
            }
            
            
        }
    }
}   