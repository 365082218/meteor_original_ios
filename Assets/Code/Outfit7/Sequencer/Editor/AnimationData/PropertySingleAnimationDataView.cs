using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;
using Outfit7.Util;
using Outfit7.Logic.StateMachineInternal;
using Outfit7.Logic.Util;

namespace Outfit7.Sequencer {
    public class PropertySingleAnimationDataView : PropertyAnimationDataView {
        PropertySingleAnimationData AnimationData = null;

        public override void OnInit(object animData) {
            AnimationData = animData as PropertySingleAnimationData;
            base.OnInit(animData);
        }

        public override string Name() {
            return "Set";
        }

        public override Vector4 GetSingleValue() {
            if (AnimationData != null)
                return AnimationData.Value.Value;
            return Vector4.one;
        }

        public override void OnDrawGui(BaseProperty property, Rect curveRect, List<Parameter> parameters, bool optimizedView, bool hover) {
            if (optimizedView) {
                if (hover) {
                    if (property.GetDisplayMode() == BaseProperty.DisplayMode.CURVE) {
                        ParameterFieldView.DrawParameterField(curveRect, AnimationData.Value, parameters, property.GetValuesUsed());
                    } else {
                        ParameterFieldView.DrawParameterFieldAsColor(curveRect, AnimationData.Value, parameters, property.GetValuesUsed());
                    }
                }
                return;
            }

            if (property.GetDisplayMode() == BaseProperty.DisplayMode.CURVE) {
                ParameterFieldView.DrawParameterField(curveRect, AnimationData.Value, parameters, property.GetValuesUsed());
            } else {
                ParameterFieldView.DrawParameterFieldAsColor(curveRect, AnimationData.Value, parameters, property.GetValuesUsed());
            }
        }

        public override void OnUpdateWhileRecording(BaseProperty property, float absoluteTime, float duration, float normalizedTime, float multiplier, float offset, bool remap, float remap0, float remap1) {
            if (property.Enabled == false)
                return;

            PropertySingleAnimationData animData = AnimationData as PropertySingleAnimationData;
            Vector4 animDataValue = AnimationData.Evaluate(absoluteTime, duration);
            bool success;
            Vector4 propertyValue = InverseRemap(property.Value(out success), remap, remap0, remap1) / multiplier - (Vector4.one * offset);
            if (!success)
                return;
            for (int i = 0; i < 4; i++) {
                if (property.IsValueActive(i) && Mathf.Abs(animDataValue[i] - propertyValue[i]) > 0.0001f) {
                    animData.Value.Value = propertyValue;
                }
            }

        }
    }

}