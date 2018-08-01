using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Outfit7.Sequencer {
    [SequencerQuickSearchAttribute("particle")]
    [SequencerQuickSearchAttribute("emit")]
    [SequencerQuickSearchDisplayAttribute("Enable Emission")]
    [SequencerPropertyAttribute("Particle/Enable Emission")]
    public class ParticleEnableEmissionPropertyView : BasePropertyView {

        public override void OnInit(object property, object data) {
            ComponentType = typeof(ParticleSystem);
            base.OnInit(property, data);
        }

        public override Type GetComponentType() {
            return typeof(ParticleSystem);
        }

        public override string Name() {
            return "Emit";
        }
    }
}