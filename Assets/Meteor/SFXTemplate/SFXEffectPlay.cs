using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class SFXEffectPlay :NetBehaviour
{
    //声音统一由自己处理，其余子特效没一个对应一个SFXUnit处理
    static GameObject SfxUnit;
    List<SFXUnit> played = new List<SFXUnit>();
    MeteorUnit owner;
    public MeteorUnit Attacher { get { return owner; } }
    public string file;
    bool _audioUpdate = false;
    bool audioUpdate
    {
        get
        {
            return this._audioUpdate;
        }
        set
        {
            this._audioUpdate = value;
            if (value)
            {
                EffectIns.Clear();
                int i = 0;
                foreach (var each in audioList)
                {
                    //如果一开始的时间跳跃过第一帧，或跳跃过第二帧，就忽略此特效 重复播放的特效除外
                    if (((each.frames[0].startTime < playedTime) || (each.frames[1].startTime < playedTime)) && each.audioLoop == 1)
                        EffectIns.Add(i++, -2);
                    else
                        EffectIns.Add(i++, -1);
                }
            }
        }
    }

    protected new void Awake()
    {
        base.Awake();
        owner = GetComponent<MeteorUnit>();
        if (SfxUnit == null) {
            SfxUnit = Resources.Load<GameObject>("SFXUnit");
        }
    }

    protected new void OnDestroy()
    {
        base.OnDestroy();
    }
    public bool loop;
    float playedTime = 0.0f;

    public override void NetUpdate()
    {
        if (isDebug)
        {
            if (OnSfxFrame != null)
                OnSfxFrame(playedTime);
        }

        playedTime += FrameReplay.deltaTime;
        if (audioUpdate)
            PlayAudioEffect();
    }

    public System.Action<float> OnSfxFrame;
    bool isDebug = false;
    public void setAsDebug()
    {
        isDebug = true;
    }

    public Transform FindEffectByName(string str)
    {
        if (string.IsNullOrEmpty(str))
            return null;
        //不允许在子层级查找。
        for (int i = 0; i < played.Count; i++)
            if (played[i].name == str && !played[i].PlayDone)
                return played[i].transform;
        return null;
    }

    //当特效强制结束时。角色死亡，招式被打断。等
    public void OnPlayAbort()
    {
        for (int i = 0; i < played.Count; i++)
        {
            played[i].pause = true;
            played[i].GetComponent<MeshRenderer>().enabled = false;
            Destroy(played[i].gameObject);
        }
        played.Clear();
        if (audioUpdate)
            audioUpdate = false;
        foreach (var each in EffectIns)
            SoundManager.Ins.StopEffect(each.Value);
        EffectIns.Clear();
        Destroy(this);
    }

    int sfxDoneCnt = 0;
    public void OnPlayDone(SFXUnit sfx)
    {
        sfxDoneCnt++;
        if (sfx != null && !loop)
            sfx.Hide();
        if (sfxDoneCnt == (played.Count + (audioList.Count == 0 ? 0 : 1)))
        {
            if (loop)
            {
                for (int i = 0; i < played.Count; i++)
                    played[i].RePlay();
                if (audioUpdate)
                    audioUpdate = false;
                if (audioList.Count != 0)
                    audioUpdate = true;
                sfxDoneCnt = 0;
                playedTime = 0.0f;
            }
            else
            {
                for (int i = 0; i < played.Count; i++)
                    Destroy(played[i].gameObject);
                played.Clear();
                if (audioUpdate)
                    audioUpdate = false;
                if (owner == null && tag != "SceneItemAgent")
                    DestroyObject(gameObject);
                else
                    Destroy(this);
            }
        }
    }

    //character 对应d_base
    List<SfxEffect> audioList = new List<SfxEffect>();
    public void Load(SfxFile effectFile, bool once = false, bool preload = false)
    {
        loop = !once;
        file = effectFile.strfile;
        var effectList = effectFile.effectList;
        playedTime = 0.0f;
        //必须看一下信息，才知道创建什么大小的预设，主要是网格可能大小不一样
        SortedDictionary<int, SFXUnit> dic = new SortedDictionary<int, SFXUnit>();
        for (int i = 0; i < effectList.Count; i++)
        {
            GameObject objSfx = null;
            SFXUnit sfx = null;
            if (effectList[i].EffectType == "AUDIO")
                audioList.Add(effectList[i]);
            else if (effectList[i].EffectType == "BILLBOARD")
            {
                objSfx = Instantiate(SfxUnit);
                sfx = objSfx.GetComponent<SFXUnit>();
                //sfx.Init(effectList[i], this, i);
            }
            else if (effectList[i].EffectType == "BOX")
            {
                objSfx = Instantiate(SfxUnit);
                sfx = objSfx.GetComponent<SFXUnit>();
                //sfx.Init(effectList[i], this, i);
            }
            else if (effectList[i].EffectType == "PLANE")
            {
                objSfx = Instantiate(SfxUnit);
                sfx = objSfx.GetComponent<SFXUnit>();
                //sfx.Init(effectList[i], this, i);
            }
            else if (effectList[i].EffectType == "DONUT")
            {
                objSfx = Instantiate(SfxUnit);
                sfx = objSfx.GetComponent<SFXUnit>();
                //sfx.Init(effectList[i], this, i);
            }
            else if (effectList[i].EffectType == "MODEL")
            {
                objSfx = Instantiate(SfxUnit);
                sfx = objSfx.GetComponent<SFXUnit>();
                //sfx.Init(effectList[i], this, i);
            }
            else if (effectList[i].EffectType == "SPHERE")
            {
                objSfx = Instantiate(SfxUnit);
                sfx = objSfx.GetComponent<SFXUnit>();
                //sfx.Init(effectList[i], this, i);
            }
            else if (effectList[i].EffectType == "PARTICLE")
            {
                objSfx = Instantiate(SfxUnit);
                sfx = objSfx.GetComponent<SFXUnit>();
                //sfx.Init(effectList[i], this, i);
            }
            else if (effectList[i].EffectType == "CYLINDER")
            {
                objSfx = Instantiate(SfxUnit);
                sfx = objSfx.GetComponent<SFXUnit>();
            }
            else if (effectList[i].EffectType == "DRAG")
            {
                objSfx = Instantiate(SfxUnit);
                sfx = objSfx.GetComponent<SFXUnit>();

                //sfx.Init(effectList[i], this, i);
            }
            else
            {
                Debug.LogError("can not recognize");
            }

            if (sfx != null)
            {
                dic.Add(i, sfx);
                //特效可以非角色发出.
                if (owner != null)
                    owner.AddAttackSFX(sfx);//因为这个特效可以做攻击
                sfx.name = effectList[i].EffectName;
            }
        }

        //第一次先创建，再初始化，否则先初始化的，如果要挂到后初始化的上面，会找不到对象
        foreach (var each in dic)
        {
            if (each.Value != null)
                played.Add(each.Value);
        }

        //分2次循环,第一次让前面的可以找到后面的,第二次都开始初始化,就可以互相引用了
        foreach (var each in dic)
        {
            if (each.Value != null)
                each.Value.Init(effectList[each.Key], this, each.Key, 0, preload);
        }
        if (audioList.Count != 0)
        {
            audioUpdate = true;
        }
    }

    public void Load(SfxFile effectFile, float timePlayed, bool preLoad = false)
    {
        loop = false;
        file = effectFile.strfile;
        var effectList = effectFile.effectList;
        playedTime = timePlayed;
        //必须看一下信息，才知道创建什么大小的预设，主要是网格可能大小不一样
        SortedDictionary<int, SFXUnit> dic = new SortedDictionary<int, SFXUnit>();
        for (int i = 0; i < effectList.Count; i++)
        {
            GameObject objSfx = null;
            SFXUnit sfx = null;
            if (effectList[i].EffectType.Equals("AUDIO"))
                audioList.Add(effectList[i]);
            else if (effectList[i].EffectType.Equals("BILLBOARD"))
            {
                objSfx = Instantiate(SfxUnit);
                sfx = objSfx.GetComponent<SFXUnit>();
                //sfx.Init(effectList[i], this, i);
            }
            else if (effectList[i].EffectType.Equals("BOX"))
            {
                objSfx = Instantiate(SfxUnit);
                sfx = objSfx.GetComponent<SFXUnit>();
                //sfx.Init(effectList[i], this, i);
            }
            else if (effectList[i].EffectType.Equals("PLANE"))
            {
                objSfx = Instantiate(SfxUnit);
                sfx = objSfx.GetComponent<SFXUnit>();
                //sfx.Init(effectList[i], this, i);
            }
            else if (effectList[i].EffectType.Equals("DONUT"))
            {
                objSfx = Instantiate(SfxUnit);
                sfx = objSfx.GetComponent<SFXUnit>();
                //sfx.Init(effectList[i], this, i);
            }
            else if (effectList[i].EffectType.Equals("MODEL"))
            {
                objSfx = Instantiate(SfxUnit);
                sfx = objSfx.GetComponent<SFXUnit>();
                //sfx.Init(effectList[i], this, i);
            }
            else if (effectList[i].EffectType.Equals("SPHERE"))
            {
                objSfx = Instantiate(SfxUnit);
                sfx = objSfx.GetComponent<SFXUnit>();
                //sfx.Init(effectList[i], this, i);
            }
            else if (effectList[i].EffectType.Equals("PARTICLE"))
            {
                objSfx = Instantiate(SfxUnit);
                sfx = objSfx.GetComponent<SFXUnit>();
                //sfx.Init(effectList[i], this, i);
            }
            else if (effectList[i].EffectType.Equals("CYLINDER"))
            {
                objSfx = Instantiate(SfxUnit);
                sfx = objSfx.GetComponent<SFXUnit>();
            }
            else if (effectList[i].EffectType.Equals("DRAG"))
            {
                objSfx = Instantiate(SfxUnit);
                sfx = objSfx.GetComponent<SFXUnit>();
            }
            else
            {
                Debug.LogError("！！！");
            }

            if (sfx != null)
            {
                dic.Add(i, sfx);
                //特效可以非角色发出.
                if (owner != null)
                    owner.AddAttackSFX(sfx);//因为这个特效可以做攻击
                sfx.name = effectList[i].EffectName;
            }
        }

        //第一次先创建，再初始化，否则先初始化的，如果要挂到后初始化的上面，会找不到对象
        foreach (var each in dic)
        {
            if (each.Value != null)
                played.Add(each.Value);
        }

        //分2次循环,第一次让前面的可以找到后面的,第二次都开始初始化,就可以互相引用了
        foreach (var each in dic)
        {
            if (each.Value != null)
                each.Value.Init(effectList[each.Key], this, each.Key, timePlayed, preLoad);
        }
        if (audioList.Count != 0)
            audioUpdate = true;
    }

    //-1未播放
    //-2已播放还未完成
    //-3已播放超过最大帧
    //大于0，播放返回的实例id,后面时间到了用来调用去停止循环声音
    SortedDictionary<int, int> EffectIns = new SortedDictionary<int, int>();
    void PlayAudioEffect()
    {
        //AUDIO是第一帧放，第二帧删
        int i = 0;
        bool canB = true;
        foreach (var each in audioList)
        {
            if (each.frames[1].startTime < playedTime && EffectIns[i] == -1 && each.audioLoop == 1)
            {
                //音效跳过，因为动作已经播放完毕.
                EffectIns[i] = -2;
            }
            else
            if (each.frames[0].startTime < playedTime && EffectIns[i] == -1)
            {
                //带loop的是环境音效
                //有个问题是绑定到对象上一起运动还是只是在那里初始化不跟随移动.
                bool use3DAudio = loop;
                //use3DAudio = true;
                int idx = SoundManager.Ins.Play3DSound(each.Tails[0], transform.position, each.audioLoop != 0, use3DAudio);
                EffectIns[i] = idx;
            }
            else if (each.frames[1].startTime < playedTime)
            {
                SoundManager.Ins.StopEffect(EffectIns[i]);
                EffectIns[i] = -3;
            }

            //检查能否退出协程
            
            foreach (var eachS in EffectIns)
            {
                if (eachS.Value != -3)
                    canB = false;
            }

            i++;
            if (canB)
            {
                audioUpdate = false;
                OnPlayDone(null);
                return;
            }
        }
    }
}
