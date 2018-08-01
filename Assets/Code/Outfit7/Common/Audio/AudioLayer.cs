using UnityEngine;

namespace Outfit7.Audio {
    public partial class MainAudioPlayer {

        private struct AudioLayer {

            private AudioSource[] AudioSources;
            private int Index;
            private float Volume;
            private float FadeInSpeed;
            private float FadeOutSpeed;

            private int NextIndex { get { return Index > 0 ? 0 : 1; } }

            public float UserVolume { get; set; }

            public AudioSource AudioSource { get { return AudioSources[Index]; } }

            public bool IsPlaying { get { return (AudioSources[0] != null && AudioSources[0].isPlaying && AudioSources[0].volume > 0.0f) || (AudioSources[1] != null && AudioSources[1].isPlaying && AudioSources[1].volume > 0.0f); } }

            public void Initialize(GameObject gameObject) {
                AudioSources = new AudioSource[2];
                for (int i = 0; i < AudioSources.Length; i++) {
                    AudioSources[i] = gameObject.AddComponent<AudioSource>();
                    AudioSources[i].playOnAwake = false;
                }
                Index = 0;
                Volume = 1.0f;
                UserVolume = 1.0f;
            }

            public void Mute(bool mute) {
                for (int i = 0; i < AudioSources.Length; i++) {
                    AudioSources[i].mute = mute;
                }
            }

            public void Update(float deltaTime) {
                if (AudioSource == null) {
                    return;
                }
                // Update active audio source
                AudioSource ActiveAudioSource = AudioSource;
                if (ActiveAudioSource.isPlaying) {
                    if (FadeInSpeed > 0.0f) {
                        Volume = Mathf.Min(1.0f, Volume + FadeInSpeed * deltaTime);
                    }
                    ActiveAudioSource.volume = Volume * UserVolume;
                } else {
                    ActiveAudioSource.Stop();
                    ActiveAudioSource.clip = null;
                }
                // Update inactive audio source
                if (FadeOutSpeed > 0.0f) {
                    AudioSource InactiveAudioSource = AudioSources[NextIndex];
                    InactiveAudioSource.volume = Mathf.Max(0.0f, InactiveAudioSource.volume - FadeOutSpeed * deltaTime);
                    // If volume gets to zero or is not playing, reset it
                    if (InactiveAudioSource.volume <= 0.0f || !InactiveAudioSource.isPlaying) {
                        InactiveAudioSource.Stop();
                        InactiveAudioSource.volume = 0.0f;
                        FadeOutSpeed = 0.0f;
                    }
                }
            }

            public AudioSource Play(AudioClip audioClip, bool loop, bool mute, float pitch, float userVolume, float fadeInTime) {

                if (AppPaused) {
                    return null;
                }

                if (AudioSource == null) {
                    return null;
                }
                FadeInSpeed = fadeInTime > 0.0f ? 1.0f / fadeInTime : 0.0f;
                AudioSource audioSource = AudioSource;
                if (FadeInSpeed > 0) {
                    Volume = 0.0f;
                } else {
                    Volume = 1.0f;
                }
                UserVolume = userVolume;
                audioSource.clip = audioClip;
                audioSource.loop = loop;
                audioSource.pitch = pitch;
                audioSource.volume = Volume * userVolume;
                audioSource.time = 0.0f;
                audioSource.Play();
                return AudioSource;
            }

            public void Stop(float fadeOutTime) {
                if (AudioSource == null) {
                    return;
                }
                if (fadeOutTime > 0.0f) {
                    FadeOutSpeed = 1.0f / fadeOutTime;
                } else {
                    // Stop sound
                    FadeOutSpeed = 0.0f;
                    AudioSource.Stop();
                    AudioSource.volume = 0.0f;
                    AudioSource.clip = null;
                }
                // Set next audio source
                Index = NextIndex;
                AudioSource.Stop();
                AudioSource.volume = 0.0f;
                AudioSource.clip = null;
            }

            public void StopAll() {
                if (AudioSource == null) {
                    return;
                }
                for (int i = 0; i < AudioSources.Length; i++) {
                    AudioSources[i].Stop();
                    AudioSources[i].clip = null;
                }
            }

            //            public void OnGUI() {
            //                for( int i=0;i<AudioSources.Length;i++ ) {
            //                    AudioSource audioSource = AudioSources[i];
            //                    GUI.color = Index == i ? Color.green : Color.black;
            //                    GUILayout.Label( string.Format("Clip:{0} {1} {2}/{3}", audioSource.isPlaying ? audioSource.clip.name : "none", audioSource.volume, audioSource.time, audioSource.clip!=null ? audioSource.clip.length : 0.0f));
            //                }
            //            }
        }
    }
}
