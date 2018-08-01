#ifdef STARLITE
#pragma once

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenInterpolation {
        public:
            RavenInterpolation() = delete;

            template <class T> static T Acceleration(const T& from, const T& to, float t, typename std::enable_if<std::is_integral<T>::value>::type* type = nullptr) {
                if (t < 0) {
                    return from;
                }
                if (t > 1) {
                    return to;
                }
                return Math::Lerp(from, to, t * t);
            }

            template <class T> static T Acceleration(const T& from, const T& to, float t, typename std::enable_if<!std::is_integral<T>::value>::type* type = nullptr) {
                if (t < 0) {
                    return from;
                }
                if (t > 1) {
                    return to;
                }
                return T::Lerp(from, to, t * t);
            }

            template <class T> static T BezierCubicCurve(const T& p0, const T& p1, const T& p2, const T& p3, float t) {
                t = Mathf::Clamp01(t);
                float b = 1.0f - t;
                float a3 = t * t * t;
                float b3 = b * b * b;
                float a2b_m3 = 3.0f * t * t * b;
                float ab2_m3 = 3.0f * t * b * b;
                return p3 * a3 + p2 * a2b_m3 + p1 * ab2_m3 + p0 * b3;
            }

            template <class T> static T BezierCubicTangent(const T& p0, const T& p1, const T& p2, const T& p3, float t) {
                auto C1 = (p3 - (3.0f * p2) + (3.0f * p1) - p0);
                auto C2 = ((3.0f * p2) - (6.0f * p1) + (3.0f * p0));
                auto C3 = ((3.0f * p1) - (3.0f * p0));
                return ((3.0f * C1 * t * t) + (2.0f * C2 * t) + C3);
            }

            template <class T> static T BezierQuadraticCurve(const T& p0, const T& p1, const T& p2, float t) {
                t = Mathf::Clamp01(t);
                float b = 1.0f - t;
                float a2 = t * t;
                float b2 = b * b;
                float ab_m2 = 2.0f * t * b;
                return p2 * a2 + p1 * ab_m2 + p0 * b2;
            }

            template <class T> static T CatmullRomSpline(const T& previous, const T& from, const T& to, const T& next, float t) {
                t = Mathf::Clamp01(t);
                float t2 = t * t;
                float t3 = t2 * t;
                return ((from * 2.0f) + (-previous + to) * t + ((previous * 2.0f) - (from * 5.0f) + (to * 4.0f) - next) * t2 + (-previous + (from * 3.0f) - (to * 3.0f) + next) * t3) * 0.5f;
            }

            template <class T> static T Deceleration(const T& from, const T& to, float t, typename std::enable_if<std::is_integral<T>::value>::type* type = nullptr) {
                if (t < 0) {
                    return from;
                }
                if (t > 1) {
                    return to;
                }
                float m1 = (1 - t);
                return Math::Lerp(from, to, 1 - m1 * m1);
            }

            template <class T> static T Deceleration(const T& from, const T& to, float t, typename std::enable_if<!std::is_integral<T>::value>::type* type = nullptr) {
                if (t < 0) {
                    return from;
                }
                if (t > 1) {
                    return to;
                }
                float m1 = (1 - t);
                return T::Lerp(from, to, 1 - m1 * m1);
            }

            template <class T> static T Linear(const T& from, const T& to, float t, typename std::enable_if<std::is_integral<T>::value>::type* type = nullptr) {
                if (t < 0) {
                    return from;
                }
                if (t > 1)
                    return to;
                return Math::Lerp(from, to, t);
            }

            template <class T> static T Linear(const T& from, const T& to, float t, typename std::enable_if<!std::is_integral<T>::value>::type* type = nullptr) {
                if (t < 0) {
                    return from;
                }
                if (t > 1) {
                    return to;
                }
                return T::Lerp(from, to, t);
            }

            template <class T> static T SmoothStep(const T& from, const T& to, float t, typename std::enable_if<std::is_integral<T>::value>::type* type = nullptr) {
                if (t < 0) {
                    return from;
                }
                if (t > 1) {
                    return to;
                }
                return Math::Lerp(from, to, t * t * (3 - 2 * t));
            }

            template <class T> static T SmoothStep(const T& from, const T& to, float t, typename std::enable_if<!std::is_integral<T>::value>::type* type = nullptr) {
                if (t < 0) {
                    return from;
                }
                if (t > 1) {
                    return to;
                }
                return T::Lerp(from, to, t * t * (3 - 2 * t));
            }
        };
    } // namespace Raven
} // namespace Starlite
#endif