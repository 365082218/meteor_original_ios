//
//   Copyright (c) 2015 Outfit7. All rights reserved.
//

using Outfit7.Common.Promo;
using Outfit7.Util;

namespace Outfit7.Promo.UpdateBanner {

    public class UpdateBannerManager : PromoManager<UpdateBannerCampaign> {

        private const string CacheName = "UpdateBanner";
        private const string PrefLastLanConnectivityTime = "UpdateBannerManager.LastLanConnectivityTime";
        private const string PrefCampaignHistory = "UpdateBannerManager.CampaignHistory";

        public static void ClearPrefs() {
            UpdateBannerCampaignPersister.ClearPrefs();
            DeleteCache(CacheName);
            UserPrefs.Remove(PrefLastLanConnectivityTime);
            UserPrefs.Remove(PrefCampaignHistory);
        }

        protected override string Tag { get { return "UpdateBannerManager"; } }

        private int LastSession = -1;

        public AppSession AppSession { get; set; }

        public override void Init() {
            PromoCampaignPersister = new UpdateBannerCampaignPersister();
            PromoUnmarshaller = new UpdateBannerUnmarshaller();
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
                return "updateBanner";
            }
        }

        protected override string AnalyticsName {
            get {
                return "Update banner";
            }
        }

        public override bool CanShow {
            get {
                if (LastSession == AppSession.SessionId) {
                    return false;
                }

                if (!base.CanShow) {
                    return false;
                }

                return true;
            }
        }

        protected override void OpenOverride(UpdateBannerCampaign campaign) {
            OnClose();
            base.OpenOverride(campaign);
        }

        public void OnClose() {
            LastSession = AppSession.SessionId;
        }
    }
}
