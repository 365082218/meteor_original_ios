using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;


public class FMCLoader:Singleton<FMCLoader>
{
    SortedDictionary<string, FMCFile> FmcFile = new SortedDictionary<string, FMCFile>();
    public FMCFile Load(string file)
    {
        string file_no_ext = file;
        file += ".fmc";
        FMCFile fi = null;
        if (FmcFile.ContainsKey(file))
            return FmcFile[file];
        if (CombatData.Ins.Chapter != null) {
            string path = CombatData.Ins.Chapter.GetResPath(FileExt.Fmc, file_no_ext);
            if (!string.IsNullOrEmpty(path)) {
                fi = new FMCFile();
                fi = fi.LoadFile(path);
                if (fi != null) {
                    FmcFile.Add(file, fi);
                    return fi;
                }
            }
        }
        fi = new FMCFile();
        fi = fi.LoadFile(file);
        if (fi != null)
            FmcFile.Add(file, fi);
        return fi;
    }

    public FMCFile Load(TextAsset asset)
    {
        if (FmcFile.ContainsKey(asset.name))
            return FmcFile[asset.name];
        FMCFile fi = new FMCFile();
        FMCFile ret = fi.LoadFile(asset.bytes);
        if (ret != null)
            FmcFile.Add(asset.name, ret);
        return ret;
    }

    public void Clear()
    {
        FmcFile.Clear();
    }
}

public class FMCFrame
{
    public int frameIdx;
    public SortedDictionary<int, Vector3> pos = new SortedDictionary<int, Vector3>();//每个物件的坐标
    public SortedDictionary<int, Quaternion> quat = new SortedDictionary<int, Quaternion>();//每个物件的旋转
}

public class FMCFile
{
    public int Fps;
    public int Frames;
    public int ScemeObjCount;
    public int DummyObjCount;
    public SortedDictionary<int, FMCFrame> frame = new SortedDictionary<int, FMCFrame>();

    public FMCFile LoadFile(byte [] body)
    {
        MemoryStream ms = new MemoryStream(body);
        StreamReader text = new StreamReader(ms);
        FMCFrame f = null;
        int fNum = -1;
        int objectIndex = -1;
        while (!text.EndOfStream)
        {
            string obj = text.ReadLine();
            string[] keyValue = obj.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (keyValue[0] == "#")
                continue;
            if (keyValue[0] == "SceneObjects" && keyValue[2] == "DummeyObjects")
            {
                ScemeObjCount = int.Parse(keyValue[1]);
                DummyObjCount = int.Parse(keyValue[3]);
                continue;
            }

            if (keyValue[0] == "FPS" && keyValue[2] == "Frames")
            {
                Fps = int.Parse(keyValue[1]);
                Frames = int.Parse(keyValue[3]);
                continue;
            }

            if (keyValue[0] == "frame")
            {
                fNum++;
                f = new FMCFrame();
                f.frameIdx = int.Parse(keyValue[1]);
                frame.Add(fNum, f);
                objectIndex = 0;
            }

            if (keyValue[0] == "t" && keyValue[4] == "q")
            {
                //objectIndex++;
                f.pos.Add(objectIndex, new Vector3(float.Parse(keyValue[1]), float.Parse(keyValue[3]), float.Parse(keyValue[2])));
                f.quat.Add(objectIndex, new Quaternion(-float.Parse(keyValue[6]), -float.Parse(keyValue[8]), -float.Parse(keyValue[7]), float.Parse(keyValue[5])));
                //quat.w = float.Parse(keyValue[1]);
                //quat.x = -float.Parse(keyValue[2]);
                //quat.y = -float.Parse(keyValue[4]);
                //quat.z = -float.Parse(keyValue[3]);
                objectIndex++;
            }
        }
        return this;
    }
    public FMCFile LoadFile(string file)
    {
        byte[] body = null;
        TextAsset asset = Resources.Load<TextAsset>(file);
        if (asset == null) {
            if (File.Exists(file)) {
                body = File.ReadAllBytes(file);
            }
        } else {
            body = asset.bytes;
        }
        if (body == null)
            return null;
        return LoadFile(body);
    }
}
