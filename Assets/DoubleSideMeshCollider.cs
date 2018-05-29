using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleSideMeshCollider : MonoBehaviour
{
    public MeshCollider meshCollider;

    private void Awake()
    {
        meshCollider = GetComponent<MeshCollider>();
    }
    void Start()
    {
        if (meshCollider == null)
        {
            Destroy(this);
            return;
        }
        var mesh1 = meshCollider.sharedMesh;
        var mesh2 = Instantiate(mesh1);

        var normals = mesh2.normals;
        for (int i = 0; i < normals.Length; ++i)
        {
            normals[i] = -normals[i];
        }
        mesh2.normals = normals;

        for (int i = 0; i < mesh2.subMeshCount; ++i)
        {
            int[] triangles = mesh2.GetTriangles(i);
            for (int j = 0; j < triangles.Length; j += 3)
            {
                int temp = triangles[j];
                triangles[j] = triangles[j + 1];
                triangles[j + 1] = temp;
            }
            mesh2.SetTriangles(triangles, i);
        }

        gameObject.AddComponent<MeshCollider>().sharedMesh = mesh2;
        Destroy(this);
    }

}