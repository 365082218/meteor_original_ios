using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

namespace Outfit7.Sequencer {
    [SequencerQuickSearchAttribute("UI")]
    [SequencerQuickSearchDisplayAttribute("Fill Amount")]
    [SequencerPropertyAttribute("UI/Fill Amount")]
    public class ImageFillAmountPropertyView : BasePropertyView {

        public override void OnInit(object property, object data) {
            ComponentType = typeof(Image);
            base.OnInit(property, data);
        }

        public override Type GetComponentType() {
            return typeof(Image);
        }

        public override string Name() {
            return "Image";
        }
    }
}