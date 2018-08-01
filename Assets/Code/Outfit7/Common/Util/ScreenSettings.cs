//
//   Copyright (c) 2015 Outfit7. All rights reserved.
//

using UnityEngine;

namespace Outfit7.Util {
    public static class ScreenSettings {

        public static float MinSupportedAspectRatio = 4.0f / 3.0f;

        public static float SideClippingInPixels {
            get {
                return (Screen.width - Screen.height / MinSupportedAspectRatio) / 2.0f;
            }
        }

        public static float SideClippingInViewPortCoords {
            get {
                return SideClippingInPixels / Screen.width;
            }
        }

        public static Rect CameraRect {
            get {
                return new Rect(SideClippingInViewPortCoords, 0.0f, 1.0f - SideClippingInViewPortCoords * 2.0f, 1.0f);
            }
        }

        public static int AdjustedScreenHeight {
            get {
                return (int) (Screen.height * CameraRect.height);
            }
        }

        public static int AdjustedScreenWidth {
            get {
                return (int) (Screen.width * CameraRect.width);
            }
        }

        public static bool UseScreenClipping {
            get {
                float aspect = (float) Screen.height / (float) Screen.width;
                return aspect < MinSupportedAspectRatio;
            }
        }
    }
}

