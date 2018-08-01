using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;
using Outfit7.Util;
using Outfit7.Logic.StateMachineInternal;
using Outfit7.Logic.Util;

namespace Outfit7.Sequencer {
    public class PropertyTweenAnimationDataView : PropertyAnimationDataView {
        PropertyAnimationData AnimationData = null;
        private Rect CurveRect;
        private bool DontUpdateOnRecord = false;
        public static float CURVE_QUALITY = 25;
        private Texture2D ColorTexture = new Texture2D(128, 1);
        private int texWidth = 128;
        private Vector4 TexturePreviousStartValue = Vector3.one * -1f;
        private Vector4 TexturePreviousEndValue = Vector3.zero * -1f;

        //private GUIContent[] content = new GUIContent[] {new GUIContent("Amp"), new GUIContent("Per")}

        public override void OnInit(object animData) {
            AnimationData = animData as PropertyAnimationData;
            base.OnInit(animData);
        }

        public override string Name() {
            return "Tween Animation Data";
        }

        public override void OnDrawGui(BaseProperty property, Rect curveRect, List<Parameter> parameters, bool optimizedView, bool hover) {
            CurveRect = curveRect;
            Vector2 minMax = AnimationData.GetMinMax(property);

            PropertyTweenAnimationData animData = AnimationData as PropertyTweenAnimationData;

            if (property.GetDisplayMode() == BaseProperty.DisplayMode.CURVE) {
                DrawAnimationData(animData, property, curveRect, minMax);
            } else {
                DrawColorAnimationData(animData, property, curveRect, minMax);
            }

            float width = Mathf.Min(100f, curveRect.width) / 2f;
            float height = 15f;
            float offset = 20f;

            if (optimizedView) {
                if (hover) {
                    if (property.GetDisplayMode() == BaseProperty.DisplayMode.CURVE) {
                        ParameterFieldView.DrawParameterField(new Rect(curveRect.x, curveRect.y + offset, width, height * 5), animData.StartValue, parameters, property.GetValuesUsed());
                        ParameterFieldView.DrawParameterField(new Rect(curveRect.x + curveRect.width - width, curveRect.y + offset, width, height * 5), animData.EndValue, parameters, property.GetValuesUsed());
                    } else {
                        ParameterFieldView.DrawParameterFieldAsColor(new Rect(curveRect.x, curveRect.y + offset, width, height * 5), animData.StartValue, parameters, property.GetValuesUsed());
                        ParameterFieldView.DrawParameterFieldAsColor(new Rect(curveRect.x + curveRect.width - width, curveRect.y + offset, width, height * 5), animData.EndValue, parameters, property.GetValuesUsed());
                    }
                }
                return;
            }

            EaseManager.Ease oldEase = animData.EaseType;
            float oldAmp = animData.Amplitude;
            float oldPeriod = animData.Period;

            float easeWidth = Mathf.Min(180, curveRect.width - 15f) / 3f;
            animData.UseCustomCurve = EditorGUI.Toggle(new Rect(curveRect.x, curveRect.y, 15f, 10f), animData.UseCustomCurve);
            if (animData.UseCustomCurve) {
                animData.CustomCurve = EditorGUI.CurveField(new Rect(curveRect.x + 15f, curveRect.y, easeWidth, 15f), "", animData.CustomCurve);
            } else {
                animData.EaseType = (EaseManager.Ease) EditorGUI.EnumPopup(new Rect(curveRect.x + 15f, curveRect.y, easeWidth, 10f), animData.EaseType);
                animData.Amplitude = EditorGUI.FloatField(new Rect(curveRect.x + 15f + easeWidth, curveRect.y, easeWidth, 20f), animData.Amplitude);
                animData.Period = EditorGUI.FloatField(new Rect(curveRect.x + 15f + 2 * easeWidth, curveRect.y, easeWidth, 20f), animData.Period);
            }

            if (oldEase != animData.EaseType || oldAmp != animData.Amplitude || oldPeriod != animData.Period)
                DontUpdateOnRecord = true;


            if (property.GetDisplayMode() == BaseProperty.DisplayMode.CURVE) {
                ParameterFieldView.DrawParameterField(new Rect(curveRect.x, curveRect.y + offset, width, height * 5), animData.StartValue, parameters, property.GetValuesUsed());
                ParameterFieldView.DrawParameterField(new Rect(curveRect.x + curveRect.width - width, curveRect.y + offset, width, height * 5), animData.EndValue, parameters, property.GetValuesUsed());
            } else {
                ParameterFieldView.DrawParameterFieldAsColor(new Rect(curveRect.x, curveRect.y + offset, width, height * 5), animData.StartValue, parameters, property.GetValuesUsed());
                ParameterFieldView.DrawParameterFieldAsColor(new Rect(curveRect.x + curveRect.width - width, curveRect.y + offset, width, height * 5), animData.EndValue, parameters, property.GetValuesUsed());
            }
            /*animData.StartValue.x = EditorGUI.FloatField(new Rect(curveRect.x, curveRect.y + offset, width, height), "", animData.StartValue.x);
            animData.StartValue.y = EditorGUI.FloatField(new Rect(curveRect.x, curveRect.y + offset + height, width, height), "", animData.StartValue.y);
            animData.StartValue.z = EditorGUI.FloatField(new Rect(curveRect.x, curveRect.y + offset + 2 * height, width, height), "", animData.StartValue.z);
            animData.StartValue.w = EditorGUI.FloatField(new Rect(curveRect.x, curveRect.y + offset + 3 * height, width, height), "", animData.StartValue.w);

            animData.EndValue.x = EditorGUI.FloatField(new Rect(curveRect.x + curveRect.width - width, curveRect.y + offset, width, height), "", animData.EndValue.x);
            animData.EndValue.y = EditorGUI.FloatField(new Rect(curveRect.x + curveRect.width - width, curveRect.y + offset + height, width, height), "", animData.EndValue.y);
            animData.EndValue.z = EditorGUI.FloatField(new Rect(curveRect.x + curveRect.width - width, curveRect.y + offset + 2 * height, width, height), "", animData.EndValue.z);
            animData.EndValue.w = EditorGUI.FloatField(new Rect(curveRect.x + curveRect.width - width, curveRect.y + offset + 3 * height, width, height), "", animData.EndValue.w);
*/
        }

