using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;
using Outfit7.Util;
using Outfit7.Logic.StateMachineInternal;
using Outfit7.Logic.Util;
using System.Linq;

namespace Outfit7.Sequencer {
    public class PropertyCurveAnimationDataView : PropertyAnimationDataView {
        protected enum DopeSheetInputState {
            INACTIVE,
            DRAGGING
        }

        PropertyCurveAnimationData AnimationData = null;
        //private GUIStyle DotStyle = (GUIStyle) "U2D.dragDot";
        public static float CURVE_QUALITY = 25;
        private Rect CurveRect;
        private bool DontUpdateOnRecord = false;

        //DOPE SHEET EDIT
        private bool OptimizedView = false;
        DopeSheetInputState InputState = DopeSheetInputState.INACTIVE;
        private Vector2 StartDragMousePosition = new Vector2();
        private Vector2 StartDragOffset = new Vector2();
        private int DraggingCurveIndex = -1;
        private int DraggingKeyIndex = -1;

        public override void OnInit(object animData) {
            AnimationData = animData as PropertyCurveAnimationData;
            base.OnInit(animData);
        }

        public override string Name() {
            return "Curve Animation Data";
        }

        public override void OnContextMenu(GenericMenu menu) {
            menu.AddItem(new GUIContent("Sync values/X to All"), false, Sync, 0);
            menu.AddItem(new GUIContent("Sync values/Y to All"), false, Sync, 1);
            menu.AddItem(new GUIContent("Sync values/Z to All"), false, Sync, 2);
            menu.AddItem(new GUIContent("Sync values/W to All"), false, Sync, 3);
        }

        private void Sync(object val) {
            int i = (int) val;
            AnimationData.Curve[0] = new AnimationCurve(AnimationData.Curve[i].keys);
            AnimationData.Curve[1] = new AnimationCurve(AnimationData.Curve[i].keys);
            AnimationData.Curve[2] = new AnimationCurve(AnimationData.Curve[i].keys);
            AnimationData.Curve[3] = new AnimationCurve(AnimationData.Curve[i].keys);
        }


        public override void OnDrawGui(BaseProperty property, Rect curveRect, List<Parameter> parameters, bool optimizedView, bool hover) {
            CurveRect = curveRect;
            PropertyCurveAnimationData animData = AnimationData as PropertyCurveAnimationData;
            Vector2 minMax = AnimationData.GetMinMax(property);
            DrawAnimationData(AnimationData, property, curveRect, minMax, optimizedView);
            float width = 50f;
            float height = 15f;
            float offset = 20f;
            //
            OptimizedView = optimizedView;

            if (optimizedView) {
                if (hover) {
                    for (int i = 0; i < property.GetNumberOfValuesUsed(); i++) {
                        if (!property.IsValueActive(i))
                            continue;
                        AnimationData.Curve[i] = EditorGUI.CurveField(new Rect(curveRect.x, curveRect.y + offset + i * height, Mathf.Min(width, curveRect.width), height), "", AnimationData.Curve[i]);
                    }
                }
                return;
            }
            EaseManager.Ease oldEase = animData.EaseType;
            animData.EaseType = (EaseManager.Ease) EditorGUI.EnumPopup(new Rect(curveRect.x, curveRect.y, Mathf.Min(60f, curveRect.width), 10f), animData.EaseType);
            if (oldEase != animData.EaseType)
                DontUpdateOnRecord = true;
            

            DrawGizmoLines(property);

            for (int i = 0; i < property.GetNumberOfValuesUsed(); i++) {
                if (!property.IsValueActive(i))
                    continue;
                AnimationData.Curve[i] = EditorGUI.CurveField(new Rect(curveRect.x, curveRect.y + offset + i * height, Mathf.Min(width, curveRect.width), height), "", AnimationData.Curve[i]);
            }
        }

        public void DrawGizmoLines(BaseProperty property) { 
            if (property is LocalPositionProperty) {
                if (property.Components.Value.Count != 1)
                    return;
                Transform actorTransform = property.Components.Value[0] as Transform;
                if (actorTransform == null)
                    return;
                Vector3 parentPosition = actorTransform.parent == null ? Vector3.zero : actorTransform.parent.position;

                Vector3 startPoint = parentPosition + (Vector3) AnimationData.Evaluate(0, 1);
                Vector3 endPoint;
                float position = 0;
                float diff = 1.0f / (float) CURVE_QUALITY;
                for (int s = 0; s < CURVE_QUALITY; s++) {
                    position += diff;
                    endPoint = (Vector3) AnimationData.Evaluate(position, 1);
                    Debug.DrawLine(startPoint, endPoint, Color.white, 0);
                    startPoint = endPoint;
                }
            }
        }

        public void DrawAnimationData(PropertyCurveAnimationData animData, BaseProperty property, Rect curveRect, Vector2 minMax, bool optimizedView) {
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
            int numValues = property.GetNumberOfValuesUsed();
            if (optimizedView)
                return;
            for (int i = 0; i < numValues; i++) {
                if (!property.IsValueActive(i))
                    continue;
                DrawDope(left, right, top, bottom, animData, i, colors[i], numValues);
            }

        }

