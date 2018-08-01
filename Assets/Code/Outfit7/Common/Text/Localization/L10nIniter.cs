//
//   Copyright (c) 2016 Outfit7. All rights reserved.
//

using Outfit7.Common;
using UnityEngine;

namespace Outfit7.Text.Localization {

    /// <summary>
    /// Localization initializer.
    /// </summary>
    public static class L10nIniter {

        public static void InitAppLanguage(TextMesh textMesh = null) {
            string langCode = L10n.LocalizationManagerInstance.OverriddenLanguage;

            if (langCode == null) {
                // Get original language from native
                langCode = AppPlugin.LanguageCode;

                // Parse to our known language
                langCode = LanguageCodes.Parse(langCode);
            }

            if (textMesh != null) {
                // Check if renders in Unity -> fallback to English if not
                langCode = LocalizationHelper.CheckLanguageRendering(langCode, textMesh);
            }

            // Finally set language & load localization assets
            L10n.ChangeLanguage(langCode, true);
        }
    }
}
