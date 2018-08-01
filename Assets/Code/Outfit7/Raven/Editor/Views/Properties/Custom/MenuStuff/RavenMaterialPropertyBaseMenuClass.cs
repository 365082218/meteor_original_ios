using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UI;

namespace Starlite.Raven {

    internal static class RavenMaterialPropertyBaseMenuClass {

        [InitializeOnLoadMethod]
        [DidReloadScripts(1)]
        private static void RegisterCustomPropertyTrackViewCallback() {
            RavenEditorCallbacks.e_GenerateCustomDataForPropertyEventMenus -= GeneratePropertyTrackMenuData;
            RavenEditorCallbacks.e_GenerateCustomDataForPropertyEventMenus += GeneratePropertyTrackMenuData;
        }

        private static void GeneratePropertyTrackMenuData(RavenPropertyTrackView arg1, List<RavenPropertyTrackView.PropertyEventMenuData> arg2, List<RavenPropertyTrackView.AnimationDataType> arg3) {
            if (arg1.Target == null) {
                return;
            }

            var renderer = arg1.Target.GetComponent<Renderer>();
            var graphicRenderer = arg1.Target.GetComponent<Graphic>();

            if (renderer != null || graphicRenderer != null) {
                var properties = arg1.GetValidProperties(typeof(RavenMaterialPropertyBase<>));
                object realRenderer = renderer != null ? (object)renderer : graphicRenderer;
                arg1.GenerateDataForPropertyEventMenus(arg2, properties, arg3, "Custom/", realRenderer, MaterialPropertyFilter);
            }
        }

        private static IEnumerable<RavenPropertyTrackView.ClassMemberInfo> MaterialPropertyFilter(List<RavenPropertyTrackView.AnimationPropertyType> properties, RavenPropertyTrackView.AnimationDataType animationData, Dictionary<Type, Type> propertyTypeCacheDict, object userData) {
            Type propertyType;
            if (!propertyTypeCacheDict.TryGetValue(animationData.m_ArgumentType, out propertyType)) {
                var property = properties.Find((y) => y.m_ArgumentType == animationData.m_ArgumentType);
                propertyType = property == null ? null : property.m_Type;
                propertyTypeCacheDict[animationData.m_ArgumentType] = propertyType;
            }

            if (propertyType != null) {
                return new RavenPropertyTrackView.ClassMemberInfo[]
                    {
                        new RavenPropertyTrackView.ClassMemberInfo() {
                            m_Name = GetMaterialClassMemberName(animationData, userData),
                            m_PropertyType = propertyType,
                            m_AnimationDataType = animationData.m_Type,
                            m_ArgumentType = animationData.m_ArgumentType
                        }
                    };
            }
            return null;
        }

        private static string GetMaterialClassMemberName(RavenPropertyTrackView.AnimationDataType animationDataType, object userData) {
            var arg = animationDataType.m_BaseType.GetGenericArguments()[0];
            return userData.GetType().ToString() + "|Material/" + arg.Name;
        }
    }
}