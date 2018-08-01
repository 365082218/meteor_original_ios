//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using UnityEngine;

namespace Outfit7.Logic.Util {

    public static partial class Geometry {
        public static Vector2 GetBarycentricFromPosition(Vector3 a, Vector3 b, Vector3 c, Vector3 p) {
            Vector3 v0 = c - a;
            Vector3 v1 = b - a;
            Vector3 v2 = p - a;

            // Compute dot products
            float dot00 = Vector3.Dot(v0, v0);
            float dot01 = Vector3.Dot(v0, v1);
            float dot02 = Vector3.Dot(v0, v2);
            float dot11 = Vector3.Dot(v1, v1);
            float dot12 = Vector3.Dot(v1, v2);

            // Compute barycentric coordinates
            float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;
            return new Vector2(u, v);
        }

        public static Vector3 GetPositionFromBarycentric(Vector3 a, Vector3 b, Vector3 c, Vector2 bc) {
            Vector3 edge0 = b - a;
            Vector3 edge1 = c - a;
            return a + bc.x * edge0 + bc.y * edge1;
        }

        public static float GetTriangleArea(Vector3 a, Vector3 b, Vector3 c) {
            float sa = Vector3.Distance(a, b);
            float sb = Vector3.Distance(b, c);
            float sc = Vector3.Distance(c, a);
            float s = (sa + sb + sc) * 0.5f;
            return Mathf.Sqrt(s * (s - sa) * (s - sb) * (s - sc));
        }

        public static bool IsPositionInTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 p, float epsilon) {
            Vector2 bc = GetBarycentricFromPosition(a, b, c, p);
            return bc.x >= -epsilon && bc.y >= -epsilon && (bc.x + bc.y) <= 1.0f + epsilon;
        }

