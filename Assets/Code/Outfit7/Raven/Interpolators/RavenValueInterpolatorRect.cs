using System;
using UnityEngine;

namespace Starlite.Raven {

    public sealed class RavenValueInterpolatorRect : RavenValueInterpolatorBase<Rect> {
        private static RavenValueInterpolatorRect s_Default;

        public static RavenValueInterpolatorRect Default {
            get {
                return s_Default ?? (s_Default = new RavenValueInterpolatorRect());
            }
        }
        
        public override Rect Interpolate(Rect start, Rect end, double t) {
            var posInterpolate = start.position + (end.position - start.position) * (float)t;
            var sizeInterpolate = start.size + (end.size - start.size) * (float)t;
            return new Rect(posInterpolate, sizeInterpolate);
        }

        public override Rect Add(Rect value, Rect addValue) {
            return new Rect(value.position + addValue.position, value.size + addValue.size);
        }

        public override Rect Multiply(Rect value, Rect multiplyValue) {
            return new Rect(Vector2.Scale(value.position, multiplyValue.position), Vector2.Scale(value.size, multiplyValue.size));
        }

        public override Rect MultiplyScalar(Rect value, double scalar) {
            return new Rect(value.position * (float)scalar, value.size * (float)scalar);
        }

        public override Rect Random(Rect min, Rect max) {
            return new Rect(StaticRandom.NextFloat(min.xMin, max.xMin),
                StaticRandom.NextFloat(min.yMin, max.yMin),
                StaticRandom.NextFloat(min.width, max.width),
                StaticRandom.NextFloat(min.height, max.height));
        }

        public override double MinValue(Rect value1, Rect value2) {
            var minPositionValue = RavenValueInterpolatorVector2.Default.MinValue(value1.position, value2.position);
            var minSizeValue = RavenValueInterpolatorVector2.Default.MinValue(value1.size, value2.size);

            return Math.Min(minPositionValue, minSizeValue);
        }

        public override double MaxValue(Rect value1, Rect value2) {
            var maxPositionValue = RavenValueInterpolatorVector2.Default.MaxValue(value1.position, value2.position);
            var maxSizeValue = RavenValueInterpolatorVector2.Default.MaxValue(value1.size, value2.size);

            return Math.Max(maxPositionValue, maxSizeValue);
        }
    }
}