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
    public bool error = false;
    public List<SfxEffect> effectList = new List<SfxEffect>();
    public bool ParseFile(string file)
    {
        strfile = file;
        TextAsset asset = Resources.Load<TextAsset>(string.Format("{0}", file));
        if (asset == null)
        {
            Main.Ins.SFXLoader.Miss++;
            //Debug.LogError(string.Format("sfx:{0} missed", file));
            error = true;
            return false;
        }

        if (asset != null && asset.bytes == null)
        {
            //Debug.LogError(string.Format("asset or it content is null file is {0}", file));
            error = true;
            return false;
        }
        MemoryStream ms = new MemoryStream(asset.bytes);
        BinaryReader reader = new BinaryReader(ms);
        reader.ReadChars(11);
        int effectNum = reader.ReadInt32();
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
                reader.BaseStream.Seek(60, SeekOrigin.Current);
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
                    frame.startTime = reader.ReadSingle();
                    frame.pos.x = reader.ReadSingle();
                    frame.pos.z = reader.ReadSingle();
                    frame.pos.y = reader.ReadSingle();
                    frame.quat.x = -reader.ReadSingle();
                    frame.quat.z = -reader.ReadSingle();
                    frame.quat.y = -reader.ReadSingle();
                    frame.quat.w = reader.ReadSingle();
                    frame.scale.x = reader.ReadSingle();
                    frame.scale.z = reader.ReadSingle();
                    frame.scale.y = reader.ReadSingle();
                    frame.colorRGB.r = reader.ReadSingle();
                    frame.colorRGB.g = reader.ReadSingle();
                    frame.colorRGB.b = reader.ReadSingle();
                    frame.colorRGB.a = reader.ReadSingle();
                    for (int j = 0; j < 13; j++)
                    {
                        frame.TailFlags[j] = reader.ReadSingle();
                    }
                    sfx.frames.Add(frame);
                }

                int TailSectionLength = reader.ReadInt32();
                if (sfx.EffectType.Equals("AUDIO"))
                {
                    int regionCnt = reader.ReadInt32();
                    string sBone = readNextString(reader, regionCnt);
                    sfx.audioLoop = reader.ReadInt32();
                    sfx.Tails[0] = sBone;
                }
                else if (sfx.EffectType.Equals("BILLBOARD") || sfx.EffectType.Equals("PLANE"))
                {
                    sfx.origScale.x = reader.ReadSingle();
                    sfx.origScale.y = reader.ReadSingle();
                }
                else if (sfx.EffectType.Equals("CYLINDER"))
                {
                    sfx.origAtt.x = reader.ReadSingle();//底部半径
                    sfx.origAtt.y = reader.ReadSingle();//顶部半径
                    sfx.origScale.x = reader.ReadSingle();//高度
                    sfx.origScale.y = reader.ReadSingle();//90
                    sfx.origScale.z = reader.ReadSingle();//450
                }
                else if (sfx.EffectType.Equals("BOX"))
                {
                    sfx.origScale.x = reader.ReadSingle();
                    sfx.origScale.y = reader.ReadSingle();
                    sfx.origScale.z = reader.ReadSingle();
                }
                else if (sfx.EffectType.Equals("PARTICLE"))
                {
                    //reader.BaseStream.Seek(4, SeekOrigin.Current);
                    //读4个字节长度，读100个字节，在读这个长度 最后面就是3个变长串
                    //就是100 +X+尾部变长。
                    int endRegionCnt = TailSectionLength;
                    //sfx.TailLength = 100 + endRegionCnt;
                    //粒子细节=96+endRegionCnt
                    sfx.version = endRegionCnt == 0 ? 0 : 1;
                    int particleBytes = 96 + endRegionCnt;
                    if (particleBytes == 96)
                        sfx.version = 0;
                    else if (particleBytes == 124)
                        sfx.version = 1;
                    else if (particleBytes == 112)
                        sfx.version = 2;
                    reader.ReadBytes(particleBytes);
                    //前面的28字节不知道干啥的。
                    reader.BaseStream.Seek(-96, SeekOrigin.Current);
                    sfx.MaxParticles = reader.ReadInt32();
                    sfx.startSizeMin = reader.ReadSingle();
                    sfx.startSizeMax = reader.ReadSingle();
                    sfx.unknwon2  = reader.ReadSingle();//1
                    sfx.unknown3 = reader.ReadSingle();//5
                    sfx.unknown0 = reader.ReadSingle();//30?可能是重力系数.其大于0时，粒子往下朝，其小于=0时，粒子向上
                    sfx.StartLifetime = reader.ReadSingle();//粒子生命周期
                    sfx.unknown1 = reader.ReadSingle();//1
                    sfx.startSpeedMin = reader.ReadSingle();//50 - startSpeed
                    sfx.startSpeedMax = reader.ReadSingle();//80 - startSpeed2
                    sfx.unknown4 = reader.ReadSingle();//0.1f
                    sfx.unknown5 = reader.ReadSingle();//0.1f;
                    sfx.unknown6 = reader.ReadSingle();//-1.0f;
                    sfx.unknown7 = reader.ReadSingle();//-1.0f;
                    sfx.unknown8 = reader.ReadSingle();//-1.0f;
                    sfx.unknown9 = reader.ReadSingle();//+1.0f;
                    sfx.unknown10 = reader.ReadSingle();//+1.0f;
                    sfx.unknown11 = reader.ReadSingle();//+1.0f;
                    sfx.unknown12 = reader.ReadInt32();//0
                    sfx.unknown13 = reader.ReadInt32();//1
                    sfx.unknown14 = reader.ReadInt32();//1
                    sfx.unknown15 = reader.ReadSingle();//0.5f
                    sfx.multiplyAlpha = reader.ReadInt32();//1
                    sfx.alpha = reader.ReadSingle();//0.3
                    int regionCnt = reader.ReadInt32();
                    string sBone = readNextString(reader, regionCnt);
                    sfx.Tails[0] = sBone;
                    regionCnt = reader.ReadInt32();
                    sBone = readNextString(reader, regionCnt);
                    sfx.Tails[1] = sBone;
                    regionCnt = reader.ReadInt32();
                    sBone = readNextString(reader, regionCnt);
                    sfx.Tails[2] = sBone;
                }
                else if (sfx.EffectType.Equals("DONUT"))
                {
                    reader.BaseStream.Seek(24, SeekOrigin.Current);
                }
                else if (sfx.EffectType.Equals("DRAG"))
                {
                    reader.BaseStream.Seek(4, SeekOrigin.Current);
                    int len4 = reader.ReadInt32();
                    sfx.Tails[0] = readNextString(reader, len4);
                    len4 = reader.ReadInt32();
                    sfx.Tails[1] = readNextString(reader, len4);
                }
                else if (sfx.EffectType.Equals("MODEL"))
                {
                    int len4 = reader.ReadInt32();
                    sfx.Tails[0] = readNextString(reader, len4);
                }
                else if (sfx.EffectType.Equals("SPHERE"))
                {
                    sfx.SphereRadius = reader.ReadSingle();
                    sfx.sphereAttr.x = reader.ReadSingle();
                    sfx.sphereAttr.y = reader.ReadSingle();
                    sfx.sphereAttr.z = reader.ReadSingle();
                    sfx.sphereAttr.w = reader.ReadSingle();
                }
                else
                {
                    Debug.LogError(string.Format("{0} can not parse: file {1}",  sfx.EffectType, file));
                    error = true;
                }
                effectList.Add(sfx);
            }
        }
        ms.Close();
        reader.Close();
        return true;
    }

    public SFXEffectPlay Play(GameObject obj, bool once, bool preLoad = false)
    {
        if (effectList.Count == 0)
            return null;
        SFXEffectPlay effectContainer = obj.AddComponent<SFXEffectPlay>();
        effectContainer.Load(this, once, preLoad);
        return effectContainer;
    }

    public SFXEffectPlay Play(CharacterLoader character, float timePlayed = 0.0f)
    {
        if (effectList.Count == 0)
            return null;
        SFXEffectPlay effectContainer = character.Target.gameObject.AddComponent<SFXEffectPlay>();
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
    public float SphereRadius;
    public List<EffectFrame> frames = new List<EffectFrame>();
    public Vector3 origScale = Vector3.one;//这个参数决定了网格生成的参数，类似cylinder,顶部半径，底部半径，以及高度都由此设置
    public Vector2 origAtt = Vector2.zero;//各自的参数设置 顶部半径，底部半径
    public Vector4 sphereAttr = Vector4.zero;
    //粒子系统使用的
    public int version;//96字节版=0 124字节版=1 112字节=2
    //public byte[] particle;
    public string[] Tails = new string[3];
    public int MaxParticles;
    public float startSizeMin;
    public float startSizeMax;
    public float startSpeedMin;
    public float startSpeedMax;
    public float unknown0;//30?
    public float StartLifetime;//粒子生命周期
    public float unknown1;//1
    public float unknwon2;//50
    public float unknown3;//80
    public float unknown4;//0.1f
    public float unknown5;//0.1f;
    public float unknown6;//-1.0f;
    public float unknown7;//-1.0f;
    public float unknown8;//-1.0f;
    public float unknown9;//+1.0f;
    public float unknown10;//+1.0f;
    public float unknown11;//+1.0f;
    public int unknown12;//0
    public int unknown13;//1
    public int unknown14;//1
    public float unknown15;//0.5f
    public int multiplyAlpha;//1是否叠加透明度
    public float alpha;//0.3透明度
}

