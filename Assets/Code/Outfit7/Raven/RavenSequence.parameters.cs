using Starlite.Raven.Internal;
using System.Collections.Generic;
using UnityEngine;

namespace Starlite.Raven {

    public partial class RavenSequence {
        private List<List<RavenPropertyComponent>> m_ParameterIndexToPropertyMap;
        private bool[] m_AutoRegisteredParameters = null;

        public void RegisterRavenComponents(GameObject targetParent) {
            var components = targetParent.GetComponentsInChildren<RavenAutoRegisterComponent>(true);
            var componentsLength = components.Length;
            if (componentsLength > 0) {
                m_AutoRegisteredParameters = new bool[m_Parameters.Count];
            }
            for (int i = 0; i < componentsLength; ++i) {
                var component = components[i];
                var parameterIndex = FindParameterIndexByName(component.m_Parameter);
                if (parameterIndex >= 0) {
                    var parameter = m_Parameters[parameterIndex];
                    if (parameter.m_ParameterType != ERavenParameterType.ActorList) {
                        RavenAssert.IsTrue(false, "Target parameter at index {0} is not an actor list!", parameterIndex);
                    }

                    if (!m_AutoRegisteredParameters[parameterIndex]) {
                        parameter.ClearGameObjectList();
                        m_AutoRegisteredParameters[parameterIndex] = true;
                    }
                    parameter.AddGameObject(component.gameObject);
                    // Don't call set parameter here because we'll handle that in UpdatePropertiesWithParameters all at once
                    // instead of one by one
                }
            }

            UpdatePropertiesWithParameters();
        }

        private void InitializeParameters() {
            CreateParameterIndexToPropertyMap();
#if UNITY_EDITOR
            if (Application.isPlaying) {
#endif
                UpdatePropertiesWithParameters();
#if UNITY_EDITOR
            }
#endif
        }

        private void DeinitializeParameters() {
            if (m_ParameterIndexToPropertyMap != null) {
                for (int i = 0; i < m_ParameterIndexToPropertyMap.Count; ++i) {
                    RavenOverseer.PushPropertyComponentList(m_ParameterIndexToPropertyMap[i]);
                }
                m_ParameterIndexToPropertyMap = null;
            }

#if !UNITY_EDITOR
            // Don't do this in editor because people might change editor stuff in runtime which triggers Deinitialize often
            if (m_AutoRegisteredParameters != null) {
                for (int i = 0; i < m_AutoRegisteredParameters.Length; ++i) {
                    if (m_AutoRegisteredParameters[i]) {
                        m_Parameters[i].ClearGameObjectList();
                    }
                }

                m_AutoRegisteredParameters = null;
            }
#endif
        }

        private void ProcessPropertyParameters(RavenPropertyComponent property, int overrideParameterIndex) {
            if (property == null) {
                return;
            }

            if (overrideParameterIndex >= 0 && property.ParameterIndex < 0) {
                m_ParameterIndexToPropertyMap[overrideParameterIndex].Add(property);
            } else if (property.ParameterIndex >= 0) {
                m_ParameterIndexToPropertyMap[property.ParameterIndex].Add(property);
            }

            // don't use track's override parameter for child properties because those are decoupled
            var childProperty = property.ChildPropertyComponent;
            while (childProperty != null) {
                if (childProperty.ParameterIndex >= 0) {
                    m_ParameterIndexToPropertyMap[childProperty.ParameterIndex].Add(childProperty);
                }
                childProperty = childProperty.ChildPropertyComponent;
            }
        }

        public int FindParameterIndexByName(string name) {
            for (int i = 0; i < m_Parameters.Count; ++i) {
                var parameter = m_Parameters[i];
                if (parameter.m_Name == name) {
                    return i;
                }
            }

            return -1;
        }

        #region Set

        public void SetBoolParameter(int index, bool value) {
            m_Parameters[index].SetBool(value);
        }

        public void SetIntParameter(int index, int value) {
            m_Parameters[index].SetInt(value);
        }

