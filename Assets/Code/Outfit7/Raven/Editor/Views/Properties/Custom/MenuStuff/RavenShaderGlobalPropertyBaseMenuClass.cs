using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Starlite.Raven {

    internal static class RavenShaderGlobalPropertyBaseMenuClass {

        [InitializeOnLoadMethod]
        [DidReloadScripts(1)]
        private static void RegisterCustomPropertyTrackViewCallback() {
            RavenEditorCallbacks.e_GenerateCustomDataForPropertyEventMenus -= GeneratePropertyTrackMenuData;
            RavenEditorCallbacks.e_GenerateCustomDataForPropertyEventMenus += GeneratePropertyTrackMenuData;
        }

        private static void GeneratePropertyTrackMenuData(RavenPropertyTrackView arg1, List<RavenPropertyTrackView.PropertyEventMenuData> arg2, List<RavenPropertyTrackView.AnimationDataType> arg3) {
            var properties = arg1.GetValidProperties(typeof(RavenShaderGlobalPropertyBase<>));
            arg1.GenerateDataForPropertyEventMenus(arg2, properties, arg3, "Custom/", null, ShaderGlobalPropertyFilter);
        }

        private static IEnumerable<RavenPropertyTrackView.ClassMemberInfo> ShaderGlobalPropertyFilter(List<RavenPropertyTrackView.AnimationPropertyType> properties, RavenPropertyTrackView.AnimationDataType animationData, Dictionary<Type, Type> propertyTypeCacheDict, object userData) {
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
                            m_Name = GetShaderGlobalPropertyClassMemberName(animationData),
                            m_PropertyType = propertyType,
                            m_AnimationDataType = animationData.m_Type,
                            m_ArgumentType = animationData.m_ArgumentType
                        }
                    };
            }
            return null;
        }

        private static string GetShaderGlobalPropertyClassMemberName(RavenPropertyTrackView.AnimationDataType animationDataType) {
            var arg = animationDataType.m_BaseType.GetGenericArguments()[0];
            return "ShaderGlobal" + "|" + arg.Name;
        }
    }
}