using Outfit7.Util;
using SimpleJSON;
using System.Collections.Generic;
using System;

namespace Outfit7.Common.Promo {
    public abstract class PromoUnmarshaller<PC> where PC : PromoCampaign {

        protected abstract PC UnmarshalAdditional(JSONNode nodeJ);

        protected virtual JSONNode MarshalAdditional(JSONNode nodeJ, PC campaign) {
            return nodeJ;
        }

        public List<PC> UnmarshalCampaigns(JSONArray arrayJ, string cachePath) {

            if (arrayJ == null) {
                return null;
            }

            List<PC> campaigns = null;
            foreach (JSONNode nodeJ in arrayJ) {
                PC campaign = Unmarshal(nodeJ, cachePath);
                if (campaign != null) {
                    if (campaigns == null) {
                        campaigns = new List<PC>();
                    }
                    campaigns.Add(campaign);
                }
            }

            return campaigns;
        }

        public PC Unmarshal(JSONNode nodeJ, string cachePath) {

            if (nodeJ == null) {
                return null;
            }

            // server side
            string adId = nodeJ["adId"];
            string actionUrl = nodeJ["actionUrl"];
            string previewUrl = nodeJ["previewUrl"];
            string impressionUrl = nodeJ["impUrl"];
            int impressionLimit = nodeJ["impCount"].AsInt;
            TimeSpan validity = TimeSpan.FromSeconds(nodeJ["expirationTime"].AsInt);
            string appId = nodeJ["appId"];

            // local specific
            DateTime firstPresentedToUserTime = nodeJ["firstPresentedToUserTime"].AsDateTime;
            int impressions = nodeJ["impressions"].AsInt;
            bool wasPresentedToUser = nodeJ["wasPresentedToUser"].AsBool;
            int sequenceTimeout = nodeJ["sequence"].AsInt;
            int autoHideTimeout = nodeJ["aHT"].AsInt;

            PC campaign = null;

            if (StringUtils.HasText(adId) && StringUtils.HasText(actionUrl) && StringUtils.HasText(previewUrl)) {

                campaign = UnmarshalAdditional(nodeJ);
                campaign.Init(adId, cachePath, actionUrl, impressionUrl, previewUrl, impressionLimit, impressions, firstPresentedToUserTime, validity, wasPresentedToUser, sequenceTimeout, appId, autoHideTimeout);
            }

            return campaign;
        }

        public JSONNode MarshalCampaign(PC campaign) {
            JSONClass nodeJ = new JSONClass();

            nodeJ["adId"] = campaign.Id;
            nodeJ["actionUrl"] = campaign.ActionUrl;
            nodeJ["previewUrl"] = campaign.PreviewUrl;
            nodeJ["impUrl"] = campaign.ImpressionUrl;
            nodeJ["impCount"].AsInt = campaign.ImpressionLimit;
            nodeJ["expirationTime"].AsInt = (int) campaign.Validity.TotalSeconds;
            nodeJ["appId"] = campaign.AppId;

            nodeJ["firstPresentedToUserTime"].AsDateTime = campaign.FirstPresentedToUserTime;

            nodeJ["wasPresentedToUser"].AsBool = campaign.WasPresentedToUser;
            nodeJ["impressions"].AsInt = campaign.Impressions;
            nodeJ["sequence"].AsInt = campaign.SequenceTimeout;
            nodeJ["aHT"].AsInt = campaign.AutoHideTimeout;

            MarshalAdditional(nodeJ, campaign);

            return nodeJ;
        }
    }
}
