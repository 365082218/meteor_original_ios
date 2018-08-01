#if !STARLITE_EDITOR
using Outfit7.Logic.Util;
#else
using Starlite;
#endif

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Starlite.Raven {

    public abstract class RavenTrackView {
        protected List<RavenEventView> m_EventViews = new List<RavenEventView>();
        protected RavenTrack m_Track = null;
        protected RavenTrackGroupView m_Parent = null;
        protected float m_Indent = 20f;

        protected int m_AddEventFrame = 0;
        protected int m_AddEventSubTrackIndex = 0;
        protected int m_HighestSubTrackIndex = 0;

        protected Rect m_TimelineTrackRect = new Rect();
        protected Rect m_SidebarTrackRect = new Rect();

        private Color m_LineColor = new Color(0.33f, 0.33f, 0.33f);
        private Color m_SplitLineColor = new Color(0.2f, 0.2f, 0.2f);

        private float m_StartHeight = 0;

        private List<RavenEventView> m_NewlyCreatedViews = new List<RavenEventView>();

        public abstract string Name {
            get;
        }

        public List<RavenEventView> EventViews {
            get {
                return m_EventViews;
            }
        }

        public RavenTrack Track {
            get {
                return m_Track;
            }
        }

        public GameObject Target {
            get {
                if (m_Parent == null) {
                    return null;
                }

                return m_Parent.TrackGroup.m_Target;
            }
        }

        public int OverrideTargetsParameterIndex {
            get {
                return m_Parent.TrackGroup.m_OverrideTargetsParameterIndex;
            }
        }

        public abstract int TrackIndex {
            get;
        }

        public virtual void Initialize(RavenTrack track, RavenTrackGroupView parent) {
            m_Track = track;
            m_Parent = parent;
            m_Track.FoldoutHeight = 90f;

            for (int i = 0; i < m_Track.Events.Count; i++) {
                AddEvent<RavenEvent>(m_Track.Events[i]);
            }
        }

        public float DrawTimelineGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, List<RavenParameter> parameters) {
            m_StartHeight = startHeight;
            m_HighestSubTrackIndex = GetHighestEventSubTrackIndex();
            var sequenceView = RavenSequenceEditor.Instance.SequenceView;
            for (int i = 0; i < m_EventViews.Count; i++) {
                var eventView = m_EventViews[i];
                if (sequenceView.IsEventVisible(eventView)) {
                    eventView.DrawGui(windowSize, startHeight, splitViewLeft, timelineData, m_Track.FoldoutEnabled, m_Track.FoldoutHeight, parameters);
                }
            }
            // We need to force layout rebuild on newly created events so we calculate event rect and possibly do some other stuff
            for (int i = m_NewlyCreatedViews.Count - 1; i >= 0; --i) {
                var eventView = m_NewlyCreatedViews[i];
                m_NewlyCreatedViews.RemoveAt(i);
                eventView.LayoutChanged(windowSize, startHeight, splitViewLeft, timelineData, m_Track.FoldoutEnabled, m_Track.FoldoutHeight, parameters);
            }
            float height = 0;
            if (m_Track.FoldoutEnabled) {
                height += m_Track.FoldoutHeight * (m_HighestSubTrackIndex + 1);
            } else {
                height = 20f;
                height *= (m_HighestSubTrackIndex + 1);
            }
            m_TimelineTrackRect = new Rect(splitViewLeft, startHeight, windowSize.width - splitViewLeft, height);
            DrawTimelineLines(windowSize, startHeight, splitViewLeft, height);

            return height;
        }

        public float DrawSidebarGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, List<RavenParameter> parameters) {
            var oldFoldout = m_Track.FoldoutEnabled;
            var foldoutRect = new Rect(m_Indent, startHeight, splitViewLeft - m_Indent - 60f, 20);
            m_Track.FoldoutEnabled = EditorGUI.Foldout(foldoutRect, m_Track.FoldoutEnabled, Name, true);

            // mute
            var muteToggleRect = new Rect(foldoutRect.x + foldoutRect.width, foldoutRect.y, 16f, 16f);
            var oldEnabled = m_Track.IsEnabled;
            m_Track.IsEnabled = EditorGUI.Toggle(muteToggleRect, m_Track.IsEnabled);

            if (m_Track.IsEnabled != oldEnabled) {
                RavenSequenceEditor.Instance.Sequence.FlagDirty();
            }

            // solo
            muteToggleRect.x += 20f;
            if (EditorGUI.Toggle(muteToggleRect, false)) {
                // turn off every track but this one
                RavenSequenceEditor.Instance.SequenceView.ToggleAllTracks(false, this);
                if (!m_Track.IsEnabled) {
                    RavenSequenceEditor.Instance.Sequence.FlagDirty();
                }
                m_Track.IsEnabled = true;
            }
            muteToggleRect.x += 20f;
            if (EditorGUI.Toggle(muteToggleRect, false)) {
                // turn on every track but this one
                RavenSequenceEditor.Instance.SequenceView.ToggleAllTracks(true, this);
                if (m_Track.IsEnabled) {
                    RavenSequenceEditor.Instance.Sequence.FlagDirty();
                }
                m_Track.IsEnabled = false;
            }

            OnDrawSidebarGui(windowSize, startHeight, splitViewLeft, timelineData, m_Track.FoldoutEnabled, m_Track.FoldoutHeight, parameters);

            float height = 0;
            if (m_Track.FoldoutEnabled) {
                height += m_Track.FoldoutHeight * (m_HighestSubTrackIndex + 1);
            } else {
                height = 20f;
                height *= (m_HighestSubTrackIndex + 1);
            }
            m_SidebarTrackRect = new Rect(0, startHeight, splitViewLeft, height);

            if (oldFoldout != m_Track.FoldoutEnabled) {
                for (int i = 0; i < m_EventViews.Count; i++) {
                    m_EventViews[i].LayoutChanged(windowSize, startHeight, splitViewLeft, timelineData, m_Track.FoldoutEnabled, m_Track.FoldoutHeight, parameters);
                }
            }
            return height;
        }

        public bool HandleInput(Vector2 mousePosition, TimelineData timelineData, RavenSequenceView sequenceView, bool optimizedView = false) {
            if (!optimizedView && m_SidebarTrackRect.Contains(mousePosition) &&
                UnityEngine.Event.current.type == EventType.mouseUp &&
                UnityEngine.Event.current.button == 1) {
                DrawTrackContextMenu();
                return true;
            } else if (!optimizedView && m_SidebarTrackRect.Contains(mousePosition) &&
                       UnityEngine.Event.current.type == EventType.mouseDown &&
                       UnityEngine.Event.current.button == 0) {
                return true; //block touches through
            }
            /*if (optimizedView && mousePosition.x < RavenEditor.LeftSpliWidth)
                return true; //block views for optimized*/
            for (int i = 0; i < m_EventViews.Count; i++) {
                var eventView = m_EventViews[i];
                if (sequenceView.IsEventVisible(eventView) || eventView.HasInput()) {
                    if (eventView.HandleInput(mousePosition, this, timelineData, sequenceView, m_TimelineTrackRect, m_HighestSubTrackIndex)) {
                        return true;
                    }
                } else {
                    eventView.ResetInput();
                }
            }
            if (m_TimelineTrackRect.Contains(mousePosition) && !optimizedView &&
                UnityEngine.Event.current.type == EventType.mouseUp &&
                UnityEngine.Event.current.button == 1) {
                DrawEventsContextMenu(mousePosition, timelineData);
                return true;
            }
            return OnHandleInput(mousePosition, timelineData, sequenceView, optimizedView);
        }

        public void DestroyTrack() {
            for (int i = 0; i < m_EventViews.Count; ++i) {
                m_EventViews[i].DestroyEvent();
            }
            OnDestroyTrack();
            Undo.DestroyObjectImmediate(m_Track);
        }

        public void SelectEvents(Rect selectionRect) {
            for (int i = 0; i < m_EventViews.Count; i++) {
                var eventView = m_EventViews[i];
                if (RavenSequenceEditor.Instance.SequenceView.IsEventVisible(eventView)) {
                    m_EventViews[i].SelectEvent(selectionRect);
                }
            }
        }

        public void AddEvent<T>(object ctrl, out RavenEventView eventView) where T : RavenEvent {
            // if controller comes in at init, it just casts it and creates View for it
            // if function is called to add new, it creates controller first, and then view afterwards
            T controller = ctrl as T;
            if (ctrl == null) {
                Undo.RecordObject(m_Track, "AddEvent");
                var sequence = RavenSequenceEditor.Instance.Sequence;
                controller = Undo.AddComponent<T>(m_Track.gameObject);

                var startFrame = m_AddEventFrame;
                var subTrackIndex = FindFirstEmptySubTrackFor(startFrame, m_AddEventSubTrackIndex);
                var lastFrame = Math.Min(Math.Max(startFrame + (int)(sequence.Fps * RavenPreferences.TimelineNewEventDuration) - 1, startFrame), FindLastEmptyFrameFrom(startFrame, subTrackIndex));
                controller.InitializeEditor(sequence, Target, m_AddEventFrame, lastFrame, TrackIndex, subTrackIndex);
                controller.IsTrackEnabled = m_Track.IsEnabled;

                m_Track.Events.Add(controller);
                sequence.AddEventToSortedLists(controller);
                EditorUtility.SetDirty(m_Track);
            }

            controller.SetHideFlags(HideFlags.HideInInspector);
            Type viewType = Type.GetType(controller.GetType().ToString() + "View", true);
            RavenEventView instance = Activator.CreateInstance(viewType) as RavenEventView;
            instance.Initialize(controller, this);
            m_EventViews.Add(instance);
            m_NewlyCreatedViews.Add(instance);
            if (m_Parent != null) {
                m_Parent.AddEventView(instance);
            }

            eventView = instance;
        }

        public void AddEvent<T>(object ctrl) where T : RavenEvent {
            RavenEventView eventView;
            AddEvent<T>(ctrl, out eventView);
        }

        public void RemoveEvent(object obj) {
            RavenEventView eventView = (RavenEventView)obj;
            eventView.DestroyEvent();
        }

        public virtual void RefreshAllEvents(object actor) {
            for (int i = 0; i < m_EventViews.Count; ++i) {
                m_EventViews[i].Refresh(actor);
            }
        }

        public void UpdateWhileRecording(double currentTime) {
            for (int i = 0; i < m_EventViews.Count; ++i) {
                m_EventViews[i].UpdateWhileRecording(currentTime);
            }
        }

        public void RecordingStart() {
            OnRecordingStart();
            for (int i = 0; i < m_EventViews.Count; i++) {
                m_EventViews[i].RecordingStart();
            }
        }

        public void RecordingStop() {
            OnRecordingStop();
            for (int i = 0; i < m_EventViews.Count; i++) {
                m_EventViews[i].RecordingStop();
            }
        }

        public bool ContainsMousePosition(Vector2 mousePosition) {
            return m_TimelineTrackRect.Contains(mousePosition);
        }

        public virtual void OnEventViewDestroyed(RavenEventView eventView) {
            m_Track.RemoveEvent(eventView.Event);
            m_EventViews.Remove(eventView);
            if (m_Parent != null) {
                m_Parent.RemoveEventView(eventView);
            }
        }

        public abstract void OnTargetChanged(GameObject target);

        protected abstract void OnDrawSidebarGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, bool foldoutEnabled, float foldoutHeight, List<RavenParameter> parameters);

        protected abstract void OnDestroyTrack();

        protected abstract void OnRecordingStart();

        protected abstract void OnRecordingStop();

        protected abstract void OnDrawEventsContextMenu(GenericMenu menu);

        protected abstract void OnDrawTrackContextMenu(GenericMenu menu);

        protected virtual bool OnHandleInput(Vector2 mousePosition, TimelineData timelineData, RavenSequenceView sequenceView, bool optimizedView) {
            return false;
        }

        protected void DrawEventsContextMenu(Vector2 mousePosition, TimelineData timelineData) {
            if (!CalculateAndSetAddEventPosition(mousePosition, timelineData)) {
                return;
            }

            GenericMenu menu = new GenericMenu();
            OnDrawEventsContextMenu(menu);
            menu.AddItem(new GUIContent("Adjust Time"), false, RavenSequenceEditor.Instance.SequenceView.AdjustTime, m_AddEventFrame);
            menu.ShowAsContext();
        }

        protected int FindFirstEmptySubTrackFor(int startFrame, int startSubTrack) {
            var sequenceView = RavenSequenceEditor.Instance.SequenceView;
            while (sequenceView.CheckFrameOverlap(null, startFrame, TrackIndex, startSubTrack)) {
                startSubTrack++;
            }

            return startSubTrack;
        }

        protected int FindFirstEmptySubTrackFor(int startFrame, int lastFrame, int startSubTrack) {
            var sequenceView = RavenSequenceEditor.Instance.SequenceView;
            while (sequenceView.CheckContinuousFrameOverlap(null, startFrame, lastFrame, TrackIndex, startSubTrack)) {
                startSubTrack++;
            }

            return startSubTrack;
        }

        /// <summary>
        /// Assumes startFrame is legit.
        /// </summary>
        protected int FindLastEmptyFrameFrom(int startFrame, int subTrackIndex) {
            var sequence = RavenSequenceEditor.Instance.Sequence;
            int lastFrame = -1;
            for (int i = 0; i < sequence.SortedEvents.Count; ++i) {
                var evnt = sequence.SortedEvents[i];
                if (evnt.TrackIndex != TrackIndex || evnt.SubTrackIndex != subTrackIndex) {
                    continue;
                }

                if (evnt.StartFrame >= startFrame) {
                    lastFrame = startFrame = evnt.StartFrame - 1;
                    break;
                }
            }

            if (lastFrame == -1) {
                lastFrame = sequence.TotalFrameCount - 1;
            }

            return lastFrame;
        }

        protected bool CalculateAndSetAddEventPosition(Vector2 mousePosition, TimelineData timelineData) {
            var frame = (int)RavenSequenceEditor.Instance.Sequence.GetFrameForTime(timelineData.GetTimeAtMousePosition(mousePosition));
            var subTrackIndex = GetLayerFromMousePosition(mousePosition);

            if (frame >= RavenSequenceEditor.Instance.Sequence.TotalFrameCount || frame < 0) {
                return false;
            }

            m_AddEventFrame = frame;
            m_AddEventSubTrackIndex = subTrackIndex;
            return true;
        }

        protected virtual void DrawTrackContextMenu() {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Remove Track (+Events)/Are you sure?"), false, m_Parent.RemoveTrack, this);
            OnDrawTrackContextMenu(menu);
            menu.ShowAsContext();
        }

        protected virtual void DrawTimelineLines(Rect windowSize, float startHeight, float splitViewWidth, float height) {
            GUIUtil.DrawLine(new Vector2(splitViewWidth + 1, startHeight), new Vector2(windowSize.width - 1, startHeight), m_LineColor);
            for (int i = 0; i < m_HighestSubTrackIndex; i++) {
                float lineHeight = (height / (m_HighestSubTrackIndex + 1)) * (i + 1);
                GUIUtil.DrawLine(new Vector2(splitViewWidth + 1, startHeight + lineHeight), new Vector2(windowSize.width - 1, startHeight + lineHeight), m_SplitLineColor);
            }
            GUIUtil.DrawLine(new Vector2(splitViewWidth + 1, startHeight + height), new Vector2(windowSize.width - 1, startHeight + height), m_LineColor);
        }

        private int GetHighestEventSubTrackIndex() {
            int maxIndex = 0;
            foreach (RavenEventView evnt in m_EventViews) {
                int index = evnt.SubTrackIndex;
                if (index > maxIndex)
                    maxIndex = index;
            }
            return maxIndex;
        }

        private int GetLayerFromMousePosition(Vector2 mousePosition) {
            float absoluteHeight = mousePosition.y - m_StartHeight;
            float layer = (absoluteHeight / m_Track.FoldoutHeight);
            return (int)layer;
        }
    }
}