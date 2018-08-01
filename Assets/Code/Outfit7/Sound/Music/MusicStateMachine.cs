#define VERBOSE

using Outfit7.Audio.MusicInternal;
using Outfit7.Logic;
using Outfit7.Logic.StateMachineInternal;
using Outfit7.Util;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

// Disclaimer:
// AudioEventMusicController and MusicStateMachine have some duplicated code. I did it intentionally, because there is no simple solution to problem,
// if we don't want to break exisitng stuff. Public functions from this class are already used (linked!), so moving them would break those links.
// If you are starting a new project, use MusicStateMachine, while existing projects can still stay on master branch.

namespace Outfit7.Audio {

    public class MusicStateMachine : StateMachine {

        public override string Tag {
            get {
                return "MusicStateMachine";
            }
        }

        public int MaxTracks = 4;
        public AudioEvent[] Tracks;
        public AudioEventInfo[] TrackInfos;
        private List<AudioEvent> Stings;

        public MusicStateMachineLayer[] InternalLayers;
        public MusicStateMachineState[] InternalStates;

        public override Layer[] Layers {
            get {
                return InternalLayers;
            }
        }

        public override State[] States {
            get {
                return InternalStates;
            }
        }

        protected void OnEventDestroyed(AudioEvent ae) {
            Stings.Remove(ae);
            ae.OnEventDestroy -= OnEventDestroyed;
        }

        public void Init() {
            Tracks = new AudioEvent[MaxTracks];
            TrackInfos = new AudioEventInfo[MaxTracks];
            Stings = new List<AudioEvent>(MaxTracks);

            for (int a = 0; a < MaxTracks; a++) {
                Tracks[a] = AudioEventManager.Instance.CreateAudioEventInstance();
                TrackInfos[a] = new AudioEventInfo();
            }
        }

        AudioEvent PrepareForTrack(int track, float volume = 1.0f, float pitch = 1.0f) {
            if (track < 0 || track >= MaxTracks) {
#if VERBOSE
                O7Log.WarnT(Tag, "Music track {0} is wrong", track);
#endif
                return null;
            }

            AudioEvent ae = Tracks[track];
            if (ae != null) {
                if (volume > -1.0f) {
                    ae.Volume = volume;
                }
                if (pitch > -1.0f) {
                    ae.Pitch = pitch;
                }
            } else {
#if VERBOSE
                O7Log.WarnT(Tag, "AudioEvent instance is null");
#endif
            }
            return ae;
        }

        // Music
        public void MusicSetTrackData(int track, AbstractAudioEventData data) {
            if (track < 0 || track >= MaxTracks) {
#if VERBOSE
                O7Log.WarnT(Tag, "Music track {0} is wrong", track);
#endif
                return;
            }
            AudioEvent ae = Tracks[track];
            if (ae != null) {
                AudioEventData aed = data.GetAudioEventData();
                if (aed != ae.EventDataInternal) {
                    ae.Stop(true);
                    ae.Initialize(data.GetAudioEventData());
                }
#if UNITY_EDITOR
                ae.name = string.Format("Music Track {0}: {1}", track, data.name);
#endif
            } else {
#if VERBOSE
                O7Log.WarnT(Tag, "AudioEvent instance is null");
#endif
            }
        }

        public void MusicPlay(int track, float volume = 1.0f, float pitch = 1.0f) {
            AudioEvent ae = PrepareForTrack(track, volume, pitch);
            if (ae != null) {
                ae.Play();
            } else {
#if VERBOSE
                O7Log.WarnT(Tag, "AudioEvent instance is null");
#endif
            }
        }

        public void MusicPlayStitch(int track1, int track2, float volume = 1.0f, float pitch = 1.0f, float delay = 0.5f) {
            if (track1 == track2) {
#if VERBOSE
                O7Log.WarnT(Tag, "Cannot stich same tracks {0}", track1);
#endif
                return;
            }
            // TODO: handle reentrant?
            AudioEvent ae1 = PrepareForTrack(track1, volume, pitch);
            AudioEventInfo aei1 = TrackInfos[track1];
            AudioEvent ae2 = PrepareForTrack(track2, volume, pitch);
            if (ae1 != null && ae2 != null) {
                // to have proper stitch both have to be played through delay, so add delay
                ae1.PlayFullArg(true, 0, aei1, delay);
                ae2.PlayFullArg(false, 0, null, aei1.CurrentClip.length + delay);
            } else {
#if VERBOSE
                O7Log.WarnT(Tag, "AudioEvent instance is null: {0} {1}", ae1, ae2);
#endif
            }
        }

