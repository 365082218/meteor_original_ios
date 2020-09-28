using UnityEngine;
using System.Collections;
/*
Attach this script as a parent to some game objects. The script will then combine the meshes at startup.
This is useful as a performance optimization since it is faster to render one big mesh than many small meshes. See the docs on graphics performance optimization for more info.

Different materials will cause multiple meshes to be created, thus it is useful to share as many textures/material as you can.
*/

[AddComponentMenu("Mesh/Combine Children")]
public class CombineChildren : MonoBehaviour {
	
	/// Usually rendering with triangle strips is faster.
	/// However when combining objects with very low triangle counts, it can be faster to use triangles.
	/// Best is to try out which value is faster in practice.
	public bool generateTriangleStrips = false;

    /// This option has a far longer preprocessing time at startup but leads to better runtime performance.
    Component[] filters;
    MeshCombineUtility.MeshElement element = null;
    MeshCombineUtility.MeshInstance[] instance;
    MeshFilter mf;
    Hashtable materialToMesh = new Hashtable();
    bool combine = false;
    void Start () {
        Combine();
	}

    public void Combine() {
        if (combine)
            return;
        combine = true;
        filters = GetComponentsInChildren(typeof(MeshFilter));
        Matrix4x4 myTransform = transform.worldToLocalMatrix;

        instance = new MeshCombineUtility.MeshInstance[filters.Length];
        for (int i = 0; i < filters.Length; i++) {
            MeshFilter filter = (MeshFilter)filters[i];
            Renderer curRenderer = filters[i].GetComponent<Renderer>();
            instance[i] = new MeshCombineUtility.MeshInstance();
            instance[i].mesh = filter.sharedMesh;
            instance[i].childIdx = i;
            if (curRenderer != null && curRenderer.enabled && instance[i].mesh != null) {
                instance[i].transform = myTransform * filter.transform.localToWorldMatrix;

                Material[] materials = curRenderer.sharedMaterials;
                for (int m = 0; m < materials.Length; m++) {
                    instance[i].subMeshIndex = System.Math.Min(m, instance[i].mesh.subMeshCount - 1);

                    ArrayList objects = (ArrayList)materialToMesh[materials[m]];
                    if (objects != null) {
                        objects.Add(instance[i]);
                    } else {
                        objects = new ArrayList();
                        objects.Add(instance[i]);
                        materialToMesh.Add(materials[m], objects);
                    }
                }

                curRenderer.enabled = false;
            }
        }

        foreach (DictionaryEntry de in materialToMesh) {
            ArrayList elements = (ArrayList)de.Value;
            MeshCombineUtility.MeshInstance[] instances = (MeshCombineUtility.MeshInstance[])elements.ToArray(typeof(MeshCombineUtility.MeshInstance));

            // We have a maximum of one material, so just attach the mesh to our own game object
            if (materialToMesh.Count == 1) {
                // Make sure we have a mesh filter & renderer
                if (GetComponent(typeof(MeshFilter)) == null)
                    gameObject.AddComponent(typeof(MeshFilter));
                if (!GetComponent("MeshRenderer"))
                    gameObject.AddComponent<MeshRenderer>();

                MeshFilter filter = (MeshFilter)GetComponent(typeof(MeshFilter));
                filter.mesh = MeshCombineUtility.CombineFirst(instances, generateTriangleStrips, ref element);
                GetComponent<Renderer>().material = (Material)de.Key;
                GetComponent<Renderer>().enabled = true;
                mf = filter;
            }
            // We have multiple materials to take care of, build one mesh / gameobject for each material
            // and parent it to this object
            else {
                GameObject combinedMesh = new GameObject("Combined mesh");
                combinedMesh.transform.parent = transform;
                combinedMesh.transform.localScale = Vector3.one;
                combinedMesh.transform.localRotation = Quaternion.identity;
                combinedMesh.transform.localPosition = Vector3.zero;
                combinedMesh.AddComponent(typeof(MeshFilter));
                combinedMesh.AddComponent<MeshRenderer>();
                combinedMesh.GetComponent<Renderer>().material = (Material)de.Key;
                MeshFilter filter = (MeshFilter)combinedMesh.GetComponent(typeof(MeshFilter));
                filter.mesh = MeshCombineUtility.Combine(instances, generateTriangleStrips);
            }
        }
    }
    //fmc动画，只能合并单材质球的多个对象，不能合并一系列材质球的混合对象.
    public void UpdateMesh()
    {
        //return;
        //foreach (DictionaryEntry de in materialToMesh)
        //{
        //    ArrayList elements = (ArrayList)de.Value;
        //    MeshCombineUtility.MeshInstance[] instances = (MeshCombineUtility.MeshInstance[])elements.ToArray(typeof(MeshCombineUtility.MeshInstance));
            for (int i = 0; i < instance.Length; i++)
                instance[i].transform = transform.worldToLocalMatrix * filters[instance[i].childIdx].transform.localToWorldMatrix;
            // We have a maximum of one material, so just attach the mesh to our own game object
            if (materialToMesh.Count == 1)
            {
                mf.mesh = MeshCombineUtility.CombineEx(instance, generateTriangleStrips, ref element);
            }
            else
            {
                Debug.LogError("FMCPlayer Mesh Can not ");
                //MeshFilter filter = (MeshFilter)combinedMesh.GetComponent(typeof(MeshFilter));
                //filter.mesh = MeshCombineUtility.Combine(instances, generateTriangleStrips, ref element, true);
            }
        //}
    }
}