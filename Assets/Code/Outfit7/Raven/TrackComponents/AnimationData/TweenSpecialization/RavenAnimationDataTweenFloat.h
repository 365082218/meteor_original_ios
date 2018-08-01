#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataTween.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataTweenFloat : public RavenAnimationDataTween<float> {
            SCLASS_SEALED(RavenAnimationDataTweenFloat);

        public:
            RavenAnimationDataTweenFloat();

        protected:
            bool GetRemap() const override;
            float GetValueFromParameterCallback(const RavenParameter* parameter) const override;
            float PerformRemap(const float& startValue, const float& endValue, double t) override;
            void CopyValuesCallback(const RavenAnimationDataComponentBase* other) override;

        private:
            SPROPERTY(CustomAttributes : ["UnityEngine.HeaderAttribute(\"Remap\")"], Access : "private");
            bool m_Remap = false;

            SPROPERTY(CustomAttributes : ["Raven.VisibleConditionAttribute(\"m_Remap\")"], Access : "private");
            float m_RemapStart = 0.f;

            SPROPERTY(CustomAttributes : ["Raven.VisibleConditionAttribute(\"m_Remap\")"], Access : "private");
            float m_RemapEnd = 1.f;
        };
    } // namespace Raven
} // namespace Starlite
#endif