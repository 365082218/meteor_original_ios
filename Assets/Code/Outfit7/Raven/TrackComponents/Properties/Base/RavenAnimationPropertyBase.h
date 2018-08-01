#ifdef STARLITE
#pragma once

#include <TrackComponents/Properties/Base/RavenAnimationPropertyComponentBase.h>
#include <TrackComponents/AnimationData/Base/RavenAnimationDataBase.h>
#include <Interpolators/Base/RavenValueInterpolator.h>
#include <Enums/RavenAnimationPropertyType.h>
#include <Utility/RavenLog.h>
#include <RavenSequence.h>

namespace Starlite {
    namespace Raven {
        template <class T> class RavenAnimationDataBase;

        template <class T> class RavenAnimationPropertyBase : public RavenAnimationPropertyComponentBase {
            SCLASS_TEMPLATE_ABSTRACT(RavenAnimationPropertyBase);

        public:
            RavenAnimationPropertyBase();
            bool CheckForDependencies() const override;
            ERavenAnimationPropertyType GetPropertyType() const;
            int GetParameterIndex() const final;
            Ref<RavenAnimationDataComponentBase>& GetAnimationData() final;
            Ref<RavenTriggerPropertyComponentBase>& GetTriggerProperty() final;
            virtual T GetValue(Object* targetComponent) const = 0;
            void EvaluateAtTime(double time, double duration) final;
            void Initialize(Ptr<RavenSequence> sequence) override;
            void OnEnter() override;
            void OnExit() override;
            void SetAnimationData(Ref<RavenAnimationDataComponentBase>& value) final;
            void SetTriggerProperty(Ref<RavenTriggerPropertyComponentBase>& value) final;

        protected:
            virtual void ProcessValueComponents(T& value, Object* targetComponent);
            virtual void PostEvaluateAtTime(double time, double duration, const T& value, Object* targetComponent) = 0;
            virtual void SetValue(const T& value, Object* targetComponent) = 0;
            void OnSetTargets(const List<Ref<SceneObject>>& gameObjects) override;

        protected:
            SPROPERTY(CustomAttributes : ["UnityEngine.HeaderAttribute(\"Settings\")"], Access : "protected");
            ERavenAnimationPropertyType m_PropertyType = ERavenAnimationPropertyType::Set;

            SPROPERTY(CustomAttributes : ["Raven.VisibleConditionAttribute(\"m_ApplyOffset\")"], Access : "protected");
            T m_Offset;

            SPROPERTY(CustomAttributes : ["Raven.VisibleConditionAttribute(\"m_ApplyMultiplier\")"], Access : "protected");
            Double m_Multiplier = 1.0;

            SPROPERTY(Access : "protected");
            bool m_ApplyMultiplier = false;

            SPROPERTY(Access : "protected");
            bool m_ApplyOffset = false;

            SPROPERTY(Access : "protected");
            bool m_ExecuteFunctionOnly = false;

            Ref<RavenAnimationDataBase<T>> m_AnimationDataCast;

        private:
            Array<T> m_OverridenEnterValues;
            T m_EnterValue;
        };

        template <class T> RavenAnimationPropertyBase<T>::RavenAnimationPropertyBase() {
        }

        template <class T> bool RavenAnimationPropertyBase<T>::CheckForDependencies() const {
            return true;
        }

        template <class T> ERavenAnimationPropertyType RavenAnimationPropertyBase<T>::GetPropertyType() const {
            return m_PropertyType;
        }

        template <class T> int RavenAnimationPropertyBase<T>::GetParameterIndex() const {
            return -1;
        }

        template <class T> Ref<RavenAnimationDataComponentBase>& RavenAnimationPropertyBase<T>::GetAnimationData() {
            return m_AnimationData;
        }

        template <class T> Ref<RavenTriggerPropertyComponentBase>& RavenAnimationPropertyBase<T>::GetTriggerProperty() {
            return m_TriggerProperty;
        }

