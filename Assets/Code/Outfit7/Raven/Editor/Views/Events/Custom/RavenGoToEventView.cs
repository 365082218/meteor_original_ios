#if !STARLITE_EDITOR
using Outfit7.Logic.Util;
#else
using Starlite;
#endif

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Starlite.Raven {

    public sealed class RavenGoToEventView : RavenTriggerEventView {
        private RavenGoToEvent m_GoToEvent = null;
        private Color m_LoopLineColor = new Color(0.2f, 0.6f, .2f);
        private ETriggerEventInputState m_GoToInputState = ETriggerEventInputState.Inactive;
        private Rect m_GoToRect;

        private int m_QueuedLastFrameReposition;

        private static GUIStyle s_Node = (GUIStyle)"flow node 3";
        private static GUIStyle s_NodeSelected = (GUIStyle)"flow node 3 on";

        public override string Name {
            get {
                return string.Format("Goto {0:0.00} ({1})", RavenSequenceEditor.Instance.Sequence.GetTimeForFrame(m_GoToEvent.FrameToJumpTo), m_GoToEvent.FrameToJumpTo);
            }
        }

        public override bool AlwaysVisible {
            get {
                return true;
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
            m_GoToEvent = evnt as RavenGoToEvent;
        }

        public override void ResetInput() {
            base.ResetInput();
            m_GoToInputState = ETriggerEventInputState.Inactive;
        }

        public override bool HasInput() {
            if (base.HasInput()) {
                return true;
            }

            return m_GoToInputState != ETriggerEventInputState.Inactive;
        }

        public override void StartResizeMode(Vector2 mousePosition, int firstEventStart, int lastEventEnd, EResizeMode resizeMode) {
            base.StartResizeMode(mousePosition, firstEventStart, lastEventEnd, resizeMode);
            var duration = (double)m_InitialResizeDelta;
            m_EndFrameRelativePositionOnResize = (m_GoToEvent.FrameToJumpTo - lastEventEnd) / duration;
        }

        public override bool TryNormalizeRepositioning(RavenSequenceView sequenceView, int newFirstEventStart, int newLastEventEnd) {
            var newDelta = newLastEventEnd - newFirstEventStart;
            var newDeltaDouble = (double)newDelta;

            var newStartFrame = (int)(newFirstEventStart + m_StartFrameRelativePositionOnResize * newDeltaDouble);
            var newEndFrame = (int)(newLastEventEnd + m_EndFrameRelativePositionOnResize * newDeltaDouble);

            if (sequenceView.CheckFrameOverlap(m_Event, newStartFrame, m_Event.TrackIndex, m_Event.SubTrackIndex, true) ||
                newStartFrame >= sequenceView.Sequence.TotalFrameCount ||
                newStartFrame < 0) {
                return false;
            }

            m_QueuedStartFrameReposition = newStartFrame;
            m_QueuedLastFrameReposition = newEndFrame;
            return true;
        }

        public override void ApplyNormalizedRepositioning() {
            Undo.RecordObject(m_GoToEvent, "ApplyNormalizedRepositioning");
            SetStartFrame(m_QueuedStartFrameReposition);
            m_GoToEvent.FrameToJumpTo = m_QueuedLastFrameReposition;
        }

        protected override void OnDrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, bool foldoutEnabled, float foldoutHeight, List<RavenParameter> parameters) {
            base.OnDrawGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutEnabled, foldoutHeight, parameters);

            float elementHeight = (foldoutEnabled ? foldoutHeight : 20f);

            var linePos = timelineData.GetPositionAtFrame(m_GoToEvent.FrameToJumpTo);
            var height = foldoutEnabled ? foldoutHeight : m_EventRect.height;
            GUIUtil.DrawLine(new Vector2(linePos, startHeight + height * m_Event.SubTrackIndex), new Vector2(linePos, startHeight + height * (m_Event.SubTrackIndex + 1)), m_LoopLineColor);
            float lineHeight = startHeight + height * m_Event.SubTrackIndex + height * 0.5f;
            GUIUtil.DrawLine(new Vector2(m_EventRect.center.x, lineHeight), new Vector2(linePos, lineHeight), m_LoopLineColor);
            float direction = linePos < m_EventRect.center.x ? 5f : -5f;
            GUIUtil.DrawLine(new Vector2(linePos + direction, lineHeight + 5f), new Vector2(linePos, lineHeight), m_LoopLineColor);
            GUIUtil.DrawLine(new Vector2(linePos + direction, lineHeight - 5f), new Vector2(linePos, lineHeight), m_LoopLineColor);

            if (linePos > splitViewLeft && linePos < windowSize.width) {
                m_GoToRect = new Rect(linePos - 3f, startHeight + m_Event.SubTrackIndex * elementHeight, 6f, foldoutEnabled ? foldoutHeight + 20f : m_EventRect.height);
                EditorGUIUtility.AddCursorRect(m_GoToRect, MouseCursor.ResizeHorizontal);
            } else {
                m_GoToRect = new Rect();
            }
        }

        override protected void DrawFrameLabels(TimelineData timelineData) {
            base.DrawFrameLabels(timelineData);

            GUIStyle centerStyle = new GUIStyle();
            centerStyle.alignment = TextAnchor.MiddleCenter;

            if (m_Event.StartFrame != m_GoToEvent.FrameToJumpTo) {

                // middle label
                int frameDiff = m_GoToEvent.FrameToJumpTo - m_Event.StartFrame;
                float oneFrameWidth = m_EventRect.width;
                float width = frameDiff * oneFrameWidth;
                Vector2 middleLeft = new Vector2(m_EventRect.x, m_SelectedRect.y - 7);
                Vector2 middleRight = new Vector2(m_EventRect.x + width, m_SelectedRect.y - 7);
                GUIUtil.DrawLine(middleLeft, middleRight, Color.gray);
                centerStyle.normal.textColor = Color.gray;
                EditorGUI.LabelField(new Rect(middleLeft.x + width * 0.5f - 15, middleRight.y - 23, 30, 30), Mathf.Abs(frameDiff).ToString(), centerStyle);

                var linePos = timelineData.GetPositionAtFrame(m_GoToEvent.FrameToJumpTo);
                Vector2 bottomLeft = new Vector2(linePos, m_SelectedRect.y);
                Vector2 topLeft = new Vector2(linePos, m_SelectedRect.y - 15);
                GUIUtil.DrawLine(bottomLeft, topLeft, Color.green);
                centerStyle.normal.textColor = Color.green;
                EditorGUI.LabelField(new Rect(bottomLeft.x - 15, topLeft.y - 25, 30, 30), m_GoToEvent.FrameToJumpTo.ToString(), centerStyle);
            }
        }

        protected override void OnDrawExtendedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, bool foldoutEnabled, float foldoutHeight, List<RavenParameter> parameters) {
            base.OnDrawExtendedGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutEnabled, foldoutHeight, parameters);
            var sequence = RavenSequenceEditor.Instance.Sequence;

            var rect = GetHorizontalRect(0.5f, 0.1f, 120f, 15f, 60f);
            var rect2 = GetHorizontalRect(0.5f, 0.3f, 120f, 15f, 60f);

            EditorGUI.LabelField(rect, "T");
            rect.x += 16f;
            rect.width -= 16f;
            m_GoToEvent.FrameToJumpTo = (int)sequence.GetFrameForTime(EditorGUICustom.DelayedDoubleField(rect, "", sequence.GetTimeForFrame(m_GoToEvent.FrameToJumpTo)));

            EditorGUI.LabelField(rect2, "F");
            rect2.x += 16f;
            rect2.width -= 16f;
            m_GoToEvent.FrameToJumpTo = EditorGUI.IntField(rect2, "", m_GoToEvent.FrameToJumpTo);

            if (m_GoToEvent.FrameToJumpTo >= sequence.TotalFrameCount) {
                m_GoToEvent.FrameToJumpTo = sequence.TotalFrameCount - 1;
            } else if (m_GoToEvent.FrameToJumpTo < 0) {
                m_GoToEvent.FrameToJumpTo = 0;
            }
        }

        public override void ApplyDragging() {
            if (UnityEngine.Event.current.modifiers == EventModifiers.Shift) {
                OffsetEvent(m_QueuedStartFrameReposition);
            } else {
                // queued frame rep. is delta here
                SetStartFrame(m_GoToEvent.StartFrame + m_QueuedStartFrameReposition);
            }
        }

        protected override bool OnHandleInput(Vector2 mousePosition, TimelineData timelineData, RavenSequenceView sequenceView, Rect timelineTrackRect, int highestEventSubTrackIndex) {
            if (m_GoToInputState == ETriggerEventInputState.Inactive) {
                if (UnityEngine.Event.current.type == EventType.mouseDown) {
                    if (m_GoToRect.Contains(mousePosition)) {
                        m_GoToInputState = ETriggerEventInputState.Dragging;
                        m_StartDragMousePosition = mousePosition;
                        m_StartDragOffset = m_StartDragMousePosition - m_GoToRect.min;
                        return true;
                    }
                }
            } else if (m_GoToInputState == ETriggerEventInputState.Dragging) {
                if (UnityEngine.Event.current.type == EventType.mouseUp || UnityEngine.Event.current.type == EventType.Ignore) {
                    m_GoToInputState = ETriggerEventInputState.Inactive;
                    return true;
                }
                if (UnityEngine.Event.current.type == EventType.mouseDrag) {
                    Vector2 mousePos = mousePosition + m_StartDragOffset;
                    m_GoToEvent.FrameToJumpTo = timelineData.GetFrameAtMousePosition(mousePos);
                    return true;
                }
            }
            return base.OnHandleInput(mousePosition, timelineData, sequenceView, timelineTrackRect, highestEventSubTrackIndex);
        }

        protected override void OnDrawEventsContextMenu(GenericMenu menu, RavenTrackView parent) {
        }

        protected override void OnUpdateWhileRecording(double currentTime) {
        }

        protected override void OnRecordingStart() {
        }

        protected override void OnRecordingStop() {
        }
    }
}