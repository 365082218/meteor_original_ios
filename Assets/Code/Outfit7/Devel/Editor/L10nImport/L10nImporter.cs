//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Outfit7.Text.Localization;
using Outfit7.Util;
using SimpleJSON;
using UnityEditor;
using UnityEngine;

namespace Outfit7.Devel.L10nImport {

    public static class L10nImport {

        private const string Tag = "L10nImport";

        public static Func<string> UrlStringCallback;

        public const string FilePath = "Assets/EditorResources/l10n.json.txt";
        private const string AssetsPath = "Assets/Resources/";
        private const string DefaultLanguage = LanguageCodes.English;

        [MenuItem("Outfit7/Localization/Download")]
        public static JSONClass DownloadJsonLocalizations() {
            Assert.NotNull(UrlStringCallback, "UrlStringCallback callback must be defined in the game - it must return the URL of the localization's Google Spreadhseet (published as CSV).");
            string Url = UrlStringCallback();

            O7Log.InfoT(Tag, "Downloading localization file from {0} to {1}", Url, FilePath);

            WWW www = new WWW(Url);
            while (!www.isDone) {
            }

            string error = www.error;
            if (!string.IsNullOrEmpty(error)) {
                O7Log.ErrorT(Tag, "Error downloading localization file from {0}: {1}", Url, error);
                return null;
            }

            byte[] dataB = www.bytes;

            O7Log.DebugT(Tag, "Converting data ({0} B)...", dataB.Length);

            string data = Encoding.UTF8.GetString(dataB);

            JSONClass exportData = new JSONClass();

            List<string> labelColumns = null;
            List<string> rows = StringUtils.QuoteDelimitedListToStringList(data, '"', '\n', true);
            for (int r = 0; r < rows.Count; r++) {
                List<string> columns = StringUtils.QuoteDelimitedListToStringList(rows[r], '"', ',', true);
                if (r == 0) {
                    // First "label" row
                    labelColumns = columns;
                    continue;
                }
                string key = columns[0];
                if (!StringUtils.HasText(key)) {
                    // Invalid key
                    O7Log.WarnT(Tag, "Key not found on row #{0}", r + 1);
                    continue;
                }

                JSONClass entry = new JSONClass();
                for (int i = 2; i < labelColumns.Count; i++) { // 0: key, 1: comment, 2: en...
                    if (!StringUtils.HasText(labelColumns[i])) continue; // Skip columns without label
                    if (i >= columns.Count) {
                        O7Log.WarnT(Tag, "More labels ({0}) than actual column texts ({1}) for key '{2}' on row #{3}; skipping the rest of the row",
                            labelColumns.Count, columns.Count, key, r + 1);
                        break;
                    }
                    if (!StringUtils.HasText(columns[i])) {
                        O7Log.WarnT(Tag, "Missing '{0}' value for key '{1}' on row #{2}", labelColumns[i], key, r + 1);
                        continue;
                    }
                    entry[labelColumns[i]] = StringUtils.UnQuote(columns[i]);
                }
                exportData[key] = entry;
            }

            O7Log.DebugT(Tag, "Writing to {0}...", FilePath);

            File.WriteAllText(FilePath, exportData.ToString(""));

            O7Log.InfoT(Tag, "Localization file downloaded from {0} to {1}", Url, FilePath);

            return exportData;
        }

        [MenuItem("Outfit7/Localization/Serialize")]
        public static void SerializeLocalJson() {
            JSONClass jsonLocalizations = JSON.Parse(File.ReadAllText(FilePath)) as JSONClass;
            if (jsonLocalizations == null) {
                O7Log.ErrorT(Tag, "Localization file ('{0}') was not found, did you want to download it first?", FilePath);
                return;
            }
            SerializeLocalizationAssets(jsonLocalizations);
        }

        public static void SerializeLocalizationAssets(JSONClass node) {
            O7Log.InfoT(Tag, "Started updating assets");
            int translationCount = node.Count;

            List<string> languageKeys = new List<string>();
            foreach (KeyValuePair<string, JSONNode> translations in node) {
                foreach (KeyValuePair<string, JSONNode> translation in translations.Value as JSONClass) {
                    languageKeys.Add(translation.Key);
                }
                break;
            }

            Dictionary<string, LocalizationAsset> localizationAssets = new Dictionary<string, LocalizationAsset>(languageKeys.Count + 1);
            for (int c = 0; c < languageKeys.Count + 1; c++) {
                string language;
                if (c < languageKeys.Count) {
                    language = languageKeys[c];
                } else {
                    language = LocalizationHelper.KeyLocalizationAssetName;
                }

                string path = AssetsPath + LocalizationHelper.GetLocalizationAssetPath(language) + ".asset";
                LocalizationAsset localizationAsset = AssetDatabase.LoadAssetAtPath<LocalizationAsset>(path);
                if (localizationAsset == null) {
                    localizationAsset = ScriptableObject.CreateInstance<LocalizationAsset>();
                    AssetDatabase.CreateAsset(localizationAsset, path);
                }
                localizationAsset.Values = new string[translationCount];
                localizationAssets[language] = localizationAsset;
            }

            int i = 0;
            foreach (KeyValuePair<string, JSONNode> translations in node) {
                localizationAssets[LocalizationHelper.KeyLocalizationAssetName].Values[i] = translations.Key;

                for (int t = 0; t < languageKeys.Count; t++) {
                    string translation = translations.Value[languageKeys[t]];
                    if (string.IsNullOrEmpty(translation)) {
                        translation = translations.Value[DefaultLanguage];
                    }
                    localizationAssets[languageKeys[t]].Values[i] = translation;
                }
                i++;
            }
            foreach (KeyValuePair<string, LocalizationAsset> asset in localizationAssets) {
                EditorUtility.SetDirty(asset.Value);
            }
            AssetDatabase.SaveAssets();
            O7Log.InfoT(Tag, "Localization assets updated");
        }

        [MenuItem("Outfit7/Localization/Download and Serialize")]
        public static void DownloadAndSerialize() {
            JSONClass jsonLocalizations = DownloadJsonLocalizations();
            SerializeLocalizationAssets(jsonLocalizations);
        }
    }
}