public class SFXLoader
{
    Dictionary<string, SfxFile> EffectList = new Dictionary<string, SfxFile>();
    public Dictionary<string, SfxFile> Effect { get { return EffectList; } }
    //
    bool initList = false;
    List<string> key;
    int effIndex = 0;
    public SFXEffectPlay PlayEffect(int idx, GameObject obj, bool once = true, bool preload = false)
    {
        if (Eff.Length > idx)
            return PlayEffect(Eff[idx], obj, once, preload);
        return null;
    }

    //一些环境特效，例如风之类的音效.
    public SFXEffectPlay PlayEffect(string file, GameObject obj, bool once = false, bool preload = false)
    {
        if (!EffectList.ContainsKey(file))
        {
            SfxFile f = new SfxFile();
            try
            {
                f.ParseFile(file);
                EffectList[file] = f;
                if (!f.error)
                    return f.Play(obj, once, preload);
            }
            catch
            {
                EffectList[file] = f;
                Debug.LogError(string.Format("{0}  parse error", file));
            }

        }
        else
        {
            if (EffectList[file].error)
                return null;
            //Debug.LogError("effect file:" + file + " 455");
            return EffectList[file].Play(obj, once, preload);
        }
        return null;
    }

    public SFXEffectPlay PlayEffect(string file, Vector3 position, bool once = false, bool preload = false)
    {
        GameObject obj = new GameObject(file);
        obj.transform.position = position;
        obj.transform.rotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;
        if (!EffectList.ContainsKey(file))
        {
            SfxFile f = new SfxFile();
            try
            {
                f.ParseFile(file);
                EffectList[file] = f;
                if (!f.error)
                    return f.Play(obj, once, preload);
            }
            catch
            {
                EffectList[file] = f;
                Debug.LogError(string.Format("{0}  parse error", file));
            }

        }
        else
        {
            if (EffectList[file].error)
                return null;
            //Debug.LogError("effect file:" + file + " 455");
            return EffectList[file].Play(obj, once, preload);
        }
        return null;
    }

