using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Starlite.Raven {

    public abstract partial class RavenPropertyComponent {
#if UNITY_EDITOR

        public GameObject Target {
            get {
                return m_Target;
            }
            set {
                m_Target = value;
            }
        }

        public UnityEngine.Object TargetComponent {
            get {
                return m_TargetComponent;
            }
            set {
                m_TargetComponent = value;
            }
        }

        public virtual void SetHideFlags(HideFlags hideFlags) {
            this.hideFlags = hideFlags;
        }

        public virtual void DestroyEditor(RavenSequence sequence) {
            Undo.DestroyObjectImmediate(this);
        }

        public abstract bool CheckForDependencies();

        public abstract string ToPrettyString();

#endif
    }
}