using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

namespace Outfit7.Sequencer {
    [SequencerQuickSearchAttribute("Enabled")]
    [SequencerQuickSearchDisplayAttribute("Interactable")]
    [SequencerPropertyAttribute("UI/Button/Interactable")]
    public class ButtonInteractablePropertyView : BasePropertyView {

        public override void OnInit(object property, object data) {
            ComponentType = typeof(Button);
            base.OnInit(property, data);
        }

        public override Type GetComponentType() {
            return typeof(Button);
        }

        public override string Name() {
            return "Interactable";
        }
    }
}