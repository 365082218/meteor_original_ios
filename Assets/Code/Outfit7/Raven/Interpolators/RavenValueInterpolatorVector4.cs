using UnityEngine;

namespace Starlite.Raven {

    public sealed class RavenValueInterpolatorVector4 : RavenValueInterpolatorBase<Vector4> {
        private const int c_ValueCount = 4;

        private static RavenValueInterpolatorVector4 s_Default;

        public static RavenValueInterpolatorVector4 Default {
            get {
                return s_Default ?? (s_Default = new RavenValueInterpolatorVector4());
            }
        }

        public override Vector4 Interpolate(Vector4 start, Vector4 end, double t) {
            return start + (end - start) * (float)t;
        }

        public override Vector4 Add(Vector4 value, Vector4 addValue) {
            return value + addValue;
        }

        public override Vector4 Multiply(Vector4 value, Vector4 multiplyValue) {
            return Vector4.Scale(value, multiplyValue);
        }

        public override Vector4 MultiplyScalar(Vector4 value, double scalar) {
            return value * (float)scalar;
        }

        public override Vector4 Random(Vector4 min, Vector4 max) {
            var v = new Vector4();
            for (int i = 0; i < c_ValueCount; ++i) {
                v[i] = StaticRandom.NextFloat(min[i], max[i]);
            }
            return v;
        }

        public override double MinValue(Vector4 value1, Vector4 value2) {
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

        public override double MaxValue(Vector4 value1, Vector4 value2) {
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