        public void MusicTweak(int track, float volume, float pitch) {
            if (track < 0 || track >= MaxTracks) {
#if VERBOSE
                O7Log.WarnT(Tag, "Music track {0} is wrong", track);
#endif
                return;
            }

            AudioEvent ae = Tracks[track];
            if (ae != null) {
                if (volume > -1.0f) {
                    ae.Volume = volume;
                }
                if (pitch > -1.0f) {
                    ae.Pitch = pitch;
                }
            } else {
#if VERBOSE
                O7Log.WarnT(Tag, "AudioEvent instance is null");
#endif
            }
        }

        public void MusicStop(int track, bool immediate) {
            if (track < 0 || track >= MaxTracks) {
#if VERBOSE
                O7Log.WarnT(Tag, "Music track {0} is wrong", track);
#endif
                return;
            }
            AudioEvent ae = Tracks[track];
            if (ae != null) {
                ae.Stop(immediate);
            } else {
#if VERBOSE
                O7Log.WarnT(Tag, "AudioEvent instance is null");
#endif
            }
        }

        public void MusicStopAll(bool immediate) {
            for (int a = 0; a < MaxTracks; a++) {
                MusicStop(a, immediate);
            }
        }

        public void MusicPlayOnMusicBeat(int trackToStop, bool stopMusic, bool stopMusicImmediate, int trackToPlay, float volume = 1.0f, float pitch = 1.0f, bool resetplaymode = true) {
            if (trackToStop < 0 || trackToStop >= MaxTracks || trackToPlay < 0 || trackToPlay >= MaxTracks) {
#if VERBOSE
                O7Log.WarnT(Tag, "Music track {0} {1} is wrong", trackToStop, trackToPlay);
#endif
                return;
            }

            AudioEvent musicToPlay = Tracks[trackToPlay];
            if (musicToPlay == null) {
#if VERBOSE
                O7Log.WarnT(Tag, "Music track {0} is null", trackToPlay);
#endif
                return;
            }

            double delay = 0.0f;
            double dspTime = AudioEvent.DspTime;

            AudioEvent musicToStop = Tracks[trackToStop];
            if (musicToStop != null && musicToStop.IsPlaying) {
                AudioEventDataClip dc = musicToStop.GetActiveAudioEventDataClip();
                float bpmTimer = dc.GetBeatsPerMinuteTimer();
                if (bpmTimer <= 0.0f) {
#if VERBOSE
                    O7Log.WarnT(Tag, "BeatsPerMinuteTimer is invalid: track: {0}", trackToStop);
#endif
                    return;
                }
                float curPos = musicToStop.GetSourceTime();
                int beats = UnityEngine.Mathf.FloorToInt(curPos / bpmTimer);
                beats++;
                float nextBeatPos = (float) beats * bpmTimer;

                delay = nextBeatPos - curPos;
                if (delay < 0.0 || delay > 100.0) {
#if VERBOSE
                    O7Log.WarnT(Tag, "Delay is wrong: track: {0}; delay {1}", trackToStop, delay);
#endif
                    return;
                }

                if (stopMusic) {
                    musicToStop.StopDelayed(delay + dspTime, stopMusicImmediate);
                }
            }

#if (UNITY_EDITOR || DEVEL_BUILD || PROD_BUILD) && VERBOSE
            O7Log.InfoT(Tag, "MusicPlayOnMusicBeat: delay set to {0} seconds; trackToStop: {1} trackToPlay: {2}", delay, trackToStop, trackToPlay);
#endif
            // first set clip
            musicToPlay.PrepareMainClip(0, resetplaymode);
            // pull lead in time, calculate delay
            double finaldelay = delay + dspTime - musicToPlay.ClipDataMain.LeadInLength;
            // enqueue play
            musicToPlay.PlayFullArg(!stopMusicImmediate, 0, null, finaldelay, resetplaymode, true);
        }

