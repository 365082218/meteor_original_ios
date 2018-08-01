#ifdef STARLITE
#pragma once

#include <TrackComponents/Properties/SpecializedAnimationProperties/Material/RavenMaterialPropertyBase.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenMaterialPropertyColor : public RavenMaterialPropertyBase<Color> {
            SCLASS_SEALED(RavenMaterialPropertyColor);

        public:
            RavenMaterialPropertyColor();
            Color GetValue(Object* targetComponent) const override;

        protected:
            void ProcessValueComponents(Color& value, Object* targetComponent) override;
            void SetValue(const Color& value, Object* targetComponent) override;

        private:
            SPROPERTY(Access : "private");
            Array<bool> m_ApplyValues;

            static const int c_ValueCount = 4;
        };
    } // namespace Raven
} // namespace Starlite
#endif
