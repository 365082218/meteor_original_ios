using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Outfit7.Util;
using Outfit7.Logic.StateMachineInternal;
using Outfit7.Logic.Util;

namespace Outfit7.Sequencer {
    [SequencerQuickSearchAttribute("for")]
    [SequencerQuickSearchDisplayAttribute("For Loop")]
    [SequencerNormalTrackAttribute("Sequence/For Loop")]
    public class SequencerForLoopEventView : SequencerTriggerEventView {
        private SequencerForLoopEvent Event = null;
        private Color LoopLineColor = new Color(0.2f, 0.6f, .2f);
        private TriggerEventInputState GoToInputState = TriggerEventInputState.INACTIVE;
        private Rect GoToRect;
        private Vector2 StartDragMousePosition = new Vector2();

        public override void OnInit(object evnt, object parent) {
            Event = evnt as SequencerForLoopEvent;
            base.OnInit(evnt, parent);
        }

        public override string GetName() {
            return "For";
        }

        public override void OnDrawExtendedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawExtendedGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
            GUI.Label(GetRectPosition(10f, 40f, 20f), GetName());

            ParameterFieldView.DrawParameterField(GetRectPosition(30f, 40f, 30f), Event.Cycle, parameters);
            EditorGUI.LabelField(GetRectPosition(60f, 40f, 15f), Event.GetRemainingCycleCount());
        }

        public override void OnDrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
            //GoTo line
            float LinePos = splitViewLeft + (timelineData.Offset + Event.GoToTime.Value) * timelineData.LenghtOfASecond;
            if (LinePos > splitViewLeft && LinePos < windowSize.width) {
                float foldoutHeight = foldoutData.Enabled ? foldoutData.Height : EventRect.height;
                GUIUtil.DrawLine(new Vector2(LinePos, startHeight + foldoutHeight * Event.UiTrackIndex), new Vector2(LinePos, startHeight + foldoutHeight * (Event.UiTrackIndex + 1)), LoopLineColor);
                GoToRect = new Rect(LinePos - 3f, startHeight, 6f, foldoutData.Enabled ? foldoutData.Height + 20f : EventRect.height);
                EditorGUIUtility.AddCursorRect(GoToRect, MouseCursor.ResizeHorizontal);
                float lineHeight = startHeight + foldoutHeight * Event.UiTrackIndex + foldoutHeight * 0.5f; 
                GUIUtil.DrawLine(new Vector2(EventRect.center.x, lineHeight), new Vector2(LinePos, lineHeight), LoopLineColor);
                float direction = LinePos < EventRect.center.x ? 5f : -5f;
                GUIUtil.DrawLine(new Vector2(LinePos + direction, lineHeight + 5f), new Vector2(LinePos, lineHeight), LoopLineColor);
                GUIUtil.DrawLine(new Vector2(LinePos + direction, lineHeight - 5f), new Vector2(LinePos, lineHeight), LoopLineColor);
            } else {
                GoToRect = new Rect();
            }
        }


        public override void OnRefresh(object actor) {
            base.OnRefresh(actor);
            if (Event.AffectingSequence == null)
                Event.AffectingSequence = Event.GetComponent<SequencerSequence>();
        }

        protected override bool OnHandleInput(TimelineData timelineData, SequencerSequenceView sequenceView, Rect timelineTrackRect, int highiestEventTrackIndex, object actor) {
            if (Event.GoToTime.FieldType == ParameterFieldType.SOLID) {
                if (GoToInputState == TriggerEventInputState.INACTIVE) {
                    if (UnityEngine.Event.current.type == EventType.mouseDown) {
                        if (GoToRect.Contains(SequencerSequenceView.GetCurrentMousePosition())) {
                            GoToInputState = TriggerEventInputState.DRAGGING;
                            StartDragMousePosition = SequencerSequenceView.GetCurrentMousePosition();
                            StartDragOffset = StartDragMousePosition - GoToRect.min;
                            return true;
                        }
                    }
                } else if (GoToInputState == TriggerEventInputState.DRAGGING) {
                    if (UnityEngine.Event.current.type == EventType.mouseUp) {
                        GoToInputState = TriggerEventInputState.INACTIVE;
                        return true;
                    }
                    if (UnityEngine.Event.current.type == EventType.mouseDrag) {
                        Vector2 mousePos = SequencerSequenceView.GetCurrentMousePosition() - StartDragOffset;
                        Event.GoToTime.Value = sequenceView.Snap(Mathf.Max(0, timelineData.GetTimeAtMousePosition(mousePos)), Event, false).Value;
                        return true;
                    }
                }
            }
            return base.OnHandleInput(timelineData, sequenceView, timelineTrackRect, highiestEventTrackIndex, actor);
        }
    }
}
