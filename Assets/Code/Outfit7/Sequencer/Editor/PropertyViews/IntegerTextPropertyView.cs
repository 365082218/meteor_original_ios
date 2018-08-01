using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

namespace Outfit7.Sequencer {
    [SequencerQuickSearchAttribute("UI")]
    [SequencerQuickSearchDisplayAttribute("Integer Text")]
    [SequencerPropertyAttribute("UI/Text (INT)")]
    public class IntegerTextPropertyView : BasePropertyView {

        public override void OnInit(object property, object data) {
            ComponentType = typeof(UnityEngine.UI.Text);
            base.OnInit(property, data);
        }

        public override Type GetComponentType() {
            return typeof(UnityEngine.UI.Text);
        }

        public override string Name() {
            return "Text (INT)";
        }
    }
}