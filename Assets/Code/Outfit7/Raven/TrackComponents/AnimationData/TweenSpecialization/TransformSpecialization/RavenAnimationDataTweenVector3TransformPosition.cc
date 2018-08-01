#ifdef STARLITE
#include "RavenAnimationDataTweenVector3TransformPosition.h"
#include "RavenAnimationDataTweenVector3TransformPosition.cs"

#include <RavenSequence.h>
#include <Interpolators/Base/RavenInterpolation.h>

using namespace Starlite::Raven::Internal;

namespace Starlite {
    namespace Raven {
        RavenAnimationDataTweenVector3TransformPosition::RavenAnimationDataTweenVector3TransformPosition() {
        }

        bool RavenAnimationDataTweenVector3TransformPosition::GetFromPointCasting() const {
            return m_FromPointCasting;
        }

        float RavenAnimationDataTweenVector3TransformPosition::GetCameraDepthDestination() const {
            return m_CameraDepthDestination;
        }

        int RavenAnimationDataTweenVector3TransformPosition::GetEndCameraParameterIndex() const {
            return m_EndCameraParameterIndex;
        }

        int RavenAnimationDataTweenVector3TransformPosition::GetStartCameraParameterIndex() const {
            return m_StartCameraParameterIndex;
        }

        const Ref<Camera>& RavenAnimationDataTweenVector3TransformPosition::GetEndCamera() const {
            return m_EndCamera;
        }

        const Ref<Camera>& RavenAnimationDataTweenVector3TransformPosition::GetStartCamera() const {
            return m_StartCamera;
        }

        void RavenAnimationDataTweenVector3TransformPosition::Initialize(Ptr<RavenSequence> sequence, Ptr<RavenAnimationPropertyComponentBase> property) {
            RavenAnimationDataTweenVector3Transform::Initialize(sequence, property);

            m_StartTangentParameter = sequence->GetParameterAtIndex(m_StartTangentParameterIndex);
            m_EndTangentParameter = sequence->GetParameterAtIndex(m_EndTangentParameterIndex);

            m_StartCameraParameter = sequence->GetParameterAtIndex(m_StartCameraParameterIndex);
            m_EndCameraParameter = sequence->GetParameterAtIndex(m_EndCameraParameterIndex);

            ValidateCustomParameters();
        }

        void RavenAnimationDataTweenVector3TransformPosition::PostprocessFinalValue(Vector3& value, Object* targetObject) const {
            if (m_RotateWithTangent && targetObject != nullptr) {
                (reinterpret_cast<SceneObject*>(targetObject))->LookAt(value + m_CalculatedTangent);
            }
        }

        void RavenAnimationDataTweenVector3TransformPosition::SetCameraDepthDestination(float value) {
            m_CameraDepthDestination = value;
        }

        void RavenAnimationDataTweenVector3TransformPosition::SetEndCamera(Ref<Camera>& value) {
            m_EndCamera = value;
        }

        void RavenAnimationDataTweenVector3TransformPosition::SetEndCameraParameterIndex(int value) {
            m_EndCameraParameterIndex = value;
        }

        void RavenAnimationDataTweenVector3TransformPosition::SetFromPointCasting(bool value) {
            m_FromPointCasting = value;
        }

        void RavenAnimationDataTweenVector3TransformPosition::SetStartCamera(Ref<Camera>& value) {
            m_StartCamera = value;
        }

        void RavenAnimationDataTweenVector3TransformPosition::SetStartCameraParameterIndex(int value) {
            m_StartCameraParameterIndex = value;
        }

        bool RavenAnimationDataTweenVector3TransformPosition::PostEvaluateAtTime(const Vector3& startValue, const Vector3& endValue, double t, Vector3& value) {
            Vector3 _startValue = startValue;
            Vector3 _endValue = endValue;
            if (m_StartCameraValue && m_EndCameraValue) {
                if (m_FromPointCasting) {
                    auto screenPoint = m_StartCameraValue->WorldToScreenPoint(_startValue);
                    screenPoint.z = m_CameraDepthDestination;
                    _startValue = m_EndCameraValue->ScreenToWorldPoint(screenPoint);
                } else {
                    auto screenPoint = m_EndCameraValue->WorldToScreenPoint(_endValue);
                    screenPoint.z = m_CameraDepthDestination;
                    _endValue = m_StartCameraValue->ScreenToWorldPoint(screenPoint);
                }
            }

            if (m_UseBezierCurve) {
                value = RavenInterpolation::BezierCubicCurve<Vector3>(_startValue, _startValue + m_StartTangentValue, _endValue + m_EndTangentValue, _endValue, (float)t);
            } else {
                value = RavenValueInterpolator<Vector3>::Interpolate(_startValue, _endValue, t);
            }

            if (m_RotateWithTangent) {
                m_CalculatedTangent = RavenInterpolation::BezierCubicTangent<Vector3>(_startValue, _startValue + m_StartTangentValue, _endValue + m_EndTangentValue, _endValue, (float)t);
            }

            return true;
        }

