using SimpleJSON;
using Outfit7.Util;

namespace Outfit7.Promo.SpecialOffers {
    public class SpecialOfferLabel {
        public string Tag { get { return this.GetType().Name; } }

        private const string JsonText = "t";
        private const string JsonTextColor = "tC";
        private const string JsonTextBorderColor = "bC";

        public string Text { get; private set; }

        public string TextColor { get; private set; }

        public string TextBorderColor { get; private set; }

        public SpecialOfferLabel(string text, string textColor, string textBorderColor) {
            Text = text;
            TextColor = textColor;
            TextBorderColor = textBorderColor;
        }

        public void UpdateText(string title) {
            Text = title;
        }

        public SpecialOfferLabel(JSONNode rawData) {
            Text = rawData[JsonText];
            TextColor = rawData[JsonTextColor];
            TextBorderColor = rawData[JsonTextBorderColor];
        }

        public virtual JSONClass ToJson() {
            JSONClass j = new JSONClass();
            j[JsonText] = Text;
            j[JsonTextColor] = TextColor;
            j[JsonTextBorderColor] = TextBorderColor;
            return j;
        }

        public virtual bool IsValid {
            get {
                if (string.IsNullOrEmpty(Text)) return true;
                if (string.IsNullOrEmpty(TextColor)) {
                    O7Log.DebugT(Tag, "TextColor not defined");
                    return false;
                }

                return true;
            }
        }

        public override string ToString() {
            return string.Format("[SpecialOfferText: Text={0}, TextColor={1}, TextBorderColor={2}]", Text, TextColor, TextBorderColor);
        }
    }
}
