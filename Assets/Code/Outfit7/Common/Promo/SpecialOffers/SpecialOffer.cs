//
//   Copyright (c) 2015 Outfit7. All rights reserved.
//

using System;
using SimpleJSON;
using Outfit7.Util;
using Outfit7.Util.Io;

namespace Outfit7.Promo.SpecialOffers {

    /// <summary>
    /// Special offer pack.
    /// </summary>
    public abstract class SpecialOffer {

        public const string JsonType = "ty";
        private const string JsonId = "id";
        private const string JsonBackImageUrl = "bIU";
        private const string JsonTimeLimit = "tL";
        private const string JsonTitle = "t";
        private const string JsonItemDescription = "iD";
        private const string JsonItemTitle = "iT";
        private const string JsonTimer = "ti";
        private const string JsonTimerTimeLeft = "tiTL";
        private const string JsonTimerBackgroundColor = "tiBC";
        private const string JsonBuyButton = "bB";
        private const string JsonImpressionUrl = "iU";
        private const string JsonClickUrl = "cU";
        private const string JsonActionUrl = "aU";
        private const string JsonOldPrice = "oP";
        private const string JsonNewPrice = "nP";
        private const string JsonBadgeIconUrl = "bU";
        private const string JsonSpecialOfferStyleType = "s";

        public virtual string Tag { get { return this.GetType().Name; } }

        public string Id { get; private set; }

        public string BackImageUrl { get; private set; }

        public string BackImagePath { get; private set; }

        public TimeSpan TimeLimit { get; private set; }

        public SpecialOfferType Type { get; private set; }

        public string ImpressionUrl { get; private set; }

        public string ClickUrl { get; private set; }

        public string ActionUrl { get; private set; }

        public SpecialOfferLabel TitleLabel { get; private set; }

        public SpecialOfferLabel ItemDescriptionLabel { get; private set; }

        public SpecialOfferLabel ItemTitleLabel { get; private set; }

        public SpecialOfferLabel TimerLabel { get; private set; }

        public SpecialOfferLabel TimerTimeLeftLabel { get; private set; }

        public string TimerBackgroundColor { get; private set; }

        public SpecialOfferButton BuyButton { get; private set; }

        public bool IsBackgroundImageCached { get; private set; }

        public bool DidTryDownloadBackgroundImage { get; private set; }

        public SpecialOfferLabel OldPriceLabel { get; private set; }

        public SpecialOfferLabel NewPriceLabel { get; private set; }

        public SpecialOfferIcon Icon { get; private set; }

        public string SpecialOfferStyleId { get; private set; }

        protected SpecialOffer(JSONNode rawData, string cachePath) {
            Id = rawData[JsonId];
            TimeLimit = TimeSpan.FromSeconds(rawData[JsonTimeLimit].AsInt);
            BackImageUrl = rawData[JsonBackImageUrl];
            Type = SpecialOfferType.GetType(rawData);
            ImpressionUrl = rawData[JsonImpressionUrl];
            ClickUrl = rawData[JsonClickUrl];
            ActionUrl = rawData[JsonActionUrl];
            TitleLabel = new SpecialOfferLabel(rawData[JsonTitle]);
            ItemDescriptionLabel = new SpecialOfferLabel(rawData[JsonItemDescription]);
            ItemTitleLabel = new SpecialOfferLabel(rawData[JsonItemTitle]);
            TimerLabel = new SpecialOfferLabel(rawData[JsonTimer]);
            TimerTimeLeftLabel = new SpecialOfferLabel(rawData[JsonTimerTimeLeft]);
            TimerBackgroundColor = rawData[JsonTimerBackgroundColor];
            BuyButton = new SpecialOfferButton(rawData[JsonBuyButton]);
            Icon = new SpecialOfferIcon(rawData[JsonBadgeIconUrl], cachePath);

            if (BackImageUrl != null) {
                BackImagePath = cachePath + CryptoUtils.Sha1(BackImageUrl);
            }

            IsBackgroundImageCached = O7File.Exists(BackImagePath);

            OldPriceLabel = new SpecialOfferLabel(rawData[JsonOldPrice]);
            NewPriceLabel = new SpecialOfferLabel(rawData[JsonNewPrice]);

            SpecialOfferStyleId = rawData[JsonSpecialOfferStyleType];
        }

