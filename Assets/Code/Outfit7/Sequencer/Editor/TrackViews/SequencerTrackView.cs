using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Outfit7.Util;
using System;
using System.Reflection;
using System.Linq;
using Outfit7.Logic.StateMachineInternal;
using Outfit7.Logic.Util;

namespace Outfit7.Sequencer {

    public class SequencerTrackAttribute : System.Attribute {
        protected string path;

        public SequencerTrackAttribute(string path) {
            this.path = path;
        }

        public string GetPath() {
            return path;
        }
    }

    public class SequencerNormalTrackAttribute : SequencerTrackAttribute {
        public SequencerNormalTrackAttribute(string path) : base(path) {
            this.path = path;
        }
    }

    public class SequencerTrackView {
        private SequencerTrack Track = null;
        public SequencerTrackGroupView Parent = null;
        public List<SequencerEventView> EventViews = new List<SequencerEventView>();

        private Rect TimelineTrackRect = new Rect();
        private Rect SidebarTrackRect = new Rect();
        protected float Indent = 20f;
        private Color LineColor = new Color(0.33f, 0.33f, 0.33f);
        private Color SplitLineColor = new Color(0.2f, 0.2f, 0.2f);
        public IEnumerable<Type> TypesEnumerator;
        private SequencerQuickSearch QuickSearch = null;
        private float StartHeight = 0;

        private Vector2 AddEventPosition = Vector2.zero;
        private int HighiestEventTrackIndex = 0;

        public void Init(object track) {
            OnInit(track);
            for (int i = 0; i < Track.Events.Count; i++) {
                Type t = Track.Events[i].GetType();
                MethodInfo method = typeof(SequencerTrackView).GetMethod("AddEvent");
                method = method.MakeGenericMethod(t);
                object[] parametersArray = new object[] { Track.Events[i] };
                method.Invoke(this, parametersArray);
            }
            GetTypesEnumerator();
        }

        public virtual string GetName() {
            return "Track";
        }

        public virtual void OnInit(object track) {
            Track = (SequencerTrack) track;
            Track.FoldoutData.Height = 90f;
        }

        public SequencerTrack GetTrack() {
            return Track;
        }

        public virtual void OnEventViewDestroyed(SequencerEventView eventView) {

        }

        protected virtual void GetTypesEnumerator() {
            Assembly assembly = typeof(SequencerEventView).Assembly;
            TypesEnumerator = assembly.GetTypes().
                Where(t => typeof(SequencerEventView).IsAssignableFrom(t)).
                Where(t => Attribute.IsDefined(t, typeof(SequencerNormalTrackAttribute)));
        }

        private int GetHighiestEventTrackIndex() {
            int maxIndex = 0;
            foreach (SequencerEventView evnt in EventViews) {
                int index = evnt.GetUiTrackIndex();
                if (index > maxIndex)
                    maxIndex = index;
            }
            return maxIndex;
        }

        public float DrawTimelineGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, List<Parameter> parameters) {
            StartHeight = startHeight;
            HighiestEventTrackIndex = GetHighiestEventTrackIndex();
            for (int i = 0; i < EventViews.Count; i++) {
                if (EventViews[i].GetEvent() != null) {
                    Undo.RecordObject(EventViews[i].GetEvent(), "DrawGuiEventChange");
                    EventViews[i].DrawGui(windowSize, startHeight, splitViewLeft, timelineData, Track.FoldoutData, parameters);
                }
            }
            //Debug.LogError(Track.FoldoutData.Height);
            float height = 0;
            if (Track.FoldoutData.Enabled) {
                height += Track.FoldoutData.Height * (HighiestEventTrackIndex + 1);
            } else {
                height = 20f;
                height *= (HighiestEventTrackIndex + 1);
            }
            TimelineTrackRect = new Rect(splitViewLeft, startHeight, windowSize.width - splitViewLeft, height);
            DrawTimelineLines(windowSize, startHeight, splitViewLeft, height);

            if (QuickSearch != null) {
                OnDrawQuickSearch();
            }
            return height;
        }