        public static Vector3 GetClosestPointOnTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 p, out Vector2 bc) {
            Vector3 diff = a - p;
            Vector3 edge0 = b - a;
            Vector3 edge1 = c - a;
            float a00 = edge0.sqrMagnitude;
            float a01 = Vector3.Dot(edge0, edge1);
            float a11 = edge1.sqrMagnitude;
            float b0 = Vector3.Dot(diff, edge0);
            float b1 = Vector3.Dot(diff, edge1);
            float det = Mathf.Abs(a00 * a11 - a01 * a01);
            float s = a01 * b1 - a11 * b0;
            float t = a01 * b0 - a00 * b1;

            if (s + t <= det) {
                if (s < 0) {
                    if (t < 0) {  // region 4
                        if (b0 < 0) {
                            t = 0;
                            if (-b0 >= a00) {
                                s = 1;
                            } else {
                                s = -b0 / a00;
                            }
                        } else {
                            s = 0;
                            if (b1 >= 0) {
                                t = 0;
                            } else if (-b1 >= a11) {
                                t = 1;
                            } else {
                                t = -b1 / a11;
                            }
                        }
                    } else {  // region 3
                        s = 0;
                        if (b1 >= 0) {
                            t = 0;
                        } else if (-b1 >= a11) {
                            t = 1;
                        } else {
                            t = -b1 / a11;
                        }
                    }
                } else if (t < 0) {  // region 5
                    t = 0;
                    if (b0 >= 0) {
                        s = 0;
                    } else if (-b0 >= a00) {
                        s = 1;
                    } else {
                        s = -b0 / a00;
                    }
                } else {  // region 0
                    // minimum at interior point
                    float invDet = (1) / det;
                    s *= invDet;
                    t *= invDet;
                }
            } else {
                float tmp0, tmp1, numer, denom;

                if (s < 0) {  // region 2
                    tmp0 = a01 + b0;
                    tmp1 = a11 + b1;
                    if (tmp1 > tmp0) {
                        numer = tmp1 - tmp0;
                        denom = a00 - (2) * a01 + a11;
                        if (numer >= denom) {
                            s = 1;
                            t = 0;
                        } else {
                            s = numer / denom;
                            t = 1 - s;
                        }
                    } else {
                        s = 0;
                        if (tmp1 <= 0) {
                            t = 1;
                        } else if (b1 >= 0) {
                            t = 0;
                        } else {
                            t = -b1 / a11;
                        }
                    }
                } else if (t < 0) {  // region 6
                    tmp0 = a01 + b1;
                    tmp1 = a00 + b0;
                    if (tmp1 > tmp0) {
                        numer = tmp1 - tmp0;
                        denom = a00 - (2) * a01 + a11;
                        if (numer >= denom) {
                            t = 1;
                            s = 0;
                        } else {
                            t = numer / denom;
                            s = 1 - t;
                        }
                    } else {
                        t = 0;
                        if (tmp1 <= 0) {
                            s = 1;
                        } else if (b0 >= 0) {
                            s = 0;
                        } else {
                            s = -b0 / a00;
                        }
                    }
                } else {  // region 1
                    numer = a11 + b1 - a01 - b0;
                    if (numer <= 0) {
                        s = 0;
                        t = 1;
                    } else {
                        denom = a00 - (2) * a01 + a11;
                        if (numer >= denom) {
                            s = 1;
                            t = 0;
                        } else {
                            s = numer / denom;
                            t = 1 - s;
                        }
                    }
                }
            }
            bc = new Vector2(s, t);
            return a + s * edge0 + t * edge1;
        }

        private static void Sort(ref float a, ref float b) {
            if (a > b) {
                float c;
                c = a;
                a = b;
                b = c;
            }
        }

        private static bool EdgeEdgeTest(Vector3 V0, Vector3 U0, Vector3 U1, int i0, int i1, ref float Ax, ref float Ay, ref float Bx, ref float By, ref float Cx, ref float Cy, ref float f, ref float d, ref float e) {
            /*  this edge to edge test is based on Franlin Antonio's gem:
            "Faster Line Segment Intersection", in Graphics Gems III,
            pp. 199-202 */
            Bx = U0[i0] - U1[i0];
            By = U0[i1] - U1[i1];
            Cx = V0[i0] - U0[i0];
            Cy = V0[i1] - U0[i1];
            f = Ay * Bx - Ax * By;
            d = By * Cx - Bx * Cy;
            if ((f > 0 && d >= 0 && d <= f) || (f < 0 && d <= 0 && d >= f)) {
                e = Ax * Cy - Ay * Cx;
                if (f > 0) {
                    if (e >= 0 && e <= f)
                        return true;
                } else {
                    if (e <= 0 && e >= f)
                        return true;
                }
            }
            return false;
        }

        private static bool EdgeAgainstTriangleEdges(Vector3 V0, Vector3 V1, Vector3 U0, Vector3 U1, Vector3 U2, int i0, int i1) {
            float Ax = 0, Ay = 0, Bx = 0, By = 0, Cx = 0, Cy = 0, e = 0, d = 0, f = 0;
            Ax = V1[i0] - V0[i0];
            Ay = V1[i1] - V0[i1];
            /* test edge U0,U1 against V0,V1 */
            if (EdgeEdgeTest(V0, U0, U1, i0, i1, ref Ax, ref Ay, ref Bx, ref By, ref Cx, ref Cy, ref f, ref d, ref e)) {
                return true;
            }
            /* test edge U1,U2 against V0,V1 */
            if (EdgeEdgeTest(V0, U1, U2, i0, i1, ref Ax, ref Ay, ref Bx, ref By, ref Cx, ref Cy, ref f, ref d, ref e)) {
                return true;
            }
            /* test edge U2,U1 against V0,V1 */
            if (EdgeEdgeTest(V0, U2, U0, i0, i1, ref Ax, ref Ay, ref Bx, ref By, ref Cx, ref Cy, ref f, ref d, ref e)) {
                return true;
            }
            return false;
        }

        private static bool PointInTriangle(Vector3 V0, Vector3 U0, Vector3 U1, Vector3 U2, int i0, int i1) {
            float a, b, c, d0, d1, d2;
            /* is T1 completly inside T2? */
            /* check if V0 is inside tri(U0,U1,U2) */
            a = U1[i1] - U0[i1];
            b = -(U1[i0] - U0[i0]);
            c = -a * U0[i0] - b * U0[i1];
            d0 = a * V0[i0] + b * V0[i1] + c;
            a = U2[i1] - U1[i1];
            b = -(U2[i0] - U1[i0]);
            c = -a * U1[i0] - b * U1[i1];
            d1 = a * V0[i0] + b * V0[i1] + c;
            a = U0[i1] - U2[i1];
            b = -(U0[i0] - U2[i0]);
            c = -a * U2[i0] - b * U2[i1];
            d2 = a * V0[i0] + b * V0[i1] + c;
            if (d0 * d1 > 0.0f) {
                if (d0 * d2 > 0.0f) {
                    return true;
                }
            }
            return false;
        }

        private static bool CoplanarTriangleTriangle(Vector3 N, Vector3 V0, Vector3 V1, Vector3 V2, Vector3 U0, Vector3 U1, Vector3 U2) {
            Vector3 A = Vector3.zero;
            short i0, i1;
            /* first project onto an axis-aligned plane, that maximizes the area */
            /* of the triangles, compute indices: i0,i1. */
            A[0] = Mathf.Abs(N[0]);
            A[1] = Mathf.Abs(N[1]);
            A[2] = Mathf.Abs(N[2]);
            if (A[0] > A[1]) {
                if (A[0] > A[2]) {
                    i0 = 1;      /* A[0] is greatest */
                    i1 = 2;
                } else {
                    i0 = 0;      /* A[2] is greatest */
                    i1 = 1;
                }
            } else {   /* A[0]<=A[1] */
                if (A[2] > A[1]) {
                    i0 = 0;      /* A[2] is greatest */
                    i1 = 1;
                } else {
                    i0 = 0;      /* A[1] is greatest */
                    i1 = 2;
                }
            }

            /* test all edges of triangle 1 against the edges of triangle 2 */
            if (EdgeAgainstTriangleEdges(V0, V1, U0, U1, U2, i0, i1)) {
                return true;
            }
            if (EdgeAgainstTriangleEdges(V1, V2, U0, U1, U2, i0, i1)) {
                return true;
            }
            if (EdgeAgainstTriangleEdges(V2, V0, U0, U1, U2, i0, i1)) {
                return true;
            }

            /* finally, test if tri1 is totally contained in tri2 or vice versa */
            if (PointInTriangle(V0, U0, U1, U2, i0, i1)) {
                return true;
            }
            if (PointInTriangle(U0, V0, V1, V2, i0, i1)) {
                return true;
            }

            return false;
        }

        private static bool ComputeTriangleIntervals(Vector3 N1, Vector3 V0, Vector3 V1, Vector3 V2, Vector3 U0, Vector3 U1, Vector3 U2, float VV0, float VV1, float VV2, float D0, float D1, float D2, float D0D1, float D0D2, ref float A, ref float B, ref float C, ref float X0, ref float X1) {
            if (D0D1 > 0.0f) {
                /* here we know that D0D2<=0.0 */
                /* that is D0, D1 are on the same side, D2 on the other or on the plane */
                A = VV2;
                B = (VV0 - VV2) * D2;
                C = (VV1 - VV2) * D2;
                X0 = D2 - D0;
                X1 = D2 - D1;
            } else if (D0D2 > 0.0f) {
                /* here we know that d0d1<=0.0 */
                A = VV1;
                B = (VV0 - VV1) * D1;
                C = (VV2 - VV1) * D1;
                X0 = D1 - D0;
                X1 = D1 - D2;
            } else if (D1 * D2 > 0.0f || D0 != 0.0f) {
                /* here we know that d0d1<=0.0 or that D0!=0.0 */
                A = VV0;
                B = (VV1 - VV0) * D0;
                C = (VV2 - VV0) * D0;
                X0 = D0 - D1;
                X1 = D0 - D2;
            } else if (D1 != 0.0f) {
                A = VV1;
                B = (VV0 - VV1) * D1;
                C = (VV2 - VV1) * D1;
                X0 = D1 - D0;
                X1 = D1 - D2;
            } else if (D2 != 0.0f) {
                A = VV2;
                B = (VV0 - VV2) * D2;
                C = (VV1 - VV2) * D2;
                X0 = D2 - D0;
                X1 = D2 - D1;
            } else {
                /* triangles are coplanar */
                return true;
            }
            return false;
        }

        public static bool IntersectTriangleTriangle(Vector3 V0, Vector3 V1, Vector3 V2, Vector3 U0, Vector3 U1, Vector3 U2) {
            Vector3 E1, E2;
            Vector3 N1, N2;
            float d1, d2;
            float du0, du1, du2, dv0, dv1, dv2;
            Vector3 D;
            float du0du1, du0du2, dv0dv1, dv0dv2;
            short index;
            float vp0, vp1, vp2;
            float up0, up1, up2;
            float bb, cc, max;

            /* compute plane equation of triangle(V0,V1,V2) */
            E1 = V1 - V0;
            E2 = V2 - V0;
            N1 = Vector3.Cross(E1, E2);
            d1 = -Vector3.Dot(N1, V0);
            /* plane equation 1: N1.X+d1=0 */

            /* put U0,U1,U2 into plane equation 1 to compute signed distances to the plane*/
            du0 = Vector3.Dot(N1, U0) + d1;
            du1 = Vector3.Dot(N1, U1) + d1;
            du2 = Vector3.Dot(N1, U2) + d1;

            du0du1 = du0 * du1;
            du0du2 = du0 * du2;

            if (du0du1 > 0.0f && du0du2 > 0.0f) /* same sign on all of them + not equal 0 ? */
                return false;                    /* no intersection occurs */

            /* compute plane of triangle (U0,U1,U2) */
            E1 = U1 - U0;
            E2 = U2 - U0;
            N2 = Vector3.Cross(E1, E2);
            d2 = -Vector3.Dot(N2, U0);
            /* plane equation 2: N2.X+d2=0 */

            /* put V0,V1,V2 into plane equation 2 */
            dv0 = Vector3.Dot(N2, V0) + d2;
            dv1 = Vector3.Dot(N2, V1) + d2;
            dv2 = Vector3.Dot(N2, V2) + d2;

            dv0dv1 = dv0 * dv1;
            dv0dv2 = dv0 * dv2;

            if (dv0dv1 > 0.0f && dv0dv2 > 0.0f) /* same sign on all of them + not equal 0 ? */
                return false;                    /* no intersection occurs */

            /* compute direction of intersection line */
            D = Vector3.Cross(N1, N2);

            /* compute and index to the largest component of D */
            max = (float) Mathf.Abs(D[0]);
            index = 0;
            bb = (float) Mathf.Abs(D[1]);
            cc = (float) Mathf.Abs(D[2]);
            if (bb > max) {
                max = bb;
                index = 1;
            }
            if (cc > max) {
                max = cc;
                index = 2;
            }

            /* this is the simplified projection onto L*/
            vp0 = V0[index];
            vp1 = V1[index];
            vp2 = V2[index];

            up0 = U0[index];
            up1 = U1[index];
            up2 = U2[index];

            /* compute interval for triangle 1 */
            float a = 0, b = 0, c = 0, x0 = 0, x1 = 0;
            if (ComputeTriangleIntervals(N1, V0, V1, V2, U0, U1, U2, vp0, vp1, vp2, dv0, dv1, dv2, dv0dv1, dv0dv2, ref a, ref b, ref c, ref x0, ref x1)) {
                return CoplanarTriangleTriangle(N1, V0, V1, V2, U0, U1, U2);
                ;
            }

            /* compute interval for triangle 2 */
            float d = 0, e = 0, f = 0, y0 = 0, y1 = 0;
            if (ComputeTriangleIntervals(N1, V0, V1, V2, U0, U1, U2, up0, up1, up2, du0, du1, du2, du0du1, du0du2, ref d, ref e, ref f, ref y0, ref y1)) {
                return CoplanarTriangleTriangle(N1, V0, V1, V2, U0, U1, U2);
                ;
            }

            float xx, yy, xxyy, tmp;
            xx = x0 * x1;
            yy = y0 * y1;
            xxyy = xx * yy;

            tmp = a * xxyy;
            float isect10 = tmp + b * x1 * yy;
            float isect11 = tmp + c * x0 * yy;

            tmp = d * xxyy;
            float isect20 = tmp + e * xx * y1;
            float isect21 = tmp + f * xx * y0;

            Sort(ref isect10, ref isect11);
            Sort(ref isect20, ref isect21);

            if (isect11 < isect20 || isect21 < isect10)
                return false;
            return true;
        }

        public static bool IntersectTriangleTriangleEdge2D(Vector2[] triangleAPoints, Vector2[] triangleBPoints, float epsilon) {
            Vector2 point = Vector3.zero;
            for (int a = 0; a < 3; a++) {
                Vector2 pa1 = triangleAPoints[a];
                Vector2 pa2 = triangleAPoints[(a + 1) % 3];
                for (int b = 0; b < 3; b++) {
                    Vector2 pb1 = triangleBPoints[b];
                    Vector2 pb2 = triangleBPoints[(b + 1) % 3];
                    if (Geometry.IntersectLineLine2D(pa1, pa2, pb1, pb2, out point, epsilon)) {
                        return true;
                    }
                }
            }
            return false;
        }

    }
}

