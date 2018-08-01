//
//   Copyright (c) 2015 Outfit7. All rights reserved.
//

using System;
using Outfit7.Util;
using SimpleJSON;

namespace Outfit7.Common.Promo.Creatives {

    /// <summary>
    /// Promo creative data.
    /// </summary>
    public class PromoCreativeData {

        public JSONClass RawData { get; set; }

        public string Id { get; set; }

        public DateTime Timestamp { get; set; }

        public string BackgroundImageUrl { get; set; }

        public PromoCreativeImagePosition BackgroundImagePosition { get; set; }

        public string OverlayImageUrl { get; set; }

        public PromoCreativeImagePosition OverlayImagePosition { get; set; }

        public string ImpressionUrl { get; set; }

        public string ClickUrl { get; set; }

        public string ActionUrl { get; set; }

        // Not set by config but determined later
        /// <value>The position of this creative in the promo data. Starts with 1.</value>
        public int Position { get; set; }

        public virtual JSONClass ToJson() {
            JSONClass j = new JSONClass();
            j["id"] = Id;
            j["ts"].AsLong = TimeUtils.ToTimeMillis(Timestamp);
            j["bckImgUrl"] = BackgroundImageUrl;
            j["bckImgPos"] = BackgroundImagePosition.ToString();
            j["ovrImgUrl"] = OverlayImageUrl;
            j["ovrImgPos"] = OverlayImagePosition.ToString();
            j["impUrl"] = ImpressionUrl;
            j["clkUrl"] = ClickUrl;
            j["actUrl"] = ActionUrl;
            j["pos"].AsInt = Position;
            return j;
        }

        public override string ToString() {
            return string.Format("[PromoCreativeData: Id={0}, Position={1}, Timestamp={2}, BackgroundImageUrl={3}, BackgroundImagePosition={4}, OverlayImageUrl={5}, OverlayImagePosition={6}, ImpressionUrl={7}, ClickUrl={8}, ActionUrl={9}]",
                Id, Position, Timestamp, BackgroundImageUrl, BackgroundImagePosition, OverlayImageUrl, OverlayImagePosition, ImpressionUrl, ClickUrl, ActionUrl);
        }
    }
}
