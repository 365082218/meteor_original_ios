using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Outfit7.Sequencer {
    [SequencerQuickSearchAttribute("UI")]
    [SequencerQuickSearchDisplayAttribute("Canvas Alpha")]
    [SequencerPropertyAttribute("UI/Canvas/Alpha")]
    public class CanvasAlphaPropertyView : BasePropertyView {

        public override void OnInit(object property, object data) {
            ComponentType = typeof(CanvasGroup);
            base.OnInit(property, data);
        }

        public override Type GetComponentType() {
            return typeof(CanvasGroup);
        }

        public override string Name() {
            return "CanvasAlpha";
        }
    }
}