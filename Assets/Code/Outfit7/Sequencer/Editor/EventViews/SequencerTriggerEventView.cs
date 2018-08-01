using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Outfit7.Util;
using Outfit7.Logic.StateMachineInternal;
using Outfit7.Logic.Util;

namespace Outfit7.Sequencer {
    public class SequencerTriggerEventView : SequencerEventView {
        protected enum TriggerEventInputState {
            INACTIVE,
            DRAGGING
        }

        private SequencerTriggerEvent Event = null;
        protected TriggerEventInputState InputState = TriggerEventInputState.INACTIVE;
        private float HotzoneWidth = 10f;
        private GUIStyle HandleLeft = (GUIStyle) "MeTransitionHandleLeft";
        private GUIStyle HandleRight = (GUIStyle) "MeTransitionHandleRight";
        //private GUIStyle Node = (GUIStyle) "MeTransPlayhead";
        //MeBlendPosition
        private Vector2 StartDragMousePosition = new Vector2();
        protected Color LineColor = new Color(0.6f, 0.6f, 1f);
        protected Color LineConditionColor = new Color(1f, 0.5f, 0f);
        private Color PreplayLineColor = new Color(1f, 1f, 1f);
        private Rect HandleTouchRect;

        public override void OnInit(object evnt, object parent) {
            Event = evnt as SequencerTriggerEvent;
            base.OnInit(evnt, parent);
        }

        public override void OnDefineEventRect(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, bool optimizedView = false) {
            float elementHeight = (foldoutData.Enabled ? foldoutData.Height : 20f);
            startHeight += (optimizedView ? 0 : Event.UiTrackIndex) * elementHeight;
            EventRect = new Rect(timelineData.Rect.x + timelineData.LenghtOfASecond * (Event.StartTime + timelineData.Offset) - HotzoneWidth * 0.5f,
                startHeight,
                HotzoneWidth,
                elementHeight);
            HandleTouchRect = EventRect;
        }

        public override void OnDrawExtendedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            bool hasConditions = Event.Conditions.Count > 0;
            GUIUtil.DrawLine(new Vector2(EventRect.center.x, EventRect.y), new Vector2(EventRect.center.x, EventRect.y + EventRect.height), hasConditions ? LineConditionColor : LineColor);
        }

        public override void OnDrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            switch (Event.EventDirection) {
                case SequencerTriggerEvent.EEventDirection.LEFT:
                    EditorGUI.LabelField(new Rect(EventRect.x - 2f, EventRect.y, EventRect.width, EventRect.height), "Event", HandleLeft);
                    break;
                case SequencerTriggerEvent.EEventDirection.MIDDLE:
                case SequencerTriggerEvent.EEventDirection.RIGHT:
                    EditorGUI.LabelField(new Rect(EventRect.x + 4f, EventRect.y, EventRect.width, EventRect.height), "Event", HandleRight);
                    break;
            }
            EditorGUIUtility.AddCursorRect(HandleTouchRect, MouseCursor.ResizeHorizontal);

