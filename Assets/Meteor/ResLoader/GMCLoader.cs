using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class GMCLoader:Singleton<GMCLoader>
{
    SortedDictionary<string, GMCFile> GMCFile = new SortedDictionary<string, global::GMCFile>();
    public GMCFile Load(string file)
    {
        string file_no_ext = file;
        file += ".gmc";
        if (GMCFile.ContainsKey(file))
            return GMCFile[file];
        GMCFile f = null;
        if (CombatData.Ins.Chapter != null) {
            string path = CombatData.Ins.Chapter.GetResPath(FileExt.Gmc, file_no_ext);
            if (!string.IsNullOrEmpty(path)) {
                f = new GMCFile();
                f.Load(path);
                if (f.errno != ParseError.None)
                    return null;
                GMCFile[file] = f;
                return f;
            }
        }
        f = new GMCFile();
        f.Load(file);
        if (f.errno != ParseError.None)
            return null;
        GMCFile[file] = f;
        return f;
    }

    public void Clear()
    {
        GMCFile.Clear();
    }
}

//设计一个Shader，给出这些段作为参数，在材质球里填写这些属性段的值
public struct ShaderUnit
{
    public int TextureArg0;
    public string TextureArg1;
    public int TwoSideArg0;
    public string BlendArg0;
    public string BlendArg1;
    public string BlendArg2;
    public float OpaqueArg0;
}

public struct MeshUnit
{
    public string name;
    public float[,] Vertices;
    public float[,] Faces;
    public List<MeshVert> vertices;
    public List<MeshFace> faces;
}

public struct MeshVert
{
    public Vector3 pos ;
    public Vector3 normal;
    public Color color;
    public Vector2 uv;
}
public struct MeshFace
{
    public int material;
    public List<int> triangle;
}

public class GMCFile
{
    public int TexturesCount;
    public int ShaderCount;
    public int SceneObjectsCount;
    public int DummeyObjectsCount;
    public int VerticesCount;
    public int FacesCount;
    public List<string> TexturesNames = new List<string>();

    public ShaderUnit [] shader;
    public MeshUnit[] mesh;
    public void Export()
    {
        for (int i = 0; i < mesh.Length; i++)
        {
            if (mesh[i].vertices == null)
                mesh[i].vertices = new List<MeshVert>();
            if (mesh[i].faces == null)
                mesh[i].faces = new List<MeshFace>();
            mesh[i].vertices.Clear();
            mesh[i].faces.Clear();
            //部分文件不符合规则.数量与文件不匹配时
            if (mesh[i].Vertices == null || mesh[i].Faces == null)
                continue;
            for (int j = 0; j < mesh[i].Vertices.Length / 15; j++)
            {
                MeshVert m = new MeshVert();
                m.pos = new Vector3(mesh[i].Vertices[j,0], mesh[i].Vertices[j, 2], mesh[i].Vertices[j, 1]);
                m.normal = new Vector3(mesh[i].Vertices[j, 4], mesh[i].Vertices[j, 5], mesh[i].Vertices[j, 6]);
                m.color = new Color(mesh[i].Vertices[j, 8], mesh[i].Vertices[j, 9], mesh[i].Vertices[j, 10], mesh[i].Vertices[j, 11]);
                //m.color = new Color(1,1,1,1);
                m.uv = new Vector2(mesh[i].Vertices[j, 13], mesh[i].Vertices[j, 14]);
                //m.uv = new Vector2(mesh[i].Vertices[j, 13], mesh[i].Vertices[j, 14]);
                mesh[i].vertices.Add(m);
            }
            for (int j = 0; j < mesh[i].Faces.Length / 7; j++)
            {
                MeshFace mf = new MeshFace();
                mf.material = (int)mesh[i].Faces[j, 0];
                mf.triangle = new List<int> { (int)mesh[i].Faces[j, 1], (int)mesh[i].Faces[j, 3], (int)mesh[i].Faces[j, 2] };
                mesh[i].faces.Add(mf);
            }
        }
    }

