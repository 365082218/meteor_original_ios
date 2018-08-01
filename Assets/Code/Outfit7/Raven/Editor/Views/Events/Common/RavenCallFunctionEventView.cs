using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Starlite.Raven {

    public sealed class RavenCallFunctionEventView : RavenTriggerEventView {
        private RavenCallFunctionEvent m_CallFunctionEvent = null;
        private RavenPropertyBaseNonGenericView m_PropertyView = null;

        private static GUIStyle s_Node = (GUIStyle)"flow node 6";
        private static GUIStyle s_NodeSelected = (GUIStyle)"flow node 6 on";

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

        public override string Name {
            get {
                if (m_CallFunctionEvent.Property != null) {
                    return m_CallFunctionEvent.Property.ToPrettyString();
                }

                return "Call Function";
            }
        }

        public override void Initialize(RavenEvent evnt, RavenTrackView parent) {
            base.Initialize(evnt, parent);
            m_CallFunctionEvent = evnt as RavenCallFunctionEvent;
            m_PropertyView = CreateTriggerPropertyView(m_CallFunctionEvent.Property);
        }

        protected override void OnDrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, bool foldoutEnabled, float foldoutHeight, List<RavenParameter> parameters) {
            base.OnDrawGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutEnabled, foldoutHeight, parameters);
            if (m_PropertyView != null) {
                m_PropertyView.DrawGui(m_TopPartRect);
            }
        }

        protected override void OnDrawExtendedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, bool foldoutEnabled, float foldoutHeight, List<RavenParameter> parameters) {
            base.OnDrawExtendedGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutEnabled, foldoutHeight, parameters);

            var rects = new Rect[] { GetHorizontalRect(0.5f, 0.03f, 120f, 16f, 80f), GetHorizontalRect(0.5f, 0.18f, 120f, 16f, 80f), GetHorizontalRect(0.5f, 0.33f, 120f, 16f, 80f) };
            JustifyRectsLeft(rects);
            GameObject target;
            if (m_ParentTrackView.Target == null) {
                target = EditorGUI.ObjectField(rects[0], m_CallFunctionEvent.Target, typeof(GameObject), true) as GameObject;
                if (target != m_CallFunctionEvent.Target) {
                    m_CallFunctionEvent.SetTargetEditor(RavenSequenceEditor.Instance.Sequence, target);
                    RavenFunctionCallEditor.Refresh();
                }
            }
            target = m_CallFunctionEvent.Target;
            RavenParameterEditor.DrawOverrideTargetsParameterFieldForTriggerProperty(m_CallFunctionEvent.Property, rects[1], RavenSequenceEditor.Instance.Sequence.Parameters);
            if (target != null) {
                string text = "None";
                if (m_CallFunctionEvent.Property != null) {
                    text = RavenUtility.GetFunctionNameFromPackedFunctionName(m_CallFunctionEvent.Property.FunctionName);
                }
                if (GUI.Button(rects[2], "#" + text)) {
                    RavenFunctionCallEditor.OpenEditor(RavenSequenceEditor.Instance.Sequence, target, m_CallFunctionEvent, m_CallFunctionEvent.Property);
                }
            }

            if (m_PropertyView != null) {
                var contentRect = GetContentRect();
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