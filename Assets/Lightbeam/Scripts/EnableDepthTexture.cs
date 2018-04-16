using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class EnableDepthTexture : MonoBehaviour
{
    public bool EnableInEditor = true;

    void OnEnable()
    {
        
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
#if UNITY_EDITOR
        if (EnableInEditor)
            if (UnityEditor.SceneView.currentDrawingSceneView != null && UnityEditor.SceneView.currentDrawingSceneView.camera != null)
                UnityEditor.SceneView.currentDrawingSceneView.camera.depthTextureMode = DepthTextureMode.Depth;
#endif
    }
    void OnDisable()
    {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.None;
#if UNITY_EDITOR
        if (UnityEditor.SceneView.currentDrawingSceneView != null && UnityEditor.SceneView.currentDrawingSceneView.camera != null)
            UnityEditor.SceneView.currentDrawingSceneView.camera.depthTextureMode = DepthTextureMode.None;
#endif
    }

    void Update()
    { 
#if UNITY_EDITOR
        if (EnableInEditor)
        {
            if (UnityEditor.SceneView.currentDrawingSceneView != null && UnityEditor.SceneView.currentDrawingSceneView.camera != null)
                UnityEditor.SceneView.currentDrawingSceneView.camera.depthTextureMode = DepthTextureMode.Depth;
        }
        else
        {
            if (UnityEditor.SceneView.currentDrawingSceneView != null && UnityEditor.SceneView.currentDrawingSceneView.camera != null)
                UnityEditor.SceneView.currentDrawingSceneView.camera.depthTextureMode = DepthTextureMode.None;
        }
#endif
        
    }
}
