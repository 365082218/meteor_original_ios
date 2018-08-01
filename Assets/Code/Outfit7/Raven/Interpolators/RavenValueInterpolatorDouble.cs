using System;

namespace Starlite.Raven {

    public sealed class RavenValueInterpolatorDouble : RavenValueInterpolatorBase<double> {
        private static RavenValueInterpolatorDouble s_Default;

        public static RavenValueInterpolatorDouble Default {
            get {
                return s_Default ?? (s_Default = new RavenValueInterpolatorDouble());
            }
        }

        public override double Interpolate(double start, double end, double t) {
            return start + (end - start) * t;
        }

        public override double Add(double value, double addValue) {
            return value + addValue;
        }

        public override double Multiply(double value, double multiplyValue) {
            return value * multiplyValue;
        }

        public override double MultiplyScalar(double value, double scalar) {
            return value * scalar;
        }

        public override double Random(double min, double max) {
            return StaticRandom.NextDouble(min, max);
        }

        public override double MinValue(double value1, double value2) {
            return Math.Min(value1, value2);
        }

        public override double MaxValue(double value1, double value2) {
            return Math.Max(value1, value2);
        }
    }
}