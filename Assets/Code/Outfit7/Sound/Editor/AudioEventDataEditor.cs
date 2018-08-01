using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using UnityEngine.Audio;
using Outfit7.Util;

namespace Outfit7.Audio {

    public static class ScriptableObjectUtility {

        public static void CreateAsset<T>() where T : ScriptableObject {
            T asset = ScriptableObject.CreateInstance<T>();
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "") {
                path = "Assets";
            } else if (Path.GetExtension(path) != "") {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            //
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New " + typeof(T).ToString() + ".asset");
            AssetDatabase.CreateAsset(asset, assetPathAndName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }

    [CustomEditor(typeof(AudioEventData))]
    [InitializeOnLoad]
    public class AudioEventDataEditor : UnityEditor.Editor {

        [MenuItem("Assets/Create/AudioEventData")]
        public static void CreateAsset() {
            ScriptableObjectUtility.CreateAsset<AudioEventData>();
        }

        static AudioEventDataEditor() {
            EditorApplication.update -= OnUpdate;
            EditorApplication.update += OnUpdate;
        }

        static void OnUpdate() {
            if (AudioEventManager.Instance != null) {
                AudioEventManager.Instance.OnPreUpdate(0.016f);
                if (AudioEventTest != null) {
                    EditorUtility.SetDirty(AudioEventTest);
                }

                if (AudioEventEditor.Instance != null) {
                    AudioEventEditor.Instance.Repaint();
                }
            }
        }

        public void EditorDraw(string label, AudioMinMax amm) {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField(label);
            EditorGUI.indentLevel++;
            amm.Min = EditorGUILayout.FloatField("Min", amm.Min);
            if (amm.Min > amm.Max) {
                amm.Min = amm.Max;
            }
            amm.Max = EditorGUILayout.FloatField("Max", amm.Max);
            if (amm.Max < amm.Min) {
                amm.Max = amm.Min;
            }
            EditorGUILayout.MinMaxSlider(ref amm.Min, ref amm.Max, amm.MinLimit, amm.MaxLimit);
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }

        public void EditorDraw(AudioEventDataClip dataClip) {
            EditorGUI.indentLevel++;
            AssetReferenceEditor.Field("Audio Clip", dataClip.ClipReference, typeof(AudioClip));
            dataClip.Probability = EditorGUILayout.Slider("Probability", dataClip.Probability, 0.0f, 100.0f);
            dataClip.Volume = EditorGUILayout.Slider("Volume", dataClip.Volume, 0.0f, 1.0f);
            dataClip.Pitch = EditorGUILayout.Slider("Pitch", dataClip.Pitch, 0.0f, 5.0f);
            dataClip.BeatsPerMinute = EditorGUILayout.Slider("Beats Per Minute", dataClip.BeatsPerMinute, 0.0f, 300.0f);
            dataClip.LeadInLength = EditorGUILayout.Slider("Lead In", dataClip.LeadInLength, 0.0f, dataClip.Clip == null ? 10.0f : dataClip.Clip.length);
            dataClip.LeadOutLength = EditorGUILayout.Slider("Lead Out", dataClip.LeadOutLength, 0.0f, dataClip.Clip == null ? 10.0f : dataClip.Clip.length);
            if (dataClip.Clip != null) {
                if (dataClip.StartPoints.Count == 0) {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Start Points");
                    if (GUILayout.Button("Add Start Point", GUILayout.Width(150))) {
                        dataClip.StartPoints.Add(0.0f);
                    }
                    EditorGUILayout.EndHorizontal();
                } else {
                    EditorGUILayout.LabelField("Start Points");
                    EditorGUILayout.BeginVertical();
                    EditorGUI.indentLevel++;
                    for (int a = 0; a < dataClip.StartPoints.Count; a++) {
                        EditorGUILayout.BeginHorizontal();
                        dataClip.StartPoints[a] = EditorGUILayout.Slider(a.ToString(), dataClip.StartPoints[a], 0.0f, dataClip.Clip.length);
                        if (GUILayout.Button("+", GUILayout.Width(25))) {
                            dataClip.StartPoints.Insert(a + 1, 0.0f);
                        }
                        if (GUILayout.Button("-", GUILayout.Width(25))) {
                            dataClip.StartPoints.RemoveAt(a);
                            a--;
                            continue;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUI.indentLevel--;
        }

        public void EditorDraw(AudioSourceData sourceData) {
            EditorGUI.indentLevel++;
            sourceData.MixerGroup = (AudioMixerGroup) EditorGUILayout.ObjectField("Audio Mixer Group", sourceData.MixerGroup, typeof(AudioMixerGroup), false);
            sourceData.bypassEffects = EditorGUILayout.Toggle("bypassEffects", sourceData.bypassEffects);
            sourceData.bypassListenerEffects = EditorGUILayout.Toggle("bypassListenerEffects", sourceData.bypassListenerEffects);
            sourceData.bypassReverbZones = EditorGUILayout.Toggle("bypassReverbZones", sourceData.bypassReverbZones);
            sourceData.dopplerLevel = EditorGUILayout.Slider("dopplerLevel", sourceData.dopplerLevel, 0.0f, 5.0f);
            sourceData.panStereo = EditorGUILayout.Slider("panStereo", sourceData.panStereo, -1.0f, 1.0f);
            sourceData.spatialBlend = EditorGUILayout.Slider("spatialBlend", sourceData.spatialBlend, 0.0f, 1.0f);
            sourceData.spread = EditorGUILayout.Slider("spread", sourceData.spread, 0.0f, 360.0f);
            sourceData.reverbZoneMix = EditorGUILayout.Slider("reverbZoneMix", sourceData.reverbZoneMix, 0.0f, 1.0f);

            EditorGUILayout.LabelField("Attenuation");
            EditorGUI.indentLevel++;
            sourceData.minDistance = EditorGUILayout.FloatField("minDistance", sourceData.minDistance);
            if (sourceData.minDistance > sourceData.maxDistance) {
                sourceData.minDistance = sourceData.maxDistance;
            }
            sourceData.maxDistance = EditorGUILayout.FloatField("maxDistance", sourceData.maxDistance);
            if (sourceData.maxDistance < sourceData.minDistance) {
                sourceData.maxDistance = sourceData.minDistance;
            }
            sourceData.rolloffMode = (AudioRolloffMode) EditorGUILayout.EnumPopup(sourceData.rolloffMode == AudioRolloffMode.Custom ? "rolloffMode - WARNING CALL DEVS" : "rolloffMode", sourceData.rolloffMode);
            if (sourceData.rolloffMode == AudioRolloffMode.Custom) {
                sourceData.RolloffCurve = EditorGUILayout.CurveField("Rolloff", sourceData.RolloffCurve);
                sourceData.ReverbZoneMixCurve = EditorGUILayout.CurveField("Reverb Zone Mix", sourceData.ReverbZoneMixCurve);
                sourceData.SpatialBlendCurve = EditorGUILayout.CurveField("Spatial Blend", sourceData.SpatialBlendCurve);
                sourceData.SpreadCurve = EditorGUILayout.CurveField("Spread", sourceData.SpreadCurve);
            }
            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
        }


        public override void OnInspectorGUI() {
            AudioEventData myTarget = (AudioEventData) target;

            // play & loop mode
            myTarget.PlayMode = (EPlayMode) EditorGUILayout.EnumPopup("Play Mode", myTarget.PlayMode);

            if (myTarget.PlayMode == EPlayMode.Custom) {
                EditorGUILayout.LabelField("Custom");
                if (myTarget.CustomPlayModeList.Count == 0) {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Add Custom Entry", GUILayout.Width(150))) {
                        myTarget.CustomPlayModeList.Add(new CustomPlayModeEntry(EPlayMode.Sequential, 1));
                    }
                    EditorGUILayout.EndHorizontal();
                } else {
                    EditorGUILayout.BeginVertical();
                    EditorGUI.indentLevel++;
                    for (int a = 0; a < myTarget.CustomPlayModeList.Count; a++) {
                        EditorGUILayout.BeginHorizontal();
                        CustomPlayModeEntry pme = myTarget.CustomPlayModeList[a];
                        pme.PlayMode = (EPlayMode) EditorGUILayout.EnumPopup(pme.PlayMode);
                        if (pme.PlayMode == EPlayMode.Custom) {
                            pme.PlayMode = EPlayMode.UserDefined;
                        }
                        pme.Value = EditorGUILayout.IntField(pme.Value);
                        if (GUILayout.Button("+", GUILayout.Width(25))) {
                            myTarget.CustomPlayModeList.Insert(a + 1, new CustomPlayModeEntry(EPlayMode.Sequential, 1));
                        }
                        if (GUILayout.Button("-", GUILayout.Width(25))) {
                            myTarget.CustomPlayModeList.RemoveAt(a);
                            a--;
                            continue;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;
                }
            }

            myTarget.LoopMode = (ELoopMode) EditorGUILayout.EnumPopup("Loop Mode", myTarget.LoopMode);
            // priority & instancing
            myTarget.InstanceLimit = EditorGUILayout.IntSlider("Instance Limit (0-unlimited)", myTarget.InstanceLimit, 0, 16);
            myTarget.Priority = (EPriority) EditorGUILayout.EnumPopup("Priority", myTarget.Priority);
            myTarget.OverlappingLimitRelativeLength = EditorGUILayout.Slider("Overlapping Limit", myTarget.OverlappingLimitRelativeLength, 0.0f, 1.0f);
            // panning
            myTarget.Use2DCameraPanning = EditorGUILayout.Toggle("Use 2D Camera Panning", myTarget.Use2DCameraPanning);
            // volume
            EditorDraw("Volume", myTarget.Volume);
            // pitch
            EditorDraw("Pitch", myTarget.Pitch);
            // fade
            myTarget.ShowFade = EditorGUILayout.Foldout(myTarget.ShowFade, "Fade");
            if (myTarget.ShowFade) {
                EditorGUI.indentLevel++;
                // fade in
                EditorGUILayout.LabelField("In");
                EditorGUI.indentLevel++;
                // time
                EditorDraw("Time", myTarget.FadeInOut.FadeInTime);
                // curve
                myTarget.FadeInOut.FadeInCurve = EditorGUILayout.CurveField("Curve", myTarget.FadeInOut.FadeInCurve);
                EditorGUI.indentLevel--;
                // fade out
                EditorGUILayout.LabelField("Out");
                EditorGUI.indentLevel++;
                // time
                EditorDraw("Time", myTarget.FadeInOut.FadeOutTime);
                // curve
                myTarget.FadeInOut.FadeOutCurve = EditorGUILayout.CurveField("Curve", myTarget.FadeInOut.FadeOutCurve);
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
            }
            // fade
            if (myTarget.LoopMode == ELoopMode.LoopCycle) {
                myTarget.ShowCrossFade = EditorGUILayout.Foldout(myTarget.ShowCrossFade, "CrossFade");
                if (myTarget.ShowCrossFade) {
                    EditorGUI.indentLevel++;
                    // fade in
                    EditorGUILayout.LabelField("In");
                    EditorGUI.indentLevel++;
                    // curve
                    myTarget.CrossFade.FadeInCurve = EditorGUILayout.CurveField("Curve", myTarget.CrossFade.FadeInCurve);
                    EditorGUI.indentLevel--;
                    // fade out
                    EditorGUILayout.LabelField("Out");
                    EditorGUI.indentLevel++;
                    // curve
                    myTarget.CrossFade.FadeOutCurve = EditorGUILayout.CurveField("Curve", myTarget.CrossFade.FadeOutCurve);
                    EditorGUI.indentLevel--;
                    EditorGUI.indentLevel--;
                }
            }
            //
            myTarget.ShowSource = EditorGUILayout.Foldout(myTarget.ShowSource, "Source");
            if (myTarget.ShowSource) {
                EditorDraw(myTarget.SourceData);
            }
            //
            myTarget.ShowAudioDefinitions = EditorGUILayout.Foldout(myTarget.ShowAudioDefinitions, "Audio Definitions");
            //
            if (myTarget.ShowAudioDefinitions) {
                if (myTarget.AudioDefinitions.Count == 0) {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Definitions");
                    if (GUILayout.Button("Add Clip Definition", GUILayout.Width(150))) {
                        myTarget.AudioDefinitions.Add(new AudioEventDataClip());
                    }
                    EditorGUILayout.EndHorizontal();
                } else {
                    for (int a = 0; a < myTarget.AudioDefinitions.Count; a++) {
                        AudioEventDataClip dataClip = myTarget.AudioDefinitions[a];
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.BeginVertical();
                        EditorDraw(dataClip);
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.BeginVertical();
                        if (GUILayout.Button("+", GUILayout.Width(25))) {
                            myTarget.AudioDefinitions.Insert(a + 1, new AudioEventDataClip());
                        }
                        if (GUILayout.Button("-", GUILayout.Width(25))) {
                            myTarget.AudioDefinitions.RemoveAt(a);
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                        EditorGUILayout.Space();
                    }
                }
            }
            myTarget.Attributes = (EAudioEventDataAttributes) EditorGUILayout.EnumMaskField("Attributes", myTarget.Attributes);
            EditorUtility.SetDirty(myTarget);

            if (AudioEventTest == null) {
                AudioEventTest = AudioEventManager.CreateAudioEventForEditor();
            }

            EditorGUILayout.Space();

            if (AudioEventTest.IsPlaying) {
                if (GUILayout.Button("Stop Hard")) {
                    AudioEventTest.Stop(true);
                }
                if (GUILayout.Button("Stop Soft")) {
                    AudioEventTest.Stop(false);
                }
            } else {
                if (GUILayout.Button("Play")) {
                    AudioEventTest.ResetForPool();
                    AudioEventTest.Initialize(myTarget);
                    AudioEventTest.Play();
                    //AudioEventTest.PlayFullArg(true, 0, null, 5.0f);
                }
            }
        }

        private static AudioEvent AudioEventTest = null;
    }
}