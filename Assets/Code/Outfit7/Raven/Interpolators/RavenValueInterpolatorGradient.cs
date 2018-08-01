using UnityEngine;

namespace Starlite.Raven {

    public sealed class RavenValueInterpolatorGradient : RavenValueInterpolatorBase<Gradient> {
        private const int c_ValueCount = 4;

        private static RavenValueInterpolatorGradient s_Default;

        public static RavenValueInterpolatorGradient Default {
            get {
                return s_Default ?? (s_Default = new RavenValueInterpolatorGradient());
            }
        }

        public override Gradient Interpolate(Gradient start, Gradient end, double t) {
            RavenAssert.IsTrue(start.alphaKeys.Length == end.alphaKeys.Length, "Alpha keys length mismatch!");
            RavenAssert.IsTrue(start.colorKeys.Length == end.colorKeys.Length, "Color keys length mismatch!");
#if UNITY_5_6_OR_NEWER
            RavenAssert.IsTrue(start.mode == end.mode, "Gradient mode mismatch!");
#endif

            Gradient g = new Gradient();
            var alphaKeys = new GradientAlphaKey[start.alphaKeys.Length];
            var colorKeys = new GradientColorKey[start.colorKeys.Length];
            for (int i = 0; i < start.alphaKeys.Length; ++i) {
                var key1 = start.alphaKeys[i];
                var key2 = end.alphaKeys[i];
                var keyValue = RavenValueInterpolatorFloat.Default.Interpolate(key1.alpha, key2.alpha, t);
                alphaKeys[i] = new GradientAlphaKey(keyValue, key1.time);
            }
            for (int i = 0; i < start.colorKeys.Length; ++i) {
                var key1 = start.colorKeys[i];
                var key2 = end.colorKeys[i];
                var keyValue = RavenValueInterpolatorColor.Default.Interpolate(key1.color, key2.color, t);
                colorKeys[i] = new GradientColorKey(keyValue, key1.time);
            }
            g.SetKeys(colorKeys, alphaKeys);
#if UNITY_5_6_OR_NEWER
            g.mode = start.mode;
#endif

            return g;
        }

        public override Gradient Add(Gradient value, Gradient addValue) {
            RavenAssert.IsTrue(value.alphaKeys.Length == addValue.alphaKeys.Length, "Alpha keys length mismatch!");
            RavenAssert.IsTrue(value.colorKeys.Length == addValue.colorKeys.Length, "Color keys length mismatch!");
#if UNITY_5_6_OR_NEWER
            RavenAssert.IsTrue(value.mode == addValue.mode, "Gradient mode mismatch!");
#endif

            Gradient g = new Gradient();
            var alphaKeys = new GradientAlphaKey[value.alphaKeys.Length];
            var colorKeys = new GradientColorKey[value.colorKeys.Length];
            for (int i = 0; i < value.alphaKeys.Length; ++i) {
                var key1 = value.alphaKeys[i];
                var key2 = addValue.alphaKeys[i];
                var keyValue = RavenValueInterpolatorFloat.Default.Add(key1.alpha, key2.alpha);
                alphaKeys[i] = new GradientAlphaKey(keyValue, key1.time);
            }
            for (int i = 0; i < value.colorKeys.Length; ++i) {
                var key1 = value.colorKeys[i];
                var key2 = addValue.colorKeys[i];
                var keyValue = RavenValueInterpolatorColor.Default.Add(key1.color, key2.color);
                colorKeys[i] = new GradientColorKey(keyValue, key1.time);
            }
            g.SetKeys(colorKeys, alphaKeys);
#if UNITY_5_6_OR_NEWER
            g.mode = value.mode;
#endif

            return g;
        }

