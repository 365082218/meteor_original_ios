using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Starlite.Raven {

    public abstract class RavenContinuousEventView : RavenEventView {

        protected enum EContinousEventInputState {
            Inactive,
            Dragging,
            ResizeLeft,
            ResizeRight
        }

        private const float c_HotzoneWidth = 6f;
        private const float c_TopBarHeight = 20f;

        protected EContinousEventInputState m_InputState = EContinousEventInputState.Inactive;
        protected Rect m_TopPartRect = new Rect();

        private RavenContinuousEvent m_ContinuousEvent = null;
        private Rect m_LeftHotzoneRect = new Rect();
        private Rect m_RightHotzoneRect = new Rect();

        private int m_QueuedStartFrameReposition;
        private int m_QueuedLastFrameReposition;

        public override int QueuedStartFrameReposition {
            get {
                return m_QueuedStartFrameReposition;
            }
        }

        public override int QueuedLastFrameReposition {
            get {
                return m_QueuedLastFrameReposition;
            }
        }

        public override void Initialize(RavenEvent evnt, RavenTrackView parent) {
            base.Initialize(evnt, parent);
            m_ContinuousEvent = evnt as RavenContinuousEvent;

            // validate
            m_ContinuousEvent.SetTargetEditor(RavenSequenceEditor.Instance.Sequence, m_ContinuousEvent.Target);
        }

        public override void ResetInput() {
            m_InputState = EContinousEventInputState.Inactive;
        }

        public override bool HasInput() {
            return m_InputState != EContinousEventInputState.Inactive;
        }

        public override bool TryEvaluateDragging(Vector2 mousePosition, TimelineData timelineData, RavenSequenceView sequenceView) {
            if (sequenceView.IsRecording()) {
                return false;
            }

            var mousePos = mousePosition - m_StartDragOffset;
            if (timelineData.GetTimeAtMousePosition(mousePos) < 0) {
                return false;
            }

            var offset = timelineData.GetFrameAtMousePosition(mousePos) - m_Event.StartFrame;

            var startFrame = m_Event.StartFrame + offset;
            var lastFrame = m_Event.LastFrame + offset;
            if (sequenceView.CheckContinuousFrameOverlap(m_Event, startFrame, lastFrame, m_Event.TrackIndex, m_Event.SubTrackIndex, true) ||
                lastFrame >= sequenceView.Sequence.TotalFrameCount) {
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
            if (resizeMode == EResizeMode.Left) {
                m_StartDragOffset = m_StartDragMousePosition - m_LeftHotzoneRect.min;
            } else {
                m_StartDragOffset = m_StartDragMousePosition - m_RightHotzoneRect.min;
            }
        }

        public override bool TryNormalizeRepositioning(RavenSequenceView sequenceView, int newFirstEventStart, int newLastEventEnd) {
            var newDelta = newLastEventEnd - newFirstEventStart;
            var newDeltaDouble = (double)newDelta;

            var newStartFrame = (int)(newFirstEventStart + m_StartFrameRelativePositionOnResize * newDeltaDouble);
            var newEndFrame = (int)(newLastEventEnd + m_EndFrameRelativePositionOnResize * newDeltaDouble);

            if (m_ContinuousEvent.IsEventLockedAtOneFrame) {
                newEndFrame = newStartFrame + 1;
            }

            if (newEndFrame == newStartFrame) {
                ++newEndFrame;
            } else if (newDelta < m_InitialResizeDelta && m_InitialResizeDurationInFrames < (newEndFrame - newStartFrame)) {
                --newEndFrame;
            }

            if (sequenceView.CheckContinuousFrameOverlap(m_Event, newStartFrame, newEndFrame - 1, m_Event.TrackIndex, SubTrackIndex, true) ||
                newEndFrame > sequenceView.Sequence.TotalFrameCount ||
                newStartFrame < 0) {
                return false;
            }

            m_QueuedStartFrameReposition = newStartFrame;
            m_QueuedLastFrameReposition = newEndFrame - 1;
            return true;
        }

        public override void ApplyNormalizedRepositioning() {
            SetStartFrame(m_QueuedStartFrameReposition);
            SetLastFrame(m_QueuedLastFrameReposition);
        }

        protected override void OnDrawExtendedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, bool foldoutEnabled, float foldoutHeight, List<RavenParameter> parameters) {
            EditorGUI.LabelField(EventRect, "", s_DefaultNodeBackground);
        }

        protected override void OnDrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, bool foldoutEnabled, float foldoutHeight, List<RavenParameter> parameters) {
            m_TopPartRect = new Rect(m_EventRect.x, m_EventRect.y, m_EventRect.width, c_TopBarHeight);
            if (!foldoutEnabled) {
                m_HandleTouchRect = m_TopPartRect;
            }

            if (m_Selected) {
                EditorGUIUtility.AddCursorRect(m_HandleTouchRect, MouseCursor.Pan);
            }

            EditorGUI.LabelField(m_TopPartRect, Name, GetNodeStyle());

            var enabledRect = m_TopPartRect;
            enabledRect.width = 16f;
            DrawEnabledBox(enabledRect);

            if (!m_ContinuousEvent.IsEventLockedAtOneFrame) {
                m_LeftHotzoneRect = new Rect(m_HandleTouchRect.x - c_HotzoneWidth * 0.5f, m_HandleTouchRect.y, c_HotzoneWidth, m_HandleTouchRect.height);
                m_RightHotzoneRect = new Rect(m_HandleTouchRect.x + m_HandleTouchRect.width - c_HotzoneWidth * 0.5f, m_HandleTouchRect.y, c_HotzoneWidth, m_HandleTouchRect.height);
                EditorGUIUtility.AddCursorRect(m_LeftHotzoneRect, MouseCursor.ResizeHorizontal);
                EditorGUIUtility.AddCursorRect(m_RightHotzoneRect, MouseCursor.ResizeHorizontal);
            }
        }

        protected override void OnDrawOptimizedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, bool foldoutEnabled, float foldoutHeight, List<RavenParameter> parameters) {
            SetupOptimizedGui();
            EditorGUIUtility.AddCursorRect(m_HandleTouchRect, MouseCursor.Pan);
            bool hasConditions = m_ContinuousEvent.Conditions.Count > 0;
            EditorGUI.LabelField(m_TopPartRect, Name, m_Selected ? (hasConditions ? NodeConditionSelectedStyle : NodeSelectedStyle) : (hasConditions ? NodeConditionStyle : NodeStyle));
        }

        protected void SetupOptimizedGui() {
            m_TopPartRect = new Rect(m_EventRect.x, m_EventRect.y, m_EventRect.width, c_TopBarHeight);
            if (!m_ContinuousEvent.IsEventLockedAtOneFrame) {
                m_LeftHotzoneRect = new Rect(m_EventRect.x - c_HotzoneWidth * 0.5f, m_EventRect.y, c_HotzoneWidth, m_EventRect.height);
                m_RightHotzoneRect = new Rect(m_EventRect.x + m_EventRect.width - c_HotzoneWidth * 0.5f, m_EventRect.y, c_HotzoneWidth, m_EventRect.height);
                EditorGUIUtility.AddCursorRect(m_LeftHotzoneRect, MouseCursor.ResizeHorizontal);
                EditorGUIUtility.AddCursorRect(m_RightHotzoneRect, MouseCursor.ResizeHorizontal);
            }
        }

        protected override bool OnHandleInput(Vector2 mousePosition, TimelineData timelineData, RavenSequenceView sequenceView, Rect timelineTrackRect, int highiestEventTrackIndex) {
            if (m_InputState == EContinousEventInputState.Inactive) {
                int eventStartFrame, eventEndFrame;
                if (UnityEngine.Event.current.type == EventType.mouseDown && UnityEngine.Event.current.button == 0) {
                    if (!m_ContinuousEvent.IsEventLockedAtOneFrame && m_LeftHotzoneRect.Contains(mousePosition)) {
                        if (!m_Selected) {
                            sequenceView.DeselectAll();
                            SelectEvent(null);
                        }

                        if (!FindStartAndEndFramesForResize(EContinousEventInputState.ResizeLeft, sequenceView, out eventStartFrame, out eventEndFrame)) {
                            return false;
                        }

                        m_InputState = EContinousEventInputState.ResizeLeft;

                        for (int i = 0; i < sequenceView.EventViews.Count; ++i) {
                            var view = sequenceView.EventViews[i];
                            if (view.m_Selected) {
                                view.StartResizeMode(mousePosition, eventStartFrame, eventEndFrame, EResizeMode.Left);
                            }
                        }
                        return true;
                    } else if (!m_ContinuousEvent.IsEventLockedAtOneFrame && m_RightHotzoneRect.Contains(mousePosition)) {
                        if (!m_Selected) {
                            sequenceView.DeselectAll();
                            SelectEvent(null);
                        }

                        if (!FindStartAndEndFramesForResize(EContinousEventInputState.ResizeRight, sequenceView, out eventStartFrame, out eventEndFrame)) {
                            return false;
                        }

                        m_InputState = EContinousEventInputState.ResizeRight;

                        for (int i = 0; i < sequenceView.EventViews.Count; ++i) {
                            var view = sequenceView.EventViews[i];
                            if (view.m_Selected) {
                                view.StartResizeMode(mousePosition, eventStartFrame, eventEndFrame, EResizeMode.Right);
                            }
                        }
                        return true;
                    } else if (m_HandleTouchRect.Contains(mousePosition)) {
                        if (!m_Selected) {
                            sequenceView.DeselectAll();
                            SelectEvent(null);
                        }
                        m_InputState = EContinousEventInputState.Dragging;
                        for (int i = 0; i < sequenceView.EventViews.Count; ++i) {
                            var view = sequenceView.EventViews[i];
                            if (view.m_Selected) {
                                view.StartDraggingMode(mousePosition, timelineData);
                            }
                        }
                        return true;
                    }
                }
            } else if (m_InputState == EContinousEventInputState.ResizeLeft) {
                if (UnityEngine.Event.current.type == EventType.mouseUp && UnityEngine.Event.current.button == 0 ||
                    UnityEngine.Event.current.type == EventType.Ignore) {
                    m_InputState = EContinousEventInputState.Inactive;

                    for (int i = 0; i < sequenceView.EventViews.Count; ++i) {
                        var view = sequenceView.EventViews[i];
                        if (view.m_Selected) {
                            view.EndResizeMode();
                        }
                    }
                    return true;
                } else if (UnityEngine.Event.current.type == EventType.mouseDrag) {
                    int eventStartFrame, eventEndFrame;

                    if (!FindStartAndEndFramesForResize(m_InputState, sequenceView, out eventStartFrame, out eventEndFrame)) {
                        return false;
                    }

                    var deltaFrames = timelineData.GetFrameAtMousePosition(m_StartDragMousePosition) - timelineData.GetFrameAtMousePosition(mousePosition);
                    var newFirstEventStartFrame = eventEndFrame - m_InitialResizeDelta - deltaFrames;

                    if (newFirstEventStartFrame < eventEndFrame) {
                        RepositionSelectedEvents(sequenceView, newFirstEventStartFrame, eventEndFrame);
                    }
                    return true;
                }
            } else if (m_InputState == EContinousEventInputState.ResizeRight) {
                if (UnityEngine.Event.current.type == EventType.mouseUp && UnityEngine.Event.current.button == 0 ||
                    UnityEngine.Event.current.type == EventType.Ignore) {
                    m_InputState = EContinousEventInputState.Inactive;

                    for (int i = 0; i < sequenceView.EventViews.Count; ++i) {
                        var view = sequenceView.EventViews[i];
                        if (view.m_Selected) {
                            view.EndResizeMode();
                        }
                    }
                    return true;
                } else if (UnityEngine.Event.current.type == EventType.mouseDrag) {
                    int eventStartFrame, eventEndFrame;

                    if (!FindStartAndEndFramesForResize(m_InputState, sequenceView, out eventStartFrame, out eventEndFrame)) {
                        return false;
                    }

                    var deltaFrames = timelineData.GetFrameAtMousePosition(m_StartDragMousePosition) - timelineData.GetFrameAtMousePosition(mousePosition);
                    var newLastEventEndFrame = eventStartFrame + m_InitialResizeDelta - deltaFrames;

                    if (newLastEventEndFrame > eventStartFrame) {
                        RepositionSelectedEvents(sequenceView, eventStartFrame, newLastEventEndFrame);
                    }
                    return true;
                }
            } else if (m_InputState == EContinousEventInputState.Dragging) {
                if (UnityEngine.Event.current.type == EventType.mouseUp && UnityEngine.Event.current.button == 0 ||
                    UnityEngine.Event.current.type == EventType.Ignore) {
                    m_InputState = EContinousEventInputState.Inactive;

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
                    EvaluateDraggingForTrackIndex(mousePosition, timelineTrackRect, highiestEventTrackIndex);
                    return true;
                }
            }

            return m_InputState != EContinousEventInputState.Inactive;
        }

        protected Rect GetContentRect() {
            return new Rect(m_EventRect.x, m_EventRect.y + m_TopPartRect.height + 2f, m_EventRect.width, m_EventRect.height - m_TopPartRect.height - 2f);
        }

        private bool FindStartAndEndFramesForResize(EContinousEventInputState resizeDirection, RavenSequenceView sequenceView, out int eventStartFrame, out int eventEndFrame) {
            eventStartFrame = m_ContinuousEvent.StartFrame;
            eventEndFrame = m_ContinuousEvent.EndFrame;

            for (int i = 0; i < sequenceView.EventViews.Count; ++i) {
                var eventView = sequenceView.EventViews[i];
                if (eventView.m_Selected) {
                    if (eventView.Event.StartFrame < eventStartFrame) {
                        eventStartFrame = eventView.Event.StartFrame;
                    }
                    if (eventView.Event.EndFrame > eventEndFrame) {
                        eventEndFrame = eventView.Event.EndFrame;
                    }
                }
            }

            return true;
        }

        private void RepositionSelectedEvents(RavenSequenceView sequenceView, int startFrame, int endFrame) {
            var repositionValid = true;
            for (int i = 0; i < sequenceView.EventViews.Count; ++i) {
                var view = sequenceView.EventViews[i];
                if (view.m_Selected) {
                    if (view.TryNormalizeRepositioning(sequenceView, startFrame, endFrame) == false) {
                        repositionValid = false;
                        break;
                    }
                }
            }

            if (repositionValid) {
                repositionValid = !sequenceView.CheckFrameOverlapOfSelectedEvents();
            }

            if (repositionValid) {
                for (int i = 0; i < sequenceView.EventViews.Count; ++i) {
                    var view = sequenceView.EventViews[i];
                    if (view.m_Selected) {
                        view.ApplyNormalizedRepositioning();
                    }
                }
            }
        }
    }
}