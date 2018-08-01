using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Outfit7.Util;
using System.Reflection;
using System;
using System.Linq;
using Outfit7.Logic.StateMachineInternal;
using Outfit7.Logic.Util;

namespace Outfit7.Sequencer {
    public class SequencerTrackGroupView {
        protected enum CollapsedEventInputState {
            INACTIVE,
            DRAGGING,
            RESIZE_LEFT,
            RESIZE_RIGHT
        }

        public List<SequencerTrackView> TrackViews = new List<SequencerTrackView>();
        private SequencerSequenceView ParentSequenceView;
        private SequencerTrackGroup TrackGroup = null;
        protected Rect SidebarTrackRect = new Rect();
        protected Rect AddTrackButton = new Rect();
        protected GUIStyle Button = (GUIStyle) "ButtonMid";
        private Color LineColor = new Color(0.33f, 0.33f, 0.33f);
        protected IEnumerable<Type> TypesEnumerator;

        //custom foldout
        protected Rect FoldoutButtonRect = new Rect();
        private static Texture2D CollapsedTexture = EditorGUIUtility.FindTexture("d_Animation.Play");
        private static Texture2D OptimizedTexture = EditorGUIUtility.FindTexture("d_UnityEditor.SceneHierarchyWindow");
        private static Texture2D ExtendedTexture = EditorGUIUtility.FindTexture("d_AnimationWrapModeMenu");

        //For optimized view
        protected List<BasePropertyView> PropertyViews;
        protected List<SequencerEventView> EventViews;
        protected Rect WindowSize;

        //For collapsed view
        private CollapsedEventInputState InputState = CollapsedEventInputState.INACTIVE;
        private float CollapsedEventStartTime = 0f;
        private float CollapsedEventEndTime = 1f;
        protected Rect CollapsedEventRect = new Rect();
        private Rect LeftCollapsedHotzoneRect = new Rect();
        private Rect RightCollapsedHotzoneRect = new Rect();
        private float HotzoneWidth = 6f;
        private Vector2 StartDragMousePosition = new Vector2();
        protected Vector2 StartDragOffset = new Vector2();

        public void Init(object track) {
            OnInit(track);
            GetTypesEnumerator();
        }

        public virtual string GetName() {
            return "TrackGroup";
        }

        public SequencerTrackGroup GetTrackGroup() {
            return TrackGroup;
        }

        public SequencerSequenceView GetSequencerView() {
            return ParentSequenceView;
        }

        public List<SequencerEventView> GetAllEvents() {
            List<SequencerEventView> allEvents = new List<SequencerEventView>();
            foreach (SequencerTrackView trackView in TrackViews) {
                foreach (SequencerEventView eventView in trackView.EventViews) {
                    allEvents.Add(eventView);
                }
            }
            return allEvents;
        }

        public virtual void OnInit(object track) {
            TrackGroup = (SequencerTrackGroup) track;
            CreateAndInitTracks();
        }

        public void DestroyTrackGroup() {
            OnDestroyTrack();
            foreach (SequencerTrackView trackView in TrackViews) {
                trackView.DestroyTrack();
            }
            Undo.DestroyObjectImmediate(TrackGroup);
        }

        public virtual void OnDestroyTrack() {
        }

        protected virtual void GetTypesEnumerator() {
            Assembly assembly = typeof(SequencerTrackView).Assembly;
            TypesEnumerator = assembly.GetTypes().Where(t => typeof(SequencerTrackView).IsAssignableFrom(t));
        }

        public void CreateAndInitTracks() {
            for (int i = 0; i < TrackGroup.Tracks.Count; i++) {
                Type t = TrackGroup.Tracks[i].GetType();
                MethodInfo method = typeof(SequencerTrackGroupView).GetMethod("AddTrack");
                method = method.MakeGenericMethod(t);
                object[] parametersArray = new object[] { TrackGroup.Tracks[i] };
                method.Invoke(this, parametersArray);
            }
        }

        private void FillPropertyViewsForOptimizedView() {
            if (PropertyViews == null)
                PropertyViews = new List<BasePropertyView>();
            if (EventViews == null)
                EventViews = new List<SequencerEventView>();
            PropertyViews.Clear();
            EventViews.Clear();
            for (int i = 0; i < TrackViews.Count; i++) {
                for (int e = 0; e < TrackViews[i].EventViews.Count; e++) {
                    EventViews.Add(TrackViews[i].EventViews[e]);
                }
                SequencerCurveTrackView curveTrackView = TrackViews[i] as SequencerCurveTrackView;
                if (curveTrackView == null)
                    continue;
                for (int j = 0; j < curveTrackView.PropertyViews.Count; j++) {
                    PropertyViews.Add(curveTrackView.PropertyViews[j]);
                }
            }
        }

