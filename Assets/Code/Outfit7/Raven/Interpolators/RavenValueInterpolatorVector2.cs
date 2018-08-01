using UnityEngine;

namespace Starlite.Raven {

    public sealed class RavenValueInterpolatorVector2 : RavenValueInterpolatorBase<Vector2> {
        private const int c_ValueCount = 2;

        private static RavenValueInterpolatorVector2 s_Default;

        public static RavenValueInterpolatorVector2 Default {
            get {
                return s_Default ?? (s_Default = new RavenValueInterpolatorVector2());
            }
        }

        public override Vector2 Interpolate(Vector2 start, Vector2 end, double t) {
            return start + (end - start) * (float)t;
        }

        public override Vector2 Add(Vector2 value, Vector2 addValue) {
            return value + addValue;
        }

        public override Vector2 Multiply(Vector2 value, Vector2 multiplyValue) {
            return Vector2.Scale(value, multiplyValue);
        }

        public override Vector2 MultiplyScalar(Vector2 value, double scalar) {
            return value * (float)scalar;
        }

        public override Vector2 Random(Vector2 min, Vector2 max) {
            var v = new Vector2();
            for (int i = 0; i < c_ValueCount; ++i) {
                v[i] = StaticRandom.NextFloat(min[i], max[i]);
            }
            return v;
        }

        public override double MinValue(Vector2 value1, Vector2 value2) {
            float minValue = float.MaxValue;
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

        public override double MaxValue(Vector2 value1, Vector2 value2) {
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