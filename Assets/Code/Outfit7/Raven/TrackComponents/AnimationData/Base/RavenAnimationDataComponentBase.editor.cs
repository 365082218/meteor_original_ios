using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Starlite.Raven {

    public abstract partial class RavenAnimationDataComponentBase {
#if UNITY_EDITOR

        public virtual Type TargetType {
            get {
                return null;
            }
        }

        public virtual string[] TargetMemberNames {
            get {
                return null;
            }
        }

        public void CopyValuesFrom(RavenAnimationDataComponentBase other) {
            Undo.RecordObject(this, "CopyValuesFrom");
            CopyValuesCallback(other);
        }

        public abstract bool CheckForDependencies();

        public abstract void SetStartingValuesEditor(object values);

        public virtual void SetHideFlags(HideFlags hideFlags) {
            this.hideFlags = hideFlags;
        }

        public virtual void DestroyEditor(RavenSequence sequence) {
            if (sequence.CanDestroyAnimationData(this)) {
                Undo.DestroyObjectImmediate(this);
            }
        }

        protected abstract void CopyValuesCallback(RavenAnimationDataComponentBase other);

#endif
    }
}