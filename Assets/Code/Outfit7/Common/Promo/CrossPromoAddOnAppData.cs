using Outfit7.Util;
using UnityEngine;

namespace Outfit7.Common.Promo {
    public class CrossPromoAddOnAppData {

        protected virtual string Tag { get { return GetType().Name; } }

        public string AddOnId { get; private set; }

        public string AppId { get; private set; }

        public string ActionUrl { get; private set; }

        public string IconUrl { get; private set; }

        public string LocalIconUrl { get; private set; }

        public string L10nKey { get; private set; }

        public string PromoText { get; private set; }

        public bool IsUnlocked { get; private set; }

        public virtual Texture2D IconTexture {
            get {
                if (LoadedIconTexture != null) {
                    O7Log.DebugT(Tag, "Returning loaded Icon for {0}", AddOnId);
                    return LoadedIconTexture;
                }
                if (LocalIconTexture == null) {
                    LocalIconTexture = Resources.Load<Texture2D>(LocalIconUrl);
                }
                O7Log.DebugT(Tag, "Returning local Icon for {0}", AddOnId);
                return LocalIconTexture;
            }
        }

        protected Texture2D LoadedIconTexture;
        protected Texture2D LocalIconTexture;

        public CrossPromoAddOnAppData(string addOnId, string appId, string actionUrl, string localIconUrl, string l10nKey, string iconUrl, string promoText, bool isUnlocked) {
            AddOnId = addOnId;
            AppId = appId;
            ActionUrl = actionUrl;
            LocalIconUrl = localIconUrl;
            L10nKey = l10nKey;
            IconUrl = iconUrl;
            PromoText = promoText;
            IsUnlocked = isUnlocked;
        }

        public virtual void SetData(string appId, string actionUrl, string iconUrl, string promoText, bool isUnlocked) {
            O7Log.DebugT(Tag, "Set data for {0}", AddOnId);
            if (!string.IsNullOrEmpty(appId)) {
                AppId = appId;
            }
            if (!string.IsNullOrEmpty(actionUrl)) {
                ActionUrl = actionUrl;
            }
            if (!string.IsNullOrEmpty(iconUrl)) {
                IconUrl = iconUrl;
            }
            PromoText = promoText;
            IsUnlocked = isUnlocked;
        }

        public virtual void SetDownloadedTexture(Texture2D texture) {
            LoadedIconTexture = texture;
        }
    }
}
