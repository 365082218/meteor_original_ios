#ifdef STARLITE
#include "RavenMaterialPropertyVector4.h"
#include "RavenMaterialPropertyVector4.cs"

namespace Starlite {
    namespace Raven {
        RavenMaterialPropertyVector4::RavenMaterialPropertyVector4() {
        }

        Vector4 RavenMaterialPropertyVector4::GetValue(Object* targetComponent) const {
            auto& material = GetMaterial(targetComponent);
            int idx = material->FindPropertyIndex(m_TargetMaterialProperty);
            DebugAssert(IsContainerIndexValid((SSize_T)idx), "Material %s does not have property %s on %s, sequence %s", material->ToString().GetCString(), m_TargetMaterialProperty.GetCString(),
                        targetComponent->ToString().GetCString(), ToString().GetCString());
            return material->GetPropertyVector4(idx);
        }

        void RavenMaterialPropertyVector4::ProcessValueComponents(Vector4& value, Object* targetComponent) {
            bool synced = false;
            Vector4 syncedValue = Vector4::Zero;

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

        void RavenMaterialPropertyVector4::SetValue(const Vector4& value, Object* targetComponent) {
            auto& material = GetMaterial(targetComponent);
            int idx = material->FindPropertyIndex(m_TargetMaterialProperty);
            DebugAssert(IsContainerIndexValid((SSize_T)idx), "Material %s does not have property %s on %s, sequence %s", material->ToString().GetCString(), m_TargetMaterialProperty.GetCString(),
                        targetComponent->ToString().GetCString(), ToString().GetCString());
            material->SetProperty(idx, value);
        }
    } // namespace Raven
} // namespace Starlite
#endif