using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;

//流星特效加载-声音模拟OK
/*
 * 11字节版本号
 * 4字节子特效数
 * 每个子特效400字节
 * 特效种类有
 * BOX			立体模型、
 AUDIO			声音指向、Audio_3音效 Audio_0 UI音效 Audio_15 多一个字节？
 PLANE			平面模型、
 DONUT			圆环模型、
 MODEL			物品模型、
 SPHERE			球体模型、
 PARTICLE		颗粒模型、
 CYLINDER		圆柱模型、
 BILLBOARD		连接模板、公告板
 AUDIO-400/401字节内部 Audio_15多一字节
 SPHERE 405字节

 Audio解析
 16字节 特效类型字符串
 4字节 特效类型下的子类型字符串长度，比如音频AUDIO作为大类，内部还有Audio_3此类子特效。
 上一个长度那么长的字符串
 16字节未知内容
 4字节参数长度
 上一个长度那么长的字符串 可能是运动
 4字节参数长度
 上一个长度那么长的字符串 绑定到骨骼
 4字节参数长度
 上一个长度那么长的字符串 贴图

尾部20-16名称长度
尾部16-1音频名称

BILLBOARD解析
16字节 类型
4字节长度
长度×1字节对象名
12字节空
4字节空
4字节长度
长度×1字节  跟着谁
4字节长度
长度×1字节 骨骼或者特定对象
4字节长度
长度×1 贴图
空4字节
14×4字节的float
空5字节
4×8
{
0 - ? 是否显示/或者加载
1 - ? 
2 - ? float值 四字节 V流动
3 - ? float值 四字节 U流动
4 - ? 
5 - 反色 【1 透明 颜色+深 带一点高亮】【2 反色 透明混合】 【3去掉ALPHA通道，整体使用透明度】[4 虑色+APLHA 高光]
6 - ?
7 - 帧数
}
每个子特效前 有其类型，特效名称 等一些信息，这些信息长度可变，完毕后是16字节的空出。然后4字节一个长度，后面跟这个长度的一个文本串，这个串和特效跟随谁缓动有关系
之后又有一个长度，一个长度长的串，也是特效和指定点的跟随关系，大部分是不设置子层级，不然他的坐标比较难调，应该是都在顶级，然后跟指定绑定点有一个位置，旋转，上的关系。
，然后空4字节，然后 有56字节头部，然后空5字节，32个字节的，特效通用属性定义，如上所示，8个标志位，代表子特效的通用shader叠加方式 uv帧数等。
每段变化 帧数×112字节 每个112字节 有效的56字节 含义是  帧起始时间/帧数 = 4 V3 = 12 Quat(x,y,z,w) = 16 Scale(x,y,z) = 12 color(grba) = 16.后面的自己暂时不知道意思
每个特效末尾，会带有一定的尾部信息，有的表述特效的基础缩放，指定是在这个缩放下，叠加每次在特效帧里指定的缩放关系. 
12字节 大小

 ROOT			基础
 FLAG			标号
 Dummey			虚构-这个会跟住人
 Channel		声道
 Character		角色
 */

