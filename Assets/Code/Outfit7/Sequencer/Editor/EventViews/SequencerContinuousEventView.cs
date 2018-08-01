using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Outfit7.Logic.StateMachineInternal;
using Outfit7.Logic.Util;

namespace Outfit7.Sequencer {
    public class SequencerContinuousEventView : SequencerEventView {
        protected enum ContinousEventInputState {
            INACTIVE,
            DRAGGING,
            RESIZE_LEFT,
            RESIZE_RIGHT
        }

        private SequencerContinuousEvent Event = null;
        protected ContinousEventInputState InputState = ContinousEventInputState.INACTIVE;
        protected Rect TopPartRect = new Rect();
        private Rect LeftHotzoneRect = new Rect();
        private Rect RightHotzoneRect = new Rect();
        private float HotzoneWidth = 6f;
        protected static GUIStyle Node = (GUIStyle) "flow node 0";
        protected static GUIStyle NodeSelected = (GUIStyle) "flow node 0 on";
        protected GUIStyle NodeCondition = (GUIStyle) "flow node 5";
        protected GUIStyle NodeConditionSelected = (GUIStyle) "flow node 5 on";
        private GUIStyle NodeBg = (GUIStyle) "sv_iconselector_labelselection";
        private Vector2 StartDragMousePosition = new Vector2();
        private Color PreplayLineColor = new Color(1f, 1f, 1f);
        private Rect HandleTouchRect;


        public override void Init(object evnt, object parent) {
            Event = evnt as SequencerContinuousEvent;
            base.Init(evnt, parent);
        }

        public override void OnDefineEventRect(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, bool optimizedView = false) {
            float elementHeight = (foldoutData.Enabled ? foldoutData.Height : 20f);
            startHeight += (optimizedView ? 0 : Event.UiTrackIndex) * elementHeight;
            EventRect = new Rect(timelineData.Rect.x + timelineData.LenghtOfASecond * (Event.StartTime + timelineData.Offset),
                startHeight,
                timelineData.LenghtOfASecond * Event.Duration,
                elementHeight);
            HandleTouchRect = EventRect;
        }

        public override void OnDrawExtendedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            EditorGUI.LabelField(EventRect, "", NodeBg);
        }

        public override void OnDrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            //Debug.LogError(timelineData.Rect.x + " " + timelineData.LenghtOfASecond + " " + Event.StartTime + " " + timelineData.Offset + " " + Event.Duration);
            SetupGui();
            bool hasConditions = Event.Conditions.Count > 0;
            if (!foldoutData.Enabled)
                HandleTouchRect = TopPartRect;
            EditorGUI.LabelField(TopPartRect, GetName(), Selected ? (hasConditions ? NodeConditionSelected : NodeSelected) : (hasConditions ? NodeCondition : Node));
            LeftHotzoneRect = new Rect(HandleTouchRect.x - HotzoneWidth * 0.5f, HandleTouchRect.y, HotzoneWidth, HandleTouchRect.height);
            RightHotzoneRect = new Rect(HandleTouchRect.x + HandleTouchRect.width - HotzoneWidth * 0.5f, HandleTouchRect.y, HotzoneWidth, HandleTouchRect.height);
            EditorGUIUtility.AddCursorRect(LeftHotzoneRect, MouseCursor.ResizeHorizontal);
            EditorGUIUtility.AddCursorRect(RightHotzoneRect, MouseCursor.ResizeHorizontal);

