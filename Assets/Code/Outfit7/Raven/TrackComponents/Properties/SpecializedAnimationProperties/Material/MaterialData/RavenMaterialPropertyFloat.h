#ifdef STARLITE
#pragma once

#include <TrackComponents/Properties/SpecializedAnimationProperties/Material/RavenMaterialPropertyBase.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenMaterialPropertyFloat : public RavenMaterialPropertyBase<float> {
            SCLASS_SEALED(RavenMaterialPropertyFloat);

        public:
            RavenMaterialPropertyFloat();
            float GetValue(Object* targetComponent) const override;

        protected:
            void SetValue(const float& value, Object* targetComponent) override;
        };
    } // namespace Raven
} // namespace Starlite
#endif
