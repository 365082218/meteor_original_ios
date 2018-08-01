using System;
using UnityEngine;

#if UNITY_EDITOR
#endif

namespace Starlite.Raven {

    public abstract partial class RavenMaterialPropertyBase<T> {
#if UNITY_EDITOR
        private string m_CachedTargetMaterialProperty = string.Empty;

        public bool UseSharedMaterial {
            get {
                return m_UseSharedMaterial;
            }
            set {
                m_UseSharedMaterial = value;
            }
        }

        public string TargetMaterialProperty {
            get {
                return m_TargetMaterialProperty;
            }
            set {
                m_TargetMaterialProperty = value;
            }
        }

        public int TargetMaterialIndex {
            get {
                return m_TargetMaterialIndex;
            }
            set {
                m_TargetMaterialIndex = value;
            }
        }

        public sealed override Type[] TypeConstraints {
            get {
                return new Type[] { typeof(T), typeof(string) };
            }
        }

        public sealed override object GetValueEditor(RavenSequence sequence) {
            if (m_CachedTargetMaterialProperty != m_TargetMaterialProperty) {
                m_CachedTargetMaterialProperty = m_TargetMaterialProperty;
                m_TargetMaterialPropertyId = Shader.PropertyToID(m_TargetMaterialProperty);
            }
            return GetValue(m_TargetComponent);
        }

        public override string ToPrettyString() {
            return MemberName;
        }

#endif
    }
}