using UnityEngine;

namespace Outfit7.UI {
    public static class RectTransformHelper {
   
        #region IsRectIntersectingRect

        public static bool IsRectTransformIntersectingRectTransform(RectTransform rectTransform1, RectTransform rectTransform2) {
            Vector3[] rectTransform1Corners = new Vector3[4];
            rectTransform1.GetWorldCorners(rectTransform1Corners);

            Vector3[] rectTransform2Corners = new Vector3[4];
            rectTransform2.GetWorldCorners(rectTransform2Corners);

            return IsRectIntersectingRect(rectTransform1Corners, rectTransform2Corners);
        }

        public static bool IsRectIntersectingRect(Vector3[] rect1, Vector3[] rect2) {

            Vector3 smallBottomLeft = rect1[0];
            Vector3 smallTopRight = rect1[2];

            Vector3 bigBottomLeft = rect2[0];
            Vector3 bigTopRight = rect2[2];

            return smallTopRight.x >= bigBottomLeft.x &&
            smallTopRight.y >= bigBottomLeft.y &&
            smallBottomLeft.x <= bigTopRight.x &&
            smallBottomLeft.y <= bigTopRight.y;
        }

        #endregion

        #region IsRectWholeInRect

        public static bool IsRectTransformWholeInRectTransform(RectTransform rectTransform1, RectTransform rectTransform2) {
            Vector3[] rectTransform1Corners = new Vector3[4];
            rectTransform1.GetWorldCorners(rectTransform1Corners);

            Vector3[] rectTransform2Corners = new Vector3[4];
            rectTransform2.GetWorldCorners(rectTransform2Corners);

            return IsRectWholeInRect(rectTransform1Corners, rectTransform2Corners);
        }

        public static bool IsRectWholeInRect(Vector3[] rect1, Vector3[] rect2) {

            Vector3 smallBottomLeft = rect1[0];
            Vector3 smallTopRight = rect1[2];

            Vector3 bigBottomLeft = rect2[0];
            Vector3 bigTopRight = rect2[2];

            return smallBottomLeft.x >= bigBottomLeft.x &&
            smallBottomLeft.y >= bigBottomLeft.y &&
            smallTopRight.x <= bigTopRight.x &&
            smallTopRight.y <= bigTopRight.y;
        }

        #endregion

        #region IsPointInRect

        public static bool IsPointTransformInRectTransform(Transform pointTransform, RectTransform rectTransform) {
            return IsPointInRectTransform(pointTransform.position, rectTransform);
        }

        public static bool IsPointInRectTransform(Vector3 point, RectTransform rectTransform) {
            Vector3[] rectTransformCorners = new Vector3[4];
            rectTransform.GetWorldCorners(rectTransformCorners);

            return IsPointInRect(point, rectTransformCorners);
        }

        public static bool IsPointInRect(Vector3 point, Vector3[] rect) {
            Vector3 rectBottomLeft = rect[0];
            Vector3 rectTopRight = rect[2];

            return point.x >= rectBottomLeft.x &&
            point.y >= rectBottomLeft.y &&
            point.x <= rectTopRight.x &&
            point.y <= rectTopRight.y;
        }

        #endregion

        #region GetPointNormalizedPositionInRect

        public static Vector2 GetPointTransformNormalizedPositionInRectTransform(Transform pointTransform, RectTransform rectTransform) {
            return GetPointNormalizedPositionInRectTransform(pointTransform.position, rectTransform);
        }

        public static Vector2 GetPointNormalizedPositionInRectTransform(Vector3 point, RectTransform rectTransform) {
            Vector3[] rectTransformCorners = new Vector3[4];
            rectTransform.GetWorldCorners(rectTransformCorners);

            return GetPointNormalizedPositionInRect(point, rectTransformCorners);
        }

        public static Vector2 GetPointNormalizedPositionInRect(Vector3 point, Vector3[] rect) {
            Vector3 rectBottomLeft = rect[0];
            Vector3 rectTopRight = rect[2];

            Vector3 rectMaxVal = rectTopRight - rectBottomLeft;
            Vector3 pointVal = point - rectBottomLeft;


            return new Vector2(pointVal.x / rectMaxVal.x, pointVal.y / rectMaxVal.y);
        }

        #endregion

        #region IfIntersectsShowElseHide

        public static void IfIntersectsShowElseHide(RectTransform rectTransform1, RectTransform rectTransform2) {
            if (IsRectTransformIntersectingRectTransform(rectTransform1, rectTransform2)) {
                if (!rectTransform1.gameObject.activeSelf) {
                    rectTransform1.gameObject.SetActive(true);
                }
            } else {
                if (rectTransform1.gameObject.activeSelf) {
                    rectTransform1.gameObject.SetActive(false);
                }
            }
        }

        #endregion
    }
}