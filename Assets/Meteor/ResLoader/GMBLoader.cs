using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using I18N.CJK;
public class GMBLoader:Singleton<GMBLoader>
{
    SortedDictionary<string, GMBFile> GmbFile = new SortedDictionary<string, GMBFile>();
    public GMBFile Load(string file)
    {
        string file_no_ext = file;
        file += ".gmb";
        if (GmbFile.ContainsKey(file))
            return GmbFile[file];
        GMBFile gmb = null;
        if (CombatData.Ins.Chapter != null) {
            string path = CombatData.Ins.Chapter.GetResPath(FileExt.Gmc, file_no_ext);
            if (!string.IsNullOrEmpty(path)) {
                gmb = new GMBFile();
                gmb = gmb.Load(path);
                if (gmb != null) {
                    GmbFile.Add(file, gmb);
                    return gmb;
                }
            }
        }
        gmb = new GMBFile();
        GMBFile gmbOK = gmb.Load(file);
        if (gmbOK != null)
            GmbFile.Add(file, gmbOK);
        return gmbOK;
    }

    public GMBFile Load(TextAsset asset)
    {
        if (GmbFile.ContainsKey(asset.name))
            return GmbFile[asset.name];
        GMBFile gmb = new GMBFile();
        GMBFile gmbOK = gmb.Load(asset);
        if (gmbOK != null)
            GmbFile.Add(asset.name, gmbOK);
        return gmbOK;
    }

    public void Clear()
    {
        GmbFile.Clear();
    }
}

public class GMBFile
{
    public int TexturesCount;
    public int ShaderCount;
    public int SceneObjectsCount;
    public int DummeyObjectsCount;
    public int VerticesCount;
    public int FacesCount;
    public List<string> TexturesNames = new List<string>();
    public ShaderUnit[] shader;
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
            for (int j = 0; j < mesh[i].Vertices.Length / 15; j++)
            {
                MeshVert m = new MeshVert();
                m.pos = new Vector3(mesh[i].Vertices[j, 0], mesh[i].Vertices[j, 2], mesh[i].Vertices[j, 1]);
                m.normal = new Vector3(mesh[i].Vertices[j, 4], mesh[i].Vertices[j, 5], mesh[i].Vertices[j, 6]);
                m.color = new Color(mesh[i].Vertices[j, 8], mesh[i].Vertices[j, 9], mesh[i].Vertices[j, 10], mesh[i].Vertices[j, 11]);
                m.uv = new Vector2(mesh[i].Vertices[j, 13], mesh[i].Vertices[j, 14]);
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
    public void Analyse(MemoryStream ms)
    {
        BinaryReader binaryReader = new BinaryReader(ms);
        binaryReader.ReadBytes("GMDL V1.00".Length);
        this.TexturesCount = binaryReader.ReadInt32();
        for (int i = 0; i < this.TexturesCount; i++)
        {
            int count = binaryReader.ReadInt32();
            string @string = I18N.CJK.GB18030Encoding.GetEncoding(950).GetString(binaryReader.ReadBytes(count));
            this.TexturesNames.Add(@string);
        }
        this.ShaderCount = binaryReader.ReadInt32();
        this.shader = new ShaderUnit[this.ShaderCount];
        for (int j = 0; j < this.ShaderCount; j++)
        {
            this.shader[j].TextureArg0 = binaryReader.ReadInt32();//.ToInt32(binaryReader.ReadBytes(4), 0);
            int count = binaryReader.ReadInt32();
            string @string = Encoding.ASCII.GetString(binaryReader.ReadBytes(count));
            this.shader[j].TextureArg1 = @string;
            this.shader[j].TwoSideArg0 = (int)binaryReader.ReadByte();
            count = binaryReader.ReadInt32();
            @string = Encoding.ASCII.GetString(binaryReader.ReadBytes(count));
            string[] array = @string.Split(new char[]
            {
                    ' '
            });
            this.shader[j].BlendArg0 = array[0];
            this.shader[j].BlendArg1 = array[1];
            this.shader[j].BlendArg2 = array[2];
            this.shader[j].OpaqueArg0 = binaryReader.ReadSingle();
        }
        this.SceneObjectsCount = binaryReader.ReadInt32();
        this.DummeyObjectsCount = binaryReader.ReadInt32();
        this.VerticesCount = binaryReader.ReadInt32();
        this.FacesCount = binaryReader.ReadInt32();
        this.mesh = new MeshUnit[this.SceneObjectsCount];
        for (int k = 0; k < this.SceneObjectsCount; k++)
        {
            int count = binaryReader.ReadInt32();
            //原版是 繁体中文制作.
            string @string = I18N.CJK.GB18030Encoding.GetEncoding(950).GetString(binaryReader.ReadBytes(count));
            this.mesh[k].name = @string;
            int num = binaryReader.ReadInt32();
            int num2 = binaryReader.ReadInt32();
            this.mesh[k].Vertices = new float[num, 15];
            this.mesh[k].Faces = new float[num2, 7];
            for (int l = 0; l < this.mesh[k].Vertices.Length / 15; l++)
            {
                for (int m = 0; m < 15; m++)
                {
                    if (m == 3)
                    {
                        this.mesh[k].Vertices[l, m] = 110f;//"n"=0x6E
                    }
                    else if (m == 7)
                    {
                        this.mesh[k].Vertices[l, m] = 99f;//"c"=0x63
                    }
                    else if (m >= 8 && m <= 11)
                    {
                        this.mesh[k].Vertices[l, m] = (float)binaryReader.ReadByte();
                    }
                    else if (m == 12)
                    {
                        this.mesh[k].Vertices[l, m] = 116f;//"t"=0x74
                    }
                    else
                    {
                        this.mesh[k].Vertices[l, m] = binaryReader.ReadSingle();
                    }
                }
            }
            for (int n = 0; n < this.mesh[k].Faces.Length / 7; n++)
            {
                for (int num3 = 0; num3 < 7; num3++)
                {
                    if (num3 <= 3)
                    {
                        this.mesh[k].Faces[n, num3] = (float)binaryReader.ReadInt32();
                    }
                    else
                    {
                        this.mesh[k].Faces[n, num3] = (float)binaryReader.ReadInt32();
                    }
                }
            }
        }
        binaryReader.Close();
        ms.Close();
        Export();
    }

    public GMBFile Load(string file)
    {
        TextAsset asset = Resources.Load<TextAsset>(file);
        if (asset == null || asset.bytes == null)
            return null;
        MemoryStream ms = new MemoryStream(asset.bytes);
        Analyse(ms);
        return this;
    }

    public GMBFile Load(TextAsset asset)
    {
        MemoryStream ms = new MemoryStream(asset.bytes);
        Analyse(ms);
        return this;
    }
}
