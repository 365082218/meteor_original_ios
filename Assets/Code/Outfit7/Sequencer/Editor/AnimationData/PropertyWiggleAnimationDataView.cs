using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;
using Outfit7.Util;
using Outfit7.Logic.StateMachineInternal;
using Outfit7.Logic.Util;

namespace Outfit7.Sequencer {
    public class PropertyWiggleAnimationDataView : PropertyAnimationDataView {
        PropertyAnimationData AnimationData = null;

        public override void OnInit(object animData) {
            AnimationData = animData as PropertyAnimationData;
            base.OnInit(animData);
        }

        public override string Name() {
            return "Wiggle Animation Data";
        }

        public override void OnDrawGui(BaseProperty property, Rect curveRect, List<Parameter> parameters, bool optimizedView, bool hover) {
            //Vector2 minMax = AnimationData.GetMinMax();

            PropertyWiggleAnimationData animData = AnimationData as PropertyWiggleAnimationData;
            //DrawAnimationData(animData, curveRect, minMax);
            float width = 50f;
            float height = 15f;
            float offset = 20f;

            float easeWidth = Mathf.Min(180, curveRect.width - 15f) / 3f;
            animData.CustomCurve = EditorGUI.CurveField(new Rect(curveRect.x, curveRect.y, easeWidth, 15f), "", animData.CustomCurve);

            EditorGUI.LabelField(new Rect(curveRect.center.x - 35f, curveRect.y, 70f, 15f), "Frequency");
            ParameterFieldView.DrawParameterField(new Rect(curveRect.center.x - 30f, curveRect.y + 15f, 60f, 30f), animData.Frequency, parameters);

            ParameterFieldView.DrawParameterField(new Rect(curveRect.x, curveRect.y + offset, width, height * 5), animData.StartValue, parameters);
            ParameterFieldView.DrawParameterField(new Rect(curveRect.x + curveRect.width - width, curveRect.y + offset, width, height * 5), animData.EndValue, parameters);
        }

        /*public void DrawAnimationData(PropertyTweenAnimationData animData, Rect curveRect, Vector2 minMax) {
            float left = curveRect.x;
            float right = left + curveRect.width;
            float top = curveRect.y;
            float bottom = top + curveRect.height;
            DrawCurve(left, right, top, bottom, minMax, animData, 0, Color.red);
            DrawCurve(left, right, top, bottom, minMax, animData, 1, Color.blue);
            DrawCurve(left, right, top, bottom, minMax, animData, 2, Color.green);
            DrawCurve(left, right, top, bottom, minMax, animData, 3, Color.white);
        }*/

        /*public void DrawCurve(float left, float right, float top, float bottom, Vector2 minMax, PropertyTweenAnimationData animData, int i, Color color) {
            Vector2 startPoint = new Vector2(Mathf.Lerp(left, right, 0), Mathf.Lerp(bottom, top, Mathf.InverseLerp(minMax.x, minMax.y, animData.StartValue.Value[i])));
            Vector2 endPoint = new Vector2(Mathf.Lerp(left, right, 1), Mathf.Lerp(bottom, top, Mathf.InverseLerp(minMax.x, minMax.y, animData.EndValue.Value[i])));
            GUIUtil.DrawFatLine(startPoint, endPoint, 5f, color);
        }*/

        /*public override void OnUpdateWhileRecording(float absoluteTime, float duration, float normalizedTime) {
            Vector4 animDataValue = AnimationData.Evaluate(absoluteTime, duration);
            Vector4 propertyValue = AnimationData.Property.Value();
            PropertyTweenAnimationData animData = AnimationData as PropertyTweenAnimationData;
            if (Vector4.Magnitude(animDataValue - propertyValue) > 0.0001f) {
                if (normalizedTime > 0.5) {
                    animData.EndValue.Value = propertyValue;
                } else {
                    animData.StartValue.Value = propertyValue;
                }
            }
        }*/
    }

}