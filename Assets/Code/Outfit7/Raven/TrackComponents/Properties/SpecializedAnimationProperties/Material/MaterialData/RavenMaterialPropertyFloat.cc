#ifdef STARLITE
#include "RavenMaterialPropertyFloat.h"
#include "RavenMaterialPropertyFloat.cs"

namespace Starlite {
    namespace Raven {
        RavenMaterialPropertyFloat::RavenMaterialPropertyFloat() {
        }

        float RavenMaterialPropertyFloat::GetValue(Object* targetComponent) const {
            auto& material = GetMaterial(targetComponent);
            int idx = material->FindPropertyIndex(m_TargetMaterialProperty);
            DebugAssert(IsContainerIndexValid((SSize_T)idx), "Material %s does not have property %s on %s, sequence %s", material->ToString().GetCString(), m_TargetMaterialProperty.GetCString(),
                        targetComponent->ToString().GetCString(), ToString().GetCString());
            return material->GetPropertyVector4(idx).x;
        }

        void RavenMaterialPropertyFloat::SetValue(const float& value, Object* targetComponent) {
            auto& material = GetMaterial(targetComponent);
            int idx = material->FindPropertyIndex(m_TargetMaterialProperty);
            DebugAssert(IsContainerIndexValid((SSize_T)idx), "Material %s does not have property %s on %s, sequence %s", material->ToString().GetCString(), m_TargetMaterialProperty.GetCString(),
                        targetComponent->ToString().GetCString(), ToString().GetCString());
            material->SetProperty(idx, (Vector4)value);
        }
    } // namespace Raven
} // namespace Starlite
#endif