        public float DrawTimelineGui(Rect windowSize, float startHeight, float splitViewWidth, TimelineData timelineData, List<Parameter> parameters) {
            RefreshAllTracks();
            float height = 0;
            height += 20f;
            WindowSize = windowSize;

            if (TrackGroup.TrackGroupMode == SequencerTrackGroupMode.EXTENDED) {
                for (int i = 0; i < TrackViews.Count; i++) {
                    height += TrackViews[i].DrawTimelineGui(windowSize, startHeight + height, splitViewWidth, timelineData, parameters);
                }
            } else if (TrackGroup.TrackGroupMode == SequencerTrackGroupMode.OPTIMIZED) {
                //count all properties
                FillPropertyViewsForOptimizedView();

                SequencerFoldoutData foldoutData = new SequencerFoldoutData();
                foldoutData.Enabled = false;

                foreach (SequencerEventView eventView in EventViews) {
                    SequencerPropertyEventView propertyEventView = eventView as SequencerPropertyEventView;
                    SequencerSetPropertyEventView setPropertyEventView = eventView as SequencerSetPropertyEventView;
                    if (propertyEventView != null || setPropertyEventView != null)
                        continue;
                    eventView.DrawGui(windowSize, startHeight, splitViewWidth, timelineData, foldoutData, parameters, true);
                }

                for (int p = 0; p < PropertyViews.Count; p++) {
                    foreach (SequencerEventView eventView in EventViews) {
                        SequencerPropertyEventView propertyEventView = eventView as SequencerPropertyEventView;
                        if (propertyEventView == null)
                            continue;
                        if (propertyEventView.GetProperty() == PropertyViews[p].GetProperty())
                            propertyEventView.DrawGui(windowSize, startHeight + height, splitViewWidth, timelineData, foldoutData, parameters, true);
                    }
                    foreach (SequencerEventView eventView in EventViews) {
                        SequencerSetPropertyEventView setPropertyEventView = eventView as SequencerSetPropertyEventView;
                        if (setPropertyEventView == null)
                            continue;
                        if (setPropertyEventView.GetProperty() == PropertyViews[p].GetProperty())
                            setPropertyEventView.DrawGui(windowSize, startHeight + height, splitViewWidth, timelineData, foldoutData, parameters, true);
                    }
                    height += 20f;
                }
            } else if (TrackGroup.TrackGroupMode == SequencerTrackGroupMode.COLLAPSED) {
                FillPropertyViewsForOptimizedView();
                if (EventViews.Count != 0) {
                    SequencerFoldoutData foldoutData = new SequencerFoldoutData();
                    foldoutData.Enabled = false;
                    CollapsedEventStartTime = Mathf.Infinity;
                    CollapsedEventEndTime = Mathf.NegativeInfinity;
                    foreach (SequencerEventView eventView in EventViews) {
                        eventView.OnDefineEventRect(windowSize, startHeight, splitViewWidth, timelineData, foldoutData, false);
                        if (eventView.GetEvent().StartTime < CollapsedEventStartTime)
                            CollapsedEventStartTime = eventView.GetEvent().StartTime;
                        if (eventView.GetEvent().GetEndPoint() > CollapsedEventEndTime)
                            CollapsedEventEndTime = eventView.GetEvent().GetEndPoint();
                    }
                    CollapsedEventRect = SequencerContinuousEventView.MimicEventDraw(CollapsedEventStartTime, CollapsedEventEndTime, timelineData, startHeight);
                    foreach (SequencerEventView eventView in EventViews) {
                        Rect eventRect = new Rect(timelineData.Rect.x + timelineData.LenghtOfASecond * (eventView.GetEvent().StartTime + timelineData.Offset),
                                             startHeight,
                                             Mathf.Max(2, timelineData.LenghtOfASecond * (eventView.GetEvent().GetEndPoint() - eventView.GetEvent().StartTime)),
                                             20f);
                        EditorGUI.DrawRect(eventRect, new Color(0f, 0f, 1f, 0.2f));
                    }
                    EditorGUI.LabelField(CollapsedEventRect, GetName());
                    LeftCollapsedHotzoneRect = new Rect(CollapsedEventRect.x - HotzoneWidth * 0.5f, CollapsedEventRect.y, HotzoneWidth, CollapsedEventRect.height);
                    RightCollapsedHotzoneRect = new Rect(CollapsedEventRect.x + CollapsedEventRect.width - HotzoneWidth * 0.5f, CollapsedEventRect.y, HotzoneWidth, CollapsedEventRect.height);
                    EditorGUIUtility.AddCursorRect(LeftCollapsedHotzoneRect, MouseCursor.ResizeHorizontal);
                    EditorGUIUtility.AddCursorRect(RightCollapsedHotzoneRect, MouseCursor.ResizeHorizontal);
                }
            }

            DrawTimelineLines(windowSize, startHeight, splitViewWidth, height, timelineData);

            return height;
        }

