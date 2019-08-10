using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXAudio {

    SFXEffectPlay Player;
    SfxEffect Audio;
    int state = -1;//-1未开始播放，-2播放时间已结束，
    public SFXAudio(SFXEffectPlay player, SfxEffect audio)
    {
        Player = player;
        Audio = audio;
    }

    public bool Update()
    {
        if (Audio.frames[1].startTime < Player.playedTime && state == -1 && Audio.audioLoop == 1)
        {
            //音效跳过，因为动作已经播放完毕.
            state = -2;
        }
        else if (Audio.frames[0].startTime < Player.playedTime && state == -1)
        {
            //带loop的是环境音效
            //有个问题是绑定到对象上一起运动还是只是在那里初始化不跟随移动.
            bool use3DAudio = Player.loop;
            //use3DAudio = true;
            int idx = SoundManager.Instance.Play3DSound(Audio.Tails[0], Player.transform.position, Audio.audioLoop != 0, use3DAudio);
            state = idx;
        }
        else if (Audio.frames[1].startTime < Player.playedTime && state > 0)
        {
            SoundManager.Instance.StopEffect(state);
            state = -2;
        }
        return state == -2;
    }
}
