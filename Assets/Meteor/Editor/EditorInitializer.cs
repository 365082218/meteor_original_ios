using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

//控制了点Unity播放按钮时的事件，当激活场景在Build列表里时被勾选，那么从游戏的入口场景开始
//否则单独以此场景为入口打开
[InitializeOnLoad]
public class EditorInitializer
{
    public const string Tag = "EditorInitializer";
    static string PreviousSceneKey = "UnityEditorInitializerPreviousSceneKey";
    private const string StartUpScenePath = "Assets/Scene/Patch.unity";
    static EditorInitializer()
    {
        EditorApplication.playmodeStateChanged -= onPlaymodeStateChanged;
        EditorApplication.playmodeStateChanged += onPlaymodeStateChanged;
    }

    private static void onPlaymodeStateChanged()
    {
        //play pressed to start playing
        if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying && !EditorApplication.isPaused)
        {
            string path = SceneManager.GetActiveScene().path;
            PlayerPrefs.SetString(PreviousSceneKey, path);
            PlayerPrefs.Save();
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;

            bool runStartUpScene = string.IsNullOrEmpty(path);

            if (!runStartUpScene)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorApplication.isPlaying = false;
                    return;
                }
            }

            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].path == path && scenes[i].enabled)
                {
                    runStartUpScene = true;
                    break;
                }
            }

            if (runStartUpScene && File.Exists(StartUpScenePath))
            {
                EditorSceneManager.OpenScene(StartUpScenePath);
            }
        }
        // Change to the scene that was launched from
        if (!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying && !EditorApplication.isPaused)
        {
            string scene = PlayerPrefs.GetString(PreviousSceneKey);
            if (!string.IsNullOrEmpty(scene))
            {
                EditorSceneManager.OpenScene(scene);
            }
        }
    }
}
