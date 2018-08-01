//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using Outfit7.Util;

namespace Outfit7.Video {

    /// <summary>
    /// Video clip data.
    /// </summary>
    public class VideoClipData {

        public string Amount { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this video clip's provider is mentioning/showing actual currency earning in its landing page.
        /// </summary>
        /// <value><c>true</c> if this video clip is mentioning currency on its landing page; otherwise, <c>false</c>.</value>
        public bool IsCurrencyMentioning { get; private set; }

        public VideoClipData(string amount, bool currencyMentioning) {
            Assert.HasText(amount, "amount");
            Amount = amount;
            IsCurrencyMentioning = currencyMentioning;
        }

        public override string ToString() {
            return string.Format("[VideoClipData: Amount={0}, IsCurrencyMentioning={1}]", Amount, IsCurrencyMentioning);
        }
    }
}