        public void DrawAnimationData(PropertyTweenAnimationData animData, BaseProperty property, Rect curveRect, Vector2 minMax) {
            float left = curveRect.x;
            float right = left + curveRect.width;
            float top = curveRect.y;
            float bottom = top + curveRect.height;

            Color[] colors = new Color[]{ Color.red, Color.green, Color.blue, Color.white };
            for (int i = 0; i < property.GetNumberOfValuesUsed(); i++) {
                if (!property.IsValueActive(i))
                    continue;
                DrawCurve(left, right, top, bottom, minMax, animData, i, colors[i]);
            }
        }

        /*public void DrawCurve(float left, float right, float top, float bottom, Vector2 minMax, PropertyTweenAnimationData animData, int i, Color color) {
            Vector2 startPoint = new Vector2(Mathf.Lerp(left, right, 0), Mathf.Lerp(bottom, top, Mathf.InverseLerp(minMax.x, minMax.y, animData.StartValue.Value[i])));
            Vector2 endPoint = new Vector2(Mathf.Lerp(left, right, 1), Mathf.Lerp(bottom, top, Mathf.InverseLerp(minMax.x, minMax.y, animData.EndValue.Value[i])));
            GUIUtil.DrawFatLine(startPoint, endPoint, 5f, color);
        }*/
        public void DrawCurve(float left, float right, float top, float bottom, Vector2 minMax, PropertyTweenAnimationData animData, int i, Color color) {
            Vector2 startPoint = new Vector2(Mathf.Lerp(left, right, 0), Mathf.Lerp(bottom, top, Mathf.InverseLerp(minMax.x, minMax.y, animData.StartValue.Value[i])));
            Vector2 endPoint;
            float position = 0;
            float diff = 1.0f / (float) CURVE_QUALITY;
            for (int s = 0; s < CURVE_QUALITY; s++) {
                position += diff;
                int loopIndex = (int) (position * Repeat);
                float inputPosition = (position * Repeat) - (loopIndex);
                if (Bounce && (loopIndex % 2 == 1))
                    inputPosition = 1 - inputPosition;

                if (s == (CURVE_QUALITY - 1) && Mathf.Abs(inputPosition) < 0.00001f && !Bounce)
                    inputPosition = 1f;
                float tweenValue = 0;
                if (animData.UseCustomCurve)
                    tweenValue = animData.CustomCurve.Evaluate(inputPosition);
                else
                    tweenValue = EaseManager.Evaluate(animData.EaseType, inputPosition, 1, animData.Amplitude, animData.Period);
                float value = Mathf.LerpUnclamped(animData.StartValue.Value[i], animData.EndValue.Value[i], tweenValue);
                endPoint = new Vector2(Mathf.Lerp(left, right, position), Mathf.Lerp(bottom, top, Mathf.InverseLerp(minMax.x, minMax.y, value)));
                GUIUtil.DrawFatLine(startPoint, endPoint, 5f, color);
                startPoint = endPoint;
            }
        }

