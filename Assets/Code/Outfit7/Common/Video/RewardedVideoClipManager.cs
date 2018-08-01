//
//   Copyright (c) 2015 Outfit7. All rights reserved.
//

using System;
using Outfit7.Analytics.Tracking;
using Outfit7.Event;
using Outfit7.Grid;
using Outfit7.Util;
using SimpleJSON;

namespace Outfit7.Video {

    /// <summary>
    /// Timed video clip manager.
    /// </summary>
    public class RewardedVideoClipManager : VideoClipManager {

        public TrackingManager TrackingManager { get; set; }

        private const string LastWatchTimePref = "RewardedVideoClipManager.LastVideoWatchTime";
        private const string LastWatchOfferCountPref = "RewardedVideoClipManager.LastVideoOfferCount";
        private readonly TimeSpan VideoRewardCompletionCallbackWaitTime = TimeSpan.FromSeconds(2);
        private TimeSpan SessionTime = BuildConfig.IsDevel ? TimeSpan.FromMinutes(1) : TimeSpan.FromHours(8);
        private DateTime LastWatchTime;
        private int VideoOfferCount;
        private int OffersPerSession = 4;
        private DateTime VideoRewardCompletionWaitTime;
        private bool VideoRewardCompletionWaitCallback;
        private bool VideoRewardWithoutCappingInProgress;
        private string VideoClipOppurtunityMissReason;

        public bool IsRewardDialogPending {
            get {
                if (!VideoRewardCompletionWaitCallback) {
                    return false;
                }

                if (VideoRewardCompletionWaitTime > DateTime.UtcNow) {
                    return true;
                }

                VideoRewardCompletionWaitCallback = false;
                return false;
            }
        }

        public GridManager GridManager { get; set; }

        public override void Init() {
            base.Init();

            LastWatchTime = UserPrefs.GetDateTime(LastWatchTimePref, DateTime.MinValue);
            VideoOfferCount = UserPrefs.GetInt(LastWatchOfferCountPref, VideoOfferCount);
            O7Log.DebugT(Tag, "Inited, LastWatchTime={0}, VideoOfferCount={1}", LastWatchTime, VideoOfferCount);

            UpdateTimeout();
        }

        public override void OnAppStartOrResume() {
            base.OnAppStartOrResume();

            UpdateVideoRewardCompletionWaitTime();
        }

        private void UpdateVideoRewardCompletionWaitTime() {
            VideoRewardCompletionWaitTime = DateTime.UtcNow.Add(VideoRewardCompletionCallbackWaitTime);
        }

        protected override void OnGridChange(object eventData) {
            base.OnGridChange(eventData);

            UpdateTimeout();
        }

        private void UpdateTimeout() {
            if (GridManager.Ready) {
                JSONNode gridJ = GridManager.JsonData;
                JSONNode rewardedVideoJ = gridJ["ad"]["rV"];
                if (rewardedVideoJ != null) {
                    int sessionTime = rewardedVideoJ["sT"].AsInt;
                    int maxOffersPerSession = rewardedVideoJ["mOPS"].AsInt;

                    if (sessionTime > 0) {
                        SessionTime = TimeSpan.FromSeconds(sessionTime);
                        O7Log.DebugT(Tag, "Grid update SessionTime {0}", SessionTime);
                    }

                    if (maxOffersPerSession > 0) {
                        OffersPerSession = maxOffersPerSession;
                        O7Log.DebugT(Tag, "Grid update OffersPerSession {0}", OffersPerSession);
                    }
                }
            }
        }

        private bool IsNewVideoSession {
            get {
                return DateTime.UtcNow >= LastWatchTime + SessionTime;
            }
        }

        public bool CheckAndReportVideoAvailability(string source) {

            if (!Common.AppPlugin.IsNetworkAvailable) {
                ReportVideoClipOpportunityMissReason("network not available", source);
                return false;
            }

            if (IsNewVideoSession) {
                ResetOfferCount();
            }

            if (VideoOfferCount >= OffersPerSession) {
                ReportVideoClipOpportunityMissReason("max offers per session reached", source);
                return false;
            }

            if (videoClipData == null) {
                ReportVideoClipOpportunityMissReason("no clip data", source);
                return false;
            }

            return true;
        }

        protected void ReportVideoClipOpportunityMissReason(string reason, string source) {
            O7Log.DebugT(Tag, "ReportVideoClipOpportunityMissReason(reason={0}, source={1})", reason, source);
            Assert.HasText(reason, "reason");
            Assert.HasText(source, "source");
            if (reason == VideoClipOppurtunityMissReason) return;

            VideoClipOppurtunityMissReason = reason;

            TrackingManager.AddEvent(CommonTrackingEventParams.GroupId.Ads,
                CommonTrackingEventParams.EventId.VideoShowOpportunityFail, source, reason, null, null, null, null);
        }

        protected void ResetVideoClipOpportunity() {
            VideoClipOppurtunityMissReason = null;
        }

        public override void ShowVideoClip() {
            VideoRewardWithoutCappingInProgress = false;
            VideoRewardCompletionWaitCallback = true;
            UpdateVideoRewardCompletionWaitTime();
            base.ShowVideoClip();
            ResetVideoClipOpportunity();
        }

        public void ShowVideoClipWithoutCapping() {
            VideoRewardWithoutCappingInProgress = true;
            base.ShowVideoClip();
        }

        internal override void OnVideoClipCompletion(string id, int amount) {

            VideoRewardCompletionWaitCallback = false;

            // Check if availability is changed when last-watch-time is set (new video-clip can become available before completion)
            VideoClipData oldData = VideoClipData;

            SaveSessionState();

            base.OnVideoClipCompletion(id, amount);

            if (VideoClipData != oldData) {
                EventBus.FireEvent(CommonEvents.VIDEO_CLIP_AVAILABILITY_CHANGE, VideoClipData);
            }
        }

        private void ResetOfferCount() {

            if (VideoOfferCount == 0) {
                return;
            }

            VideoOfferCount = 0;
            UserPrefs.SetInt(LastWatchOfferCountPref, VideoOfferCount);
            UserPrefs.SaveDelayed();

            O7Log.DebugT(Tag, "ResetWatchCount, LastWatchTime={0}, VideoOfferCount={1}", LastWatchTime, VideoOfferCount);
        }

        private void SaveSessionState() {

            if (IsNewVideoSession) {
                LastWatchTime = DateTime.UtcNow;
                UserPrefs.SetDateTime(LastWatchTimePref, LastWatchTime);
                UserPrefs.SaveDelayed();
            }

            if (VideoRewardWithoutCappingInProgress) return;

            VideoOfferCount++;
            UserPrefs.SetInt(LastWatchOfferCountPref, VideoOfferCount);
            UserPrefs.SaveDelayed();

            O7Log.DebugT(Tag, "SaveWatchState, LastWatchTime={0}, VideoOfferCount={1}", LastWatchTime, VideoOfferCount);
        }
    }
}