//bitconverter巨卡，不要用
public class SfxFile
{
    //给定流，和长度，在长度内找0X00找到返回
    public string readNextString(BinaryReader s, int length, bool fixedSize = true)
    {
        string ret = "";
        if (fixedSize)
        {
            byte[] buff = new byte[length];
            for (int i = 0; i < length; i++)
            {
                byte b = s.ReadByte();
                buff[i] = b;
            }
            ret = I18N.CJK.GB18030Encoding.GetEncoding(950).GetString(buff, 0, length);
            return ret;
        }
        try
        {
            byte[] buff = new byte[length];
            for (int i = 0; i < length; i++)
            {
                byte b = s.ReadByte();
                if (b == 0x00)
                {
                    s.BaseStream.Seek(length - i - 1, SeekOrigin.Current);
                    ret = I18N.CJK.GB18030Encoding.GetEncoding(950).GetString(buff, 0, i);
                    return ret;
                }
                buff[i] = b;
            }
        }
        catch
        {
            return null;
        }
        return null;
    }
    public string strfile;
    public List<SfxEffect> effectList = new List<SfxEffect>();
    public bool ParseFile(string file)
    {
        strfile = file;
        TextAsset asset = Resources.Load<TextAsset>(file);
        if (asset == null)
            asset = Resources.Load<TextAsset>(file + ".ef");
        if (asset == null)
        {
            SFXLoader.Instance.Miss++;
            Debug.LogError("sfx:" + file + " missed");
            return false;
        }
        int offset = 0;
        if (asset != null && asset.bytes == null)
        {
            Debug.LogError("asset or it content is null file is " + file);
            return false;
        }
        MemoryStream ms = new MemoryStream(asset.bytes);
        BinaryReader reader = new BinaryReader(ms);
        string ver = new string(reader.ReadChars(11));
        //System.Text.Encoding.UTF8.GetString(asset.bytes, offset, 11);
        //offset += 11;
        int effectNum = reader.ReadInt32();
        //int effectNum = System.BitConverter.ToInt32(asset.bytes, offset);
        //offset += 4;
        //只读一个特效的。
        //if (effectNum != 1)
        //    return false;
        for (int i = 0; i < effectNum; i++)
        {
            SfxEffect sfx = new SfxEffect();
            sfx.EffectType = readNextString(reader, 16, false);
            int len = reader.ReadInt32();
            sfx.EffectName = readNextString(reader, len);
            reader.BaseStream.Seek(12, SeekOrigin.Current);
            sfx.localSpace = reader.ReadInt32();
            {
                int headlen = 4;
                int haveBone = reader.ReadInt32();
                sfx.Bone0 = readNextString(reader, haveBone);
                headlen += haveBone;
                haveBone = reader.ReadInt32();
                sfx.Bone1 = readNextString(reader, haveBone);
                headlen += 4;
                headlen += haveBone;
                haveBone = reader.ReadInt32();
                sfx.Texture = readNextString(reader, haveBone);
                reader.BaseStream.Seek(60, SeekOrigin.Current);//这60字节分别是 环境光 镜面光 漫反射 自发光 还有未知的3字节
                headlen += 4;
                headlen += haveBone;
                reader.BaseStream.Seek(5, SeekOrigin.Current);
                sfx.Active = reader.ReadInt32();
                sfx.Hidden = reader.ReadInt32();
                sfx.uSpeed = reader.ReadSingle();
                sfx.vSpeed = reader.ReadSingle();
                sfx.BlendType = reader.ReadInt32();
                sfx.Unknown2 = reader.ReadInt32();
                sfx.Unknown3 = reader.ReadInt32();
                sfx.FrameCnt = reader.ReadInt32();
                for (int y = 0; y < sfx.FrameCnt; y++)
                {
                    EffectFrame frame = new EffectFrame();
                    frame.startTime = reader.ReadSingle();// asset.bytes, offset);//可能是int可能是float
                    //offset += 4;
                    frame.pos.x = reader.ReadSingle();//可能是int可能是float
                    offset += 4;
                    frame.pos.z = reader.ReadSingle();//可能是int可能是float
                    offset += 4;
                    frame.pos.y = reader.ReadSingle();//可能是int可能是float
                    offset += 4;
                    frame.quat.x = -reader.ReadSingle();//可能是int可能是float
                    offset += 4;
                    frame.quat.z = -reader.ReadSingle();//可能是int可能是float
                    offset += 4;
                    frame.quat.y = -reader.ReadSingle();//可能是int可能是float
                    offset += 4;
                    frame.quat.w = reader.ReadSingle();//可能是int可能是float
                    offset += 4;
                    frame.scale.x = reader.ReadSingle();//可能是int可能是float
                    offset += 4;
                    frame.scale.z = reader.ReadSingle();//可能是int可能是float
                    offset += 4;
                    frame.scale.y = reader.ReadSingle();//可能是int可能是float
                    offset += 4;
                    frame.colorRGB.r = reader.ReadSingle();//可能是int可能是float
                    offset += 4;
                    frame.colorRGB.g = reader.ReadSingle();//可能是int可能是float
                    offset += 4;
                    frame.colorRGB.b = reader.ReadSingle();//可能是int可能是float
                    offset += 4;
                    frame.colorRGB.a = reader.ReadSingle();//4 15*4 = 60 
                    offset += 4;
                    
                    //offset += 52;//后面52字节暂时不知道什么意思.13个float
                    for (int j = 0; j < 13; j++)
                    {
                        frame.TailFlags[j] = reader.ReadSingle();
                    }
                    sfx.frames.Add(frame);
                }

                int TailSectionLength = reader.ReadInt32();
                if (sfx.EffectType == "AUDIO")
                {
                    int regionCnt = reader.ReadInt32();
                    string sBone = readNextString(reader, regionCnt);
                    sfx.audioLoop = reader.ReadInt32();
                    sfx.Tails[0] = sBone;
                }
                else if (sfx.EffectType == "BILLBOARD" || sfx.EffectType == "PLANE")
                {
                    sfx.origScale.x = reader.ReadSingle();
                    offset += 4;
                    sfx.origScale.y = reader.ReadSingle();
                    offset += 4;
                }
                else if (sfx.EffectType == "CYLINDER")
                {
                    sfx.origAtt.x = reader.ReadSingle();//底部半径
                    offset += 4;
                    sfx.origAtt.y = reader.ReadSingle();//顶部半径
                    offset += 4;
                    sfx.origScale.x = reader.ReadSingle();//高度
                    offset += 4;
                    sfx.origScale.y = reader.ReadSingle();//90
                    offset += 4;
                    sfx.origScale.z = reader.ReadSingle();//450
                    offset += 4;
                    //sfx.TailLength = 24;
                }
                else if (sfx.EffectType == "BOX")
                {
                    sfx.origScale.x = reader.ReadSingle();//System.BitConverter.ToSingle(asset.bytes, offset);//可能是int可能是float
                    offset += 4;
                    sfx.origScale.y = reader.ReadSingle();//System.BitConverter.ToSingle(asset.bytes, offset);//可能是int可能是float
                    offset += 4;
                    sfx.origScale.z = reader.ReadSingle();//System.BitConverter.ToSingle(asset.bytes, offset);//可能是int可能是float
                    offset += 4;
                    //sfx.TailLength = 16;
                }
                else if (sfx.EffectType == "PARTICLE")
                {
                    //reader.BaseStream.Seek(4, SeekOrigin.Current);
                    //读4个字节长度，读100个字节，在读这个长度 最后面就是3个变长串
                    //就是100 +X+尾部变长。
                    int endRegionCnt = TailSectionLength;
                    //sfx.TailLength = 100 + endRegionCnt;
                    reader.BaseStream.Seek(96 + endRegionCnt, SeekOrigin.Current);//到终点
                    //offset += sfx.TailLength;
                    //后面还有3个变长串，可为空的
                    int regionCnt = reader.ReadInt32();// (asset.bytes, offset);
                    //offset += 4;
                    //sfx.TailLength += 4;
                    string sBone = readNextString(reader, regionCnt);
                    //string sBone = System.Text.Encoding.UTF8.GetString(asset.bytes, offset, regionCnt);
                    sfx.Tails[0] = sBone;
                    //offset += regionCnt;
                    //sfx.TailLength += regionCnt;
                    regionCnt = reader.ReadInt32();
                    //offset += 4;
                    //sfx.TailLength += 4;
                    sBone = readNextString(reader, regionCnt);//  System.Text.Encoding.UTF8.GetString(asset.bytes, offset, regionCnt);
                    sfx.Tails[1] = sBone;
                    //offset += regionCnt;
                    //sfx.TailLength += regionCnt;
                    regionCnt = reader.ReadInt32();// (asset.bytes, offset);
                    offset += 4;
                    //sfx.TailLength += 4;
                    sBone = readNextString(reader, regionCnt);// System.Text.Encoding.UTF8.GetString(asset.bytes, offset, regionCnt);
                    sfx.Tails[2] = sBone;
                    offset += regionCnt;
                    //sfx.TailLength += regionCnt;
                }
                else if (sfx.EffectType == "DONUT")
                {
                    reader.BaseStream.Seek(24, SeekOrigin.Current);
                    //sfx.TailLength = 28;
                }
                else if (sfx.EffectType == "DRAG")
                {
                    //sfx.TailLength += 8;
                    offset += 8;
                    reader.BaseStream.Seek(4, SeekOrigin.Current);
                    int len4 = reader.ReadInt32();// (asset.bytes, offset);
                    offset += 4;
                    //sfx.TailLength += 4;
                    sfx.Tails[0] =  readNextString(reader, len4);// (asset.bytes, offset, len4);
                    //sfx.TailLength += len4;
                    offset += len4;
                    len4 = reader.ReadInt32();// System.BitConverter.ToInt32(asset.bytes, offset);
                    offset += 4;
                    //sfx.TailLength += 4;
                    sfx.Tails[1] = readNextString(reader, len4);// System.Text.Encoding.UTF8.GetString(asset.bytes, offset, len4);
                    //sfx.TailLength += len4;
                    offset += len4;
                }
                else if (sfx.EffectType == "MODEL")
                {
                    //4 + 4 + 长度
                    //sfx.TailLength += 4;
                    int len4 = reader.ReadInt32();// System.BitConverter.ToInt32(asset.bytes, offset);
                    offset += 4;
                    //sfx.TailLength += 4;

                    sfx.Tails[0] = readNextString(reader, len4);// System.Text.Encoding.UTF8.GetString(asset.bytes, offset, len4);
                    //sfx.TailLength += len4;
                }
                else if (sfx.EffectType == "SPHERE")
                {
                    sfx.SphereScale = reader.ReadSingle();
                    sfx.sphereAttr.x = reader.ReadSingle();
                    sfx.sphereAttr.y = reader.ReadSingle();
                    sfx.sphereAttr.z = reader.ReadSingle();
                    sfx.sphereAttr.w = reader.ReadSingle();
                    //reader.BaseStream.Seek(16, SeekOrigin.Current);
                }
                else
                {
                    Debug.LogError(sfx.EffectType + " can not parse: file " + file);
                }
                effectList.Add(sfx);
            }
        }
        ms.Close();
        reader.Close();
        return true;
    }

