#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataBase.h>
#include <TrackComponents/Properties/Base/RavenAnimationPropertyBase.h>
#include <Enums/RavenValueType.h>

namespace Starlite {
    namespace Raven {
        template <class T> class RavenAnimationDataCurve : public RavenAnimationDataBase<T> {
            SCLASS_TEMPLATE_ABSTRACT(RavenAnimationDataCurve);

        public:
            RavenAnimationDataCurve();
            bool GetMirror() const;
            bool GetSyncToCurrent() const;
            bool ShouldSyncStartingValues() const override;
            const Array<Ref<AnimationCurve>>& GetCurves() const;
            ERavenEaseType GetEaseType() const;
            int GetRepeatCount() const;
            void Initialize(Ptr<RavenSequence> sequence, Ptr<RavenAnimationPropertyComponentBase> property) override;
            void SetEaseType(ERavenEaseType value);
            void SetSyncToCurrent(bool value);

        protected:
            double GetCurrentTime(double time, double duration) const;
            T GetValueFromParameterCallback(const RavenParameter* parameter) const final;
            void CopyValuesCallback(const RavenAnimationDataComponentBase* other) override;
            void OnEnterCallback() override;
            void OnExitCallback() override;

        protected:
            SPROPERTY(CustomAttributes : ["UnityEngine.HeaderAttribute(\"Settings\")"], Access : "protected");
            Array<Ref<AnimationCurve>> m_Curves;

            SPROPERTY(Access : "protected");
            ERavenEaseType m_EaseType = ERavenEaseType::Linear;

            SPROPERTY(Access : "protected");
            ERavenValueType m_ValueType = ERavenValueType::Constant;

            SPROPERTY(CustomAttributes : ["UnityEngine.HeaderAttribute(\"Repeat\")"], Access : "protected");
            int m_RepeatCount = 1;

            SPROPERTY(CustomAttributes : ["Raven.VisibleConditionAttribute(\"m_RepeatCount > 1\")"], Access : "protected");
            bool m_Mirror = false;
        };

        template <class T> RavenAnimationDataCurve<T>::RavenAnimationDataCurve() {
        }

        template <class T> bool RavenAnimationDataCurve<T>::GetMirror() const {
            return m_Mirror;
        }

        template <class T> bool RavenAnimationDataCurve<T>::GetSyncToCurrent() const {
            return m_ValueType == ERavenValueType::Current;
        }

        template <class T> bool RavenAnimationDataCurve<T>::ShouldSyncStartingValues() const {
            return m_ValueType == ERavenValueType::Current;
        }

        template <class T> const Array<Ref<AnimationCurve>>& RavenAnimationDataCurve<T>::GetCurves() const {
            return m_Curves;
        }

        template <class T> ERavenEaseType RavenAnimationDataCurve<T>::GetEaseType() const {
            return m_EaseType;
        }

        template <class T> int RavenAnimationDataCurve<T>::GetRepeatCount() const {
            return m_RepeatCount;
        }

        template <class T> void RavenAnimationDataCurve<T>::Initialize(Ptr<RavenSequence> sequence, Ptr<RavenAnimationPropertyComponentBase> property) {
            RavenAnimationDataBase<T>::Initialize(sequence, property);
        }

        template <class T> void RavenAnimationDataCurve<T>::SetEaseType(ERavenEaseType value) {
            m_EaseType = value;
        }

        template <class T> void RavenAnimationDataCurve<T>::SetSyncToCurrent(bool value) {
            if (value) {
                m_ValueType = ERavenValueType::Current;
            } else {
                m_ValueType = ERavenValueType::Constant;
            }
        }

        template <class T> double RavenAnimationDataCurve<T>::GetCurrentTime(double time, double duration) const {
            if (m_RepeatCount > 1) {
                return this->GetTimeForRepeatableMirror(time, duration, m_RepeatCount, m_Mirror);
            }

            return time;
        }

        template <class T> T RavenAnimationDataCurve<T>::GetValueFromParameterCallback(const RavenParameter* parameter) const {
            Internal::pRavenLog->ErrorT(RavenSequence::Tag.GetCString(), "Not implemented!");
            return T();
        }

        template <class T> void RavenAnimationDataCurve<T>::CopyValuesCallback(const RavenAnimationDataComponentBase* other) {
            auto otherReal = reinterpret_cast<const RavenAnimationDataCurve<T>*>(other);
            m_Curves = Array<Ref<AnimationCurve>>(otherReal->m_Curves.Size());
            for (int i = 0; i < otherReal->m_Curves.Size(); ++i) {
                m_Curves[i] = Object::New<AnimationCurve>();
                m_Curves[i]->SetKeyframes(otherReal->m_Curves[i]->GetKeyframes());
            }

            m_EaseType = otherReal->m_EaseType;
            m_ValueType = otherReal->m_ValueType;
            m_RepeatCount = otherReal->m_RepeatCount;
            m_Mirror = otherReal->m_Mirror;
        }

        template <class T> void RavenAnimationDataCurve<T>::OnEnterCallback() {
        }

        template <class T> void RavenAnimationDataCurve<T>::OnExitCallback() {
        }
    } // namespace Raven
} // namespace Starlite
#endif