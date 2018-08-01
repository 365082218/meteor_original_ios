using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;
using Outfit7.Util;

namespace Outfit7.Audio {

    public abstract class AbstractAudioEventData : ScriptableObject {
        public abstract AudioEventData GetAudioEventData();
    }

    [System.Serializable]
    public class AudioEventDataClip {
        public AssetReference ClipReference = new AssetReference();
        public float Probability = 100.0f;
        public float Volume = 1.0f;
        public float Pitch = 1.0f;
        //
        public AudioClip Clip { get { return ClipReference.Load(typeof(AudioClip)) as AudioClip; } }

        public List<float> StartPoints = new List<float>();
        public float BeatsPerMinute = 0.0f;
        public float LeadInLength = 0.0f;
        public float LeadOutLength = 0.0f;

        public float GetRandomStartPoint() {
            if (StartPoints.Count == 0) {
                return 0.0f;
            }
            int idx = UnityEngine.Random.Range(0, StartPoints.Count);
            return StartPoints[idx];
        }

        public float GetBeatsPerMinuteTimer() {
            return 60.0f / BeatsPerMinute * 4.0f;
        }
    }

    [System.Serializable]
    public class AudioEventDataFade {
        public AudioMinMax FadeInTime = new AudioMinMax(0.0f, 10.0f, 0.0f, 0.0f);
        public AudioMinMax FadeOutTime = new AudioMinMax(0.0f, 10.0f, 0.0f, 0.0f);
        public AnimationCurve FadeInCurve = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 1.0f));
        public AnimationCurve FadeOutCurve = new AnimationCurve(new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 0.0f));
    }

    [System.Serializable]
    public class AudioSourceData {
        public AudioMixerGroup MixerGroup = null;
        //
        public bool bypassEffects = false;
        public bool bypassListenerEffects = false;
        public bool bypassReverbZones = false;
        //
        [Range(0.0f, 5.0f)]
        public float dopplerLevel = 0.0f;
        [Range(-1.0f, 1.0f)]
        public float panStereo = 0.0f;
        [Range(0.0f, 1.0f)]
        public float spatialBlend = 0.0f;
        [Range(0.0f, 360.0f)]
        public float spread;
        [Range(0.0f, 1.0f)]
        public float reverbZoneMix = 0.5f;
        //
        public float minDistance = 0.0f;
        public float maxDistance = 100.0f;
        public AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;
        public AnimationCurve RolloffCurve = new AnimationCurve(new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 0.0f));
        public AnimationCurve ReverbZoneMixCurve = new AnimationCurve(new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f));
        public AnimationCurve SpatialBlendCurve = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 0.0f));
        public AnimationCurve SpreadCurve = new AnimationCurve(new Keyframe(0.0f, 1.0f), new Keyframe(1.0f, 1.0f));
    }

    [System.Serializable]
    public class CustomPlayModeEntry {
        public EPlayMode PlayMode;
        public int Value;

        public CustomPlayModeEntry(EPlayMode pm, int v) {
            PlayMode = pm;
            Value = v;
        }
    }

    public class AudioEventData : AbstractAudioEventData {

#if UNITY_EDITOR
        [HideInInspector]
        public bool ShowFade = false;
        [HideInInspector]
        public bool ShowCrossFade = false;
        [HideInInspector]
        public bool ShowSource = false;
        [HideInInspector]
        public bool ShowAudioDefinitions = false;
#endif

        public List<AudioEventDataClip> AudioDefinitions = new List<AudioEventDataClip>();
        public AudioSourceData SourceData = new AudioSourceData();

        public bool Use2DCameraPanning = false;
        public EPlayMode PlayMode = EPlayMode.Sequential;
        public ELoopMode LoopMode = ELoopMode.OneShot;
        public EPriority Priority = EPriority.Normal;
        [Tooltip("0 - unlimited"), Range(0, 16)]
        public int InstanceLimit = 0;

        public List<CustomPlayModeEntry> CustomPlayModeList = new List<CustomPlayModeEntry>();

        [HideInInspector]
        private int InstanceCounterInternal = 0;
        public int InstanceCounter {
            get { 
                return InstanceCounterInternal;
            }
            set {
                InstanceCounterInternal = value;
            }
        }

        [Tooltip("0 - ignore; if set above 0.0, overlapping events will fail to play"), Range(0f, 1.0f)]
        public float OverlappingLimitRelativeLength = 0.0f;

        public AudioMinMax Volume = new AudioMinMax(0.0f, 1.0f, 1.0f, 1.0f);
        public AudioMinMax Pitch = new AudioMinMax(0.0f, 1.0f, 1.0f, 1.0f);

        public AudioEventDataFade FadeInOut = new AudioEventDataFade();
        public AudioEventDataFade CrossFade = new AudioEventDataFade();

        public EAudioEventDataAttributes Attributes = EAudioEventDataAttributes.None;

        public override AudioEventData GetAudioEventData() {
            return this;
        }
    }
}