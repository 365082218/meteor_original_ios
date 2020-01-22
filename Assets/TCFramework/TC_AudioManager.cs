using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager
{
    public bool _musicOpen = true;
    public bool _soundEffectOpen = true;

    private AudioSource _musicAudioSource;
    private AudioClip _musicClip;

    public string AudioPath = "Audio/MusicAudio";
    private Dictionary<int, UserAudioData> _loopAudioDataDic = new Dictionary<int, UserAudioData>();
    private Dictionary<int, UserAudioData> playerAudioClips = new Dictionary<int, UserAudioData>();
    private static int AudioIndex = 0;
    public void Init()
    {
        //TC_EventManager.Instance.AddListener<TC_PlayAudioEvent>(OnPlayAudioEvent);
        //_musicAudioSource = GameObject.Find("Main Camera").GetComponent<AudioSource>();
    }

    //public void OnPlayAudioEvent(TC_PlayAudioEvent e)
    //{
    //    Debuger.LogError("play sound really :" + e.id);
    //    int audioId = e.id;
    //    AudioInfoDatas.AudioInfoDatas _audioInfo = TC_DatasManager.Instance.GetData<AudioInfoDatas.AudioInfoDatas>(audioId);
    //    if (_audioInfo == null)
    //        return;
    //    switch (_audioInfo.audioType)
    //    {
    //        case 1://音效类型
    //            if (!_soundEffectOpen)
    //                return;
    //            TC_PoolManager.Instance.Spawn<GameObject>(TC_AssetType.AT_Modle, AudioPath, (_path, _asset, _param) =>
    //            {
    //                if (null == _asset)
    //                    return;
    //                GameObject _audio = (GameObject)_asset;
    //                AudioSource _audioSource = _audio.transform.GetComponent<AudioSource>();
    //                if (_audioSource == null)
    //                    return;
    //                TC_PoolManager.Instance.Spawn<AudioClip>(TC_AssetType.AT_Clip, _audioInfo.audioPath, (__path, __asset, __param) =>
    //                {
    //                    if (null == __asset)
    //                        return;
    //                    AudioClip _clip = (AudioClip)__asset;
    //                    _audioSource.clip = _clip;
    //                    _audioSource.Play();
    //                    _audioSource.volume = _audioInfo.volume;
    //                    _audioSource.loop = _audioInfo.isLoop;
    //                    if (e.callback == null)
    //                    {
    //                        //如果调用者不想自己控制生命周期，那么按通常逻辑走.
    //                        if (!_audioInfo.isLoop)//不循环播放
    //                        {
    //                            TC_TimeManager.AddTime(() =>
    //                            {
    //                                if (_clip != null)
    //                                {
    //                                    TC_PoolManager.Instance.Despawn<AudioClip>(TC_AssetType.AT_Clip, _audioInfo.audioPath, _clip);
    //                                    _clip = null;
    //                                }
    //                                if (_audio != null)
    //                                {
    //                                    TC_PoolManager.Instance.Despawn<GameObject>(TC_AssetType.AT_Modle, AudioPath, _audio);
    //                                    _audio = null;
    //                                }
    //                            }, _clip.length, 1);
    //                        }
    //                        else
    //                        {
    //                            if (!_loopAudioDataDic.ContainsKey(audioId))
    //                            {
    //                                UserAudioData _loopAudioData = new UserAudioData();
    //                                _loopAudioData._audio = _audio;
    //                                _loopAudioData._audioSource = _audioSource;
    //                                _loopAudioData._clip = _clip;
    //                                _loopAudioData._audioInfo = _audioInfo;
    //                                _loopAudioDataDic.Add(audioId, _loopAudioData);
    //                            }
    //                        }
    //                    }
    //                    else
    //                    {
    //                        UserAudioData AudioData = new UserAudioData();
    //                        AudioData._audio = _audio;
    //                        AudioData._audioSource = _audioSource;
    //                        AudioData._clip = _clip;
    //                        AudioData._audioInfo = _audioInfo;
    //                        int audioIndex = AudioIndex;
    //                        playerAudioClips.Add(AudioIndex, AudioData);
    //                        AudioIndex++;
    //                        e.callback(audioIndex, e.id);
    //                    }
    //                });
    //            });
    //            break;
    //        case 2://背景音乐类型
    //            if (_musicAudioInfo == _audioInfo && _musicAudioSource.isPlaying)//如果正在播放当前音乐就不再重新加载播放
    //                return;
    //            if (_musicClip != null)
    //            {
    //                TC_PoolManager.Instance.Despawn<AudioClip>(TC_AssetType.AT_Clip, _musicAudioInfo.audioPath, _musicClip);
    //                _musicClip = null;
    //            }
    //            _musicAudioInfo = _audioInfo;
    //            TC_PoolManager.Instance.Spawn<AudioClip>(TC_AssetType.AT_Clip, _musicAudioInfo.audioPath, (_path, _asset, _param) =>
    //            {
    //                if (null == _asset)
    //                    return;
    //                //if (_musicClip != null)
    //                //{
    //                //    TC_PoolManager.Instance.Despawn<AudioClip>(TC_AssetType.AT_Clip, _musicAudioInfo.audioPath, _musicClip);
    //                //    _musicClip = null;
    //                //}
    //                _musicClip = (AudioClip)_asset;
    //                _musicAudioSource.clip = _musicClip;
    //                _musicAudioSource.Play();
    //                if (_musicOpen)
    //                {
    //                    _musicAudioSource.volume = _musicAudioInfo.volume;
    //                }
    //                else
    //                {
    //                    _musicAudioSource.volume = 0f;
    //                }
    //                _musicAudioSource.loop = _musicAudioInfo.isLoop;
    //            });
    //            break;
    //    }
    //}

    ///// <summary>
    ///// 暂停音乐
    ///// </summary>
    //public void PauseAudio(bool isOpen)
    //{
    //    _musicOpen = isOpen;
    //    if (isOpen)
    //    {
    //        if (!_musicAudioSource.isPlaying)
    //        {
    //            _musicAudioSource.Play();

    //        }
    //        if (_musicAudioInfo != null)
    //        {
    //            _musicAudioSource.volume = _musicAudioInfo.volume;
    //        }
    //    }
    //    else
    //    {
    //        _musicAudioSource.Pause();
    //    }
    //}
    ///// <summary>
    ///// 关闭音乐
    ///// </summary>
    //public void StopAudio()
    //{
    //    _musicAudioSource.Stop();
    //}
    //public void OnDestroy()
    //{
    //    TC_EventManager.Instance.RemoveListener<TC_PlayAudioEvent>(OnPlayAudioEvent);
    //    foreach (KeyValuePair<int, UserAudioData> kv in _loopAudioDataDic)
    //    {
    //        kv.Value.Clear();
    //    }
    //    if (_musicClip != null)
    //    {
    //        TC_PoolManager.Instance.Despawn<AudioClip>(TC_AssetType.AT_Clip, _musicAudioInfo.audioPath, _musicClip);
    //        _musicClip = null;
    //    }
    //    _musicAudioSource = null;
    //    _musicAudioInfo = null;
    //}

    //public void PlaySound(int audioIndex)
    //{
    //    Debuger.LogError("play sound:" + audioIndex);
    //    TC_EventManager.Instance.Trigger<TC_PlayAudioEvent>(TC_PlayAudioEvent.Get().Set(audioIndex));
    //}

    ////得到音效自己控制的
    //public void PlaySound(RTCTankController player, int audioIndex)
    //{
    //    TC_EventManager.Instance.Trigger<TC_PlayAudioEvent>(TC_PlayAudioEvent.Get().SetContext(player.OnSoundPlay, audioIndex));
    //}

    //停止播放背景音乐
    public void StopMusic()
    {
        if (_musicAudioSource != null)
            _musicAudioSource.Stop();
    }

    //用户自己管理声音的播放和停止的.
    public void PlayerSoundEx(int Instance)
    {
        if (playerAudioClips.ContainsKey(Instance))
        {
            if (playerAudioClips[Instance] != null && playerAudioClips[Instance]._audioSource != null)
                playerAudioClips[Instance]._audioSource.Play();
        }
    }

    public void StopSoundEx(int Instance)
    {
        if (playerAudioClips.ContainsKey(Instance))
        {
            if (playerAudioClips[Instance] != null && playerAudioClips[Instance]._audioSource != null)
                playerAudioClips[Instance]._audioSource.Pause();
        }
    }

    public void RemoveSoundEx(int Instance)
    {
        if (playerAudioClips.ContainsKey(Instance))
        {
            playerAudioClips[Instance].Clear();
            playerAudioClips.Remove(Instance);
        }
    }

    public bool IsPlaying(int instance)
    {
        if (playerAudioClips.ContainsKey(instance) && playerAudioClips[instance] != null && playerAudioClips[instance]._audioSource != null)
            return playerAudioClips[instance]._audioSource.isPlaying;
        return false;
    }

    //删除战斗内角色相关的音效
    public void DestroyPlayerSound()
    {
        foreach (KeyValuePair<int, UserAudioData> kv in playerAudioClips)
        {
            kv.Value.Clear();
        }
        playerAudioClips.Clear();
        //AudioIndex = 0;
    }
}

public class UserAudioData
{
    public GameObject _audio;
    public AudioSource _audioSource;
    public AudioClip _clip;
    //public AudioInfoDatas.AudioInfoDatas _audioInfo;
    public void Clear()
    {
        //if (_audio != null)
        //{
        //    TC_PoolManager.Instance.Despawn<GameObject>(TC_AssetType.AT_Modle, TC_AudioManager.GetInstance().AudioPath, _audio);
        //    _audio = null;
        //}
        //if (_clip != null)
        //{
        //    TC_PoolManager.Instance.Despawn<AudioClip>(TC_AssetType.AT_Clip, _audioInfo.audioPath, _clip);
        //    _clip = null;
        //}
        //_audioSource = null;
        //_audioInfo = null;
    }
}
