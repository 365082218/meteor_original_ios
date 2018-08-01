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
    [SequencerQuickSearchDisplayAttribute("Ui Follow 3D")]
    [SequencerCurveTrackAttribute("UI/Ui Follow 3D")]
    public class UiFollow3DEventView : SequencerContinuousEventView {
        private UiFollow3DEvent Event = null;
        private List<Vector3> SavePosition = new List<Vector3>();

        public override void OnInit(object evnt, object parent) {
            Event = evnt as UiFollow3DEvent;
            base.OnInit(evnt, parent);
        }

        public override string GetName() {
            return "Ui Follow 3D";
        }

        protected override void OnDestroy() {
        }

        public override void OnDrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
        }

        public override void OnDrawExtendedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawExtendedGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);

            ParameterFieldView.DrawParameterField(new Rect(EventRect.x, EventRect.y + 20f, 60f, 30f), Event.FromCamera, typeof(Camera), parameters);
            ParameterFieldView.DrawParameterField(new Rect(EventRect.x + EventRect.width - 60f, EventRect.y + 20f, 60f, 30f), Event.ToCamera, typeof(Camera), parameters);
            Event.ToCameraDepth = EditorGUI.FloatField(new Rect(EventRect.center.x - 30f, EventRect.y + 50f, 60f, 20f), Event.ToCameraDepth);

            ParameterFieldView.DrawParameterField(new Rect(EventRect.x, EventRect.y + 55f, 60f, 70f), Event.PointField, parameters, 3);

        }

        protected override bool OnHandleInput(TimelineData timelineData, SequencerSequenceView sequenceView, Rect timelineTrackRect, int highiestEventTrackIndex, object actor) {
            return base.OnHandleInput(timelineData, sequenceView, timelineTrackRect, highiestEventTrackIndex, actor);
        }

        protected override void OnDrawEventsContextMenu(GenericMenu menu, SequencerTrackView parent, object actor) {
        }

        public override void OnUpdateWhileRecording(float currentTime) {
            /*float normalizedTime = Event.GetNormalizedTime(currentTime);
            float absoluteTime = currentTime - Event.StartTime;
            float duration = Event.Duration;
            if (Event.Objects.Count == 0)
                return;
            if (Event.Objects[0].Components.Count == 0)
                return;
            Transform t = Event.Objects[0].Components[0] as Transform;
            if (t == null)
                return;
            
            if (normalizedTime < 0.5 && Vector4.Magnitude(t.position - (Vector3) Event.FromPositionField.Value) > 0.0001f) {
                Event.FromPositionField.Value = t.position;
            } else if (normalizedTime > 0.5 && Vector4.Magnitude(t.position - (Vector3) Event.ToPositionField.Value) > 0.0001f) {
                Event.ToPositionField.Value = t.position;
            }*/
        }

        protected override void OnDraggingEnd() {
            SequencerSequence sequence = Event.gameObject.GetComponent<SequencerSequence>();
            if (sequence != null) {
                Event.Evaluate(0, sequence.GetCurrentTime());
            }
        }

        public override void OnRecordingStart() {
            SavePosition.Clear();
            for (int i = 0; i < Event.Objects.Count; i++) {
                RectTransform t = Event.Objects[i].Components[0] as RectTransform;
                SavePosition.Add(t.localPosition);
            }
        }

        public override void OnRecordingStop() {
            for (int i = 0; i < Event.Objects.Count; i++) {
                RectTransform t = Event.Objects[i].Components[0] as RectTransform;
                if (i >= SavePosition.Count)
                    return;
                t.localPosition = SavePosition[i];
            }
        }
    }
}

