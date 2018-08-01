using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataCurveVector2 {
#if UNITY_EDITOR

        public override bool UniformCurves {
            get {
                return m_UniformCurves;
            }
            set {
                m_UniformCurves = value;
            }
        }

        protected sealed override void SetStartingValues(Vector2 values) {
            m_Curves = new AnimationCurve[c_ValueCount];
            for (int i = 0; i < c_ValueCount; ++i) {
                m_Curves[i] = new AnimationCurve();
                m_Curves[i].AddKey(new Keyframe(0, values[i]));
                m_Curves[i].AddKey(new Keyframe(1, values[i]));
            }
        }

#endif
    }
}