        public void DrawColorAnimationData(PropertyTweenAnimationData animData, BaseProperty property, Rect curveRect, Vector2 minMax) {
            RecalculateTexture(animData);

            EditorGUI.DrawTextureTransparent(curveRect, ColorTexture);
        }

        public void RecalculateTexture(PropertyTweenAnimationData animData) {
            if (Vector4.SqrMagnitude(TexturePreviousStartValue - animData.StartValue.Value) < 0.001f &&
                Vector4.SqrMagnitude(TexturePreviousEndValue - animData.EndValue.Value) < 0.001f)
                return;


            TexturePreviousStartValue = animData.StartValue.Value;
            TexturePreviousEndValue = animData.EndValue.Value;

            Color startColor = new Color(animData.StartValue.ValueX, animData.StartValue.ValueY, animData.StartValue.ValueZ, animData.StartValue.ValueW);
            Color endColor = new Color(animData.EndValue.ValueX, animData.EndValue.ValueY, animData.EndValue.ValueZ, animData.EndValue.ValueW);

            Color[] pixels = new Color[texWidth];
            for (int s = 0; s < texWidth; s++) {
                float inputPosition = (float) s / texWidth;
                float tweenValue;
                if (animData.UseCustomCurve)
                    tweenValue = animData.CustomCurve.Evaluate(inputPosition);
                else
                    tweenValue = EaseManager.Evaluate(animData.EaseType, inputPosition, 1, animData.Amplitude, animData.Period);

                pixels[s] = Color.LerpUnclamped(startColor, endColor, tweenValue);
            }
            ColorTexture.SetPixels(pixels);
            ColorTexture.Apply();
        }

        public override bool OnHandleInput(BaseProperty property, TimelineData timelineData, SequencerSequenceView sequenceView, Rect timelineTrackRect, int highiestEventTrackIndex, object actor) {
            PropertyTweenAnimationData animData = AnimationData as PropertyTweenAnimationData;
            if (CurveRect.Contains(SequencerSequenceView.GetCurrentMousePosition())) {
                EaseManager.Ease oldEase = animData.EaseType;
                animData.EaseType = EaseManager.EaseHotkeys(animData.EaseType);
                if (oldEase != animData.EaseType)
                    DontUpdateOnRecord = true;
            }
            return false;
        }

        public override void OnUpdateWhileRecording(BaseProperty property, float absoluteTime, float duration, float normalizedTime, float multiplier, float offset, bool remap, float remap0, float remap1) {
            if (!(normalizedTime == 0 || (normalizedTime <= 1 && normalizedTime > 0.999998f)))
                return;
            if (DontUpdateOnRecord) {
                DontUpdateOnRecord = false;
                return;
            }
            if (property.Enabled == false)
                return;
            Vector4 animDataValue = AnimationData.Evaluate(absoluteTime, duration);
            bool success;
            Vector4 propertyValue = InverseRemap(property.Value(out success), remap, remap0, remap1) / multiplier - (Vector4.one * offset);
            ;
            if (!success)
                return;
            PropertyTweenAnimationData animData = AnimationData as PropertyTweenAnimationData;
            if (Vector4.Magnitude(animDataValue - propertyValue) > 0.0001f) {
                if (normalizedTime == 0f) {
                    animData.StartValue.Value = propertyValue;
                } else {
                    animData.EndValue.Value = propertyValue;
                }
            }
        }
    }

}