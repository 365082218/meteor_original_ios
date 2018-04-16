using UnityEngine;
using System.Collections;

//用来修改某个网格的数据，并反应到场景中
public class MeshInfo : MonoBehaviour {

    public Vector3[] vertex;
    public Color[] color;
    public Vector3[] normal;
    public MeshFilter mf;
    public int[] triangles;
    public Vector2[] uvs;
    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Init()
    {
        if (mf != null)
        {
            vertex = mf.mesh.vertices;
            color = mf.mesh.colors;
            normal = mf.mesh.normals;
            triangles = mf.mesh.triangles;
            uvs = mf.mesh.uv;
        }
    }

    public void UpdateMesh()
    {
        if (mf != null)
        {
            mf.mesh.SetVertices(new System.Collections.Generic.List<Vector3>(vertex));
            mf.mesh.SetTriangles(triangles, 0);
            mf.mesh.SetNormals(new System.Collections.Generic.List<Vector3>(normal));
            mf.mesh.SetColors(new System.Collections.Generic.List<Color>(color));
            mf.mesh.SetUVs(0, new System.Collections.Generic.List<Vector2>(uvs));
            mf.mesh.RecalculateBounds();
            mf.mesh.RecalculateNormals();
        }
    }
}
