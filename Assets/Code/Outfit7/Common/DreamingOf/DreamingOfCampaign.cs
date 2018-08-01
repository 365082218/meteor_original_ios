using System;
using Outfit7.Common.Promo;
using Outfit7.Util;
using Outfit7.Video.Gallery;

namespace Outfit7.DreamingOf {
    public class DreamingOfCampaign : PromoCampaign {

        public string VideoGalleryId { get; private set; }

        public int UserWatchCount { get; set; }

        public string ActionUrlChildSafe { get; set; }

        public override string Tag { get { return "DreamingOfCampaign"; } }

        public override void Init(string id, string cachePath, string actionUrl, string clickUrl, string previewUrl, int impressionLimit, int impressions,
            DateTime firstPresentedToUserTime, TimeSpan validity, bool wasPresentedToUser, int sequenceTimeout, string appId, int autoHideTimeout) {
            base.Init(id, cachePath, actionUrl, clickUrl, previewUrl, impressionLimit, impressions, firstPresentedToUserTime, validity, wasPresentedToUser, sequenceTimeout, appId, autoHideTimeout);

            VideoGalleryId = VideoGalleryManager.ParseVideoUrl(actionUrl);
        }

        public override string ToString() {
            return string.Format("[DreamingOfCampaign: Id={0}, ActionUrl={1}, ImpressionUrl={2}, PreviewUrl={3}, VideoGalleryId={4}, FirstPresentedToUserTime={5}, Validity={6}, UserWatchCount={7}, ImpressionLimit={8}, Impressions={9}, WasPresentedToUser={10}]", Id, ActionUrl, ImpressionUrl, PreviewUrl, VideoGalleryId, FirstPresentedToUserTime, Validity, UserWatchCount, ImpressionLimit, Impressions, WasPresentedToUser);
        }

        public void WasWatched() {
            UserWatchCount++;
        }

        public override bool Valid {
            get {

                if (UserWatchCount > 0) { // only one view
                    return false;
                }

                return base.Valid;
            }
        }

        public string GetAgeProperActionUrl(bool didPassAgeGate) {
            if (didPassAgeGate && StringUtils.HasText(ActionUrlChildSafe)) {
                return ActionUrlChildSafe;
            }

            return ActionUrl;
        }

    }
}
