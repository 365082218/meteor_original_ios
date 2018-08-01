
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Outfit7.Util;

namespace Outfit7.Audio {
    public partial class MainAudioPlayer : MonoBehaviour {

        public bool DebugInfo = false;

        public const string Tag = "MainAudioPlayer";

        public AudioManager AudioManager { get; set; }

        private static bool AppPaused;
        public const int MaxSfxSounds = 7;
        private List<O7AudioSource> SfxSounds;
        private AudioLayer[] AnimationAudioLayers = new AudioLayer[2];
        private bool muteSfx;
        private bool muteMusic;
        private O7AudioSource BackgroundMusic;

#region properties

        public float BackgroundMusicVolume {
            get { return BackgroundMusic == null ? -1 : BackgroundMusic.Volume; }
            set {
                if (BackgroundMusic != null) {
                    BackgroundMusic.Volume = value;
                }
            }
        }

        public float BackgroundMusicPitch {
            get { return BackgroundMusic == null ? -1 : BackgroundMusic.Pitch; }
            set {
                if (BackgroundMusic != null) {
                    BackgroundMusic.Pitch = value;
                }
            }
        }

        public bool MuteMusic {
            get {
                return muteMusic;
            }
            set {
                if (muteMusic != value) {
                    muteMusic = value;
                    BackgroundMusic.Mute = muteMusic;
                }
            }
        }


        public bool MuteSfx {
            get {
                return muteSfx;
            }
            set {
                if (muteSfx != value) {
                    muteSfx = value;
//                    foreach (O7AudioSource audioSfx in SfxSounds) { // find stopped audio sources
                    for (int i = 0; i < SfxSounds.Count; i++) {
                        O7AudioSource audioSfx = SfxSounds[i];
                        if (audioSfx.IsPlaying) {
                            audioSfx.Mute = muteSfx;
                        }
                    }

                    for (int i = 0; i < AnimationAudioLayers.Length; i++) {
                        AnimationAudioLayers[i].Mute(muteSfx);
                    }
                }
            }
        }

#endregion

        public virtual void Init() {
        }

        public void Awake() {

            DontDestroyOnLoad(this);
            BackgroundMusic = new O7AudioSource(gameObject.AddComponent<AudioSource>());
            for (int i = 0; i < AnimationAudioLayers.Length; i++) {
                AnimationAudioLayers[i].Initialize(gameObject);
            }
            BackgroundMusic.PlayOnAwake = false;
            SfxSounds = new List<O7AudioSource>(MaxSfxSounds);
        }

        public void Start() {
            MuteSfx = AudioManager.MutedSfx;
            MuteMusic = AudioManager.MutedMusic;
        }

        public void AnimationSoundPlay(string name, bool loop, float fadeInTime, int layer) {
            AnimationSoundPlay(AudioManager.GetAudioClip(name), loop, fadeInTime, layer);
        }

        public void AnimationSoundPlay(string name, bool loop) {
            AnimationSoundPlay(name, loop, 0.0f, 0);
        }

        public void AnimationSoundPlay(AudioClip audioClip, bool loop, float fadeInTime, int layer) {
            AnimationAudioLayers[layer].Play(audioClip, loop, MuteSfx, 1.0f, 1.0f, fadeInTime);
        }

        public void AnimationSoundPlay(AudioClip audioClip, bool loop) {
            AnimationSoundPlay(audioClip, loop, 0.0f, 0);
        }

        public void AnimationSoundPause(int layer) {
            AudioSource audioSource = AnimationAudioLayers[layer].AudioSource;
            audioSource.Pause();
        }

        public void AnimationSoundPause() {
            AnimationSoundPause(0);
        }

        public void AnimationSoundResume(int layer) {

            if (AppPaused) {
                return;
            }

            AudioSource audioSource = AnimationAudioLayers[layer].AudioSource;
            audioSource.Play();
        }

        public void AnimationSoundResume() {
            AnimationSoundResume(0);
        }

        public void AnimationSoundStop(float fadeOutTime, int layer) {
            AnimationAudioLayers[layer].Stop(fadeOutTime);
        }

