//
//   Copyright (c) 2015 Outfit7. All rights reserved.
//

using System;
using Outfit7.Event;
using Outfit7.Util;
using Outfit7.Grid;
using SimpleJSON;

namespace Outfit7.Video {

    /// <summary>
    /// Timed video clip manager.
    /// </summary>
    public class TimedVideoClipManager : VideoClipManager {

        private const string JsonExt = "ext";
        private const string JsonOrder = "gamesOrder";
        private const string LastWatchTimePref = "TimedVideoClipManager.LastVideoWatchTime";
        private TimeSpan MinDurationBetweenVideos = BuildConfig.IsDevel ? TimeSpan.FromMinutes(1) : TimeSpan.FromMinutes(30);
        private DateTime LastWatchTime;

        public GridManager GridManager { get; set; }

        public override void Init() {
            base.Init();

            LastWatchTime = UserPrefs.GetDateTime(LastWatchTimePref, DateTime.MinValue);
            O7Log.DebugT(Tag, "Inited, LastWatchTime={0}", LastWatchTime);
            UpdateTimeout();
        }

        protected override void OnGridChange(object eventData) {
            base.OnGridChange(eventData);

            UpdateTimeout();
        }

        private void UpdateTimeout() {
            if (GridManager.Ready) {
                JSONNode gridJ = GridManager.JsonData;
                int interval = gridJ["ad"]["rewardedVideoTimeout"].AsInt;
                if (interval > 0) {
                    MinDurationBetweenVideos = TimeSpan.FromSeconds(interval);
                    O7Log.DebugT(Tag, "UpdateTimeout, timespan {0}", MinDurationBetweenVideos);
                }
            }
        }

        public override VideoClipData VideoClipData {
            get {
                if (!IsTime)
                    return null;
                return base.VideoClipData;
            }
            internal set {
                if (this.videoClipData == value)
                    return;
                this.videoClipData = value;

                // Fire event only if video clip is available
                if (value == null || IsTime) {
                    EventBus.FireEvent(CommonEvents.VIDEO_CLIP_AVAILABILITY_CHANGE, value);
                }
            }
        }

        private bool IsTime {
            get {
                return (DateTime.UtcNow >= LastWatchTime + MinDurationBetweenVideos);
            }
        }

        internal override void OnVideoClipCompletion(string id, int amount) {
            // Check if availability is changed when last-watch-time is set (new video-clip can become available before completion)
            VideoClipData oldData = VideoClipData;

            SaveLastWatchTime();
            base.OnVideoClipCompletion(id, amount);

            if (VideoClipData != oldData) {
                EventBus.FireEvent(CommonEvents.VIDEO_CLIP_AVAILABILITY_CHANGE, VideoClipData);
            }
        }

        private void SaveLastWatchTime() {
            LastWatchTime = DateTime.UtcNow;
            UserPrefs.SetDateTime(LastWatchTimePref, LastWatchTime);
            UserPrefs.SaveDelayed();
        }
    }
}
