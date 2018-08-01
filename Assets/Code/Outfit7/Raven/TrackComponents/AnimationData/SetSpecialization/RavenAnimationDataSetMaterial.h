#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataSet.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataSetMaterial : public RavenAnimationDataSet<Ref<Material>> {
            SCLASS_SEALED(RavenAnimationDataSetMaterial);

        public:
            RavenAnimationDataSetMaterial() = default;

        protected:
            Ref<Material> GetValueFromParameterCallback(const RavenParameter* parameter) const override {
                return const_cast<Material*>(reinterpret_cast<const Material*>(parameter->m_ValueObject.GetObject()));
            }
        };
    } // namespace Raven
} // namespace Starlite
#endif
