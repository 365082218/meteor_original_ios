using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;
using System;

namespace Outfit7.Audio {

    public static class AudioEventConstants {
        public static double StichingOverlappingOffset = 1 / 60 * 2; // 2 frames at 60fps
        public static double FadeCompensation = 1 / 60; // 1 frame at 60fps
    }

    public enum EPlayMode {
        Sequential,
        Probability,
        RandomNoRepeat,
        UserDefined,
        Custom
    }

    public enum EFadeDirection {
        Idle,
        FadeIn,
        FadeOut
    }

    public enum ELoopMode {
        OneShot,
        LoopSingle,
        LoopCycle,
    }

    public enum EPriority {
        Low,
        Normal,
        High,
        Epic
    }

    [Flags]
    public enum EAudioEventAttributes {
        None = 0x00,
        AutoDelete = 0x01,
        StopOnSceneChange = 0x02
    }

    [Flags]
    public enum EAudioEventState {
        Stopped = 0x00,
        Playing = 0x01,
        Paused = 0x02,
        Stopping = 0x04,
        DelayedPlay = 0x08,
        DelayedStop = 0x10,
        DelayedFade = 0x20,
        CrossFade = 0x40,
    }

    [Flags]
    public enum EAudioEventDataAttributes {
        None = 0x00,
        StopOnSceneChange = 0x01,
    }

    [System.Serializable]
    public class AudioMinMax {
        public float MinLimit = 1.0f;
        public float MaxLimit = 1.0f;
        public float Min = 1.0f;
        public float Max = 1.0f;

        public AudioMinMax() {
            MinLimit = 0.0f;
            MaxLimit = 1.0f;
            Min = Max = 1.0f;
        }

        public AudioMinMax(float minl, float maxl, float minv, float maxv) {
            MinLimit = minl;
            MaxLimit = maxl;
            Min = minv;
            Max = maxv;
        }

        public float Value {
            get { 
                return UnityEngine.Random.Range(Min, Max);
            }
            set { }
        }
    }
}

