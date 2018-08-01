//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using Outfit7.Event;
using Outfit7.Grid;
using Outfit7.Util;
using SimpleJSON;

namespace Outfit7.Video.Gallery {

    /// <summary>
    /// Video gallery manager.
    /// </summary>
    public class VideoGalleryManager {

        public const string O7InternalParam = "o7internal";

        private const string Tag = "VideoGalleryManager";
        private const string LastBadgeTimestampPref = "VideoGalleryManager.LastBadgeTimestamp";

        public static string ParseVideoUrl(string videoUrl) {
            if (videoUrl == null) return null;

            // Parse URL (something like o7talkingapp://video/Gltp0iSA-60?param=value)
            try {
                Uri uri = new Uri(videoUrl);
                if (uri.Host != "video") return null;

                string id = uri.PathAndQuery;
                id = id.Trim('/'); // Remove leading and/or trailing (if no query) slash
                return id;

            } catch (Exception e) {
                O7Log.WarnT(Tag, e, "Invalid video URL: {0}", videoUrl);
                return null;
            }
        }

        private bool ready;
        private DateTime BadgeTime;

        public bool Ready {
            get {
                return this.ready;
            }
            internal set {
                this.ready = value;
                if (value) {
                    ParseBadge();
                }
            }
        }

        public int Badge { get; private set; }

        public string PushVideoId { get; private set; }

        public EventBus EventBus { get; set; }

        public GridManager GridManager { get; set; }

        public VideoGalleryPlugin VideoGalleryPlugin { get; set; }

#if UNITY_WP8
        public string VideoGalleryRedirectUrl { get; set; }
#endif

        public void Init() {
            EventBus.AddListener(CommonEvents.PUSH_START, OnPush);
            EventBus.AddListener(CommonEvents.PUSH_RECEIVE, OnPush);

#if UNITY_WP8
            OnGridChange(null);
            EventBus.AddListener(CommonEvents.FRESH_GRID_DOWNLOAD, OnGridChange);
#endif

        }
#if UNITY_WP8
        private void OnGridChange(object data) {
            JSONNode gridJ = GridManager.JsonData;
            if (gridJ == null) return;

            string url = gridJ["videoGalleryRedirectUrl"];
            bool validUrl = !StringUtils.IsNullOrEmpty(url);
            if (validUrl) {
                VideoGalleryRedirectUrl = url;
            }

            Ready = validUrl;
        }
#endif

        private void ParseBadge() {
            JSONNode gridJ = GridManager.JsonData;
            if (gridJ == null) return;

            BadgeTime = UserPrefs.GetDateTime(LastBadgeTimestampPref, DateTime.MinValue);

            JSONNode badgeTsJ = gridJ["videoGalleryBadgeTimestamp"];
            DateTime badgeTime;
            try {
                badgeTime = TimeUtils.ToDateTime(badgeTsJ.AsLong);

            } catch (Exception e) {
                O7Log.WarnT(Tag, e, "Unparsable videoGalleryBadgeTimestamp: {0}", badgeTsJ);
                return;
            }
            if (badgeTime > DateTime.UtcNow) {
                O7Log.WarnT(Tag, "Badge timestamp is in future: {0}", badgeTime);

            } else if (badgeTime <= BadgeTime) {
                // Already seen
                O7Log.DebugT(Tag, "Badge with time={0} is already seen", badgeTime);
                return;
            }

            Badge = gridJ["videoGalleryBadge"].AsInt;
            BadgeTime = badgeTime;

            O7Log.VerboseT(Tag, "Got badge={0}, badgeTime={1}", Badge, BadgeTime);
        }

        private void OnPush(object eventData) {
            JSONNode payloadJ = (JSONNode) eventData;

            PushVideoId = ParseVideoUrl(payloadJ["action"]);

            if (PushVideoId != null) {
                O7Log.DebugT(Tag, "Got push video ID: {0}", PushVideoId);
            }
        }

        public void OpenVideoGalleryView(string videoId, string room) {
#if UNITY_WP8
            UnityEngine.Application.OpenURL(VideoGalleryRedirectUrl);

            // MTA-7298 Close dialog for blocking touches since we start browser in separate instance and unity app is never paused during this call.
            // This is temporary fix - update for this is followed up in MTA-7300
            EventBus.FireEvent(CommonEvents.NATIVE_DIALOG_CANCEL);
#else
            VideoGalleryPlugin.OpenVideoGalleryView(videoId, room);

            Badge = 0; // Hide badge on opening
            UserPrefs.SetDateTime(LastBadgeTimestampPref, BadgeTime);
            UserPrefs.SaveDelayed();

            PushVideoId = null;
#endif
        }

        public void OpenVideoGalleryViewWithUrl(string videoUrl) {
#if UNITY_WP8
            UnityEngine.Application.OpenURL(VideoGalleryRedirectUrl);

            // MTA-7298 Close dialog for blocking touches since we start browser in separate instance and unity app is never paused during this call.
            // This is temporary fix - update for this is followed up in MTA-7300
            EventBus.FireEvent(CommonEvents.NATIVE_DIALOG_CANCEL);
#else
            VideoGalleryPlugin.OpenVideoGalleryViewWithUrl(videoUrl);

            Badge = 0; // Hide badge on opening
            UserPrefs.SetDateTime(LastBadgeTimestampPref, BadgeTime);
            UserPrefs.SaveDelayed();

            PushVideoId = null;
#endif
        }

        public virtual bool IsInternalVideoGalleryUrl(string url) {
            return StringUtils.HasText(url) && url.Contains(O7InternalParam);
        }
    }
}