    public SFXEffectPlay PlayEffect(int id, CharacterLoader target, float timePlayed = 0.0f)
    {
        return PlayEffect(Eff[id], target, timePlayed);
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
                f.ParseFile(file);
                EffectList[file] = f;
                //Debug.LogError("effect file:" + file);
                return f.Play(target, timePlayed);
            }
            catch (Exception exp)
            {
                Debug.LogError(string.Format("{0}  parse error {1}", file, exp.Message));
                EffectList[file] = f;
            }

        }
        else
        {
            //Debug.LogError("effect file:" + file);
            if (EffectList[file].error)
                return null;
            return EffectList[file].Play(target, timePlayed);
        }
        return null;
    }

    public int TotalSfx = 0;
    public int Miss = 0;
    public string[] Eff;
    public IEnumerator Init()
    {
        TextAsset list = Resources.Load<TextAsset>("effect.lst");
        Eff = list.text.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        TotalSfx = Eff.Length;
        for (int i = 0; i < Eff.Length; i++)
        {
            if (!EffectList.ContainsKey(Eff[i]))
            {
                SfxFile f = new SfxFile();
                f.ParseFile(Eff[i]);
                EffectList[Eff[i]] = f;
            }
            if (i % 50 == 0)
                yield return 0;
        }
    }

    public void InitSync()
    {
        TextAsset list = Resources.Load<TextAsset>("effect.lst");
        Eff = list.text.Split(new char[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        TotalSfx = Eff.Length;
        for (int i = 0; i < Eff.Length; i++)
        {
            if (!EffectList.ContainsKey(Eff[i]))
            {
                SfxFile f = new SfxFile();
                f.ParseFile(Eff[i]);
                EffectList[Eff[i]] = f;
            }
        }
    }
}
