using System;

namespace Starlite.Raven {

    public sealed class RavenValueInterpolatorFloat : RavenValueInterpolatorBase<float> {
        private static RavenValueInterpolatorFloat s_Default;

        public static RavenValueInterpolatorFloat Default {
            get {
                return s_Default ?? (s_Default = new RavenValueInterpolatorFloat());
            }
        }

        public override float Interpolate(float start, float end, double t) {
            return (float)(start + (end - start) * t);
        }

        public override float Add(float value, float addValue) {
            return value + addValue;
        }

        public override float Multiply(float value, float multiplyValue) {
            return value * multiplyValue;
        }

        public override float MultiplyScalar(float value, double scalar) {
            return (float)(value * scalar);
        }

        public override float Random(float min, float max) {
            return StaticRandom.NextFloat(min, max);
        }

        public override double MinValue(float value1, float value2) {
            return Math.Min(value1, value2);
        }

        public override double MaxValue(float value1, float value2) {
            return Math.Max(value1, value2);
        }
    }
}