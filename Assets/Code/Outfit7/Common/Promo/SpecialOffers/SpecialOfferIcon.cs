using Outfit7.Util;
using Outfit7.Util.Io;

namespace Outfit7.Promo.SpecialOffers {
    public class SpecialOfferIcon {

        private const string Tag = "SpecialOfferIcon";

        public string Url { get; private set; }

        public string IconPath { get; private set; }

        public bool IsIconCached { get; private set; }

        public bool DidTryDownloadIcon { get; set; }

        public bool Required {
            get {
                return StringUtils.HasText(Url);
            }
        }

        public SpecialOfferIcon(string url) {
            Url = url;
        }

        public SpecialOfferIcon(string url, string cachePath) {
            Url = url;

            if (Required) {
                IconPath = cachePath + CryptoUtils.Sha1(Url);
                IsIconCached = O7File.Exists(IconPath);
            }
        }

        public void OnIconDownload(bool success) {
            O7Log.DebugT(Tag, "OnIconDownload {0}", success);
            DidTryDownloadIcon = true;
            IsIconCached = success;
        }

        public override string ToString() {
            return string.Format("[SpecialOfferIcon: Url={0}, IconPath={1}, IsIconCached={2}, DidTryDownloadIcon={3}, Required={4}]", Url, IconPath, IsIconCached, DidTryDownloadIcon, Required);
        }
    }
}
