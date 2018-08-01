using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Outfit7.Logic;
using Outfit7.Logic.StateMachineInternal;
using UnityEditor;

namespace Outfit7.Sequencer {
    [SequencerQuickSearchAttribute("asm")]
    [SequencerQuickSearchDisplayAttribute("StateMachine Parameter")]
    [SequencerPropertyAttribute("StateMachine/Parameter")]
    public class StateMachineParameterPropertyView : BasePropertyView {
        StateMachineParameterProperty Property;

        public override void OnInit(object property, object data) {
            Property = property as StateMachineParameterProperty;
            ComponentType = typeof(StateMachine);
            base.OnInit(property, data);
        }

        public override Type GetComponentType() {
            return typeof(StateMachine);
        }

        public override float OnDrawGui(float indent, float offset, List<Parameter> parameters) {
            Property.Enabled = GUI.Toggle(new Rect(indent - 20f, offset, 40f, 15f), Property.Enabled, Name());
            if (Property.Components.Value.Count > 0) {
                int oldIndex = Property.ParameterIndex;
                StateMachine stateMachine = Property.Components.Value[0] as StateMachine;
                if (stateMachine == null)
                    return 15f;
                string[] stateMachineParamNames = StateMachineEditor.GetParameterNames(stateMachine, null, false);
                Property.ParameterIndex = EditorGUI.Popup(new Rect(indent + 25, offset, 100f, 15f), Property.ParameterIndex, stateMachineParamNames);

                if (oldIndex != Property.ParameterIndex) {
                    Property.ParameterName = stateMachineParamNames[Property.ParameterIndex];
                    Property.ParameterType = stateMachine.GetParameterByIndex(Property.ParameterIndex).ParameterType;
                }
            }
            return 15f;
        }

        public override string Name() {
            return "SM";
        }
    }
}