        public float DrawSidebarGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, List<Parameter> parameters) {
            //GUIStyle Button = (GUIStyle) "ButtonMid";
            Track.FoldoutData.Enabled = EditorGUI.Foldout(new Rect(Indent, startHeight, splitViewLeft - Indent - 20f, 20), Track.FoldoutData.Enabled, GetName());

            OnDrawGui(windowSize, startHeight, splitViewLeft, timelineData, Track.FoldoutData, parameters);

            float height = 0;
            if (Track.FoldoutData.Enabled) {
                height += Track.FoldoutData.Height * (HighiestEventTrackIndex + 1);
            } else {
                height = 20f;
                height *= (HighiestEventTrackIndex + 1);
            }
            SidebarTrackRect = new Rect(0, startHeight, splitViewLeft, height);
            return height;
        }

        public virtual void OnDrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {

        }

        protected virtual void DrawEventsContextMenu(Vector2 location) {
            AddEventPosition = location;
            GenericMenu menu = new GenericMenu();
            foreach (System.Type t in TypesEnumerator) {
                SequencerTrackAttribute attr = (SequencerTrackAttribute) t.GetCustomAttributes(typeof(SequencerTrackAttribute), false)[0];
                menu.AddItem(new GUIContent(attr.GetPath()), false, AddEventCallback, t);
            }
            menu.AddItem(new GUIContent("Insert Time"), false, Parent.GetSequencerView().InsertTime, location);
            menu.ShowAsContext();
        }

        protected virtual void DrawTrackContextMenu() {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Remove Track (+Events)/Are you sure?"), false, Parent.RemoveTrack, this);
            OnDrawTrackContextMenu(menu);
            menu.ShowAsContext();
        }

        protected virtual void OnDrawTrackContextMenu(GenericMenu menu) {
        }

        public void DestroyTrack() {
            OnDestroyTrack();
            foreach (SequencerEventView eventView in EventViews) {
                eventView.DestroyEvent();
            }
            Undo.DestroyObjectImmediate(Track);
        }

        public virtual void OnDestroyTrack() {
        }

        protected virtual void DrawTimelineLines(Rect windowSize, float startHeight, float splitViewWidth, float height) {
            GUIUtil.DrawLine(new Vector2(splitViewWidth + 1, startHeight), new Vector2(windowSize.width - 1, startHeight), LineColor);
            for (int i = 0; i < HighiestEventTrackIndex; i++) {
                float lineHeight = (height / (HighiestEventTrackIndex + 1)) * (i + 1);
                GUIUtil.DrawLine(new Vector2(splitViewWidth + 1, startHeight + lineHeight), new Vector2(windowSize.width - 1, startHeight + lineHeight), SplitLineColor);
            }
            GUIUtil.DrawLine(new Vector2(splitViewWidth + 1, startHeight + height), new Vector2(windowSize.width - 1, startHeight + height), LineColor);
        }

        public void SelectEvents(Rect selectionRect) {
            for (int i = 0; i < EventViews.Count; i++) {
                EventViews[i].SelectEvent(selectionRect);
            }
        }

        public void AddEventCallback(object o) {
            Type t = (System.Type) o;

            Assembly assembly = typeof(SequencerTrack).Assembly;
            Type eType = Type.GetType("Outfit7.Sequencer." + t.Name.Replace("View", "") + ", " + assembly.GetName(), true);
            MethodInfo method = typeof(SequencerTrackView).GetMethod("AddEvent");
            method = method.MakeGenericMethod(eType);
            object[] parametersArray = new object[] { null };
            method.Invoke(this, parametersArray);
        }

        public bool AddEvent<T>(object ctrl) where T : Component {
            //if controller comes in at init, it just casts it and creates View for it
            //if function is called to add new, it creates controller first, and then view afterwards
            Undo.RecordObject(Track, "AddEvent");
            T controller;
            if (ctrl == null) {
                controller = (T) Undo.AddComponent<T>(Track.gameObject);
                SequencerEvent eventController = controller as SequencerEvent;
                eventController.Init(AddEventPosition);
                Track.Events.Add(eventController);
                EditorUtility.SetDirty(Track);

            } else
                controller = (T) ctrl;

            controller.hideFlags = HideFlags.HideInInspector;
            Type viewType = Type.GetType("Outfit7.Sequencer." + controller.GetType().Name + "View", true);
            SequencerSequence.LastEventCreated = viewType;
            SequencerEventView instance = Activator.CreateInstance(viewType) as SequencerEventView;
            instance.Init(controller, this);
            EventViews.Add(instance);
            return true;
        }

        public void RefreshAllEvents(object actor) {
            OnRefresh(actor);
            foreach (SequencerEventView eventView in EventViews) {
                if (eventView.GetEvent() != null) {
                    eventView.Refresh(actor);
                }
            }
        }

        public void UpdateWhileRecording(float currentTime) {
            foreach (SequencerEventView eventView in EventViews) {
                eventView.UpdateWhileRecording(currentTime);
            }
        }

        public void RecordingStart() {
            OnRecordingStart();
            for (int i = 0; i < EventViews.Count; i++) {
                EventViews[i].RecordingStart();
            }
        }

        public virtual void OnRecordingStart() {
        }

        public void RecordingStop() {
            OnRecordingStop();
            for (int i = 0; i < EventViews.Count; i++) {
                EventViews[i].RecordingStop();
            }
        }

        public virtual void OnRecordingStop() {
        }


        public virtual void OnRefresh(object actor) {
        }

        public bool HandleInput(TimelineData timelineData, SequencerSequenceView sequenceView, object actor, bool optimizedView = false) {
            Vector2 mousePosition = SequencerSequenceView.GetCurrentMousePosition();
            if (!optimizedView && SidebarTrackRect.Contains(mousePosition) &&
                UnityEngine.Event.current.type == EventType.mouseUp &&
                UnityEngine.Event.current.button == 1) {
                DrawTrackContextMenu();
                return true;
            } else if (!optimizedView && SidebarTrackRect.Contains(mousePosition) &&
                       UnityEngine.Event.current.type == EventType.mouseDown &&
                       UnityEngine.Event.current.button == 0) {
                return true; //block touches through
            }
            /*if (optimizedView && mousePosition.x < SequencerEditor.LeftSpliWidth)
                return true; //block views for optimized*/
            for (int i = 0; i < EventViews.Count; i++) {
                if (EventViews[i].GetEvent() != null) {
                    Undo.RecordObject(EventViews[i].GetEvent(), "HandleInputEventChange");
                    if (EventViews[i].HandleInput(this, timelineData, sequenceView, TimelineTrackRect, HighiestEventTrackIndex, actor))
                        return true;
                }
            }
            if (TimelineTrackRect.Contains(mousePosition) && !optimizedView &&
                UnityEngine.Event.current.type == EventType.mouseUp &&
                UnityEngine.Event.current.button == 1) {
                DrawEventsContextMenu(new Vector2(timelineData.GetTimeAtMousePosition(UnityEngine.Event.current.mousePosition), GetLayerFromMousePosition()));
                return true;
            }
            if (TimelineTrackRect.Contains(mousePosition) && !optimizedView &&
                UnityEngine.Event.current.type == EventType.keyDown &&
                UnityEngine.Event.current.keyCode == KeyCode.Tab) {
                DrawQuickSearch(new Vector2(timelineData.GetTimeAtMousePosition(UnityEngine.Event.current.mousePosition), GetLayerFromMousePosition()));
                return true;
            }
            return false;
        }

        private float GetLayerFromMousePosition() {
            Vector2 mousePos = SequencerSequenceView.GetCurrentMousePosition();
            float absoluteHeight = mousePos.y - StartHeight;
            float layer = (absoluteHeight / Track.FoldoutData.Height);
            return layer;

        }

        public void SetAddEventPosition(Vector2 pos) {
            AddEventPosition = pos;
        }

        private void DrawQuickSearch(Vector2 pos) {
            AddEventPosition = pos;
            QuickSearch = new SequencerQuickSearch(SequencerSequenceView.GetCurrentMousePosition(), TypesEnumerator, SequencerSequence.LastEventCreated);
            /*SequencerQuickSearchWindow quickSearch = EditorWindow.GetWindow<SequencerQuickSearchWindow>("QuickSearch");
            quickSearch.SetPosition(WindowPosition + UnityEngine.Event.current.mousePosition);*/

        }

        private void OnDrawQuickSearch() {
            Type typeToCreate;
            if (QuickSearch.OnGui(out typeToCreate)) {
                QuickSearch = null;
                if (typeToCreate != null)
                    AddEventCallback(typeToCreate);
            }
        }

        public void RemoveEvent(object obj) {
            SequencerEventView eventView = (SequencerEventView) obj;
            Track.Events.Remove(eventView.GetEvent());
            eventView.DestroyEvent();
            EventViews.Remove(eventView);
        }
    }
}