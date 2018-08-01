#ifdef STARLITE
#include "RavenAnimationDataPropertyVector4.h"
#include "RavenAnimationDataPropertyVector4.cs"

namespace Starlite {
    namespace Raven {
        RavenAnimationDataPropertyVector4::RavenAnimationDataPropertyVector4() {
        }

        void RavenAnimationDataPropertyVector4::ProcessValueComponents(Vector4& value, Object* targetComponent) {
            bool synced = false;
            Vector4 syncedValue = Vector4::Zero;

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