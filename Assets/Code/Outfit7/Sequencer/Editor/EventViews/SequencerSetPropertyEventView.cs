using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;
using Outfit7.Util;
using Outfit7.Logic.StateMachineInternal;
using Outfit7.Logic.Util;


namespace Outfit7.Sequencer {
    [SequencerQuickSearchDisplayAttribute("Set Property")]
    [SequencerCurveTrackAttribute("Set/Property Value")]
    public class SequencerSetPropertyEventView : SequencerTriggerEventView {
        private SequencerSetPropertyEvent Event = null;
        protected SequencerCurveTrackView Parent = null;
        public PropertyAnimationDataView AnimationDataView;
        protected IEnumerable<Type> TypesEnumerator;

        public override void OnInit(object evnt, object parent) {
            Event = evnt as SequencerSetPropertyEvent;
            if (Event.AnimationData != null) {
                Type t = Event.AnimationData.GetType();
                MethodInfo method = typeof(SequencerSetPropertyEventView).GetMethod("AddAnimationData");

                method = method.MakeGenericMethod(t);
                object[] parametersArray = new object[] { Event.AnimationData, null, null };
                method.Invoke(this, parametersArray);
            }

            Assembly assembly = typeof(SequencerSetPropertyEventView).Assembly;
            TypesEnumerator = assembly.GetTypes().Where(t => typeof(BasePropertyView).IsAssignableFrom(t)).
                Where(t => Attribute.IsDefined(t, typeof(SequencerPropertyAttribute)));

            base.OnInit(evnt, parent);
        }

        public override string GetName() {
            return "Set";
        }

        public BaseProperty GetProperty() {
            return Event.Property;
        }

        public override void SetParent(SequencerTrackView parent) {
            base.SetParent(parent);
            Parent = (SequencerCurveTrackView) parent;
        }

        protected override void OnDrawEventsContextMenu(GenericMenu menu, SequencerTrackView parent, object actor) {
            Parent = (SequencerCurveTrackView) parent;

            GameObject actorGameObject = actor as GameObject;
            if (actorGameObject == null) {
                return;
            }

            if (Event.AnimationData == null) {
                foreach (Type t in TypesEnumerator) {
                    SequencerPropertyAttribute attr = (SequencerPropertyAttribute) t.GetCustomAttributes(typeof(SequencerPropertyAttribute), false)[0];
                    menu.AddItem(new GUIContent("AddProperty/" + attr.GetPath()), false, AddSingleAnimationDataCallback, t);
                }
                foreach (MonoBehaviour monoBehaviour in actorGameObject.GetComponents<MonoBehaviour>()) {
                    FieldInfo[] fields = monoBehaviour.GetType().GetFields();
                    foreach (FieldInfo field in fields) {
                        menu.AddItem(new GUIContent("AddProperty/" + monoBehaviour.GetType().Name + "/" + field.Name), false, AddCustomSingleAnimationDataCallback, new object[] {
                            typeof(CustomFieldProperty),
                            monoBehaviour.GetType(),
                            field
                        });
                    }
                }
            } else {
                foreach (Type t in TypesEnumerator) {
                    foreach (SequencerPropertyAttribute attr in t.GetCustomAttributes(typeof(SequencerPropertyAttribute), false)) {
                        menu.AddItem(new GUIContent("Replace Property/" + attr.GetPath()), false, ReplaceProperty, t);
                    }
                }
                foreach (MonoBehaviour monoBehaviour in actorGameObject.GetComponents<MonoBehaviour>()) {
                    FieldInfo[] fields = monoBehaviour.GetType().GetFields();
                    foreach (FieldInfo field in fields) {
                        menu.AddItem(new GUIContent("Replace Property/" + monoBehaviour.GetType().Name + "/" + field.Name), false, CustomReplaceProperty, new object[] {
                            typeof(CustomFieldProperty),
                            monoBehaviour.GetType(),
                            field
                        });
                    }
                }
            }

            if (Event.AnimationData != null && !(Event.AnimationData is PropertyAspectRatioAnimationData)) {
                menu.AddItem(new GUIContent("Aspect Ratio Specific Animation"), false, ConvertToAspectRatioAnimationData);
            }
        }