    //public SfxEffectPlay Play(Vector3 position, bool once)
    //{
    //    if (effectList.Count == 0)
    //        return null;
    //    GameObject obj = new GameObject("sfxeffect");
    //    obj.transform.position = position;
    //    obj.transform.localScale = Vector3.one;
    //    obj.transform.rotation = Quaternion.identity;
    //    obj.layer = 
    //    SFXEffectPlay effectContainer = obj.AddComponent<SFXEffectPlay>();
    //    effectContainer.Load(this, once);
    //    return effectContainer;
    //}

    public SFXEffectPlay Play(GameObject obj, bool once)
    {
        if (effectList.Count == 0)
            return null;
        SFXEffectPlay effectContainer = obj.AddComponent<SFXEffectPlay>();
        effectContainer.Load(this, once);
        return effectContainer;
    }

    public SFXEffectPlay Play(CharacterLoader character, float timePlayed = 0.0f)
    {
        if (effectList.Count == 0)
            return null;
        SFXEffectPlay effectContainer = character.gameObject.AddComponent<SFXEffectPlay>();
        effectContainer.Load(this, timePlayed);
        return effectContainer;
    }
}
[System.Serializable]
public class EffectFrame
{
    public float startTime;
    public Vector3 pos;
    public Quaternion quat;
    public Vector3 scale;
    public Color colorRGB;
    public float[] TailFlags;//9透明度
    public EffectFrame()
    {
        pos = Vector3.zero;
        quat = Quaternion.identity;
        scale = Vector3.one;
        colorRGB = Color.white;
        TailFlags = new float[13];
    }
}
[System.Serializable]
public class SfxEffect
{
    public int localSpace;//相当于是否设置其为父级。
    public string EffectType;
    public string EffectName;
    public string Bone0;
    public string Bone1;
    public string Texture;
    public int Active;
    public int Hidden;//是否隐藏
    public float uSpeed;
    public float vSpeed;
    public int Unknown2;
    public int BlendType;
    public int Unknown3;
    public int FrameCnt;
    public int audioLoop;
    public float SphereScale;
    public List<EffectFrame> frames = new List<EffectFrame>();
    public Vector3 origScale = Vector3.one;//这个参数决定了网格生成的参数，类似cylinder,顶部半径，底部半径，以及高度都由此设置
    public Vector2 origAtt = Vector2.zero;//各自的参数设置 顶部半径，底部半径
    public Vector4 sphereAttr = Vector4.zero;
    //粒子系统使用的
    public string[] Tails = new string[3];
}

