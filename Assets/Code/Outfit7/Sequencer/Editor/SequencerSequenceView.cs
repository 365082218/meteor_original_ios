using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Outfit7.Util;
using System;
using System.Reflection;
using System.Linq;
using Outfit7.Logic.Util;
using Outfit7.Logic.StateMachineInternal;
using UnityEditor.SceneManagement;

namespace Outfit7.Sequencer {

    public class SequencerSequenceAttribute : System.Attribute {
    }

    public class SequencerSequenceView {
        protected enum SequenceInputState {
            INACTIVE,
            DRAGGING_TIMELINE,
            DRAGGING_CURRENTTIME,
            DRAGGING_SELECTION,
            DRAGGING_TIMELINESCALE,
            DRAGGING_DURATION,
        }

        public struct SnapInfo {
            public float Value;
            public bool SnappedFromEnd;
            public SequencerEvent Event;
            public bool SnappedToEnd;
        }

        private SequencerSequence Sequence = null;
        public List<SequencerTrackGroupView> TrackGroupViews = new List<SequencerTrackGroupView>();
        private Rect SeqNameButton = new Rect(3, 0, 65, 20);
        private Rect PlayButton = new Rect(70, 0, 35, 20);
        Texture2D PlayTex = EditorGUIUtility.FindTexture("d_Animation.Play");
        private Rect RecordButton = new Rect(105, 0, 35, 20);
        Texture2D RecordTex = EditorGUIUtility.FindTexture("d_Animation.Record");
        private Rect AddTrackButton = new Rect(125, 0, 75, 15);
        private Rect CurrentTimeRect = new Rect(20, 0, 40, 30);
        private Rect DurationTimeRect = new Rect(20, 0, 40, 30);
        private Rect TimelineRect = new Rect();
        private Rect ScrollRect = new Rect(0, 0, 500, 100);
        private static bool Snapping = false;
        private static bool AltSnapping = false;
        private TimelineData TimeLineData = null;
        private Vector2 MousePositionStart = new Vector2();
        private Vector2 ScaleMousePositionStart = new Vector2();
        private float ScaleMouseValueStart = -1f;
        private static Vector2 ScrollPosition = Vector2.zero;
        private SequenceInputState InputState = SequenceInputState.INACTIVE;
        private Color TickColor = new Color(0.33f, 0.33f, 0.33f);
        private Color TransparentTickColor = new Color(0.156f, 0.156f, 0.156f);
        private float UpdateTime = 0f;
        private float LastUpdateTime = 0f;
        private float TimelineLenght;
        private static float TopHeight = 15f;
        //default offset is 0.5s
        private int MajorFrameEvery = 30;
        GUIStyle OffTimelineBG = (GUIStyle) "AnimationKeyFrameBackground";
        GUIStyle LeftPartBG = (GUIStyle) "AnimationKeyframeBackground";
        public IEnumerable<Type> TypesEnumerator;

        private Vector2 InsertTimeLocation;
        private bool ShowInsertTime = false;
        private Rect InsertTimeRect;
        private InsertTimePopup InsertTimeWindow = null;

        public SequencerSequenceView(SequencerSequence sequence) {
            Sequence = sequence;
            ErrorCheck();
            TimeLineData = new TimelineData();
            for (int i = 0; i < Sequence.TrackGroups.Count; i++) {
                Type t = Sequence.TrackGroups[i].GetType();
                MethodInfo method = typeof(SequencerSequenceView).GetMethod("AddTrackGroup");
                method = method.MakeGenericMethod(t);
                object[] parametersArray = new object[] { Sequence.TrackGroups[i] };
                method.Invoke(this, parametersArray);
            }

            bool mainTrackFound = false;
            for (int i = 0; i < Sequence.TrackGroups.Count; i++) {
                if (Sequence.TrackGroups[i] is SequencerMainTrackGroup) {
                    mainTrackFound = true;
                    break;
                }
            }
            if (!mainTrackFound) {
                AddTrackGroup<SequencerMainTrackGroup>(null);
                TrackGroupViews[TrackGroupViews.Count - 1].AddTrack<SequencerTrack>(null);
            }


            GetTypesEnumerator();

            for (int i = 0; i < Sequence.TrackGroups.Count; i++) {
                Sequence.TrackGroups[i].LiveInit(Sequence);
            }
        }