        public void AnimationSoundStop() {
            AnimationSoundStop(0.0f, 0);
        }

        public void AnimationSoundSetVolume(float volume, int layer) {
            AnimationAudioLayers[layer].UserVolume = volume;
        }

        public void AnimationSoundSetVolume(float volume) {
            AnimationSoundSetVolume(volume, 0);
        }

        public void AnimationSoundSetPitch(float pitch, int layer) {
            AudioSource audioSource = AnimationAudioLayers[layer].AudioSource;
            audioSource.pitch = pitch;
        }

        public void AnimationSoundSetPitch(float pitch) {
            AnimationSoundSetPitch(pitch, 0);
        }

        public bool AnimationSoundIsPlayingClip(AudioClip audioClip, int layer) {
            AudioSource audioSource = AnimationAudioLayers[layer].AudioSource;
            return audioSource.isPlaying && audioSource.clip == audioClip;
        }

        public bool AnimationSoundIsPlayingClip(AudioClip audioClip) {
            return AnimationSoundIsPlayingClip(audioClip, 0);
        }

        public bool AnimationSoundIsPlaying(int layer) {
            AudioSource audioSource = AnimationAudioLayers[layer].AudioSource;
            return audioSource.isPlaying;
        }

        public bool AnimationSoundIsPlaying() {
            return AnimationSoundIsPlaying(0);
        }

        public void AnimationSoundStopAllLayers() {
            for (int i = 0; i < AnimationAudioLayers.Length; i++) {
                AnimationAudioLayers[i].StopAll();
            }
        }

        public void PlayOneShotSoundSFX(string name) {
            PlayOneShotSoundSFX(name, true);
        }

        public void PlayOneShotSoundSFX(string name, bool stopOnSceneChange) {
            AudioClip clip = AudioManager.GetAudioClip(name);
            PlayOneShotSoundSFX(clip, 1, stopOnSceneChange);
        }

        public void PlayOneShotSoundSFX(AudioClip[] clip) {
            PlayOneShotSoundSFX(StaticRandom.NextElement(clip));
        }

        public void PlayOneShotSoundSFX(AudioClip clip, bool stopOnSceneChange = false) {
            PlayOneShotSoundSFX(clip, 1f, stopOnSceneChange);
        }

        public void PlayOneShotSoundSFX(AudioClip clip, float pitch, float delay) {
            StartCoroutine(PlayDelayed(clip, delay, pitch));
        }

        public void PlayOneShotSoundSFX(AudioClip clip, float pitch) {
            PlayOneShotSoundSFX(clip, pitch, true);
        }

        private IEnumerator PlayDelayed(List<string> list, float delay) {
            yield return new WaitForSeconds(delay);
            PlayOneShotSoundSFX(list);
        }

        private IEnumerator PlayDelayed(AudioClip clip, float delay, float pitch) {
            yield return new WaitForSeconds(delay);
            PlayOneShotSoundSFX(clip, pitch);
        }

        public void PlayOneShotSoundSFX(List<string> list, float delay) {
            StartCoroutine(PlayDelayed(list, delay));
        }

        public void PlayOneShotSoundSFX(List<string> list) {
            PlayOneShotSoundSFX(list, true);
        }

        public void PlayOneShotSoundSFX(List<string> list, bool stopOnSceneChange) {

            string pickedSound = list[StaticRandom.Next() % list.Count];
            PlayOneShotSoundSFX(pickedSound, stopOnSceneChange);
        }

        public void PlayOneShotSoundSFX(List<AudioClip> list) {

            AudioClip pickedSound = list[StaticRandom.Next() % list.Count];
            PlayOneShotSoundSFX(pickedSound);
        }

        public void PlayOneShotSoundSFX(AudioClip clip, float pitch, bool stopOnSceneChange) {
            PlayOneShotSoundSFX(clip, pitch, stopOnSceneChange, 1);
        }

