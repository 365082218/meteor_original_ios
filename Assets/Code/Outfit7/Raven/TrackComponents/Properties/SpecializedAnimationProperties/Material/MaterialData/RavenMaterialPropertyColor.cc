#ifdef STARLITE
#include "RavenMaterialPropertyColor.h"
#include "RavenMaterialPropertyColor.cs"

namespace Starlite {
    namespace Raven {
        RavenMaterialPropertyColor::RavenMaterialPropertyColor() {
        }

        Color RavenMaterialPropertyColor::GetValue(Object* targetComponent) const {
            auto& material = GetMaterial(targetComponent);
            int idx = material->FindPropertyIndex(m_TargetMaterialProperty);
            DebugAssert(IsContainerIndexValid((SSize_T)idx), "Material %s does not have property %s on %s, sequence %s", material->ToString().GetCString(), m_TargetMaterialProperty.GetCString(),
                        targetComponent->ToString().GetCString(), ToString().GetCString());
            return (Color)material->GetPropertyVector4(idx);
        }

        void RavenMaterialPropertyColor::ProcessValueComponents(Color& value, Object* targetComponent) {
            bool synced = false;
            Color syncedValue = Color::Clear;

            for (int i = 0; i < c_ValueCount; ++i) {
                // a choice between call + get vs if, I choose if
                if (!m_ApplyValues[i]) {
                    if (!synced) {
                        synced = true;
                        syncedValue = GetValue(targetComponent);
                    }

                    value.data[i] = syncedValue.data[i];
                }
            }
        }

        void RavenMaterialPropertyColor::SetValue(const Color& value, Object* targetComponent) {
            auto& material = GetMaterial(targetComponent);
            int idx = material->FindPropertyIndex(m_TargetMaterialProperty);
            DebugAssert(IsContainerIndexValid((SSize_T)idx), "Material %s does not have property %s on %s, sequence %s", material->ToString().GetCString(), m_TargetMaterialProperty.GetCString(),
                        targetComponent->ToString().GetCString(), ToString().GetCString());
            material->SetProperty(idx, (Vector4)value);
        }
    } // namespace Raven
} // namespace Starlite
#endif