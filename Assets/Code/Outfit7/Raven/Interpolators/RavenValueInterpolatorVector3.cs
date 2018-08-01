using UnityEngine;

namespace Starlite.Raven {

    public sealed class RavenValueInterpolatorVector3 : RavenValueInterpolatorBase<Vector3> {
        private const int c_ValueCount = 3;

        private static RavenValueInterpolatorVector3 s_Default;

        public static RavenValueInterpolatorVector3 Default {
            get {
                return s_Default ?? (s_Default = new RavenValueInterpolatorVector3());
            }
        }

        public override Vector3 Interpolate(Vector3 start, Vector3 end, double t) {
            return start + (end - start) * (float)t;
        }

        public override Vector3 Add(Vector3 value, Vector3 addValue) {
            return value + addValue;
        }

        public override Vector3 Multiply(Vector3 value, Vector3 multiplyValue) {
            return Vector3.Scale(value, multiplyValue);
        }

        public override Vector3 MultiplyScalar(Vector3 value, double scalar) {
            return value * (float)scalar;
        }

        public override Vector3 Random(Vector3 min, Vector3 max) {
            var v = new Vector3();
            for (int i = 0; i < c_ValueCount; ++i) {
                v[i] = StaticRandom.NextFloat(min[i], max[i]);
            }
            return v;
        }

        public override double MinValue(Vector3 value1, Vector3 value2) {
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

        public override double MaxValue(Vector3 value1, Vector3 value2) {
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

        public Vector3 BezierCubicCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, double t) {
            return RavenInterpolation.BezierCubicCurve(p0, p1, p2, p3, (float)t);
        }

        public Vector3 BezierCubicTangent(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, double t) {
            return RavenInterpolation.BezierCubicTangent(p0, p1, p2, p3, (float)t);
        }
    }
}