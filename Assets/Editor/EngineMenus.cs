using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System;

public class EngineMenus : MonoBehaviour
{
    [MenuItem("GameObject/Engine/LogSelectMeshName")]
    static void LogSelectedTransformName()
    {
        List<string> selectedNames = new List<string>();
        foreach (GameObject gameObject in Selection.gameObjects)
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                if (!selectedNames.Contains(renderer.name))
                    selectedNames.Add(renderer.name);
            }
        }

        string totalName = "";
        foreach (string name in selectedNames)
            totalName += name + "\n";
        Debug.Log(totalName);
    }


    [MenuItem("GameObject/Engine/LogNodeNum")]
    static void LoadTransformNum()
    {
        foreach (GameObject gameObject in Selection.gameObjects)
        {
            Transform[] transforms = gameObject.GetComponentsInChildren<Transform>();
            Debug.Log(gameObject.name + ":" + transforms.Length);
        }
    }

    [MenuItem("GameObject/Engine/LogDelayAnimation")]
    static void LogDelayAnimation()
    {
        foreach (GameObject gameObject in Selection.gameObjects)
        {
            DelayAnimation[] delayAnimations = gameObject.GetComponentsInChildren<DelayAnimation>();
            foreach (DelayAnimation anim in delayAnimations)
            {
                if (anim.Animation == null)
                    Debug.LogError("Empty animation: " + anim.name);

                if (anim.GetComponent<Animation>() == null)
                    Debug.LogError("Not animation: " + anim.name);
            }
        }
    }

    [MenuItem("GameObject/Engine/ReplaceEffectTime")]
    static void ReplaceEffectTime()
    {
        foreach (GameObject gameObject in Selection.gameObjects)
        {
            destroyThisTimed component = gameObject.GetComponent<destroyThisTimed>();
            if (component)
            {
                gameObject.AddComponent<EffectTime>().DestroyTime = component.destroyTime;

                GameObject.DestroyImmediate(component, true);
            }
            else
            {
                float maxTime = 0.0f;
                ParticleEmitter[] emitters = gameObject.GetComponentsInChildren<ParticleEmitter>(true);
                foreach (ParticleEmitter emitter in emitters)
                    maxTime = Mathf.Max(maxTime, emitter.maxEnergy);
                EffectTime effectTime = gameObject.GetComponent<EffectTime>();
                if (effectTime && maxTime > 0)
                    effectTime.DestroyTime = maxTime;
            }

            ParticleAnimator[] animators = gameObject.GetComponentsInChildren<ParticleAnimator>(true);
            foreach (ParticleAnimator animator in animators)
                animator.autodestruct = false;
        }
    }

    [MenuItem("GameObject/Engine/Build AllScene")]
    static void ExportResourceAllScene()
    {
        string path = EditorUtility.SaveFilePanel("Save Resource", "", "Scene-", "");
        if (string.IsNullOrEmpty(path))
            return;

        EditorBuildSettingsScene[] mScenes = EditorBuildSettings.scenes;
        for (int i = 0; i < mScenes.Length; i++)
        {
            string scenePath = mScenes[i].path;
            string[] levels = { scenePath };
            BuildPipeline.BuildStreamedSceneAssetBundle(levels, 
                path + Path.GetFileNameWithoutExtension(scenePath) + ".unity", 
                BuildTarget.Android);
        }
    }

    [MenuItem("GameObject/Engine/Build Tables")]
    static void ExportResourceTables()
    {
        Type requestType = typeof(ITableManager);
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                if (!requestType.IsAssignableFrom(type) || 
                    type.IsInterface ||
                    type.IsAbstract)
                    continue;

                ITableManager tableInstance = Activator.CreateInstance(type) as ITableManager;
                PropertyInfo tableInfo = type.GetProperty("TableData");
                object tableData = tableInfo.GetValue(tableInstance, null);
                string path = string.Format("{0}/Table/{1}.bytes", Application.dataPath, tableInstance.TableName());
                using (FileStream stream = new FileStream(path, FileMode.Create))
                {
                    ProtoBuf.Meta.RuntimeTypeModel.Default.Serialize(stream, tableData);
                    stream.Close();
                }
            }
        }
        AssetDatabase.Refresh();
    }

    static void ExportResourceNoTrack(BuildTarget target)
    {
        // Bring up save panel
        string path = EditorUtility.SaveFilePanel("Save Resource", "", "New Resource", "unity3d");
        if (path.Length == 0)
            return;
        
        // Build the resource file from the active selection.
        BuildPipeline.BuildAssetBundle(
            Selection.activeObject, 
            Selection.objects, 
            path, 
            BuildAssetBundleOptions.CompleteAssets, 
            target);
    }

    [MenuItem("Assets/Build AssetBundle [iPhone]")]
    static void BuildAssetBundle_iPhone()
    {
        ExportResourceNoTrack(BuildTarget.iOS);
    }

    [MenuItem("Assets/Build AssetBundle [Android]")]
    static void BuildAssetBundle_Android()
    {
        ExportResourceNoTrack(BuildTarget.Android);
    }
}
