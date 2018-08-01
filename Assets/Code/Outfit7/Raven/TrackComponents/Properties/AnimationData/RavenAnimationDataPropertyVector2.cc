#ifdef STARLITE
#include "RavenAnimationDataPropertyVector2.h"
#include "RavenAnimationDataPropertyVector2.cs"

namespace Starlite {
    namespace Raven {
        RavenAnimationDataPropertyVector2::RavenAnimationDataPropertyVector2() {
        }

        void RavenAnimationDataPropertyVector2::ProcessValueComponents(Vector2& value, Object* targetComponent) {
            bool synced = false;
            Vector2 syncedValue = Vector2::Zero;

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