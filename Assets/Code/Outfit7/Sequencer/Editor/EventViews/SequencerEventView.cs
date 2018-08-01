using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
using Outfit7.Logic.StateMachineInternal;
using Outfit7.Logic.Util;

namespace Outfit7.Sequencer {
    public class SequencerEventView {
        public static Dictionary<SequencerEvent, SequencerEventView> ViewLookup = new Dictionary<SequencerEvent, SequencerEventView>();

        private SequencerEvent Event = null;
        protected SequencerTrackView ParentTrackView;
        protected Rect EventRect = new Rect();
        protected Vector2 StartDragOffset = new Vector2();
        public bool Selected = false;
        protected bool UsingOptimizedGui = false;

        public virtual void Init(object evnt, object parent) {
            Event = evnt as SequencerEvent;
            ViewLookup[Event] = this;
            SetParent(parent as SequencerTrackView);
            OnInit(evnt, parent);
        }

        public virtual void OnInit(object evnt, object parent) {
        }

        public virtual string GetName() {
            return "Event";
        }

        public SequencerEvent GetEvent() {
            return Event;
        }

        public Rect GetEventRect() {
            return EventRect;
        }

        public int GetUiTrackIndex() {
            return Event.UiTrackIndex;
        }

        public void Refresh(object actor) {
            Undo.RecordObject(Event, "EventInit");
            Event.Init(Vector2.one * -1f);
            Event.Objects.Clear();

            SequencerEvent.ActorComponentObject componentObject = new SequencerEvent.ActorComponentObject();

            if (Event.ComponentType != null) {
                MethodInfo method = typeof(SequencerEventView).GetMethod("GetActorComponent");
                method = method.MakeGenericMethod(Event.ComponentType);
                object[] parametersArray = new object[] { actor };
                componentObject.Components.Add((Component) method.Invoke(this, parametersArray));
            } else {
                componentObject.Components.Add(null);
            }

            Event.Objects.Add(componentObject);

            OnRefresh(actor);
        }

        public virtual void OnRefresh(object actor) {
        }

        public void UpdateWhileRecording(float currentTime) {
            OnUpdateWhileRecording(currentTime);
        }

        public virtual void OnUpdateWhileRecording(float currentTime) {
        }


        public void SelectEvent(Rect selectionRect) {
            Selected = selectionRect.Contains(EventRect.center);
            if (Selected) {
                Event.hideFlags = HideFlags.None;
            } else {
                Event.hideFlags = HideFlags.HideInInspector;
            }
            OnSelectionChanged(Selected);
        }

        public void DeselectEvent() {
            Selected = false;
            Event.hideFlags = HideFlags.HideInInspector;
            OnSelectionChanged(Selected);
        }

        public virtual void OnSelectionChanged(bool selected) {

        }

        public Component GetActorComponent<T>(GameObject actor) where T : Component {
            if (actor)
                return actor.GetComponent<T>();
            else
                return null;
        }

        public void DrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters, bool optimizedView = false) {
            OnDefineEventRect(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, optimizedView);
            if (foldoutData.Enabled) {
                OnDrawExtendedGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
                DrawLinks();
            }
            UsingOptimizedGui = optimizedView;
            if (optimizedView)
                OnDrawOptimizedGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
            else
                OnDrawGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
                
        }

        protected void DrawLinks() {
            if (Selected) {
                SequencerEventView view;
                for (int i = 0; i < Event.SnapNodes.Length; ++i) {
                    var node = Event.SnapNodes[i];

                    var middlePoint = Vector2.zero;
                    var target = Vector2.zero;
                    var leftLink = node.GetLink(SequencerEventSnapNode.ELinkDirection.Left);
                    var rightLink = node.GetLink(SequencerEventSnapNode.ELinkDirection.Right);
                    if (leftLink != null && ViewLookup.TryGetValue(leftLink.GetEvent(), out view)) {
                        var rect = view.GetEventRect();
                        middlePoint = new Vector2(rect.center.x + 10f, (rect.center.y + EventRect.center.y) / 2f);
                        target = rect.center;
                    } else if (rightLink != null && ViewLookup.TryGetValue(rightLink.GetEvent(), out view)) {
                        var rect = view.GetEventRect();
                        middlePoint = new Vector2(rect.center.x - 10f, (rect.center.y + EventRect.center.y) / 2f);
                        target = rect.center;
                    }
                    if (middlePoint != Vector2.zero) {
                        GUIUtil.DrawFatLine(EventRect.center, middlePoint, 5f, Color.red);
                        GUIUtil.DrawFatLine(middlePoint, target, 5f, Color.red);
                    }
                }
            }
        }

