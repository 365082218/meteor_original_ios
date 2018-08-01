//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System.Globalization;
using UnityEngine;

namespace Outfit7.Util {

    /// <summary>
    /// Culture utilities.
    /// </summary>
    public static class CultureUtils {

        private const string Tag = "CultureUtils";

        private static void SetCulture(CultureInfo culture) {
// FIXME TineL: Crashes on iOS
//            CultureInfo.CurrentCulture = culture;
//            CultureInfo.CurrentUICulture = culture;
        }

        private static void SetCulture(string cultureName) {
// FIXME TineL: Crashes on iOS
//            CultureInfo culture = CultureInfo.CreateSpecificCulture(cultureName);
//            SetCulture(culture);
        }

        public static void SetupCulture() {
            SystemLanguage sysLang = Application.systemLanguage;
            string cultureName = TranslateCulture(sysLang);
            SetCulture(cultureName);

            O7Log.InfoT(Tag, "System language = '{0}', culture name = '{1}', culture = {2}", sysLang, cultureName, CultureInfo.CurrentCulture);
        }

        private static string TranslateCulture(SystemLanguage sysLang) {
            // Support only cultures that our QA is testing
            // http://www.csharp-examples.net/culture-names/
            switch (sysLang) {
                case SystemLanguage.Arabic:
                    return "ar-SA"; // Saudi Arabia
                case SystemLanguage.Chinese:
                    return "zh-CN"; // China mainland
                case SystemLanguage.French:
                    return "fr-FR";
                case SystemLanguage.German:
                    return "de-DE";
                case SystemLanguage.Italian:
                    return "it-IT";
                case SystemLanguage.Japanese:
                    return "ja-JP";
                case SystemLanguage.Korean:
                    return "ko-KR";
                case SystemLanguage.Portuguese:
                    return"pt-BR"; // Brazil
                case SystemLanguage.Russian:
                    return "ru-RU";
                case SystemLanguage.Spanish:
                    return "es-ES";
                case SystemLanguage.Turkish:
                    return "tr-TR";
                default:
                    return "en-US"; // USA
            }
        }
    }
}
