//
//   Copyright (c) 2016 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;

namespace Outfit7.Text.Localization {

    /// <summary>
    /// A list of language codes that we support.
    /// </summary>
    public static class LanguageCodes {

        public const string Arabic = "ar";
        public const string English = "en";
        public const string Chinese = "zh";
        public const string ChineseTraditional = "zh-hant";
        public const string French = "fr";
        public const string German = "de";
        public const string Hindi = "hi";
        public const string Italian = "it";
        public const string Japanese = "ja";
        public const string Korean = "ko";
        public const string Portuguese = "pt";
        public const string Russian = "ru";
        public const string Spanish = "es";
        public const string Turkish = "tr";

        public static readonly List<string> All = new List<string>(20) {
            // Ordered by frequency of use
            Chinese, ChineseTraditional, English, Russian, Spanish, Portuguese,
#if UNITY_IOS || UNITY_EDITOR
            Arabic,
#endif
            Turkish, French, German, Italian, Korean, Japanese, Hindi
        };

        public static string Parse(string rawLangCode) {
            if (rawLangCode == null) return English;
            rawLangCode = rawLangCode.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(rawLangCode)) return English;
            if (All.Contains(rawLangCode)) return rawLangCode;

            for (int i = 0; i < All.Count; i++) {
                string lc = All[i];
                if (!rawLangCode.StartsWith(lc, StringComparison.OrdinalIgnoreCase)) continue;
                if (lc == Chinese && ContainsTraditionalChineseCountry(rawLangCode)) return ChineseTraditional;
                return lc;
            }

            return English;
        }

        private static bool ContainsTraditionalChineseCountry(string rawLangCode) {
            if (rawLangCode.Contains("hant")) return true;
            if (rawLangCode.Contains("tw")) return true;
            if (rawLangCode.Contains("hk")) return true;
            if (rawLangCode.Contains("mo")) return true;
            return false;
        }

        public static bool IsEasternLanguage(string langCode) {
            switch (langCode) {
                case Chinese:
                case ChineseTraditional:
                case Arabic:
                case Korean:
                case Japanese:
                case Hindi:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsHanLanguage(string langCode) {
            switch (langCode) {
                case Chinese:
                case ChineseTraditional:
                case Korean:
                case Japanese:
                    return true;
                default:
                    return false;
            }
        }
    }
}