        public float DrawSidebarGui(Rect windowSize, float startHeight, float splitViewWidth, TimelineData timelineData, List<Parameter> parameters) {
            //TrackGroup.Foldout = EditorGUI.Foldout(new Rect(0, startHeight, 20, 20), TrackGroup.Foldout, GetName());
                
            FoldoutButtonRect = new Rect(0, startHeight, 20, 20);
            Texture2D ViewModeTex = ExtendedTexture;
            switch (TrackGroup.TrackGroupMode) {
                case SequencerTrackGroupMode.COLLAPSED:
                    ViewModeTex = CollapsedTexture;
                    break;
                case SequencerTrackGroupMode.OPTIMIZED:
                    ViewModeTex = OptimizedTexture;
                    break;
            }
            GUI.Label(FoldoutButtonRect, ViewModeTex);
            SidebarTrackRect = new Rect(0, startHeight, splitViewWidth, 20);
            float height = 0;
            height += 20f;
            AddTrackButton = new Rect(splitViewWidth - 30, startHeight, 30, 20);
            OnDrawGui(windowSize, startHeight, splitViewWidth, timelineData, parameters);

            if (TrackGroup.TrackGroupMode == SequencerTrackGroupMode.EXTENDED) {
                for (int i = 0; i < TrackViews.Count; i++) {
                    height += TrackViews[i].DrawSidebarGui(windowSize, startHeight + height, splitViewWidth, timelineData, parameters);
                }
            } else if (TrackGroup.TrackGroupMode == SequencerTrackGroupMode.OPTIMIZED) {
                for (int i = 0; i < TrackViews.Count; i++) {
                    //TrackViews[i].CalculateSidebarTrackRect(startHeight, splitViewWidth, true);
                    //TrackViews[i].CalculateTimelineTrackRect(startHeight, splitViewWidth, windowSize, true);
                }
                for (int p = 0; p < PropertyViews.Count; p++) {
                    PropertyViews[p].DrawGui(35f, startHeight + height, parameters);
                    height += 20f;
                }
            }
            return height;
        }

        public virtual void OnDrawGui(Rect windowSize, float startHeight, float splitViewWidth, TimelineData timelineData, List<Parameter> parameters) {
            if (GUI.Button(AddTrackButton, "+", Button))
                DrawEventsContextMenu();
        }

        private void DrawTimelineLines(Rect windowSize, float startHeight, float splitViewWidth, float height, TimelineData timelineData) {
            GUIUtil.DrawFatLine(new Vector2(splitViewWidth + 1, startHeight), new Vector2(windowSize.width - 1, startHeight), 4, LineColor);
            GUIUtil.DrawLine(new Vector2(splitViewWidth + 1, startHeight + height), new Vector2(windowSize.width - 1, startHeight + height), LineColor);
        }

        protected virtual void DrawEventsContextMenu(float location = 0f) {
            GenericMenu menu = new GenericMenu();
            foreach (System.Type t in TypesEnumerator) {
                menu.AddItem(new GUIContent(t.ToString().Split('.').Last()), false, AddTrackCallback, t);
            }
            menu.ShowAsContext();
        }

        public void SelectEvents(Rect selectionRect) {
            for (int i = 0; i < TrackViews.Count; i++) {
                TrackViews[i].SelectEvents(selectionRect);
            }
        }

