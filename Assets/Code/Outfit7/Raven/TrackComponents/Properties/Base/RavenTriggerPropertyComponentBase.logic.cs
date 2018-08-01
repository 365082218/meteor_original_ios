using UnityEngine;

namespace Starlite.Raven {

    public abstract partial class RavenTriggerPropertyComponentBase {

        public sealed override RavenPropertyComponent ChildPropertyComponent {
            get {
                return null;
            }
        }

        public sealed override int ParameterIndex {
            get {
                return m_ParameterIndex;
            }
        }

        public ulong TargetHash {
            get {
                return m_TargetHash;
            }
#if UNITY_EDITOR
            set {
                m_TargetHash = value;
            }
#endif
        }

        public bool IsValid() {
            return m_TargetHash != RavenUtility.s_InvalidHash
#if UNITY_EDITOR
                && UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode
#endif
                ;
        }
    }
}