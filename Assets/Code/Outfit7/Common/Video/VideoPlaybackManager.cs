using Outfit7.Util;
using SimpleJSON;
using System;

namespace Outfit7.Video {

    public partial class VideoPlaybackManager {

        protected const string UrlParam = "url";
        protected const string ResumeParam = "resume";

        protected const string Tag = "VideoPlaybackManager";

        public enum PlayBackStopReason {
            None,
            FinishedPlaying,
            Paused,
            Skipped,
            Error
        }

        public virtual VideoPlaybackPlugin VideoPlaybackPlugin { get; set; }

        public virtual bool IsPlaybackSupported { get; protected set; }

        public virtual bool IsPlaybackInProgress { get; protected set; }

        public Action PlaybackStartedCallback;
        public Action<PlayBackStopReason,string> PlaybackStoppedCallback;


        public virtual void Init() {
            IsPlaybackSupported = VideoPlaybackPlugin.IsPlaybackSupported;
            IsPlaybackInProgress = false;
        }

        public virtual bool CanPlay() {
            if (!IsPlaybackSupported) return false;
            if (IsPlaybackInProgress) return false;

            return true;
        }

        public virtual void Play(string url) {
            Play(url, false);
        }

        public virtual void Play(string url, bool resume) {
            Assert.HasText(url, "url");
            Assert.State(IsPlaybackSupported, "Playback not supported");
            Assert.State(!IsPlaybackInProgress, "Playback already in progress");

            JSONClass json = new JSONClass();
            json[UrlParam] = url;
            json[ResumeParam].AsBool = resume;

            IsPlaybackInProgress = true;
            VideoPlaybackPlugin.Play(json.ToString());
        }

        public virtual void Hide() {
            VideoPlaybackPlugin.Hide();
        }

        public virtual void PlaybackStarted(string jsonString) {
            Assert.State(IsPlaybackInProgress, "Playback in progress");
            O7Log.DebugT(Tag, "PlaybackStarted {0}", jsonString);

            if (PlaybackStartedCallback != null) {
                PlaybackStartedCallback();
            }
        }

        public virtual void PlaybackStopped(string jsonString) {
            Assert.State(IsPlaybackInProgress, "Playback in progress");
            O7Log.DebugT(Tag, "PlaybackStopped {0}", jsonString);
            IsPlaybackInProgress = false;

            JSONNode json = JSONNode.Parse(jsonString);
            string stopReasonString = json["reason"].Value;

            PlayBackStopReason stopReason;
            switch (stopReasonString) {
                case "finished":
                    stopReason = PlayBackStopReason.FinishedPlaying;
                    break;
                case "skipped":
                    stopReason = PlayBackStopReason.Skipped;
                    break;
                case "error":
                    stopReason = PlayBackStopReason.Error;
                    break;
                case "pause":
                    stopReason = PlayBackStopReason.Paused;
                    break;
                default:
                    throw new ArgumentException("Unrecognised stop reason: " + stopReasonString);
            }

            if (PlaybackStoppedCallback != null) {
                string errorDescription = json["error"].Value;
                PlaybackStoppedCallback(stopReason, errorDescription);
            }
        }

    }
}
