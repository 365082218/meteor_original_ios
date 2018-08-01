#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataSet.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataSetSprite : public RavenAnimationDataSet<Ref<Sprite>> {
            SCLASS_SEALED(RavenAnimationDataSetSprite);

        public:
            RavenAnimationDataSetSprite() = default;

        protected:
            Ref<Sprite> GetValueFromParameterCallback(const RavenParameter* parameter) const override {
                return const_cast<Sprite*>(reinterpret_cast<const Sprite*>(parameter->m_ValueObject.GetObject()));
            }
        };
    } // namespace Raven
} // namespace Starlite
#endif
