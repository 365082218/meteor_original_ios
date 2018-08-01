using UnityEngine;

namespace Outfit7.Graphics {
    public static class Interpolation {

#region Color

        public static Color Linear(Color from, Color to, float t) {
            if (t < 0)
                return from;
            if (t > 1)
                return to;
            return Color.Lerp(from, to, t);
        }

        public static Color SmoothStep(Color from, Color to, float t) {
            if (t < 0)
                return from;
            if (t > 1)
                return to;
            return Color.Lerp(from, to, t * t * (3 - 2 * t));
        }

        public static Color Deceleration(Color from, Color to, float t) {
            if (t < 0)
                return from;
            if (t > 1)
                return to;
            float m1 = (1 - t);
            return Color.Lerp(from, to, 1 - m1 * m1);
        }

        public static Color Acceleration(Color from, Color to, float t) {
            if (t < 0)
                return from;
            if (t > 1)
                return to;
            return Color.Lerp(from, to, t * t);
        }

#endregion

#region Vector2

        public static Vector2 Linear(Vector2 from, Vector2 to, float t) {
            if (t < 0)
                return from;
            if (t > 1)
                return to;
            return Vector2.Lerp(from, to, t);
        }

        public static Vector2 SmoothStep(Vector2 from, Vector2 to, float t) {
            if (t < 0)
                return from;
            if (t > 1)
                return to;
            return Vector2.Lerp(from, to, t * t * (3 - 2 * t));
        }

        public static Vector2 Deceleration(Vector2 from, Vector2 to, float t) {
            if (t < 0)
                return from;
            if (t > 1)
                return to;
            float m1 = (1 - t);
            return Vector2.Lerp(from, to, 1 - m1 * m1);
        }

        public static Vector2 Acceleration(Vector2 from, Vector2 to, float t) {
            if (t < 0)
                return from;
            if (t > 1)
                return to;
            return Vector2.Lerp(from, to, t * t);
        }

#endregion

#region Vector3

        public static Vector3 Linear(Vector3 from, Vector3 to, float t) {
            if (t < 0)
                return from;
            if (t > 1)
                return to;
            return Vector3.Lerp(from, to, t);
        }

        public static Vector3 SmoothStep(Vector3 from, Vector3 to, float t) {
            if (t < 0)
                return from;
            if (t > 1)
                return to;
            return Vector3.Lerp(from, to, t * t * (3 - 2 * t));
        }

        public static Vector3 Deceleration(Vector3 from, Vector3 to, float t) {
            if (t < 0)
                return from;
            if (t > 1)
                return to;
            float m1 = (1 - t);
            return Vector3.Lerp(from, to, 1 - m1 * m1);
        }

        public static Vector3 Acceleration(Vector3 from, Vector3 to, float t) {
            if (t < 0)
                return from;
            if (t > 1)
                return to;
            return Vector3.Lerp(from, to, t * t);
        }

        public static Vector3 CatmullRomSpline(Vector3 previous, Vector3 from, Vector3 to, Vector3 next, float t) {
            t = Mathf.Clamp01(t);
            float t2 = t * t;
            float t3 = t2 * t;
            return ((from * 2.0f) +
                (-previous + to) * t +
                ((previous * 2.0f) - (from * 5.0f) + (to * 4.0f) - next) * t2 +
                (-previous + (from * 3.0f) - (to * 3.0f) + next) * t3) * 0.5f;
        }

        public static Vector3 BezierQuadraticCurve(Vector3 p0, Vector3 p1, Vector3 p2, float t) {
            t = Mathf.Clamp01(t);
            float b = 1.0f - t;
            float a2 = t * t;
            float b2 = b * b;
            float ab_m2 = 2.0f * t * b;
            return p2 * a2 + p1 * ab_m2 + p0 * b2;
        }

        public static Vector3 BezierCubicCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
            t = Mathf.Clamp01(t);
            float b = 1.0f - t;
            float a3 = t * t * t;
            float b3 = b * b * b;
            float a2b_m3 = 3.0f * t * t * b;
            float ab2_m3 = 3.0f * t * b * b;
            return p3 * a3 + p2 * a2b_m3 + p1 * ab2_m3 + p0 * b3;

        }

        public static Vector3 BezierCubicTangent(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {

            Vector3 C1 = (p3 - (3.0f * p2) + (3.0f * p1) - p0);
            Vector3 C2 = ((3.0f * p2) - (6.0f * p1) + (3.0f * p0));
            Vector3 C3 = ((3.0f * p1) - (3.0f * p0));
            return ((3.0f * C1 * t * t) + (2.0f * C2 * t) + C3);

        }

#endregion

#region float

        public static float Linear(float from, float to, float t) {
            if (t < 0)
                return from;
            if (t > 1)
                return to;
            return Mathf.Lerp(from, to, t);
        }

        public static float SmoothStep(float from, float to, float t) {
            if (t < 0)
                return from;
            if (t > 1)
                return to;
            return Mathf.Lerp(from, to, t * t * (3 - 2 * t));
        }

        public static float Deceleration(float from, float to, float t) {
            if (t < 0)
                return from;
            if (t > 1)
                return to;
            float m1 = (1 - t);
            return Mathf.Lerp(from, to, 1 - m1 * m1);
        }

        public static float Acceleration(float from, float to, float t) {
            if (t < 0)
                return from;
            if (t > 1)
                return to;
            return Mathf.Lerp(from, to, t * t);
        }

        public static float CatmullRomSpline(float previous, float from, float to, float next, float t) {
            t = Mathf.Clamp01(t);
            float t2 = t * t;
            float t3 = t2 * t;
            return ((from * 2.0f) +
                (-previous + to) * t +
                ((previous * 2.0f) - (from * 5.0f) + (to * 4.0f) - next) * t2 +
                (-previous + (from * 3.0f) - (to * 3.0f) + next) * t3) * 0.5f;
        }

        public static float BezierQuadraticCurve(float p0, float p1, float p2, float t) {
            t = Mathf.Clamp01(t);
            float b = 1.0f - t;
            float a2 = t * t;
            float b2 = b * b;
            float ab_m2 = 2.0f * t * b;
            return p2 * a2 + p1 * ab_m2 + p0 * b2;
        }

        public static float BezierCubicCurve(float p0, float p1, float p2, float p3, float t) {
            t = Mathf.Clamp01(t);
            float b = 1.0f - t;
            float a3 = t * t * t;
            float b3 = b * b * b;
            float a2b_m3 = 3.0f * t * t * b;
            float ab2_m3 = 3.0f * t * b * b;
            return p3 * a3 + p2 * a2b_m3 + p1 * ab2_m3 + p0 * b3;
        }

#endregion
    }
}
