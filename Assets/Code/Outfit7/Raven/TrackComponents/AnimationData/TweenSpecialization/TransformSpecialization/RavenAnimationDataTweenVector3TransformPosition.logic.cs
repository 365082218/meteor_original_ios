using Starlite.Raven.Internal;
using UnityEngine;

namespace Starlite.Raven {

    public partial class RavenAnimationDataTweenVector3TransformPosition {
        
        [System.NonSerialized]
        private RavenParameter m_StartCameraParameter;

        [System.NonSerialized]
        private RavenParameter m_EndCameraParameter;

        private Vector3 m_StartTangentValue;
        private Vector3 m_EndTangentValue;

        private Camera m_StartCameraValue;
        private Camera m_EndCameraValue;

        private Vector3 m_CalculatedTangent;

        public override void Initialize(RavenSequence sequence, RavenAnimationPropertyComponentBase property) {
            base.Initialize(sequence, property);

            m_StartTangentParameter = sequence.GetParameterAtIndex(m_StartTangentParameterIndex);
            m_EndTangentParameter = sequence.GetParameterAtIndex(m_EndTangentParameterIndex);

            m_StartCameraParameter = sequence.GetParameterAtIndex(m_StartCameraParameterIndex);
            m_EndCameraParameter = sequence.GetParameterAtIndex(m_EndCameraParameterIndex);

            ValidateCustomParameters();
        }

        protected override void OnEnterCallback() {
            base.OnEnterCallback();

            if (m_StartTangentParameter != null) {
                m_StartTangentValue = m_StartTangentParameter.m_ValueVector;
            } else {
                if (m_StartTangentValueType == ERavenValueType.Range) {
                    m_StartTangentValue = RavenValueInterpolatorVector3.Default.Random(m_StartTangentStart, m_StartTangentEnd);
                } else {
                    m_StartTangentValue = m_StartTangentStart;
                }
            }

            if (m_EndTangentParameter != null) {
                m_EndTangentValue = m_EndTangentParameter.m_ValueVector;
            } else {
                if (m_EndTangentValueType == ERavenValueType.Range) {
                    m_EndTangentValue = RavenValueInterpolatorVector3.Default.Random(m_EndTangentStart, m_EndTangentEnd);
                } else {
                    m_EndTangentValue = m_EndTangentStart;
                }
            }

            m_StartCameraValue = m_StartCameraParameter != null ? m_StartCameraParameter.m_ValueObject as Camera : m_StartCamera;
            m_EndCameraValue = m_EndCameraParameter != null ? m_EndCameraParameter.m_ValueObject as Camera : m_EndCamera;
        }

        protected override bool PostEvaluateAtTime(Vector3 startValue, Vector3 endValue, double t, ref Vector3 value) {
            if (m_StartCameraValue != null && m_EndCameraValue != null) {
                if (m_FromPointCasting) {
                    var screenPoint = m_StartCameraValue.WorldToScreenPoint(startValue);
                    screenPoint.z = m_CameraDepthDestination;
                    startValue = m_EndCameraValue.ScreenToWorldPoint(screenPoint);
                } else {
                    var screenPoint = m_EndCameraValue.WorldToScreenPoint(endValue);
                    screenPoint.z = m_CameraDepthDestination;
                    endValue = m_StartCameraValue.ScreenToWorldPoint(screenPoint);
                }
            }

            if (m_UseBezierCurve) {
                value = (m_Interpolator as RavenValueInterpolatorVector3).BezierCubicCurve(startValue, startValue + m_StartTangentValue, endValue + m_EndTangentValue, endValue, t);
            } else {
                value = m_Interpolator.Interpolate(startValue, endValue, t);
            }

            if (m_RotateWithTangent) {
                m_CalculatedTangent = (m_Interpolator as RavenValueInterpolatorVector3).BezierCubicTangent(startValue, startValue + m_StartTangentValue, endValue + m_EndTangentValue, endValue, t);
            }

            return true;
        }

        public override void PostprocessFinalValue(Vector3 value, UnityEngine.Object targetObject) {
            if (m_RotateWithTangent && targetObject != null) {
                (targetObject as Transform).LookAt(value + m_CalculatedTangent);
            }
        }

        private void ValidateCustomParameters() {
            if (m_StartTangentParameterIndex >= 0 && m_StartTangentParameter == null) {
                RavenLog.ErrorT(RavenSequence.Tag, "Parameter at index {0} does not exist for {1}! Ignoring.", m_StartTangentParameterIndex, this);
            }
            if (m_EndTangentParameterIndex >= 0 && m_EndTangentParameter == null) {
                RavenLog.ErrorT(RavenSequence.Tag, "Parameter at index {0} does not exist for {1}! Ignoring.", m_EndTangentParameterIndex, this);
            }
            if (m_StartCameraParameterIndex >= 0 && m_StartCameraParameter == null) {
                RavenLog.ErrorT(RavenSequence.Tag, "Parameter at index {0} does not exist for {1}! Ignoring.", m_StartCameraParameterIndex, this);
            }
            if (m_EndCameraParameterIndex >= 0 && m_EndCameraParameter == null) {
                RavenLog.ErrorT(RavenSequence.Tag, "Parameter at index {0} does not exist for {1}! Ignoring.", m_EndCameraParameterIndex, this);
            }
        }
    }
}