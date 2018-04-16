using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ShowBorder : MonoBehaviour {

    GameObject [] mBorder = new GameObject[12];
    LineRenderer[] mLineRender = new LineRenderer[12];
    public Color startColor = Color.red;
    public Color endColor = Color.red;
	// Use this for initialization
	void Start () {
	    for (int i = 0; i < 12; i++)
        {
            mBorder[i] = new GameObject();
            mBorder[i].transform.parent = gameObject.transform;
            mBorder[i].layer = gameObject.layer;
            mLineRender[i] = mBorder[i].AddComponent<LineRenderer>();
            mLineRender[i].SetWidth(0.08f, 0.08f);
            mLineRender[i].SetVertexCount(2);
            mLineRender[i].SetColors(startColor, endColor);
            mLineRender[i].material = new Material(Shader.Find("Particles/Additive"));
        }
	}
	
	// Update is called once per frame
	void Update () {
        Mesh me = gameObject.GetComponent<MeshFilter>().mesh;
        BoxCollider collider = gameObject.GetComponent<BoxCollider>();
        Vector3[] mVertices = new Vector3[me.vertexCount];

        List<Vector3> exist = new List<Vector3>();
        
        Quaternion qu = Quaternion.Euler(gameObject.transform.eulerAngles);
        Matrix4x4 mat = new Matrix4x4();
        mat.SetTRS(new Vector3(0, 0, 0), qu, Vector3.one);
        for (int i = 0; i < me.vertexCount; i++)
        {
            //me.vertices[i].x == + - 0.5f
			//mVertices[i] = mat.MultiplyPoint(me.vertices[i]);
			//mVertices[i].x = (gameObject.transform.position.x + collider.center.x + me.vertices[i].x * collider.size.x);
			//mVertices[i].y = (gameObject.transform.position.y + collider.center.y + me.vertices[i].y * collider.size.y);
			//mVertices[i].z = (gameObject.transform.position.z + collider.center.z + me.vertices[i].z * collider.size.z);

			mVertices[i].x = (gameObject.transform.localScale.x * collider.center.x + me.vertices[i].x * collider.size.x * gameObject.transform.lossyScale.x);
			mVertices[i].y = (gameObject.transform.localScale.y * collider.center.y + me.vertices[i].y * collider.size.y * gameObject.transform.lossyScale.y);
			mVertices[i].z = (gameObject.transform.localScale.z * collider.center.z + me.vertices[i].z * collider.size.z * gameObject.transform.lossyScale.z);

//            mVertices[i].x = (gameObject.transform.position.x + gameObject.transform.localScale.x * collider.center.x + me.vertices[i].x * collider.size.x * gameObject.transform.localScale.x);
//            mVertices[i].y = (gameObject.transform.position.y + gameObject.transform.localScale.y * collider.center.y + me.vertices[i].y * collider.size.y * gameObject.transform.localScale.y);
//            mVertices[i].z = (gameObject.transform.position.z + gameObject.transform.localScale.z * collider.center.z + me.vertices[i].z * collider.size.z * gameObject.transform.localScale.z);
            if (!exist.Contains(mVertices[i]))
                exist.Add(mVertices[i]);
        }

        //bug
        //修正顶点，旋转后，部分顶点与旋转轴平行，顶点不会得到调整.
        //中心点加旋转后的向量的一半或者减旋转后的向量一半,得到新的旋转后的向量.
        //中心点坐标
        Vector3 orig = new Vector3(0, 0, 0);
        int nUsed = 0;
        List<KeyValuePair<Vector3, Vector3>> DrawedLine = new List<KeyValuePair<Vector3, Vector3>>(); 
        for (int i = 0; i < exist.Count; i++)
        {
            if (DrawedLine.Count == 12)
                break;
            List<Vector3> near = GetNextVertix(exist[i], exist, DrawedLine);
            for (int j = 0; j < near.Count; j++)
            {
//                Vector3 line = exist[i] - near[j];
//                Vector3 half = line / 2.0f + near[j];
//                line = mat.MultiplyPoint(line);
//                Vector3 head = half + line / 2.0f;
//                Vector3 tail = half - line / 2.0f;
//                mLineRender[nUsed].SetPosition(0, head);
//                mLineRender[nUsed].SetPosition(1, tail);
				Vector3 begin = mat.MultiplyPoint(exist[i]);
				Vector3 end = mat.MultiplyPoint(near[j]);
				mLineRender[nUsed].SetPosition(0, gameObject.transform.position + begin);
				mLineRender[nUsed].SetPosition(1, gameObject.transform.position + end);
                //mLineRender[nUsed].SetPosition(0, exist[i]);
                //mLineRender[nUsed].SetPosition(1, near[j]);
                DrawedLine.Add(new KeyValuePair<Vector3, Vector3>(exist[i], near[j]));
                nUsed++;
            }
            
        }
	}

    List<Vector3> GetNextVertix(Vector3 pos, List<Vector3> Select, List<KeyValuePair<Vector3, Vector3>> drawed)
    {
        List<Vector3> vNext = new List<Vector3>();
        foreach (var each in Select)
        {
            if (pos.x == each.x && pos.y == each.y && pos.z != each.z)
            {
                vNext.Add(each);
            }
            else if (pos.y == each.y && pos.z == each.z && pos.x != each.x)
            {
                vNext.Add(each);
            }
            else if (pos.x == each.x && pos.z == each.z && pos.y != each.y)
            {
                vNext.Add(each);
            }
        }

        List<Vector3> deleted = new List<Vector3>();
        foreach (var each in vNext)
        {
            KeyValuePair<Vector3, Vector3> ex = new KeyValuePair<Vector3, Vector3>(pos, each);
            KeyValuePair<Vector3, Vector3> ex1 = new KeyValuePair<Vector3, Vector3>(each, pos);
            if (drawed.Contains(ex) || drawed.Contains(ex1))
                deleted.Add(each);
        }

        foreach (var each in deleted)
        {
            if (vNext.Contains(each))
                vNext.Remove(each);
        }
        return vNext;
    }

    public void SetColors(Color b, Color e)
    {
        startColor = b;
        endColor = e;
        foreach (var each in mLineRender)
        {
            if (each != null)
            {
                each.SetColors(startColor, endColor);
            }
        }
    }
}