            //draw preplay
            if (Event.Preplay) {
                float LinePos = splitViewLeft + (timelineData.Offset) * timelineData.LenghtOfASecond;
                float foldoutHeight = foldoutData.Enabled ? foldoutData.Height : EventRect.height;
                float lineHeight = startHeight + foldoutHeight * Event.UiTrackIndex + foldoutHeight * 0.66f; 
                GUIUtil.DrawLine(new Vector2(LinePos, lineHeight), new Vector2(EventRect.xMin, lineHeight), PreplayLineColor);
                float midPoint = (EventRect.center.x + LinePos) / 2f;
                if (Mathf.Abs(EventRect.center.x - LinePos) > 90)
                    GUI.Label(new Rect(midPoint - 25f, lineHeight - 15f, 50f, 15f), "Preplay");
            }
        }

        static public Rect MimicEventDraw(float start, float end, TimelineData timelineData, float startHeight) {
            Rect MimicRect = new Rect(timelineData.Rect.x + timelineData.LenghtOfASecond * (start + timelineData.Offset),
                                 startHeight,
                                 timelineData.LenghtOfASecond * (end - start),
                                 20f);
            EditorGUI.LabelField(MimicRect, "", Node);
            return MimicRect;
        }

        public override void OnDrawOptimizedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            SetupGui();
            bool hasConditions = Event.Conditions.Count > 0;
            EditorGUI.LabelField(TopPartRect, GetName(), Selected ? (hasConditions ? NodeConditionSelected : NodeSelected) : (hasConditions ? NodeCondition : Node));
        }

        protected void SetupGui() {
            TopPartRect = new Rect(EventRect.x, EventRect.y, EventRect.width, 20f);
            LeftHotzoneRect = new Rect(EventRect.x - HotzoneWidth * 0.5f, EventRect.y, HotzoneWidth, EventRect.height);
            RightHotzoneRect = new Rect(EventRect.x + EventRect.width - HotzoneWidth * 0.5f, EventRect.y, HotzoneWidth, EventRect.height);
            EditorGUIUtility.AddCursorRect(LeftHotzoneRect, MouseCursor.ResizeHorizontal);
            EditorGUIUtility.AddCursorRect(RightHotzoneRect, MouseCursor.ResizeHorizontal);
        }

        protected virtual void OnDraggingEnd() {

        }

        protected override bool OnHandleInput(TimelineData timelineData, SequencerSequenceView sequenceView, Rect timelineTrackRect, int highiestEventTrackIndex, object actor) {
            if (EventType.KeyDown == UnityEngine.Event.current.type && KeyCode.Space == UnityEngine.Event.current.keyCode && UnityEngine.Event.current.alt && EventRect.Contains(SequencerSequenceView.GetCurrentMousePosition())) {
                sequenceView.SetPreviewEvent(Event);
            }

            if (InputState == ContinousEventInputState.INACTIVE) {
                if (UnityEngine.Event.current.type == EventType.mouseDown && UnityEngine.Event.current.button == 0) {
                    if (LeftHotzoneRect.Contains(SequencerSequenceView.GetCurrentMousePosition())) {
                        if (!Selected)
                            sequenceView.DeselectAll();
                        InputState = ContinousEventInputState.RESIZE_LEFT;
                        StartDragMousePosition = SequencerSequenceView.GetCurrentMousePosition();
                        StartDragOffset = StartDragMousePosition - LeftHotzoneRect.min;
                        return true;
                    } else if (RightHotzoneRect.Contains(SequencerSequenceView.GetCurrentMousePosition())) {
                        if (!Selected)
                            sequenceView.DeselectAll();
                        InputState = ContinousEventInputState.RESIZE_RIGHT;
                        StartDragMousePosition = SequencerSequenceView.GetCurrentMousePosition();
                        StartDragOffset = StartDragMousePosition - RightHotzoneRect.min;
                        return true;
                    } else if (HandleTouchRect.Contains(SequencerSequenceView.GetCurrentMousePosition())) {
                        if (!Selected)
                            sequenceView.DeselectAll();
                        InputState = ContinousEventInputState.DRAGGING;
                        SetDraggingMode(SequencerSequenceView.GetCurrentMousePosition());
                        foreach (SequencerEventView selectedEventView in sequenceView.GetAllEvents(true)) {
                            selectedEventView.SetDraggingMode(SequencerSequenceView.GetCurrentMousePosition());
                        }
                        return true;
                    }
                }
            } else if (InputState == ContinousEventInputState.RESIZE_LEFT) {
                if (UnityEngine.Event.current.type == EventType.mouseUp && UnityEngine.Event.current.button == 0) {
                    InputState = ContinousEventInputState.INACTIVE;
                    if (sequenceView.IsRecording())
                        OnDraggingEnd();
                    return true;
                } else if (UnityEngine.Event.current.type == EventType.mouseDrag) {
                    float firstEventStart = Event.StartTime;
                    float lastEventEnd = EventEndTime();

                    foreach (SequencerEventView selectedEventView in sequenceView.GetAllEvents(true)) {
                        if (selectedEventView.GetEvent().StartTime < firstEventStart) {
                            return false;
                        }   
                    }

                    foreach (SequencerEventView selectedEventView in sequenceView.GetAllEvents(true)) {
                        if (selectedEventView.EventEndTime() > lastEventEnd)
                            lastEventEnd = selectedEventView.EventEndTime();
                    }

                    Vector2 mousePos = UnityEngine.Event.current.mousePosition - StartDragOffset;
                    float newFirstEventStart = sequenceView.Snap(timelineData.GetTimeAtMousePosition(mousePos)).Value;

                    NormalizedRepositioning(firstEventStart, lastEventEnd, newFirstEventStart, lastEventEnd);
                    foreach (SequencerEventView selectedEventView in sequenceView.GetAllEvents(true)) {
                        if (selectedEventView != this)
                            selectedEventView.NormalizedRepositioning(firstEventStart, lastEventEnd, newFirstEventStart, lastEventEnd);
                    }
                    return true;
                }

            } else if (InputState == ContinousEventInputState.RESIZE_RIGHT) {
                if (UnityEngine.Event.current.type == EventType.mouseUp && UnityEngine.Event.current.button == 0) {
                    InputState = ContinousEventInputState.INACTIVE;
                    if (sequenceView.IsRecording())
                        OnDraggingEnd();
                    return true;
                } else if (UnityEngine.Event.current.type == EventType.mouseDrag) {
                    float firstEventStart = Event.StartTime;
                    float lastEventEnd = EventEndTime();

                    foreach (SequencerEventView selectedEventView in sequenceView.GetAllEvents(true)) {
                        if (selectedEventView.EventEndTime() > lastEventEnd) {
                            return false;
                        }   
                    }

                    foreach (SequencerEventView selectedEventView in sequenceView.GetAllEvents(true)) {
                        if (selectedEventView.GetEvent().StartTime < firstEventStart)
                            firstEventStart = selectedEventView.GetEvent().StartTime;
                    }

                    Vector2 mousePos = UnityEngine.Event.current.mousePosition - StartDragOffset;
                    float newLastEventEnd = sequenceView.Snap(timelineData.GetTimeAtMousePosition(mousePos), Event, true).Value;

                    NormalizedRepositioning(firstEventStart, lastEventEnd, firstEventStart, newLastEventEnd);
                    foreach (SequencerEventView selectedEventView in sequenceView.GetAllEvents(true)) {
                        if (selectedEventView != this)
                            selectedEventView.NormalizedRepositioning(firstEventStart, lastEventEnd, firstEventStart, newLastEventEnd);
                    }

                    return true;
                }

            } else if (InputState == ContinousEventInputState.DRAGGING) {
                if (UnityEngine.Event.current.type == EventType.mouseUp && UnityEngine.Event.current.button == 0) {
                    InputState = ContinousEventInputState.INACTIVE;
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

            return InputState != ContinousEventInputState.INACTIVE;
        }

        public override void SetDraggingMode(Vector2 mousePosition) {
            StartDragMousePosition = mousePosition;
            StartDragOffset = StartDragMousePosition - EventRect.min;
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
            Event.StartTime = sequenceView.Snap(Mathf.Max(0, timelineData.GetTimeAtMousePosition(mousePos)), Event).Value;

            if (sequenceView.IsRecording())
                OnDraggingEnd();
        }

        public override float EventEndTime() {
            return Event.StartTime + Event.Duration;
        }

        public override void NormalizedRepositioning(float firstEventStart, float lastEventEnd, float newFirstEventStart, float newLastEventEnd) {
            float oldStartNormalizedTime = Mathf.InverseLerp(firstEventStart, lastEventEnd, Event.StartTime);
            float oldEndNormalizedTime = Mathf.InverseLerp(firstEventStart, lastEventEnd, EventEndTime());

            Event.StartTime = Mathf.Lerp(newFirstEventStart, newLastEventEnd, oldStartNormalizedTime);
            Event.Duration = Mathf.Lerp(newFirstEventStart, newLastEventEnd, oldEndNormalizedTime) - Event.StartTime;
        }
    }
}
