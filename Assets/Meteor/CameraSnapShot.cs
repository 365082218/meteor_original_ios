using UnityEngine;
using System.Collections;

public class CameraSnapShot : MonoBehaviour {

    public Camera targetCamera;
    public int LevelIdx = 0;
	// Use this for initialization
	void Start () {
	        
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnSaveMapFogTexture()
    {
        if (targetCamera == null)
            targetCamera = gameObject.GetComponent<Camera>();
        if (targetCamera == null)
        {
            Debug.Log("TargetCamera is Null");
            return;
        }
        int width = targetCamera.targetTexture.width;
        int height = targetCamera.targetTexture.height;
        Texture2D texture = new Texture2D(width, height);
        RenderTexture.active = null;
        RenderTexture.active = targetCamera.targetTexture;
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture.Apply();
        System.IO.File.WriteAllBytes(Application.persistentDataPath + "/" + LevelIdx + "_mask.png", texture.EncodeToPNG());
        RenderTexture.active = null;
    }
}
