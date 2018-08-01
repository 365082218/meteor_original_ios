using UnityEngine;
using System.Collections;
using Outfit7.Util;

namespace Outfit7.Audio {
    public partial class MainAudioPlayer {

        protected class O7AudioSource {

            private float PauseTime;
            private AudioSource AudioSource;
            private AudioClip AudioClip;

            public bool StopOnSceneChange { get; private set; }

            public AudioClip Clip {
                get {
                    return AudioClip;
                }
                set {
                    AudioClip = value;
                    AudioSource.clip = AudioClip;
                }
            }

            public bool PlayOnAwake {
                get {
                    return AudioSource.playOnAwake;
                }
                set {
                    AudioSource.playOnAwake = value;
                }
            }

            public bool Loop {
                get {
                    return AudioSource.loop;
                }
                set {
                    AudioSource.loop = value;
                }
            }

            public bool Mute {
                get {
                    return AudioSource.mute;
                }
                set {
                    AudioSource.mute = value;
                }
            }

            public bool IsPlaying {
                get {
                    return AudioSource.isPlaying;
                }
            }

            public float Time {
                get {
                    return AudioSource.time;
                }
            }

            public float Pitch {
                get {
                    return AudioSource.pitch;
                }
                set {
                    AudioSource.pitch = value;
                }
            }

            public float Volume {
                get {
                    return AudioSource.volume;
                }
                set {
                    if (AudioSource == null) {
                        return;
                    }
                    AudioSource.volume = value;
                }
            }

            public O7AudioSource(AudioSource audioSource) {
                AudioSource = audioSource;
            }

            public void Play() {
                Play(true);
            }

            public void Stop() {
                AudioClip = null;
                AudioSource.Stop();
            }

            public void Pause() {

                if (AudioSource.isPlaying) {
                    PauseTime = AudioSource.time;
                    AudioSource.Stop(); // due too unity bug - not pausing correctly
                }
            }

            public void Play(bool stopOnSceneChange) {
                Play(false, stopOnSceneChange);
            }

            public void Play(bool resumePlaying, bool stopOnSceneChange) {
                Play(resumePlaying, stopOnSceneChange, 0);
            }

            public void Play(bool resumePlaying, bool stopOnSceneChange, float startAtTime) {
                StopOnSceneChange = stopOnSceneChange;

                if (resumePlaying) {
                    if (AudioSource.clip != Clip) {
                        AudioSource.clip = Clip;
                    } else if (AudioSource.isPlaying) {
                        return;
                    }

                    AudioSource.time = PauseTime;
                }

                if (PauseTime == 0) {
                    AudioSource.time = startAtTime;
                }
                PauseTime = 0;

//                if (O7Log.DebugEnabled && AudioSource.clip != null) {
//                    O7Log.Debug("Playing audio source '{0}' at time = {1}", AudioSource.clip.name, AudioSource.time);
//                }

                AudioSource.Play();
            }
        }
    }
}
