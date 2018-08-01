using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
using System.Linq;
using System;
using Outfit7.Util;
using Outfit7.Logic.StateMachineInternal;


namespace Outfit7.Sequencer {

    [SequencerQuickSearchAttribute("Property")]
    [SequencerQuickSearchDisplayAttribute("Curve-Tween")]
    [SequencerCurveTrackAttribute("Animation/Curve-Tween")]
    public class SequencerPropertyEventView : SequencerContinuousEventView {
        private enum AnimTypesEnum {
            CURVE,
            TWEEN
        }

        private SequencerPropertyEvent Event = null;
        protected SequencerCurveTrackView Parent = null;
        public PropertyAnimationDataView AnimationDataView;
        protected IEnumerable<Type> TypesEnumerator;
        private SequencerQuickSearch QuickSearch = null;
        private AnimTypesEnum QuickSearchType = AnimTypesEnum.TWEEN;
        private string Text = "Curve-Tween";

        public override void OnInit(object evnt, object parent) {
            Event = evnt as SequencerPropertyEvent;
            if (Event.AnimationData != null) {
                Type t = Event.AnimationData.GetType();
                MethodInfo method = typeof(SequencerPropertyEventView).GetMethod("AddAnimationData");

                method = method.MakeGenericMethod(t);
                object[] parametersArray = new object[] { Event.AnimationData, null, null };
                method.Invoke(this, parametersArray);
            }

            Assembly assembly = typeof(SequencerPropertyEventView).Assembly;
            TypesEnumerator = assembly.GetTypes().Where(t => typeof(BasePropertyView).IsAssignableFrom(t)).
                Where(t => Attribute.IsDefined(t, typeof(SequencerPropertyAttribute)));
            
            base.OnInit(evnt, parent);
        }

        public override string GetName() {
            return Text;
        }

        public BaseProperty GetProperty() {
            return Event.Property;
        }

        public override void SetParent(SequencerTrackView parent) {
            base.SetParent(parent);
            Parent = (SequencerCurveTrackView) parent;
        }

