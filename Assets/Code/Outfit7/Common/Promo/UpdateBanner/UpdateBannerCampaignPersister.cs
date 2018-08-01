//
//   Copyright (c) 2015 Outfit7. All rights reserved.
//

using Outfit7.Common.Promo;

namespace Outfit7.Promo.UpdateBanner {

    public class UpdateBannerCampaignPersister : PromoCampaignPersister {

        private const string CampaignFileName = "UpdateBannerCampaign.json";

        public static void ClearPrefs() {
            DeleteFile(CampaignFileName);
        }

        public UpdateBannerCampaignPersister() : base(CampaignFileName) {
        }

        public override void DeleteCampaign() {
            ClearPrefs();
        }
    }
}
