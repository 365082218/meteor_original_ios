using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Outfit7.Util;
using Outfit7.Logic.StateMachineInternal;
using Outfit7.Logic.Util;
using Outfit7.Logic;

namespace Outfit7.Sequencer {
    [SequencerQuickSearchAttribute("Custom Code")]
    [SequencerQuickSearchDisplayAttribute("Invoke")]
    [SequencerNormalTrackAttribute("Custom Code/Invoke")]
    public class SequencerInvokeEventView : SequencerTriggerEventView {
        private SequencerInvokeEvent Event = null;
        private GameObject ActorGameObject;
        //private Color LineColor = new Color(0.2f, 0.6f, .2f);

        public override void OnInit(object evnt, object parent) {
            Event = evnt as SequencerInvokeEvent;
            base.OnInit(evnt, parent);
        }

        public override string GetName() {
            return "Invoke";
        }

        public override void OnDrawExtendedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawExtendedGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
            //GUI.Label(new Rect(EventRect.center.x - 20f, EventRect.yMin + 20f, 40f, 20f), GetName());
            ParameterFieldView.DrawParameterField(GetRectPosition(15f, 80f, 35f), Event.ComponentField, typeof(Transform), parameters);
            Transform t = Event.ComponentField.Value as Transform;
            if (t != null) {
                string text = "None";
                if (Event.InvokeEvent.Method != null)
                    text = Event.InvokeEvent.Method.ToString();
                if (GUI.Button(GetRectPosition(50f, 80f, 15f), "#" + text)) {
                    LogicEditorCommon.OpenEventEditor(t.gameObject, new MonoBehaviour[]{ }, Event.InvokeEvent);
                }
            }
        }

        public override void OnDrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
        }

        public override void OnRefresh(object actor) {
            base.OnRefresh(actor);
        }
    }
}
