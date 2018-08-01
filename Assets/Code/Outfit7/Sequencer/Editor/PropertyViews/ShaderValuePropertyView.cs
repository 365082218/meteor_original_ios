using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Outfit7.Logic.StateMachineInternal;
using System;

namespace Outfit7.Sequencer {
    [SequencerQuickSearchDisplayAttribute("Shader Value")]
    [SequencerPropertyAttribute("Shader/Custom Value")]
    public class ShaderValuePropertyView : BasePropertyView {
        ShaderValueProperty Property;

        public override void OnInit(object property, object data) {
            Property = property as ShaderValueProperty;
            ComponentType = typeof(Renderer);
            base.OnInit(property, data);
        }

        public override float OnDrawGui(float indent, float offset, List<Parameter> parameters) {
            Property.Enabled = GUI.Toggle(new Rect(indent - 20f, offset, 30f, 15f), Property.Enabled, Name());
            Property.ValueType = (ShaderValueProperty.ShaderValueType) EditorGUI.EnumPopup(new Rect(indent + 10, offset, 50f, 15f), Property.ValueType);
            Property.ValueName = EditorGUI.TextField(new Rect(indent + 60, offset, 60f, 15f), Property.ValueName);
            return 15f;
        }

        public override Type GetComponentType() {
            return typeof(Renderer);
        }

        public override string Name() {
            return "S ";
        }
    }
}