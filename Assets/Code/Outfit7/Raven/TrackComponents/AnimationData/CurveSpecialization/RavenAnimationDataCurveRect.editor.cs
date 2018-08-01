using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataCurveRect {
#if UNITY_EDITOR

        public override bool UniformCurves {
            get {
                return m_UniformCurves;
            }
            set {
                m_UniformCurves = value;
            }
        }

        protected sealed override void SetStartingValues(Rect values) {
            m_Curves = new AnimationCurve[c_ValueCount];
            var vectorValues = values.ToVector4();
            for (int i = 0; i < c_ValueCount; ++i) {
                m_Curves[i] = new AnimationCurve();
                m_Curves[i].AddKey(new Keyframe(0, vectorValues[i]));
                m_Curves[i].AddKey(new Keyframe(1, vectorValues[i]));
            }
        }

#endif
    }
}