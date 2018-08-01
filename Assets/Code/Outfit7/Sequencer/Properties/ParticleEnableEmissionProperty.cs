using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Outfit7.Sequencer {
    public class ParticleEnableEmissionProperty : BaseProperty {
        public override int GetNumberOfValuesUsed() {
            return 1;
        }

        public override void OnApply(Component component, Vector4 value) {
            ParticleSystem particleSystem = component as ParticleSystem;
            if (particleSystem == null)
                return;
            ParticleSystem.EmissionModule em = particleSystem.emission;
            em.enabled = value.x >= 1;
        }

        public override Vector4 OnValue(Component component, out bool success) {
            ParticleSystem particleSystem = component as ParticleSystem;
            if (particleSystem == null) {
                success = false;
                return Vector4.zero;
            }
            success = true;
            ParticleSystem.EmissionModule em = particleSystem.emission;
            return new Vector4(em.enabled ? 1 : 0, 0, 0, 0);
        }
    }
}