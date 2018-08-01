using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Starlite.Raven {

    public class RavenPropertyEventView : RavenContinuousEventView {
        private RavenPropertyEvent m_PropertyEvent = null;
        private RavenPropertyBaseNonGenericView m_PropertyView;
        private RavenPropertyTrackView m_ParentPropertyTrackView;

        private static RavenPropertyEvent s_PropertyEventCopy = null;
        private static RavenAnimationDataComponentBase s_AnimationDataCopy = null;

        private static GUIStyle s_Node = (GUIStyle)"flow node 1";
        private static GUIStyle s_NodeSelected = (GUIStyle)"flow node 1 on";

        public override string Name {
            get {
                if (m_PropertyEvent.Property != null) {
                    return m_PropertyEvent.Property.ToPrettyString();
                }

                return "Property Setter";
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
            m_PropertyEvent = evnt as RavenPropertyEvent;
            m_PropertyView = CreateAnimationPropertyView(m_PropertyEvent.Property);
            m_ParentPropertyTrackView = parent as RavenPropertyTrackView;
        }

        public void GeneratePropertyMenuCallback(object data) {
            var entry = data as RavenPropertyTrackView.PropertyEventMenuData;
            bool restoreToPrevious;
            GeneratePropertyEventProperty(m_PropertyEvent.Target, entry.m_ComponentType, entry.m_MemberName, entry.m_PropertyType, entry.m_AnimationDataType, entry.m_ArgumentType, out restoreToPrevious);
        }

        protected override void OnDrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, bool foldoutEnabled, float foldoutHeight, List<RavenParameter> parameters) {
            base.OnDrawGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutEnabled, foldoutHeight, parameters);
            if (m_PropertyView != null) {
                m_PropertyView.DrawGui(m_TopPartRect);
            }
        }

        protected override void OnDrawExtendedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, bool foldoutEnabled, float foldoutHeight, List<RavenParameter> parameters) {
            base.OnDrawExtendedGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutEnabled, foldoutHeight, parameters);
            if (m_PropertyView != null) {
                var contentRect = GetContentRect();

                m_PropertyView.DrawExtendedGui(contentRect);
            }
        }

        protected override void OnDrawEventsContextMenu(GenericMenu menu, RavenTrackView parent) {
            if (m_ParentTrackView.Track.TrackType == ERavenTrackType.PropertyTrack) {
                var menuData = (m_ParentTrackView as RavenPropertyTrackView).PropertyMenuData;
                var startPath = m_PropertyEvent.Property != null ? "Replace Property/" : "";
                for (int i = 0; i < menuData.Count; ++i) {
                    var menuEntry = menuData[i];
                    menu.AddItem(new GUIContent(startPath + menuEntry.m_Entry), false, GeneratePropertyMenuCallback, menuEntry);
                }
                if (m_PropertyEvent.Property != null && m_PropertyEvent.Property.AnimationData != null) {
                    menu.AddItem(new GUIContent("Copy Animation Data"), false, CopyAnimationData);
                }
                if (s_AnimationDataCopy != null && IsAnimationDataPasteValid(s_AnimationDataCopy, m_PropertyEvent)) {
                    menu.AddItem(new GUIContent("Paste Animation Data Reference"), false, PasteAnimationDataReference, s_AnimationDataCopy);
                    menu.AddItem(new GUIContent("Paste Animation Data Copy"), false, PasteAnimationDataCopy, s_AnimationDataCopy);
                }
            }
        }

        protected override bool OnHandleInput(Vector2 mousePosition, TimelineData timelineData, RavenSequenceView sequenceView, Rect timelineTrackRect, int highiestEventTrackIndex) {
            if (base.OnHandleInput(mousePosition, timelineData, sequenceView, timelineTrackRect, highiestEventTrackIndex)) {
                return true;
            }

            if (m_HandleTouchRect.Contains(mousePosition) &&
                UnityEngine.Event.current.type == EventType.KeyDown) {
                if (UnityEngine.Event.current.keyCode == KeyCode.T) {
                    return sequenceView.GenerateQuickSearchForAnimationProperty(ERavenAnimationDataFilter.Tween, m_ParentPropertyTrackView.PropertyMenuData, GeneratePropertyMenuCallback);
                } else if (UnityEngine.Event.current.keyCode == KeyCode.C) {
                    return sequenceView.GenerateQuickSearchForAnimationProperty(ERavenAnimationDataFilter.Curve, m_ParentPropertyTrackView.PropertyMenuData, GeneratePropertyMenuCallback);
                } else if (UnityEngine.Event.current.keyCode == KeyCode.S) {
                    return sequenceView.GenerateQuickSearchForAnimationProperty(ERavenAnimationDataFilter.Set, m_ParentPropertyTrackView.PropertyMenuData, GeneratePropertyMenuCallback);
                }
            }

            if (m_PropertyView != null) {
                return m_PropertyView.HandleInput(mousePosition);
            }
            return false;
        }

        protected override void OnGeneratePropertyEventProperty(RavenAnimationPropertyComponentBase property) {
            m_PropertyView = CreateAnimationPropertyView(property);
        }

        protected override void OnUpdateWhileRecording(double currentTime) {
        }

        protected override void OnRecordingStart() {
            if (m_PropertyView != null) {
                m_PropertyView.RecordStart();
            }
        }

        protected override void OnRecordingStop() {
            if (m_PropertyView != null) {
                m_PropertyView.RecordEnd();
            }
        }

        private void CopyAnimationData() {
            s_AnimationDataCopy = m_PropertyEvent.Property.AnimationData;
            s_PropertyEventCopy = m_PropertyEvent;
        }

        private void PasteAnimationDataReference(object animationDataObj) {
            var animationData = animationDataObj as RavenAnimationDataComponentBase;

            var animationDataOld = m_PropertyEvent.Property.AnimationData;

            Undo.RecordObject(m_PropertyEvent.Property, "PasteAnimationDataReference");
            m_PropertyEvent.Property.AnimationData = animationData;
            PostprocessAnimationDataChange(m_PropertyEvent, animationData.GetType());

            m_PropertyView = CreateAnimationPropertyView(m_PropertyEvent.Property);

            if (animationDataOld != null) {
                animationDataOld.DestroyEditor(RavenSequenceEditor.Instance.Sequence);
            }
        }

        private void PasteAnimationDataCopy(object animationDataObj) {
            var animationData = animationDataObj as RavenAnimationDataComponentBase;

            var animationDataOld = m_PropertyEvent.Property.AnimationData;

            Undo.RecordObject(m_PropertyEvent.Property, "PasteAnimationDataCopy");
            m_PropertyEvent.Property.AnimationData = GenerateAnimationData(m_PropertyEvent.Target,
                animationData.GetType(),
                m_PropertyEvent.Property,
                false,
                RavenSequenceEditor.Instance.Sequence);
            m_PropertyEvent.Property.AnimationData.CopyValuesFrom(animationData);
            PostprocessAnimationDataChange(m_PropertyEvent, animationData.GetType());

            m_PropertyView = CreateAnimationPropertyView(m_PropertyEvent.Property);

            if (animationDataOld != null) {
                animationDataOld.DestroyEditor(RavenSequenceEditor.Instance.Sequence);
            }
        }

        private bool IsAnimationDataPasteValid(RavenAnimationDataComponentBase animationData, RavenPropertyEvent propertyEvent) {
            if (animationData == null) {
                return false;
            }

            if (propertyEvent == s_PropertyEventCopy) {
                return false;
            }

            var property = propertyEvent.Property;
            if (property == null) {
                return false;
            }

            if (property.GetPropertyType() != s_PropertyEventCopy.Property.GetPropertyType()) {
                return false;
            }

            return true;
        }
    }
}