            //draw preplay
            if (Event.Preplay) {
                float LinePos = splitViewLeft + (timelineData.Offset) * timelineData.LenghtOfASecond;
                float foldoutHeight = foldoutData.Enabled ? foldoutData.Height : EventRect.height;
                float lineHeight = startHeight + foldoutHeight * Event.UiTrackIndex + foldoutHeight * 0.66f; 
                GUIUtil.DrawLine(new Vector2(LinePos, lineHeight), new Vector2(EventRect.center.x, lineHeight), PreplayLineColor);
                float midPoint = (EventRect.center.x + LinePos) / 2f;
                if (Mathf.Abs(EventRect.center.x - LinePos) > 90)
                    GUI.Label(new Rect(midPoint - 25f, lineHeight - 15f, 50f, 15f), "Preplay");
            }
        }

        public override void OnDrawOptimizedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            OnDrawGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
        }

        protected Rect GetRectPosition(float y, float width, float height) {
            Rect r = new Rect(0, EventRect.yMin + y, width, height);
            switch (Event.EventDirection) {
                case SequencerTriggerEvent.EEventDirection.LEFT:
                    r.x = EventRect.center.x - width;
                    break;
                case SequencerTriggerEvent.EEventDirection.MIDDLE:
                case SequencerTriggerEvent.EEventDirection.RIGHT:
                    r.x = EventRect.center.x;
                    break;
            }

            return r;
        }

        protected override bool OnHandleInput(TimelineData timelineData, SequencerSequenceView sequenceView, Rect timelineTrackRect, int highiestEventTrackIndex, object actor) {
            if (InputState == TriggerEventInputState.INACTIVE) {
                if (UnityEngine.Event.current.type == EventType.mouseDown && UnityEngine.Event.current.button == 0) {
                    if (HandleTouchRect.Contains(SequencerSequenceView.GetCurrentMousePosition())) {
                        if (!Selected)
                            sequenceView.DeselectAll();
                        InputState = TriggerEventInputState.DRAGGING;
                        SetDraggingMode(SequencerSequenceView.GetCurrentMousePosition());
                        foreach (SequencerEventView selectedEventView in sequenceView.GetAllEvents(true)) {
                            selectedEventView.SetDraggingMode(SequencerSequenceView.GetCurrentMousePosition());
                        }
                        return true;
                    }
                }
            } else if (InputState == TriggerEventInputState.DRAGGING) {
                if (UnityEngine.Event.current.type == EventType.mouseUp && UnityEngine.Event.current.button == 0) {
                    InputState = TriggerEventInputState.INACTIVE;
                    return true;
                } else if (UnityEngine.Event.current.type == EventType.mouseDrag) {
                    EvaluateDragging(SequencerSequenceView.GetCurrentMousePosition(), timelineData, sequenceView);
                    EvaluateDraggingForTrackIndex(SequencerSequenceView.GetCurrentMousePosition(), timelineTrackRect, highiestEventTrackIndex);
                    foreach (SequencerEventView selectedEventView in sequenceView.GetAllEvents(true)) {
                        selectedEventView.EvaluateDragging(SequencerSequenceView.GetCurrentMousePosition(), timelineData, sequenceView);
                    }
                    return true;
                }
            }
            return false;
        }

        public override void SetDraggingMode(Vector2 mousePosition) {
            StartDragMousePosition = mousePosition;
            StartDragOffset = StartDragMousePosition - EventRect.min + new Vector2(-HotzoneWidth * 0.5f, 0f);
        }

        private void EvaluateDraggingForTrackIndex(Vector2 mousePosition, Rect trackRect, int highiestTrackIndex) {
            if (UsingOptimizedGui)
                return;
            if (mousePosition.y < trackRect.y)
                return;

            if (mousePosition.y > trackRect.yMax) {
                Event.UiTrackIndex++;
                return;
            }

            Event.UiTrackIndex = (int) (((mousePosition.y - trackRect.y) / trackRect.height) * (highiestTrackIndex + 1));
        }

        public override void EvaluateDragging(Vector2 mousePosition, TimelineData timelineData, SequencerSequenceView sequenceView) {
            Vector2 mousePos = mousePosition - StartDragOffset;
            Event.StartTime = sequenceView.Snap(Mathf.Max(0, timelineData.GetTimeAtMousePosition(mousePos)), Event, ref Event.EventDirection);
        }

        public override void NormalizedRepositioning(float firstEventStart, float lastEventEnd, float newFirstEventStart, float newLastEventEnd) {
            float oldStartNormalizedTime = Mathf.InverseLerp(firstEventStart, lastEventEnd, Event.StartTime);
            Event.StartTime = Mathf.Lerp(newFirstEventStart, newLastEventEnd, oldStartNormalizedTime);
        }
    }
}
