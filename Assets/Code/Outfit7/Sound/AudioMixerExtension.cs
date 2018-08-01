using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using Outfit7.Util;

namespace Outfit7.Audio {

    public static class AudioMixerExtension {

        const string Tag = "AudioMixerExtension";

        const int InitSize = 8;
        public const float ParameterValueMin = -80.0f;
        public const float ParameterValueMax = 0.0f;
        public const float DefaultBlendTime = 0.75f;

        class AudioMixerGroupKey {
            public AudioMixer Mixer;
            public string Name;
        }

        class AudioMixerGroupState {
            public AudioMixerGroupKey Key = new AudioMixerGroupKey();
            public int MutedRefCount;

            public AudioMixerGroupState() {
                MutedRefCount = 0;
            }

            public void Mute(bool mute, bool instant = false) {
                if (mute) {
                    MutedRefCount++;
                    if (MutedRefCount == 1) {
                        if (!Key.Mixer.SetFloat(Key.Name, ParameterValueMin, instant == true ? 0.0f : DefaultBlendTime)) {
                            O7Log.WarnT(Tag, "Failed to set parameter {0}", Key.Name);
                        }
                    }
                } else {
                    MutedRefCount--;
                    //
                    if (MutedRefCount < 0) {
                        MutedRefCount = 0;
                    }
                    //
                    if (MutedRefCount == 0) {
                        if (!Key.Mixer.SetFloat(Key.Name, ParameterValueMax, instant == true ? 0.0f : DefaultBlendTime)) {
                            O7Log.WarnT(Tag, "Failed to set parameter {0}", Key.Name);
                        }
                    }
                }
            }

            public bool IsMuted() {
                return MutedRefCount > 0;
            }
        }

        class ParameterObject {
            public AudioMixerGroupKey Key = new AudioMixerGroupKey();
            public float TargetValue;
            public float BlendTime;
            float BlendTimer;
            float StartValue;

            public bool Init(AudioMixer mixer, string name, float targetValue, float blendTime) {
                Key.Mixer = mixer;
                Key.Name = name;
                TargetValue = targetValue;
                BlendTime = blendTime;
                BlendTimer = 0.0f;
                return Key.Mixer.GetFloat(name, out StartValue);
            }

            public bool OnUpdate(float deltaTime) {
                BlendTimer += deltaTime;
                float rel = Mathf.Clamp(BlendTimer / BlendTime, 0.0f, 1.0f);
                float val = Mathf.Lerp(StartValue, TargetValue, rel);
                if (!Key.Mixer.SetFloat(Key.Name, val)) {
                    O7Log.WarnT(Tag, "Failed to set parameter {0}", Key.Name);
                }
                if (BlendTimer < BlendTime) {
                    return false;
                } else {
                    return true;
                }
            }
        }

        private static List<AudioMixerGroupState> AudioMixerGroupStates = new List<AudioMixerGroupState>(InitSize);
        private static List<AudioMixerGroupState> AudioMixerGroupStatePool = new List<AudioMixerGroupState>(InitSize);
        //
        private static List<ParameterObject> ParameterObjectPool = new List<ParameterObject>(InitSize);
        private static List<ParameterObject> ParameterObjectActive = new List<ParameterObject>(InitSize);

        private static void GrowGroupStatePool() {
            for (int a = 0; a < InitSize; a++) {
                AudioMixerGroupState s = new AudioMixerGroupState();
                AudioMixerGroupStatePool.Add(s);
            }
        }

        private static void GrowParameterObjectPool() {
            for (int a = 0; a < InitSize; a++) {
                ParameterObject po = new ParameterObject();
                ParameterObjectPool.Add(po);
            }
        }

        public static void OnInit() {
            GrowGroupStatePool();
            GrowParameterObjectPool();
        }

        public static void OnUpdate(float deltaTime) {
            for (int a = 0; a < ParameterObjectActive.Count; a++) {
                ParameterObject po = ParameterObjectActive[a];
                if (po.OnUpdate(deltaTime)) {
                    ParameterObjectActive.RemoveAt(a);
                    ParameterObjectPool.Add(po);
                    a--;
                }
            }
        }

        private static AudioMixerGroupState FindGroupState(AudioMixer mixer, string name) {
            for (int a = 0; a < AudioMixerGroupStates.Count; a++) {
                AudioMixerGroupState s = AudioMixerGroupStates[a];
                if (s.Key.Name == name && s.Key.Mixer == mixer) {
                    return s;
                }
            }
            //
            if (AudioMixerGroupStatePool.Count == 0) {
                GrowGroupStatePool();
                O7Log.WarnT(Tag, "AudioMixerExtension - growing group state pool!");
            }
            //
            int idx = AudioMixerGroupStatePool.Count - 1;
            AudioMixerGroupState ps = AudioMixerGroupStatePool[idx];
            AudioMixerGroupStatePool.RemoveAt(idx);
            AudioMixerGroupStates.Add(ps);
            ps.Key.Mixer = mixer;
            ps.Key.Name = name;
            return ps;
        }

        private static ParameterObject FindParameterObject(AudioMixer mixer, string name) {
            for (int a = 0; a < ParameterObjectActive.Count; a++) {
                ParameterObject po = ParameterObjectActive[a];
                if (po.Key.Name == name && po.Key.Mixer == mixer) {
                    return po;
                }
            }
            //
            return null;
        }

        private static ParameterObject GetParameterObject(AudioMixer mixer, string name) {
            ParameterObject po = FindParameterObject(mixer, name);
            if (po != null) {
                return po;
            }
            //
            if (ParameterObjectPool.Count == 0) {
                GrowParameterObjectPool();
                O7Log.WarnT(Tag, "AudioMixerExtension - growing parameter object pool!");
            }
            //
            int idx = ParameterObjectPool.Count - 1;
            ParameterObject ppo = ParameterObjectPool[idx];
            ParameterObjectPool.RemoveAt(idx);
            ParameterObjectActive.Add(ppo);
            return ppo;
        }

        public static bool SetFloat(this AudioMixer mixer, string name, float targetValue, float blendTime) {
            // if blendtime is set, reuse param object if found
            if (blendTime > 0.0f) {
                ParameterObject po = GetParameterObject(mixer, name);
                return po.Init(mixer, name, targetValue, blendTime);
            } else {
                // if instant, kill param object if found and set direct
                ParameterObject po = FindParameterObject(mixer, name);
                if (po != null) {
                    ParameterObjectPool.Remove(po);
                    ParameterObjectActive.Add(po);
                }
                return mixer.SetFloat(name, targetValue);
            }
        }

        public static bool IsMuted(this AudioMixer mixer, string name) {
            AudioMixerGroupState s = FindGroupState(mixer, name);
            return s.IsMuted();
        }

        public static void Mute(this AudioMixer mixer, string name, bool mute, bool instant = false) {
            AudioMixerGroupState s = FindGroupState(mixer, name);
            s.Mute(mute, instant);
        }
    }
}