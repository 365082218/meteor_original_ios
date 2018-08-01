using Outfit7.Common;
using Outfit7.Common.Promo;
using Outfit7.Util;
using Outfit7.Video.Gallery;

namespace Outfit7.DreamingOf {
    public abstract class DreamingOfManager : PromoManager<DreamingOfCampaign> {

        private const string CacheName = "DreamingOf";
        private const string PrefLastLanConnectivityTime = "DreamingOfManager.LastLanConnectivityTime";
        private const string PrefCampaignHistory = "DreamingOfManager.CampaignHistory";

        public static void ClearPrefs() {
            DreamingOfCampaignPersister.ClearPrefs();
            DeleteCache(CacheName);
            UserPrefs.Remove(PrefLastLanConnectivityTime);
            UserPrefs.Remove(PrefCampaignHistory);
        }

        protected override string Tag { get { return "DreamingOfManager"; } }

        public VideoGalleryManager VideoGalleryManager { get; set; }

        public AgeGateManager AgeGateManager { get; set; }

        public abstract bool OpenVideoGallery(string videoGalleryId);

        public override void Init() {
            PromoCampaignPersister = new DreamingOfCampaignPersister();
            PromoUnmarshaller = new DreamingOfUnmarshaller();
            base.Init();
        }

        protected override string LastLanConnectivityTimePrefKey {
            get {
                return PrefLastLanConnectivityTime;
            }
        }

        protected override string CampaignHistoryPrefKey {
            get {
                return PrefCampaignHistory;
            }
        }

        protected override string CachePath {
            get {
                return CreateCachePath(CacheName);
            }
        }

        protected override string GridCampaignJsonKey {
            get {
                return "adDreamingOf";
            }
        }

        protected override string AnalyticsName {
            get {
                return "Dream bubble";
            }
        }

        public override bool CanShow {
            get {
                if (!base.CanShow) {
                    return false;
                }

                if (!VideoGalleryManager.Ready
                    && (StringUtils.HasText(Campaign.VideoGalleryId)
                        || (VideoGalleryManager.IsInternalVideoGalleryUrl(Campaign.GetAgeProperActionUrl(AgeGateManager.DidPass))))) {
                    return false;
                }

                return true;
            }
        }

        protected override void OpenOverride(DreamingOfCampaign campaign) {
            campaign.WasWatched();
            SaveCampaign(campaign);
            string ageProperUrl = campaign.GetAgeProperActionUrl(AgeGateManager.DidPass);

            if (VideoGalleryManager.IsInternalVideoGalleryUrl(ageProperUrl)) {
                O7Log.DebugT(Tag, "Opening video gallery with url: {0}", ageProperUrl);
                OpenVideoGallery(ageProperUrl);
            } else if (StringUtils.HasText(campaign.VideoGalleryId)) {
                O7Log.DebugT(Tag, "Opening video gallery with id {0}", campaign.VideoGalleryId);
                OpenVideoGallery(campaign.VideoGalleryId);
            } else {
                O7Log.DebugT(Tag, "Opening url {0}", ageProperUrl);
                AppPlugin.ResolveAndOpenUrl(ageProperUrl);
            }
        }
    }
}
