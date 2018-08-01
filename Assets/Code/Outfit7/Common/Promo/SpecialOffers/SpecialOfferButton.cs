using SimpleJSON;
using Outfit7.Util;

namespace Outfit7.Promo.SpecialOffers {
    public class SpecialOfferButton : SpecialOfferLabel {

        private const string JsonGradientColor = "gC";

        public string GradientColor { get; private set; }

        public SpecialOfferButton(JSONNode rawData) : base(rawData) {
            GradientColor = rawData[JsonGradientColor];
        }

        public override JSONClass ToJson() {
            JSONClass j = base.ToJson();
            j[JsonGradientColor] = GradientColor;
            return j;
        }

        public override bool IsValid {
            get {
                if (string.IsNullOrEmpty(Text)) return false;
                if (string.IsNullOrEmpty(TextColor)) return false;
                if (string.IsNullOrEmpty(GradientColor)) return false;
                return true;
            }
        }

        public override string ToString() {
            return string.Format("[SpecialOfferButton: TextGradientColor={0} base={1}]", GradientColor, base.ToString());
        }
    }
}
