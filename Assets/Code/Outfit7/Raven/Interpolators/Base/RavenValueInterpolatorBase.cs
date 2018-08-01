namespace Starlite.Raven {

    public abstract class RavenValueInterpolatorBase<T> {

        /// <summary>
        /// Returns lerp between start and end on t.
        /// </summary>
        public abstract T Interpolate(T start, T end, double t);

        /// <summary>
        /// Adds addValue to value and returns it.
        /// </summary>
        public abstract T Add(T value, T addValue);

        /// <summary>
        /// Multiplies value by multiplyValue.
        /// </summary>
        public abstract T Multiply(T value, T multiplyValue);

        /// <summary>
        /// Multiplies value by a scalar.
        /// </summary>
        public abstract T MultiplyScalar(T value, double scalar);

        /// <summary>
        /// Returns a random value between min (inclusive) and max (exclusive).
        /// </summary>
        public abstract T Random(T min, T max);

        /// <summary>
        /// Returns minimum float value among all the values. Used only in editor.
        /// </summary>
        public abstract double MinValue(T value1, T value2);

        /// <summary>
        /// Returns maximum float value among all the values. Used only in editor.
        /// </summary>
        public abstract double MaxValue(T value1, T value2);
    }
}