        protected override void OnDrawEventsContextMenu(GenericMenu menu, SequencerTrackView parent, object actor) {
            GameObject actorGameObject = actor as GameObject;
            if (actorGameObject == null) {
                return;
            }

            int id;
            string[] copyBuffer = EditorGUIUtility.systemCopyBuffer.Split('|');
            if (copyBuffer.Length == 2 && Int32.TryParse(copyBuffer[0], out id)) {
                menu.AddItem(new GUIContent("Paste AnimationData"), false, PasteAnimationData);
            }

            if (Event.AnimationData == null) {
                foreach (Type t in TypesEnumerator) {
                    foreach (SequencerPropertyAttribute attr in t.GetCustomAttributes(typeof(SequencerPropertyAttribute), false)) {
                        menu.AddItem(new GUIContent("Add Property/" + attr.GetPath() + "/Curve"), false, AddCurveAnimationDataCallback, t);
                        menu.AddItem(new GUIContent("Add Property/" + attr.GetPath() + "/Tween"), false, AddTweenAnimationDataCallback, t);
                    }
                }
                foreach (MonoBehaviour monoBehaviour in actorGameObject.GetComponents<MonoBehaviour>()) {
                    FieldInfo[] fields = monoBehaviour.GetType().GetFields();
                    foreach (FieldInfo field in fields) {
                        menu.AddItem(new GUIContent("Add Property/" + monoBehaviour.GetType().Name + "/" + field.Name + "/Curve"), false, AddCustomCurveAnimationDataCallback, new object[] {
                            typeof(CustomFieldProperty),
                            monoBehaviour.GetType(),
                            field
                        });
                        menu.AddItem(new GUIContent("Add Property/" + monoBehaviour.GetType().Name + "/" + field.Name + "/Tween"), false, AddCustomTweenAnimationDataCallback, new object[] {
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

            if (Event.AnimationData != null) {
                menu.AddItem(new GUIContent("Copy AnimationData"), false, CopyAnimationData);
            }

            if (Event.AnimationData != null) {
                menu.AddItem(new GUIContent("Remap"), Event.DoRemap, RemapSwitch);
            }

            if (Event.AnimationData != null && !(Event.AnimationData is PropertyAspectRatioAnimationData)) {
                menu.AddItem(new GUIContent("Aspect Ratio Specific Animation"), false, ConvertToAspectRatioAnimationData);
            }

            if (AnimationDataView != null) {
                AnimationDataView.OnContextMenu(menu);
            }
        }

        public void RemapSwitch() {
            Event.DoRemap = !Event.DoRemap;
        }

        public override void OnSelectionChanged(bool selected) {
            base.OnSelectionChanged(selected);
            if (Event.AnimationData != null) {
                if (selected) {
                    if (Event.Property != null)
                        Event.Property.hideFlags = HideFlags.None;
                    Event.AnimationData.hideFlags = HideFlags.None;
                } else {
                    if (Event.Property != null)
                        Event.Property.hideFlags = HideFlags.HideInInspector;
                    Event.AnimationData.hideFlags = HideFlags.HideInInspector;
                }
            }
        }

        public void CopyAnimationData() {
            EditorGUIUtility.systemCopyBuffer = Event.AnimationData.GetInstanceID().ToString() + "|" + Event.Property.GetType().FullName;
        }

        public void PasteAnimationData() {
            int id;
            string[] copyBuffer = EditorGUIUtility.systemCopyBuffer.Split('|');
            if (copyBuffer.Length != 2) {
                Debug.LogError("You are not pasting animation data");
                return;
            }
            bool success = Int32.TryParse(copyBuffer[0], out id);
            if (!success) {
                Debug.LogError("You are not pasting animation data");
                return;
            }
            PropertyAnimationData animData = EditorUtility.InstanceIDToObject(id) as PropertyAnimationData;
            if (animData == null) {
                Debug.LogError("You are not pasting animation data");
                return;
            }
            if (Event.AnimationData != null)
                OnDestroy();
            Event.AnimationData = animData;
            Type t = animData.GetType();
            Assembly assembly = typeof(SequencerPropertyEvent).Assembly;
            Type propertyType = Type.GetType(copyBuffer[1] + ", " + assembly.GetName(), true);
            MethodInfo method = typeof(SequencerPropertyEventView).GetMethod("AddAnimationData");
            method = method.MakeGenericMethod(t);
            object[] parametersArray = new object[] { animData, propertyType, null };
            method.Invoke(this, parametersArray);
        }

        public void AddCustomCurveAnimationDataCallback(object data) {
            object[] dataArray = (object[]) data;
            Type t = (Type) dataArray[0];
            Type componentType = (Type) dataArray[1];
            AddAnimationData<PropertyCurveAnimationData>(null, t, new object[] { componentType, dataArray[2] });
        }

        public void AddCustomTweenAnimationDataCallback(object data) {
            object[] dataArray = (object[]) data;
            Type t = (Type) dataArray[0];
            Type componentType = (Type) dataArray[1];
            AddAnimationData<PropertyTweenAnimationData>(null, t, new object[] { componentType, dataArray[2] });
        }

        public void AddCurveAnimationDataCallback(object t) {
            Type viewType = (Type) t;
            Assembly assembly = typeof(SequencerPropertyEvent).Assembly;
            Type eType = Type.GetType("Outfit7.Sequencer." + viewType.Name.Replace("View", "") + ", " + assembly.GetName(), true);
            AddAnimationData<PropertyCurveAnimationData>(null, eType, null);
        }

        public void AddTweenAnimationDataCallback(object t) {
            Type viewType = (Type) t;
            Assembly assembly = typeof(SequencerPropertyEvent).Assembly;
            Type eType = Type.GetType("Outfit7.Sequencer." + viewType.Name.Replace("View", "") + ", " + assembly.GetName(), true);
            AddAnimationData<PropertyTweenAnimationData>(null, eType, null);
        }

        public void ConvertToAspectRatioAnimationData() {
            //save current property
            BaseProperty property = Event.Property;
            //save curent anim data
            PropertyAnimationData baseAnimData = Event.AnimationData;
            PropertyAnimationDataView baseAnimDataView = AnimationDataView;

            AddAnimationData<PropertyAspectRatioAnimationData>(null, property.GetType(), null);
            PropertyAspectRatioAnimationDataView animDataView = AnimationDataView as PropertyAspectRatioAnimationDataView;
            animDataView.AddAnimationData(baseAnimData, baseAnimDataView, 1.5f); //3:2
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
            BaseProperty property = (BaseProperty) method.Invoke(Parent, new object[]{ null });
            Event.Property = property;
            SetTextFromProperty();
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
                BaseProperty property = (BaseProperty) method.Invoke(Parent, new object[]{ data });
                Event.Property = property;
                bool success;
                animationData.SetStartingValues(property.Value(out success));
                Event.AnimationData = animationData;
            } else if (ctrl != null && t != null) { //this happens in paste anim data case
                controller = (T) ctrl;
                MethodInfo method = typeof(SequencerCurveTrackView).GetMethod("GetProperty");
                method = method.MakeGenericMethod(propertyType);
                BaseProperty property = (BaseProperty) method.Invoke(Parent, new object[]{ data });
                Event.Property = property;
            } else
                controller = (T) ctrl;
            Type viewType = Type.GetType("Outfit7.Sequencer." + controller.GetType().Name + "View", true);
            PropertyAnimationDataView instance = Activator.CreateInstance(viewType) as PropertyAnimationDataView;
            instance.Init(controller);
            AnimationDataView = instance;
            AnimationDataView.ParentEvent = Event;
            SetTextFromProperty();
            return true;
        }

        private void SetTextFromProperty() {
            if (Event.Property == null)
                return;
            Type type = Event.Property.GetType();

            if (type.Name == "CustomFieldProperty") {
                Text = "Custom";
                return;
            }

            Assembly assembly = typeof(SequencerPropertyEventView).Assembly;
            Type eType = Type.GetType("Outfit7.Sequencer." + type.Name + "View" + ", " + assembly.GetName(), true);
            SequencerPropertyAttribute attr = eType.GetCustomAttributes(typeof(SequencerPropertyAttribute), false)[0] as SequencerPropertyAttribute;
            string[] split = attr.GetPath().Split('/');
            Text = split[split.Length - 1];

        }

        public override void OnDrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
            if (QuickSearch != null) {
                OnDrawQuickSearch();
            }
        }

        public override void OnDrawOptimizedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            SetupGui();
            bool hasConditions = Event.Conditions.Count > 0;
            EditorGUI.LabelField(TopPartRect, "", Selected ? (hasConditions ? NodeConditionSelected : NodeSelected) : (hasConditions ? NodeCondition : Node));
            Rect CurveRect = EventRect;
            DrawData(CurveRect, parameters, true);
        }

        public override void OnDrawExtendedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawExtendedGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
            Rect CurveRect = new Rect(EventRect.x, EventRect.y + 20f, EventRect.width, EventRect.height - 20f);
            DrawData(CurveRect, parameters);

            float halfWidth = Mathf.Min(200f, CurveRect.width) / 8;
            EditorGUI.LabelField(new Rect(CurveRect.xMax - halfWidth * 4, CurveRect.yMax - 15f, halfWidth, 15f), "Mult");
            Event.Multiplier = EditorGUI.FloatField(new Rect(CurveRect.xMax - halfWidth * 3, CurveRect.yMax - 15f, halfWidth, 15f), Event.Multiplier);
            EditorGUI.LabelField(new Rect(CurveRect.xMax - halfWidth * 2, CurveRect.yMax - 15f, halfWidth, 15f), "Offset");
            Event.Offset = EditorGUI.FloatField(new Rect(CurveRect.xMax - halfWidth * 1, CurveRect.yMax - 15f, halfWidth, 15f), Event.Offset);

            if (Event.DoRemap) {
                EditorGUI.LabelField(new Rect(CurveRect.xMin + halfWidth * 0, CurveRect.yMax - 15f, halfWidth, 15f), "Val0");
                Event.Remap0 = EditorGUI.FloatField(new Rect(CurveRect.xMin + halfWidth * 1, CurveRect.yMax - 15f, halfWidth, 15f), Event.Remap0);
                EditorGUI.LabelField(new Rect(CurveRect.xMin + halfWidth * 2, CurveRect.yMax - 15f, halfWidth, 15f), "Val1");
                Event.Remap1 = EditorGUI.FloatField(new Rect(CurveRect.xMin + halfWidth * 3, CurveRect.yMax - 15f, halfWidth, 15f), Event.Remap1);
            }
        }

        protected override void OnDraggingEnd() {
            SequencerSequence sequence = Event.gameObject.GetComponent<SequencerSequence>();
            if (sequence != null) {
                Event.Evaluate(0, sequence.GetCurrentTime());
            }
        }

        public virtual void DrawData(Rect curveRect, List<Parameter> parameters, bool optimizedView = false) {
            if (AnimationDataView == null || Event.Property == null)
                return;

            Undo.RecordObject(AnimationDataView.GetAnimData(), "AnimDataChange");
            AnimationDataView.SetData(Event.Repeat, Event.Bounce);
            AnimationDataView.DrawGui(Event.Property, curveRect, parameters, optimizedView, EventRect.Contains(UnityEngine.Event.current.mousePosition) || Selected);
        }

        public override void OnUpdateWhileRecording(float currentTime) {
            if (AnimationDataView == null || Event.Property == null)
                return;
            if (Event.Active) {
                float normalizedTime = Event.GetNormalizedTime(currentTime);
                AnimationDataView.OnUpdateWhileRecording(Event.Property, currentTime - Event.StartTime, Event.Duration, normalizedTime, Event.Multiplier, Event.Offset, Event.DoRemap, Event.Remap0, Event.Remap1);
            }
        }

        protected override bool OnHandleInput(TimelineData timelineData, SequencerSequenceView sequenceView, Rect timelineTrackRect, int highiestEventTrackIndex, object actor) {
            if (AnimationDataView != null && Event.Property != null && InputState == ContinousEventInputState.INACTIVE) {
                if (AnimationDataView.OnHandleInput(Event.Property, timelineData, sequenceView, timelineTrackRect, highiestEventTrackIndex, actor))
                    return true;
            }
            if (EventRect.Contains(UnityEngine.Event.current.mousePosition) &&
                UnityEngine.Event.current.type == EventType.keyDown &&
                UnityEngine.Event.current.keyCode == KeyCode.T) {
                DrawQuickSearch(AnimTypesEnum.TWEEN);
                return true;
            } else if (EventRect.Contains(UnityEngine.Event.current.mousePosition) &&
                       UnityEngine.Event.current.type == EventType.keyDown &&
                       UnityEngine.Event.current.keyCode == KeyCode.C) {
                DrawQuickSearch(AnimTypesEnum.CURVE);
                return true;
            }
            return base.OnHandleInput(timelineData, sequenceView, timelineTrackRect, highiestEventTrackIndex, actor);
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

        bool firstUpdate = false;

        private void DrawQuickSearch(AnimTypesEnum type) {
            QuickSearchType = type;
            QuickSearch = new SequencerQuickSearch(UnityEngine.Event.current.mousePosition, TypesEnumerator, SequencerSequence.LastPropertyCreated);
            firstUpdate = true;
        }

        private void OnDrawQuickSearch() {
            Type typeToCreate;
            if (QuickSearch.OnGui(out typeToCreate)) {
                QuickSearch = null;
                if (typeToCreate != null) {
                    switch (QuickSearchType) {
                        case AnimTypesEnum.CURVE:
                            AddCurveAnimationDataCallback(typeToCreate);
                            break;
                        case AnimTypesEnum.TWEEN:
                            AddTweenAnimationDataCallback(typeToCreate);
                            break;
                    }
                }
                if (firstUpdate) {
                    if (QuickSearch != null)
                        QuickSearch.searchLine = "";
                    firstUpdate = false;
                }
            }
        }
    }
}