        public bool HandleInput(TimelineData timelineData, SequencerSequenceView sequenceView) {
            ParentSequenceView = sequenceView;
            if (FoldoutButtonRect.Contains(SequencerSequenceView.GetCurrentMousePosition()) &&
                UnityEngine.Event.current.type == EventType.mouseDown &&
                UnityEngine.Event.current.button == 0) {
                switch (TrackGroup.TrackGroupMode) {
                    case SequencerTrackGroupMode.EXTENDED:
                        if (UnityEngine.Event.current.alt) {
                            foreach (SequencerTrackGroupView trackGroupView in sequenceView.TrackGroupViews)
                                trackGroupView.GetTrackGroup().TrackGroupMode = SequencerTrackGroupMode.COLLAPSED;
                        } else
                            TrackGroup.TrackGroupMode = SequencerTrackGroupMode.COLLAPSED;
                        break;
                    case SequencerTrackGroupMode.COLLAPSED:
                        if (UnityEngine.Event.current.alt) { 
                            foreach (SequencerTrackGroupView trackGroupView in sequenceView.TrackGroupViews)
                                trackGroupView.GetTrackGroup().TrackGroupMode = SequencerTrackGroupMode.OPTIMIZED;
                        } else
                            TrackGroup.TrackGroupMode = SequencerTrackGroupMode.OPTIMIZED;
                        break;
                    case SequencerTrackGroupMode.OPTIMIZED:
                        if (UnityEngine.Event.current.alt) {
                            foreach (SequencerTrackGroupView trackGroupView in sequenceView.TrackGroupViews)
                                trackGroupView.GetTrackGroup().TrackGroupMode = SequencerTrackGroupMode.EXTENDED;
                        } else
                            TrackGroup.TrackGroupMode = SequencerTrackGroupMode.EXTENDED;
                        break;
                }
                return true;
            }
            if (SidebarTrackRect.Contains(SequencerSequenceView.GetCurrentMousePosition()) &&
                UnityEngine.Event.current.type == EventType.mouseUp &&
                UnityEngine.Event.current.button == 1) {
                DrawTrackGroupContextMenu();
                return true;
            }
            if (SidebarTrackRect.Contains(SequencerSequenceView.GetCurrentMousePosition()) &&
                UnityEngine.Event.current.type == EventType.KeyDown) {
                if (UnityEngine.Event.current.keyCode == KeyCode.DownArrow)
                    ParentSequenceView.MoveTrackGroupDown(this);
                else if (UnityEngine.Event.current.keyCode == KeyCode.UpArrow)
                    ParentSequenceView.MoveTrackGroupUp(this);
                return true;
            }
            if (TrackGroup.TrackGroupMode == SequencerTrackGroupMode.EXTENDED || TrackGroup.TrackGroupMode == SequencerTrackGroupMode.OPTIMIZED) {
                for (int i = 0; i < TrackViews.Count; i++) {
                    if (OnHandleInput(timelineData, sequenceView, i))
                        return true;
                }
            } else {
                //collapsed view
                if (InputState == CollapsedEventInputState.INACTIVE) {
                    if (UnityEngine.Event.current.type == EventType.mouseDown && UnityEngine.Event.current.button == 0) {
                        if (LeftCollapsedHotzoneRect.Contains(SequencerSequenceView.GetCurrentMousePosition())) {
                            InputState = CollapsedEventInputState.RESIZE_LEFT;
                            StartDragMousePosition = SequencerSequenceView.GetCurrentMousePosition();
                            StartDragOffset = StartDragMousePosition - LeftCollapsedHotzoneRect.min;
                            return true;
                        } else if (RightCollapsedHotzoneRect.Contains(SequencerSequenceView.GetCurrentMousePosition())) {
                            InputState = CollapsedEventInputState.RESIZE_RIGHT;
                            StartDragMousePosition = SequencerSequenceView.GetCurrentMousePosition();
                            StartDragOffset = StartDragMousePosition - RightCollapsedHotzoneRect.min;
                            return true;
                        } else if (CollapsedEventRect.Contains(SequencerSequenceView.GetCurrentMousePosition())) {
                            InputState = CollapsedEventInputState.DRAGGING;
                            foreach (SequencerEventView eventView in GetAllEvents()) {
                                eventView.SetDraggingMode(SequencerSequenceView.GetCurrentMousePosition());
                            }
                            return true;
                        }
                    }
                } else if (InputState == CollapsedEventInputState.RESIZE_LEFT) {
                    if (UnityEngine.Event.current.type == EventType.mouseUp && UnityEngine.Event.current.button == 0) {
                        InputState = CollapsedEventInputState.INACTIVE;
                        return true;
                    } else if (UnityEngine.Event.current.type == EventType.mouseDrag) {
                        Vector2 mousePos = UnityEngine.Event.current.mousePosition - StartDragOffset;
                        float newFirstEventStart = sequenceView.Snap(timelineData.GetTimeAtMousePosition(mousePos)).Value;

                        foreach (SequencerEventView eventView in GetAllEvents()) {
                            eventView.NormalizedRepositioning(CollapsedEventStartTime, CollapsedEventEndTime, newFirstEventStart, CollapsedEventEndTime);
                        }
                        return true;
                    }

                } else if (InputState == CollapsedEventInputState.RESIZE_RIGHT) {
                    if (UnityEngine.Event.current.type == EventType.mouseUp && UnityEngine.Event.current.button == 0) {
                        InputState = CollapsedEventInputState.INACTIVE;
                        return true;
                    } else if (UnityEngine.Event.current.type == EventType.mouseDrag) {
                        Vector2 mousePos = UnityEngine.Event.current.mousePosition - StartDragOffset;
                        float newLastEventEnd = sequenceView.Snap(timelineData.GetTimeAtMousePosition(mousePos)).Value;

                        foreach (SequencerEventView eventView in GetAllEvents()) {
                            eventView.NormalizedRepositioning(CollapsedEventStartTime, CollapsedEventEndTime, CollapsedEventStartTime, newLastEventEnd);
                        }

                        return true;
                    }

                } else if (InputState == CollapsedEventInputState.DRAGGING) {
                    if (UnityEngine.Event.current.type == EventType.mouseUp && UnityEngine.Event.current.button == 0) {
                        InputState = CollapsedEventInputState.INACTIVE;
                        return true;
                    } else if (UnityEngine.Event.current.type == EventType.mouseDrag) {
                        foreach (SequencerEventView eventView in GetAllEvents()) {
                            eventView.EvaluateDragging(SequencerSequenceView.GetCurrentMousePosition(), timelineData, sequenceView);
                        }
                        return true;
                    }
                }
                return InputState != CollapsedEventInputState.INACTIVE;
            }
            return false;
        }

