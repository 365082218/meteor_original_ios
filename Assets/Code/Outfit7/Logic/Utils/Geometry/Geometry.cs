//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using UnityEngine;

namespace Outfit7.Logic.Util {

    /// <summary>
    /// A collection of Vector and non-vector, 2D and 3D math functions
    /// </summary>
    public static partial class Geometry {

        public static readonly Vector3[] Axis = { Vector2.right, Vector3.up, Vector3.forward };

        /// <summary>
        /// Gets the closest point on line segment.
        /// </summary>
        public static Vector3 GetClosestPointOnLineSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd) {
            Vector3 lineVec = lineEnd - lineStart;
            float lineLen = lineVec.magnitude;
            // If line is zero or almost zero just return
            if (lineLen < 0.00001f)
                return lineStart;
            lineVec /= lineLen;
            Vector3 pointVec = point - lineStart;
            float delta = Vector3.Dot(lineVec, pointVec);
            return lineStart + lineVec * Mathf.Clamp(delta, 0.0f, lineLen);
        }

        /// <summary>
        /// Determines if the point is in capsule.
        /// </summary>
        public static bool IsPointInCapsule(Vector3 point, Vector3 start, Vector3 end, float radius) {
            Vector3 closestPoint = GetClosestPointOnLineSegment(point, start, end);
            float distance = Vector3.Distance(point, closestPoint);
            return distance <= radius;
        }

        /// <summary>
        /// Determines if the point is in box.
        /// </summary>
        public static bool IsPointInBox(Vector3 point, Vector3 center, Vector3 extents) {
            Vector3 centerRelativePoint = point - center;
            return Mathf.Abs(centerRelativePoint.x) <= extents.x && Mathf.Abs(centerRelativePoint.y) <= extents.y && Mathf.Abs(centerRelativePoint.z) <= extents.z;
        }

        /// <summary>
        /// Determines if the point is in sphere.
        /// </summary>
        public static bool IsPointInSphere(Vector3 point, Vector3 center, float radius) {
            float distance = Vector3.Distance(center, point);
            return distance <= radius;
        }

        private static bool CheckLiangBrasky(float p, float q, ref float u1, ref float u2) {
            if (p == 0.0f) {
                if (q < 0) {
                    return false;
                }
            }
            float t = q / p;
            if (p < 0 && u1 < t) {
                u1 = t;
            }
            if (p > 0 && u2 > t) {
                u2 = t;
            }
            return true;
        }

        /// <summary>
        /// Intersects the line with a rectangle.
        /// </summary>
        public static bool IntersectLineRectangle(Vector2 a, Vector2 b, Rect rect, out Vector2 intersection) {
            Vector2 v = b - a;
            float u1 = -100000.0f;
            float u2 = 100000.0f;
            intersection = Vector2.zero;
            if (!CheckLiangBrasky(-v.x, a.x - rect.xMin, ref u1, ref u2) ||
                !CheckLiangBrasky(v.x, rect.xMax - a.x, ref u1, ref u2) ||
                !CheckLiangBrasky(-v.y, a.y - rect.yMin, ref u1, ref u2) ||
                !CheckLiangBrasky(v.y, rect.yMax - a.y, ref u1, ref u2)) {
                return false;
            }
            if (u1 > u2 || u1 > 1 || u1 < 0)
                return false;
            intersection = a + v * u1;

            return true;
        }

        public static bool IntersectLineSphere(Vector3 lineStart, Vector3 lineEnd, Vector3 spherePosition, float sphereRadius, out Vector3 hitPoint) {
            Vector3 lineDirection = lineEnd - lineStart;
            Vector3 diff = lineStart - spherePosition;
            float a0 = Vector3.Dot(diff, diff) - (sphereRadius * sphereRadius);
            float a1 = Vector3.Dot(lineDirection, diff);
            float discr = a1 * a1 - a0;
            if (discr < 0.0f) {
                hitPoint = Vector3.zero;
                return false;
            } else if (discr >= 0.000001f) {
                float root = Mathf.Sqrt(discr);
                float lineParameter = -a1 - root;
                hitPoint = lineStart + lineParameter * lineDirection;
                return true;
            } else {
                float lineParameter = -a1;
                hitPoint = lineStart + lineParameter * lineDirection;
                return true;
            }
        }

        public static bool IntersectLineLine2D(Vector2 line1A, Vector2 line1B, Vector2 line2A, Vector2 line2B, out Vector2 intersectionPoint, float epsilon) {
            Vector2 s1 = line1B - line1A;
            Vector2 s2 = line2B - line2A;
            Vector2 u = line1A - line2A;
            float ip = 1.0f / (-s2.x * s1.y + s1.x * s2.y);
            float s = (-s1.y * u.x + s1.x * u.y) * ip;
            float t = (s2.x * u.y - s2.y * u.x) * ip;
            float oneMinusEpsilon = 1.0f - epsilon;
            if (s >= epsilon && s <= oneMinusEpsilon && t >= epsilon && t <= oneMinusEpsilon) {
                intersectionPoint = line1A + (s1 * t);
                return true;
            }
            intersectionPoint = Vector2.zero;
            return false;
        }

        public static bool VectorConeClamp(Vector3 initialiDirection, float angle, ref Vector3 direction) {
            float cos = Mathf.Cos(angle * Mathf.Deg2Rad);
            if (Vector3.Dot(initialiDirection, direction) > cos) {
                return false;
            } else {
                Vector3 axis = Vector3.Cross(initialiDirection, direction);
                if (axis.sqrMagnitude < 0.0000001f) {
                    axis = Vector3.up;
                }
                Quaternion rotation = Quaternion.AngleAxis(angle, axis);
                direction = rotation * initialiDirection;
                return true;
            }
        }

    }

}
