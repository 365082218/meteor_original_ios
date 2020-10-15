using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Idevgame.Util;


public class SoundManager:Singleton<SoundManager>
{
    bool enable = true;
    class AudioClipInfo
    {
        public string Name;
        AudioClip mClip;
        public AudioClipInfo(string name, AudioClip clip)
        {
            Name = name;
            mClip = clip;
        }

        public AudioClip Clip
        { 
			get { return mClip ?? (mClip = Resources.Load<AudioClip>(Name)); } 
            set { mClip = value; } 
        }
    }

    float min = 100;
    float max = 200;
    bool FadeLinear = true;
    float FadeDistance = 300;//2D音效超过300不播放了.
    List<AudioClipInfo> mRuntimeClips = new List<AudioClipInfo>();
    SortedDictionary<string, int> mSoundIndexMap = new SortedDictionary<string, int>();
    AudioSource mAudioSource;
    AudioSource mAudioMusic;
    float AduioVolume = 0;
    float MusicVolume = 0;
    float FadeVolumeScale;

    public AudioSource CurrentMusicSource
    {
        get
        {
            if (mAudioMusic == null)
                mAudioMusic = Main.Ins != null ? Main.Ins.Music : null;
            return mAudioMusic;
        }
        set { mAudioMusic = value; }
    }

    //2D音频源
    public AudioSource CurrentAudioSource
    {
        get
        {
            if (mAudioSource == null)
                mAudioSource = Main.Ins.Sound;
            return mAudioSource;
        }
        set { mAudioSource = value; }
    }

    //3D音频源
    List<GameObject> effectList = new List<GameObject>();
    public AudioSource GetFreeAudio(Vector3 pos)
    {
        if (!enable)
            return null;
        AudioSource ret = null;
        AudioSource tmp = null;
        for (int i = 0; i < effectList.Count; i++)
        {
            tmp = effectList[i].GetComponent<AudioSource>();
            if (tmp.clip == null && !tmp.isPlaying)
            {
                ret = tmp;
                break;
            }
            if (effectList[i].GetComponent<AudioSource>().isPlaying)
                continue;
            ret = effectList[i].GetComponent<AudioSource>();
            break;
        }

        if (ret == null)
        {
            GameObject obj = new GameObject("3DAS");
            ret = obj.AddComponent<AudioSource>();
            effectList.Add(obj);
        }

        //try clean res
        if (effectList.Count >= 32)
        {
            Debug.LogWarning("audio >= 32");
            List<AudioSource> canFreed = new List<AudioSource>();
            for (int i = 0; i < effectList.Count; i++)
            {
                if (effectList[i].GetComponent<AudioSource>().isPlaying)
                    continue;
                canFreed.Add(effectList[i].GetComponent<AudioSource>());
            }
            canFreed.Remove(ret);
            for (int i = 0; i < canFreed.Count; i++)
            {
                effectList.Remove(canFreed[i].gameObject);
                GameObject.Destroy(canFreed[i].gameObject);
            }
        }
        ret.transform.position = pos;
        return ret;
    }

    // Use this for initialization
    public SoundManager()
    {
        
    }

    public void Init()
    {
        if (Main.Ins != null)
        {
            SetMusicVolume(GameStateMgr.Ins.gameStatus.MusicVolume);
            SetSoundVolume(GameStateMgr.Ins.gameStatus.SoundVolume);
        }
    }

    public int GetSoundIndex(string soundName)
    {
        if (!enable)
            return -1;
        if (string.IsNullOrEmpty(soundName)) return -1;
		
        int ret;
        if (mSoundIndexMap.TryGetValue(soundName, out ret))
            return ret;

        AudioClip audioClip = null;
        if (soundName.ToLower().EndsWith(".wav"))
        {
            int index = soundName.ToLower().IndexOf(".wav");
            if (index != -1)
            {
                string sound = soundName.Substring(0, index);
                audioClip = Resources.Load<AudioClip>(sound);
                if (audioClip != null)
                {
                    ret = mRuntimeClips.Count;
                    mSoundIndexMap[soundName] = ret;
                    mRuntimeClips.Add(new AudioClipInfo(soundName, audioClip));
                    return ret;
                }
                else
                    Debug.LogError("sound miss:" + soundName);
            }
        }

        audioClip = Resources.Load<AudioClip>(soundName);
        if (audioClip != null)
        {
            ret = mRuntimeClips.Count;
            mSoundIndexMap[soundName] = ret;
            mRuntimeClips.Add(new AudioClipInfo(soundName, audioClip));
            return ret;
        }
        Debug.LogError("Fail to found sound: " + soundName);
        return -1;
    }

    public void UnloadRuntimeClips()
    {
        if (!enable)
            return;
        foreach (AudioClipInfo info in mRuntimeClips)
            info.Clip = null;
		mAudioSource = null;
    }

    AudioClip GetAudioClip(int soundIndex)
    {
        if (!enable)
            return null;
        if (soundIndex < 0 || soundIndex >= mRuntimeClips.Count)
            return null;

        return mRuntimeClips[soundIndex].Clip;
    }

    public int Play3DSound(int soundIndex, Vector3 position, bool loop, bool D3Audio)
    {
        if (!enable)
            return -2;
        return Play3DSound(soundIndex, position, AduioVolume, loop, D3Audio);
    }

