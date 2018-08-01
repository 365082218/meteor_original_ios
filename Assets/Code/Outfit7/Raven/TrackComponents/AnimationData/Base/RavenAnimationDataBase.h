#ifdef STARLITE
#pragma once

#include <Utility/RavenEaseUtility.h>
#include <RavenSequence.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        template <class T> class RavenAnimationPropertyBase;

        template <class T> class RavenAnimationDataBase : public RavenAnimationDataComponentBase {
            SCLASS_TEMPLATE_ABSTRACT(RavenAnimationDataBase);

        public:
            RavenAnimationDataBase();
            virtual bool ShouldSyncStartingValues() const = 0;
            virtual T EvaluateAtTime(double time, double duration) = 0;
            virtual void PostprocessFinalValue(T& value, Object* targetObject) const;
            void Initialize(Ptr<RavenSequence> sequence, Ptr<RavenAnimationPropertyComponentBase> property) override;
            void OnEnter();
            void OnExit();
            void TrySyncStartingValues(const T& values);

        protected:
            bool IsObjectValueComponent(const Ref<Object>& obj) const;
            double GetNormalizedTime(double time, double duration) const;
            double GetNormalizedTime(double time, double duration, ERavenEaseType easeType) const;
            double GetNormalizedTime(double time, double duration, ERavenEaseType easeType, double amplitude, double period) const;
            double GetTimeForRepeatableMirror(double time, double duration, int repeatCount, bool mirror) const;
            virtual T GetValueFromParameter(RavenParameter* parameter) const;
            virtual T GetValueFromParameterCallback(const RavenParameter* parameter) const = 0;
            virtual void OnEnterCallback() = 0;
            virtual void OnExitCallback() = 0;
            virtual void SetStartingValues(const T& values) = 0;
            virtual void SyncStartingValues(const T& values) = 0;

        protected:
            Ref<RavenAnimationPropertyBase<T>> m_Property;
        };

        template <class T> RavenAnimationDataBase<T>::RavenAnimationDataBase() {
        }

        template <class T> void RavenAnimationDataBase<T>::PostprocessFinalValue(T& value, Object* targetObject) const {
        }

        template <class T> void RavenAnimationDataBase<T>::Initialize(Ptr<RavenSequence> sequence, Ptr<RavenAnimationPropertyComponentBase> property) {
            m_Property = reinterpret_cast<RavenAnimationPropertyBase<T>*>(property.GetPointer());
        }

        template <class T> void RavenAnimationDataBase<T>::OnEnter() {
            OnEnterCallback();
        }

        template <class T> void RavenAnimationDataBase<T>::OnExit() {
            OnExitCallback();
        }

        template <class T> void RavenAnimationDataBase<T>::TrySyncStartingValues(const T& values) {
            if (ShouldSyncStartingValues()) {
                SyncStartingValues(values);
            }
        }

        template <class T> bool RavenAnimationDataBase<T>::IsObjectValueComponent(const Ref<Object>& obj) const {
            return obj != nullptr && (obj->GetObjectTypeId() == SceneObject::TypeId || obj->GetObjectTypeId() == SceneObjectComponent::TypeId || obj->GetObjectTypeId() == UiSceneObject::TypeId);
        }

        template <class T> double RavenAnimationDataBase<T>::GetNormalizedTime(double time, double duration) const {
            return time / duration;
        }

        template <class T> double RavenAnimationDataBase<T>::GetNormalizedTime(double time, double duration, ERavenEaseType easeType) const {
            auto value = RavenEaseUtility::Evaluate(easeType, time, duration, 1, 1);
            return value;
        }

        template <class T> double RavenAnimationDataBase<T>::GetNormalizedTime(double time, double duration, ERavenEaseType easeType, double amplitude, double period) const {
            auto value = RavenEaseUtility::Evaluate(easeType, time, duration, amplitude, period);
            return value;
        }

        template <class T> double RavenAnimationDataBase<T>::GetTimeForRepeatableMirror(double time, double duration, int repeatCount, bool mirror) const {
            auto repeatInterval = duration / repeatCount;
            auto repeatIndexTime = time / repeatInterval;
            auto repeatIndex = (int)repeatIndexTime;
            auto repeatIndexRemainder = repeatIndexTime - repeatIndex;
            auto newTime = repeatIndexRemainder * duration;
            auto flip = mirror ? repeatIndex % 2 == 1 : repeatIndexRemainder == 0 && time != 0; // mad logic -- last bool check handles case where duration == time because it would return 0 otherwise
            return flip ? duration - newTime : newTime;
        }

        template <class T> T RavenAnimationDataBase<T>::GetValueFromParameter(RavenParameter* parameter) const {
            if (parameter->m_ParameterType == ERavenParameterType::Object && IsObjectValueComponent(parameter->m_ValueObject)) {
                return m_Property->GetValue(parameter->m_ValueObject);
            }

            return GetValueFromParameterCallback(parameter);
        }
    } // namespace Raven
} // namespace Starlite
#endif