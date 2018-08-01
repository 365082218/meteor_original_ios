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
        template <class T> class RavenAnimationDataSet : public RavenAnimationDataBase<T> {
            SCLASS_TEMPLATE_ABSTRACT(RavenAnimationDataSet);

        public:
            RavenAnimationDataSet() = default;
            bool ShouldSyncStartingValues() const override;
            T EvaluateAtTime(double time, double duration) override;
            void Initialize(Ptr<RavenSequence> sequence, Ptr<RavenAnimationPropertyComponentBase> property) override;

        protected:
            T GetStartValue(const T& currentValue, const T& valueStart, const T& valueEnd, ERavenValueType valueType, const Ref<RavenParameter>& parameter, bool isObjectLink) const;
            void CopyValuesCallback(const RavenAnimationDataComponentBase* other) override;
            void OnEnterCallback() override;
            void OnExitCallback() override;
            void SetStartingValues(const T& values) override;
            void SyncStartingValues(const T& values) override;

        private:
            void ValidateParameters();

        protected:
            SPROPERTY(CustomAttributes : ["Raven.VisibleConditionAttribute(\"m_StartParameterIndex < 0\")"], Access : "protected");
            T m_StartValueStart;

            SPROPERTY(CustomAttributes : ["Raven.VisibleConditionAttribute(\"m_StartParameterIndex < 0\")"], Access : "protected");
            T m_StartValueEnd;

            SPROPERTY(CustomAttributes : ["Raven.VisibleConditionAttribute(\"m_StartParameterIndex < 0\")"], Access : "protected");
            ERavenValueType m_StartValueType = ERavenValueType::Constant;

            SPROPERTY(CustomAttributes : ["Raven.VisibleConditionAttribute(\"m_StartParameterIndex < 0\")"], Access : "protected");
            bool m_StartValueIsObjectLink;

            SPROPERTY(CustomAttributes : ["Raven.VisibleConditionAttribute(\"m_StartParameterIndex < 0\")"], Access : "protected");
            Ref<Object> m_StartValueObjectLink;

            SPROPERTY(CustomAttributes : ["UnityEngine.HeaderAttribute(\"Parameters\")"], Access : "protected");
            int m_StartParameterIndex = -1;

            Ref<RavenParameter> m_StartParameter;
            T m_StartValue;
        };

        template <class T> bool RavenAnimationDataSet<T>::ShouldSyncStartingValues() const {
            return m_StartValueType == ERavenValueType::Current;
        }

        template <class T> T RavenAnimationDataSet<T>::EvaluateAtTime(double time, double duration) {
            T startValue = m_StartParameter ? this->GetValueFromParameter(m_StartParameter) : (m_StartValueIsObjectLink ? this->m_Property->GetValue(m_StartValueObjectLink) : m_StartValue);
            return startValue;
        }

        template <class T> void RavenAnimationDataSet<T>::Initialize(Ptr<RavenSequence> sequence, Ptr<RavenAnimationPropertyComponentBase> property) {
        }

        template <class T>
        T RavenAnimationDataSet<T>::GetStartValue(const T& currentValue, const T& valueStart, const T& valueEnd, ERavenValueType valueType, const Ref<RavenParameter>& parameter,
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

        template <class T> void RavenAnimationDataSet<T>::CopyValuesCallback(const RavenAnimationDataComponentBase* other) {
            auto otherReal = reinterpret_cast<const RavenAnimationDataSet<T>*>(other);

            m_StartParameterIndex = otherReal->m_StartParameterIndex;
            m_StartValueEnd = otherReal->m_StartValueEnd;
            m_StartValueStart = otherReal->m_StartValueStart;
            m_StartValueType = otherReal->m_StartValueType;
            m_StartValueIsObjectLink = otherReal->m_StartValueIsObjectLink;
            m_StartValueObjectLink = otherReal->m_StartValueObjectLink;
        }

        template <class T> void RavenAnimationDataSet<T>::OnEnterCallback() {
            m_StartValue = GetStartValue(m_StartValue, m_StartValueStart, m_StartValueEnd, m_StartValueType, m_StartParameter, m_StartValueIsObjectLink);
        }

        template <class T> void RavenAnimationDataSet<T>::OnExitCallback() {
        }

        template <class T> void RavenAnimationDataSet<T>::SetStartingValues(const T& values) {
            m_StartValueStart = values;
            m_StartValueEnd = values;
        }

        template <class T> void RavenAnimationDataSet<T>::SyncStartingValues(const T& values) {
            if (m_StartParameterIndex < 0 && m_StartValueType == ERavenValueType::Current) {
                m_StartValue = values;
            }
        }

        template <class T> void RavenAnimationDataSet<T>::ValidateParameters() {
            if (m_StartParameterIndex >= 0 && m_StartParameter == nullptr) {
                Internal::pRavenLog->ErrorT(RavenSequence::Tag.GetCString(), "Parameter at index %d does not exist for %s! Ignoring.", m_StartParameterIndex, this);
            }
        }
    } // namespace Raven
} // namespace Starlite
#endif