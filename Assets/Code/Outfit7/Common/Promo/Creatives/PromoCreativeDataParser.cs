//
//   Copyright (c) 2016 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using Outfit7.Util;
using SimpleJSON;

namespace Outfit7.Common.Promo.Creatives {

    /// <summary>
    /// Promo creative data parser.
    /// </summary>
    public abstract class PromoCreativeDataParser {

        public PromoCreativeImagePosition DefaultBackgroundImagePosition = PromoCreativeImagePosition.Top;
        public PromoCreativeImagePosition DefaultOverlayImagePosition = PromoCreativeImagePosition.Top;

        public event Action<JSONNode, Exception> OnCreativeParseError;

        public virtual List<PromoCreativeData> ParseCreativeDatas(JSONArray creativesJ) {
            if (creativesJ == null || creativesJ.Count == 0) return null;

            List<PromoCreativeData> creatives = new List<PromoCreativeData>(creativesJ.Count);
            for (int i = 0; i < creativesJ.Count; i++) {
                JSONNode creativeJ = creativesJ[i];
                PromoCreativeData c = CreateCreativeData();
                try {
                    ParseCreativeData(c, creativeJ, i + 1);
                    ValidateCreativeData(c);

                } catch (Exception e) {
                    if (OnCreativeParseError != null) {
                        OnCreativeParseError(creativeJ, e);
                    }
                    continue;
                }
                creatives.Add(c);
            }
            return creatives;
        }

        protected abstract PromoCreativeData CreateCreativeData();

        public virtual void ParseCreativeData(PromoCreativeData data, JSONNode dataJ, int position) {
            string id = dataJ["id"];
            long timestamp = dataJ["ts"].AsLong;
            string backImageUrl = dataJ["bIU"];
            string backImagePosS = dataJ["bIP"];
            string overImageUrl = dataJ["oIU"];
            string overImagePosS = dataJ["oIP"];
            string impressionUrl = dataJ["iU"];
            string clickUrl = dataJ["cU"];
            string actionUrl = dataJ["aU"];

            PromoCreativeImagePosition backImagePos = ParseImagePosition(backImagePosS, DefaultBackgroundImagePosition);
            PromoCreativeImagePosition overImagePos = ParseImagePosition(overImagePosS, DefaultOverlayImagePosition);

            data.RawData = dataJ.AsObject;
            data.Id = id;
            data.Position = position;
            if (timestamp != 0) {
                data.Timestamp = TimeUtils.ToDateTime(timestamp);
            }
            data.BackgroundImageUrl = backImageUrl;
            data.BackgroundImagePosition = backImagePos;
            data.OverlayImageUrl = overImageUrl;
            data.OverlayImagePosition = overImagePos;
            data.ImpressionUrl = impressionUrl;
            data.ClickUrl = clickUrl;
            data.ActionUrl = actionUrl;
        }

        protected virtual PromoCreativeImagePosition ParseImagePosition(string positionS,
            PromoCreativeImagePosition defaultPosition) {
            if (positionS == null) return defaultPosition;
            switch (positionS) {
                case "top":
                    return PromoCreativeImagePosition.Top;
                case "center":
                    return PromoCreativeImagePosition.Center;
                case "bottom":
                    return PromoCreativeImagePosition.Bottom;
            }
            return defaultPosition;
        }

        public virtual void ValidateCreativeData(PromoCreativeData data) {
            Assert.HasText(data.Id, "Id");
            Assert.IsTrue(data.Timestamp != DateTime.MinValue, "Timestamp must be defined");
            Assert.HasText(data.BackgroundImageUrl, "BackgroundImageUrl");
            Assert.HasText(data.ClickUrl, "ClickUrl");
        }
    }
}
