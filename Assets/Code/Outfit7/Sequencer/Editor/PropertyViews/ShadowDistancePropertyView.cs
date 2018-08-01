using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using Outfit7.Logic.StateMachineInternal;

namespace Outfit7.Sequencer {
    [SequencerQuickSearchAttribute("UI")]
    [SequencerQuickSearchDisplayAttribute("UI Shadow Distance")]
    [SequencerPropertyAttribute("UI/Shadow/Distance")]
    public class ShadowDistancePropertyView : BasePropertyView {

        public override void OnInit(object property, object data) {
            ComponentType = typeof(Shadow);
            base.OnInit(property, data);
        }

        protected override bool ForceExactComponentName() {
            return true;
        }

        public override float OnDrawGui(float indent, float offset, List<Parameter> parameters) {
            DrawVector2ApplyField(new Rect(indent + 60, offset, 60f, 15f));
            return base.OnDrawGui(indent, offset, parameters);
        }

        public override Type GetComponentType() {
            return typeof(Shadow);
        }

        public override string Name() {
            return "Shadow Dst";
        }
    }
}