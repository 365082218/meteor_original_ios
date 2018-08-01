#ifdef STARLITE
#pragma once

#include <TrackComponents/Properties/SpecializedAnimationProperties/Material/RavenMaterialPropertyBase.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenMaterialPropertyVector4 : public RavenMaterialPropertyBase<Vector4> {
            SCLASS_SEALED(RavenMaterialPropertyVector4);

        public:
            RavenMaterialPropertyVector4();
            Vector4 GetValue(Object* targetComponent) const override;

        protected:
            void ProcessValueComponents(Vector4& value, Object* targetComponent) override;
            void SetValue(const Vector4& value, Object* targetComponent) override;

        private:
            SPROPERTY(Access : "private");
            Array<bool> m_ApplyValues;

            static const int c_ValueCount = 4;
        };
    } // namespace Raven
} // namespace Starlite
#endif
