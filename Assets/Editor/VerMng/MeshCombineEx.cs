using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class MeshCombineEx:Editor
{
    [MenuItem("Meteor/Mesh/AutoCombine", false, 0)]
    public static void AutoCombine()
    {
        GameObject obj = Selection.activeObject as GameObject;
        if (obj == null)
            return;
        materialDic.Clear();
        GameObjectDic.Clear();
        MeshRenderer[] mrs = obj.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < mrs.Length; i++)
        {
            if (!materialDic.ContainsKey(mrs[i].gameObject))
                materialDic.Add(mrs[i].gameObject, new List<string>());
            for (int j = 0; j < mrs[i].sharedMaterials.Length; j++)
            {
                string path = mrs[i].sharedMaterials[j].name;
                materialDic[mrs[i].gameObject].Add(path);
            }
        }

        foreach (var each in materialDic)
        {
            string k = "";
            for (int i = 0; i < each.Value.Count; i++)
                k += each.Value[i];
            if (!GameObjectDic.ContainsKey(k))
            {
                GameObjectDic.Add(k, new List<GameObject>() { each.Key });
            }
            else
            {
                GameObjectDic[k].Add(each.Key);
            }
        }

        foreach (var each in GameObjectDic)
        {
            if (each.Value.Count == 1)
                continue;
            GameObject combined = new GameObject();
            combined.name = each.Key;
            combined.transform.SetParent(obj.transform);
            combined.transform.position = Vector3.zero;
            combined.transform.rotation = Quaternion.identity;
            combined.transform.localScale = Vector3.one;
            for (int i = 0; i < each.Value.Count; i++)
            {
                GameObject objclone = GameObject.Instantiate(each.Value[i]);
                objclone.transform.SetParent(combined.transform);
                each.Value[i].SetActive(false);
            }
        }
    }

    static Dictionary<GameObject, List<string>> materialDic = new Dictionary<GameObject, List<string>>();
    static Dictionary<string, List<GameObject>> GameObjectDic = new Dictionary<string, List<GameObject>>();
}
