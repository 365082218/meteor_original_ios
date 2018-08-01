using System;
using Outfit7.Util;
using Outfit7.Util.Io;

namespace Outfit7.Common.Promo {
    public abstract class PromoCampaign {

        public abstract string Tag { get; }

        public string Id { get; private set; }

        public string ActionUrl { get; private set; }

        public string ImpressionUrl { get; private set; }

        public string PreviewUrl { get; private set; }

        public string PreviewPath { get; private set; }

        public string AppId { get; private set; }

        public DateTime FirstPresentedToUserTime { get; private set; }

        public TimeSpan Validity { get; private set; }

        public int ImpressionLimit { get; private set; }

        public int Impressions { get; private set; }

        public bool WasPresentedToUser { get; private set; }

        public int SequenceTimeout { get; private set; }

        public bool AppInstalled { get; private set; }

        public int AutoHideTimeout { get; private set; }

        public bool IsPreviewImageCached { get; private set; }

        public DateTime LastPreviewImageDownloadTry { get; private set; }

        public virtual void Init(string id, string cachePath, string actionUrl, string clickUrl, string previewUrl, int impressionLimit, int impressions,
            DateTime firstPresentedToUserTime, TimeSpan validity, bool wasPresentedToUser, int sequenceTimeout, string appId, int autoHideTimeout) {
            Id = id;
            ActionUrl = actionUrl;
            ImpressionUrl = clickUrl;
            PreviewUrl = previewUrl;
            FirstPresentedToUserTime = firstPresentedToUserTime;
            WasPresentedToUser = wasPresentedToUser;
            Validity = validity;
            ImpressionLimit = impressionLimit;
            Impressions = impressions;
            SequenceTimeout = sequenceTimeout;
            AppId = appId;
            CheckIfAppInstalled();
            AutoHideTimeout = autoHideTimeout;
            PreviewPath = cachePath + CryptoUtils.Sha1(PreviewUrl);
            IsPreviewImageCached = O7File.Exists(PreviewPath);
            LastPreviewImageDownloadTry = DateTime.MinValue;
        }

        public virtual void OnAppResume() {
            CheckIfAppInstalled();
        }

        public void Update() {
            CheckIfAppInstalled();
        }

        private void CheckIfAppInstalled() {
            if (!StringUtils.IsNullOrEmpty(AppId)) {
                AppInstalled = AppPlugin.IsAppInstalled(AppId);
            }
        }

        public void WasPresented() {
            if (!WasPresentedToUser) {
                WasPresentedToUser = true;
                FirstPresentedToUserTime = DateTime.UtcNow;
            }
            Impressions++;
        }

        public virtual bool Valid {
            get {
                if (FirstPresentedToUserTime.Add(Validity) < DateTime.UtcNow) { // expired
                    return false;
                }

                if (Impressions >= ImpressionLimit) { // too many impressions
                    return false;
                }

                if (AppInstalled) {
                    return false;
                }

                return true;
            }
        }

        public void OnPreviewImageDownload(bool success) {
            O7Log.DebugT(Tag, "OnPreviewImageDownload {0}", success);
            LastPreviewImageDownloadTry = DateTime.UtcNow;
            IsPreviewImageCached = success;
        }

        public override string ToString() {
            return string.Format("[PromoCampaign: Tag={0}, Id={1}, ActionUrl={2}, ImpressionUrl={3}, PreviewUrl={4}, PreviewPath={5}, AppId={6}, FirstPresentedToUserTime={7}, Validity={8}, ImpressionLimit={9}, Impressions={10}, WasPresentedToUser={11}, SequenceTimeout={12}, AppInstalled={13}, AutoHideTimeout={14}, IsPreviewImageCached={15}, Valid={16}]", Tag, Id, ActionUrl, ImpressionUrl, PreviewUrl, PreviewPath, AppId, FirstPresentedToUserTime, Validity, ImpressionLimit, Impressions, WasPresentedToUser, SequenceTimeout, AppInstalled, AutoHideTimeout, IsPreviewImageCached, Valid);
        }
    }
}
