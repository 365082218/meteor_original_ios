#if UNITY_5_6_OR_NEWER

using System;
using UnityEngine;

#if UNITY_EDITOR
#endif

namespace Starlite.Raven {

    public abstract partial class RavenParticleSystemMainPropertyBase<T> {
#if UNITY_EDITOR

        public override Type[] TypeConstraints {
            get {
                return new Type[] { typeof(T), typeof(ParticleSystem.MainModule) };
            }
        }

        public sealed override object GetValueEditor(RavenSequence sequence) {
            return GetValue(m_TargetComponent);
        }

        public override string ToPrettyString() {
            return MemberName;
        }

#endif
    }
}
#endif