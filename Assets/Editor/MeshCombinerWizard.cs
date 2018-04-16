using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Xml;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class MeshCombinerWizard : ScriptableWizard
{
    public GameObject RootNode;
    public bool WriteUv2 = false;
    public int MinCheckSize = 1024;
    public int MaxCheckSize = 8000;
    public int CheckStep = 50;

    [MenuItem("GameObject/Engine/Mesh Combiner")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<MeshCombinerWizard>("Mesh Combiner", "Close", "Combine");
    }

    void OnWizardCreate()
    {
    }

    void OnWizardOtherButton()
    {
        List<string> materials = new List<string>();
        MeshFilter[] meshFilters = RootNode.GetComponentsInChildren<MeshFilter>();

        if (WriteUv2)
            MeshToFile(meshFilters, true);
        else
            MeshToFile(meshFilters, false);
    }

    void MeshToFile(MeshFilter[] meshFilters, bool uv2)
    {
        StringBuilder sb = new StringBuilder();

        Rect unitRect = new Rect(0, 0, 1, 1);
        Rect[] rects = uv2 ? PackMeshFilters(meshFilters) : null;

        int vertexNum = 0;
        for (int i = 0; i < meshFilters.Length; i++)
        {
            Rect uvModify = uv2 ? rects[i] : unitRect;
            sb.Append(MeshToString(meshFilters[i], ref vertexNum, uv2 ? 1 : 0, uvModify));
        }
        sb.Append("\ng");

        string fileName = EditorUtility.SaveFilePanel(
                    "Save combined mesh as obj",
                    "",
                    uv2 ? "unname1" : "unname",
                    "obj");

        if (fileName.Length > 0)
            File.WriteAllText(fileName, sb.ToString());
    }

    Rect[] PackMeshFilters(MeshFilter[] meshFilters)
    {
        int textureSize = 1024;
        float maxArea = 0;
        int maxTextureSize = 0;

        Rect[] result = null;
        for (int testSize = MinCheckSize; testSize < MaxCheckSize; testSize += CheckStep)
        {
            Rect[] testResult = PackMeshFilters(meshFilters, textureSize, testSize);
            System.GC.Collect();

            float area = 0.0f;
            foreach (Rect rect in testResult)
                area += rect.width * rect.height;

            if (area > maxArea)
            {
                maxArea = area;
                maxTextureSize = testSize;
                result = testResult;
            }
        }
        Debug.Log(string.Format("The max fit size is: {0} {1}", maxTextureSize, maxArea));
        return result;
    }

    Rect[] PackMeshFilters(MeshFilter[] meshFilters, int textureSize, int baseSize)
    {
        Texture2D packTexture = new Texture2D(textureSize, textureSize, TextureFormat.Alpha8, false);
        Texture2D[] meshTextures = new Texture2D[meshFilters.Length];
        for (int i = 0; i < meshFilters.Length; i++)
        {
            MeshFilter filter = meshFilters[i];
            MeshRenderer render = filter.GetComponent<MeshRenderer>();
            if (render == null)
            {
                meshTextures[i] = new Texture2D(1, 1);
                continue;
            }

            int width = (int)Mathf.Max(1, Mathf.Round(render.lightmapScaleOffset.x * baseSize));
            int height = (int)Mathf.Max(1, Mathf.Round(render.lightmapScaleOffset.y * baseSize));
            meshTextures[i] = new Texture2D(width, height, TextureFormat.Alpha8, false);
        }

        Rect[] result = packTexture.PackTextures(meshTextures, 1, textureSize);
        foreach (Texture2D tex in meshTextures)
            Texture2D.DestroyImmediate(tex);
        Texture2D.DestroyImmediate(packTexture);
        return result;
    }

    string MeshToString(MeshFilter fileter, ref int vertexNum, int uvIdx, Rect uvModify)
    {
        Mesh mesh = fileter.sharedMesh;
        if (mesh == null)
            return "";

        StringBuilder sb = new StringBuilder();
        Material[] materials = fileter.gameObject.GetComponent<MeshRenderer>().sharedMaterials;

        sb.Append("g\n");
        foreach (Vector3 v in mesh.vertices)
        {
            Vector3 tv = fileter.transform.TransformPoint(v);
            sb.Append(string.Format("v {0} {1} {2}\n", -tv.x, tv.y, tv.z));
        }

        sb.Append("\n");
        Vector2[] uv = mesh.uv;
        if (uvIdx == 1) uv = mesh.uv2;
        else if (uvIdx == 2) uv = mesh.uv2;

        // check the uv exist.
        if (uv == null || uv.Length == 0)
            Debug.LogError(string.Format("Mesh has no uv: ", fileter.name));

        foreach (Vector3 v in uv)
        {
            Vector3 tv = new Vector3(
                v.x * uvModify.width + uvModify.x,
                v.y * uvModify.height + uvModify.y,
                v.z);
            sb.Append(string.Format("vt {0} {1}\n", tv.x, tv.y));
        }

        sb.Append("\n");
        foreach (Vector3 v in mesh.normals)
        {
            Vector3 tv = fileter.transform.TransformDirection(v);
            sb.Append(string.Format("vn {0} {1} {2}\n", -tv.x, tv.y, tv.z));
        }

        sb.Append("\n");
        sb.Append("g ").Append(fileter.name).Append("\n");
        for (int subMeshIdx = 0; subMeshIdx < mesh.subMeshCount; subMeshIdx++)
        {
            sb.Append("usemtl ").Append(materials[subMeshIdx].name).Append("\n");

            int[] triangles = mesh.GetTriangles(subMeshIdx);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                    triangles[i] + 1 + vertexNum,
                    triangles[i + 2] + 1 + vertexNum,
                    triangles[i + 1] + 1 + vertexNum));
            }
        }
        sb.Append("\n");

        vertexNum += mesh.vertices.Length;
        return sb.ToString();
    }
}
