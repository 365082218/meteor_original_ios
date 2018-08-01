using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Starlite.Raven {

    public abstract partial class RavenShaderGlobalPropertyBase<T> {
#if UNITY_EDITOR
        private string m_CachedTargetShaderProperty = string.Empty;

        public string TargetShaderProperty {
            get {
                return m_TargetShaderProperty;
            }
            set {
                m_TargetShaderProperty = value;
            }
        }

        public sealed override Type[] TypeConstraints {
            get {
                return new Type[] { typeof(T), typeof(string) };
            }
        }

        public sealed override object GetValueEditor(RavenSequence sequence) {
            if (m_TargetShaderProperty != m_CachedTargetShaderProperty) {
                m_CachedTargetShaderProperty = m_TargetShaderProperty;
                m_TargetShaderPropertyId = Shader.PropertyToID(m_TargetShaderProperty);
            }
            return GetValue(m_TargetComponent);
        }

        public override bool ValidateProperty(RavenSequence sequence, GameObject target) {
            Undo.RecordObject(this, "ValidateProperty");

            m_TargetComponent = target;
            return true;
        }

#endif
    }
}