        public virtual void OnDefineEventRect(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, bool optimizedView = false) {
        }

        public virtual void OnDrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
        }

        public virtual void OnDrawOptimizedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            OnDrawGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
        }

        public virtual void OnDrawExtendedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
        }

        public bool HandleInput(SequencerTrackView parent, TimelineData timelineData, SequencerSequenceView sequenceView, Rect timelineTrackRect, int HighiestEventTrackIndex, object actor) {
            if (EventRect.Contains(SequencerSequenceView.GetCurrentMousePosition()) &&
                UnityEngine.Event.current.type == EventType.mouseUp &&
                UnityEngine.Event.current.button == 1) {
                DrawEventsContextMenu(parent, actor);
                return true;
            }

            bool result = OnHandleInput(timelineData, sequenceView, timelineTrackRect, HighiestEventTrackIndex, actor);
            if (result &&
                EventRect.Contains(SequencerSequenceView.GetCurrentMousePosition()) &&
                UnityEngine.Event.current.type == EventType.mouseUp &&
                UnityEngine.Event.current.button == 0) {
                SelectEvent(new Rect(0, 0, 4000, 4000));
                return true;
            }
            return result;
        }

        public virtual void SetParent(SequencerTrackView parent) {
            ParentTrackView = parent;
        }

        protected virtual bool OnHandleInput(TimelineData timelineData, SequencerSequenceView sequenceView, Rect timelineTrackRect, int HighiestEventTrackIndex, object actor) {
            return false;
        }

        protected virtual void DrawEventsContextMenu(SequencerTrackView parent, object actor) {
            GenericMenu menu = new GenericMenu();
            OnDrawEventsContextMenu(menu, parent, actor);
            menu.AddItem(new GUIContent("Preplay"), Event.Preplay, SwitchPreplay);
            menu.AddItem(new GUIContent("Remove event"), false, parent.RemoveEvent, this);
            menu.ShowAsContext();
        }

        private void SwitchPreplay() {
            Event.Preplay = !Event.Preplay;
        }

        protected virtual void OnDrawEventsContextMenu(GenericMenu menu, SequencerTrackView parent, object actor) {
        }

        public void DestroyEvent() {
            OnDestroy();
            ParentTrackView.OnEventViewDestroyed(this);
            Undo.DestroyObjectImmediate(Event);
            ViewLookup.Remove(Event);
        }

        protected virtual void OnDestroy() {

        }

        public void RecordingStart() {
            OnRecordingStart();
        }

        public void RecordingStop() {
            OnRecordingStop();
        }

        public virtual void OnRecordingStart() {
        }

        public virtual void OnRecordingStop() {
        }

        /*public float Snap(float value) {
            if (AltSnapping) {
                List<SequencerEvent> events = new List<SequencerEvent>();
                Event.gameObject.GetComponents<SequencerEvent>(events);
                events.Remove(Event);
                if (events.Count == 0)
                    return value;
                float closestMatch = Mathf.Infinity;
                float closestDif = Mathf.Infinity;
                foreach (SequencerEvent e in events) {
                    //check start point
                    if (Mathf.Abs(value - e.StartTime) < closestDif) {
                        closestDif = Mathf.Abs(value - e.StartTime);
                        closestMatch = e.StartTime;
                    }
                    //check endpoint
                    if (Mathf.Abs(value - e.GetEndPoint()) < closestDif) {
                        closestDif = Mathf.Abs(value - e.GetEndPoint());
                        closestMatch = e.GetEndPoint();
                    }
                }
                if (closestDif < 0.1f)
                    return closestMatch;
                else
                    return value;

            } else if (Snapping) {
                return Mathf.Round(value * 30f) / 30f;
            } else {
                return value;
            }
        }*/

        public virtual float EventEndTime() {
            return Event.StartTime;
        }

        public virtual void SetDraggingMode(Vector2 mousePosition) {
        }

        public virtual void EvaluateDragging(Vector2 mousePosition, TimelineData timelineData, SequencerSequenceView sequenceView) {
        }

        public virtual void NormalizedRepositioning(float firstEventStart, float lastEventEnd, float newFirstEventStart, float newLastEventEnd) {
        }

    }

}
