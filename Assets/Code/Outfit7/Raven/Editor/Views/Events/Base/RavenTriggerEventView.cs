using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Starlite.Raven {

    public abstract class RavenTriggerEventView : RavenEventView {

        protected enum ETriggerEventInputState {
            Inactive,
            Dragging
        }

        private const float c_HotzoneWidth = 6f;

        protected RavenTriggerEvent m_TriggerEvent;
        protected ETriggerEventInputState m_InputState = ETriggerEventInputState.Inactive;
        protected Rect m_TopPartRect = new Rect();

        protected int m_QueuedStartFrameReposition;

        public override int QueuedStartFrameReposition {
            get {
                return m_QueuedStartFrameReposition;
            }
        }

        public override int QueuedLastFrameReposition {
            get {
                return m_QueuedStartFrameReposition;
            }
        }

        public override void Initialize(RavenEvent evnt, RavenTrackView parent) {
            base.Initialize(evnt, parent);
            m_TriggerEvent = evnt as RavenTriggerEvent;

            // validate
            // null check is here for bookmarks which don't inherit triggerevent
            if (m_TriggerEvent != null) {
                m_TriggerEvent.SetTargetEditor(RavenSequenceEditor.Instance.Sequence, m_TriggerEvent.Target);
            }
        }

        public override void ResetInput() {
            m_InputState = ETriggerEventInputState.Inactive;
        }

        public override bool HasInput() {
            return m_InputState != ETriggerEventInputState.Inactive;
        }

        public override bool TryEvaluateDragging(Vector2 mousePosition, TimelineData timelineData, RavenSequenceView sequenceView) {
            var mousePos = mousePosition - m_StartDragOffset;
            if (timelineData.GetTimeAtMousePosition(mousePos) < 0) {
                return false;
            }

            var newFrame = timelineData.GetFrameAtMousePosition(mousePos);
            var offset = newFrame - m_Event.StartFrame;

            if (sequenceView.CheckFrameOverlap(m_Event, newFrame, m_Event.TrackIndex, m_Event.SubTrackIndex, true) ||
                newFrame >= sequenceView.Sequence.TotalFrameCount) {
                return false;
            }

            m_QueuedStartFrameReposition = offset;
            return true;
        }

        public override void ApplyDragging() {
            OffsetEvent(m_QueuedStartFrameReposition);
        }

        public override void StartResizeMode(Vector2 mousePosition, int firstEventStart, int lastEventEnd, EResizeMode resizeMode) {
            base.StartResizeMode(mousePosition, firstEventStart, lastEventEnd, resizeMode);
            m_StartDragOffset = m_StartDragMousePosition - m_EventRect.min;
        }

        public override bool TryNormalizeRepositioning(RavenSequenceView sequenceView, int newFirstEventStart, int newLastEventEnd) {
            var newDelta = newLastEventEnd - (double)newFirstEventStart;
            var newStartFrame = (int)(newFirstEventStart + m_StartFrameRelativePositionOnResize * newDelta);

            if (sequenceView.CheckFrameOverlap(m_Event, newStartFrame, m_Event.TrackIndex, m_Event.SubTrackIndex, true) ||
                newStartFrame >= sequenceView.Sequence.TotalFrameCount ||
                newStartFrame < 0) {
                return false;
            }

            m_QueuedStartFrameReposition = newStartFrame;
            return true;
        }

        public override void ApplyNormalizedRepositioning() {
            SetStartFrame(m_QueuedStartFrameReposition);
        }

        protected override void OnDrawExtendedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, bool foldoutEnabled, float foldoutHeight, List<RavenParameter> parameters) {
            EditorGUI.LabelField(m_EventRect, "", s_DefaultNodeBackground);
        }

        protected override void OnDrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, bool foldoutEnabled, float foldoutHeight, List<RavenParameter> parameters) {
            m_TopPartRect = new Rect(m_EventRect.x, m_EventRect.y, m_EventRect.width, 20f);
            if (!foldoutEnabled) {
                m_HandleTouchRect = m_TopPartRect;
            }
            EditorGUI.LabelField(m_TopPartRect, Name, GetNodeStyle());

            var enabledRect = m_TopPartRect;
            enabledRect.width = 16f;
            DrawEnabledBox(enabledRect);

            if (m_Selected) {
                EditorGUIUtility.AddCursorRect(m_HandleTouchRect, MouseCursor.Pan);
            }
        }

        protected override void OnDrawOptimizedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, bool foldoutEnabled, float foldoutHeight, List<RavenParameter> parameters) {
            base.OnDrawOptimizedGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutEnabled, foldoutHeight, parameters);
            bool hasConditions = m_TriggerEvent.Conditions.Count > 0;
            EditorGUI.LabelField(m_TopPartRect, Name, m_Selected ? (hasConditions ? NodeConditionSelectedStyle : NodeSelectedStyle) : (hasConditions ? NodeConditionStyle : NodeStyle));
        }

        protected override bool OnHandleInput(Vector2 mousePosition, TimelineData timelineData, RavenSequenceView sequenceView, Rect timelineTrackRect, int highestEventSubTrackIndex) {
            if (m_InputState == ETriggerEventInputState.Inactive) {
                if (UnityEngine.Event.current.type == EventType.mouseDown && UnityEngine.Event.current.button == 0) {
                    if (m_HandleTouchRect.Contains(mousePosition)) {
                        if (!m_Selected) {
                            sequenceView.DeselectAll();
                            SelectEvent(null);
                        }
                        m_InputState = ETriggerEventInputState.Dragging;

                        for (int i = 0; i < sequenceView.EventViews.Count; ++i) {
                            var view = sequenceView.EventViews[i];
                            if (view.m_Selected) {
                                view.StartDraggingMode(mousePosition, timelineData);
                            }
                        }
                        return true;
                    }
                }
            } else if (m_InputState == ETriggerEventInputState.Dragging) {
                if (UnityEngine.Event.current.type == EventType.mouseUp && UnityEngine.Event.current.button == 0 ||
                    UnityEngine.Event.current.type == EventType.Ignore) {
                    m_InputState = ETriggerEventInputState.Inactive;

                    for (int i = 0; i < sequenceView.EventViews.Count; ++i) {
                        var view = sequenceView.EventViews[i];
                        if (view.m_Selected) {
                            view.EndDraggingMode();
                        }
                    }
                    return true;
                } else if (UnityEngine.Event.current.type == EventType.mouseDrag) {
                    mousePosition = SnapPosition(sequenceView, mousePosition, timelineData);
                    DragSelectedEvents(sequenceView, mousePosition, timelineData);
                    EvaluateDraggingForTrackIndex(mousePosition, timelineTrackRect, highestEventSubTrackIndex);
                    return true;
                }
            }
            return false;
        }

        protected Rect GetContentRect() {
            return new Rect(m_EventRect.x, m_EventRect.y + m_TopPartRect.height, m_EventRect.width, m_EventRect.height - m_TopPartRect.height);
        }

        protected Rect GetHorizontalRect(float xOffsetPercentage, float yOffsetPercentage, float width, float height, float overflowSize = -1f) {
            return GetHorizontalRect(GetContentRect(), xOffsetPercentage, yOffsetPercentage, width, height, overflowSize);
        }
    }
}