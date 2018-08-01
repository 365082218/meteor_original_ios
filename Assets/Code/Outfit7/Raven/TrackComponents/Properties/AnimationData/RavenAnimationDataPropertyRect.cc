#ifdef STARLITE
#include "RavenAnimationDataPropertyRect.h"
#include "RavenAnimationDataPropertyRect.cs"

#include <RavenSequence.h>

namespace Starlite {
    namespace Raven {
        RavenAnimationDataPropertyRect::RavenAnimationDataPropertyRect() {
        }

        void RavenAnimationDataPropertyRect::ProcessValueComponents(Rectangle& value, Object* targetComponent) {
            bool synced = false;
            Rectangle syncedValue = Rectangle();

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
    } // namespace Raven
} // namespace Starlite
#endif