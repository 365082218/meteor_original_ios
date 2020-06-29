using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class FMCLoader
{
    Dictionary<string, FMCFile> FmcFile = new Dictionary<string, FMCFile>();
    public FMCFile Load(string file)
    {
        file += ".fmc";
        if (FmcFile.ContainsKey(file))
            return FmcFile[file];
        FMCFile fi = new FMCFile();
        FMCFile ret = fi.LoadFile(file);
        if (ret != null)
            FmcFile.Add(file, ret);
        return ret;
    }

    public FMCFile Load(TextAsset asset)
    {
        if (FmcFile.ContainsKey(asset.name))
            return FmcFile[asset.name];
        FMCFile fi = new FMCFile();
        FMCFile ret = fi.LoadFile(asset);
        if (ret != null)
            FmcFile.Add(asset.name, ret);
        return ret;
    }

    public void Refresh()
    {
        FmcFile.Clear();
    }
}

public class FMCFrame
{
    public int frameIdx;
    public Dictionary<int, Vector3> pos = new Dictionary<int, Vector3>();//每个物件的坐标
    public Dictionary<int, Quaternion> quat = new Dictionary<int, Quaternion>();//每个物件的旋转
}

public class FMCFile
{
    public int Fps;
    public int Frames;
    public int ScemeObjCount;
    public int DummyObjCount;
    public Dictionary<int, FMCFrame> frame = new Dictionary<int, FMCFrame>();

    public FMCFile LoadFile(TextAsset asset)
    {
        MemoryStream ms = new MemoryStream(asset.bytes);
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
        TextAsset asset = Resources.Load<TextAsset>(file);
        if (asset == null)
            return null;
        return LoadFile(asset);
    }
}
