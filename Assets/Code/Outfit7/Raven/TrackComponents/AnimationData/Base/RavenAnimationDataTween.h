#ifdef STARLITE
#pragma once

#include <Enums/RavenValueType.h>
#include <TrackComponents/Components/RavenParameter.h>
#include <TrackComponents/AnimationData/Base/RavenAnimationDataBase.h>
#include <TrackComponents/Properties/RavenAnimationDataPropertyBase.h>
#include <Interpolators/Base/RavenValueInterpolator.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        template <class T> class RavenAnimationDataTween : public RavenAnimationDataBase<T> {
            SCLASS_TEMPLATE_ABSTRACT(RavenAnimationDataTween);

        public:
            RavenAnimationDataTween() = default;

            bool GetMirror() const;
            bool GetUseCustomEaseCurve() const;
            bool ShouldSyncStartingValues() const override;
            double GetEaseAmplitude() const;
            double GetEasePeriod() const;
            ERavenEaseType GetEaseType() const;
            int GetRepeatCount() const;
            Ref<AnimationCurve>& GetCustomCurve();
            T EvaluateAtTime(double time, double duration) final;
            void Initialize(Ptr<RavenSequence> sequence, Ptr<RavenAnimationPropertyComponentBase> property) override;
            void SetCustomCurve(const AnimationCurve* value);
            void SetEaseAmplitude(double value);
            void SetEasePeriod(double value);
            void SetEaseType(ERavenEaseType value);
            void SetUseCustomEaseCurve(bool value);

        protected:
            double GetCurrentTime(double time, double duration) const;
            T GetStartValue(const T& currentValue, const T& valueStart, const T& valueEnd, ERavenValueType valueType, const Ref<RavenParameter>& parameter, bool isObjectLink) const;
            virtual bool GetRemap() const;
            virtual bool PostEvaluateAtTime(const T& startValue, const T& endValue, double t, T& value);
            virtual T PerformRemap(const T& startValue, const T& endValue, double t);
            virtual void ValidateParameters();
            void CopyValuesCallback(const RavenAnimationDataComponentBase* other) override;
            void OnEnterCallback() override;
            void OnExitCallback() override;
            void SetStartingValues(const T& values) override;
            void SyncStartingValues(const T& values) override;

        protected:
            SPROPERTY(CustomAttributes : ["Raven.VisibleConditionAttribute(\"m_StartParameterIndex < 0\")"], Access : "protected");
            T m_StartValueStart;

            SPROPERTY(CustomAttributes : ["Raven.VisibleConditionAttribute(\"m_StartParameterIndex < 0\")"], Access : "protected");
            T m_StartValueEnd;

            SPROPERTY(CustomAttributes : ["Raven.VisibleConditionAttribute(\"m_StartParameterIndex < 0\")"], Access : "protected");
            ERavenValueType m_StartValueType = ERavenValueType::Constant;

            SPROPERTY(CustomAttributes : ["Raven.VisibleConditionAttribute(\"m_StartParameterIndex < 0\")"], Access : "protected");
            bool m_StartValueIsObjectLink = false;

            SPROPERTY(CustomAttributes : ["Raven.VisibleConditionAttribute(\"m_StartParameterIndex < 0\")"], Access : "protected");
            Ref<Object> m_StartValueObjectLink;

            SPROPERTY(CustomAttributes : ["Raven.VisibleConditionAttribute(\"m_EndParameterIndex < 0\")"], Access : "protected");
            T m_EndValueStart;

            SPROPERTY(CustomAttributes : ["Raven.VisibleConditionAttribute(\"m_EndParameterIndex < 0\")"], Access : "protected");
            T m_EndValueEnd;

            SPROPERTY(CustomAttributes : ["Raven.VisibleConditionAttribute(\"m_EndParameterIndex < 0\")"], Access : "protected");
            ERavenValueType m_EndValueType = ERavenValueType::Constant;

            SPROPERTY(CustomAttributes : ["Raven.VisibleConditionAttribute(\"m_EndParameterIndex < 0\")"], Access : "protected");
            bool m_EndValueIsObjectLink = false;

            SPROPERTY(CustomAttributes : ["Raven.VisibleConditionAttribute(\"m_EndParameterIndex < 0\")"], Access : "protected");
            Ref<Object> m_EndValueObjectLink;

            SPROPERTY(CustomAttributes : ["UnityEngine.HeaderAttribute(\"Parameters\")"], Access : "protected");
            int m_StartParameterIndex = -1;

            SPROPERTY(Access : "protected");
            int m_EndParameterIndex = -1;

            SPROPERTY(CustomAttributes : ["UnityEngine.HeaderAttribute(\"Easing\")"], Access : "protected");
            bool m_UseCustomEaseCurve = false;

            SPROPERTY(CustomAttributes : ["Raven.VisibleConditionAttribute(\"!m_UseCustomEaseCurve\")"], Access : "protected");
            ERavenEaseType m_EaseType;

            SPROPERTY(CustomAttributes
                      : ["Raven.VisibleConditionAttribute(\"m_UseCustomEaseCurve\")"], Access
                      : "protected", Default
                      : "new UnityEngine.AnimationCurve(new UnityEngine.Keyframe(0f, 0f), new UnityEngine.Keyframe(1f, 1f))");
            Ref<AnimationCurve> m_EaseCurve;

            SPROPERTY(CustomAttributes : ["Raven.VisibleConditionAttribute(\"!m_UseCustomEaseCurve\")"], Access : "protected");
            Double m_EaseAmplitude = 1.0;

            SPROPERTY(CustomAttributes : ["Raven.VisibleConditionAttribute(\"!m_UseCustomEaseCurve\")"], Access : "protected");
            Double m_EasePeriod = 1.0;

            SPROPERTY(CustomAttributes : ["UnityEngine.HeaderAttribute(\"Repeat\")"], Access : "protected");
            int m_RepeatCount = 1;

            SPROPERTY(CustomAttributes : ["Raven.VisibleConditionAttribute(\"m_RepeatCount > 1\")"], Access : "protected");
            bool m_Mirror = false;

            Ref<RavenParameter> m_EndParameter;
            Ref<RavenParameter> m_EndTangentParameter;
            Ref<RavenParameter> m_StartParameter;
            Ref<RavenParameter> m_StartTangentParameter;
            T m_EndValue;
            T m_StartValue;
        };

        template <class T> bool RavenAnimationDataTween<T>::GetMirror() const {
            return m_Mirror;
        }

        template <class T> bool RavenAnimationDataTween<T>::GetUseCustomEaseCurve() const {
            return m_UseCustomEaseCurve;
        }

        template <class T> bool RavenAnimationDataTween<T>::ShouldSyncStartingValues() const {
            // We'll handle logic in the SyncStartingValues function
            return true;
        }

        template <class T> double RavenAnimationDataTween<T>::GetEaseAmplitude() const {
            return m_EaseAmplitude;
        }

        template <class T> double RavenAnimationDataTween<T>::GetEasePeriod() const {
            return m_EasePeriod;
        }

        template <class T> ERavenEaseType RavenAnimationDataTween<T>::GetEaseType() const {
            return m_EaseType;
        }

        template <class T> int RavenAnimationDataTween<T>::GetRepeatCount() const {
            return m_RepeatCount;
        }

        template <class T> Ref<AnimationCurve>& RavenAnimationDataTween<T>::GetCustomCurve() {
            return m_EaseCurve;
        }

        template <class T> T RavenAnimationDataTween<T>::EvaluateAtTime(double time, double duration) {
            T startValue = m_StartParameter ? this->GetValueFromParameter(m_StartParameter) : (m_StartValueIsObjectLink ? this->m_Property->GetValue(m_StartValueObjectLink) : m_StartValue);
            T endValue = m_EndParameter ? this->GetValueFromParameter(m_EndParameter) : (m_EndValueIsObjectLink ? this->m_Property->GetValue(m_EndValueObjectLink) : m_EndValue);
            double t = m_UseCustomEaseCurve ? m_EaseCurve->Evaluate((float)this->GetNormalizedTime(GetCurrentTime(time, duration), duration))
                                            : this->GetNormalizedTime(GetCurrentTime(time, duration), duration, m_EaseType, m_EaseAmplitude, m_EasePeriod);
            if (GetRemap()) {
                return PerformRemap(startValue, endValue, t);
            }
            T value = T();
            if (PostEvaluateAtTime(startValue, endValue, t, value)) {
                return value;
            }
            return RavenValueInterpolator<T>::Interpolate(startValue, endValue, t);
        }

        template <class T> void RavenAnimationDataTween<T>::Initialize(Ptr<RavenSequence> sequence, Ptr<RavenAnimationPropertyComponentBase> property) {
            RavenAnimationDataBase<T>::Initialize(sequence, property);
            m_StartParameter = sequence->GetParameterAtIndex(m_StartParameterIndex);
            m_EndParameter = sequence->GetParameterAtIndex(m_EndParameterIndex);
            ValidateParameters();
        }

        template <class T> void RavenAnimationDataTween<T>::SetCustomCurve(const AnimationCurve* value) {
            m_EaseCurve = value;
        }

        template <class T> void RavenAnimationDataTween<T>::SetEaseAmplitude(double value) {
            m_EaseAmplitude = value;
        }

        template <class T> void RavenAnimationDataTween<T>::SetEasePeriod(double value) {
            m_EasePeriod = value;
        }

        template <class T> void RavenAnimationDataTween<T>::SetEaseType(ERavenEaseType value) {
            m_EaseType = value;
        }

        template <class T> void RavenAnimationDataTween<T>::SetUseCustomEaseCurve(bool value) {
            m_UseCustomEaseCurve = value;
        }

        template <class T> double RavenAnimationDataTween<T>::GetCurrentTime(double time, double duration) const {
            if (m_RepeatCount > 1) {
                return this->GetTimeForRepeatableMirror(time, duration, m_RepeatCount, m_Mirror);
            }

            return time;
        }

        template <class T>
        T RavenAnimationDataTween<T>::GetStartValue(const T& currentValue, const T& valueStart, const T& valueEnd, ERavenValueType valueType, const Ref<RavenParameter>& parameter,
                                                    bool isObjectLink) const {
            if (parameter) {
                return currentValue;
            }

            if (isObjectLink) {
                return currentValue;
            }

            switch (valueType) {
            case ERavenValueType::Constant:
                return valueStart;

            case ERavenValueType::Range:
                return RavenValueInterpolator<T>::Random(valueStart, valueEnd);

            case ERavenValueType::Current:
                return currentValue;
            }

            return T();
        }

        template <class T> bool RavenAnimationDataTween<T>::GetRemap() const {
            return false;
        }

        template <class T> bool RavenAnimationDataTween<T>::PostEvaluateAtTime(const T& startValue, const T& endValue, double t, T& value) {
            return false;
        }

        template <class T> T RavenAnimationDataTween<T>::PerformRemap(const T& startValue, const T& endValue, double t) {
            return T();
        }

        template <class T> void RavenAnimationDataTween<T>::ValidateParameters() {
            if (m_StartParameterIndex >= 0 && m_StartParameter == nullptr) {
                Internal::pRavenLog->ErrorT(RavenSequence::Tag.GetCString(), "Parameter at index %d does not exist for %s! Ignoring.", m_StartParameterIndex, this);
            }
            if (m_EndParameterIndex >= 0 && m_EndParameter == nullptr) {
                Internal::pRavenLog->ErrorT(RavenSequence::Tag.GetCString(), "Parameter at index %d does not exist for %s! Ignoring.", m_EndParameterIndex, this);
            }
        }

        template <class T> void RavenAnimationDataTween<T>::CopyValuesCallback(const RavenAnimationDataComponentBase* other) {
            const RavenAnimationDataTween<T>* otherReal = reinterpret_cast<const RavenAnimationDataTween<T>*>(other);

            m_Mirror = otherReal->m_Mirror;
            m_EaseAmplitude = otherReal->m_EaseAmplitude;
            m_EaseCurve = Object::New<AnimationCurve>();
            m_EaseCurve->SetKeyframes(otherReal->m_EaseCurve->GetKeyframes());
            m_EasePeriod = otherReal->m_EasePeriod;
            m_EaseType = otherReal->m_EaseType;
            m_EndParameter = otherReal->m_EndParameter;
            m_EndParameterIndex = otherReal->m_EndParameterIndex;
            m_EndValueEnd = otherReal->m_EndValueEnd;
            m_EndValueStart = otherReal->m_EndValueStart;
            m_EndValueType = otherReal->m_EndValueType;
            m_EndValueIsObjectLink = otherReal->m_EndValueIsObjectLink;
            m_EndValueObjectLink = otherReal->m_EndValueObjectLink;
            m_RepeatCount = otherReal->m_RepeatCount;
            m_StartParameterIndex = otherReal->m_StartParameterIndex;
            m_StartValueEnd = otherReal->m_StartValueEnd;
            m_StartValueStart = otherReal->m_StartValueStart;
            m_StartValueType = otherReal->m_StartValueType;
            m_StartValueIsObjectLink = otherReal->m_StartValueIsObjectLink;
            m_StartValueObjectLink = otherReal->m_StartValueObjectLink;
            m_UseCustomEaseCurve = otherReal->m_UseCustomEaseCurve;
        }

        template <class T> void RavenAnimationDataTween<T>::OnEnterCallback() {
            m_StartValue = GetStartValue(m_StartValue, m_StartValueStart, m_StartValueEnd, m_StartValueType, m_StartParameter, m_StartValueIsObjectLink);
            m_EndValue = GetStartValue(m_EndValue, m_EndValueStart, m_EndValueEnd, m_EndValueType, m_EndParameter, m_EndValueIsObjectLink);
        }

        template <class T> void RavenAnimationDataTween<T>::OnExitCallback() {
        }

        template <class T> void RavenAnimationDataTween<T>::SetStartingValues(const T& values) {
            m_StartValueStart = values;
            m_StartValueEnd = values;
            m_EndValueStart = values;
            m_EndValueEnd = values;
        }

        template <class T> void RavenAnimationDataTween<T>::SyncStartingValues(const T& values) {
            if (m_StartParameterIndex < 0 && m_StartValueType == ERavenValueType::Current) {
                m_StartValue = values;
            }
            if (m_EndParameterIndex < 0 && m_EndValueType == ERavenValueType::Current) {
                m_EndValue = values;
            }
        }
    } // namespace Raven
} // namespace Starlite
#endif
