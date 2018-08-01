using UnityEngine;

namespace Starlite.Raven {

    public sealed class RavenValueInterpolatorMaterial : RavenValueInterpolatorBase<Material> {
        private static RavenValueInterpolatorMaterial s_Default;

        public static RavenValueInterpolatorMaterial Default {
            get {
                return s_Default ?? (s_Default = new RavenValueInterpolatorMaterial());
            }
        }

        public override Material Add(Material value, Material addValue) {
            return value;
        }

        public override Material Interpolate(Material start, Material end, double t) {
            return start;
        }

        public override double MaxValue(Material value1, Material value2) {
            return 1;
        }

        public override double MinValue(Material value1, Material value2) {
            return 0;
        }

        public override Material Multiply(Material value, Material multiplyValue) {
            return value;
        }

        public override Material MultiplyScalar(Material value, double scalar) {
            return value;
        }

        public override Material Random(Material min, Material max) {
            return StaticRandom.NextBool() ? min : max;
        }
    }
}