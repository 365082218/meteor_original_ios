#if UNITY_5_6_OR_NEWER

using UnityEngine;

namespace Starlite.Raven {

    public sealed class RavenValueInterpolatorMinMaxGradient : RavenValueInterpolatorBase<ParticleSystem.MinMaxGradient> {
        private const int c_ValueCount = 4;

        private static RavenValueInterpolatorMinMaxGradient s_Default;

        public static RavenValueInterpolatorMinMaxGradient Default {
            get {
                return s_Default ?? (s_Default = new RavenValueInterpolatorMinMaxGradient());
            }
        }

        public override ParticleSystem.MinMaxGradient Interpolate(ParticleSystem.MinMaxGradient start, ParticleSystem.MinMaxGradient end, double t) {
            RavenAssert.IsTrue(start.mode == end.mode, "MinMaxGradient modes don't match!");

            ParticleSystem.MinMaxGradient g = new ParticleSystem.MinMaxGradient();
            g.mode = start.mode;
            switch (start.mode) {
                case ParticleSystemGradientMode.Color:
                    g.color = RavenValueInterpolatorColor.Default.Interpolate(start.color, end.color, t); ;
                    break;
                case ParticleSystemGradientMode.Gradient:
                    g.gradient = RavenValueInterpolatorGradient.Default.Interpolate(start.gradient, end.gradient, t);
                    break;
                case ParticleSystemGradientMode.TwoColors:
                    g.colorMin = RavenValueInterpolatorColor.Default.Interpolate(start.colorMin, end.colorMin, t);
                    g.colorMax = RavenValueInterpolatorColor.Default.Interpolate(start.colorMax, end.colorMax, t);
                    break;
                case ParticleSystemGradientMode.TwoGradients:
                    g.gradientMin = RavenValueInterpolatorGradient.Default.Interpolate(start.gradientMin, end.gradientMin, t);
                    g.gradientMax = RavenValueInterpolatorGradient.Default.Interpolate(start.gradientMax, end.gradientMax, t);
                    break;
                default:
                    RavenAssert.IsTrue(false, "MinMaxGradient mode {0} not implemented!", start.mode);
                    break;
            }
            return g;
        }

        public override ParticleSystem.MinMaxGradient Add(ParticleSystem.MinMaxGradient value, ParticleSystem.MinMaxGradient addValue) {
            RavenAssert.IsTrue(value.mode == addValue.mode, "MinMaxGradient modes don't match!");

            ParticleSystem.MinMaxGradient g = new ParticleSystem.MinMaxGradient();
            g.mode = value.mode;
            switch (value.mode) {
                case ParticleSystemGradientMode.Color:
                    g.color = RavenValueInterpolatorColor.Default.Add(value.color, addValue.color);
                    break;
                case ParticleSystemGradientMode.Gradient:
                    g.gradient = RavenValueInterpolatorGradient.Default.Add(value.gradient, addValue.gradient);
                    break;
                case ParticleSystemGradientMode.TwoColors:
                    g.colorMin = RavenValueInterpolatorColor.Default.Add(value.colorMin, addValue.colorMin);
                    g.colorMax = RavenValueInterpolatorColor.Default.Add(value.colorMax, addValue.colorMax);
                    break;
                case ParticleSystemGradientMode.TwoGradients:
                    g.gradientMin = RavenValueInterpolatorGradient.Default.Add(value.gradientMin, addValue.gradientMin);
                    g.gradientMax = RavenValueInterpolatorGradient.Default.Add(value.gradientMax, addValue.gradientMax);
                    break;
                default:
                    RavenAssert.IsTrue(false, "MinMaxGradient mode {0} not implemented!", value.mode);
                    break;
            }
            return g;
        }

        public override ParticleSystem.MinMaxGradient Multiply(ParticleSystem.MinMaxGradient value, ParticleSystem.MinMaxGradient multiplyValue) {
            RavenAssert.IsTrue(value.mode == multiplyValue.mode, "MinMaxGradient modes don't match!");

            ParticleSystem.MinMaxGradient g = new ParticleSystem.MinMaxGradient();
            g.mode = value.mode;
            switch (value.mode) {
                case ParticleSystemGradientMode.Color:
                    g.color = RavenValueInterpolatorColor.Default.Multiply(value.color, multiplyValue.color);
                    break;
                case ParticleSystemGradientMode.Gradient:
                    g.gradient = RavenValueInterpolatorGradient.Default.Multiply(value.gradient, multiplyValue.gradient);
                    break;
                case ParticleSystemGradientMode.TwoColors:
                    g.colorMin = RavenValueInterpolatorColor.Default.Multiply(value.colorMin, multiplyValue.colorMin);
                    g.colorMax = RavenValueInterpolatorColor.Default.Multiply(value.colorMax, multiplyValue.colorMax);
                    break;
                case ParticleSystemGradientMode.TwoGradients:
                    g.gradientMin = RavenValueInterpolatorGradient.Default.Multiply(value.gradientMin, multiplyValue.gradientMin);
                    g.gradientMax = RavenValueInterpolatorGradient.Default.Multiply(value.gradientMax, multiplyValue.gradientMax);
                    break;
                default:
                    RavenAssert.IsTrue(false, "MinMaxGradient mode {0} not implemented!", value.mode);
                    break;
            }
            return g;
        }