        public override void OnSelectionChanged(bool selected) {
            base.OnSelectionChanged(selected);
            if (selected) {
                if (Event.Property != null)
                    Event.Property.hideFlags = HideFlags.None;
                if (Event.AnimationData)
                    Event.AnimationData.hideFlags = HideFlags.None;
            } else {
                if (Event.Property != null)
                    Event.Property.hideFlags = HideFlags.HideInInspector;
                if (Event.AnimationData)
                    Event.AnimationData.hideFlags = HideFlags.HideInInspector;
            }
        }

        public void AddCustomSingleAnimationDataCallback(object data) {
            object[] dataArray = (object[]) data;
            Type t = (Type) dataArray[0];
            Type componentType = (Type) dataArray[1];
            AddAnimationData<PropertySingleAnimationData>(null, t, new object[] { componentType, dataArray[2] });
        }

        public void AddSingleAnimationDataCallback(object t) {
            Type viewType = (Type) t;
            Assembly assembly = typeof(SequencerSetPropertyEvent).Assembly;
            Type eType = Type.GetType("Outfit7.Sequencer." + viewType.Name.Replace("View", "") + ", " + assembly.GetName(), true);
            AddAnimationData<PropertySingleAnimationData>(null, eType, null);
        }


        public void ConvertToAspectRatioAnimationData() {
            BaseProperty property = Event.Property;
            PropertyAnimationData baseAnimData = Event.AnimationData;
            AddAnimationData<PropertyAspectRatioAnimationData>(null, property.GetType(), null);
            PropertyAspectRatioAnimationDataView animDataView = AnimationDataView as PropertyAspectRatioAnimationDataView;
            animDataView.AddAnimationData(baseAnimData, AnimationDataView, 1.5f); //3:2
            animDataView.ParentEvent = Event;
        }

        public void CustomReplaceProperty(object data) {
            object[] dataArray = (object[]) data;
            Type viewType = (Type) dataArray[0];
            Assembly assembly = typeof(SequencerPropertyEvent).Assembly;
            Type eType = Type.GetType("Outfit7.Sequencer." + viewType.Name.Replace("View", "") + ", " + assembly.GetName(), true);

            Type componentType = (Type) dataArray[1];

            MethodInfo method = typeof(SequencerCurveTrackView).GetMethod("GetProperty");
            method = method.MakeGenericMethod(eType);
            BaseProperty property = (BaseProperty) method.Invoke(Parent, new object[] { componentType, dataArray[2] });
            Event.Property = property;
        }

        public void ReplaceProperty(object t) {
            Type viewType = (Type) t;
            Assembly assembly = typeof(SequencerPropertyEvent).Assembly;
            Type eType = Type.GetType("Outfit7.Sequencer." + viewType.Name.Replace("View", "") + ", " + assembly.GetName(), true);

            MethodInfo method = typeof(SequencerCurveTrackView).GetMethod("GetProperty");
            method = method.MakeGenericMethod(eType);
            BaseProperty property = (BaseProperty) method.Invoke(Parent, new object[] { null });
            Event.Property = property;
        }

