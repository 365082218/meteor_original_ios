using UnityEngine;
using System.Collections;

//show vertices
//显示网格的全部顶点.
public class ShowVertices : MonoBehaviour {
	// Use this for initialization
	LineRenderer line;
	Mesh me;
	public int [] verIndex;
	public Vector3[] vertices;
	int nTriangleIndex = 0;
	void Start () 
	{
		line = gameObject.GetComponent<LineRenderer>();
		if (line == null)
			line = gameObject.AddComponent<LineRenderer>();
		MeshCollider meshColl = gameObject.GetComponent<MeshCollider>();
		if (meshColl != null)
			me = meshColl.sharedMesh;
		line.SetWidth(0.02f, 0.02f);
		line.SetColors(Color.red, Color.red);
		if (me != null)
		{
			line.SetVertexCount(me.triangles.Length + 6);
            for (int i = 0; i < me.triangles.Length + 6; i++)
                line.SetPosition(i, Vector3.zero);
			verIndex = me.triangles;
			vertices = me.vertices;
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
        if (me != null)
        {
            Matrix4x4 mat = new Matrix4x4();
            mat.SetTRS(gameObject.transform.position, gameObject.transform.rotation, gameObject.transform.lossyScale);

            for (int i = 0; i < verIndex.Length / 3; i++)
            {
                Vector3[] lineVer = new Vector3[3];
                for (int j = 0; j < 3; j++)
                {
                    lineVer[j] = transform.TransformPoint(vertices[verIndex[i * 3 + j]]);//mat.MultiplyPoint(vertices[verIndex[i*3+j]]);
                    line.SetPosition(i * 3 + j, lineVer[j]);
                }
                //line.SetPosition(i*3, lineVer[0]);
                //line.SetPosition(i*3+1, lineVer[1]);
                //line.SetPosition(i*3+1, lineVer[1]);
                //line.SetPosition(i*3+2, lineVer[2]);
                //line.SetPosition(i*3+2, lineVer[2]);
                //line.SetPosition(i*3, lineVer[0]);

            }
        }
	}

	public void SelectVertices(int nIndex)
	{
		nTriangleIndex = nIndex;
	}

    public void ChangeShow(bool bShow)
    {
        if (!bShow)
        {
            if (line != null)
                line.enabled = false;
        }
        else
        {
            if (line != null)
                line.enabled = true;
        }
    }
}