        private void ErrorCheck() {
            //track groups
            /*SequencerTrackGroup[] trackGroups = Sequence.gameObject.GetComponentsInChildren<SequencerTrackGroup>();
            List<SequencerTrackGroup> UsedTrackGroups;
            int anyNullTrackGroups = 0;
            for (int i = 0; i < Sequence.TrackGroups.Count; i++) {
                if (Sequence.TrackGroups[i] != null) {
                    UsedTrackGroups.Add(Sequence.TrackGroups[i]);
                } else {
                    anyNullTrackGroups++;
                }
            }
            if (trackGroups.Length != UsedTrackGroups.Count) {
                //constructive fix
                List<SequencerTrackGroup> UnusedTrackGroups = new List<SequencerTrackGroup>();
                foreach (SequencerTrackGroup stg in trackGroups) {
                    if (!UsedTrackGroups.Contains(stg)) {
                        UnusedTrackGroups.Add(stg);
                    }
                }

                if (anyNullTrackGroups == UnusedTrackGroups.Count) {
                    int count = 0;
                    for (int i = 0; i < Sequence.TrackGroups.Count; i++) {
                        if (Sequence.TrackGroups[i] == null) {
                            Sequence.TrackGroups[i] = UnusedTrackGroups[count];
                            count++;
                        }
                    }
                } else {
                    //destructive fix
                    Sequence.TrackGroups = UsedTrackGroups;
                    for (int i = 0; i < UnusedTrackGroups.Count; i++) {
                        Component.DestroyImmediate(UnusedTrackGroups, true);
                    }
                }
            }*/

            //groups

            //events

        }

        public void DrawGui(Rect windowSize, float startHeight, float splitViewPosition, EditorWindow window) {
            EditorGUI.BeginDisabledGroup(ShowInsertTime);

            GUIStyle SeqButton = (GUIStyle) "toolbarbutton";
            GUIStyle Button = (GUIStyle) "OL Titlemid";
            if (GUI.Button(AddTrackButton, "Group+", Button))
                DrawEventsContextMenu();
            bool wasPlayingBefore = Sequence.Playing;
            if (GUI.Button(SeqNameButton, Sequence.name.Substring(4, Math.Min(8, Sequence.name.Length - 4)), SeqButton)) {
                Selection.activeGameObject = Sequence.gameObject;
            }
            Sequence.Playing = GUI.Toggle(PlayButton, Sequence.Playing, PlayTex);
            if (!wasPlayingBefore && Sequence.Playing) {
                for (int i = 0; i < Sequence.TrackGroups.Count; i++) {
                    Sequence.TrackGroups[i].LiveInit(Sequence);
                }

                if (Sequence.GetCurrentTime() >= Sequence.Duration)
                    Sequence.SetCurrentTime(0f);
            }

            Sequence.Recording = GUI.Toggle(RecordButton, Sequence.Recording, RecordTex);

            TimeLineData.Scale = EditorGUI.Slider(new Rect(windowSize.width - 100f, 0, 50f, 18f), "Scale", TimeLineData.Scale, 0.1f, 5.0f);

            DrawCurrentTime(windowSize, startHeight, splitViewPosition);
            CalculateDurationRect(windowSize, startHeight, splitViewPosition);

            ScrollPosition = GUI.BeginScrollView(new Rect(0, TopHeight, windowSize.width, windowSize.height - TopHeight), ScrollPosition, ScrollRect, false, false);

            DrawTimeline(windowSize, startHeight, splitViewPosition);
            float height = 0;
            for (int i = 0; i < TrackGroupViews.Count; i++) {
                height += TrackGroupViews[i].DrawTimelineGui(windowSize, startHeight + height, splitViewPosition, TimeLineData, Sequence.Parameters);
            }
            EditorGUI.LabelField(new Rect(0, 0, splitViewPosition, ScrollRect.height), "", LeftPartBG);
            height = 0;
            for (int i = 0; i < TrackGroupViews.Count; i++) {
                height += TrackGroupViews[i].DrawSidebarGui(windowSize, startHeight + height, splitViewPosition, TimeLineData, Sequence.Parameters);
            }
            if (InputState == SequenceInputState.DRAGGING_SELECTION) {
                EditorGUI.DrawRect(new Rect(MousePositionStart, UnityEngine.Event.current.mousePosition - MousePositionStart), new Color(1, 1, 1, 0.2f));
            }
            GUI.EndScrollView();
            ScrollRect.width = windowSize.width - 15f;
            ScrollRect.height = height;

            EditorGUI.EndDisabledGroup();

            if (ShowInsertTime && InsertTimeWindow == null) {
                InsertTimeWindow = new InsertTimePopup(InsertTimeImpl, InsertTimeWindowOnClose);
                PopupWindow.Show(InsertTimeRect, InsertTimeWindow);
            }
        }

        protected virtual void GetTypesEnumerator() {
            Assembly assembly = typeof(SequencerTrackGroupView).Assembly;
            TypesEnumerator = assembly.GetTypes().
                Where(t => typeof(SequencerTrackGroupView).IsAssignableFrom(t)).
                Where(t => Attribute.IsDefined(t, typeof(SequencerSequenceAttribute)));
        }

        protected void DrawEventsContextMenu() {
            //FOR NOW THAT WE HAVE ONLY ONE GROUP TYPE TO ADD
            AddEventCallback(typeof(SequencerActorTrackGroupView));

            /*GenericMenu menu = new GenericMenu();
            foreach (System.Type t in TypesEnumerator) {
                menu.AddItem(new GUIContent(t.ToString().Split('.').Last()), false, AddEventCallback, t);
            }
            menu.ShowAsContext();*/
        }

