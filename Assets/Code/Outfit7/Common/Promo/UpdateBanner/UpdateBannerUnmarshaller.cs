//
//   Copyright (c) 2015 Outfit7. All rights reserved.
//

using Outfit7.Common.Promo;
using SimpleJSON;

namespace Outfit7.Promo.UpdateBanner {

    public class UpdateBannerUnmarshaller : PromoUnmarshaller<UpdateBannerCampaign> {

        protected override UpdateBannerCampaign UnmarshalAdditional(JSONNode nodeJ) {
            UpdateBannerCampaign campaign = new UpdateBannerCampaign();
            campaign.Type = nodeJ["t"].Value;
            return campaign;
        }

        protected override JSONNode MarshalAdditional(JSONNode nodeJ, UpdateBannerCampaign campaign) {
            nodeJ["t"] = campaign.Type;
            return nodeJ;
        }
    }
}
