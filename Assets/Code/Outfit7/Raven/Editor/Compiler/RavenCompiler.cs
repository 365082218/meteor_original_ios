using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Starlite.Raven.Compiler {

    public static class RavenCompiler {
        private const string c_OutputDir = "Assets/Scripts/Compiled";
        private const string c_OutputFile = "PropertyReflectionCompiledOutput_.cs";

        private static bool s_IsCompiling = false;
        private static List<CallbackFunction> s_Callbacks = new List<CallbackFunction>();

        [MenuItem("Raven/Compile Scene")]
        public static void CompileScene() {
            Compile(false, false, true, null);
        }

        [MenuItem("Raven/Compile Scene and Reserialize Properties")]
        public static void CompileSceneAndSerialize() {
            Compile(false, true, true, null);
        }

        [MenuItem("Raven/Compile All")]
        public static void CompileAll() {
            Compile(false, false, false, null);
        }

        [MenuItem("Raven/Compile All and Reserialize Properties")]
        public static void CompileAllAndSerialize() {
            Compile(false, true, false, null);
        }

        [MenuItem("Raven/Compile First Time (Clean)")]
        public static void FirstTimeCompile() {
            Compile(true, true, false, null);
        }

        public static void CompileProperty(RavenPropertyComponent property) {
            Compile(false, false, false, property);
        }

        private static bool Compile(bool firstTimeCompilation, bool reserializeProperties, bool sceneUpdate, RavenPropertyComponent property) {
            CallbackFunction callback;
            if (!CanCompile()) {
                callback = FindCallback(firstTimeCompilation, reserializeProperties, sceneUpdate, property);
                if (callback == null) {
                    callback = new CallbackFunction(firstTimeCompilation, reserializeProperties, sceneUpdate, property);
                    s_Callbacks.Add(callback);
                }

                EditorApplication.update -= callback.m_Function;
                EditorApplication.update += callback.m_Function;
                return false;
            }

            callback = FindCallback(firstTimeCompilation, reserializeProperties, sceneUpdate, property);
            if (callback != null) {
                EditorApplication.update -= callback.m_Function;
            }
            s_Callbacks.Remove(callback);

            if (!firstTimeCompilation) {
                if (!sceneUpdate && property == null) {
                    if (!EditorUtility.DisplayDialog("Raven Compiler", "Are you sure you would like to recompile Raven Properties? This will save your current scene and go through the project and update every property in it.", "Yes", "No")) {
                        return false;
                    }
                }
            } else {
                EditorUtility.DisplayDialog("property Compiler", "Compiling Properties for the first time. This might take a while.", "Ok");
            }

            s_IsCompiling = true;
            EditorUtility.DisplayProgressBar("property Compiler", "Compiling Properties...", 0f);
            RavenCodeGenerator codeGenerator = new RavenCodeGenerator(reserializeProperties, sceneUpdate, property, RavenPreferences.ValidateProperties, RavenPreferences.DumpFunctionInfo, firstTimeCompilation);
            codeGenerator.e_OnSceneBeingProcessed += OnSceneBeingProcessed;

            if (!Directory.Exists(c_OutputDir)) {
                Directory.CreateDirectory(c_OutputDir);
            }
            string file = Path.Combine(c_OutputDir, c_OutputFile);
            string[] infos;
            string[] warnings;
            try {
                codeGenerator.Run(file, out warnings, out infos);

                if (firstTimeCompilation) {
                    foreach (BuildTargetGroup bGrp in Enum.GetValues(typeof(BuildTargetGroup))) {
                        var obsoleteAttributes = typeof(BuildTargetGroup).GetMember(bGrp.ToString())[0].GetCustomAttributes(typeof(ObsoleteAttribute), false);
                        if (bGrp == BuildTargetGroup.Unknown ||
#if UNITY_5_6_OR_NEWER
                            bGrp == BuildTargetGroup.Switch ||
#endif
                            (obsoleteAttributes != null && obsoleteAttributes.Length > 0)) {
                            continue;
                        }
                        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(bGrp);
                        if (!defines.Contains("RAVEN_COMPILED")) {
                            PlayerSettings.SetScriptingDefineSymbolsForGroup(bGrp, defines + ";RAVEN_COMPILED");
                        }
                    }
                }

                AssetDatabase.Refresh();
            } catch (Exception e) {
                Debug.LogException(e);
                Debug.LogError("FATAL ERROR: Raven compilation failed!");
                EditorUtility.DisplayDialog("Raven Compiler", string.Format("Compilation failed! Check console for errors. {0}", firstTimeCompilation ? "You can retry manually by going to Raven->Compile First Time." : ""), "Ok");
                return false;
            } finally {
                EditorUtility.ClearProgressBar();
                s_IsCompiling = false;
            }

            foreach (var info in infos) {
                Debug.Log(info);
            }
            foreach (var warning in warnings) {
                Debug.LogWarning(warning);
            }
            EditorUtility.DisplayDialog("Raven Compiler", string.Format("Compilation succeeded{0}!", warnings.Length == 0 ? "" : " but there were warnings; check console"), "Ok");
            return true;
        }

        private static bool CanCompile() {
            return !EditorApplication.isUpdating && !s_IsCompiling && !EditorApplication.isCompiling && !EditorApplication.isPlayingOrWillChangePlaymode;
        }

        private static CallbackFunction FindCallback(bool firstTimeCompilation, bool reserializeProperties, bool sceneUpdate, RavenPropertyComponent property) {
            return s_Callbacks.Find((x) =>
                x.m_FirstTimeCompilation == firstTimeCompilation
                && x.m_ReserializeProperties == reserializeProperties
                && x.m_SceneUpdate == sceneUpdate
                && x.m_Property == property
            );
        }

        [DidReloadScripts(1)]
        [InitializeOnLoadMethod]
        private static void Init() {
#if !RAVEN_COMPILED
            EditorApplication.update -= QueuedCompile;
            EditorApplication.update += QueuedCompile;
#endif
        }

        private static void QueuedCompile() {
            if (CanCompile()) {
                EditorApplication.update -= QueuedCompile;
                FirstTimeCompile();
            }
        }

        private static void OnSceneBeingProcessed(string scene, int index, int total) {
            EditorUtility.DisplayProgressBar("Raven Compiler", string.Format("Compiling Properties in {0}...", scene), (float)index / total);
        }

        private class CallbackFunction {
            public bool m_FirstTimeCompilation;
            public bool m_ReserializeProperties;
            public bool m_SceneUpdate;
            public RavenPropertyComponent m_Property;

            public EditorApplication.CallbackFunction m_Function;

            public CallbackFunction(bool firstTimeCompilation, bool reserializeProperties, bool sceneUpdate, RavenPropertyComponent property) {
                m_FirstTimeCompilation = firstTimeCompilation;
                m_ReserializeProperties = reserializeProperties;
                m_SceneUpdate = sceneUpdate;
                m_Property = property;

                m_Function = new EditorApplication.CallbackFunction(() => Compile(m_FirstTimeCompilation, m_ReserializeProperties, m_SceneUpdate, m_Property));
            }
        }
    }
}