//
//   Copyright (c) 2016 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Text;
using Outfit7.Util;
using SimpleJSON;
using UnityEngine;

namespace Outfit7.Text.Localization {

    /// <summary>
    /// Localization text helper.
    /// </summary>
    public static class LocalizationHelper {

        private const string Tag = "LocalizationHelper";

#region Loader

        public const string KeyLocalizationAssetName = "key";

        public static string GetLocalizationAssetPath(string lang) {
            return "Localizations/" + lang;
        }

        public static Dictionary<string, string> LoadLocalizationAssets(string langCode) {
            O7Log.DebugT(Tag, "Loading localization key asset");
            LocalizationAsset keysAsset = Resources.Load<LocalizationAsset>(GetLocalizationAssetPath(KeyLocalizationAssetName));
            Assert.State(keysAsset != null, "Invalid localization key asset");

            O7Log.DebugT(Tag, "Loading localization translation asset for language={0}", langCode);
            LocalizationAsset localizationAsset = Resources.Load<LocalizationAsset>(GetLocalizationAssetPath(langCode));
            Assert.State(localizationAsset != null, "Invalid localization translation asset for language={0}", langCode);

            Assert.IsTrue(keysAsset.Values.Length == localizationAsset.Values.Length, "{0} {1} Number of keys does not match number of translations!",
                keysAsset.Values.Length, localizationAsset.Values.Length);

            var keyTranslations = new Dictionary<string, string>(keysAsset.Values.Length);
            for (int i = 0; i < keysAsset.Values.Length; i++) {
                if (!keyTranslations.ContainsKey(keysAsset.Values[i]))
                {
                    keyTranslations.Add(keysAsset.Values[i], localizationAsset.Values[i]);
                }
            }

            Resources.UnloadAsset(keysAsset);
            Resources.UnloadAsset(localizationAsset);

            return keyTranslations;
        }

#endregion

#region Text

        public static string GetText(JSONNode langMap, string langCode, string defaultLangCode,
            bool rightToLeftEnabled, params object[] args) {
            string text = langMap[langCode];
            if (text == null) {
                if (langCode == defaultLangCode) return null;
                // Fallback to default language
                langCode = defaultLangCode;
                text = langMap[langCode];
                if (text == null) return null; // No value even for default language
            }

            return FormatText(text, langCode, rightToLeftEnabled, args);
        }

        public static string FormatText(string text, string langCode, bool rightToLeftEnabled, params object[] args) {
            if (args != null) {
                text = string.Format(text, args);
            }

#if UNITY_IOS || UNITY_EDITOR
            if (rightToLeftEnabled && langCode == LanguageCodes.Arabic) {
                text = FixArabic(text);
            }
#endif

            return text;
        }

#if UNITY_IOS || UNITY_EDITOR
        public static string FixArabic(string text) {
            if (string.IsNullOrEmpty(text)) return text;
            if (text.Contains("\n")) {
                text = FixArabicNewlineText(text);
            } else {
                text = UnityArabicSupport.ArabicFixer.Fix(text);
            }

            return text;
        }

        private static string FixArabicNewlineText(string str) {
            StringBuilder sb = new StringBuilder();
            char[] separators = { '\n' };
            string[] lines = str.Split(separators, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++) {
                sb.Append(UnityArabicSupport.ArabicFixer.Fix(lines[i]));
                if (i < lines.Length - 1)
                    sb.Append("\n");
            }

            return sb.ToString();
        }
#endif

#endregion

#region Language Rendering

        public static string CheckLanguageRendering(string langCode, TextMesh textMesh) {
            bool renders = TryLanguageRendering(langCode, textMesh);
            return renders ? langCode : LanguageCodes.English;
        }

        public static bool TryLanguageRendering(string langCode, TextMesh textMesh) {
            // MTT-2760 Check for Korean language on LG Optimus L series
            // Using unicode converter http://www.endmemo.com/unicode/unicodeconverter.php
            switch (langCode) {
                case LanguageCodes.Chinese:
                case LanguageCodes.ChineseTraditional:
                    textMesh.text = "\u7279\u4EF7"; // 2 Chinese characters
                    break;
                case LanguageCodes.Korean:
                    textMesh.text = "\uCE68\uC2E4"; // 2 Korean characters
                    break;
                case LanguageCodes.Japanese:
                    textMesh.text = "\u30C3\u30B7"; // 2 Japanese characters
                    break;
                default:
                    return true;
            }

            // Check if size is almost 0
            bool noText = Math.Abs(textMesh.GetComponent<Renderer>().bounds.size.x) < 0.01f;

            textMesh.text = ""; // Clear
            return !noText;
        }

#endregion
    }
}
