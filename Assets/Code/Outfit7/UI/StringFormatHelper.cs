using System.Globalization;
using Outfit7.Text.Localization;


namespace Outfit7.UI {
    public static class StringFormatHelper {
        private static NumberFormatInfo CustomNumberFormat;

        public static string FormatCurrencyAmountShort(int amount) {
            return FormatCurrencyAmountShort(amount, "K", "M");
        }

        public static string FormatCurrencyAmountShort(int amount, string magnitudeThousands, string magnitudeMillions) {
            if (amount >= 1000000) {
                int shortAmount = (int) (amount / 100000f); // Cut decimals
                return string.Format("{0:#,0.0}{1}", shortAmount / 10f, magnitudeMillions);

            } else if (amount >= 10000) {
                int shortAmount = (int) (amount / 100f); // Cut decimals
                return string.Format("{0:#,0.0}{1}", shortAmount / 10f, magnitudeThousands);
            }
            return amount.ToString("#,0");
        }

        public static string FormatCurrencyAmountSemiShort(int amount) {
            if (amount >= 1000000) {
                int shortAmount = (int) (amount / 100000f); // Cut decimals
                return string.Format("{0:#,0.0}M", shortAmount / 10f);

            }
            return amount.ToString("#,0");
        }

        public static string FormatCurrencyAmountLong(int amount) {
            return amount.ToString("#,0");
        }

        public static string FormatCurrencyAmountLongSigned(int amount) {
            return amount.ToString("+#,0;-#,0;");
        }

        public static string FormatWithHairSpace(int amount) {
            if (amount < 10000) {
                return amount.ToString();
            } else {
                if (CustomNumberFormat == null) {
                    CustomNumberFormat = (NumberFormatInfo) CultureInfo.InvariantCulture.NumberFormat.Clone();
                    CustomNumberFormat.NumberGroupSeparator = "\u200a";
                }
                return amount.ToString("#,0", CustomNumberFormat);
            }
        }
    }
}