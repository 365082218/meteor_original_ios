using UnityEngine;

namespace Starlite.Raven {

    public sealed class RavenValueInterpolatorColor : RavenValueInterpolatorBase<Color> {
        private const int c_ValueCount = 4;

        private static RavenValueInterpolatorColor s_Default;

        public static RavenValueInterpolatorColor Default {
            get {
                return s_Default ?? (s_Default = new RavenValueInterpolatorColor());
            }
        }

        public override Color Interpolate(Color start, Color end, double t) {
            return start + (end - start) * (float)t;
        }

        public override Color Add(Color value, Color addValue) {
            return value + addValue;
        }

        public override Color Multiply(Color value, Color multiplyValue) {
            return value * multiplyValue;
        }

        public override Color MultiplyScalar(Color value, double scalar) {
            return value * (float)scalar;
        }

        public override Color Random(Color min, Color max) {
            var v = new Color();
            for (int i = 0; i < c_ValueCount; ++i) {
                v[i] = StaticRandom.NextFloat(min[i], max[i]);
            }
            return v;
        }

        public override double MinValue(Color value1, Color value2) {
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

        public override double MaxValue(Color value1, Color value2) {
            float maxValue = float.MinValue;
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