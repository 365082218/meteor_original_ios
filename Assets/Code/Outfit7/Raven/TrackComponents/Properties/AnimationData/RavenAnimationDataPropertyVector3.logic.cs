using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataPropertyVector3 {
        private const int c_ValueCount = 3;

        protected override Vector3 ProcessValueComponents(Vector3 value, UnityEngine.Object targetComponent) {
            var synced = false;
            var syncedValue = default(Vector3);

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