        public virtual bool OnHandleInput(TimelineData timelineData, SequencerSequenceView sequenceView, int index) {
            return TrackViews[index].HandleInput(timelineData, sequenceView, null, TrackGroup.TrackGroupMode == SequencerTrackGroupMode.OPTIMIZED);
        }

        public void RecordingStart() {
            foreach (SequencerTrackView trackView in TrackViews) {
                trackView.RecordingStart();
            }
        }

        public void RecordingStop() {
            foreach (SequencerTrackView trackView in TrackViews) {
                trackView.RecordingStop();
            }
        }

        public void UpdateWhileRecording(float currentTime) {
            foreach (SequencerTrackView trackView in TrackViews) {
                trackView.UpdateWhileRecording(currentTime);
            }
        }

        protected virtual void DrawTrackGroupContextMenu() {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Remove Track Group/Are you sure?"), false, ParentSequenceView.RemoveTrackGroup, this);
            menu.AddItem(new GUIContent("Move Up"), false, ParentSequenceView.MoveTrackGroupUp, this);
            menu.AddItem(new GUIContent("Move Down"), false, ParentSequenceView.MoveTrackGroupDown, this);
            menu.ShowAsContext();
        }

        public void AddTrackCallback(object o) {
            Type t = (System.Type) o;
            Assembly assembly = typeof(SequencerEvent).Assembly;
            Type trackType = Type.GetType("Outfit7.Sequencer." + t.Name.Replace("View", "") + ", " + assembly.GetName(), true);
            MethodInfo method = typeof(SequencerTrackGroupView).GetMethod("AddTrack");
            method = method.MakeGenericMethod(trackType);
            object[] parametersArray = new object[] { null };
            method.Invoke(this, parametersArray);
        }

        public bool AddTrack<T>(object ctrl) where T : Component {
            //if controller comes in at init, it just casts it and creates View for it
            //if function is called to add new, it creates controller first, and then view afterwards
            Undo.RecordObject(TrackGroup, "AddTrack");
            T controller;
            if (ctrl == null) {
                controller = (T) Undo.AddComponent<T>(TrackGroup.gameObject);
                SequencerTrack trackController = controller as SequencerTrack;
                trackController.Init();
                TrackGroup.Tracks.Add(trackController);
                EditorUtility.SetDirty(TrackGroup);
            } else
                controller = (T) ctrl;

            controller.hideFlags = HideFlags.HideInInspector;
            Type viewType = Type.GetType("Outfit7.Sequencer." + controller.GetType().Name + "View", true);
            SequencerTrackView instance = Activator.CreateInstance(viewType) as SequencerTrackView;
            instance.Init(controller);
            instance.Parent = this;
            TrackViews.Add(instance);
            return true;
        }

        public virtual void RefreshAllTracks() {
            foreach (SequencerTrackView trackView in TrackViews) {
                trackView.RefreshAllEvents(new List<GameObject>());
            }
        }

        public void RemoveTrack(object obj) {
            SequencerTrackView trackView = (SequencerTrackView) obj;
            TrackGroup.Tracks.Remove(trackView.GetTrack());
            trackView.DestroyTrack();
            TrackViews.Remove(trackView);
        }
    }
}