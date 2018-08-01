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
    [SequencerQuickSearchDisplayAttribute("Rect Target Tween")]
    [SequencerCurveTrackAttribute("Animation/Rect Target Tween")]
    public class SequencerRectTargetTweenEventView : SequencerContinuousEventView {
        private SequencerRectTargetTweenEvent Event = null;
        private List<Vector3> SavePosition = new List<Vector3>();
        private List<Quaternion> SaveRotation = new List<Quaternion>();
        private List<Vector3> SaveScale = new List<Vector3>();
        private List<Vector2> SaveSize = new List<Vector2>();
        private bool ShowTangent = false;

        public override void OnInit(object evnt, object parent) {
            Event = evnt as SequencerRectTargetTweenEvent;
            base.OnInit(evnt, parent);
        }

        public override string GetName() {
            return "Rect Target Tween";
        }

        protected override void OnDestroy() {
        }

        public override void OnDrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
        }

        public override void OnDrawExtendedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawExtendedGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
            Event.EaseType = (EaseManager.Ease) EditorGUI.EnumPopup(new Rect(EventRect.center.x - 30f, EventRect.y + 40f, 60f, 20f), Event.EaseType);

            Event.FromCamera = (Camera) EditorGUI.ObjectField(new Rect(EventRect.x, EventRect.y + 20f, 60f, 15f), "", Event.FromCamera, typeof(Camera), true);
            Event.ToCamera = (Camera) EditorGUI.ObjectField(new Rect(EventRect.x + EventRect.width - 60f, EventRect.y + 20f, 60f, 15f), "", Event.ToCamera, typeof(Camera), true);
            Event.ToCameraDepth = EditorGUI.FloatField(new Rect(EventRect.center.x - 30f, EventRect.y + 20f, 60f, 20f), Event.ToCameraDepth);

            ParameterFieldView.DrawParameterField(new Rect(EventRect.x, EventRect.y + 35f, 60f, 30f), Event.FromComponentField, typeof(RectTransform), parameters);
            ParameterFieldView.DrawParameterField(new Rect(EventRect.x + EventRect.width - 60f, EventRect.y + 35f, 60f, 30f), Event.ToComponentField, typeof(RectTransform), parameters);
            if (Event.FromComponentField.FieldType == ParameterFieldType.SOLID) {
                Event.FromPositionField.FieldType = ParameterFieldType.SOLID;
                Event.FromRotationField.FieldType = ParameterFieldType.SOLID;
                Event.FromScaleField.FieldType = ParameterFieldType.SOLID;
                Event.FromSizeField.FieldType = ParameterFieldType.SOLID;
            }
            if (Event.FromComponentField.FieldType == ParameterFieldType.CURRENT) {
                Event.FromPositionField.FieldType = ParameterFieldType.CURRENT;
                Event.FromRotationField.FieldType = ParameterFieldType.CURRENT;
                Event.FromScaleField.FieldType = ParameterFieldType.CURRENT;
                Event.ToSizeField.FieldType = ParameterFieldType.CURRENT;
            }


            if (ShowTangent) {
                if (GUI.Button(new Rect(EventRect.center.x - 5f, EventRect.yMax - 45f, 40f, 15f), "DATA"))
                    ShowTangent = false;
                ParameterFieldView.DrawParameterField(new Rect(EventRect.x, EventRect.y + 60f, 60f, 75f), Event.TangentStart, parameters, 3);
                ParameterFieldView.DrawParameterField(new Rect(EventRect.x + EventRect.width - 60f, EventRect.y + 60f, 60f, 75f), Event.TangentEnd, parameters, 3);
            } else {
                if (Event.UseBezierCurve.Value) {
                    if (GUI.Button(new Rect(EventRect.center.x - 5f, EventRect.yMax - 45f, 40f, 15f), "TAN"))
                        ShowTangent = true;
                }
                if (Event.FromComponentField.Value == null) {
                    ParameterFieldView.DrawParameterField(new Rect(EventRect.x, EventRect.y + 60f, 40f, 75f), Event.FromPositionField, parameters, 3);
                    ParameterFieldView.DrawParameterField(new Rect(EventRect.x + 40f, EventRect.y + 60f, 40f, 75f), Event.FromRotationField, parameters, 3);
                    ParameterFieldView.DrawParameterField(new Rect(EventRect.x + 80f, EventRect.y + 60f, 40f, 75f), Event.FromScaleField, parameters, 3);
                    ParameterFieldView.DrawParameterField(new Rect(EventRect.x + 120, EventRect.y + 60f, 40f, 75f), Event.FromSizeField, parameters, 3);
                }
                if (Event.ToComponentField.Value == null) {
                    ParameterFieldView.DrawParameterField(new Rect(EventRect.x + EventRect.width - 160f, EventRect.y + 60f, 40f, 75f), Event.ToPositionField, parameters, 3);
                    ParameterFieldView.DrawParameterField(new Rect(EventRect.x + EventRect.width - 120f, EventRect.y + 60f, 40f, 75f), Event.ToRotationField, parameters, 3);
                    ParameterFieldView.DrawParameterField(new Rect(EventRect.x + EventRect.width - 80f, EventRect.y + 60f, 40f, 75f), Event.ToScaleField, parameters, 3);
                    ParameterFieldView.DrawParameterField(new Rect(EventRect.x + EventRect.width - 40f, EventRect.y + 60f, 40f, 75f), Event.ToSizeField, parameters, 3);
                }
            }
            ParameterFieldView.DrawParameterField(new Rect(EventRect.center.x - 20f, EventRect.yMin + 60f, 40f, 30f), Event.UseBezierCurve, parameters);
            EditorGUI.LabelField(new Rect(EventRect.center.x - 35f, EventRect.yMax - 30f, 75f, 15f), "Tr Rt Sc Sz");
            Event.AffectPosition = EditorGUI.Toggle(new Rect(EventRect.center.x - 30f, EventRect.yMax - 15f, 15f, 15f), "", Event.AffectPosition);
            Event.AffectRotation = EditorGUI.Toggle(new Rect(EventRect.center.x - 15f, EventRect.yMax - 15f, 15f, 15f), "", Event.AffectRotation);
            Event.AffectScale = EditorGUI.Toggle(new Rect(EventRect.center.x + 0f, EventRect.yMax - 15f, 15f, 15f), "", Event.AffectScale);
            Event.AffectSize = EditorGUI.Toggle(new Rect(EventRect.center.x + 15f, EventRect.yMax - 15f, 15f, 15f), "", Event.AffectSize);
        }

        protected override bool OnHandleInput(TimelineData timelineData, SequencerSequenceView sequenceView, Rect timelineTrackRect, int highiestEventTrackIndex, object actor) {
            if (EventRect.Contains(SequencerSequenceView.GetCurrentMousePosition())) {
                Event.EaseType = EaseManager.EaseHotkeys(Event.EaseType);
            }
            return base.OnHandleInput(timelineData, sequenceView, timelineTrackRect, highiestEventTrackIndex, actor);
        }

        protected override void OnDrawEventsContextMenu(GenericMenu menu, SequencerTrackView parent, object actor) {
            menu.AddItem(new GUIContent("Set START value"), false, SetFromValue);
            menu.AddItem(new GUIContent("Set END value"), false, SetToValue);
        }

        private void SetFromValue() {
            if (Selection.activeGameObject == null)
                return;
            Event.FromPositionField.Value = Selection.activeGameObject.transform.position;
            Event.FromRotationField.Value = Selection.activeGameObject.transform.eulerAngles;
            Event.FromScaleField.Value = Selection.activeGameObject.transform.localScale;
            Event.FromSizeField.Value = Selection.activeGameObject.GetComponent<RectTransform>().sizeDelta;
        }

        private void SetToValue() {

            if (Selection.activeGameObject == null)
                return;
            Event.ToPositionField.Value = Selection.activeGameObject.transform.position;
            Event.ToRotationField.Value = Selection.activeGameObject.transform.eulerAngles;
            Event.ToScaleField.Value = Selection.activeGameObject.transform.localScale;
            Event.ToSizeField.Value = Selection.activeGameObject.GetComponent<RectTransform>().sizeDelta;
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
            SaveRotation.Clear();
            SaveScale.Clear();
            SaveSize.Clear();
            for (int i = 0; i < Event.Objects.Count; i++) {
                RectTransform t = Event.Objects[i].Components[0] as RectTransform;
                SavePosition.Add(t.localPosition);
                SaveRotation.Add(t.localRotation);
                SaveScale.Add(t.localScale);
                SaveSize.Add(t.sizeDelta);
            }
        }

        public override void OnRecordingStop() {
            for (int i = 0; i < Event.Objects.Count; i++) {
                RectTransform t = Event.Objects[i].Components[0] as RectTransform;
                if (i >= SavePosition.Count)
                    return;
                t.localPosition = SavePosition[i];
                t.localRotation = SaveRotation[i];
                t.localScale = SaveScale[i];
                t.sizeDelta = SaveSize[i];
            }
        }
    }
}

