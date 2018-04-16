using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CobDataLoader
{
    public static int[] headArray = new int[4];
    public static float[] pos = new float[9];
    public static List<int[]> content = new List<int[]>();
    public static void Load(TextAsset asset)
    {
        System.IO.BinaryReader reader = new System.IO.BinaryReader(new System.IO.MemoryStream(asset.bytes));
        reader.ReadBytes(10);
        int i = 0;
        while (i < 4)
        {
            headArray[i] = reader.ReadInt32();
            i++;
        }
        int num2 = 0;
        if (0 < pos.LongLength)
        {
            do
            {
                pos[num2] = reader.ReadSingle();
                num2++;
            }
            while ((long)num2 < pos.LongLength);
        }
        if (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            do
            {
                int num3 = reader.ReadInt32();
                int[] array = new int[num3];
                int num4 = 0;
                if (0 < num3)
                {
                    do
                    {
                        array[num4] = reader.ReadInt32();
                        num4++;
                    }
                    while (num4 < num3);
                }
                content.Add(array);
            }
            while (reader.BaseStream.Position < reader.BaseStream.Length);
        }
        reader.Close();
    }

    public static void GenerateCobMesh(Transform parent, GMBFile gmb)
    {
        GameObject obj = new GameObject("hellBox");
        obj.transform.position = Vector3.zero;
        obj.transform.rotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;
        obj.transform.SetParent(parent);
        obj.layer = parent.gameObject.layer;
        BoxCollider boc = obj.AddComponent<BoxCollider>();
        //右手转换左手
        Vector3 lefttop = new Vector3(pos[3], pos[5], pos[4]);
        Vector3 rightbottom = new Vector3(pos[6], pos[8], pos[7]);
        boc.center = (lefttop + rightbottom)/ 2;
        boc.size = lefttop - rightbottom;

        //for (int i = 0; i < content.Count; i++)
        //{
        //    GameObject objsub = new GameObject("submesh" + i);
        //    objsub.transform.position = Vector3.zero;
        //    objsub.transform.rotation = Quaternion.identity;
        //    objsub.transform.localScale = Vector3.one;
        //    objsub.transform.SetParent(parent);
        //    objsub.layer = parent.gameObject.layer;
        //    MeshCollider mec = objsub.AddComponent<MeshCollider>();
        //    MeshFilter mf = objsub.AddComponent<MeshFilter>();
        //    MeshRenderer mr = objsub.AddComponent<MeshRenderer>();

        //    Mesh mesh = new Mesh();
        //    List<Vector3> vec = new List<Vector3>();
        //    List<int> triangles = new List<int>();
        //    //得到低模的顶点缓冲区，索引缓冲区，然后应用到新网格上，使用这个网格做碰撞.
        //    for (int j = 0; j < content[i].Length; j++)
        //    {
        //        //得到每个面的顶点索引序号.
        //        List<int> vecIndexFace = GetVerticesIndex(content[i][j], gmb);
        //    }
        //    mesh.SetVertices(vec);
        //    mesh.SetTriangles();
        //}
    }

    //给一个面序号，得到面的3个顶点
    static List<Vector3> GetVertices(int faceid, GMBFile gmb)
    {
        List<Vector3> vec = new List<Vector3>();
        int faceIndex = faceid;
        if (faceid < gmb.FacesCount)
        {
            for (int i = 0; i < gmb.mesh.Length; i++)
            {
                if (faceIndex - gmb.mesh[i].Faces.Length < 0)
                {
                    float x0 = gmb.mesh[i].Vertices[(int)gmb.mesh[i].Faces[faceIndex, 1], 0];
                    float y0 = gmb.mesh[i].Vertices[(int)gmb.mesh[i].Faces[faceIndex, 1], 0];
                    float z0 = gmb.mesh[i].Vertices[(int)gmb.mesh[i].Faces[faceIndex, 1], 0];
                    float x1 = gmb.mesh[i].Vertices[(int)gmb.mesh[i].Faces[faceIndex, 3], 2];
                    float y1 = gmb.mesh[i].Vertices[(int)gmb.mesh[i].Faces[faceIndex, 3], 2];
                    float z1 = gmb.mesh[i].Vertices[(int)gmb.mesh[i].Faces[faceIndex, 3], 2];
                    float x2 = gmb.mesh[i].Vertices[(int)gmb.mesh[i].Faces[faceIndex, 2], 1];
                    float y2 = gmb.mesh[i].Vertices[(int)gmb.mesh[i].Faces[faceIndex, 2], 1];
                    float z2 = gmb.mesh[i].Vertices[(int)gmb.mesh[i].Faces[faceIndex, 2], 1];
                    vec.Add(new Vector3(x0, y0, z0));
                    vec.Add(new Vector3(x1, y1, z1));
                    vec.Add(new Vector3(x2, y2, z2));
                    return vec;
                }
                //new List<int> { (int)mesh[i].Faces[j, 1], (int)mesh[i].Faces[j, 3], (int)mesh[i].Faces[j, 2] };
                else
                    faceIndex -= gmb.mesh[i].Faces.Length;
            }
        }
        return vec;
    }

}
public class CobLoader : MonoBehaviour {
    public TextAsset CobData;
    public string snLevel;//"sn14"
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Load()
    {
        if (CobData != null)
        {
            CobDataLoader.Load(CobData);
            CobDataLoader.GenerateCobMesh(transform, GMBLoader.Instance.Load(snLevel));
        }
    }
}
