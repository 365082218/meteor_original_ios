#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataTween.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataTweenDouble : public RavenAnimationDataTween<Double> {
            SCLASS_SEALED(RavenAnimationDataTweenDouble);

        public:
            RavenAnimationDataTweenDouble();

        protected:
            bool GetRemap() const override;
            Double GetValueFromParameterCallback(const RavenParameter* parameter) const override;
            Double PerformRemap(const double& startValue, const double& endValue, double t) override;
            void CopyValuesCallback(const RavenAnimationDataComponentBase* other) override;

        private:
            SPROPERTY(CustomAttributes : ["UnityEngine.HeaderAttribute(\"Remap\")"], Access : "private");
            bool m_Remap = false;

            SPROPERTY(CustomAttributes : ["Raven.VisibleConditionAttribute(\"m_Remap\")"], Access : "private");
            Double m_RemapStart = 0.0;

            SPROPERTY(CustomAttributes : ["Raven.VisibleConditionAttribute(\"m_Remap\")"], Access : "private");
            Double m_RemapEnd = 1.0;
        };
    } // namespace Raven
} // namespace Starlite
#endif