    public ParseError errno = ParseError.None;
    public void Load(string file)
    {
        byte[] body = null;
        TextAsset asset = Resources.Load<TextAsset>(file);
        if (asset == null) {
            if (File.Exists(file)) {
                body = File.ReadAllBytes(file);
            }
        } else
            body = asset.bytes;
        if (body == null) {
            errno = ParseError.Miss;
            return;
        }
        MemoryStream ms = new MemoryStream(body);
        TextReader textReader = new System.IO.StreamReader(ms);
        string text = string.Empty;
        string[] array;
        do
        {
            text = textReader.ReadLine().Trim().Replace('\t', ' ');
            array = text.Split(new char[] { ' '}, System.StringSplitOptions.RemoveEmptyEntries);
        }
        while (!(array[0] == "Textures"));
        this.TexturesCount = int.Parse(array[1]);
        while ("{" != textReader.ReadLine().Trim().Substring(0, 1))
        {
        }
        text = textReader.ReadLine().Trim();
        while (text != "}")
        {
            this.TexturesNames.Add(text);
            text = textReader.ReadLine().Trim();
        }
        text = textReader.ReadLine().Trim().Replace('\t', ' ');
        array = text.Split(new char[]{ ' '}, System.StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == "Shaders" || array[i] == "shaders")
            {
                this.ShaderCount = int.Parse(array[i + 1]);
                break;
            }
        }
        this.shader = new ShaderUnit[ShaderCount];
        for (int j = 0; j < this.ShaderCount; j++)
        {
            textReader.ReadLine();
            textReader.ReadLine();
            text = textReader.ReadLine();
            array = text.Trim().Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            shader[j].TextureArg0 = int.Parse(array[1]);
            shader[j].TextureArg1 = array[2];
            text = textReader.ReadLine();
            array = text.Trim().Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            shader[j].TwoSideArg0 = int.Parse(array[1]);
            text = textReader.ReadLine();
            array = text.Trim().Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            shader[j].BlendArg0 = array[1];
            shader[j].BlendArg1 = array[2];
            shader[j].BlendArg2 = array[3];
            text = textReader.ReadLine();
            array = text.Trim().Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            shader[j].OpaqueArg0 = float.Parse(array[1]);
            textReader.ReadLine();
        }
        text = textReader.ReadLine().Trim();
        array = text.Trim().Replace('\t', ' ').Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        SceneObjectsCount = int.Parse(array[1]);
        DummeyObjectsCount = int.Parse(array[3]);
        text = textReader.ReadLine().Trim();
        array = text.Trim().Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        VerticesCount = int.Parse(array[1]);
        FacesCount = int.Parse(array[3]);
        mesh = new MeshUnit[SceneObjectsCount];
        //Debug.Log("face:" + FacesCount + " object count:" + SceneObjectsCount);
        for (int k = 0; k < this.SceneObjectsCount; k++)
        {
            //Debug.Log("read object index:" + k);
            text = textReader.ReadLine();
            if (text == null)
            {
                break;
            }
            text = text.Trim().Replace('\t', ' ');
            array = text.Trim().Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            this.mesh[k].name = array[1];
            textReader.ReadLine();
            text = textReader.ReadLine().Trim();
            array = text.Trim().Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            this.mesh[k].Vertices = new float[int.Parse(array[1]), 15];
            this.mesh[k].Faces = new float[int.Parse(array[3]), 7];
            for (int l = 0; l < mesh[k].Vertices.Length / 15; l++)
            {
                text = textReader.ReadLine().Replace('\t', ' ');
                array = text.Trim().Split(new char[]{' '}, System.StringSplitOptions.RemoveEmptyEntries);
                for (int m = 0; m < 15; m++)
                {
                    float num;
                    try
                    {
                        bool flag = float.TryParse(array[m + 1], out num);
                        if (flag)
                        {
                            this.mesh[k].Vertices[l, m] = num;
                        }
                        else
                        {
                            this.mesh[k].Vertices[l, m] = (float)array[m + 1].ToCharArray()[0];
                        }
                    }
                    catch (Exception exp)
                    {
                        Debug.LogError(exp.Message);
                        Debug.LogError("mesh index = " + k + " row:" + l + " col:" +  m);
                    }
                }
            }
            for (int n = 0; n < this.mesh[k].Faces.Length / 7; n++)
            {
                text = textReader.ReadLine().Trim().Replace("\t", " ");
                array = text.Trim().Split(new char[]{' '}, System.StringSplitOptions.RemoveEmptyEntries);
                //要看下文件是否包含面法线，若不包含，则无需写入
                if (array.Length % 5 == 0)
                {
                    for (int num2 = 0; num2 < 4; num2++)
                    {
                        float num3;
                        bool flag2 = float.TryParse(array[num2 + 1], out num3);
                        if (flag2)
                        {
                            this.mesh[k].Faces[n, num2] = num3;
                        }
                        else
                        {
                            this.mesh[k].Faces[n, num2] = (float)array[num2 + 1].ToCharArray()[0];
                        }
                    }
                }
                else if (array.Length % 8 == 0)
                {
                    for (int num2 = 0; num2 < 7; num2++)
                    {
                        float num3;
                        bool flag2 = float.TryParse(array[num2 + 1], out num3);
                        if (flag2)
                        {
                            this.mesh[k].Faces[n, num2] = num3;
                        }
                        else
                        {
                            this.mesh[k].Faces[n, num2] = (float)array[num2 + 1].ToCharArray()[0];
                        }
                    }
                }
            }
            textReader.ReadLine();
        }
        textReader.Close();
        Export();
    }
}