        public virtual bool AddAnimationData<T>(object ctrl, object t, object data) where T : Component {
            //if controller comes in at init, it just casts it and creates View for it
            //if function is called to add new, it creates controller first, and then view afterwards
            Undo.RecordObject(Event, "AddAnimationData");
            T controller;
            Type propertyType = (Type) t;
            if (ctrl == null) {
                controller = (T) Undo.AddComponent<T>(Event.gameObject);
                PropertyAnimationData animationData = controller as PropertyAnimationData;
                MethodInfo method = typeof(SequencerCurveTrackView).GetMethod("GetProperty");
                method = method.MakeGenericMethod(propertyType);
                BaseProperty property = (BaseProperty) method.Invoke(Parent, new object[] { data });
                Event.Property = property;
                bool success;
                animationData.SetStartingValues(property.Value(out success));
                Event.AnimationData = animationData;
            } else
                controller = (T) ctrl;
            Type viewType = Type.GetType("Outfit7.Sequencer." + controller.GetType().Name + "View", true);
            PropertyAnimationDataView instance = Activator.CreateInstance(viewType) as PropertyAnimationDataView;
            instance.Init(controller);
            AnimationDataView = instance;
            AnimationDataView.ParentEvent = Event;
            return true;
        }

        public override void OnDrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
        }

        public override void OnDrawOptimizedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            if (Event.Property.GetDisplayMode() == BaseProperty.DisplayMode.CURVE) {
                bool hasConditions = Event.Conditions.Count > 0;
                GUIUtil.DrawLine(new Vector2(EventRect.center.x, EventRect.y), new Vector2(EventRect.center.x, EventRect.y + EventRect.height), hasConditions ? LineConditionColor : LineColor);
                string stringValue = "";
                bool[] valUsed = Event.Property.GetValuesUsed();
                Vector4 val = AnimationDataView.GetSingleValue();
                if (valUsed[0])
                    stringValue += val.x + ",";
                if (valUsed[1])
                    stringValue += val.y + ",";
                if (valUsed[2])
                    stringValue += val.z + ",";
                if (valUsed[3])
                    stringValue += val.w + ",";
                stringValue = stringValue.TrimEnd(',');
                EditorGUI.LabelField(new Rect(EventRect.x + 6, EventRect.y, 100, EventRect.height), stringValue);
            } else {
                Vector4 value = AnimationDataView.GetSingleValue();
                EditorGUI.DrawRect(new Rect(EventRect.x + 5, EventRect.y, EventRect.width - 5, EventRect.height), new Color(value.x, value.y, value.z));
                EditorGUI.DrawRect(new Rect(EventRect.x + 5, EventRect.y + EventRect.height - 3, EventRect.width - 5, 3), new Color(value.w, value.w, value.w));
            }
            AnimationDataView.DrawGui(Event.Property, GetRectPosition(20f, 40f, 75f), parameters, true, EventRect.Contains(UnityEngine.Event.current.mousePosition) || Selected);
        }

        public override void OnDrawExtendedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawExtendedGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);

            GUI.Label(GetRectPosition(20f, 30f, 20f), GetName());
            if (AnimationDataView == null || Event.Property == null)
                return;
            Undo.RecordObject(AnimationDataView.GetAnimData(), "AnimDataChange");
            AnimationDataView.DrawGui(Event.Property, GetRectPosition(40f, 40f, 75f), parameters, false);
        }

        protected override void OnDestroy() {
            bool animDataInUse = false;
            foreach (SequencerEvent e in Event.gameObject.GetComponents<SequencerEvent>()) {
                SequencerPropertyEvent propertyEvent = e as SequencerPropertyEvent;
                if (propertyEvent == null)
                    continue;
                if (propertyEvent == Event)
                    continue;
                if (propertyEvent.AnimationData == Event.AnimationData) {
                    animDataInUse = true;
                    break;
                }
            }
            if (!animDataInUse && Event.AnimationData != null)
                Undo.DestroyObjectImmediate(Event.AnimationData);
        }

        public override void OnUpdateWhileRecording(float currentTime) {
            if (AnimationDataView == null || Event.Property == null)
                return;
            if (Mathf.Abs(currentTime - Event.StartTime) < 0.0001f) {
                AnimationDataView.OnUpdateWhileRecording(Event.Property, 0, 0, 0, 1, 0, false, 0, 1);
            }
        }
    }
}
