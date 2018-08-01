
using System;
using UnityEngine;

namespace Outfit7.Util {

    public static class FloatingPointUtils {

        public static int FloatToHalf(float f) {
            int i = BitConverter.ToInt32(BitConverter.GetBytes(f), 0);
            int s = (i >> 16) & 0x00008000;
            int e = ((i >> 23) & 0x000000ff) - (127 - 15);
            int m = i & 0x007fffff;
            if (e <= 0) {
                if (e < -10) {
                    return 0;
                }
                m = (m | 0x00800000) >> (1 - e);
                return s | (m >> 13);
            } else if (e == 0xff - (127 - 15)) {
                if (m == 0) { // Inf
                    return s | 0x7c00;
                } else {    // NAN
                    m >>= 13;
                    return s | 0x7c00 | m | (m == 0 ? 1 : 0);
                }
            } else {
                if (e > 30) { // Overflow
                    return s | 0x7c00;
                }
                return s | (e << 10) | (m >> 13);
            }
        }

        public static float HalfToFloat(int y) {
            int s = (y >> 15) & 0x00000001;
            int e = (y >> 10) & 0x0000001f;
            int m = y & 0x000003ff;
            if (e == 0) {
                if (m == 0) { // Plus or minus zero
                    return BitConverter.ToSingle(BitConverter.GetBytes(s << 31), 0);
                } else { // Denormalized number -- renormalize it
                    while ((m & 0x00000400) == 0) {
                        m <<= 1;
                        e -= 1;
                    }
                    e += 1;
                    m &= ~0x00000400;
                }
            } else if (e == 31) {
                if (m == 0) { // Inf
                    return BitConverter.ToSingle(BitConverter.GetBytes((s << 31) | 0x7f800000), 0);
                } else { // NaN
                    return BitConverter.ToSingle(BitConverter.GetBytes((s << 31) | 0x7f800000 | (m << 13)), 0);
                }
            }
            e = e + (127 - 15);
            m = m << 13;
            return BitConverter.ToSingle(BitConverter.GetBytes((s << 31) | (e << 23) | m), 0);
        }

        public static int Vector3ToCMP(Vector3 vec) {
            int iX = (int) (vec.x * 1023.0f);
            int iY = (int) (vec.y * 1023.0f);
            int iZ = (int) (vec.z * 511.0f);
            //
            return (iX & 0x7FF) | ((iY & 0x7FF) << 11) | ((iZ & 0x3FF) << 22);
        }

        public static void CMPToFVector3(int num, ref Vector3 vector) {
            int ix = (num);
            int iy = (num >> 11);
            int iz = (num >> 22);
            vector.x = (float) ((ix & 0x3FF) | (~0x3FF * ((ix & 0x400) >> 10))) / 1023.0f;
            vector.y = (float) ((iy & 0x3FF) | (~0x3FF * ((iy & 0x400) >> 10))) / 1023.0f;
            vector.z = (float) ((iz & 0x1FF) | (~0x1FF * ((iz & 0x200) >> 9))) / 511.0f;
        }

        public static bool AreAlmostEqual(float a, float b) {
            return AreAlmostEqual(a, b, 0.0001f);
        }

        public static bool AreAlmostEqual(float a, float b, float epsilon) {
            return Mathf.Abs(a - b) < epsilon;
        }

        public static bool AreAlmostEqual(double a, double b) {
            return AreAlmostEqual(a, b, 0.0001);
        }

        public static bool AreAlmostEqual(double a, double b, double epsilon) {
            return Math.Abs(a - b) < epsilon;
        }
    }
}
