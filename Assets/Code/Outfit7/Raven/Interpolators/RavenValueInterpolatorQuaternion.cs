using UnityEngine;

namespace Starlite.Raven {

    public sealed class RavenValueInterpolatorQuaternion : RavenValueInterpolatorBase<Quaternion> {
        private const int c_ValueCount = 4;

        private static RavenValueInterpolatorQuaternion s_Default;

        public static RavenValueInterpolatorQuaternion Default {
            get {
                return s_Default ?? (s_Default = new RavenValueInterpolatorQuaternion());
            }
        }

        public override Quaternion Interpolate(Quaternion start, Quaternion end, double t) {
            return Quaternion.SlerpUnclamped(start, end, (float)t);
        }

        public override Quaternion Add(Quaternion value, Quaternion addValue) {
            return value * addValue;
        }

        public override Quaternion Multiply(Quaternion value, Quaternion multiplyValue) {
            // effort hax
            return Quaternion.Euler(RavenValueInterpolatorVector3.Default.Multiply(value.eulerAngles, multiplyValue.eulerAngles));
        }

        public override Quaternion MultiplyScalar(Quaternion value, double scalar) {
            return Quaternion.Euler(value.eulerAngles * (float)scalar);
        }

        public override Quaternion Random(Quaternion min, Quaternion max) {
            // effort hax
            return Quaternion.Euler(RavenValueInterpolatorVector3.Default.Random(min.eulerAngles, max.eulerAngles));
        }

        public override double MinValue(Quaternion value1, Quaternion value2) {
            double minValue = double.MaxValue;
            for (int i = 0; i < c_ValueCount; ++i) {
                if (value1[i] < minValue) {
                    minValue = value1[i];
                }
                if (value2[i] < minValue) {
                    minValue = value2[i];
                }
            }

            return minValue;
        }

        public override double MaxValue(Quaternion value1, Quaternion value2) {
            double maxValue = double.MinValue;
            for (int i = 0; i < c_ValueCount; ++i) {
                if (value1[i] > maxValue) {
                    maxValue = value1[i];
                }
                if (value2[i] > maxValue) {
                    maxValue = value2[i];
                }
            }

            return maxValue;
        }
    }
}