        public virtual JSONClass ToJson() {
            JSONClass j = new JSONClass();
            j[JsonId] = Id;
            j[JsonTimeLimit].AsInt = (int) TimeLimit.TotalSeconds;
            j[JsonBackImageUrl] = BackImageUrl;
            j[JsonType] = Type.StringType;
            j[JsonActionUrl] = ActionUrl;
            j[JsonImpressionUrl] = ImpressionUrl;
            j[JsonClickUrl] = ClickUrl;
            j[JsonTitle] = TitleLabel.ToJson();
            j[JsonItemDescription] = ItemDescriptionLabel.ToJson();
            j[JsonItemTitle] = ItemTitleLabel.ToJson();
            j[JsonTimer] = TimerLabel.ToJson();
            j[JsonTimerTimeLeft] = TimerTimeLeftLabel.ToJson();
            j[JsonTimerBackgroundColor] = TimerBackgroundColor;
            j[JsonBuyButton] = BuyButton.ToJson();
            j[JsonOldPrice] = OldPriceLabel.ToJson();
            j[JsonNewPrice] = NewPriceLabel.ToJson();
            j[JsonBadgeIconUrl] = Icon.Url;
            j[JsonSpecialOfferStyleType] = SpecialOfferStyleId;
            return j;
        }

        public virtual void InvalidateStateData() {
        }

        public virtual void OnAppStartOrResume(bool activated) {
            if (activated) return;
            DidTryDownloadBackgroundImage = false;
        }

        public virtual bool CanActivate {
            get {
                return IsCreativesCached;
            }
        }

        public virtual bool IsCreativesCached {
            get {
                if (!IsBackgroundImageCached) return false;
                if (Icon.Required && !Icon.IsIconCached) return false;
                return true;
            }
        }

        public virtual bool IsValid {
            get {
                if (Type == null) {
                    O7Log.DebugT(Tag, "Type not valid");
                    return false;
                }
                if (!StringUtils.HasText(Id)) {
                    O7Log.DebugT(Tag, "Id not defined");
                    return false;
                }
                if (!StringUtils.HasText(BackImageUrl)) {
                    O7Log.DebugT(Tag, "BackImageUrl not defined");
                    return false;
                }
                if (TimeLimit <= TimeSpan.Zero) {
                    O7Log.DebugT(Tag, "TimeLimit <= TimeSpan.Zero");
                    return false;
                }
                // UI check
                if (!TitleLabel.IsValid) {
                    O7Log.DebugT(Tag, "TitleLabel not valid");
                    return false;
                }
                if (!ItemTitleLabel.IsValid) {
                    O7Log.DebugT(Tag, "ItemTitleLabel not valid");
                    return false;
                }
                if (!ItemDescriptionLabel.IsValid) {
                    O7Log.DebugT(Tag, "ItemDescriptionLabel not valid");
                    return false;
                }
                if (!BuyButton.IsValid) {
                    O7Log.DebugT(Tag, "BuyButton not valid");
                    return false;
                }

                if (string.IsNullOrEmpty(ItemDescriptionLabel.Text)) {
                    if (string.IsNullOrEmpty(OldPriceLabel.TextColor)) {
                        O7Log.DebugT(Tag, "ItemDescriptionLabel not defined so should be OldPriceLabel.TextColor");
                        return false;
                    }
                    if (string.IsNullOrEmpty(NewPriceLabel.TextColor)) {
                        O7Log.DebugT(Tag, "ItemDescriptionLabel not defined so should be NewPriceLabel.TextColor");
                        return false;
                    }
                }

                return true;
            }
        }

        public void OnBackgroundImageDownload(bool success) {
            O7Log.DebugT(Tag, "OnBackgroundImageDownload {0}", success);
            DidTryDownloadBackgroundImage = true;
            IsBackgroundImageCached = success;
        }

        public override string ToString() {
            return string.Format("[SpecialOffer: Id={0}, BackImageUrl={1}, BackImagePath={2}, TimeLimit={3}, Type={4}, ImpressionUrl={5}, ClickUrl={6}, ActionUrl={7}, TitleLabel={8}, ItemDescriptionLabel={9}, ItemTitleLabel={10}, TimerLabel={11}, TimerTimeLeftLabel={12}, TimerBackgroundColor={13}, BuyButton={14}, IsBackgroundImageCached={15}, DidTryDownloadBackgroundImage={16}, OldPriceLabel={17}, NewPriceLabel={18}, Icon={19}, SpecialOfferStyleId={20}]", Id, BackImageUrl, BackImagePath, TimeLimit, Type, ImpressionUrl, ClickUrl, ActionUrl, TitleLabel, ItemDescriptionLabel, ItemTitleLabel, TimerLabel, TimerTimeLeftLabel, TimerBackgroundColor, BuyButton, IsBackgroundImageCached, DidTryDownloadBackgroundImage, OldPriceLabel, NewPriceLabel, Icon, SpecialOfferStyleId);
        }
    }
}