        void RavenAnimationDataTweenVector3TransformPosition::CopyValuesCallback(const RavenAnimationDataComponentBase* other) {
            RavenAnimationDataTweenVector3Transform::CopyValuesCallback(other);

            if (other->IsDerivedFrom(RavenAnimationDataTweenVector3TransformPosition::TypeId)) {
                return;
            }

            auto otherReal = reinterpret_cast<const RavenAnimationDataTweenVector3TransformPosition*>(other);
            m_StartTangentStart = otherReal->m_StartTangentStart;
            m_StartTangentEnd = otherReal->m_StartTangentEnd;
            m_StartTangentParameterIndex = otherReal->m_StartTangentParameterIndex;
            m_StartTangentValueType = otherReal->m_StartTangentValueType;
            m_EndTangentStart = otherReal->m_EndTangentStart;
            m_EndTangentEnd = otherReal->m_EndTangentEnd;
            m_EndTangentParameterIndex = otherReal->m_EndTangentParameterIndex;
            m_EndTangentValueType = otherReal->m_EndTangentValueType;

            m_UseBezierCurve = otherReal->m_UseBezierCurve;
            m_RotateWithTangent = otherReal->m_RotateWithTangent;

            m_StartCamera = otherReal->m_StartCamera;
            m_EndCamera = otherReal->m_EndCamera;
            m_StartCameraParameterIndex = otherReal->m_StartCameraParameterIndex;
            m_EndCameraParameterIndex = otherReal->m_EndCameraParameterIndex;
            m_CameraDepthDestination = otherReal->m_CameraDepthDestination;
            m_FromPointCasting = otherReal->m_FromPointCasting;
        }

        void RavenAnimationDataTweenVector3TransformPosition::OnEnterCallback() {
            RavenAnimationDataTweenVector3Transform::OnEnterCallback();

            if (m_StartTangentParameter) {
                m_StartTangentValue = (Vector3)m_StartTangentParameter->m_ValueVector;
            } else {
                if (m_StartTangentValueType == ERavenValueType::Range) {
                    m_StartTangentValue = RavenValueInterpolator<Vector3>::Random(m_StartTangentStart, m_StartTangentEnd);
                } else {
                    m_StartTangentValue = m_StartTangentStart;
                }
            }

            if (m_EndTangentParameter) {
                m_EndTangentValue = (Vector3)m_EndTangentParameter->m_ValueVector;
            } else {
                if (m_EndTangentValueType == ERavenValueType::Range) {
                    m_EndTangentValue = RavenValueInterpolator<Vector3>::Random(m_EndTangentStart, m_EndTangentEnd);
                } else {
                    m_EndTangentValue = m_EndTangentStart;
                }
            }

            m_StartCameraValue = m_StartCameraParameter ? reinterpret_cast<Camera*>(m_StartCameraParameter->m_ValueObject.GetObject()) : m_StartCamera.GetObject();
            m_EndCameraValue = m_EndCameraParameter ? reinterpret_cast<Camera*>(m_EndCameraParameter->m_ValueObject.GetObject()) : m_EndCamera.GetObject();
        }

        void RavenAnimationDataTweenVector3TransformPosition::ValidateCustomParameters() {
            if (m_StartTangentParameterIndex >= 0 && m_StartTangentParameter == nullptr) {
                pRavenLog->ErrorT(RavenSequence::Tag.GetCString(), "Parameter at index %d does not exist for %s! Ignoring.", m_StartTangentParameterIndex, this);
            }
            if (m_EndTangentParameterIndex >= 0 && m_EndTangentParameter == nullptr) {
                pRavenLog->ErrorT(RavenSequence::Tag.GetCString(), "Parameter at index %d does not exist for %s! Ignoring.", m_EndTangentParameterIndex, this);
            }
            if (m_StartCameraParameterIndex >= 0 && m_StartCameraParameter == nullptr) {
                pRavenLog->ErrorT(RavenSequence::Tag.GetCString(), "Parameter at index %d does not exist for %s! Ignoring.", m_StartCameraParameterIndex, this);
            }
            if (m_EndCameraParameterIndex >= 0 && m_EndCameraParameter == nullptr) {
                pRavenLog->ErrorT(RavenSequence::Tag.GetCString(), "Parameter at index %d does not exist for %s! Ignoring.", m_EndCameraParameterIndex, this);
            }
        }
    } // namespace Raven
} // namespace Starlite
#endif