using System;
using UnityEngine;
using Starlite.Raven.Internal;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Starlite.Raven {

    public abstract partial class RavenTriggerPropertyComponentBase {
#if UNITY_EDITOR

        public string ComponentType {
            get {
                return m_ComponentType;
            }
            set {
                m_ComponentType = value;
            }
        }

        public string FunctionName {
            get {
                return m_FunctionName;
            }
            set {
                m_FunctionName = value;
            }
        }

        public string FullFunctionName {
            get {
                return RavenUtility.CombineComponentTypeAndFunctionName(m_ComponentType, m_FunctionName);
            }
        }

        public Type ComponentBaseType {
            get {
                return RavenUtility.GetBaseTypeForMemberInType(m_TargetComponent.GetType(),
                    RavenUtility.GetFunctionNameFromPackedFunctionName(m_FunctionName),
                    RavenUtility.EMemberType.Function,
                    RavenUtility.GetFunctionParameterTypesUnpacked(RavenUtility.GetFunctionParametersFromPackedFunctionName(m_FunctionName)));
            }
        }

        public void SetParameterIndexEditor(int parameterIndex) {
            if (parameterIndex == m_ParameterIndex) {
                return;
            }

            Undo.RecordObject(this, "SetParameterIndex");
            m_ParameterIndex = parameterIndex;
        }

        public sealed override bool CheckForDependencies() {
            return true;
        }

        public virtual bool ValidateProperty(RavenSequence sequence, GameObject target) {
            Undo.RecordObject(this, "ValidateProperty");

            var compType = RavenUtility.GetTypeFromLoadedAssemblies(m_ComponentType);
            var fail = false;
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
                RavenLog.WarnT(RavenSequence.Tag, "Validation for {0} failed! Removing property {1}.{2}.", this, m_TargetComponent, m_FunctionName);
                DestroyEditor(sequence);
            }
            return !fail;
        }

        public override string ToPrettyString() {
            return RavenUtility.GetTypeWithoutNamespace(m_ComponentType) + "." + RavenUtility.GetFunctionNameFromPackedFunctionName(m_FunctionName);
        }

#endif
    }
}