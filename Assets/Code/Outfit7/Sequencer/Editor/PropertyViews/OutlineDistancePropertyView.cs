using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using Outfit7.Logic.StateMachineInternal;

namespace Outfit7.Sequencer {
    [SequencerQuickSearchAttribute("UI")]
    [SequencerQuickSearchDisplayAttribute("UI Outline Distance")]
    [SequencerPropertyAttribute("UI/Outline/Distance")]
    public class OutlineDistancePropertyView : BasePropertyView {

        public override void OnInit(object property, object data) {
            ComponentType = typeof(Outline);
            base.OnInit(property, data);
        }

        public override float OnDrawGui(float indent, float offset, List<Parameter> parameters) {
            DrawVector2ApplyField(new Rect(indent + 60, offset, 60f, 15f));
            return base.OnDrawGui(indent, offset, parameters);
        }

        public override Type GetComponentType() {
            return typeof(Outline);
        }

        public override string Name() {
            return "Outline Dst";
        }
    }
}