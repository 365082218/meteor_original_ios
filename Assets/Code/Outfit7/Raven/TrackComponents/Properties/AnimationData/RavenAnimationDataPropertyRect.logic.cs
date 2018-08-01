using Starlite.Raven.Internal;
using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataPropertyRect {
        private const int c_ValueCount = 4;

        protected override Rect ProcessValueComponents(Rect value, UnityEngine.Object targetComponent) {
            var synced = false;
            var syncedValue = default(Rect);

            for (int i = 0; i < c_ValueCount; ++i) {
                // a choice between call + get vs if, I choose if
                if (!m_ApplyValues[i]) {
                    if (!synced) {
                        synced = true;
                        syncedValue = GetValue(targetComponent);
                    }

                    switch (i) {
                        case 0:
                            value.xMin = syncedValue.xMin;
                            break;
                        case 1:
                            value.yMin = syncedValue.yMin;
                            break;
                        case 2:
                            value.width = syncedValue.width;
                            break;
                        case 3:
                            value.height = syncedValue.height;
                            break;
                        default:
                            RavenLog.ErrorT(RavenSequence.Tag, "Unhandled component index {0} in Rect!", i);
                            break;
                    }
                }
            }

            return value;
        }
    }
}