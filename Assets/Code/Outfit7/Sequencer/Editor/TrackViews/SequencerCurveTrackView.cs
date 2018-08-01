using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Outfit7.Util;
using System;
using System.Reflection;
using System.Linq;
using Outfit7.Logic.StateMachineInternal;

namespace Outfit7.Sequencer {
    public class SequencerCurveTrackAttribute : SequencerTrackAttribute {
        public SequencerCurveTrackAttribute(string path) : base(path) {
            this.path = path;
        }
    }

    [SequencerActorTrackGroupAttribute]
    public class SequencerCurveTrackView : SequencerTrackView {
        private SequencerCurveTrack Track = null;
        public List<BasePropertyView> PropertyViews = new List<BasePropertyView>(3);

        public override string GetName() {
            return "Curve Tracks";
        }

        public override void OnEventViewDestroyed(SequencerEventView eventView) {
            base.OnEventViewDestroyed(eventView);
            ClearUnusedProperties();
        }

        protected override void GetTypesEnumerator() {
            Assembly assembly = typeof(SequencerEventView).Assembly;
            TypesEnumerator = assembly.GetTypes().
                Where(t => typeof(SequencerEventView).IsAssignableFrom(t)).
                Where(t => Attribute.IsDefined(t, typeof(SequencerCurveTrackAttribute)));
        }

        public override void OnRefresh(object actor) {
            foreach (BasePropertyView propertyView in PropertyViews) {
                propertyView.Refresh(actor, this);
            }
        }

        public override void OnDestroyTrack() {
            foreach (BasePropertyView propertyView in PropertyViews) {
                Undo.DestroyObjectImmediate(propertyView.GetProperty());
            }
            PropertyViews.Clear();
        }

        public Component GetActorComponent<T>(GameObject actor, bool exact) where T : Component {
            if (actor) {
                if (exact) {
                    T[] components = actor.GetComponents<T>();
                    for (int i = 0; i < components.Length; i++) {
                        if (components[i].GetType().FullName == typeof(T).FullName)
                            return components[i];
                    }
                    return null;
                } else
                    return actor.GetComponent<T>();

            } else
                return null;
        }

        protected override void OnDrawTrackContextMenu(GenericMenu menu) {
            menu.AddItem(new GUIContent("Clear unused properties"), false, ClearUnusedProperties);
        }

        public BaseProperty GetProperty<T>(object data) where T : BaseProperty {
            foreach (BaseProperty property in Track.Properties) {
                if (property.GetType() == typeof(T)) {
                    return property;
                }
            }
            AddProperty<T>(null, data);
            Parent.RefreshAllTracks();
            return PropertyViews.Last().GetProperty();

        }

        public bool AddProperty<T>(object ctrl, object data) where T : Component {
            //if controller comes in at init, it just casts it and creates View for it
            //if function is called to add new, it creates controller first, and then view afterwards
            Undo.RecordObject(Track, "AddProperty");
            T controller;
            if (ctrl == null) {
                controller = (T) Undo.AddComponent<T>(Track.gameObject);
                BaseProperty propertyController = controller as BaseProperty;
                Track.Properties.Add(propertyController);
                EditorUtility.SetDirty(Track);
            } else
                controller = (T) ctrl;
            Type viewType = Type.GetType("Outfit7.Sequencer." + controller.GetType().Name + "View", true);
            SequencerSequence.LastPropertyCreated = viewType;
            BasePropertyView instance = Activator.CreateInstance(viewType) as BasePropertyView;
            instance.Init(controller, data, this);
            PropertyViews.Add(instance);
            return true;
        }


        public override void OnDrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            if (!foldoutData.Enabled)
                return;
            float myIndent = Indent + 15f;
            float heightOffset = startHeight + 20f;
            foreach (BasePropertyView propertyView in PropertyViews) {
                var prop = propertyView.GetProperty();
                if (prop != null) {
                    Undo.RecordObject(prop, "DrawGuiPropertyChange");
                }
                heightOffset += propertyView.DrawGui(myIndent, heightOffset, parameters);
            }
            //Track.LocalPosition.Enabled = GUI.Toggle(new Rect(myIndent, startHeight + 20f, 40f, 15f), Track.LocalPosition.Enabled, "LP");
            //Track.LocalRotation.Enabled = GUI.Toggle(new Rect(myIndent + 40f, startHeight + 20f, 40f, 15f), Track.LocalRotation.Enabled, "LR");
            //Track.LocalScale.Enabled = GUI.Toggle(new Rect(myIndent + 80f, startHeight + 20f, 40f, 15f), Track.LocalScale.Enabled, "LS");
            //Track.WorldPosition.Enabled = GUI.Toggle(new Rect(myIndent, startHeight + 35f, 40f, 15f), Track.WorldPosition.Enabled, "WP");
            //Track.WorldRotation.Enabled = GUI.Toggle(new Rect(myIndent + 40f, startHeight + 35f, 40f, 15f), Track.WorldRotation.Enabled, "WR");
        }

        public void ClearUnusedProperties() {
            for (int i = 0; i < PropertyViews.Count; ++i) {
                var propertyView = PropertyViews[i];

                BaseProperty property = propertyView.GetProperty();
                bool inUse = false;
                foreach (SequencerEvent e in Track.Events) {
                    SequencerPropertyEvent propertyEvent = e as SequencerPropertyEvent;
                    if (propertyEvent != null) {
                        if (propertyEvent.Property == property) {
                            inUse = true;
                            break;
                        }
                    }

                    SequencerSetPropertyEvent setPropertyEvent = e as SequencerSetPropertyEvent;
                    if (setPropertyEvent != null) {
                        if (setPropertyEvent.Property == property) {
                            inUse = true;
                            break;
                        }
                    }
                }
                if (!inUse) {
                    PropertyViews.RemoveAt(i--);
                    Track.Properties.Remove(property);
                    Undo.DestroyObjectImmediate(property);
                }
            }
        }

        public override void OnInit(object track) {
            Track = (SequencerCurveTrack) track;
            base.OnInit(track);
            //temporary until i figure this out
            Track.FoldoutData.Height = 120f;
            foreach (BaseProperty property in Track.Properties) {
                Type t = property.GetType();
                MethodInfo method = typeof(SequencerCurveTrackView).GetMethod("AddProperty");
                method = method.MakeGenericMethod(t);
                object[] parametersArray = new object[] { property, null };
                method.Invoke(this, parametersArray);
            }
            ClearUnusedProperties();
        }

        public override void OnRecordingStart() {
            foreach (BasePropertyView propertyView in PropertyViews) {
                propertyView.RecordingStart();
            }
        }

        public override void OnRecordingStop() {
            foreach (BasePropertyView propertyView in PropertyViews) {
                propertyView.RecordingStop();
            }
        }
    }
}