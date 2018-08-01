
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Outfit7.Util;
using Outfit7.Event;

namespace Outfit7.Audio {
    public class AudioManager {

        const string MUTE_KEY = "O7.AudioManager.muted";
        const string MUTE_MUSIC_KEY = "O7.AudioManager.muted.music";
        const string MUTE_SFX_KEY = "O7.AudioManager.muted.sfx";
        const string MINIGAME_MUSIC_ON_KEY = "O7.minigame.music_on";

        public MainAudioPlayer MainAudioPlayer { get; set; }

        public bool Muted { get; private set; }

        public bool MutedSfx { get; private set; }

        public bool MutedMusic { get; private set; }

        public bool MiniGameMusicOn { get; private set; }

        public EventBus EventBus { get; set; }

        public AudioManager() {

            Muted = UserPrefs.GetBool(MUTE_KEY, false);
            MutedMusic = UserPrefs.GetBool(MUTE_MUSIC_KEY, Muted);
            MutedSfx = UserPrefs.GetBool(MUTE_SFX_KEY, Muted);
            MiniGameMusicOn = UserPrefs.GetBool(MINIGAME_MUSIC_ON_KEY, true);
        }

        public void SetMiniGameMusicOn(bool value) {

            MiniGameMusicOn = value;
            UserPrefs.SetBool(MINIGAME_MUSIC_ON_KEY, value);
        }

        public void ToggleMute() {

            Muted = !Muted;
            UserPrefs.SetBool(MUTE_KEY, Muted);
            UserPrefs.SaveDelayed();
            ToggleMuteMusic = Muted;
            ToggleMuteSfx = Muted;
        }

        public bool ToggleMuteMusic {
            set {
                MutedMusic = value;
                UserPrefs.SetBool(MUTE_MUSIC_KEY, MutedMusic);
                UserPrefs.SaveDelayed();
                MainAudioPlayer.MuteMusic = MutedMusic;
                EventBus.FireEvent(CommonEvents.SOUND_MUSIC_MUTE, MutedMusic);
            }
        }

        public virtual bool ToggleMuteSfx {
            set {
                MutedSfx = value;
                UserPrefs.SetBool(MUTE_SFX_KEY, MutedSfx);
                UserPrefs.SaveDelayed();
                MainAudioPlayer.MuteSfx = MutedSfx;
                EventBus.FireEvent(CommonEvents.SOUND_SFX_MUTE, MutedSfx);
            }
        }

        public AudioClip GetAudioClip(string name) {
            return Resources.Load(name, typeof(AudioClip)) as AudioClip;
        }
    }
}
