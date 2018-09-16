using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GmbLoader : MonoBehaviour {
    [SerializeField] private TextAsset Gmb = null;
    GMBFile GmbFile;
    private void Awake()
    {
        if (Gmb != null)
        {
            GmbFile = new GMBFile();
            MemoryStream ms = new MemoryStream(Gmb.bytes);
            GmbFile.Analyse(ms);
            LoadModel();
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void LoadModel()
    {
        Material[] mat = new Material[GmbFile.TexturesCount];
        Dictionary<int, string> iflMaterials = new Dictionary<int, string>();
        for (int x = 0; x < GmbFile.TexturesCount; x++)
        {
            mat[x] = null;
            string tex = GmbFile.TexturesNames[x];
            //若不是以ifl结尾的材质，那么要分割文件后缀，若是ifl类型的，则直接是xx.ifl.bytes类型的数据，是OK的
            bool iflMaterial = true;
            if (!tex.ToLower().EndsWith(".ifl"))
            {
                iflMaterial = false;
                int del = tex.LastIndexOf('.');
                if (del != -1)
                    tex = tex.Substring(0, del);
            }

            if (iflMaterial)
            {
                if (!iflMaterials.ContainsKey(x))
                    iflMaterials.Add(x, tex);//记录x号材质是ifl类型的材质,以后会用到，这个序号的材质并且动态更换这个材质的贴图的
            }
            else
            {
                string Model = string.Format("{0}_{1:D2}{2}", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, Gmb.name, x);//还要考虑重载贴图也要生成对应的新材质
                mat[x] = Resources.Load<Material>(Model);
                if (mat[x] == null)
                {
                    Texture texture = Resources.Load<Texture>(tex);
                    if (texture == null)
                        Debug.LogError(string.Format("texture miss on load gmb:{0} texture name:{1}", tex, tex));
                    mat[x] = new Material(Resources.Load<Shader>("Mobile-Diffuse"));
                    mat[x].SetTexture("_MainTex", texture);
                    mat[x].name = Model;
                }
            }
        }

        for (int i = 0; i < GmbFile.SceneObjectsCount; i++)
        {
            GameObject objMesh = new GameObject();
            bool addIflComponent = false;
            int iflParam = -1;
            int iflMatIndex = 0;
            objMesh.name = Gmb.name;
            objMesh.transform.localRotation = Quaternion.identity;
            objMesh.transform.localPosition = Vector3.zero;
            objMesh.transform.localScale = Vector3.one;
            Mesh w = new Mesh();
            //前者子网格编号，后者 索引缓冲区
            Dictionary<int, List<int>> tr = new Dictionary<int, List<int>>();
            List<Vector3> ve = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            List<Vector3> nor = new List<Vector3>();
            List<Color> col = new List<Color>();
            for (int k = 0; k < GmbFile.mesh[i].faces.Count; k++)
            {
                int key = GmbFile.mesh[i].faces[k].material;
                if (tr.ContainsKey(key))
                {
                    tr[key].Add(GmbFile.mesh[i].faces[k].triangle[0]);
                    tr[key].Add(GmbFile.mesh[i].faces[k].triangle[1]);
                    tr[key].Add(GmbFile.mesh[i].faces[k].triangle[2]);
                }
                else
                {
                    tr.Add(key, new List<int>());
                    tr[key].Add(GmbFile.mesh[i].faces[k].triangle[0]);
                    tr[key].Add(GmbFile.mesh[i].faces[k].triangle[1]);
                    tr[key].Add(GmbFile.mesh[i].faces[k].triangle[2]);
                }

            }
            for (int k = 0; k < GmbFile.mesh[i].vertices.Count; k++)
            {
                ve.Add(GmbFile.mesh[i].vertices[k].pos);
                uv.Add(GmbFile.mesh[i].vertices[k].uv);
                col.Add(GmbFile.mesh[i].vertices[k].color);
                nor.Add(GmbFile.mesh[i].vertices[k].normal);
            }
            w.SetVertices(ve);
            w.uv = uv.ToArray();
            w.subMeshCount = tr.Count;
            int ss = 0;
            List<Material> targetMat = new List<Material>();
            foreach (var each in tr)
            {
                w.SetIndices(each.Value.ToArray(), MeshTopology.Triangles, ss++);
                if (each.Key >= 0 && each.Key < GmbFile.shader.Length)
                {
                    int materialIndex = GmbFile.shader[each.Key].TextureArg0;
                    if (materialIndex >= 0 && materialIndex < mat.Length)
                    {
                        if (mat[materialIndex] == null)
                        {
                            targetMat.Add(new Material(Resources.Load<Shader>("Mobile-Diffuse")));
                            addIflComponent = true;
                            iflParam = materialIndex;
                            iflMatIndex = targetMat.Count - 1;
                        }
                        else
                            targetMat.Add(mat[materialIndex]);
                    }
                    else
                    {
                        //占位材质，代表原文件里用到了序号<0的材质(即空材质)，这里使用默认材质代替,一般武器加载不会触发这里，但是有极个别情况
                        Material defaults = new Material(Resources.Load<Shader>("Mobile-Diffuse"));
                        defaults.name = string.Format("{0}_{1:D2}", objMesh.name, materialIndex);
                        targetMat.Add(defaults);
                    }
                }
            }
            MeshRenderer mr = objMesh.AddComponent<MeshRenderer>();
            MeshFilter mf = objMesh.AddComponent<MeshFilter>();
            mf.mesh = w;
            mf.mesh.colors = col.ToArray();
            mf.mesh.normals = nor.ToArray();
            mf.mesh.RecalculateBounds();
            mf.mesh.RecalculateNormals();

            mr.materials = targetMat.ToArray();
            objMesh.transform.SetParent(transform);
        }
    }


}
