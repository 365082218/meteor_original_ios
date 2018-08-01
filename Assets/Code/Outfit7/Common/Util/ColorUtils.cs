//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using UnityEngine;

namespace Outfit7.Util {

    /// <summary>
    /// Color utilities.
    /// </summary>
    public static class ColorUtils {

        public struct HsbColor {
            public float h;
            public float s;
            public float b;
            public float a;

            public HsbColor(float h, float s, float b, float a) {
                this.h = h;
                this.s = s;
                this.b = b;
                this.a = a;
            }

            public HsbColor(float h, float s, float b) {
                this.h = h;
                this.s = s;
                this.b = b;
                this.a = 1f;
            }

            public HsbColor(Color col) {
                HsbColor temp = FromColor(col);
                h = temp.h;
                s = temp.s;
                b = temp.b;
                a = temp.a;
            }

            public static HsbColor FromColor(Color color) {
                HsbColor ret = new HsbColor(0f, 0f, 0f, color.a);

                float r = color.r;
                float g = color.g;
                float b = color.b;

                float max = Mathf.Max(r, Mathf.Max(g, b));

                if (max <= 0) {
                    return ret;
                }

                float min = Mathf.Min(r, Mathf.Min(g, b));
                float dif = max - min;

                if (max > min) {
                    if (g == max) {
                        ret.h = (b - r) / dif * 60f + 120f;
                    } else if (b == max) {
                        ret.h = (r - g) / dif * 60f + 240f;
                    } else if (b > g) {
                        ret.h = (g - b) / dif * 60f + 360f;
                    } else {
                        ret.h = (g - b) / dif * 60f;
                    }
                    if (ret.h < 0) {
                        ret.h = ret.h + 360f;
                    }
                } else {
                    ret.h = 0;
                }

                ret.h *= 1f / 360f;
                ret.s = (dif / max) * 1f;
                ret.b = max;

                return ret;
            }

            public static Color ToColor(HsbColor hsbColor) {
                float r = hsbColor.b;
                float g = hsbColor.b;
                float b = hsbColor.b;
                if (hsbColor.s != 0) {
                    float max = hsbColor.b;
                    float dif = hsbColor.b * hsbColor.s;
                    float min = hsbColor.b - dif;

                    float h = hsbColor.h * 360f;

                    if (h < 60f) {
                        r = max;
                        g = h * dif / 60f + min;
                        b = min;
                    } else if (h < 120f) {
                        r = -(h - 120f) * dif / 60f + min;
                        g = max;
                        b = min;
                    } else if (h < 180f) {
                        r = min;
                        g = max;
                        b = (h - 120f) * dif / 60f + min;
                    } else if (h < 240f) {
                        r = min;
                        g = -(h - 240f) * dif / 60f + min;
                        b = max;
                    } else if (h < 300f) {
                        r = (h - 240f) * dif / 60f + min;
                        g = min;
                        b = max;
                    } else if (h <= 360f) {
                        r = max;
                        g = min;
                        b = -(h - 360f) * dif / 60 + min;
                    } else {
                        r = 0;
                        g = 0;
                        b = 0;
                    }
                }

                return new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b), hsbColor.a);
            }

            public Color ToColor() {
                return ToColor(this);
            }

            public override string ToString() {
                return "H:" + h + " S:" + s + " B:" + b;
            }

            public static HsbColor Lerp(HsbColor a, HsbColor b, float t) {
                float h, s;

                //check special case black (color.b==0): interpolate neither hue nor saturation!
                //check special case grey (color.s==0): don't interpolate hue!
                if (a.b == 0) {
                    h = b.h;
                    s = b.s;
                } else if (b.b == 0) {
                    h = a.h;
                    s = a.s;
                } else {
                    if (a.s == 0) {
                        h = b.h;
                    } else if (b.s == 0) {
                        h = a.h;
                    } else {
                        // works around bug with LerpAngle
                        float angle = Mathf.LerpAngle(a.h * 360f, b.h * 360f, t);
                        while (angle < 0f)
                            angle += 360f;
                        while (angle > 360f)
                            angle -= 360f;
                        h = angle / 360f;
                    }
                    s = Mathf.Lerp(a.s, b.s, t);
                }
                return new HsbColor(h, s, Mathf.Lerp(a.b, b.b, t), Mathf.Lerp(a.a, b.a, t));
            }
        }

        public static Color32 IntToColor32(int intColor) {
            byte A = (byte) ((intColor >> 24) & 0xFF);
            byte B = (byte) ((intColor >> 16) & 0xFF);
            byte G = (byte) ((intColor >> 8) & 0xFF);
            byte R = (byte) (intColor & 0xFF);
            return new Color32(R, G, B, A);
        }

        public static int Color32ToInt(Color32 color) {
            int intColor = (((int) color.a) << 24) + (((int) color.b) << 16) + (((int) color.g) << 8) + ((int) color.r);
            return intColor;
        }
    }
}