    public int Play3DSound(int soundIndex, Vector3 position, float volume, bool loop, bool D3Audio)
    {
        if (!enable)
            return -2;
        Vector3 listenerPos = position;
        if (Main.Ins.MainCamera != null) {
            listenerPos = Main.Ins.MainCamera.transform.position;
        }
        // clip sound too far.仅在2D音效下，判断距离是否超越范围，打击音效只有播放或者不播放，没有3D效果
        float distance = MathUtility.DistanceMax(listenerPos, position);
        if (!D3Audio && distance > FadeDistance)
            return -2;

        AudioClip clip = GetAudioClip(soundIndex);
        if (clip == null)
            return -2;

        //float volumeScale = (FadeDistance - distance) * FadeVolumeScale;
        //if (!FadeLinear)
        //    volumeScale = volumeScale * volumeScale;

        AudioSource rs = GetFreeAudio(position);
        if (rs != null)
        {
            rs.clip = clip;
            rs.loop = loop;
            rs.volume = AduioVolume;
            rs.spatialBlend = D3Audio ? 1.0f : 0.0f;
            rs.minDistance = min;
            rs.maxDistance = max;
            rs.rolloffMode = FadeLinear ? AudioRolloffMode.Linear : AudioRolloffMode.Logarithmic;
            if (loop)
                LoopAudio.Add(nEffectIndex, rs);
            rs.Play();
        }
        if (loop)
            return nEffectIndex++;
        return -2;
    }
    int nEffectIndex = 1;
    SortedDictionary<int, AudioSource> LoopAudio = new SortedDictionary<int, AudioSource>();
    public void StopEffect(int clipInsIdx)
    {
        if (!enable)
            return;
        if (LoopAudio.ContainsKey(clipInsIdx))
        {
            LoopAudio[clipInsIdx].Stop();
            LoopAudio[clipInsIdx].clip = null;
            LoopAudio.Remove(clipInsIdx);
        }
    }

    public int Play3DSound(string clipname, Vector3 position, bool loop, bool D3Audio)
    {
        if (!enable)
            return -2;
        return Play3DSound(GetSoundIndex(clipname), position, loop, D3Audio);
    }

    public void PlayClip(AudioClip clip)
    {
        CurrentAudioSource.PlayOneShot(clip);
    }

    public void PlaySound(int idx)
    {
        if (!enable)
            return;
        AudioClip clip = GetAudioClip(idx);
        CurrentAudioSource.PlayOneShot(clip);
    }

    public void PlaySound(string clipname)
    {
        if (!enable)
            return;
        AudioClip clip = GetAudioClip(GetSoundIndex(clipname));
        if (clip != null && CurrentAudioSource != null) {
            CurrentAudioSource.volume = AduioVolume;
            CurrentAudioSource.PlayOneShot(clip);
        }
    }
	
    public void PlayMusic(AudioClip clip) {
        if (clip != null && CurrentMusicSource != null) {
            CurrentMusicSource.loop = true;
            CurrentMusicSource.clip = clip;
            CurrentMusicSource.Play();
        }
    }

    public void PlayMusic(string clipname)
    {
        if (!enable)
            return;
        AudioClip clip = GetAudioClip(GetSoundIndex(clipname));
        if (clip != null && CurrentMusicSource != null) {
            CurrentMusicSource.loop = true;
            CurrentMusicSource.clip = clip;
            CurrentMusicSource.Play();
        }
    }

    //设置背景音乐音量
	public void SetMusicVolume(float volume)
	{
        if (CurrentMusicSource != null)
            CurrentMusicSource.volume = volume;
        if (MainMenuState.Exist)
            MainMenuState.Instance.menu.volume = volume;
        MusicVolume = volume;
	}
	
    //设置特效音量.
	public void SetSoundVolume(float volume)
	{
        AudioSource[] ASources = GameObject.FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
		for(int i = 0;i<ASources.Length;i++)
		{
            if (MainMenuState.Exist)
                if (ASources[i] == MainMenuState.Instance.menu)
                    continue;
			if (ASources[i] != CurrentMusicSource)
                ASources[i].volume = volume;
		}
        AduioVolume = volume;
    }

    public void StopAll()
    {
        if (!enable)
            return;
        CurrentMusicSource.Stop();
        CurrentAudioSource.Stop();
        foreach (var each in LoopAudio)
        {
            each.Value.Stop();
        }
        try
        {
            for (int i = 0; i < effectList.Count; i++)
            {
                if (effectList[i] == null)
                    continue;
                effectList[i].GetComponent<AudioSource>().Stop();
            }
        }
        catch (System.Exception exp)
        {
            U3D.PopupTip(exp.Message + "|" + exp.StackTrace);
            Debug.Log(exp.Message + "|" + exp.StackTrace);
        }
        LoopAudio.Clear();
        effectList.Clear();
    }

    public void Enable(bool e)
    {
        enable = e;
    }

    bool Muted = false;
    public void Mute(bool mute)
    {
        Muted = mute;
        if (mute)
        {
            SetMusicVolume(0);
            SetSoundVolume(0);
        }
        else
        {
            SetMusicVolume(GameStateMgr.Ins.gameStatus.MusicVolume);
            SetSoundVolume(GameStateMgr.Ins.gameStatus.SoundVolume);
        }
    }

    public void MuteMusic(bool mute) {
        if (mute) {
            SetMusicVolume(0);
        } else {
            SetMusicVolume(GameStateMgr.Ins.gameStatus.MusicVolume);
        }
    }
}
