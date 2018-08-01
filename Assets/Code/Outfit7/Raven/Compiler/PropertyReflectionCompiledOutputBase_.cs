using System;
using System.Collections.Generic;

namespace Starlite.Raven.Compiler {
#if !RAVEN_COMPILED
    public static class PropertyReflectionCompiledOutput {
#if UNITY_EDITOR
        public static readonly Dictionary<string, ulong> s_NameToTargetHashMap = new Dictionary<string, ulong>();
        public static readonly Dictionary<ulong, FunctionDeclaration[]> s_TargetHashToFuncMap = new Dictionary<ulong, FunctionDeclaration[]>();
        public static readonly Dictionary<string, string> s_TargetClassSpecializationToNameMap = new Dictionary<string, string>();
#endif

        public static bool HasPropertySpecialization(string packedTypes, out Type outSpecializationType) {
            outSpecializationType = null;
            return false;
        }

        public static bool HasInDatabase(string targetObject, string targetProperty, out ulong hash) {
            hash = RavenUtility.s_InvalidHash;
            return false;
        }

        public static void ConfigureRavenAnimationProperty<T>(RavenAnimationPropertyBase<T> animationProperty) {
        }
    }

    #region DeclarationOnly

    // Need these just for declaration, they're empty otherwise
    public abstract partial class RavenTriggerPropertyBase1<T0> : RavenTriggerPropertyComponentBase {
        public override void Initialize(RavenSequence sequence) {
        }

        public override void OnEnter() {
        }

        public virtual void ManualExecute(T0 value0) {
        }
    }

    public abstract partial class RavenTriggerPropertyBase2<T0, T1> : RavenTriggerPropertyComponentBase {
        public override void Initialize(RavenSequence sequence) {
        }

        public override void OnEnter() {
        }

        public virtual void ManualExecute(T0 value0, T1 value1) {
        }
    }

    #endregion DeclarationOnly

#endif

#if UNITY_EDITOR

    public class FunctionDeclaration {
        public int m_ID;
        public string m_FunctionName;
        public string m_ArgumentType;
        public string m_ComponentType;
        public string m_MemberName;
        public string m_CastStatement;
        public ulong m_Hash;
        public List<string> m_Objects;
        public bool m_IsValid;

        private bool m_Validated;

        public FunctionDeclaration() {
            m_Objects = new List<string>();
            m_IsValid = true;
            m_Validated = false;
        }

        public FunctionDeclaration(int id, string argType, string componentType, string memberName, ulong hash, string delegateName) : this() {
            m_ID = id;
            string funcPrefix = null;
            switch (id) {
                case 0:
                    funcPrefix = "GetValue";
                    m_CastStatement = string.Format("{0}<{1}>", delegateName, argType);
                    break;

                case 1:
                    funcPrefix = "SetValue";
                    m_CastStatement = string.Format("{0}<{1}>", delegateName, argType);
                    break;

                case 2:
                    funcPrefix = "CallFunction";
                    m_CastStatement = string.Format("{0}{1}", delegateName, RavenUtility.GetTemplateTypesForTypeArguments(argType));
                    break;
            }
            // Replace subclass type +
            m_CastStatement = RavenUtility.GetTypeableTypeFromOriginal(m_CastStatement);
            m_FunctionName = string.Format("{0}_{1}", funcPrefix, hash);
            m_ArgumentType = argType;
            m_ComponentType = componentType;
            m_MemberName = memberName;
            m_Hash = hash;
            // constructing it here means it's coming from an already validated path
            m_Validated = true;
        }

        public void AddResponsibleGameObject(object obj, string scene) {
            var go = obj as UnityEngine.Object;
            m_Objects.Add(string.Format("{0} ({1}) in [{2}]", go.name, go.GetInstanceID(), scene));
        }

        public bool GetValidated() {
            return m_Validated;
        }

        public void SetValidated(bool validated) {
            m_Validated = validated;
        }

        public string GetCastStatementForDelegate() {
            var types = RavenUtility.GetFunctionParameterTypesUnpacked(m_ArgumentType);
            if (types.Length == 0 || string.IsNullOrEmpty(types[0])) {
                return m_CastStatement;
            }

            var newCastStatement = m_CastStatement;
            int pos = 0;
            var defaultType = RavenUtility.s_DefaultFallbackType.ToString();
            for (int i = 0; i < types.Length; ++i) {
                var t = RavenUtility.GetTypeFromLoadedAssemblies(types[i]);
                if (t == null) {
                    newCastStatement = newCastStatement.ReplaceFirst(RavenUtility.GetTypeableTypeFromOriginal(types[i]), defaultType, out pos, pos);
                    pos += defaultType.Length;
                }
            }
            return RavenUtility.GetTypeableTypeFromOriginal(newCastStatement);
        }

        public string GetCastStatementForComponentType() {
            var componentType = RavenUtility.GetTypeFromLoadedAssemblies(m_ComponentType);
            if (componentType == null) {
                return ""; // "object"
            }

            return RavenUtility.GetTemplatedGenericFunction(componentType);
        }

        public override string ToString() {
            return m_FunctionName;
        }
    }

#endif
}