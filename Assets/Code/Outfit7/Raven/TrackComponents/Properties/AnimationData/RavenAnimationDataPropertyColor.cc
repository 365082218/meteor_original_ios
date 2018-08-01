#ifdef STARLITE
#include "RavenAnimationDataPropertyColor.h"
#include "RavenAnimationDataPropertyColor.cs"

namespace Starlite {
    namespace Raven {
        RavenAnimationDataPropertyColor::RavenAnimationDataPropertyColor() {
        }

        void RavenAnimationDataPropertyColor::ProcessValueComponents(Color& value, Object* targetComponent) {
            bool synced = false;
            Color syncedValue = Color::Clear;

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