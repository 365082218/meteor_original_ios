#ifdef STARLITE
#pragma once

#include <Scripts/Compiled/RavenTriggerPropertyBase.h>
#include <TrackComponents/Properties/Base/RavenAnimationPropertyBase.h>

#include <Utility/RavenLog.h>

namespace Starlite {
    namespace Raven {
        using namespace Compiler;

        template <class T> class RavenAnimationDataPropertyBase : public RavenAnimationPropertyBase<T> {
            SCLASS_TEMPLATE_ABSTRACT(RavenAnimationDataPropertyBase);

        public:
            RavenAnimationDataPropertyBase();
            bool IsCustom() const final;
            T GetValue(Object* targetComponent) const final;
            void Initialize(Ptr<RavenSequence> sequence) final;

        protected:
            bool IsCustomValid() const final;
            void PostEvaluateAtTime(double time, double duration, const T& value, Object* targetComponent) final;
            void SetValue(const T& value, Object* targetComponent) final;

        private:
            Ref<RavenTriggerPropertyBase1<T>> m_TriggerPropertyCast;

            Ptr<ReflectionProperty> m_ReflectionProperty;
        };

        template <class T> RavenAnimationDataPropertyBase<T>::RavenAnimationDataPropertyBase() {
        }

        template <class T> bool RavenAnimationDataPropertyBase<T>::IsCustom() const {
            return false;
        }

        template <class T> T RavenAnimationDataPropertyBase<T>::GetValue(Object* targetComponent) const {
            if (m_ReflectionProperty->IsGetter()) {
                return m_ReflectionProperty->Get(targetComponent);
            }
            auto variant = m_ReflectionProperty->GetVariant(targetComponent);
            return variant.template Get<T>();
        }

        template <class T> void RavenAnimationDataPropertyBase<T>::Initialize(Ptr<RavenSequence> sequence) {
            RavenAnimationPropertyBase<T>::Initialize(sequence);
            m_TriggerPropertyCast = reinterpret_cast<RavenTriggerPropertyBase1<T>*>(this->m_TriggerProperty.GetObject());

            auto type = pReflection->GetType(this->m_TargetComponent->GetObjectTypeId());
            const String& memberName = this->GetMemberName();
            auto index = type->FindPropertyIndex(memberName);
            if (index == (Size_T)-1) {
                Internal::pRavenLog->ErrorT(RavenSequence::Tag.GetCString(), "%s failed to find reflection property %s on type %s(%u)", this, memberName, this->m_TargetComponent->GetObjectTypeName(),
                                            this->m_TargetComponent->GetObjectTypeId());
            }
            int property_index = (int)index;
            m_ReflectionProperty = type->GetProperties()[property_index];
            DebugAssert(m_ReflectionProperty->GetName() == memberName, "Property names do not match '%s' != '%s'!", m_ReflectionProperty->GetName().GetCString(), memberName.GetCString());
        }

        template <class T> bool RavenAnimationDataPropertyBase<T>::IsCustomValid() const {
            return false;
        }

        template <class T> void RavenAnimationDataPropertyBase<T>::PostEvaluateAtTime(double time, double duration, const T& value, Object* targetComponent) {
            if (m_TriggerPropertyCast) {
                if (!this->m_ExecuteFunctionOnly) {
                    SetValue(value, targetComponent);
                }
                m_TriggerPropertyCast->ManualExecute(value);
            } else {
                SetValue(value, targetComponent);
            }
        }

        template <class T> void RavenAnimationDataPropertyBase<T>::SetValue(const T& value, Object* targetComponent) {
            if (m_ReflectionProperty->IsSetter()) {
                m_ReflectionProperty->Set(targetComponent, value);
                return;
            }
            auto variant = m_ReflectionProperty->GetVariant(targetComponent);
            T& variantValue = variant.template Get<T>();
            variantValue = value;
        }
    } // namespace Raven
} // namespace Starlite
#endif