        public void AddEventCallback(object o) {
            Type t = (System.Type) o;

            Assembly assembly = typeof(SequencerSequence).Assembly;
            Type eType = Type.GetType("Outfit7.Sequencer." + t.Name.Replace("View", "") + ", " + assembly.GetName(), true);
            MethodInfo method = typeof(SequencerSequenceView).GetMethod("AddTrackGroup");
            method = method.MakeGenericMethod(eType);
            object[] parametersArray = new object[] { null };
            method.Invoke(this, parametersArray);
        }

        private void DrawTimeline(Rect windowSize, float startHeight, float splitViewPosition) {
            TimelineLenght = windowSize.width - splitViewPosition;
            TimelineRect = new Rect(splitViewPosition, 20f, TimelineLenght, ScrollRect.height - 20f);
            //TimeLineData.LenghtOfASecond = (TimelineLenght - 15) / (Sequence.Duration / TimeLineData.Scale);
            TimeLineData.LenghtOfASecond = TimeLineData.Scale * 200f;
            /*if (TimeLineData.LenghtOfASecond / 30f > windowSize.width)
                return;*/
            if (Sequence.Duration <= 0)
                return;
            TimeLineData.Rect = new Rect(splitViewPosition, 0, windowSize.width - splitViewPosition, ScrollRect.height);

            if (TimeLineData.GetTimeAtMousePosition(TimeLineData.Rect.min) < 0) {
                EditorGUI.LabelField(new Rect(TimeLineData.Rect.min, new Vector2(TimeLineData.LenghtOfASecond * TimeLineData.Offset, TimeLineData.Rect.height)), "", OffTimelineBG);
            }

            float timeAtEnd = TimeLineData.GetTimeAtMousePosition(TimeLineData.Rect.max);
            float start = (Sequence.Duration + TimeLineData.Offset) * TimeLineData.LenghtOfASecond;
            if (timeAtEnd > Sequence.Duration) {
                EditorGUI.LabelField(new Rect(TimeLineData.Rect.min.x + start, TimeLineData.Rect.min.y, TimeLineData.Rect.width - start, TimeLineData.Rect.height), "", OffTimelineBG);
            }

            float currentPosition = splitViewPosition + TimeLineData.LenghtOfASecond * TimeLineData.Offset;
            int frameIndex = 0;
            //draw on-second lines
            while (currentPosition <= TimeLineData.Rect.min.x + start) {
                float width = TimeLineData.LenghtOfASecond / 30f; //every frame tick
                bool isMajorLine = (frameIndex % MajorFrameEvery == 0);
                if (currentPosition > splitViewPosition)
                    GUIUtil.DrawFatLine(new Vector2(currentPosition, isMajorLine ? 0 : 0), new Vector2(currentPosition, ScrollRect.height), isMajorLine ? 4 : 2, isMajorLine ? TickColor : Color.Lerp(TransparentTickColor, TickColor, Mathf.InverseLerp(2, 20, width)));
                currentPosition += width;
                frameIndex++;
            }
            //draw half-second ticks
            /*currentPosition = splitViewPosition + TimeLineData.LenghtOfASecond * TimeLineData.Offset + TimeLineData.LenghtOfASecond * 0.5f;
            while (currentPosition < windowSize.width) {
                if (currentPosition > splitViewPosition)
                    GUIUtils.DrawFatLine(new Vector2(currentPosition, 12), new Vector2(currentPosition, 16), 2, TickColor);
                currentPosition += TimeLineData.LenghtOfASecond;
            }*/
        }

        private void DrawCurrentTime(Rect windowSize, float startHeight, float splitViewPosition) {
            float timePosition = splitViewPosition + TimeLineData.LenghtOfASecond * (TimeLineData.Offset + Sequence.GetCurrentTime());
            if (timePosition > splitViewPosition) {
                GUIUtil.DrawFatLine(new Vector2(timePosition, 0), new Vector2(timePosition, windowSize.height), 2, Color.red);
                CurrentTimeRect = new Rect(timePosition - 5f, 0, 10, 20);
                EditorGUIUtility.AddCursorRect(CurrentTimeRect, MouseCursor.ResizeHorizontal);
            } else {
                CurrentTimeRect = new Rect(0, 0, 0, 0);
            }
        }

        private void CalculateDurationRect(Rect windowSize, float startHeight, float splitViewPosition) {
            float timePosition = splitViewPosition + TimeLineData.LenghtOfASecond * (TimeLineData.Offset + Sequence.Duration);
            if (timePosition > splitViewPosition) {
                DurationTimeRect = new Rect(timePosition - 5f, 0, 10, 20);
                EditorGUIUtility.AddCursorRect(DurationTimeRect, MouseCursor.ResizeHorizontal);
            } else {
                DurationTimeRect = new Rect(0, 0, 0, 0);
            }
        }

