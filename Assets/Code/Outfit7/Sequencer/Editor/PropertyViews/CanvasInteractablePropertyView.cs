using UnityEngine;
using System;

namespace Outfit7.Sequencer {
    [SequencerQuickSearchAttribute("UI")]
    [SequencerQuickSearchDisplayAttribute("Canvas Interactable")]
    [SequencerPropertyAttribute("UI/Canvas/Interactable")]
    public class CanvasInteractablePropertyView : BasePropertyView {

        public override void OnInit(object property, object data) {
            ComponentType = typeof(CanvasGroup);
            base.OnInit(property, data);
        }

        public override Type GetComponentType() {
            return typeof(CanvasGroup);
        }

        public override string Name() {
            return "CanvasInteractable";
        }
    }
}