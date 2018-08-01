using SimpleJSON;
using Outfit7.Common.Promo;

namespace Outfit7.DreamingOf {
    public class DreamingOfUnmarshaller : PromoUnmarshaller<DreamingOfCampaign> {

        protected override DreamingOfCampaign UnmarshalAdditional(JSONNode nodeJ) {
            DreamingOfCampaign campaign = new DreamingOfCampaign();
            campaign.UserWatchCount = nodeJ["userWatchCount"].AsInt;
            campaign.ActionUrlChildSafe = nodeJ["aUCS"];

            return campaign;
        }

        protected override JSONNode MarshalAdditional(JSONNode nodeJ, DreamingOfCampaign campaign) {
            nodeJ["userWatchCount"].AsInt = campaign.UserWatchCount;
            nodeJ["aUCS"] = campaign.ActionUrlChildSafe;
            return nodeJ;
        }
    }
}
