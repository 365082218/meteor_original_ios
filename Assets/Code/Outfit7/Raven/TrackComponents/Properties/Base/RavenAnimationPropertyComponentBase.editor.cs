using System;
using UnityEngine;
using Starlite.Raven.Internal;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Starlite.Raven {

    public abstract partial class RavenAnimationPropertyComponentBase {
#if UNITY_EDITOR

        public string ComponentType {
            get {
                return m_ComponentType;
            }
            set {
                m_ComponentType = value;
            }
        }

        public string MemberName {
            get {
                return m_MemberName;
            }
            set {
                m_MemberName = value;
            }
        }

        public Type ComponentBaseType {
            get {
                return RavenUtility.GetBaseTypeForMemberInType(m_TargetComponent.GetType(),
                    m_MemberName,
                    RavenUtility.EMemberType.FieldOrProperty,
                    GetPropertyType().ToString());
            }
        }

        public abstract RavenAnimationDataComponentBase AnimationData {
            get;
            set;
        }

        public abstract RavenTriggerPropertyComponentBase TriggerProperty {
            get;
            set;
        }

        public abstract Type[] TypeConstraints {
            get;
        }

        public override void SetHideFlags(HideFlags hideFlags) {
            base.SetHideFlags(hideFlags);
            if (m_TriggerProperty != null) {
                m_TriggerProperty.SetHideFlags(hideFlags);
            }
            if (m_AnimationData != null) {
                m_AnimationData.SetHideFlags(hideFlags);
            }
        }

        public virtual bool ValidateProperty(RavenSequence sequence, GameObject target) {
            Undo.RecordObject(this, "ValidateProperty");

            var fail = false;
            var compType = RavenUtility.GetTypeFromLoadedAssemblies(m_ComponentType);
            if (target == null || compType == null) {
                fail = true;
            } else {
                if (compType == typeof(GameObject)) {
                    m_TargetComponent = target;
                } else {
                    var component = target.GetComponent(compType);
                    if (component == null) {
                        fail = true;
                    } else {
                        m_TargetComponent = component;
                    }
                }
            }

            if (fail) {
                RavenLog.WarnT(RavenSequence.Tag, "Validation for {0} failed! Removing property {1}.{2}.", this, m_TargetComponent, m_MemberName);
                DestroyEditor(sequence);
            }
            return !fail;
        }

        public override void DestroyEditor(RavenSequence sequence) {
            base.DestroyEditor(sequence);
            if (m_TriggerProperty != null) {
                m_TriggerProperty.DestroyEditor(sequence);
            }
            if (m_AnimationData != null) {
                m_AnimationData.DestroyEditor(sequence);
            }
        }

        public override string ToPrettyString() {
            return RavenUtility.GetTypeWithoutNamespace(m_ComponentType) + "." + m_MemberName;
        }

        public abstract object GetValueEditor(RavenSequence sequence);

#endif
    }
}