        // Stings
        public void StingPlay(AbstractAudioEventData data, float volume = 1.0f, float pitch = 1.0f) {
            AudioEvent ae = AudioEventManager.Instance.CreateAudioEvent(data.GetAudioEventData(), null, EAudioEventAttributes.AutoDelete);
            if (ae != null) {
                if (volume > -1.0f) {
                    ae.Volume = volume;
                }
                if (pitch > -1.0f) {
                    ae.Pitch = pitch;
                }
                ae.Play();
                ae.OnEventDestroy += OnEventDestroyed;
                Stings.Add(ae);
            } else {
#if VERBOSE
                O7Log.WarnT(Tag, "AudioEvent instance is null");
#endif
            }
        }

        public bool StingPlayOnNextMusicBeat(AbstractAudioEventData data, int track, int prevTracks, bool stopMusic, bool stopMusicImmediate, float volume = 1.0f, float pitch = 1.0f) {
            if (track < 0 || track >= MaxTracks || data == null) {
#if VERBOSE
                O7Log.WarnT(Tag, "Music track {0} is wrong", track);
#endif
                return false;
            }

            bool res = true;
            double delay = 0.0;
            double dspTime = AudioEvent.DspTime;
            AudioEvent musicToStop = Tracks[track];
            if (musicToStop != null && musicToStop.IsPlaying) {

                // if current track is queued, fall back to previous one
                if ((musicToStop.EventState & (EAudioEventState.DelayedFade | EAudioEventState.DelayedPlay)) > 0) {
                    AudioEvent prevMusicToStop = Tracks[prevTracks];
                    if (prevMusicToStop != null && prevMusicToStop.IsPlaying) {
                        musicToStop.Stop(true);
                        musicToStop = prevMusicToStop;
                        res = false;
                    }
                }

                AudioEventDataClip dc = musicToStop.GetActiveAudioEventDataClip();
                float bpmTimer = dc.GetBeatsPerMinuteTimer();
                if (bpmTimer <= 0.0f) {
#if VERBOSE
                    O7Log.WarnT(Tag, "BeatsPerMinuteTimer is invalid: track: {0}; sting {1}", track, data.name);
#endif
                    return false;
                }
                float curPos = musicToStop.GetSourceTime();
                int beats = Mathf.FloorToInt(curPos / bpmTimer);
                beats++;
                float nextBeatPos = (float) beats * bpmTimer;
                delay = nextBeatPos - curPos;

                if (delay < 0.0 || delay > 100.0) {
#if VERBOSE
                    O7Log.WarnT(Tag, "Delay is wrong: track: {0}; sting {1}; delay {2}", track, data.name, delay);
#endif
                    return false;
                }

#if (UNITY_EDITOR || DEVEL_BUILD || PROD_BUILD) && VERBOSE
                O7Log.InfoT(Tag, "StingPlayOnNextMusicBeat: delay set to {0} seconds; track: {1}", delay, track);
#endif
                if (stopMusic) {
                    musicToStop.StopDelayed(delay + dspTime, stopMusicImmediate);
                }
            }

            AudioEvent ae = AudioEventManager.Instance.CreateAudioEvent(data.GetAudioEventData(), null, EAudioEventAttributes.AutoDelete);
            if (ae != null) {
                if (volume > -1.0f) {
                    ae.Volume = volume;
                }
                if (pitch > -1.0f) {
                    ae.Pitch = pitch;
                }

                // first set clip
                ae.PrepareMainClip(0, true);
                // pull lead in time, calculate delay
                double stingdelay = delay + dspTime - ae.ClipDataMain.LeadInLength;
                // enqueue play
                ae.PlayFullArg(!stopMusicImmediate, 0, null, stingdelay, false, true);
                ae.OnEventDestroy += OnEventDestroyed;
                Stings.Add(ae);
            } else {
#if VERBOSE
                O7Log.WarnT(Tag, "AudioEvent instance is null");
#endif
            }
            return res;
        }

        public void StingStopAll() {
            for (int a = 0; a < Stings.Count; a++) {
                AudioEvent ae = Stings[a];
                ae.Stop(true);
#if VERBOSE
                O7Log.InfoT(Tag, "StingStopAll: {0}", ae.name);
#endif
            }
        }

        // Mixer
        public void MixerMute(AudioMixer mixer, string parameterName, bool mute) {
            if (mixer != null) {
                mixer.Mute(parameterName, mute);
            } else {
#if VERBOSE
                O7Log.WarnT(Tag, "mixer is null while setting {0} to {1}", parameterName, mute);
#endif
            }
        }

        public void MixerSetSnapshot(AudioMixerSnapshot snapshot, float blendTime) {
            snapshot.TransitionTo(blendTime);
        }
    }
}