        public void SelectEvents(Rect selectionRect) {
            for (int i = 0; i < TrackGroupViews.Count; i++) {
                TrackGroupViews[i].SelectEvents(selectionRect);
            }
        }

        public bool HandleInput() {
            Snapping = UnityEngine.Event.current.shift;
            AltSnapping = UnityEngine.Event.current.alt;
            if (!Application.isPlaying) {
                if ((Sequence.Recording || Sequence.Playing) && !AnimationMode.InAnimationMode()) {
                    Sequence.ForceSplitUpdate = false;
                    Sequence.StopProcessing = false;
                    AnimationMode.StartAnimationMode();
                    StartRecording();
                    Sequence.CustomUpdate(0.000001f, true, 0, Sequence.Duration, Sequence.Loop);
                } else if (!Sequence.Recording && !Sequence.Playing && AnimationMode.InAnimationMode()) {
                    AnimationMode.StopAnimationMode();
                    EndRecording();
                }
            }

            if (EventType.KeyDown == UnityEngine.Event.current.type && KeyCode.Space == UnityEngine.Event.current.keyCode) {
                Sequence.Playing = !Sequence.Playing;
                return true;
            }

            if (!Sequence.Playing)
                Sequence.PreviewEvent = null;

            //timeline scaling
            if (UnityEngine.Event.current.type == EventType.ScrollWheel) {
                MousePositionStart = GetCurrentMousePosition();
                TimeLineData.Scale += UnityEngine.Event.current.delta.y / (20f);
                if (TimeLineData.Scale <= 0)
                    TimeLineData.Scale = 0.1f;
                TimeLineData.LenghtOfASecond = TimeLineData.Scale * 200f;

                TimeLineData.Offset += TimeLineData.GetTimeAtMousePosition(ScaleMousePositionStart) - ScaleMouseValueStart;
                return true;
            }
            if (InputState == SequenceInputState.DRAGGING_TIMELINE && (UnityEngine.Event.current.type == EventType.mouseDrag) && UnityEngine.Event.current.button == 2) {
                float mouseDelta = UnityEngine.Event.current.mousePosition.x - MousePositionStart.x;
                MousePositionStart = GetCurrentMousePosition();
                TimeLineData.Offset += mouseDelta / TimeLineData.LenghtOfASecond;
                return true;
            } else if (InputState == SequenceInputState.DRAGGING_TIMELINESCALE && (UnityEngine.Event.current.type == EventType.mouseDrag) && UnityEngine.Event.current.button == 2) {
                float mouseDelta = UnityEngine.Event.current.mousePosition.x - MousePositionStart.x;
                MousePositionStart = GetCurrentMousePosition();

                TimeLineData.Scale += mouseDelta / (250f);
                if (TimeLineData.Scale <= 0)
                    TimeLineData.Scale = 0.1f;
                TimeLineData.LenghtOfASecond = TimeLineData.Scale * 200f;

                TimeLineData.Offset += TimeLineData.GetTimeAtMousePosition(ScaleMousePositionStart) - ScaleMouseValueStart;

                return true;
            } else if (InputState == SequenceInputState.DRAGGING_CURRENTTIME && (UnityEngine.Event.current.type == EventType.mouseDrag) && UnityEngine.Event.current.button == 0) {

                float position = Snap(TimeLineData.GetTimeAtMousePosition(UnityEngine.Event.current.mousePosition), null).Value;
                Sequence.SetCurrentTime(Mathf.Clamp(position, 0, Sequence.Duration));
                return true;

            } else if (InputState == SequenceInputState.DRAGGING_DURATION && (UnityEngine.Event.current.type == EventType.mouseDrag) && UnityEngine.Event.current.button == 0) {
                float position = Snap(TimeLineData.GetTimeAtMousePosition(UnityEngine.Event.current.mousePosition), null).Value;
                Sequence.Duration = Mathf.Clamp(position, 0, 100);
                return true;
            }

            for (int i = 0; i < TrackGroupViews.Count; i++) {
                if (TrackGroupViews[i].HandleInput(TimeLineData, this)) {
                    return true;
                }
            }

            if (InputState == SequenceInputState.DRAGGING_CURRENTTIME && UnityEngine.Event.current.type == EventType.mouseUp) {
                InputState = SequenceInputState.INACTIVE;
                return true;
            } else if (InputState == SequenceInputState.DRAGGING_TIMELINE && UnityEngine.Event.current.type == EventType.mouseUp) {
                InputState = SequenceInputState.INACTIVE;
                return true;
            } else if (InputState == SequenceInputState.DRAGGING_TIMELINESCALE && UnityEngine.Event.current.type == EventType.mouseUp) {
                InputState = SequenceInputState.INACTIVE;
                return true;
            } else if (InputState == SequenceInputState.DRAGGING_DURATION && UnityEngine.Event.current.type == EventType.mouseUp) {
                InputState = SequenceInputState.INACTIVE;
                return true;
            } else if (InputState == SequenceInputState.DRAGGING_SELECTION && UnityEngine.Event.current.type == EventType.mouseUp) {
                InputState = SequenceInputState.INACTIVE;
                Rect nonNormalized = new Rect(MousePositionStart, GetCurrentMousePosition() - MousePositionStart);
                Rect normalized = new Rect();
                normalized.x = Mathf.Min(nonNormalized.xMin, nonNormalized.xMax);
                normalized.y = Mathf.Min(nonNormalized.yMin, nonNormalized.yMax);
                normalized.width = Mathf.Abs(nonNormalized.width);
                normalized.height = Mathf.Abs(nonNormalized.height);

                SelectEvents(normalized);
                return true;
            }
            if (TimeLineData.Rect.Contains(GetCurrentMousePosition())) {

                if (UnityEngine.Event.current.type == EventType.mouseDown && UnityEngine.Event.current.button == 2 && UnityEngine.Event.current.alt) {
                    InputState = SequenceInputState.DRAGGING_TIMELINESCALE;
                    MousePositionStart = GetCurrentMousePosition();
                    ScaleMousePositionStart = UnityEngine.Event.current.mousePosition;
                    ScaleMouseValueStart = TimeLineData.GetTimeAtMousePosition(UnityEngine.Event.current.mousePosition);
                    return true;
                } else if (UnityEngine.Event.current.type == EventType.mouseDown && UnityEngine.Event.current.button == 2) {
                    InputState = SequenceInputState.DRAGGING_TIMELINE;
                    MousePositionStart = GetCurrentMousePosition();
                    return true;
                }
            } else if (CurrentTimeRect.Contains(UnityEngine.Event.current.mousePosition)) {
                if (UnityEngine.Event.current.type == EventType.mouseDown && UnityEngine.Event.current.button == 0) {
                    InputState = SequenceInputState.DRAGGING_CURRENTTIME;
                    MousePositionStart = UnityEngine.Event.current.mousePosition;
                    return true;
                }
            } else if (DurationTimeRect.Contains(UnityEngine.Event.current.mousePosition)) {
                if (UnityEngine.Event.current.type == EventType.mouseDown && UnityEngine.Event.current.button == 0) {
                    InputState = SequenceInputState.DRAGGING_DURATION;
                    MousePositionStart = UnityEngine.Event.current.mousePosition;
                    return true;
                }
            }

            //Click on Timeline
            if (InputState == SequenceInputState.INACTIVE && UnityEngine.Event.current.type == EventType.mouseDown) {
                Rect TopTimelineRect = new Rect(TimelineRect.x, 0, TimelineRect.width, 20);
                if (TopTimelineRect.Contains(UnityEngine.Event.current.mousePosition)) {
                    float position = Snap(TimeLineData.GetTimeAtMousePosition(UnityEngine.Event.current.mousePosition), null).Value;
                    Sequence.SetCurrentTime(Mathf.Clamp(position, 0, Sequence.Duration));
                    InputState = SequenceInputState.DRAGGING_CURRENTTIME;
                    GUIUtility.keyboardControl = -1;
                    return true;
                }
            }
            if (TimelineRect.Contains(GetCurrentMousePosition())) {
                if (UnityEngine.Event.current.type == EventType.mouseDown && UnityEngine.Event.current.button == 0) {
                    InputState = SequenceInputState.DRAGGING_SELECTION;
                    MousePositionStart = GetCurrentMousePosition();

                    GUIUtility.keyboardControl = -1;
                    return true;
                }
            }
            if (Sequence.Recording && !HasTimeChanged() && InputState == SequenceInputState.INACTIVE) {
                foreach (SequencerTrackGroupView trackGroupView in TrackGroupViews) {
                    trackGroupView.UpdateWhileRecording(Sequence.GetCurrentTime());
                }
            }
            if (UnityEngine.Event.current.keyCode == KeyCode.F) {
                FocusTimeline(Sequence.GetCurrentTime());
            }
            if (UnityEngine.Event.current.type == EventType.mouseDown && UnityEngine.Event.current.button == 0) {
                GUIUtility.keyboardControl = -1;
            }

            return false;
        }

