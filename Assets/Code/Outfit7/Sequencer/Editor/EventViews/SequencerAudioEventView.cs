using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Outfit7.Logic.StateMachineInternal;
using Outfit7.Audio;
using Outfit7.Logic.Util;

namespace Outfit7.Sequencer {
    [SequencerQuickSearchAttribute("Sound")]
    [SequencerQuickSearchDisplayAttribute("Audio")]
    [SequencerAudioTrackAttribute("Audio Event")]
    public class SequencerAudioEventView : SequencerContinuousEventView {
        private SequencerAudioEvent Event = null;
        private Texture2D WaveTexture;
        private Color[] Blank;
        private float[] Samples;
        public static float CURVE_QUALITY = 25;
        public bool RedrawTexture = true;

        public override void OnInit(object evnt, object parent) {
            Event = evnt as SequencerAudioEvent;
            base.OnInit(evnt, parent);
        }

        public override string GetName() {
            if (Event.AudioEventData != null) {
                return Event.AudioEventData.name;
            } else {
                return "No Audio";
            }
        }

        public override void OnDrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
        }

        public override void OnDrawExtendedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            base.OnDrawExtendedGui(windowSize, startHeight, splitViewLeft, timelineData, foldoutData, parameters);
            DrawGraphics(new Rect(EventRect.x, EventRect.y + 35f, EventRect.width, EventRect.height - 50f), timelineData);
            AudioEventData oldAudioEventData = Event.AudioEventData;
            Event.AudioEventData = (AudioEventData) EditorGUI.ObjectField(new Rect(EventRect.x, EventRect.y + 20f, EventRect.width, 15f), "", Event.AudioEventData, typeof(AudioEventData), false);
            if (oldAudioEventData != Event.AudioEventData) {
                OnAudioEventDataChanged();
            }
            EditorGUI.LabelField(new Rect(EventRect.x, EventRect.yMax - 15f, 50f, 15f), "Volume:");
            Event.VolumeCurve = EditorGUI.CurveField(new Rect(EventRect.x + 50f, EventRect.yMax - 15f, 50f, 15f), Event.VolumeCurve);
            EditorGUI.LabelField(new Rect(EventRect.xMax - 100f, EventRect.yMax - 15f, 50f, 15f), "Pitch:");
            Event.PitchCurve = EditorGUI.CurveField(new Rect(EventRect.xMax - 50f, EventRect.yMax - 15f, 50f, 15f), Event.PitchCurve);
        }

        private void OnAudioEventDataChanged() {
            if (Event.AudioEventData == null) {
                RedrawTexture = true;
                return;
            }
            
            if (Event.AudioEventData.AudioDefinitions.Count == 0)
                return;

            float longestClip = 0;
            foreach (AudioEventDataClip dataClip in Event.AudioEventData.AudioDefinitions) {
                if (dataClip.Clip.length > longestClip)
                    longestClip = dataClip.Clip.length;
            }
            RedrawTexture = true;
            Event.Duration = longestClip;
        }

        void GetCurWave(Rect curveRect) {
            AudioClip clip = Event.AudioEventData.AudioDefinitions[0].ClipReference.Load(typeof(AudioClip)) as AudioClip;
            /*AudioClip clipDecompressed = 
            // clear the texture
                curveRect.width = 512;
            ClearTexture(curveRect);
            // get samples from channel 0 (left)
            int size = clip.samples * clip.channels;
            Samples = new float[size];
            clip.GetData(Samples, 0);
            // draw the waveform
            for (int i = 0; i < size; i++) {
                WaveTexture.SetPixel((int) (curveRect.width * i / size), (int) (curveRect.height * (Samples[i] + 1f) / 2), Color.yellow);
            }
            // upload to the graphics card
            WaveTexture.Apply();*/
            WaveTexture = AssetPreview.GetAssetPreview(clip);
        }

        private void ClearTexture(Rect curveRect) {
            WaveTexture = new Texture2D((int) curveRect.width, (int) curveRect.height);
            Blank = new Color[(int) curveRect.width * (int) curveRect.height];
            WaveTexture.SetPixels(Blank);
            RedrawTexture = false;
        }

        public void DrawGraphics(Rect curveRect, TimelineData timelineData) {
            if (RedrawTexture) {
                if (Event.AudioEventData != null) {
                    if (Event.AudioEventData.AudioDefinitions.Count > 0) {
                        if (Event.AudioEventData.AudioDefinitions[0].Clip != null)
                            GetCurWave(curveRect); //TODO only on refresh
                    }
                } else {
                    ClearTexture(curveRect);
                    WaveTexture.Apply();
                }
            }
            float textureWidth = 0f;
            if (Event.AudioEventData != null) {
                if (Event.AudioEventData.AudioDefinitions.Count > 0) {
                    if (Event.AudioEventData.AudioDefinitions[0].Clip != null)
                        textureWidth = Event.AudioEventData.AudioDefinitions[0].Clip.length * timelineData.LenghtOfASecond;
                }
            }

            GUI.DrawTexture(new Rect(curveRect.x, curveRect.y, textureWidth, curveRect.height), WaveTexture);
            Vector2 minMaxVolume = new Vector2(0, 1);
            Vector2 minMaxPitch = new Vector2(0, 2);
            float left = curveRect.x;
            float right = left + curveRect.width;
            float top = curveRect.y;
            float bottom = top + curveRect.height;
            DrawVolumeFade(left, right, top, bottom, minMaxVolume, Event.VolumeCurve, new Color(0, 0, 0, 0.5f));
            DrawCurve(left, right, top, bottom, minMaxPitch, Event.PitchCurve, Color.blue);
        }

        public void DrawVolumeFade(float left, float right, float top, float bottom, Vector2 minMax, AnimationCurve curve, Color color) {
            Vector2 startPoint = new Vector2(Mathf.Lerp(left, right, 0), Mathf.Lerp(bottom, top, Mathf.InverseLerp(minMax.x, minMax.y, curve.Evaluate(0))));
            Vector2 endPoint;
            float position = 0;
            float diff = 1.0f / (float) CURVE_QUALITY;
            for (int s = 0; s < CURVE_QUALITY; s++) {
                position += diff;
                endPoint = new Vector2(Mathf.Lerp(left, right, position), Mathf.Lerp(bottom, top, Mathf.InverseLerp(minMax.x, minMax.y, curve.Evaluate(position))));
                GUIUtil.DrawQuads(new Vector2[] {
                    endPoint,
                    startPoint,
                    new Vector2(startPoint.x, top),
                    new Vector2(endPoint.x, top)
                }, color);
                startPoint = endPoint;
            }
        }

        public void DrawCurve(float left, float right, float top, float bottom, Vector2 minMax, AnimationCurve curve, Color color) {
            Vector2 startPoint = new Vector2(Mathf.Lerp(left, right, 0), Mathf.Lerp(bottom, top, Mathf.InverseLerp(minMax.x, minMax.y, curve.Evaluate(0))));
            Vector2 endPoint;
            float position = 0;
            float diff = 1.0f / (float) CURVE_QUALITY;
            for (int s = 0; s < CURVE_QUALITY; s++) {
                position += diff;
                endPoint = new Vector2(Mathf.Lerp(left, right, position), Mathf.Lerp(bottom, top, Mathf.InverseLerp(minMax.x, minMax.y, curve.Evaluate(position))));
                GUIUtil.DrawFatLine(startPoint, endPoint, 5f, color);
                startPoint = endPoint;
            }
        }


        public override void OnRecordingStart() {
        }

        public override void OnRecordingStop() {
            AudioEvent ae = Event.GetAudioEvent();
            if (ae != null)
                ae.Stop(true);
        }
    }
}