        public override Gradient Multiply(Gradient value, Gradient multiplyValue) {
            RavenAssert.IsTrue(value.alphaKeys.Length == multiplyValue.alphaKeys.Length, "Alpha keys length mismatch!");
            RavenAssert.IsTrue(value.colorKeys.Length == multiplyValue.colorKeys.Length, "Color keys length mismatch!");
#if UNITY_5_6_OR_NEWER
            RavenAssert.IsTrue(value.mode == multiplyValue.mode, "Gradient mode mismatch!");
#endif

            Gradient g = new Gradient();
            var alphaKeys = new GradientAlphaKey[value.alphaKeys.Length];
            var colorKeys = new GradientColorKey[value.colorKeys.Length];
            for (int i = 0; i < value.alphaKeys.Length; ++i) {
                var key1 = value.alphaKeys[i];
                var key2 = multiplyValue.alphaKeys[i];
                var keyValue = RavenValueInterpolatorFloat.Default.Multiply(key1.alpha, key2.alpha);
                alphaKeys[i] = new GradientAlphaKey(keyValue, key1.time);
            }
            for (int i = 0; i < value.colorKeys.Length; ++i) {
                var key1 = value.colorKeys[i];
                var key2 = multiplyValue.colorKeys[i];
                var keyValue = RavenValueInterpolatorColor.Default.Multiply(key1.color, key2.color);
                colorKeys[i] = new GradientColorKey(keyValue, key1.time);
            }
            g.SetKeys(colorKeys, alphaKeys);
#if UNITY_5_6_OR_NEWER
            g.mode = value.mode;
#endif

            return g;
        }

        public override Gradient MultiplyScalar(Gradient value, double scalar) {
            Gradient g = new Gradient();
            var alphaKeys = new GradientAlphaKey[value.alphaKeys.Length];
            var colorKeys = new GradientColorKey[value.colorKeys.Length];
            for (int i = 0; i < value.alphaKeys.Length; ++i) {
                var key1 = value.alphaKeys[i];
                var keyValue = RavenValueInterpolatorFloat.Default.MultiplyScalar(key1.alpha, scalar);
                alphaKeys[i] = new GradientAlphaKey(keyValue, key1.time);
            }
            for (int i = 0; i < value.colorKeys.Length; ++i) {
                var key1 = value.colorKeys[i];
                var keyValue = RavenValueInterpolatorColor.Default.MultiplyScalar(key1.color, scalar);
                colorKeys[i] = new GradientColorKey(keyValue, key1.time);
            }

            g.SetKeys(colorKeys, alphaKeys);
#if UNITY_5_6_OR_NEWER
            g.mode = value.mode;
#endif

            return g;
        }

        public override Gradient Random(Gradient min, Gradient max) {
            RavenAssert.IsTrue(min.alphaKeys.Length == max.alphaKeys.Length, "Alpha keys length mismatch!");
            RavenAssert.IsTrue(min.colorKeys.Length == max.colorKeys.Length, "Color keys length mismatch!");
#if UNITY_5_6_OR_NEWER
            RavenAssert.IsTrue(min.mode == max.mode, "Gradient mode mismatch!");
#endif

            Gradient g = new Gradient();
            var alphaKeys = new GradientAlphaKey[min.alphaKeys.Length];
            var colorKeys = new GradientColorKey[min.colorKeys.Length];
            for (int i = 0; i < min.alphaKeys.Length; ++i) {
                var key1 = min.alphaKeys[i];
                var key2 = max.alphaKeys[i];
                var keyValue = RavenValueInterpolatorFloat.Default.Random(key1.alpha, key2.alpha);
                alphaKeys[i] = new GradientAlphaKey(keyValue, key1.time);
            }
            for (int i = 0; i < min.colorKeys.Length; ++i) {
                var key1 = min.colorKeys[i];
                var key2 = max.colorKeys[i];
                var keyValue = RavenValueInterpolatorColor.Default.Random(key1.color, key2.color);
                colorKeys[i] = new GradientColorKey(keyValue, key1.time);
            }

            g.SetKeys(colorKeys, alphaKeys);
#if UNITY_5_6_OR_NEWER
            g.mode = min.mode;
#endif
            return g;
        }

        public override double MinValue(Gradient value1, Gradient value2) {
            RavenAssert.IsTrue(value1.colorKeys.Length == value2.colorKeys.Length, "Color keys length mismatch!");

            double minValue = double.MaxValue;
            for (int i = 0; i < value1.colorKeys.Length; ++i) {
                var value = RavenValueInterpolatorColor.Default.MinValue(value1.colorKeys[i].color, value2.colorKeys[i].color);
                if (value < minValue) {
                    minValue = value;
                }
            }

            return minValue;
        }

        public override double MaxValue(Gradient value1, Gradient value2) {
            RavenAssert.IsTrue(value1.colorKeys.Length == value2.colorKeys.Length, "Color keys length mismatch!");

            double maxValue = double.MinValue;
            for (int i = 0; i < value1.colorKeys.Length; ++i) {
                var value = RavenValueInterpolatorColor.Default.MaxValue(value1.colorKeys[i].color, value2.colorKeys[i].color);
                if (value > maxValue) {
                    maxValue = value;
                }
            }

            return maxValue;
        }
    }
}