        public static Vector2 GetCurrentMousePosition() {
            return UnityEngine.Event.current.mousePosition + new Vector2(0, -1f * TopHeight + ScrollPosition.y);
        }

        public bool HasTimeChanged() {
            return UpdateTime != LastUpdateTime;
        }

        private void StartRecording() {
            foreach (SequencerTrackGroupView trackGroupView in TrackGroupViews) {
                trackGroupView.RecordingStart();
            }
        }

        private void EndRecording() {
            foreach (SequencerTrackGroupView trackGroupView in TrackGroupViews) {
                trackGroupView.RecordingStop();
            }
        }

        public bool IsRecording() {
            return Sequence.Recording;
        }

        public void PrePaint() {
            UpdateTime = Sequence.GetCurrentTime();
        }

        public void PostPaint() {
            LastUpdateTime = UpdateTime;
        }

        public void AddTrackGroupCallback(object o) {
            Type t = (System.Type) o;
            MethodInfo method = typeof(SequencerSequenceView).GetMethod("AddEvent");
            method = method.MakeGenericMethod(t);
            object[] parametersArray = new object[] { null };
            method.Invoke(this, parametersArray);
        }

        public bool AddTrackGroup<T>(object ctrl) where T : Component {
            //if controller comes in at init, it just casts it and creates View for it
            //if function is called to add new, it creates controller first, and then view afterwards
            Undo.RecordObject(Sequence, "AddTrackGroup");
            T controller;
            if (ctrl == null) {
                controller = (T) Undo.AddComponent<T>(Sequence.gameObject);
                SequencerTrackGroup eventController = controller as SequencerTrackGroup;
                Sequence.TrackGroups.Add(eventController);
                EditorUtility.SetDirty(Sequence);
            } else
                controller = (T) ctrl;

            controller.hideFlags = HideFlags.HideInInspector;
            Type viewType = Type.GetType("Outfit7.Sequencer." + controller.GetType().Name + "View", true);
            SequencerTrackGroupView instance = Activator.CreateInstance(viewType) as SequencerTrackGroupView;
            instance.Init(controller);
            TrackGroupViews.Add(instance);
            return true;
        }

