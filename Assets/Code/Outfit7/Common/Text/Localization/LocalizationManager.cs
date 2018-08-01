//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System.Collections.Generic;
using Outfit7.Util;
using SimpleJSON;

namespace Outfit7.Text.Localization {

    /// <summary>
    /// Localization text manager.
    /// </summary>
    public class LocalizationManager {

        private const string Tag = "LocalizationManager";
        private const string OverriddenLanguagePref = "LocalizationManager.OverriddenLanguage";

        private Dictionary<string, string> KeyTranslations;
        private string KeyTranslationsLanguage;

        public string DefaultLanguage { get; set; }

        //public string Language { get; private set; }
        public string Language { get { return LanguageCodes.Chinese; } private set { } }

        public string OverriddenLanguage { get; private set; }

        public bool IsEasternLanguage {
            get {
                return LanguageCodes.IsEasternLanguage(Language);
            }
        }

        public bool IsHanLanguage {
            get {
                return LanguageCodes.IsHanLanguage(Language);
            }
        }

        public bool IsLanguage(string langCode) {
            return Language == langCode;
        }

        public void Init() {
            DefaultLanguage = LanguageCodes.English;
            Language = DefaultLanguage;
            OverriddenLanguage = UserPrefs.GetString(OverriddenLanguagePref, null);

            O7Log.DebugT(Tag, "Inited; OverriddenLanguage={0}", OverriddenLanguage);
        }

        public void ChangeLanguage(string langCode, bool loadLocalizations = false) {
            O7Log.VerboseT(Tag, "ChangeLanguage({0})", langCode);

            string lang = LanguageCodes.Parse(langCode);

            if (Language != lang) {
                Language = lang;

                O7Log.DebugT(Tag, "Language changed to: {0}", Language);
            }

            if (loadLocalizations && KeyTranslationsLanguage != lang) {
                KeyTranslations = LocalizationHelper.LoadLocalizationAssets(Language);
                KeyTranslationsLanguage = Language;
            }
        }

        public void ChangeOverriddenLanguage(string langCode, bool loadLocalizations = true) {
            O7Log.VerboseT(Tag, "ChangeOverriddenLanguage({0})", langCode);

            // langCode can be null to remove OverriddenLanguage - Language will fallback to English

            ChangeLanguage(langCode, loadLocalizations);

            if (OverriddenLanguage == langCode) return;

            OverriddenLanguage = langCode;
            UserPrefs.SetString(OverriddenLanguagePref, OverriddenLanguage);
            UserPrefs.SaveDelayed();
        }

        public void ChangeToNextOverriddenLanguage(bool loadLocalizations = true) {
            ChangeToNextOverriddenLanguage(LanguageCodes.All, loadLocalizations);
        }

        public void ChangeToNextOverriddenLanguage(List<string> langs, bool loadLocalizations = true) {
            int idx = langs.IndexOf(Language);
            idx++;
            string lang = langs[idx % langs.Count];
            ChangeOverriddenLanguage(lang, loadLocalizations);
        }

        public string GetText(string key) {
            return GetText(key, true, null);
        }

        public string GetText(string key, bool rightToLeftEnabled, params object[] args) {
            string text;
            if (!KeyTranslations.TryGetValue(key, out text)) {
                O7Log.WarnT(Tag, "Translation for key '{0}' not found", key);
                UnityEngine.Debug.LogError(" the key: "+key+" of Localizations is no value");
                return key;
            }

            return LocalizationHelper.FormatText(text, Language, rightToLeftEnabled, args);
        }

        public string GetText(JSONNode langMap) {
            return GetText(langMap, true, null);
        }

        public string GetText(JSONNode langMap, bool rightToLeftEnabled, params object[] args) {
            return LocalizationHelper.GetText(langMap, Language, DefaultLanguage, rightToLeftEnabled, args);
        }
    }
}
