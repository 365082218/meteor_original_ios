using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Outfit7.Util;
using Outfit7.Logic.StateMachineInternal;
using System.Reflection;
using System.Linq;
using System;

namespace Outfit7.Sequencer {
    public class SequencerActorTrackGroupAttribute : System.Attribute {
    }

    [SequencerSequenceAttribute]
    public class SequencerActorTrackGroupView : SequencerTrackGroupView {
        private SequencerActorTrackGroup TrackGroup = null;
        private Vector2 NewEventLocation;

        public override void OnInit(object track) {
            TrackGroup = (SequencerActorTrackGroup) track;
            base.OnInit(track);
            if (TrackGroup.Tracks.Count == 0) { //HOPEFULLY JUST FIRST START
                AddTrack<SequencerCurveTrack>(null);
                if (Selection.activeGameObject != null) {
                    if (Selection.activeGameObject.GetComponent<SequencerSequence>() == null)
                        TrackGroup.Actor = Selection.activeGameObject;
                }
            }
        }

        protected override void GetTypesEnumerator() {
            Assembly assembly = typeof(SequencerTrackView).Assembly;
            TypesEnumerator = assembly.GetTypes().Where(t => typeof(SequencerTrackView).IsAssignableFrom(t)).
                Where(t => Attribute.IsDefined(t, typeof(SequencerActorTrackGroupAttribute)));
        }

        public override string GetName() {
            return TrackGroup.Actor != null ? TrackGroup.Actor.name : "None";
        }

        public override void OnDrawGui(Rect windowSize, float startHeight, float splitViewWidth, TimelineData timelineData, List<Parameter> parameters) {
            if (GUI.Button(AddTrackButton, "+", Button))
                DrawEventsContextMenu();
            
            TrackGroup.Actor = (GameObject) EditorGUI.ObjectField(new Rect(30, startHeight, splitViewWidth - 60, 15), "", TrackGroup.Actor, typeof(GameObject), true);
        }

        public override bool OnHandleInput(TimelineData timelineData, SequencerSequenceView sequenceView, int index) {
            if (TrackGroup.TrackGroupMode == SequencerTrackGroupMode.OPTIMIZED) {
                float height = SidebarTrackRect.y;
                float splitViewWidth = SidebarTrackRect.width;
                height += 15f;
                for (int p = 0; p < PropertyViews.Count; p++) {
                    Rect propertyTrackRect = new Rect(splitViewWidth + 1, height, WindowSize.width - splitViewWidth - 2, 19f);
                    if (propertyTrackRect.Contains(SequencerSequenceView.GetCurrentMousePosition())) {
                        if (UnityEngine.Event.current.type == EventType.keyDown) {                            
                            switch (UnityEngine.Event.current.keyCode) {
                                case KeyCode.T:
                                    PropertyViews[p].TrackView.SetAddEventPosition(new Vector2(timelineData.GetTimeAtMousePosition(UnityEngine.Event.current.mousePosition), 0));
                                    AddTweenEventCallback(PropertyViews[p]);
                                    return true;
                                case KeyCode.C:
                                    PropertyViews[p].TrackView.SetAddEventPosition(new Vector2(timelineData.GetTimeAtMousePosition(UnityEngine.Event.current.mousePosition), 0));
                                    AddCurveEventCallback(PropertyViews[p]);
                                    return true;
                                case KeyCode.S:
                                    PropertyViews[p].TrackView.SetAddEventPosition(new Vector2(timelineData.GetTimeAtMousePosition(UnityEngine.Event.current.mousePosition), 0));
                                    AddSetEventCallback(PropertyViews[p]);
                                    return true;
                            }
                        }

                        if (UnityEngine.Event.current.type == EventType.mouseUp &&
                            UnityEngine.Event.current.button == 1) {
                            DrawOptimizedContextMenu(PropertyViews[p], timelineData);
                        }
                    }
                    height += 20f;
                }
            }
            return TrackViews[index].HandleInput(timelineData, sequenceView, TrackGroup.Actor, TrackGroup.TrackGroupMode == SequencerTrackGroupMode.OPTIMIZED);
        }

        public void DrawOptimizedContextMenu(BasePropertyView propertyView, TimelineData timelineData) {
            NewEventLocation = new Vector2(timelineData.GetTimeAtMousePosition(UnityEngine.Event.current.mousePosition), 0);
            propertyView.TrackView.SetAddEventPosition(NewEventLocation);
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Tween"), false, AddTweenEventCallback, propertyView);
            menu.AddItem(new GUIContent("Curve"), false, AddCurveEventCallback, propertyView);
            menu.AddItem(new GUIContent("Set"), false, AddSetEventCallback, propertyView);
            menu.AddItem(new GUIContent("Insert Time"), false, GetSequencerView().InsertTime, NewEventLocation);
            menu.ShowAsContext();
        }

        private void AddTweenEventCallback(object propertyObj) {
            BasePropertyView propertyView = propertyObj as BasePropertyView;
            propertyView.TrackView.AddEvent<SequencerPropertyEvent>(null);
            SequencerPropertyEventView eventView = propertyView.TrackView.EventViews[propertyView.TrackView.EventViews.Count - 1] as SequencerPropertyEventView;
            eventView.AddTweenAnimationDataCallback(propertyView.GetType());
            eventView.SelectEvent(eventView.GetEventRect());
            eventView.Selected = true; //hack
        }

        private void AddCurveEventCallback(object propertyObj) {
            BasePropertyView propertyView = propertyObj as BasePropertyView;
            propertyView.TrackView.AddEvent<SequencerPropertyEvent>(null);
            SequencerPropertyEventView eventView = propertyView.TrackView.EventViews[propertyView.TrackView.EventViews.Count - 1] as SequencerPropertyEventView;
            eventView.AddCurveAnimationDataCallback(propertyView.GetType());
            eventView.SelectEvent(eventView.GetEventRect());
            eventView.Selected = true; //hack
        }

        private void AddSetEventCallback(object propertyObj) {
            BasePropertyView propertyView = propertyObj as BasePropertyView;
            propertyView.TrackView.AddEvent<SequencerSetPropertyEvent>(null);
            SequencerSetPropertyEventView eventView = propertyView.TrackView.EventViews[propertyView.TrackView.EventViews.Count - 1] as SequencerSetPropertyEventView;
            eventView.AddSingleAnimationDataCallback(propertyView.GetType());
            eventView.SelectEvent(eventView.GetEventRect());
            eventView.Selected = true; //hack
        }

        public override void RefreshAllTracks() {
            foreach (SequencerTrackView trackView in TrackViews) {
                trackView.RefreshAllEvents(TrackGroup.Actor);
            }
        }
    }
}