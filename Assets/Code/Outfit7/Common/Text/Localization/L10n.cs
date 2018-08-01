//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

namespace Outfit7.Text.Localization {

    /// <summary>
    /// Localization text manager shortcut.
    /// </summary>
    public static class L10n {

        private static LocalizationManager localizationManager;

        public static LocalizationManager LocalizationManagerInstance {
            get {
                if (localizationManager == null) {
                    localizationManager = new LocalizationManager();
                    localizationManager.Init();
                }
                return localizationManager;
            }
        }

        public static string Language {
            get {
                return LocalizationManagerInstance.Language;
            }
        }

        public static bool IsEasternLanguage {
            get {
                return LocalizationManagerInstance.IsEasternLanguage;
            }
        }

        public static bool IsHanLanguage {
            get {
                return LocalizationManagerInstance.IsHanLanguage;
            }
        }

        public static bool IsArabicLanguage {
            get {
                return IsLanguage(LanguageCodes.Arabic);
            }
        }

        public static bool IsLanguage(string language) {
            return LocalizationManagerInstance.IsLanguage(language);
        }

        public static void ChangeLanguage(string langCode, bool loadLocalizations = false) {
            LocalizationManagerInstance.ChangeLanguage(langCode, loadLocalizations);
        }

        public static void ChangeToNextOverriddenLanguage() {
            LocalizationManagerInstance.ChangeToNextOverriddenLanguage();
        }

        public static string CheckAndFixArabic(string text) {
            return CheckAndFixArabic(text, null);
        }

        public static string CheckAndFixArabic(string text, params object[] args) {
            if (args != null) {
                text = string.Format(text, args);
            }

#if UNITY_IOS || UNITY_EDITOR
            return IsArabicLanguage ? LocalizationHelper.FixArabic(text) : text;
#else
            return text;
#endif
        }

        public static string GetText(string key) {
            return GetText(key, true, null);
        }

        public static string GetText(string key, bool rightToLeft) {
            return GetText(key, rightToLeft, null);
        }

        public static string GetText(string key, params object[] args) {
            return LocalizationManagerInstance.GetText(key, true, args);
        }

        public static string GetText(string key, bool rightToLeft, params object[] args) {
            return LocalizationManagerInstance.GetText(key, rightToLeft, args);
        }
    }
}
