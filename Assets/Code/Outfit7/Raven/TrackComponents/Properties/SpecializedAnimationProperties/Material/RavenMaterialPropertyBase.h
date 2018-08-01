#ifdef STARLITE
#pragma once

#include <Scripts/Compiled/RavenTriggerPropertyBase.h>
#include <TrackComponents/Properties/Base/RavenAnimationPropertyBase.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        using namespace Compiler;

        template <class T> class RavenMaterialPropertyBase : public RavenAnimationPropertyBase<T> {
            SCLASS_TEMPLATE_ABSTRACT(RavenMaterialPropertyBase);

        public:
            RavenMaterialPropertyBase() = default;
            bool GetUseSharedMaterial() const;
            bool IsCustom() const final;
            int GetTargetMaterialIndex() const;
            String GetTargetMaterialProperty() const;
            void Initialize(Ptr<RavenSequence> sequence) final;
            void SetTargetMaterialIndex(int value);
            void SetTargetMaterialProperty(String value);
            void SetUseSharedMaterial(bool value);

        protected:
            bool IsCustomValid() const final;
            Array<Ref<Material>> GetAllMaterials(Object* targetComponent) const;
            Ref<Material>& GetMaterial(Object* targetComponent) const;
            void PostEvaluateAtTime(double time, double duration, const T& value, Object* targetComponent) final;

        protected:
            // At the moment this is not used because we do not support shared materials.
            SPROPERTY(Access : "protected");
            bool m_UseSharedMaterial = false;

            // At the moment this is not used because we do not support multiple materials per object
            SPROPERTY(Access : "protected");
            int m_TargetMaterialIndex = 0;

            SPROPERTY(Access : "protected");
            String m_TargetMaterialProperty = "";

            SPROPERTY(Access : "protected", CustomAttributes : ["Raven.VisibleConditionAttribute(\"!m_UseSharedMaterial\")"]);
            bool m_RestoreMaterialOnEnd = false;

        private:
            bool m_IsRenderer;
            Ref<RavenTriggerPropertyBase2<T, String>> m_TriggerPropertyCast;
            String m_CachedTargetMaterialProperty;
        };

        template <class T> bool RavenMaterialPropertyBase<T>::GetUseSharedMaterial() const {
            return m_UseSharedMaterial;
        }

        template <class T> bool RavenMaterialPropertyBase<T>::IsCustom() const {
            return true;
        }

        template <class T> int RavenMaterialPropertyBase<T>::GetTargetMaterialIndex() const {
            return m_TargetMaterialIndex;
        }

        template <class T> String RavenMaterialPropertyBase<T>::GetTargetMaterialProperty() const {
            return m_TargetMaterialProperty;
        }

        template <class T> void RavenMaterialPropertyBase<T>::Initialize(Ptr<RavenSequence> sequence) {
            RavenAnimationPropertyBase<T>::Initialize(sequence);
            m_TriggerPropertyCast = reinterpret_cast<RavenTriggerPropertyBase2<T, String>*>(this->m_TriggerProperty.GetObject());
            m_IsRenderer = this->m_TargetComponent->template IsDerivedFrom<Renderer>();
        }

        template <class T> void RavenMaterialPropertyBase<T>::SetTargetMaterialIndex(int value) {
            m_TargetMaterialIndex = value;
        }

        template <class T> void RavenMaterialPropertyBase<T>::SetTargetMaterialProperty(String value) {
            m_TargetMaterialProperty = value;
        }

        template <class T> void RavenMaterialPropertyBase<T>::SetUseSharedMaterial(bool value) {
            m_UseSharedMaterial = value;
        }

        template <class T> bool RavenMaterialPropertyBase<T>::IsCustomValid() const {
            return this->m_TargetComponent->template IsDerivedFrom<Renderer>() || this->m_TargetComponent->template IsDerivedFrom<UiRenderer>();
        }

        template <class T> Array<Ref<Material>> RavenMaterialPropertyBase<T>::GetAllMaterials(Object* targetComponent) const {
            if (m_IsRenderer) {
                return {reinterpret_cast<Renderer*>(targetComponent)->GetMaterial()};
            } else {
                return {reinterpret_cast<UiRenderer*>(targetComponent)->GetMaterial()};
            }
        }

        template <class T> Ref<Material>& RavenMaterialPropertyBase<T>::GetMaterial(Object* targetComponent) const {
            auto& material = m_IsRenderer ? reinterpret_cast<Renderer*>(targetComponent)->GetMaterial() : reinterpret_cast<UiRenderer*>(targetComponent)->GetMaterial();
            DebugAssert(material, "Material is null on %s, sequence %s", targetComponent->ToString().GetCString(), this->ToString().GetCString());
            return material;
        }

        template <class T> void RavenMaterialPropertyBase<T>::PostEvaluateAtTime(double time, double duration, const T& value, Object* targetComponent) {
            if (m_TriggerPropertyCast) {
                if (!this->m_ExecuteFunctionOnly) {
                    this->SetValue(value, targetComponent);
                }
                m_TriggerPropertyCast->ManualExecute(value, m_TargetMaterialProperty);
            } else {
                this->SetValue(value, targetComponent);
            }
        }
    } // namespace Raven
} // namespace Starlite
#endif