public class SFXLoader :Singleton<SFXLoader>{
    Dictionary<string, SfxFile> EffectList = new Dictionary<string, SfxFile>();
    public Dictionary<string, SfxFile> Effect { get { return EffectList; } }
    //
    bool initList = false;
    List<string> key;
    int effIndex = 0;
    public void PlayNextEffect()
    {
        if (!initList)
        {
            key = EffectList.Keys.ToList();
            for (int i = 0; i < key.Count; i++)
            {
                if (key[i] == "GetItem.ef")
                    effIndex = i + 1;
            }
            initList = true;
        }
        PlayEffect(key[effIndex], MeteorManager.Instance.LocalPlayer.GetComponent<CharacterLoader>());
        effIndex++;
    }

    public SFXEffectPlay PlayEffect(int idx, GameObject obj, bool once = true)
    {
        if (Eff.Length > idx)
            return PlayEffect(Eff[idx], obj, once);
        return null;
    }

    //一些环境特效，例如风之类的音效.
    public SFXEffectPlay PlayEffect(string file, GameObject obj, bool once = false)
    {
        if (!EffectList.ContainsKey(file))
        {
            SfxFile f = new SfxFile();
            try
            {
                if (f.ParseFile(file))
                {
                    EffectList.Add(file, f);
                    //Debug.LogError("effect file:" + file);
                    return f.Play(obj, once);
                }
                else
                    Debug.LogError(file + " parse error445");
            }
            catch
            {
                Debug.LogError(file + " parse error449");
            }

        }
        else
        {
            //Debug.LogError("effect file:" + file + " 455");
            return EffectList[file].Play(obj, once);
        }
        return null;
    }
    //target描述，此特效的主调者
    //timePlayed用于同步动作和特效。快速出招时，特效要加一个已经播放时间，否则特效跟不上动作的播放步骤
    public SFXEffectPlay PlayEffect(string file, CharacterLoader target, float timePlayed = 0.0f)
    {
        if (!EffectList.ContainsKey(file))
        {
            SfxFile f = new SfxFile();
            try
            {
                if (f.ParseFile(file))
                {
                    EffectList.Add(file, f);
                    //Debug.LogError("effect file:" + file);
                    return f.Play(target, timePlayed);
                }
            }
            catch
            {
                Debug.LogError(file + " parse error");
            }

        }
        else
        {
            //Debug.LogError("effect file:" + file);
            return EffectList[file].Play(target, timePlayed);
        }
        return null;
    }

    public void LoadEffect(string file)
    {
        if (!EffectList.ContainsKey(file))
        {
            SfxFile f = new SfxFile();
            try
            {
                if (f.ParseFile(file))
                {
                    //过滤掉声音的
                    //bool add = false;
                    //for (int i = 0; i < f.effectList.Count; i++)
                    //{
                    //    if (f.effectList[i].EffectType != "AUDIO")
                    //    {
                    //        add = true;
                    //        break;
                    //    }
                    //}
                    //if (add)
                        EffectList.Add(file, f);
                }
            }
            catch (Exception exp)
            {
                Debug.LogError(file + " parse error" + exp.StackTrace);
                throw new Exception("effect load failed");
            }
            
        }
    }

    public int TotalSfx = 0;
    public int Miss = 0;
    public string[] Eff;
    public void Init()
    {
        TextAsset list = Resources.Load<TextAsset>("effect.lst");
        Eff = list.text.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        //try
        //{
        //    for (int i = 0; i < Eff.Length; i++)
        //    {
        //        LoadEffect(Eff[i]);
        //    }
        //}
        //catch
        //{

        //}
        TotalSfx = Eff.Length;
    }
}