        public List<SequencerEventView> GetAllEvents(bool onlySelected = false) {
            List<SequencerEventView> allEvents = new List<SequencerEventView>();
            foreach (SequencerTrackGroupView groupView in TrackGroupViews) {
                foreach (SequencerTrackView trackView in groupView.TrackViews) {
                    foreach (SequencerEventView eventView in trackView.EventViews) {
                        if (onlySelected && eventView.Selected)
                            allEvents.Add(eventView);
                        else if (!onlySelected)
                            allEvents.Add(eventView);
                    }
                }
            }
            return allEvents;
        }

        public void DeselectAll() {
            foreach (SequencerTrackGroupView groupView in TrackGroupViews) {
                foreach (SequencerTrackView trackView in groupView.TrackViews) {
                    foreach (SequencerEventView eventView in trackView.EventViews) {
                        eventView.DeselectEvent();
                    }
                }
            }
        }

        public void InsertTime(object obj) {
            InsertTimeLocation = (Vector2) obj;
            ShowInsertTime = true;
            InsertTimeRect = new Rect(Vector2.Scale(InsertTimeLocation / Sequence.Duration, DurationTimeRect.max), new Vector2(Screen.width / 6f, Screen.height / 6f));
        }

        private void InsertTimeImpl(float time) {
            Undo.RecordObject(Sequence, "InsertTime");
            Sequence.Duration += time;
            for (int i = 0; i < TrackGroupViews.Count; ++i) {
                var trackGroup = TrackGroupViews[i];
                for (int j = 0; j < trackGroup.TrackViews.Count; ++j) {
                    var trackView = trackGroup.TrackViews[j];
                    for (int k = 0; k < trackView.EventViews.Count; ++k) {
                        var evntView = trackView.EventViews[k];
                        var evnt = evntView.GetEvent();
                        if (evnt.StartTime >= InsertTimeLocation.x) {
                            Undo.RecordObject(evnt, "InsertTime");
                            evnt.StartTime += time;
                        }
                    }
                }
            }
        }

        public void RemoveTimeImpl(float startPoint, float endPoint, SequencerEventView viewToDestroy) {
            var delta = endPoint - startPoint;

            Undo.RecordObject(Sequence, "RemoveTime");
            for (int i = 0; i < TrackGroupViews.Count; ++i) {
                var trackGroup = TrackGroupViews[i];
                for (int j = 0; j < trackGroup.TrackViews.Count; ++j) {
                    var trackView = trackGroup.TrackViews[j];
                    for (int k = 0; k < trackView.EventViews.Count; ++k) {
                        var evntView = trackView.EventViews[k];
                        var evnt = evntView.GetEvent();
                        if (evnt == null) {
                            // could be destroyed already
                            continue;
                        }
                        bool timeCheck1 = evnt.StartTime > startPoint && evnt.StartTime <= endPoint && evnt.EventDirection == SequencerEvent.EEventDirection.LEFT;
                        bool timeCheck2 = evnt.StartTime >= startPoint && evnt.StartTime < endPoint && evnt.EventDirection != SequencerEvent.EEventDirection.LEFT;
                        bool bookmarkCheck = evnt is SequencerBookmarkEvent;

                        if (((timeCheck1 || timeCheck2) && (!bookmarkCheck)) || evntView == viewToDestroy) {

                            // delete events at starttime that aren't oriented left AND delete events at end time oriented left
                            var track = trackView.GetTrack();
                            Undo.RecordObject(track, "RemoveTime");
                            track.Events.Remove(evnt);
                            EditorUtility.SetDirty(track);
                            evntView.DestroyEvent();
                        } else if (evnt.StartTime >= endPoint) {
                            Undo.RecordObject(evnt, "RemoveTime");
                            // try to avoid fpu errors
                            if (evnt.StartTime == endPoint) {
                                evnt.StartTime = startPoint;
                            } else {
                                evnt.StartTime -= delta;
                            }
                        }
                    }
                }
            }
            Sequence.Duration -= delta;
            SequencerWindow.SequencerEditor.RemakeView();
        }

