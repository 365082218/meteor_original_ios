using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Outfit7.Sequencer {
    [SequencerQuickSearchAttribute("Enabled")]
    [SequencerQuickSearchDisplayAttribute("Active State")]
    [SequencerPropertyAttribute("GameObject/Active State")]
    public class ActiveStatePropertyView : BasePropertyView {

        public override void OnInit(object property, object data) {
            ComponentType = typeof(Transform);
            base.OnInit(property, data);
        }

        public override Type GetComponentType() {
            return typeof(Transform);
        }

        public override string Name() {
            return "ActiveState";
        }
    }
}