        public void SetFloatParameter(int index, float value) {
            m_Parameters[index].SetFloat(value);
        }

        public void SetVectorParameter(int index, Vector4 value) {
            m_Parameters[index].SetVector(value);
        }

        public void SetObjectParameter(int index, UnityEngine.Object value) {
            m_Parameters[index].SetObject(value);
        }

        public void SetActorListParameter(int index, List<GameObject> value) {
            m_Parameters[index].SetGameObjectList(value);
            OnSetActorList(index, value);
        }

        public void SetActorParameter(int index, GameObject value) {
            var parameter = m_Parameters[index];
            parameter.m_ValueGameObjectList.Clear();
            if (value != null) {
                parameter.m_ValueGameObjectList.Add(value);
            }
            OnSetActorList(index, parameter.m_ValueGameObjectList);
        }

        public void ClearActorListParameter(int index) {
            var parameter = m_Parameters[index];
            parameter.m_ValueGameObjectList.Clear();
            OnSetActorList(index, parameter.m_ValueGameObjectList);
        }

        public void SetBoolParameter(string name, bool value) {
            var idx = FindParameterIndexByName(name);
            if (idx == -1) {
                RavenLog.ErrorT(Tag, "Failed to find parameter by name {0}", name);
                return;
            }
            SetBoolParameter(idx, value);
        }

        public void SetIntParameter(string name, int value) {
            var idx = FindParameterIndexByName(name);
            if (idx == -1) {
                RavenLog.ErrorT(Tag, "Failed to find parameter by name {0}", name);
                return;
            }
            SetIntParameter(idx, value);
        }

        public void SetFloatParameter(string name, float value) {
            var idx = FindParameterIndexByName(name);
            if (idx == -1) {
                RavenLog.ErrorT(Tag, "Failed to find parameter by name {0}", name);
                return;
            }
            SetFloatParameter(idx, value);
        }

        public void SetVectorParameter(string name, Vector4 value) {
            var idx = FindParameterIndexByName(name);
            if (idx == -1) {
                RavenLog.ErrorT(Tag, "Failed to find parameter by name {0}", name);
                return;
            }
            SetVectorParameter(idx, value);
        }

        public void SetObjectParameter(string name, UnityEngine.Object value) {
            var idx = FindParameterIndexByName(name);
            if (idx == -1) {
                RavenLog.ErrorT(Tag, "Failed to find parameter by name {0}", name);
                return;
            }
            SetObjectParameter(idx, value);
        }

        public void SetActorListParameter(string name, List<GameObject> value) {
            var idx = FindParameterIndexByName(name);
            if (idx == -1) {
                RavenLog.ErrorT(Tag, "Failed to find parameter by name {0}", name);
                return;
            }
            SetActorListParameter(idx, value);
        }

        public void SetActorParameter(string name, GameObject value) {
            var idx = FindParameterIndexByName(name);
            if (idx == -1) {
                RavenLog.ErrorT(Tag, "Failed to find parameter by name {0}", name);
                return;
            }

            SetActorParameter(idx, value);
        }

        public void ClearActorListParameter(string name) {
            var idx = FindParameterIndexByName(name);
            if (idx == -1) {
                RavenLog.ErrorT(Tag, "Failed to find parameter by name {0}", name);
                return;
            }

            ClearActorListParameter(idx);
        }

        #endregion Set

        #region Get

        public RavenParameter GetParameterAtIndex(int index) {
            if (index < 0 || index >= m_Parameters.Count) {
                return null;
            }

            return m_Parameters[index];
        }

        public bool GetBoolParameter(int index) {
            return m_Parameters[index].m_ValueInt != 0;
        }

        public int GetIntParameter(int index) {
            return m_Parameters[index].m_ValueInt;
        }

        public float GetFloatParameter(int index) {
            return m_Parameters[index].m_ValueFloat;
        }

        public Vector4 GetVectorParameter(int index) {
            return m_Parameters[index].m_ValueVector;
        }

