//
//   Copyright (c) 2015 Outfit7. All rights reserved.
//

using Outfit7.Common.Promo;

namespace Outfit7.Promo.UpdateBanner {

    public class UpdateBannerCampaign : PromoCampaign {

        public override string Tag { get { return "UpdateBannerCampaign"; } }

        public string Type { get; set; }
    }
}
