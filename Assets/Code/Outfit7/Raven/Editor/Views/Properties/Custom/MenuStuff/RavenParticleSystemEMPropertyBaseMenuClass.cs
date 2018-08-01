using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Starlite.Raven {

    internal static class RavenParticleSystemEMPropertyBaseMenuClass {

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

            var particleSystem = arg1.Target.GetComponent<ParticleSystem>();

            if (particleSystem != null) {
                var properties = arg1.GetValidProperties(typeof(RavenParticleSystemEMPropertyBase<>));
                arg1.GenerateDataForPropertyEventMenus(arg2, properties, arg3, "Custom/", particleSystem, ParticleSystemEMPropertyFilter);
            }
        }

        private static IEnumerable<RavenPropertyTrackView.ClassMemberInfo> ParticleSystemEMPropertyFilter(List<RavenPropertyTrackView.AnimationPropertyType> properties, RavenPropertyTrackView.AnimationDataType animationData, Dictionary<Type, Type> propertyTypeCacheDict, object userData) {
            var validProperties = properties.FindAll(x => x.m_ArgumentType == animationData.m_ArgumentType);
            var classMemberInfos = new List<RavenPropertyTrackView.ClassMemberInfo>(validProperties.Count);
            for (int i = 0; i < validProperties.Count; ++i) {
                var propertyType = validProperties[i].m_Type;
                classMemberInfos.Add(new RavenPropertyTrackView.ClassMemberInfo() {
                    m_Name = GetMaterialClassMemberName(animationData, propertyType, userData),
                    m_PropertyType = propertyType,
                    m_AnimationDataType = animationData.m_Type,
                    m_ArgumentType = animationData.m_ArgumentType
                });
            }
            return classMemberInfos;
        }

        private static string GetMaterialClassMemberName(RavenPropertyTrackView.AnimationDataType animationDataType, Type propertyType, object userData) {
            return userData.GetType().ToString() + "|EmissionModule/" + RavenUtility.GetTypeWithoutNamespace(propertyType.ToString()).Replace("RavenParticleSystemEMProperty", "");
        }
    }
}