using UnityEngine;

namespace Starlite.Raven {

    public sealed class RavenValueInterpolatorSprite : RavenValueInterpolatorBase<Sprite> {
        private static RavenValueInterpolatorSprite s_Default;

        public static RavenValueInterpolatorSprite Default {
            get {
                return s_Default ?? (s_Default = new RavenValueInterpolatorSprite());
            }
        }

        public override Sprite Add(Sprite value, Sprite addValue) {
            return value;
        }

        public override Sprite Interpolate(Sprite start, Sprite end, double t) {
            return start;
        }

        public override double MaxValue(Sprite value1, Sprite value2) {
            return 1;
        }

        public override double MinValue(Sprite value1, Sprite value2) {
            return 0;
        }

        public override Sprite Multiply(Sprite value, Sprite multiplyValue) {
            return value;
        }

        public override Sprite MultiplyScalar(Sprite value, double scalar) {
            return value;
        }

        public override Sprite Random(Sprite min, Sprite max) {
            return StaticRandom.NextBool() ? min : max;
        }
    }
}