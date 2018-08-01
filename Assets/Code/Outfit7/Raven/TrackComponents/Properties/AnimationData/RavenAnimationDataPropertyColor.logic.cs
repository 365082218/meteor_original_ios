using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataPropertyColor {
        private const int c_ValueCount = 4;

        protected override Color ProcessValueComponents(Color value, UnityEngine.Object targetComponent) {
            var synced = false;
            var syncedValue = default(Color);

            for (int i = 0; i < c_ValueCount; ++i) {
                // a choice between call + get vs if, I choose if
                if (!m_ApplyValues[i]) {
                    if (!synced) {
                        synced = true;
                        syncedValue = GetValue(targetComponent);
                    }

                    value[i] = syncedValue[i];
                }
            }

            return value;
        }
    }
}