using System;

namespace Starlite.Raven {

    public sealed class RavenValueInterpolatorInt : RavenValueInterpolatorBase<int> {
        private static RavenValueInterpolatorInt s_Default;

        public static RavenValueInterpolatorInt Default {
            get {
                return s_Default ?? (s_Default = new RavenValueInterpolatorInt());
            }
        }

        public override int Interpolate(int start, int end, double t) {
            return (int)(start + (end - start) * t);
        }

        public override int Add(int value, int addValue) {
            return value + addValue;
        }

        public override int Multiply(int value, int multiplyValue) {
            return value * multiplyValue;
        }

        public override int MultiplyScalar(int value, double scalar) {
            return (int)(value * scalar);
        }

        public override int Random(int min, int max) {
            return StaticRandom.Next(min, max);
        }

        public override double MinValue(int value1, int value2) {
            return Math.Min(value1, value2);
        }

        public override double MaxValue(int value1, int value2) {
            return Math.Max(value1, value2);
        }
    }
}