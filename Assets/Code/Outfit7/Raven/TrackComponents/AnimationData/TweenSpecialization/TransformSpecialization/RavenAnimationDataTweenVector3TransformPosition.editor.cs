using UnityEngine;

namespace Starlite.Raven {

    public partial class RavenAnimationDataTweenVector3TransformPosition {
#if UNITY_EDITOR

        public override string[] TargetMemberNames {
            get {
                return new string[] { "position", "localPosition" };
            }
        }

        public Camera StartCamera {
            get {
                return m_StartCamera;
            }
            set {
                m_StartCamera = value;
            }
        }

        public Camera EndCamera {
            get {
                return m_EndCamera;
            }
            set {
                m_EndCamera = value;
            }
        }

        public float CameraDepthDestination {
            get {
                return m_CameraDepthDestination;
            }
            set {
                m_CameraDepthDestination = value;
            }
        }

        public int StartCameraParameterIndex {
            get {
                return m_StartCameraParameterIndex;
            }
            set {
                m_StartCameraParameterIndex = value;
            }
        }

        public int EndCameraParameterIndex {
            get {
                return m_EndCameraParameterIndex;
            }
            set {
                m_EndCameraParameterIndex = value;
            }
        }

        public bool FromPointCasting {
            get {
                return m_FromPointCasting;
            }
            set {
                m_FromPointCasting = value;
            }
        }

        protected override void CopyValuesCallback(RavenAnimationDataComponentBase other) {
            base.CopyValuesCallback(other);

            var otherReal = other as RavenAnimationDataTweenVector3TransformPosition;
            if (otherReal == null) {
                return;
            }

            m_StartTangentStart = otherReal.m_StartTangentStart;
            m_StartTangentEnd = otherReal.m_StartTangentEnd;
            m_StartTangentParameterIndex = otherReal.m_StartTangentParameterIndex;
            m_StartTangentValueType = otherReal.m_StartTangentValueType;
            m_EndTangentStart = otherReal.m_EndTangentStart;
            m_EndTangentEnd = otherReal.m_EndTangentEnd;
            m_EndTangentParameterIndex = otherReal.m_EndTangentParameterIndex;
            m_EndTangentValueType = otherReal.m_EndTangentValueType;

            m_UseBezierCurve = otherReal.m_UseBezierCurve;
            m_RotateWithTangent = otherReal.m_RotateWithTangent;

            m_StartCamera = otherReal.m_StartCamera;
            m_EndCamera = otherReal.m_EndCamera;
            m_StartCameraParameterIndex = otherReal.m_StartCameraParameterIndex;
            m_EndCameraParameterIndex = otherReal.m_EndCameraParameterIndex;
            m_CameraDepthDestination = otherReal.m_CameraDepthDestination;
            m_FromPointCasting = otherReal.m_FromPointCasting;
        }

#endif
    }
}