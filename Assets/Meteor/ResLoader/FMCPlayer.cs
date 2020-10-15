using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;


public class MiniPose
{
    public int idx;
    public int start;
    public int end;
}

public class FMCPoseLoader:Singleton<FMCPoseLoader>
{
    public void Clear() {
        fmcPose.Clear();
    }
    public SortedDictionary<string, FMCPose> fmcPose = new SortedDictionary<string, FMCPose>();
    public FMCPose LoadPose(string file)
    {
        string file_no_ext = file;
        file += ".pos";
        if (fmcPose.ContainsKey(file))
            return fmcPose[file];
        byte[] body = null;
        if (CombatData.Ins.Chapter != null) {
            string path = CombatData.Ins.Chapter.GetResPath(FileExt.Pos, file_no_ext);
            if (!string.IsNullOrEmpty(path)) {
                if (File.Exists(path)) {
                    body = File.ReadAllBytes(path);
                }
            }
        }
        if (body == null) {
            TextAsset asset = Resources.Load<TextAsset>(file);
            if (asset == null)
                return null;
            body = asset.bytes;
        }
        FMCPose pose = new FMCPose();
        string text = System.Text.Encoding.ASCII.GetString(body);
        string[] pos = text.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        int idx = 0;
        MiniPose po = null;
        for (int i = 0; i < pos.Length; i++)
        {
            string[] eachobj = pos[i].Split(new char[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (eachobj[0] == "pose")
            {
                po = new MiniPose();
                po.idx = idx;
                pose.pos.Add(po);
                idx++;
            }
            if (eachobj[0] == "start")
            {
                if (po != null)
                    po.start = int.Parse(eachobj[1]);
            }
            if (eachobj[0] == "end")
            {
                if (po != null)
                    po.end = int.Parse(eachobj[1]);
            }
        }
        fmcPose.Add(file, pose);
        return pose;
    }
}

public class FMCPose
{
    public List<MiniPose> pos = new List<MiniPose>();
}

public class FMCPlayer :NetBehaviour {
    public FMCFile frames;
    FMCPose pose;
    CombineChildren combine;
    public string fmcFile;
    float f = 0;
    protected new void Awake()
    {
        base.Awake();
    }
    protected new void OnDestroy()
    {
        base.OnDestroy();
    }
    // Use this for initialization
    void Start () {
        if (frames != null)
            f = 1.0f / frames.Fps;
        combine = GetComponent<CombineChildren>();
    }

    public void ChangeFrame(int fp)
    {
        currentFrame = fp;
    }

    bool looped = false;
    float tick;
    // Update is called once per frame
    public override void NetUpdate () {
        int oldframe = currentFrame;
        if (frames == null || state == 1)
            return;
        tick += FrameReplay.deltaTime;
        if (tick >= f)//不允许跳帧
        {
            currentFrame += 1;
            if (currentFrame >= end)
            {
                if (looped)
                {
                    currentFrame = start;
                }
                else
                {
                    state = 3;
                    currentFrame = end;
                }
            }
            tick -= f;
        }

        int i = 0;
        foreach (var each in frames.frame[currentFrame].pos)
        {
            if (transform.childCount <= i)
            {
                //Debug.LogError(gameObject.name + " fmcplayer error");
                continue;
            }
            if (currentFrame == end)
                transform.GetChild(i).localPosition = each.Value;
            else
                transform.GetChild(i).localPosition = Vector3.Lerp(each.Value, frames.frame[currentFrame + 1].pos[each.Key], tick / f);
            i++;
        }

        i = 0;
        foreach (var each in frames.frame[currentFrame].quat)
        {
            if (transform.childCount <= i)
            {
                //Debug.LogError(gameObject.name + " fmcplayer error");
                continue;
            }
            if (currentFrame == end)
                transform.GetChild(i).localRotation = each.Value;
            else
                transform.GetChild(i).localRotation = Quaternion.Slerp(each.Value, frames.frame[currentFrame + 1].quat[each.Key], tick / f);
            i++;
        }
        if (oldframe != currentFrame && combine != null)
            combine.UpdateMesh();
    }

    int start;
    int end;
    int currentFrame;
    int currentPosIdx;
    public int state;//1还未播放 2开始播放 3播放到最后一帧并停止
    public void ChangePose(int i, int loop)
    {
        if (pose != null && pose.pos.Count > i)
        {
            currentPosIdx = i;
            start = pose.pos[i].start;
            end = pose.pos[i].end;
            currentFrame = start;
            looped = (loop == 1);
            state = 2;
        }
    }

    public void Init(TextAsset asset)
    {
        int i = asset.name.IndexOf('.');
        fmcFile = asset.name.Substring(0, i);
        frames = FMCLoader.Ins.Load(asset);
        pose = FMCPoseLoader.Ins.LoadPose(fmcFile);
        state = 1;
    }

    public void Init(string file, FMCFile f = null)
    {
        fmcFile = file;
        frames = f != null ? f : FMCLoader.Ins.Load(fmcFile);
        pose = FMCPoseLoader.Ins.LoadPose(fmcFile);
        state = 1;
        //ChangePose(0, 0);
    }

    public int GetPose()
    {
        return currentPosIdx;
    }

    public int GetStatus()
    {
        return state;
    }

    //GameObject MeshCombined;
    //MeshFilter mf;
    //MeshRenderer mr;
    //List<GameObject> mesh = new List<GameObject>();
    //public void CombineMesh()
    //{

        //合并使用同一个材质球得网格,大部分就是瓶瓶罐罐，木箱子铁箱子.
        //MeshRenderer[] renderers = transform.GetComponentsInChildren<MeshRenderer>();
        //for (int i = 0; i < renderers.Length; i++)
        //{
        //    if (renderers[i].sharedMaterials.Length == 1)
        //    {

        //    }
        //}
        //MeshCombined = new GameObject();
        //MeshCombined.name = name;
        //MeshCombined.transform.SetParent(transform);
        //MeshCombined.transform.localPosition = Vector3.zero;
        //MeshCombined.transform.localRotation = Quaternion.identity;
        //MeshCombined.transform.localScale = Vector3.one;
        //mr = MeshCombined.AddComponent<MeshRenderer>();
        //mf = MeshCombined.AddComponent<MeshFilter>();
    //}

    //void UpdateCombinedMesh()
    //{
        //for (int i = 0; i < mesh.Count; i++)
        //{

        //}
        //mf.sharedMesh;
    //}
}
