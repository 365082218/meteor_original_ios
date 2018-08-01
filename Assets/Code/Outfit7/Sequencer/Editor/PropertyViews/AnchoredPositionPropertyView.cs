using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Outfit7.Logic.StateMachineInternal;

namespace Outfit7.Sequencer {
    [SequencerQuickSearchAttribute("ui")]
    [SequencerQuickSearchDisplayAttribute("Anchored Position")]
    [SequencerPropertyAttribute("UI/Anchored Position")]
    public class AnchoredPositionPropertyView : BasePropertyView {

        public override void OnInit(object property, object data) {
            ComponentType = typeof(RectTransform);
            base.OnInit(property, data);
        }

        public override Type GetComponentType() {
            return typeof(RectTransform);
        }

        public override float OnDrawGui(float indent, float offset, List<Parameter> parameters) {
            DrawVector2ApplyField(new Rect(indent + 60, offset, 60f, 15f));
            return base.OnDrawGui(indent, offset, parameters);
        }

        public override string Name() {
            return "AnchorPos";
        }
    }
}