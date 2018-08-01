using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Starlite.Raven {

    public sealed class RavenPropertyTrackView : RavenTrackView {

        public class AnimationPropertyType {
            public Type m_Type;
            public Type m_ArgumentType;
            public Type m_BaseType;
        }

        public class AnimationDataType {
            public Type m_Type;
            public Type m_ArgumentType;
            public Type m_BaseType;
        }

        public class PropertyEventMenuData {
            public string m_Entry;
            public Type m_PropertyType;
            public Type m_AnimationDataType;
            public Type m_ArgumentType;

            public string m_ComponentType;
            public string m_MemberName;
        }

        public class ClassMemberInfo {
            public string m_Name;
            public Type m_PropertyType;
            public Type m_AnimationDataType;
            public Type m_ArgumentType;
        }

        private RavenPropertyTrack m_PropertyTrack = null;
        private List<PropertyEventMenuData> m_PropertyMenuData;
        private List<PropertyEventMenuData> m_FunctionMenuData;

        private static Regex s_AnimationDataNameReplacementRegex = new Regex("RavenAnimationData(.+?)`.*");

        public override string Name {
            get {
                return "Property Track";
            }
        }

        public override int TrackIndex {
            get {
                return m_Parent.TrackGroup.PropertyTrackIndex;
            }
        }

        public List<PropertyEventMenuData> PropertyMenuData {
            get {
                return m_PropertyMenuData;
            }
        }

        public override void Initialize(RavenTrack track, RavenTrackGroupView parent) {
            base.Initialize(track, parent);
            m_PropertyTrack = track as RavenPropertyTrack;
            m_PropertyTrack.FoldoutHeight = 120f;

            m_PropertyMenuData = GenerateDataForPropertyEventMenus();
            m_FunctionMenuData = GenerateDataForFunctionEventMenus();
        }

        public override void OnTargetChanged(GameObject target) {
            for (int i = 0; i < m_PropertyTrack.Events.Count; ++i) {
                var evnt = m_PropertyTrack.Events[i];
                Undo.RecordObject(evnt, "Target Change");
                evnt.SetTargetEditor(RavenSequenceEditor.Instance.Sequence, target);
            }
            m_PropertyMenuData = GenerateDataForPropertyEventMenus();
            m_FunctionMenuData = GenerateDataForFunctionEventMenus();
        }

        public List<AnimationPropertyType> GetValidProperties(string genericType) {
            var validProperties = new List<AnimationPropertyType>();

            var animationProperties = RavenUtility.GetFinalTypesForGenericType(genericType, 1);
            GetValidPropertiesInternal(validProperties, animationProperties);
            return validProperties;
        }

        public List<AnimationPropertyType> GetValidProperties(Type genericType) {
            var validProperties = new List<AnimationPropertyType>();

            var animationProperties = RavenUtility.GetFinalTypesForGenericType(genericType);
            GetValidPropertiesInternal(validProperties, animationProperties);
            return validProperties;
        }

        protected override void OnDestroyTrack() {
        }

        protected override void OnDrawTrackContextMenu(GenericMenu menu) {
        }

        protected override void OnDrawSidebarGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, bool foldoutEnabled, float foldoutHeight, List<RavenParameter> parameters) {
        }

        protected override void OnRecordingStart() {
        }

        protected override void OnRecordingStop() {
        }

        protected override void OnDrawEventsContextMenu(GenericMenu menu) {
            menu.AddItem(new GUIContent("Function Call"), false, AddEvent<RavenCallFunctionEvent>, null);
            menu.AddItem(new GUIContent("Set Property"), false, AddEvent<RavenPropertyEvent>, null);
            menu.AddItem(new GUIContent("Goto"), false, AddEvent<RavenGoToEvent>, null);
            menu.AddItem(new GUIContent("Function Call Continuous"), false, AddEvent<RavenCallFunctionContinuousEvent>, null);
            if (RavenEditorCallbacks.e_GenerateCustomEventsForPropertyTrackContextMenu != null) {
                RavenEditorCallbacks.e_GenerateCustomEventsForPropertyTrackContextMenu(menu, this);
            }
        }

        protected override bool OnHandleInput(Vector2 mousePosition, TimelineData timelineData, RavenSequenceView sequenceView, bool optimizedView) {
            if (base.OnHandleInput(mousePosition, timelineData, sequenceView, optimizedView)) {
                return true;
            }

            if (m_TimelineTrackRect.Contains(mousePosition)) {
                if (UnityEngine.Event.current.type == EventType.KeyDown) {
                    if (UnityEngine.Event.current.keyCode == KeyCode.T) {
                        if (!CalculateAndSetAddEventPosition(mousePosition, timelineData)) {
                            return false;
                        }
                        return sequenceView.GenerateQuickSearchForAnimationProperty(ERavenAnimationDataFilter.Tween, m_PropertyMenuData, QuickSearchAnimationCallback);
                    } else if (UnityEngine.Event.current.keyCode == KeyCode.C) {
                        if (!CalculateAndSetAddEventPosition(mousePosition, timelineData)) {
                            return false;
                        }
                        return sequenceView.GenerateQuickSearchForAnimationProperty(ERavenAnimationDataFilter.Curve, m_PropertyMenuData, QuickSearchAnimationCallback);
                    } else if (UnityEngine.Event.current.keyCode == KeyCode.S) {
                        if (!CalculateAndSetAddEventPosition(mousePosition, timelineData)) {
                            return false;
                        }
                        return sequenceView.GenerateQuickSearchForAnimationProperty(ERavenAnimationDataFilter.Set, m_PropertyMenuData, QuickSearchAnimationCallback);
                    } else if (UnityEngine.Event.current.keyCode == KeyCode.D) {
                        if (!CalculateAndSetAddEventPosition(mousePosition, timelineData)) {
                            return false;
                        }
                        return sequenceView.GenerateQuickSearchForFunctionProperty(m_FunctionMenuData, QuickSearchFunctionCallback, null);
                    }
                }
            }

            return false;
        }

        private void QuickSearchAnimationCallback(object obj) {
            RavenEventView eventView;
            AddEvent<RavenPropertyEvent>(null, out eventView);
            var propertyEventView = eventView as RavenPropertyEventView;
            propertyEventView.GeneratePropertyMenuCallback(obj);
        }

        private void QuickSearchFunctionCallback(object obj) {
            RavenEventView eventView;
            AddEvent<RavenCallFunctionEvent>(null, out eventView);

            RavenTriggerPropertyComponentBase triggerProperty;
            RavenFunctionCallEditor.GenerateFunctionCallProperty((obj as PropertyEventMenuData).m_MemberName, Target, eventView.Event, true, out triggerProperty);
        }

        private List<PropertyEventMenuData> GenerateDataForFunctionEventMenus() {
            if (Target == null) {
                return new List<PropertyEventMenuData>();
            }

            var data = new List<PropertyEventMenuData>(RavenEditorUtility.GetFunctionList(Target, (Type[])null)
                .Select((x) => {
                    var split = x.Split('|');
                    var prettyName = RavenUtility.GetTypeWithoutNamespace(split[0]) + "." + split[1] + "(" + split[2] + ")";
                    return new PropertyEventMenuData() {
                        m_Entry = prettyName,
                        m_MemberName = x,
                    };
                }));

            return data;
        }

        private void GetValidPropertiesInternal(List<AnimationPropertyType> validProperties, List<Type> finalTypes) {
            for (int i = 0; i < finalTypes.Count; ++i) {
                var type = finalTypes[i];
                validProperties.Add(new AnimationPropertyType() {
                    m_Type = type,
                    m_ArgumentType = type.BaseType.GetGenericArguments()[0],
                    m_BaseType = type.BaseType
                });
            }
        }

        private List<AnimationDataType> GetValidAnimationData() {
            var validAnimationData = new List<AnimationDataType>();

            var animationDatas = RavenUtility.GetFinalTypesForGenericType(typeof(RavenAnimationDataBase<>), false);
            for (int i = 0; i < animationDatas.Count; ++i) {
                var type = animationDatas[i];

                // ignore specialized types which are handled on instantiate
                if (!type.BaseType.IsGenericType) {
                    continue;
                }
                validAnimationData.Add(new AnimationDataType() {
                    m_Type = type,
                    m_ArgumentType = type.BaseType.GetGenericArguments()[0],
                    m_BaseType = type.BaseType
                });
            }

            return validAnimationData;
        }

        private List<PropertyEventMenuData> GenerateDataForPropertyEventMenus() {
            var propertyData = new List<PropertyEventMenuData>();

            var animationDatas = GetValidAnimationData();

            // generic stuff
            if (Target != null) {
                var properties = GetValidProperties(typeof(RavenAnimationDataPropertyBase<>));
                GenerateDataForPropertyEventMenus(propertyData, properties, animationDatas, "", null, GenericPropertyFilter);
            }
            // custom
            GenerateCustomDataForPropertyEventMenus(propertyData, animationDatas);
            return propertyData;
        }

        private void GenerateCustomDataForPropertyEventMenus(List<PropertyEventMenuData> menuData, List<AnimationDataType> validAnimationData) {
            if (RavenEditorCallbacks.e_GenerateCustomDataForPropertyEventMenus != null) {
                RavenEditorCallbacks.e_GenerateCustomDataForPropertyEventMenus(this, menuData, validAnimationData);
            }
        }

        public void GenerateDataForPropertyEventMenus(List<PropertyEventMenuData> menuData, List<AnimationPropertyType> properties, List<AnimationDataType> animationDatas, string menuPrefix, object userData, Func<List<AnimationPropertyType>, AnimationDataType, Dictionary<Type, Type>, object, IEnumerable<ClassMemberInfo>> filter) {
            var animationDataTypeDict = new Dictionary<string, List<ClassMemberInfo>>(properties.Count);
            var propertyTypeCacheDict = new Dictionary<Type, Type>(properties.Count);

            for (int i = 0; i < animationDatas.Count; ++i) {
                var animationData = animationDatas[i];
                var animationDataTypeName = GetTypeNameFromAnimationData(animationData);
                List<ClassMemberInfo> animationDataTargets;
                if (!animationDataTypeDict.TryGetValue(animationDataTypeName, out animationDataTargets)) {
                    animationDataTargets = new List<ClassMemberInfo>();
                    animationDataTypeDict[animationDataTypeName] = animationDataTargets;
                }

                var newTargets = filter(properties, animationData, propertyTypeCacheDict, userData);
                if (newTargets != null) {
                    animationDataTargets.AddRange(newTargets);
                }
            }

            foreach (var kvp in animationDataTypeDict) {
                var members = kvp.Value;
                members.Sort((x, y) => x.m_Name.CompareTo(y.m_Name));
                for (int i = 0; i < members.Count; ++i) {
                    var target = members[i];
                    var componentType = RavenUtility.GetComponentNameFromFullMemberName(target.m_Name);
                    var memberName = RavenUtility.GetMemberNameFromFullMemberName(target.m_Name);
                    menuData.Add(new PropertyEventMenuData() {
                        m_Entry = menuPrefix + kvp.Key + "/" + RavenUtility.GetTypeWithoutNamespace(componentType) + "/" + memberName,
                        m_AnimationDataType = target.m_AnimationDataType,
                        m_PropertyType = target.m_PropertyType,
                        m_ArgumentType = target.m_ArgumentType,
                        m_ComponentType = componentType,
                        m_MemberName = memberName
                    });
                }
            }
        }

        private IEnumerable<ClassMemberInfo> GenericPropertyFilter(List<AnimationPropertyType> properties, AnimationDataType animationData, Dictionary<Type, Type> propertyTypeCacheDict, object userData) {
            Type propertyType;
            if (!propertyTypeCacheDict.TryGetValue(animationData.m_ArgumentType, out propertyType)) {
                var property = properties.Find((y) => y.m_ArgumentType == animationData.m_ArgumentType);
                propertyType = property == null ? null : property.m_Type;
                propertyTypeCacheDict[animationData.m_ArgumentType] = propertyType;
            }

            if (propertyType == null) {
                return null;
            }

            return RavenEditorUtility.GetMemberList(animationData.m_ArgumentType, Target)
                .Select((x) => {
                    return new ClassMemberInfo() {
                        m_Name = x,
                        m_PropertyType = propertyType,
                        m_AnimationDataType = animationData.m_Type,
                        m_ArgumentType = animationData.m_ArgumentType
                    };
                });
        }

        private string GetTypeNameFromAnimationData(AnimationDataType animationDataType) {
            return s_AnimationDataNameReplacementRegex.Replace(animationDataType.m_BaseType.Name, "$1");
        }
    }
}