        public override ParticleSystem.MinMaxGradient MultiplyScalar(ParticleSystem.MinMaxGradient value, double scalar) {
            ParticleSystem.MinMaxGradient g = new ParticleSystem.MinMaxGradient();
            g.mode = value.mode;
            switch (value.mode) {
                case ParticleSystemGradientMode.Color:
                    g.color = RavenValueInterpolatorColor.Default.MultiplyScalar(value.color, scalar);
                    break;
                case ParticleSystemGradientMode.Gradient:
                    g.gradient = RavenValueInterpolatorGradient.Default.MultiplyScalar(value.gradient, scalar);
                    break;
                case ParticleSystemGradientMode.TwoColors:
                    g.colorMin = RavenValueInterpolatorColor.Default.MultiplyScalar(value.colorMin, scalar);
                    g.colorMax = RavenValueInterpolatorColor.Default.MultiplyScalar(value.colorMax, scalar);
                    break;
                case ParticleSystemGradientMode.TwoGradients:
                    g.gradientMin = RavenValueInterpolatorGradient.Default.MultiplyScalar(value.gradientMin, scalar);
                    g.gradientMax = RavenValueInterpolatorGradient.Default.MultiplyScalar(value.gradientMax, scalar);
                    break;
                default:
                    RavenAssert.IsTrue(false, "MinMaxGradient mode {0} not implemented!", value.mode);
                    break;
            }
            return g;
        }

        public override ParticleSystem.MinMaxGradient Random(ParticleSystem.MinMaxGradient min, ParticleSystem.MinMaxGradient max) {
            RavenAssert.IsTrue(min.mode == max.mode, "MinMaxGradient modes don't match!");

            ParticleSystem.MinMaxGradient g = new ParticleSystem.MinMaxGradient();
            g.mode = min.mode;
            switch (min.mode) {
                case ParticleSystemGradientMode.Color:
                    g.color = RavenValueInterpolatorColor.Default.Random(min.color, max.color);
                    break;
                case ParticleSystemGradientMode.Gradient:
                    g.gradient = RavenValueInterpolatorGradient.Default.Random(min.gradient, max.gradient);
                    break;
                case ParticleSystemGradientMode.TwoColors:
                    g.colorMin = RavenValueInterpolatorColor.Default.Random(min.colorMin, max.colorMin);
                    g.colorMax = RavenValueInterpolatorColor.Default.Random(min.colorMax, max.colorMax);
                    break;
                case ParticleSystemGradientMode.TwoGradients:
                    g.gradientMin = RavenValueInterpolatorGradient.Default.Random(min.gradientMin, max.gradientMin);
                    g.gradientMax = RavenValueInterpolatorGradient.Default.Random(min.gradientMax, max.gradientMax);
                    break;
                default:
                    RavenAssert.IsTrue(false, "MinMaxGradient mode {0} not implemented!", min.mode);
                    break;
            }
            return g;
        }

        public override double MinValue(ParticleSystem.MinMaxGradient value1, ParticleSystem.MinMaxGradient value2) {
            double minValue = double.MaxValue;

            ParticleSystem.MinMaxGradient g = new ParticleSystem.MinMaxGradient();
            g.mode = value1.mode;
            double mv2;
            switch (value1.mode) {
                case ParticleSystemGradientMode.Color:
                    minValue = RavenValueInterpolatorColor.Default.MinValue(value1.color, value2.color);
                    break;
                case ParticleSystemGradientMode.Gradient:
                    minValue = RavenValueInterpolatorGradient.Default.MinValue(value1.gradient, value2.gradientMin);
                    break;
                case ParticleSystemGradientMode.TwoColors:
                    minValue = RavenValueInterpolatorColor.Default.MinValue(value1.colorMin, value2.colorMin);
                    mv2 = RavenValueInterpolatorColor.Default.MinValue(value1.colorMax, value2.colorMax);
                    if (mv2 < minValue) {
                        minValue = mv2;
                    }
                    break;
                case ParticleSystemGradientMode.TwoGradients:
                    minValue = RavenValueInterpolatorGradient.Default.MinValue(value1.gradientMin, value2.gradientMin);
                    mv2 = RavenValueInterpolatorGradient.Default.MinValue(value1.gradientMax, value2.gradientMax);
                    if (mv2 < minValue) {
                        minValue = mv2;
                    }
                    break;
                default:
                    RavenAssert.IsTrue(false, "MinMaxGradient mode {0} not implemented!", value1.mode);
                    break;
            }
            return minValue;
        }

        public override double MaxValue(ParticleSystem.MinMaxGradient value1, ParticleSystem.MinMaxGradient value2) {
            double maxValue = double.MinValue;

            ParticleSystem.MinMaxGradient g = new ParticleSystem.MinMaxGradient();
            g.mode = value1.mode;
            double mv2;
            switch (value1.mode) {
                case ParticleSystemGradientMode.Color:
                    maxValue = RavenValueInterpolatorColor.Default.MaxValue(value1.color, value2.color);
                    break;
                case ParticleSystemGradientMode.Gradient:
                    maxValue = RavenValueInterpolatorGradient.Default.MaxValue(value1.gradient, value2.gradientMin);
                    break;
                case ParticleSystemGradientMode.TwoColors:
                    maxValue = RavenValueInterpolatorColor.Default.MaxValue(value1.colorMin, value2.colorMin);
                    mv2 = RavenValueInterpolatorColor.Default.MaxValue(value1.colorMax, value2.colorMax);
                    if (mv2 > maxValue) {
                        maxValue = mv2;
                    }
                    break;
                case ParticleSystemGradientMode.TwoGradients:
                    maxValue = RavenValueInterpolatorGradient.Default.MaxValue(value1.gradientMin, value2.gradientMin);
                    mv2 = RavenValueInterpolatorGradient.Default.MaxValue(value1.gradientMax, value2.gradientMax);
                    if (mv2 > maxValue) {
                        maxValue = mv2;
                    }
                    break;
                default:
                    RavenAssert.IsTrue(false, "MinMaxGradient mode {0} not implemented!", value1.mode);
                    break;
            }
            return maxValue;
        }
    }
}

#endif