using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Outfit7.Logic.StateMachineInternal;
using System;
using UnityEngine.UI;

namespace Outfit7.Sequencer {
    [SequencerQuickSearchDisplayAttribute("UI Shader Value")]
    [SequencerPropertyAttribute("Shader/UI Value")]
    public class UiShaderValuePropertyView : BasePropertyView {
        UiShaderValueProperty Property;

        public override void OnInit(object property, object data) {
            Property = property as UiShaderValueProperty;
            ComponentType = typeof(Graphic);
            base.OnInit(property, data);
        }

        public override float OnDrawGui(float indent, float offset, List<Parameter> parameters) {
            Property.Enabled = GUI.Toggle(new Rect(indent - 20f, offset, 30f, 15f), Property.Enabled, Name());
            Property.ValueType = (ShaderValueProperty.ShaderValueType) EditorGUI.EnumPopup(new Rect(indent + 10, offset, 50f, 15f), Property.ValueType);
            Property.ValueName = EditorGUI.TextField(new Rect(indent + 60, offset, 60f, 15f), Property.ValueName);
            return 15f;
        }

        public override Type GetComponentType() {
            return typeof(Graphic);
        }

        public override string Name() {
            return "S ";
        }
    }
}