#ifdef STARLITE
#include "RavenAnimationDataPropertyVector3.h"
#include "RavenAnimationDataPropertyVector3.cs"

namespace Starlite {
    namespace Raven {
        RavenAnimationDataPropertyVector3::RavenAnimationDataPropertyVector3() {
        }

        void RavenAnimationDataPropertyVector3::ProcessValueComponents(Vector3& value, Object* targetComponent) {
            bool synced = false;
            Vector3 syncedValue = Vector3::Zero;

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