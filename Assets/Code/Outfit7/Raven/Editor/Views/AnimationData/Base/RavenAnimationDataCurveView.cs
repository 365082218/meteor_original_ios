#if !STARLITE_EDITOR
using Outfit7.Logic.Util;
#else
using Starlite;
#endif

using UnityEditor;
using UnityEngine;

namespace Starlite.Raven {

    public class RavenAnimationDataCurveView<T> : RavenAnimationDataBaseView<T> {
        protected const int c_CurveQuality = 25;

        protected RavenAnimationDataCurve<T> m_AnimationDataCurveBase;

        protected readonly Color[] m_Colors = new Color[] { Color.red, Color.green, Color.blue, Color.white };

        public override void Initialize(RavenAnimationPropertyBaseView<T> propertyView, RavenAnimationDataComponentBase animationData) {
            base.Initialize(propertyView, animationData);
            m_AnimationDataCurveBase = animationData as RavenAnimationDataCurve<T>;
        }

        protected override void OnDrawGui(Rect position) {
        }

        protected override void OnDrawExtendedGui(Rect position) {
            const float width = 50f;
            const float height = 15f;
            const float offset = 20f;
            const float syncSize = 20f;

            bool drawGui = position.width > 100f;

            DrawAnimationData(position);

            if (drawGui) {
                var easeTypePopupWidth = Mathf.Min(60f, position.width - syncSize);
                m_AnimationDataCurveBase.EaseType = (ERavenEaseType)EditorGUI.EnumPopup(new Rect(position.x, position.y, easeTypePopupWidth, 15f), m_AnimationDataCurveBase.EaseType);
                m_AnimationDataCurveBase.SyncToCurrent = EditorGUI.Toggle(new Rect(position.x + easeTypePopupWidth, position.y, syncSize, 15f), m_AnimationDataCurveBase.SyncToCurrent);
                var nAnimationCurves = m_AnimationDataCurveBase.UniformCurves ? 1 : m_AnimationDataCurveBase.Curves.Length;
                for (int i = 0; i < nAnimationCurves; ++i) {
                    var curve = m_AnimationDataCurveBase.Curves[i];
                    curve = EditorGUI.CurveField(new Rect(position.x, position.y + offset + i * height, Mathf.Min(width, position.width), height), "", curve);
                }
                if (m_AnimationDataCurveBase.Curves.Length > 1) {
                    m_AnimationDataCurveBase.UniformCurves = EditorGUI.Toggle(new Rect(position.x, position.y + offset + nAnimationCurves * height, 16f, 16f), m_AnimationDataCurveBase.UniformCurves);
                }
            }
        }

        protected override bool OnHandleInput(Vector2 mousePosition) {
            return false;
        }

        protected bool ShouldDrawComponent(int index) {
            return m_PropertyBase.ApplyValues == null || m_PropertyBase.ApplyValues[index];
        }

        private void DrawAnimationData(Rect position) {
            var left = position.x;
            var right = left + position.width / m_AnimationDataCurveBase.RepeatCount;
            var top = position.y;
            var bottom = top + position.height;
            var width = right - left;

            var minMax = m_AnimationDataCurveBase.GetMinMax();
            var nCurves = m_AnimationDataCurveBase.Curves.Length;
            for (int i = 0; i < m_AnimationDataCurveBase.RepeatCount; ++i) {
                for (int j = 0; j < nCurves; j++) {
                    if (m_PropertyBase.PropertyType != ERavenAnimationPropertyType.Set || ShouldDrawComponent(j)) {
                        var curve = m_AnimationDataCurveBase.Curves[j];
                        DrawCurve(left, right, top, bottom, minMax, curve, j, i);
                        DrawDope(left, right, top, bottom, curve, j, nCurves);
                    }
                }
                left += width;
                right += width;
            }
        }

        private void DrawCurve(float left, float right, float top, float bottom, Vector2 minMax, AnimationCurve curve, int curveIndex, int repeatIndex) {
            var startPoint = new Vector2(Mathf.Lerp(left, right, 0), Mathf.Lerp(bottom, top, Mathf.InverseLerp(minMax.x, minMax.y, curve.Evaluate(0))));
            var interval = 1.0f / c_CurveQuality;
            for (int s = 0; s <= c_CurveQuality; s++) {
                var position = interval * s;
                var inputPosition = (float)m_AnimationDataCurveBase.GetTimeForRepeatableMirrorEditor(position, 1, m_AnimationDataCurveBase.RepeatCount, m_AnimationDataCurveBase.Mirror);
                var curvePosition = RavenEaseUtility.Evaluate(m_AnimationDataCurveBase.EaseType, inputPosition, 1, 1, 1);
                var endPoint = new Vector2(Mathf.Lerp(left, right, position), Mathf.Lerp(bottom, top, Mathf.InverseLerp(minMax.x, minMax.y, curve.Evaluate((float)curvePosition))));
                GUIUtil.DrawFatLine(startPoint, endPoint, 5f, m_Colors[curveIndex]);
                startPoint = endPoint;
            }
        }

        private void DrawDope(float left, float right, float top, float bottom, AnimationCurve curve, int curveIndex, int totalCurves) {
            for (int i = 0; i < curve.keys.Length; ++i) {
                var key = curve.keys[i];
                EditorGUI.DrawRect(new Rect(Mathf.Lerp(left, right, key.time) - 3f, bottom - 8 * (totalCurves - curveIndex) + 2f, 6f, 6f), m_Colors[curveIndex]);
            }
        }
    }
}