        public void PlayOneShotSoundSFX(AudioClip clip, float pitch, bool stopOnSceneChange, float volume) {

            if (AppPaused) {
                return;
            }

            O7AudioSource reusedAudioSource = null;

            // reuse if any available
            for (int i = 0; i < SfxSounds.Count; i++) { // find stopped audio sources
                O7AudioSource audioSfx = SfxSounds[i];
                if (!audioSfx.IsPlaying) {
                    reusedAudioSource = audioSfx;
                    break;
                }
            }

            if (reusedAudioSource != null) { // reuse old
                SfxSounds.Remove(reusedAudioSource);

            } else { // reuse last or create new

                if (SfxSounds.Count > MaxSfxSounds) {
                    SfxSounds[MaxSfxSounds - 1].Stop();
                    reusedAudioSource = SfxSounds[MaxSfxSounds - 1];
                    SfxSounds.RemoveAt(MaxSfxSounds - 1);
                }

                if (reusedAudioSource == null) {
                    reusedAudioSource = new O7AudioSource(gameObject.AddComponent<AudioSource>());
                }
            }

            SfxSounds.Insert(0, reusedAudioSource);
            SfxSounds[0].Clip = clip;
            SfxSounds[0].PlayOnAwake = false;
            SfxSounds[0].Loop = false;
            SfxSounds[0].Mute = MuteSfx;
            SfxSounds[0].Pitch = pitch;
            SfxSounds[0].Volume = volume;
            SfxSounds[0].Play(stopOnSceneChange);
        }

        public void PlayBackgroundMusic(List<string> list, bool loop) {
            PlayBackgroundMusic(list, loop, false);
        }

        public void PlayBackgroundMusic(List<string> list, bool loop, bool resumeIfPaused) {
            PlayBackgroundMusic(list, loop, resumeIfPaused, true);
        }

        public void PlayBackgroundMusic(List<string> list, bool loop, bool resumeIfPaused, bool stopOnSceneChange) {
            string pickedSound = list[StaticRandom.Next() % list.Count];
            PlayBackgroundMusic(pickedSound, loop, resumeIfPaused, stopOnSceneChange);
        }

        public void PlayBackgroundMusic(List<AudioClip> list, bool loop) {

            AudioClip pickedSound = list[StaticRandom.Next() % list.Count];
            PlayBackgroundMusic(pickedSound, loop);
        }

        public void PlayBackgroundMusic(string name, bool loop) {
            PlayBackgroundMusic(name, loop, false);
        }

        public void PlayBackgroundMusic(string name, bool loop, bool resumeIfPaused) {
            PlayBackgroundMusic(name, loop, resumeIfPaused, true);
        }

        public void PlayBackgroundMusic(string name, bool loop, bool resumeIfPaused, bool stopOnSceneChange) {
            PlayBackgroundMusic(AudioManager.GetAudioClip(name), loop, resumeIfPaused, stopOnSceneChange);
        }

        public void PlayBackgroundMusic(AudioClip audioClip, bool loop) {
            PlayBackgroundMusic(audioClip, loop, false);
        }

        public void PlayBackgroundMusic(AudioClip audioClip, bool loop, bool resumeIfPaused) {
            PlayBackgroundMusic(audioClip, loop, resumeIfPaused, true);
        }

        public void PlayBackgroundMusic(AudioClip audioClip, bool loop, bool resumeIfPaused, bool stopOnSceneChange) {
            PlayBackgroundMusic(audioClip, loop, resumeIfPaused, stopOnSceneChange, 0);
        }

        public void PlayBackgroundMusic(AudioClip audioClip, bool loop, bool resumeIfPaused, bool stopOnSceneChange, float startAtTime) {
            if (AppPaused) {
                return;
            }

            if (O7Log.DebugEnabled)
                O7Log.DebugT(Tag, "PlayBackgroundMusic clip {0} resume {1}", audioClip.name, resumeIfPaused);

            if (resumeIfPaused && BackgroundMusic.Clip != null && BackgroundMusic.Clip.name == audioClip.name) {
                // just resume
            } else {
                BackgroundMusic.Clip = audioClip;

            }

            BackgroundMusic.Loop = loop;
            BackgroundMusic.Mute = MuteMusic;
            BackgroundMusic.Play(resumeIfPaused, stopOnSceneChange, startAtTime);
        }

