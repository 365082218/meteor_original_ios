#ifdef STARLITE
#include "RavenAnimationDataComponentBase.h"
#include "RavenAnimationDataComponentBase.cs"

#include <RavenSequence.h>

namespace Starlite {
    namespace Raven {
        RavenAnimationDataComponentBase::RavenAnimationDataComponentBase() {
        }

        void RavenAnimationDataComponentBase::DestroyEditor(Ptr<RavenSequence> sequence) {
            if (sequence->CanDestroyAnimationData(this)) {
                // TODO: destroy it
            }
        }

        void RavenAnimationDataComponentBase::CopyValuesFrom(const RavenAnimationDataComponentBase* other) {
            CopyValuesCallback(other);
        }
    } // namespace Raven
} // namespace Starlite
#endif