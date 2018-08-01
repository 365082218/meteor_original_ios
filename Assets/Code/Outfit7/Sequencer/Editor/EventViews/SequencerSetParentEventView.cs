using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;
using Outfit7.Util;
using Outfit7.Logic.StateMachineInternal;


namespace Outfit7.Sequencer {
    [SequencerQuickSearchDisplayAttribute("Set Parent")]
    [SequencerCurveTrackAttribute("Set/Parent")]
    public class SequencerSetParentEventView : SequencerTriggerEventView {
        private SequencerSetParentEvent Event = null;
        private List<Transform> Parents = new List<Transform>();

        public override void OnInit(object evnt, object parent) {
            Event = evnt as SequencerSetParentEvent;
            base.OnInit(evnt, parent);
        }

        public override string GetName() {
            return "Parent";
        }

        protected override void OnDestroy() {
        }

        public override void OnDrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
        }

        public override void OnDrawExtendedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawExtendedGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
            GUI.Label(GetRectPosition(20f, 40f, 20f), GetName());

            ParameterFieldView.DrawParameterField(GetRectPosition(40f, 60f, 30f), Event.ComponentField, typeof(Transform), parameters);
        }

        public override void OnRecordingStart() {
            Parents.Clear();
            for (int i = 0; i < Event.Objects.Count; i++) {
                Transform t = Event.Objects[i].Components[0] as Transform;
                Parents.Add(t.parent);
            }
        }

        public override void OnRecordingStop() {
            for (int i = 0; i < Event.Objects.Count; i++) {
                Transform t = Event.Objects[i].Components[0] as Transform;
                if (i >= Parents.Count)
                    return;
                t.SetParent(Parents[i]);
            }
        }
    }
}