        public void DrawCurve(float left, float right, float top, float bottom, Vector2 minMax, PropertyCurveAnimationData animData, int i, Color color) {
            Vector2 startPoint = new Vector2(Mathf.Lerp(left, right, 0), Mathf.Lerp(bottom, top, Mathf.InverseLerp(minMax.x, minMax.y, animData.Curve[i].Evaluate(0))));
            Vector2 endPoint;
            float position = 0;
            float diff = 1.0f / (float) CURVE_QUALITY;
            for (int s = 0; s < CURVE_QUALITY; s++) {
                position += diff;
                endPoint = new Vector2(Mathf.Lerp(left, right, position), Mathf.Lerp(bottom, top, Mathf.InverseLerp(minMax.x, minMax.y, animData.Curve[i].Evaluate(position))));
                GUIUtil.DrawFatLine(startPoint, endPoint, 5f, color);
                startPoint = endPoint;
            }
        }

        public void DrawDope(float left, float right, float top, float bottom, PropertyCurveAnimationData animData, int i, Color c, int numValues) {
            AnimationCurve curve = animData.Curve[i];
            foreach (Keyframe key in curve.keys) {
                //EditorGUI.LabelField(new Rect(Mathf.Lerp(left, right, key.time) - 5f, bottom - 10f, 10f, 10f), "", DotStyle);
                EditorGUI.DrawRect(new Rect(Mathf.Lerp(left, right, key.time) - 3f, bottom - 8 * (numValues - i) + 2f, 6f, 6f), c);
            }
        }

        public override bool OnHandleInput(BaseProperty property, TimelineData timelineData, SequencerSequenceView sequenceView, Rect timelineTrackRect, int highiestEventTrackIndex, object actor) {
            //ease change
            PropertyCurveAnimationData animData = AnimationData as PropertyCurveAnimationData;
            if (CurveRect.Contains(SequencerSequenceView.GetCurrentMousePosition())) {
                EaseManager.Ease oldEase = animData.EaseType;
                animData.EaseType = EaseManager.EaseHotkeys(animData.EaseType);
                if (oldEase != animData.EaseType)
                    DontUpdateOnRecord = true;
            }

            float left = CurveRect.x;
            float right = left + CurveRect.width;
            float top = CurveRect.y;
            float bottom = top + CurveRect.height;

            if (OptimizedView)
                return false;

            if (InputState == DopeSheetInputState.INACTIVE) {
                int numValues = property.GetNumberOfValuesUsed();
                for (int curveIndex = 0; curveIndex < numValues; curveIndex++) {
                    int keyIndex = 0;
                    foreach (Keyframe key in AnimationData.Curve[curveIndex].keys) {
                        Rect keyRect = new Rect(Mathf.Lerp(left, right, key.time) - 5f, bottom - 8f * (numValues - curveIndex), 10f, 10f);
                        if (UnityEngine.Event.current.type == EventType.mouseDown) {
                            if (keyRect.Contains(SequencerSequenceView.GetCurrentMousePosition())) {
                                InputState = DopeSheetInputState.DRAGGING;
                                StartDragMousePosition = SequencerSequenceView.GetCurrentMousePosition();
                                StartDragOffset = StartDragMousePosition - keyRect.min;
                                DraggingCurveIndex = curveIndex;
                                DraggingKeyIndex = keyIndex;
                                return true;
                            }
                        }
                        keyIndex++;
                    }
                }
            } else if (InputState == DopeSheetInputState.DRAGGING) {
                if (UnityEngine.Event.current.type == EventType.mouseUp) {
                    InputState = DopeSheetInputState.INACTIVE;
                    DraggingCurveIndex = -1;
                    DraggingKeyIndex = -1;
                    return true;
                }
                if (UnityEngine.Event.current.type == EventType.mouseDrag) {
                    Vector2 mousePos = SequencerSequenceView.GetCurrentMousePosition() - StartDragOffset;
                    float keyValue = AnimationData.Curve[DraggingCurveIndex].keys[DraggingKeyIndex].value;
                    float keyTime = Mathf.Clamp01(Mathf.InverseLerp(left, right, mousePos.x));
                    AnimationData.Curve[DraggingCurveIndex].MoveKey(DraggingKeyIndex, new Keyframe(keyTime, keyValue));
                    //Event.StartTime = Snap(Mathf.Max(0, timelineData.GetTimeAtMousePosition(mousePos)));
                    return true;
                }
            }
            return false;
        }

        public override void OnUpdateWhileRecording(BaseProperty property, float absoluteTime, float duration, float normalizedTime, float multiplier, float offset, bool remap, float remap0, float remap1) {
            if (property.Enabled == false)
                return;
            if (DontUpdateOnRecord) {
                DontUpdateOnRecord = false;
                return;
            }
            Vector4 animDataValue = AnimationData.Evaluate(absoluteTime, duration);
            bool success;
            Vector4 propertyValue = InverseRemap(property.Value(out success), remap, remap0, remap1) / multiplier - (Vector4.one * offset);
            if (!success)
                return;
            for (int i = 0; i < 4; i++) {
                if (property.IsValueActive(i) && Mathf.Abs(animDataValue[i] - propertyValue[i]) > 0.0001f) {
                    int keyIndex = -3;
                    for (int key = 0; key < AnimationData.Curve[i].keys.Length; key++) {
                        if (Mathf.Abs(AnimationData.Curve[i].keys[key].time - normalizedTime) < 0.05f) {
                            keyIndex = key;
                            break;
                        }
                    }
                    if (keyIndex < 0) { //UNDER 9000!
                        Keyframe editKey = new Keyframe();
                        editKey.time = normalizedTime;
                        editKey.value = propertyValue[i];
                        AnimationData.Curve[i].AddKey(editKey);
                    } else {
                        AnimationData.Curve[i].MoveKey(keyIndex, new Keyframe(normalizedTime, propertyValue[i]));
                    }
                }
            }
        }
    }

}