        public bool IsPlayingSfx() {
            for (int i = 0; i < SfxSounds.Count; i++) {
                if (SfxSounds[i].IsPlaying) {
                    return true;
                }
            }
            return false;
        }

        public bool IsPlayingSfxClip(AudioClip audioClip) {
            for (int i = 0; i < SfxSounds.Count; i++) {
                if (SfxSounds[i].IsPlaying && SfxSounds[i].Clip == audioClip) {
                    return true;
                }
            }
            return false;
        }

        public bool IsPlayingBackgroundMusic() {
            return BackgroundMusic.IsPlaying;
        }

        public bool IsPlaying() {

            if (IsPlayingSfx()) {
                return true;
            }

            for (int i = 0; i < AnimationAudioLayers.Length; i++) {
                if (AnimationAudioLayers[i].IsPlaying) {
                    return true;
                }
            }

            return IsPlayingBackgroundMusic();
        }

        public void PauseBackgroudMusic() {
            BackgroundMusic.Pause();
        }

        public void StopBackgroundMusic() {
            StopBackgroundMusic(true, true);
        }

        public void StopBackgroundMusic(bool force, bool sceneChanged) {
            O7Log.DebugT(Tag, "StopBackgroundMusic");

			if (gameObject != null && gameObject.activeSelf) StopAllCoroutines();

            if (BackgroundMusic != null) {
                if (force || BackgroundMusic.StopOnSceneChange == sceneChanged) {
                    BackgroundMusic.Stop();
                    BackgroundMusic.Clip = null;
                }
            }
        }

        public void StopAllSounds() {
            StopBackgroundMusic();
            StopAllSfx();
            AnimationSoundStopAllLayers();
        }

        public void StopSfxAndBackgroundSoundsOnSceneChange() {
            StopAllSfx(false, true);
            StopBackgroundMusic(false, true);
        }

        public void StopAllSfx() {
            StopAllSfx(true, true);
        }

        public void StopAllSfx(bool forceStop, bool sceneChanged) {
            StopAllCoroutines();
            if (SfxSounds != null) {
                for (int i = 0; i < SfxSounds.Count; i++) {

                    if (forceStop || SfxSounds[i].StopOnSceneChange == sceneChanged) {
                        SfxSounds[i].Stop();
                        SfxSounds[i].Clip = null;
                    }
                }
            }
        }

        void OnApplicationPause(bool paused) {
            AppPaused = paused;
        }

        void LateUpdate() {
            // Update animation audio layers
            float deltaTime = Time.smoothDeltaTime;
            for (int i = 0; i < AnimationAudioLayers.Length; i++) {
                AnimationAudioLayers[i].Update(deltaTime);
            }
        }

        //        void OnGUI() {
        //            // Debug info
        //            if (DebugInfo) {
        //                // Animation audio layers debug info
        //                GUILayout.BeginArea(new Rect(10.0f, Screen.height / 4.0f, Screen.width - 20.0f, Screen.height / 2.0f), GUI.skin.box);
        //                for (int i=0; i<AnimationAudioLayers.Length; i++) {
        //                    GUI.color = Color.yellow;
        //                    GUILayout.Label(string.Format("AnimationAudioLayer:{0}", i));
        //                    AnimationAudioLayers[i].OnGUI();
        //                }
        //                // Sound debug info
        //                GUI.color = IsPlayingBackgroundMusic() ? Color.green : Color.cyan;
        //                GUILayout.Label(string.Format("BackgroundMusic:{0}", IsPlayingBackgroundMusic() ? BackgroundMusic.Clip.name : "none"));
        //                for (int i=0; i<SfxSounds.Count; i++) {
        //                    O7AudioSource audioSource = SfxSounds[i];
        //                    GUI.color = audioSource.IsPlaying ? Color.green : Color.cyan;
        //                    GUILayout.Label(string.Format("Sound#{0}:{1} {2}/{3}", i, audioSource.IsPlaying ? audioSource.Clip.name : "none", audioSource.Time, audioSource.Clip != null ? audioSource.Clip.length : 0.0f));
        //                }
        //                GUILayout.EndArea();
        //            }
        //        }
    }
}
