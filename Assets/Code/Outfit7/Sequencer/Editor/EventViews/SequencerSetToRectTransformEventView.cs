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
    [SequencerQuickSearchDisplayAttribute("Set To RectTransform")]
    [SequencerCurveTrackAttribute("Set/To RectTransform")]
    public class SequencerSetToRectTransformEventView : SequencerTriggerEventView {
        private SequencerSetToRectTransformEvent Event = null;
        private List<Vector3> SavePosition = new List<Vector3>();
        private List<Quaternion> SaveRotation = new List<Quaternion>();
        private List<Vector3> SaveScale = new List<Vector3>();

        public override void OnInit(object evnt, object parent) {
            Event = evnt as SequencerSetToRectTransformEvent;
            base.OnInit(evnt, parent);
        }

        public override string GetName() {
            return "SetRT";
        }

        protected override void OnDestroy() {
        }

        public override void OnDrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
        }

        public override void OnDrawExtendedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawExtendedGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
            GUI.Label(GetRectPosition(20f, 40f, 20f), GetName());

            ParameterFieldView.DrawParameterField(GetRectPosition(40f, 60f, 30f), Event.ComponentField, typeof(RectTransform), parameters);

            EditorGUI.LabelField(GetRectPosition(90f, 45f, 15f), "T  R  S");
            Rect t = GetRectPosition(105f, 45f, 15f);
            Rect r = GetRectPosition(105f, 45f, 15f);
            Rect s = GetRectPosition(105f, 45f, 15f);
            t.width = 15f;
            r.width = 15f;
            s.width = 15f;
            t.width = 15f;
            r.x += 15f;
            s.x += 30f;
            Event.AffectPosition = EditorGUI.Toggle(t, "", Event.AffectPosition);
            Event.AffectRotation = EditorGUI.Toggle(r, "", Event.AffectRotation);
            Event.AffectScale = EditorGUI.Toggle(s, "", Event.AffectScale);
        }

        public override void OnRecordingStart() {
            SavePosition.Clear();
            SaveRotation.Clear();
            SaveScale.Clear();
            for (int i = 0; i < Event.Objects.Count; i++) {
                RectTransform t = Event.Objects[i].Components[0] as RectTransform;
                SavePosition.Add(t.localPosition);
                SaveRotation.Add(t.localRotation);
                SaveScale.Add(t.localScale);
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
            }
        }
    }
}

