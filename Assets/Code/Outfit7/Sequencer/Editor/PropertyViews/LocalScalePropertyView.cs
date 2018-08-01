using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Outfit7.Logic.StateMachineInternal;

namespace Outfit7.Sequencer {
    [SequencerQuickSearchAttribute("lsca")]
    [SequencerQuickSearchDisplayAttribute("Local Scale")]
    [SequencerPropertyAttribute("Transform/Local Scale")]
    [SequencerPropertyAttribute("UI/Local Scale")]
    public class LocalScalePropertyView : BasePropertyView {
        LocalScaleProperty Property;

        public override void OnInit(object property, object data) {
            Property = property as LocalScaleProperty;
            ComponentType = typeof(Transform);
            base.OnInit(property, data);
        }

        public override float OnDrawGui(float indent, float offset, List<Parameter> parameters) {
            Property.Enabled = GUI.Toggle(new Rect(indent - 20f, offset, 80f, 15f), Property.Enabled, Name());
            Property.IsUniform = GUI.Toggle(new Rect(indent + 60, offset, 70f, 15f), Property.IsUniform, "Uniform");
            return 15f;
        }

        public override Type GetComponentType() {
            return typeof(Transform);
        }

        public override string Name() {
            return "Local Scale";
        }
    }
}