        private void InsertTimeWindowOnClose() {
            InsertTimeWindow = null;
            ShowInsertTime = false;
        }

        public void RemoveTrackGroup(object obj) {
            SequencerTrackGroupView trackView = (SequencerTrackGroupView) obj;
            Sequence.TrackGroups.Remove(trackView.GetTrackGroup());
            trackView.DestroyTrackGroup();
            TrackGroupViews.Remove(trackView);
        }

        public void MoveTrackGroupDown(object obj) {
            SequencerTrackGroupView trackView = (SequencerTrackGroupView) obj;

            int index = TrackGroupViews.IndexOf(trackView);
            if (index + 1 >= TrackGroupViews.Count)
                return;
            SequencerTrackGroup thisTrackGroup = Sequence.TrackGroups[index];
            SequencerTrackGroupView bottomTrackGroupView = TrackGroupViews[index + 1];
            SequencerTrackGroup bottomTrackGroup = Sequence.TrackGroups[index + 1];

            Sequence.TrackGroups[index] = bottomTrackGroup;
            Sequence.TrackGroups[index + 1] = thisTrackGroup;

            TrackGroupViews[index] = bottomTrackGroupView;
            TrackGroupViews[index + 1] = trackView;

            if (!Application.isPlaying) {
                EditorUtility.SetDirty(Sequence.gameObject);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }

        public void MoveTrackGroupUp(object obj) {
            SequencerTrackGroupView trackView = (SequencerTrackGroupView) obj;

            int index = TrackGroupViews.IndexOf(trackView);
            if (index - 1 < 0)
                return;
            SequencerTrackGroup thisTrackGroup = Sequence.TrackGroups[index];
            SequencerTrackGroupView bottomTrackGroupView = TrackGroupViews[index - 1];
            SequencerTrackGroup bottomTrackGroup = Sequence.TrackGroups[index - 1];

            Sequence.TrackGroups[index] = bottomTrackGroup;
            Sequence.TrackGroups[index - 1] = thisTrackGroup;

            TrackGroupViews[index] = bottomTrackGroupView;
            TrackGroupViews[index - 1] = trackView;
            if (!Application.isPlaying) {
                EditorUtility.SetDirty(Sequence.gameObject);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }

        public SnapInfo Snap(float value, SequencerEvent evnt = null, bool rightSnap = false, bool link = true) {
            SnapInfo snapInfo = new SnapInfo();

            if (AltSnapping) {
                snapInfo.SnappedFromEnd = rightSnap;
                List<SequencerEvent> events = new List<SequencerEvent>();
                Sequence.gameObject.GetComponents<SequencerEvent>(events);
                if (evnt != null) {
                    events.Remove(evnt);
                }
                if (events.Count == 0) {
                    snapInfo.Value = value;
                } else {
                    float closestMatch = Mathf.Infinity;
                    float closestDif = Mathf.Infinity;
                    SequencerEvent closestEvent = null;
                    foreach (SequencerEvent e in events) {
                        //check start point
                        if (Mathf.Abs(value - e.StartTime) < closestDif) {
                            closestDif = Mathf.Abs(value - e.StartTime);
                            closestMatch = e.StartTime;
                            closestEvent = e;
                            // ugly but at this point can't do much better
                            snapInfo.SnappedToEnd = false;
                        }
                        //check endpoint
                        if (Mathf.Abs(value - e.GetEndPoint()) < closestDif) {
                            closestDif = Mathf.Abs(value - e.GetEndPoint());
                            closestMatch = e.GetEndPoint();
                            closestEvent = e;
                            snapInfo.SnappedToEnd = true;
                        }
                    }
                    if (closestDif < 0.1f) {
                        snapInfo.Value = closestMatch;
                        snapInfo.Event = closestEvent;
                    } else {
                        snapInfo.Value = value;
                    }
                }
            } else if (Snapping) {
                snapInfo.Value = Mathf.Round(value * 30f) / 30f;
            } else {
                snapInfo.Value = value;
            }

            if (link) {
                SnapEvent(evnt, snapInfo.Event, value, snapInfo.Value, snapInfo.SnappedFromEnd, snapInfo.SnappedToEnd);
            }
            return snapInfo;
        }

        public float Snap(float value, SequencerEvent evnt, ref SequencerTriggerEvent.EEventDirection handle) {
            //micro offset snap for trigger events
            float oldValue = value;
            float newValue;
            var info = Snap(value, evnt);
            if (Snapping || AltSnapping) {
                newValue = info.Value;
                if (newValue < oldValue) {
                    handle = SequencerTriggerEvent.EEventDirection.RIGHT;
                } else if (newValue > oldValue) {
                    handle = SequencerTriggerEvent.EEventDirection.LEFT;
                } else {
                    // mid
                    handle = SequencerTriggerEvent.EEventDirection.RIGHT;
                }
            } else {
                newValue = value;
                // mid
                handle = SequencerTriggerEvent.EEventDirection.RIGHT;
            }

            return newValue;
        }

        public void FocusTimeline(float startTime, bool timeScaleAlso = false) {
            TimeLineData.Offset = -startTime + 0.1f;

            /*if (timeScaleAlso) {
                float endTime = Sequence.Duration;
                for (int i = 0; i < Sequence.Bookmarks.Count; i++) {
                    if (Sequence.Bookmarks[i].StartTime > startTime && Sequence.Bookmarks[i].StartTime < endTime) {
                        endTime = Sequence.Bookmarks[i].StartTime;
                    }
                }
                float duration = endTime - startTime;
                Debug.LogError(TimelineRect.width);
                TimeLineData.Scale = TimelineRect.width / (duration * 200f);
                TimeLineData.LenghtOfASecond = TimeLineData.Scale * 200f;
            }*/
        }

        private void SnapEvent(SequencerEvent srcEvent, SequencerEvent toEvent, float oldTime, float newTime, bool snappedFromEnd, bool snappedToEnd) {
            // this is ugly but can't do much without breaking data links and rewriting most of the sequencer

            if (srcEvent == null) {
                return;
            }

            bool linkEvent = true;
            bool goToLastEvent = true;
            SequencerTriggerEvent.EEventDirection handle;
            if (newTime < oldTime) {
                handle = SequencerTriggerEvent.EEventDirection.RIGHT;
            } else if (newTime > oldTime) {
                handle = SequencerTriggerEvent.EEventDirection.LEFT;
            } else {
                // mid
                handle = SequencerTriggerEvent.EEventDirection.RIGHT;
                // if we don't have an event entry here, it means we're not linked to anything
                if (toEvent == null) {
                    linkEvent = false;
                } else {
                    goToLastEvent = false;
                }
            }

            if (linkEvent) {
                LinkEvents(srcEvent, toEvent, handle, goToLastEvent, snappedFromEnd, snappedToEnd);
            } else {
                srcEvent.ClearLinks();
            }
        }

        public void SetPreviewEvent(SequencerContinuousEvent evnt) {
            Sequence.PreviewEvent = evnt;
        }

        public static string[] GetParameterNames(SequencerSequence sequence, System.Predicate<Parameter> match, bool canBeNone) {
            string[] names = new string[canBeNone ? 1 : 0];
            if (canBeNone) {
                names[0] = "None";
            }
            for (int i = 0; i < sequence.Parameters.Count; i++) {
                if (match == null || match.Invoke(sequence.Parameters[i])) {
                    ArrayUtility.Add(ref names, sequence.Parameters[i].Name);
                }
            }
            return names;
        }

        protected void LinkEvents(SequencerEvent srcEvent, SequencerEvent toEvent, SequencerEvent.EEventDirection direction, bool goToLastInChain, bool snappedFromEnd, bool snappedToEnd) {
            //O7Log.Error("Linking {0} to {1} {2}", srcEvent ? srcEvent.GetInstanceID().ToString() : "null", toEvent ? toEvent.GetInstanceID().ToString() : "null", direction);

            if (toEvent != null && !toEvent.CanBeLinkedTo()) {
                toEvent = null;
            }

            var srcEventNodeIndex = snappedFromEnd ? 1 : 0;
            var toEventNodeIndex = snappedToEnd ? 1 : 0;

            srcEvent.LinkToEvent(toEvent, direction, srcEventNodeIndex, toEventNodeIndex, goToLastInChain);
        }

        private class InsertTimePopup : PopupWindowContent {
            private Action<float> ConfirmAction;
            private Action OnCloseAction;
            private float TimeInsert;

            public InsertTimePopup(Action<float> onOk, Action onClose) {
                ConfirmAction = onOk;
                OnCloseAction = onClose;
                TimeInsert = 0f;
            }

            public override Vector2 GetWindowSize() {
                return new Vector2(200, 150);
            }

            public override void OnGUI(Rect rect) {
                var position = rect;
                position.height = EditorGUIUtility.singleLineHeight * 1.3f;

                EditorGUI.LabelField(position, "Insert Time", EditorStyles.boldLabel);
                position.y += position.height;
                TimeInsert = EditorGUI.FloatField(position, "Time", TimeInsert);
                position.y += position.height;
                if (TimeInsert < 0f) {
                    TimeInsert = 0f;
                }

                position.y = rect.height - 30f;
                position.height = rect.height - position.y;
                if (GUI.Button(position, "Confirm")) {
                    if (ConfirmAction != null) {
                        ConfirmAction(TimeInsert);
                    }
                    editorWindow.Close();
                }
            }

            public override void OnClose() {
                if (OnCloseAction != null) {
                    OnCloseAction();
                }
            }
        }
    }
}