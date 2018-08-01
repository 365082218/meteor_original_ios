using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataPropertyVector2 {
        private const int c_ValueCount = 2;

        protected override Vector2 ProcessValueComponents(Vector2 value, UnityEngine.Object targetComponent) {
            var synced = false;
            var syncedValue = default(Vector2);

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