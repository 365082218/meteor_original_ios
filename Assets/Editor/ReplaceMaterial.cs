using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class ReplaceMaterial: EditorWindow
{
    Object root;
    const string texturePathsn27 = "Assets/Materials/sn27/{0}";
    const string matPathsn27 = "Assets/Resource/Version/Resources/sn27_{0:d2}.mat";
    private void OnGUI()
    {
        root = EditorGUILayout.ObjectField("MapRoot", root, typeof(GameObject), true, GUILayout.Width(500));
        if (GUILayout.Button("Replace"))
        {
            StartReplace();
        }

        if (GUILayout.Button("Replace Texture"))
        {
            StartReplaceTexture();
        }
    }

    [MenuItem("Meteor/Material/ReplaceMaterial", false, 1)]
    static void Replace()
    {
        ReplaceMaterial window = (ReplaceMaterial)EditorWindow.GetWindow(typeof(ReplaceMaterial));
        window.titleContent = new GUIContent("Material Replace");
        window.Show();
    }

    void StartReplace()
    {
        if (root != null)
        {
            Dictionary<MeshRenderer, Material[]> matDict = new Dictionary<MeshRenderer, Material[]>();
            MeshRenderer[] mr = (root as GameObject).GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < mr.Length; i++)
            {
                if (mr[i].sharedMaterials != null)
                {
                    matDict.Add(mr[i], new Material[mr[i].sharedMaterials.Length]);
                    for (int j = 0; j < mr[i].sharedMaterials.Length; j++)
                    {
                        if (mr[i].sharedMaterials[j] == null)
                        {
                            matDict[mr[i]][j] = null;
                            continue;
                        }
                        string mat = mr[i].sharedMaterials[j].name;
                        string m = mat.Replace("sn19", "sn27");
                        string oldPath = AssetDatabase.GetAssetPath(mr[i].sharedMaterials[j]);
                        string newPath = oldPath.Replace("sn19", "sn27");
                        //AssetDatabase.CopyAsset(oldPath, newPath);
                        if (mat.StartsWith("sn19"))
                        {
                            matDict[mr[i]][j] = AssetDatabase.LoadAssetAtPath<Material>(newPath);
                            //mr[i].materials[j] = ;
                        }
                        else
                        {
                            matDict[mr[i]][j] = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GetAssetPath(mr[i].sharedMaterials[j]));
                        }
                    }

                    mr[i].materials = matDict[mr[i]];
                    //break;
                }
            }

            //foreach (var each in matDict)
            //{
            //    for (int i = 0; i < each.Value.Length; i++)
            //    {
            //        Debug.LogError(string.Format("mat:{0}", each.Value[i].name));
            //    }
            //}
        }
    }

    void StartReplaceTexture()
    {
        for (int i = 0; i < 76; i++)
        {
            string mat = string.Format(matPathsn27, i);
            Material m = AssetDatabase.LoadAssetAtPath<Material>(mat);
            if (m.mainTexture != null)
            {
                string full = AssetDatabase.GetAssetPath(m.mainTexture);
                int idx = full.IndexOf('.');
                if (idx != -1)
                {
                    string tex = string.Format(texturePathsn27, m.mainTexture.name) + full.Substring(idx);
                    m.mainTexture = AssetDatabase.LoadAssetAtPath<Texture>(tex);
                }
                else
                    Debug.LogError("error");
            }
        }
    }
}
