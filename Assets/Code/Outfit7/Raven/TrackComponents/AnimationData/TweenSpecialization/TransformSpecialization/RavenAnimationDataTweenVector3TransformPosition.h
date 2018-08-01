#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/TweenSpecialization/TransformSpecialization/RavenAnimationDataTweenVector3Transform.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataTweenVector3TransformPosition : public RavenAnimationDataTweenVector3Transform {
            SCLASS(RavenAnimationDataTweenVector3TransformPosition);

        public:
            RavenAnimationDataTweenVector3TransformPosition();
            bool GetFromPointCasting() const;
            float GetCameraDepthDestination() const;
            int GetEndCameraParameterIndex() const;
            int GetStartCameraParameterIndex() const;
            const Ref<Camera>& GetEndCamera() const;
            const Ref<Camera>& GetStartCamera() const;
            void Initialize(Ptr<RavenSequence> sequence, Ptr<RavenAnimationPropertyComponentBase> property) override;
            void PostprocessFinalValue(Vector3& value, Object* targetObject) const override;
            void SetCameraDepthDestination(float value);
            void SetEndCamera(Ref<Camera>& value);
            void SetEndCameraParameterIndex(int value);
            void SetFromPointCasting(bool value);
            void SetStartCamera(Ref<Camera>& value);
            void SetStartCameraParameterIndex(int value);

        protected:
            bool PostEvaluateAtTime(const Vector3& startValue, const Vector3& endValue, double t, Vector3& value) override;
            void CopyValuesCallback(const RavenAnimationDataComponentBase* other) override;
            void OnEnterCallback() override;

        private:
            void ValidateCustomParameters();

        private:
            SPROPERTY(CustomAttributes : ["UnityEngine.HeaderAttribute(\"Tangents\")"], Access : "private");
            int m_StartTangentParameterIndex = -1;

            SPROPERTY(CustomAttributes : ["Raven.VisibleConditionAttribute(\"m_StartTangentParameterIndex < 0\")"], Access : "private");
            ERavenValueType m_StartTangentValueType = ERavenValueType::Constant;

            SPROPERTY(CustomAttributes : ["Raven.VisibleConditionAttribute(\"m_StartTangentParameterIndex < 0\")"], Access : "private");
            Vector3 m_StartTangentStart;

            SPROPERTY(CustomAttributes : ["Raven.VisibleConditionAttribute(\"m_StartTangentParameterIndex < 0\")"], Access : "private");
            Vector3 m_StartTangentEnd;

            SPROPERTY(Access : "private");
            int m_EndTangentParameterIndex = -1;

            SPROPERTY(CustomAttributes : ["Raven.VisibleConditionAttribute(\"m_EndTangentParameterIndex < 0\")"], Access : "private");
            ERavenValueType m_EndTangentValueType = ERavenValueType::Constant;

            SPROPERTY(CustomAttributes : ["Raven.VisibleConditionAttribute(\"m_EndTangentParameterIndex < 0\")"], Access : "private");
            Vector3 m_EndTangentStart;

            SPROPERTY(CustomAttributes : ["Raven.VisibleConditionAttribute(\"m_EndTangentParameterIndex < 0\")"], Access : "private");
            Vector3 m_EndTangentEnd;

            SPROPERTY(Access : "private");
            bool m_UseBezierCurve = false;

            SPROPERTY(Access : "private");
            bool m_RotateWithTangent = false;

            SPROPERTY(CustomAttributes : ["UnityEngine.HeaderAttribute(\"Camera Interpolation\")"], Access : "private");
            Ref<Camera> m_StartCamera;

            SPROPERTY(Access : "private");
            int m_StartCameraParameterIndex = -1;

            SPROPERTY(Access : "private");
            Ref<Camera> m_EndCamera;

            SPROPERTY(Access : "private");
            int m_EndCameraParameterIndex = -1;

            SPROPERTY(Access : "private");
            float m_CameraDepthDestination = 10;

            SPROPERTY(Access : "private");
            bool m_FromPointCasting = false;

            Ref<Camera> m_EndCameraValue;
            Ref<Camera> m_StartCameraValue;
            Ref<RavenParameter> m_EndCameraParameter;
            Ref<RavenParameter> m_StartCameraParameter;
            Vector3 m_CalculatedTangent;
            Vector3 m_EndTangentValue;
            Vector3 m_StartTangentValue;
        };
    } // namespace Raven
} // namespace Starlite
#endif