        template <class T> void RavenAnimationPropertyBase<T>::EvaluateAtTime(double time, double duration) {
            bool hasOverridenTargetComponents = HasOverridenTargetComponents();
            Size_T iterationCount = hasOverridenTargetComponents ? m_OverridenTargetComponents.Count() : 1;

            for (Size_T i = 0; i < iterationCount; ++i) {
                T enterValue;
                Object* targetComponent;
                if (hasOverridenTargetComponents) {
                    // have to do this because animation data holds only 1 data and objects might have different starting data points
                    m_AnimationDataCast->TrySyncStartingValues(m_OverridenEnterValues[i]);
                    enterValue = m_OverridenEnterValues[i];
                    targetComponent = m_OverridenTargetComponents[i];
                } else {
                    enterValue = m_EnterValue;
                    targetComponent = m_TargetComponent;
                }

                T value = m_AnimationDataCast->EvaluateAtTime(time, duration);
                if (m_ApplyMultiplier) {
                    value = RavenValueInterpolator<T>::MultiplyScalar(value, m_Multiplier);
                }
                if (m_ApplyOffset) {
                    value = RavenValueInterpolator<T>::Add(value, m_Offset);
                }

                switch (m_PropertyType) {
                case ERavenAnimationPropertyType::Set:
                    ProcessValueComponents(value, targetComponent);
                    break;

                case ERavenAnimationPropertyType::Add:
                    value = RavenValueInterpolator<T>::Add(enterValue, value);
                    break;

                case ERavenAnimationPropertyType::RelativeAdd:
                    value = RavenValueInterpolator<T>::Add(GetValue(targetComponent), value);
                    break;

                case ERavenAnimationPropertyType::Multiply:
                    value = RavenValueInterpolator<T>::Multiply(enterValue, value);
                    break;

                case ERavenAnimationPropertyType::RelativeMultiply:
                    value = RavenValueInterpolator<T>::Multiply(GetValue(targetComponent), value);
                    break;

                default:
                    Internal::pRavenLog->ErrorT(RavenSequence::Tag.GetCString(), "%s not handled", m_PropertyType);
                    break;
                }

                m_AnimationDataCast->PostprocessFinalValue(value, targetComponent);
                // This should not modify the value any further!
                PostEvaluateAtTime(time, duration, value, targetComponent);
            }
        }

        template <class T> void RavenAnimationPropertyBase<T>::Initialize(Ptr<RavenSequence> sequence) {
            RavenAnimationPropertyComponentBase::Initialize(sequence);
            m_AnimationDataCast = reinterpret_cast<RavenAnimationDataBase<T>*>(m_AnimationData.GetObject());
        }

        template <class T> void RavenAnimationPropertyBase<T>::OnEnter() {
            if (HasOverridenTargetComponents()) {
                for (Size_T i = 0; i < m_OverridenTargetComponents.Count(); ++i) {
                    m_OverridenEnterValues[i] = GetValue(m_OverridenTargetComponents[i]);
                }
            } else {
                m_EnterValue = GetValue(m_TargetComponent);
                m_AnimationDataCast->TrySyncStartingValues(m_EnterValue);
            }
            m_AnimationDataCast->OnEnter();
        }

        template <class T> void RavenAnimationPropertyBase<T>::OnExit() {
            m_AnimationDataCast->OnExit();
        }

        template <class T> void RavenAnimationPropertyBase<T>::SetAnimationData(Ref<RavenAnimationDataComponentBase>& value) {
            m_AnimationData = value;
            m_AnimationDataCast = reinterpret_cast<RavenAnimationDataBase<T>*>(m_AnimationData.GetObject());
        }

        template <class T> void RavenAnimationPropertyBase<T>::SetTriggerProperty(Ref<RavenTriggerPropertyComponentBase>& value) {
            m_TriggerProperty = value;
        }

        template <class T> void RavenAnimationPropertyBase<T>::ProcessValueComponents(T& value, Object* targetComponent) {
        }

        template <class T> void RavenAnimationPropertyBase<T>::OnSetTargets(const List<Ref<SceneObject>>& gameObjects) {
            RavenAnimationPropertyComponentBase::OnSetTargets(gameObjects);
            if (m_OverridenEnterValues.Size() != gameObjects.Count()) {
                auto tmp = m_OverridenEnterValues;
                m_OverridenEnterValues.SetSize(gameObjects.Count());
                memcpy(m_OverridenEnterValues.GetData(), tmp.GetData(), Math::Min(tmp.Size(), m_OverridenEnterValues.Size()) * sizeof(T));
            }
        }
    } // namespace Raven
} // namespace Starlite
#endif