//#define VERBOSE

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;
using Outfit7.Util;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Outfit7.Audio {

    // TODO:
    // - check CopyAudioSourceSettings unity defines


    public class AudioEventFade {
        private float FadeVolumeInternal;
        private AudioEventDataFade FadeData = null;
        private EFadeDirection FadeDirectionInternal = EFadeDirection.Idle;
        private float FadeSpeed = 0.0f;
        private float Accu = 0.0f;

        public float FadeVolume {
            get {
                return FadeVolumeInternal;
            }
        }

        public EFadeDirection FadeDirection {
            get {
                return FadeDirectionInternal;
            }
        }

        public override string ToString() {
            return string.Format("[Volume={0:F2}, Mode={1}, Accu={2:F2}]", FadeVolume, FadeDirectionInternal.ToString(), Accu);
        }

        public void Initialize(AudioEventDataFade dataFade) {
            FadeData = dataFade;
            Reset();
        }

        public void Reset() {
            FadeVolumeInternal = 0.0f;
            FadeSpeed = 0.0f;
            Accu = 0.0f;
            FadeDirectionInternal = EFadeDirection.Idle;
        }

        public bool IsActive {
            get {
                return FadeDirectionInternal != EFadeDirection.Idle;
            }
        }

        public void StartFadeIn(bool skip = false, float fadeintime = -1.0f) {
            float fit = fadeintime;
            if (fadeintime < 0.0f) {
                fit = FadeData.FadeInTime.Value;
            }
            if (fit > 0.0f && !skip) {
                FadeSpeed = 1.0f / fit;
                FadeDirectionInternal = EFadeDirection.FadeIn;
                Accu = FadeVolumeInternal;
            } else {
                FadeVolumeInternal = 1.0f;
                FadeDirectionInternal = EFadeDirection.Idle;
            }
        }

        public void StartFadeOut(bool skip = false, float fadeouttime = -1.0f) {
            float fot = fadeouttime;
            if (fadeouttime < 0.0f) {
                fot = FadeData.FadeOutTime.Value;
            }
            if (fot > 0.0f && !skip) {
                FadeSpeed = 1.0f / fot;
                FadeDirectionInternal = EFadeDirection.FadeOut;
                Accu = FadeVolumeInternal;
            } else {
                FadeVolumeInternal = 0.0f;
                FadeDirectionInternal = EFadeDirection.Idle;
            }
        }

        public void OnUpdate(float dt) {
            switch (FadeDirectionInternal) {
                case EFadeDirection.FadeIn: {
                        Accu += FadeSpeed * dt;
                        FadeVolumeInternal = FadeData.FadeInCurve.Evaluate(Accu);
                        //
                        if (Accu >= 1.0f) {
                            FadeDirectionInternal = EFadeDirection.Idle;
                            Accu = 1.0f;
                        }
                        FadeVolumeInternal = Mathf.Clamp(FadeVolumeInternal, 0.0f, 1.0f);
                    }
                    break;
                case EFadeDirection.FadeOut: {
                        Accu -= FadeSpeed * dt;
                        FadeVolumeInternal = FadeData.FadeOutCurve.Evaluate(1.0f - Accu);
                        //
                        if (Accu <= 0.0f) {
                            FadeDirectionInternal = EFadeDirection.Idle;
                            Accu = 0.0f;
                        }
                        FadeVolumeInternal = Mathf.Clamp(FadeVolumeInternal, 0.0f, 1.0f);
                    }
                    break;
            }
        }
    }

    public class AudioEventInfo {
        public EAudioEventState EventState = EAudioEventState.Stopped;
        public AudioClip CurrentClip = null;
    }

    [AddComponentMenu("")]
    [System.Serializable]
    public class AudioEvent : MonoBehaviour {

        // If set, audio event will follow this transforms position/rotation
        public Transform FollowTransfrom = null;

        // playback source component
        public AudioSource SourceMain = null;
        public AudioSource SourceAux = null;

        // attributes
        public EAudioEventAttributes EventAttributes = EAudioEventAttributes.None;

        // actual event data handle
        public AudioEventData EventDataInternal = null;

        // user is responsible for delegates, make sure to remove them!
        public delegate void EventDestroy(AudioEvent ae);
        public EventDestroy OnEventDestroy;

        // volume & pitch
        private float VolumeInitial = 1.0f;
        private float VolumeInternal = 1.0f;
        private float PitchInitial = 1.0f;
        private float PitchInternal = 1.0f;
        private float PanStereoInternal = 0.0f;

        // play mode logic
        public AudioEventDataClip ClipDataMain = null;
        public AudioEventDataClip ClipDataAux = null;
        private int PlayModeIndex = 0;
        private List<byte> PlayModeIndexPool = null;
        private int CustomPlayModeIndex = 0;
        private int CustomPlayModeValue = 0;
        private float StartPoint = 0.0f;

        // fader
        private AudioEventFade FadeMain = new AudioEventFade();

        private AudioEventFade CrossFadeMain = new AudioEventFade();
        private AudioEventFade CrossFadeAux = new AudioEventFade();

        // state handling
        private EAudioEventState EventStateInternal = EAudioEventState.Stopped;

        // delay time markers
        private double StartDelayDspTime = 0.0;
        private double StopDelayDspTime = 0.0;

        // output info
        private AudioEventInfo OutInfo = null;

        // shared list for randomization
        private static List<byte> RandomizerIndexFill = new List<byte>(32);

        public EAudioEventState EventState {
            get {
                return EventStateInternal;
            }
        }

        public EFadeDirection EventFadeStatus {
            get {
                return FadeMain.FadeDirection;
            }
        }

        public float Volume {
            get {
                return VolumeInternal;
            }
            set {
                VolumeInternal = Mathf.Clamp(value, 0.0f, 1.0f);
            }
        }

        public float Pitch {
            get {
                return PitchInternal;
            }
            set {
                PitchInternal = Mathf.Clamp(value, 0.0f, 5.0f);
            }
        }

        public float PanStereo {
            get {
                return PanStereoInternal;
            }
            set {
                PanStereoInternal = Mathf.Clamp(value, -1.0f, 1.0f);
            }
        }

        public bool IsPlaying {
            get {
                return (EventStateInternal & EAudioEventState.Playing) == EAudioEventState.Playing;
            }
        }

        public bool IsStopping {
            get {
                return (EventStateInternal & EAudioEventState.Stopping) == EAudioEventState.Stopping;
            }
        }

        public bool IsStoppingOrStopped {
            get {
                return IsStopping || (EventStateInternal == EAudioEventState.Stopped);
            }
        }

        public bool IsAutoDelete {
            get {
                return (EventAttributes & EAudioEventAttributes.AutoDelete) == EAudioEventAttributes.AutoDelete;
            }
        }

        public void Release() {
            EventAttributes |= EAudioEventAttributes.AutoDelete;
        }

        public static double DspTime {
            get {
                return AudioSettings.dspTime;
            }
        }

        public bool Mute {
            set {
                SourceMain.mute = value;
                SourceAux.mute = value;
            }
        }

#if VERBOSE
        void CheckIfValid() {
            if ((EventStateInternal & EAudioEventState.Playing) == EAudioEventState.Playing) {
                if (!SourceMain.isPlaying && !SourceAux.isPlaying) {
                    int aaa = 667;
                    if (aaa != 667) {
                        aaa = 667;
                    }
                }
            }
        }
#endif

        public void ResetForPool() {
            VolumeInternal = VolumeInitial = 1.0f;
            PitchInternal = PitchInitial = 1.0f;
            ResetPlaybackMode();
            EventStateInternal = EAudioEventState.Stopped;
            EventAttributes = EAudioEventAttributes.None;
            OutInfo = null;
            EventDataInternal = null;
            StartPoint = 0.0f;
            SourceMain.Stop();
            SourceMain.clip = null;
            SourceMain.mute = false;
            SourceAux.Stop();
            SourceAux.clip = null;
            SourceAux.mute = false;
            StartDelayDspTime = 0.0;
            StopDelayDspTime = 0.0;
            FadeMain.Reset();
            CrossFadeMain.Reset();
            CrossFadeAux.Reset();
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            FollowTransfrom = null;
        }

        public void Initialize(AudioEventData eventData) {
            EventDataInternal = eventData;
            if ((EventDataInternal.Attributes & EAudioEventDataAttributes.StopOnSceneChange) == EAudioEventDataAttributes.StopOnSceneChange) {
                EventAttributes |= EAudioEventAttributes.StopOnSceneChange;
            }
            VolumeInitial = EventDataInternal.Volume.Value;
            PitchInitial = EventDataInternal.Pitch.Value;
            CopyAudioSourceSettings(SourceMain, EventDataInternal.SourceData);
            CopyAudioSourceSettings(SourceAux, EventDataInternal.SourceData);
            FadeMain.Initialize(EventDataInternal.FadeInOut);
            CrossFadeMain.Initialize(EventDataInternal.CrossFade);
            CrossFadeAux.Initialize(EventDataInternal.CrossFade);
            ResetPlaybackMode();
#if UNITY_EDITOR
            gameObject.name = eventData.name;
#endif
        }

        private void ResetPlaybackMode() {
            PlayModeIndex = 0;
            PlayModeIndexPool = null;
            CustomPlayModeIndex = 0;
            CustomPlayModeValue = 0;
        }

        private void CopyAudioSourceSettings(AudioSource dst, AudioSourceData src) {
            dst.dopplerLevel = src.dopplerLevel;
            dst.maxDistance = src.maxDistance;
            dst.minDistance = src.minDistance;
            dst.panStereo = src.panStereo;
            dst.spatialBlend = src.spatialBlend;
            dst.spread = src.spread;
            dst.bypassEffects = src.bypassEffects;
            dst.bypassListenerEffects = src.bypassListenerEffects;
            dst.bypassReverbZones = src.bypassReverbZones;
            //
            dst.rolloffMode = src.rolloffMode;
#if UNITY_5_2
            if (dst.rolloffMode == AudioRolloffMode.Custom) {
                if (src.RolloffCurve.keys.Length > 0) {
                    dst.SetCustomCurve(AudioSourceCurveType.CustomRolloff, src.RolloffCurve);
                }
                if (src.ReverbZoneMixCurve.keys.Length > 0) {
                    dst.SetCustomCurve(AudioSourceCurveType.ReverbZoneMix, src.ReverbZoneMixCurve);
                }
                if (src.SpatialBlendCurve.keys.Length > 0) {
                    dst.SetCustomCurve(AudioSourceCurveType.SpatialBlend, src.SpatialBlendCurve);
                }
                if (src.SpreadCurve.keys.Length > 0) {
                    dst.SetCustomCurve(AudioSourceCurveType.Spread, src.SpreadCurve);
                }
            }
#endif

            dst.outputAudioMixerGroup = src.MixerGroup;
        }

        private void SourcePlay(bool withfade = true) {
#if VERBOSE
            Util.O7Log.WarnT("AudioEvent", "{0} - SourcePlay: {1}", gameObject.name, withfade);
#endif
            if (SourceMain.clip != null) {
                SourceMain.Play();
                SourceMain.time = StartPoint;
            }
            // fade
            if (withfade) {
                FadeMain.StartFadeIn();
            } else {
                FadeMain.StartFadeIn(true);
            }
        }

        private void SourcePlayDelayed(double dspDelay, bool withfade = true) {
#if VERBOSE
            Util.O7Log.WarnT("AudioEvent", "{0} - SourcePlayDelayed: {1} {2}", gameObject.name, dspDelay, withfade);
#endif
            EventStateInternal |= EAudioEventState.DelayedPlay;
            //
            if (SourceMain.clip != null) {
                StartDelayDspTime = dspDelay;
                SourceMain.PlayScheduled(StartDelayDspTime);
                SourceMain.SetScheduledStartTime(StartDelayDspTime);
                SourceMain.time = StartPoint;
                if (withfade) {
                    EventStateInternal |= EAudioEventState.DelayedFade;
                    FadeMain.Reset();
                } else {
                    FadeMain.StartFadeIn(true);
                }
            }
        }

        private void SetClipData(AudioSource targetSource, ref AudioEventDataClip targetDataClip, AudioEventDataClip dataClip) {
            targetDataClip = EventDataInternal.AudioDefinitions[PlayModeIndex];
            targetSource.clip = dataClip.Clip;
            StartPoint = targetDataClip.GetRandomStartPoint();
        }

        private void InitClipRandomNoRepeat(AudioSource targetSource, ref AudioEventDataClip targetClipData) {
            if (PlayModeIndexPool == null) {
                PlayModeIndexPool = new List<byte>(EventDataInternal.AudioDefinitions.Count);
            }
            // when pool gets to count 1 or empty, repopulate randomly
            if (PlayModeIndexPool.Count <= 1) {
                RandomizerIndexFill.Clear();
                for (byte a = 0; a < EventDataInternal.AudioDefinitions.Count; a++) {
                    RandomizerIndexFill.Add(a);
                }
                while (RandomizerIndexFill.Count > 0) {
                    byte rnd = (byte) (UnityEngine.Random.Range(0, RandomizerIndexFill.Count)); //exclusive int
                    byte idx = RandomizerIndexFill[rnd];
                    RandomizerIndexFill.RemoveAt(rnd);
                    if (!PlayModeIndexPool.Contains(idx)) {
                        PlayModeIndexPool.Add(idx);
                    }
                }
            }
            //
            PlayModeIndex = PlayModeIndexPool[0];
            PlayModeIndexPool.RemoveAt(0);
            SetClipData(targetSource, ref targetClipData, EventDataInternal.AudioDefinitions[PlayModeIndex]);
        }

        private void InitClipProbability(AudioSource targetSource, ref AudioEventDataClip targetClipData) {
            float totalchance = 0.0f;
            for (int a = 0; a < EventDataInternal.AudioDefinitions.Count; a++) {
                totalchance += EventDataInternal.AudioDefinitions[a].Probability;
            }

            totalchance *= UnityEngine.Random.Range(0.0f, 1.0f);
            for (int a = 0; a < EventDataInternal.AudioDefinitions.Count; a++) {
                totalchance -= EventDataInternal.AudioDefinitions[a].Probability;
                if (totalchance <= 0.0f) {
                    PlayModeIndex = a;
                    SetClipData(targetSource, ref targetClipData, EventDataInternal.AudioDefinitions[PlayModeIndex]);
                    break;
                }
            }
        }

        private void InitClipSequential(AudioSource targetSource, ref AudioEventDataClip targetClipData) {
            SetClipData(targetSource, ref targetClipData, EventDataInternal.AudioDefinitions[PlayModeIndex]);
            //
            PlayModeIndex++;
            if (PlayModeIndex >= EventDataInternal.AudioDefinitions.Count) {
                PlayModeIndex = 0;
            }
        }

        private void InitClipUserDefined(int playmodeindex, AudioSource targetSource, ref AudioEventDataClip targetClipData) {
            PlayModeIndex = Mathf.Clamp(playmodeindex, 0, EventDataInternal.AudioDefinitions.Count - 1);
            if (PlayModeIndex < EventDataInternal.AudioDefinitions.Count) {
                SetClipData(targetSource, ref targetClipData, EventDataInternal.AudioDefinitions[PlayModeIndex]);
            }
        }

        private void InitClipFromPlayMode(int playmodeindex, AudioSource targetSource, ref AudioEventDataClip targetClipData) {
            // play mode
            switch (EventDataInternal.PlayMode) {
                case EPlayMode.RandomNoRepeat:
                    InitClipRandomNoRepeat(targetSource, ref targetClipData);
                    break;
                case EPlayMode.Probability:
                    InitClipProbability(targetSource, ref targetClipData);
                    break;
                case EPlayMode.Sequential:
                    InitClipSequential(targetSource, ref targetClipData);
                    break;
                case EPlayMode.UserDefined:
                    InitClipUserDefined(playmodeindex, targetSource, ref targetClipData);
                    break;
                case EPlayMode.Custom: {
                        CustomPlayModeEntry cpme = EventDataInternal.CustomPlayModeList[CustomPlayModeIndex];
                        switch (cpme.PlayMode) {
                            case EPlayMode.Sequential:
                                InitClipSequential(targetSource, ref targetClipData);
                                break;
                            case EPlayMode.Probability:
                                InitClipProbability(targetSource, ref targetClipData);
                                break;
                            case EPlayMode.RandomNoRepeat:
                                InitClipRandomNoRepeat(targetSource, ref targetClipData);
                                break;
                            case EPlayMode.UserDefined:
                                InitClipUserDefined(cpme.Value, targetSource, ref targetClipData);
                                break;
                        }
                        CustomPlayModeValue++;
                        if (cpme.Value >= 0 && (CustomPlayModeValue >= cpme.Value || cpme.PlayMode == EPlayMode.UserDefined)) {
                            CustomPlayModeValue = 0;
                            CustomPlayModeIndex++;
                            if (CustomPlayModeIndex >= EventDataInternal.CustomPlayModeList.Count) {
                                CustomPlayModeIndex = 0;
                            }
                        }
                    }
                    break;
            }
        }

        public void PrepareMainClip(int playmodeindex = 0, bool resetplaymode = true) {
            if (!SourceMain.isPlaying) {
                // prepare clip
                if (resetplaymode) {
                    ResetPlaybackMode();
                }
                InitClipFromPlayMode(playmodeindex, SourceMain, ref ClipDataMain);
            }
        }

        public void Play() {
            PlayFullArg();
        }

        public void PlayFullArg(bool withfade = true, int playmodeindex = 0, AudioEventInfo outinfo = null, double dspDelay = 0.0, bool resetplaymode = true, bool clipalreadyset = false) {
            // reentrant play
            if (IsPlaying) {
                // delayed stop?
                if ((EventStateInternal & EAudioEventState.DelayedStop) == EAudioEventState.DelayedStop) {
                    // handle stiching, was anything queued?
                    if (SourceAux.clip != null) {
#if VERBOSE
                        Util.O7Log.WarnT("AudioEvent", "{0} - PlayFullArg: restore original stich", gameObject.name);
#endif
                        // yes, restore original stich
                        SourceMain.SetScheduledEndTime(StartDelayDspTime);
                        SourceAux.PlayScheduled(StartDelayDspTime);
                        SourceAux.SetScheduledStartTime(StartDelayDspTime);
                    } else {
#if VERBOSE
                        Util.O7Log.WarnT("AudioEvent", "{0} - PlayFullArg: no, just clear the time and restore play mode", gameObject.name);
#endif
                        // no, just clear the time and restore play mode
                        if (EventDataInternal.LoopMode == ELoopMode.LoopSingle) {
                            SourceMain.SetScheduledEndTime(DspTime + (30 * 24 * 3600)); // offical Unity response, schedule end one month ahead... wtf
                        } else {
                            SourceMain.SetScheduledEndTime(DspTime + (SourceMain.clip.length - SourceMain.time));
                        }
                    }
                    EventStateInternal &= ~EAudioEventState.DelayedStop;
                    EventStateInternal &= ~EAudioEventState.DelayedFade;
                }
                // fade-ing out?
                FadeMain.StartFadeIn();
                EventStateInternal &= ~EAudioEventState.Stopping;
#if VERBOSE
                Util.O7Log.WarnT("AudioEvent", "{0} - PlayFullArg: fade-ing out?", gameObject.name);
#endif
            }

            if (!SourceMain.isPlaying) {

                if (!clipalreadyset) {
                    PrepareMainClip(playmodeindex, resetplaymode);
                }

                // initial values
                VolumeInitial = EventDataInternal.Volume.Value;
                PitchInitial = EventDataInternal.Pitch.Value;

                CrossFadeMain.StartFadeIn(true);

                // source
                SourceMain.time = 0.0f;
                SourceMain.loop = EventDataInternal.LoopMode == ELoopMode.LoopSingle;

                // set playing state
                EventStateInternal = EAudioEventState.Playing;

                // delayed?
                if (dspDelay <= DspTime) {
                    SourcePlay(withfade);
                } else {
                    SourcePlayDelayed(dspDelay, withfade);
                }

                // register for update
                AudioEventManager.Instance.RegisterActiveAudioEvent(this);

                // force update (apply volumes, etc...)
                OnUpdate(0.0f, false);
            }

            OutInfo = outinfo;
        }

        public void Stop(bool immediate = false) {
#if VERBOSE
            Util.O7Log.WarnT("AudioEvent", "{0} - Stop: immediate {1}", gameObject.name, immediate);
#endif
            if (EventDataInternal == null) {
                return;
            }
            //
            bool dostop = false;
            // immediate is hard, else check if needs fade out
            if (immediate) {
                dostop = true;
            } else {
                // only start fade out if event is actually playing
                if ((EventStateInternal & EAudioEventState.Playing) == EAudioEventState.Playing) {
                    FadeMain.StartFadeOut();
                }
                // if fade is active, mark stopping mode else hard stop it
                if (FadeMain.IsActive) {
                    EventStateInternal |= EAudioEventState.Stopping;
                } else {
                    dostop = true;
                }
            }
            // hard stop
            if (dostop) {
                // clear state
                EventStateInternal = EAudioEventState.Stopped;
                // fade
                FadeMain.Reset();
                //
                SourceMain.Stop();
                SourceMain.clip = null;
                SourceAux.Stop();
                SourceAux.clip = null;
                //
                AudioEventManager.Instance.UnregisterActiveAudioEvent(this);
            }
        }

        public void StopDelayed(double dspDelay, bool immediate) {
            StopDelayDspTime = dspDelay;
            SourceMain.SetScheduledEndTime(StopDelayDspTime);

            if (SourceAux.clip != null) {
                //SourceAux.Stop();
                SourceAux.SetScheduledEndTime(StopDelayDspTime);
            }

#if VERBOSE
            Util.O7Log.WarnT("AudioEvent", "{0} - StopDelayed: immediate: {1}; DelayDspTime: {2}, DspTimer: {3}", gameObject.name, immediate, StopDelayDspTime, DspTime);
#endif

            // handle state
            EventStateInternal &= ~EAudioEventState.DelayedPlay;
            EventStateInternal |= EAudioEventState.DelayedStop;
            if (immediate) {
                EventStateInternal &= ~EAudioEventState.DelayedFade;
            } else {
                EventStateInternal |= EAudioEventState.DelayedFade;
            }
        }

        public void Pause(bool pause) {
            if (pause) {
                EventStateInternal |= EAudioEventState.Paused;
                SourceMain.Pause();
                SourceAux.Pause();
            } else {
                EventStateInternal &= ~EAudioEventState.Paused;
                SourceMain.UnPause();
                SourceAux.UnPause();
            }
        }

        public void OnUpdate(float deltaTime, bool updateLoop = true) {

#if VERBOSE
            CheckIfValid();
#endif

            if (FollowTransfrom != null) {
                transform.position = FollowTransfrom.position;
                transform.rotation = FollowTransfrom.rotation;
            }

            double currentDspTime = DspTime;

            // provide out info if needed
            if (OutInfo != null) {
                OutInfo.EventState = EventStateInternal;
                OutInfo.CurrentClip = SourceMain.clip;
            }
            // is delayed?
            if ((EventStateInternal & EAudioEventState.DelayedPlay) == EAudioEventState.DelayedPlay) {
                double delayDiff = StartDelayDspTime - currentDspTime;
                if (delayDiff <= 0.0) {
                    EventStateInternal &= ~EAudioEventState.DelayedPlay;
                    // start fader?
                    if ((EventStateInternal & EAudioEventState.DelayedFade) == EAudioEventState.DelayedFade) {
                        FadeMain.StartFadeIn();
                        EventStateInternal &= ~EAudioEventState.DelayedFade;
                    }
                } else {
                    return;
                }
            }

            // update fade
            if (FadeMain.IsActive) {
                FadeMain.OnUpdate(deltaTime);
            }
            if (CrossFadeMain.IsActive) {
                CrossFadeMain.OnUpdate(deltaTime);
            }
            if (CrossFadeAux.IsActive) {
                CrossFadeAux.OnUpdate(deltaTime);
            }
            // has fade out finished?
            if (((EventStateInternal & EAudioEventState.Stopping) == EAudioEventState.Stopping) && !FadeMain.IsActive) {
                Stop(true);
            }

            // delayed stop
            if ((EventStateInternal & EAudioEventState.DelayedStop) == EAudioEventState.DelayedStop) {
                // fade out?
                if ((EventStateInternal & EAudioEventState.DelayedFade) == EAudioEventState.DelayedFade) {
                    float fot = EventDataInternal.FadeInOut.FadeOutTime.Value;
                    if (fot > 0.0f) {
                        double fadeOutStartDspTime = StopDelayDspTime - ((double) fot + AudioEventConstants.FadeCompensation);
#if VERBOSE
                        Util.O7Log.WarnT("AudioEvent", "{0} - OnUpdate: Delayed FadeOut Check: {1}, currentDspTime: {2}", gameObject.name, fadeOutStartDspTime, currentDspTime);
#endif
                        if (currentDspTime >= fadeOutStartDspTime) {
#if VERBOSE
                            Util.O7Log.WarnT("AudioEvent", "{0} - OnUpdate: FadeOutStarted", gameObject.name);
#endif
                            FadeMain.StartFadeOut(false, fot);
                            EventStateInternal &= ~EAudioEventState.DelayedFade;
                        }
                    } else {
                        EventStateInternal &= ~EAudioEventState.DelayedFade;
                    }
                }

                // force stop after expected time has passed
                if (currentDspTime >= StopDelayDspTime) {
#if VERBOSE
                    Util.O7Log.WarnT("AudioEvent", "{0} - OnUpdate: Force Stop: currentDspTime: {1}; StopDelayDspTime: {2}", gameObject.name, currentDspTime, StopDelayDspTime);
#endif
                    Stop(true);
                }
            }

            // not playing, skip update
            if ((EventStateInternal & EAudioEventState.Playing) != EAudioEventState.Playing) {
                return;
            }

            // special case
            if (EventDataInternal.LoopMode == ELoopMode.LoopCycle) {
                // enqueue next playback
                double leftover = SourceMain.clip.length - SourceMain.time;
                if (SourceAux.clip == null && leftover < Mathf.Min(2.0f, SourceMain.clip.length * 0.5f)) {
                    InitClipFromPlayMode(PlayModeIndex, SourceAux, ref ClipDataAux);
                    SourceAux.loop = false;
                    double t = DspTime + leftover;
                    SourceMain.SetScheduledEndTime(t);
                    double leads = ClipDataMain.LeadOutLength + ClipDataAux.LeadInLength;
                    t -= leads;
                    if (leads <= 0.0) {
                        t -= AudioEventConstants.StichingOverlappingOffset;
                    }
                    SourceAux.PlayScheduled(t);
                    SourceAux.SetScheduledStartTime(t);
#if VERBOSE
                    Util.O7Log.WarnT("AudioEvent", "{0} - OnUpdate: enqueue next playback", gameObject.name);
#endif
                }

                // cross fade
                if ((EventStateInternal & EAudioEventState.CrossFade) != EAudioEventState.CrossFade && ClipDataAux != null) {
                    double leads = ClipDataMain.LeadOutLength + ClipDataAux.LeadInLength;
                    double trigger = SourceMain.clip.length - leads;
                    if (SourceMain.time >= trigger && leads > 0.0) {
                        EventStateInternal |= EAudioEventState.CrossFade;
                        CrossFadeMain.StartFadeOut(false, ClipDataMain.LeadOutLength);
                        CrossFadeAux.StartFadeIn(false, ClipDataAux.LeadInLength);
                    }
                }
            }

#if VERBOSE
            CheckIfValid();
#endif

            // update loop?
            if (!SourceMain.isPlaying && updateLoop) {
                switch (EventDataInternal.LoopMode) {
                    case ELoopMode.OneShot: {
                            Stop(true);
                        }
                        break;
                    case ELoopMode.LoopSingle: {
                            // should be already playing
                        }
                        break;
                    case ELoopMode.LoopCycle: {
                            // swap
                            if (!SourceMain.isPlaying && SourceAux.isPlaying) {
                                AudioSource temp = SourceMain;
                                SourceMain = SourceAux;
                                SourceAux = temp;
                                SourceAux.clip = null;
                                EventStateInternal &= ~EAudioEventState.CrossFade;
                                CrossFadeMain.StartFadeIn(true);
                                CrossFadeAux.StartFadeOut(true);
#if VERBOSE
                                Util.O7Log.WarnT("AudioEvent", "{0} - OnUpdate: swap", gameObject.name);
#endif
                            }
                        }
                        break;
                }
            }

            // 2D camera panning
            float panVolume = 1.0f;
            float panStereo = 0.0f;
            if (EventDataInternal.Use2DCameraPanning) {
                Assert.NotNull(AudioEventManager.Instance.Panning2DCamera, "AudioEvent", "AudioEventManager.Instance.Panning2DCamera not set");
                Assert.NotNull(AudioEventManager.Instance.Panning2DStereoCurve, "AudioEvent", "AudioEventManager.Instance.Panning2DStereoCurve not set");
                Assert.NotNull(AudioEventManager.Instance.Panning2DVolumeCurve, "AudioEvent", "AudioEventManager.Instance.Panning2DVolumeCurve not set");

                Vector3 vp = AudioEventManager.Instance.Panning2DCamera.WorldToViewportPoint(transform.position);
                panStereo = AudioEventManager.Instance.Panning2DStereoCurve.Evaluate(vp.x);
                panVolume = AudioEventManager.Instance.Panning2DVolumeCurve.Evaluate(vp.x);
            }

            // apply volume, pitch, pan
            float finalVolume = VolumeInitial * VolumeInternal * ClipDataMain.Volume * FadeMain.FadeVolume * panVolume;

            SourceMain.volume = finalVolume * CrossFadeMain.FadeVolume;
            SourceAux.volume = finalVolume * CrossFadeAux.FadeVolume;

            SourceMain.pitch = SourceAux.pitch = PitchInitial * PitchInternal * ClipDataMain.Pitch;

            SourceMain.panStereo = SourceAux.panStereo = Mathf.Clamp((PanStereoInternal + panStereo), -1.0f, 1.0f);

            // provide out info if needed
            if (OutInfo != null) {
                OutInfo.EventState = EventStateInternal;
                OutInfo.CurrentClip = SourceMain.clip;
            }

#if UNITY_EDITOR
            if (EditorApplication.isPlayingOrWillChangePlaymode) {
                EditorUtility.SetDirty(this);
            }
#endif
#if VERBOSE
            CheckIfValid();
#endif
        }

        public AudioEventDataClip GetActiveAudioEventDataClip() {
            return EventDataInternal.AudioDefinitions[PlayModeIndex];
        }

        public float GetSourceTime() {
            if (SourceMain != null) {
                return SourceMain.time - ClipDataMain.LeadInLength;
            }
            return 0.0f;
        }

        public float GetSourceLength() {
            if (SourceMain != null && SourceMain.clip != null) {
                return SourceMain.clip.length;
            }
            return 0.0f;
        }

        public override string ToString() {
            return string.Format("[AudioEvent: {0}\nVolume={1:F2}, Pitch={2:F2}, IsPlaying={3}\nAttr={4}\nState={5}\n{6}\nTime={7:F2} / {8:F2}]",
                EventDataInternal == null ? "none" : EventDataInternal.name,
                Volume, Pitch, IsPlaying,
                EventAttributes.ToString(),
                EventStateInternal.ToString(),
                FadeMain.ToString(),
                SourceMain.time, GetSourceLength());
        }

#if UNITY_EDITOR
        public void EditorGUIRender() {
            if (EventDataInternal != null) {
                EditorGUILayout.LabelField(string.Format("EventData: {0}", EventDataInternal.name));
            }
            EditorGUILayout.LabelField(string.Format("Status: {0}", EventStateInternal.ToString()));
            EditorGUILayout.LabelField(string.Format("Attr: {0}", EventAttributes.ToString()));
            EditorGUILayout.LabelField("FadeMain", FadeMain.ToString());
            EditorGUILayout.LabelField("CrossFadeMain", CrossFadeMain.ToString());
            EditorGUILayout.LabelField("CrossFadeAux", CrossFadeAux.ToString());
            EditorGUILayout.LabelField(string.Format("Volume: Main:{0:F2} Clip:{1:F2} Init:{2:F2} FadeMain:{3:F2}", VolumeInternal, ClipDataMain == null ? 0.0f : ClipDataMain.Volume, VolumeInitial, FadeMain.FadeVolume));
            EditorGUILayout.LabelField(string.Format("CrossFadeMain: {0:F2} CrossFadeAux: {1:F2}", CrossFadeMain.FadeVolume, CrossFadeAux.FadeVolume));
            EditorGUILayout.LabelField(string.Format("Pitch: {0:F2}", Pitch));
            EditorGUILayout.LabelField(string.Format("TimeMain: {0:F2} / {1:F2}", SourceMain.time, SourceMain.clip == null ? 0.0f : SourceMain.clip.length));
            EditorGUILayout.LabelField(string.Format("TimeAux: {0:F2} / {1:F2}", SourceAux.time, SourceAux.clip == null ? 0.0f : SourceAux.clip.length));
            EditorGUILayout.LabelField(string.Format("DSPTime: {0:F2}; StartDelay {1:F2}; StopDelay {2:F2}", AudioSettings.dspTime, StartDelayDspTime, StopDelayDspTime));
            EditorGUILayout.LabelField(string.Format("SourceMain: playing: {0} ", SourceMain.isPlaying));
            EditorGUILayout.LabelField(string.Format("SourceAux:  playing: {0} ", SourceAux.isPlaying));
        }
#endif

        public void RenderDebugGui(bool full = false) {
            if (full) {
                GUILayout.Box(ToString());
            } else {
                GUILayout.Box(string.Format("[AudioEvent: {0}]", EventDataInternal.name));
            }
        }
    }
}