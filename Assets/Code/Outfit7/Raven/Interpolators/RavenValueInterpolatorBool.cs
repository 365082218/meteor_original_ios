namespace Starlite.Raven {

    public sealed class RavenValueInterpolatorBool : RavenValueInterpolatorBase<bool> {
        private static RavenValueInterpolatorBool s_Default;

        public static RavenValueInterpolatorBool Default {
            get {
                return s_Default ?? (s_Default = new RavenValueInterpolatorBool());
            }
        }

        public override bool Interpolate(bool start, bool end, double t) {
            return t < 0.5 ? start : end;
        }

        public override bool Add(bool value, bool addValue) {
            return value | addValue;
        }

        public override bool Multiply(bool value, bool multiplyValue) {
            return value & multiplyValue;
        }

        public override bool MultiplyScalar(bool value, double scalar) {
            return value;
        }

        public override double MaxValue(bool value1, bool value2) {
            return value1 == true || value2 == true ? 1 : 0;
        }

        public override double MinValue(bool value1, bool value2) {
            return value1 == false || value2 == false ? 0 : 1;
        }

        public override bool Random(bool min, bool max) {
            return StaticRandom.NextBool();
        }
    }
}