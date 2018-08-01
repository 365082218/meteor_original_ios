#ifdef STARLITE
#include "RavenAnimationDataPropertyQuaternion.h"
#include "RavenAnimationDataPropertyQuaternion.cs"

namespace Starlite {
    namespace Raven {
        RavenAnimationDataPropertyQuaternion::RavenAnimationDataPropertyQuaternion() {
        }

        void RavenAnimationDataPropertyQuaternion::ProcessValueComponents(Quaternion& value, Object* targetComponent) {
            bool synced = false;
            Quaternion syncedValue = Quaternion::Identity;

            for (int i = 0; i < c_ValueCount; ++i) {
                // a choice between call + get vs if, I choose if
                if (!m_ApplyValues[i]) {
                    if (!synced) {
                        synced = true;
                        syncedValue = GetValue(targetComponent);
                    }

                    value.vector.data[i] = syncedValue.vector.data[i];
                }
            }
        }
    } // namespace Raven
} // namespace Starlite
#endif