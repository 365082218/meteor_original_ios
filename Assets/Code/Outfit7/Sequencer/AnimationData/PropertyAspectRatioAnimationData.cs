using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Outfit7.Sequencer {
    public class PropertyAspectRatioAnimationData : PropertyAnimationData {
        public List<PropertyAnimationData> AnimationDatas = new List<PropertyAnimationData>();
        public List<float> AspectRatios = new List<float>();
        public int SelectedIndex = 0;
        //just for editor playback

        public override void SetStartingValues(Vector4 current) {
            
        }

        public override Vector4 Evaluate(float absoluteTime, float duration) {
            if (Application.isPlaying) {
                return AspectRatioAdjustedEvaluation(absoluteTime, duration);
            } else {
                return AnimationDatas[SelectedIndex].Evaluate(absoluteTime, duration);
            }
        }

        private Vector4 AspectRatioAdjustedEvaluation(float absoluteTime, float duration) {
            float aspectRatio = (float) Screen.width / (float) Screen.height;
            if (AnimationDatas.Count == 1)
                return AnimationDatas[0].Evaluate(absoluteTime, duration);

            //left index match
            int closestLeftDataIndex = -1;
            float closestLeftMatch = -1;
            for (int i = 0; i < AspectRatios.Count; i++) {
                if (AspectRatios[i] <= aspectRatio && AspectRatios[i] > closestLeftMatch) {
                    closestLeftMatch = AspectRatios[i];
                    closestLeftDataIndex = i;
                }
            }

            //right index match
            int closestRightDataIndex = -1;
            float closestRightMatch = 100;
            for (int i = 0; i < AspectRatios.Count; i++) {
                if (AspectRatios[i] >= aspectRatio && AspectRatios[i] < closestRightMatch) {
                    closestRightMatch = AspectRatios[i];
                    closestRightDataIndex = i;
                }
            }

            //exact match
            if (closestLeftDataIndex == closestRightDataIndex)
                return AnimationDatas[closestLeftDataIndex].Evaluate(absoluteTime, duration);

            //left non-existent
            if (closestLeftDataIndex == -1)
                return AnimationDatas[closestRightDataIndex].Evaluate(absoluteTime, duration);

            //right non-existent
            if (closestRightDataIndex == -1)
                return AnimationDatas[closestLeftDataIndex].Evaluate(absoluteTime, duration);

            // both found
            return Vector4.Lerp(
                AnimationDatas[closestLeftDataIndex].Evaluate(absoluteTime, duration),
                AnimationDatas[closestRightDataIndex].Evaluate(absoluteTime, duration),
                Mathf.InverseLerp(closestLeftMatch, closestRightMatch, aspectRatio));
        }


        public override Vector2 GetMinMax(BaseProperty property) {
            return AnimationDatas[SelectedIndex].GetMinMax(property);
        }
    }
}