        public UnityEngine.Object GetObjectParameter(int index) {
            return m_Parameters[index].m_ValueObject;
        }

        public List<GameObject> GetActorListParameter(int index) {
            return m_Parameters[index].m_ValueGameObjectList;
        }

        public bool GetBoolParameter(string name) {
            var idx = FindParameterIndexByName(name);
            if (idx == -1) {
                RavenLog.ErrorT(Tag, "Failed to find parameter by name {0}", name);
                return false;
            }
            return GetBoolParameter(idx);
        }

        public int GetIntParameter(string name) {
            var idx = FindParameterIndexByName(name);
            if (idx == -1) {
                RavenLog.ErrorT(Tag, "Failed to find parameter by name {0}", name);
                return 0;
            }
            return GetIntParameter(idx);
        }

        public float GetFloatParameter(string name) {
            var idx = FindParameterIndexByName(name);
            if (idx == -1) {
                RavenLog.ErrorT(Tag, "Failed to find parameter by name {0}", name);
                return 0f;
            }
            return GetFloatParameter(idx);
        }

        public Vector4 GetVectorParameter(string name) {
            var idx = FindParameterIndexByName(name);
            if (idx == -1) {
                RavenLog.ErrorT(Tag, "Failed to find parameter by name {0}", name);
                return Vector4.zero;
            }
            return GetVectorParameter(idx);
        }

        public UnityEngine.Object GetObjectParameter(string name) {
            var idx = FindParameterIndexByName(name);
            if (idx == -1) {
                RavenLog.ErrorT(Tag, "Failed to find parameter by name {0}", name);
                return null;
            }
            return GetObjectParameter(idx);
        }

        public List<GameObject> GetActorListParameter(string name) {
            var idx = FindParameterIndexByName(name);
            if (idx == -1) {
                RavenLog.ErrorT(Tag, "Failed to find parameter by name {0}", name);
                return null;
            }
            return GetActorListParameter(idx);
        }

        #endregion Get

        private void CreateParameterIndexToPropertyMap() {
            m_ParameterIndexToPropertyMap = new List<List<RavenPropertyComponent>>(m_Parameters.Count);
            for (int i = 0; i < m_Parameters.Count; ++i) {
                m_ParameterIndexToPropertyMap.Add(RavenOverseer.PopPropertyComponentList());
            }

            // first do tracks because they have priority
            // we only override targets for the topmost property here
            // as they can have child properties which may be independent
            for (int i = 0; i < m_SortedTrackGroups.Count; ++i) {
                var trackGroup = m_SortedTrackGroups[i];
                var overrideParameterIndex = trackGroup.m_OverrideTargetsParameterIndex;
                var propertyTrack = trackGroup.m_PropertyTrack;
                if (propertyTrack != null) {
                    for (int j = 0; j < propertyTrack.Events.Count; ++j) {
                        ProcessPropertyParameters(propertyTrack.Events[j].PropertyComponent, overrideParameterIndex);
                    }
                }

                var audioTrack = trackGroup.m_AudioTrack;
                if (audioTrack != null) {
                    for (int j = 0; j < audioTrack.Events.Count; ++j) {
                        ProcessPropertyParameters(audioTrack.Events[j].PropertyComponent, overrideParameterIndex);
                    }
                }
            }
        }

        private void UpdatePropertiesWithParameters() {
            for (int i = 0; i < m_ParameterIndexToPropertyMap.Count; ++i) {
                var parameter = m_Parameters[i];
                if (parameter.m_ParameterType == ERavenParameterType.ActorList) {
                    OnSetActorList(i, parameter.m_ValueGameObjectList);
                }
            }
        }

        private void OnSetActorList(int parameterIndex, List<GameObject> value) {
            if (m_ParameterIndexToPropertyMap != null && parameterIndex < m_ParameterIndexToPropertyMap.Count)
            {
                var properties = m_ParameterIndexToPropertyMap[parameterIndex];
                for (int i = 0; i < properties.Count; ++i)
                {
                    properties[i].SetTargets(value);
                }
            }
        }
    }
}