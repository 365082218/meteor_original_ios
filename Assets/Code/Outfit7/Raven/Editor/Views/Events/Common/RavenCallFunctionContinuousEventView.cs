using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Starlite.Raven {

    public sealed class RavenCallFunctionContinuousEventView : RavenContinuousEventView {
        private RavenCallFunctionContinuousEvent m_CallFunctionContinuousEvent = null;
        private RavenPropertyBaseNonGenericView m_PropertyView = null;

        private static GUIStyle s_Node = (GUIStyle)"flow node 6";
        private static GUIStyle s_NodeSelected = (GUIStyle)"flow node 6 on";

        public override string Name {
            get {
                if (m_CallFunctionContinuousEvent.Property != null) {
                    return m_CallFunctionContinuousEvent.Property.ToPrettyString();
                }

                return "Call Function";
            }
        }

        protected override GUIStyle NodeStyle {
            get {
                return s_Node;
            }
        }

        protected override GUIStyle NodeSelectedStyle {
            get {
                return s_NodeSelected;
            }
        }

        public override void Initialize(RavenEvent evnt, RavenTrackView parent) {
            base.Initialize(evnt, parent);
            m_CallFunctionContinuousEvent = evnt as RavenCallFunctionContinuousEvent;
            m_PropertyView = CreateTriggerPropertyView(m_CallFunctionContinuousEvent.Property);
        }

        protected override void OnDrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, bool foldoutEnabled, float foldoutHeight, List<RavenParameter> parameters) {
            base.OnDrawGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutEnabled, foldoutHeight, parameters);
            if (m_PropertyView != null) {
                m_PropertyView.DrawGui(m_TopPartRect);
            }
        }

        protected override void OnDrawExtendedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, bool foldoutEnabled, float foldoutHeight, List<RavenParameter> parameters) {
            base.OnDrawExtendedGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutEnabled, foldoutHeight, parameters);

            var contentRect = GetContentRect();
            var rects = new Rect[] { GetHorizontalRect(contentRect, 0.5f, 0.03f, 120f, 16f, 80f), GetHorizontalRect(contentRect, 0.5f, 0.18f, 120f, 16f, 80f), GetHorizontalRect(contentRect, 0.5f, 0.33f, 120f, 16f, 80f), GetHorizontalRect(contentRect, 0.5f, 0.48f, 16f, 16f, 16f) };
            JustifyRectsLeft(rects);
            GameObject target;
            if (m_ParentTrackView.Target == null) {
                target = EditorGUI.ObjectField(rects[0], m_CallFunctionContinuousEvent.Target, typeof(GameObject), true) as GameObject;
                if (target != m_CallFunctionContinuousEvent.Target) {
                    m_CallFunctionContinuousEvent.SetTargetEditor(RavenSequenceEditor.Instance.Sequence, target);
                    RavenFunctionCallEditor.Refresh();
                }
            }
            target = m_CallFunctionContinuousEvent.Target;
            RavenParameterEditor.DrawOverrideTargetsParameterFieldForTriggerProperty(m_CallFunctionContinuousEvent.Property, rects[1], RavenSequenceEditor.Instance.Sequence.Parameters);
            if (target != null) {
                string text = "None";
                if (m_CallFunctionContinuousEvent.Property != null) {
                    text = RavenUtility.GetFunctionNameFromPackedFunctionName(m_CallFunctionContinuousEvent.Property.FunctionName);
                }
                if (GUI.Button(rects[2], "#" + text)) {
                    RavenFunctionCallEditor.OpenEditor(RavenSequenceEditor.Instance.Sequence, target, m_CallFunctionContinuousEvent, m_CallFunctionContinuousEvent.Property);
                }
            }

            //m_CallFunctionContinuousEvent.Interpolate = EditorGUI.Toggle(rects[3], m_CallFunctionContinuousEvent.Interpolate);

            if (m_PropertyView != null) {
                m_PropertyView.DrawExtendedGui(contentRect);
            }
        }

        protected override bool OnHandleInput(Vector2 mousePosition, TimelineData timelineData, RavenSequenceView sequenceView, Rect timelineTrackRect, int highestEventSubTrackIndex) {
            if (base.OnHandleInput(mousePosition, timelineData, sequenceView, timelineTrackRect, highestEventSubTrackIndex)) {
                return true;
            }

            if (m_PropertyView != null) {
                return m_PropertyView.HandleInput(mousePosition);
            }
            return false;
        }

        protected override void OnDrawEventsContextMenu(GenericMenu menu, RavenTrackView parent) {
        }

        protected override void OnGenerateFunctionCallProperty(RavenTriggerPropertyComponentBase property) {
            m_PropertyView = CreateTriggerPropertyView(property);
        }

        protected override void OnUpdateWhileRecording(double currentTime) {
        }

        protected override void OnRecordingStart() {
        }

        protected override void OnRecordingStop() {
        }
    }
}