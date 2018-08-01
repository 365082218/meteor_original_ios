using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Outfit7.Text.Localization;
using Outfit7.Util;

namespace Outfit7.UI {
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Localizer), true)]
    public class LocalizerEditor : UnityEditor.Editor {
        public static List<string> AllLocalizationKeys { get; private set; }

        private static LocalizationAsset KeysAsset = null;
        private static Dictionary<string, LocalizationAsset> LocalizationAssets = new Dictionary<string, LocalizationAsset>();
        private SerializedProperty TextComponent = null;

        private void OnEnable() {
            TextComponent = serializedObject.FindProperty("Text");
            LoadLocalizationAssets();
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            Localizer ThisTarget = (Localizer) target;

            EditorGUILayout.PropertyField(TextComponent);

            GUILayout.Space(6f);
            EditorGUIUtility.labelWidth = 80f;

            ThisTarget.Dynamic = GUILayout.Toggle(ThisTarget.Dynamic, " Dynamic (Enable if localization is set from code)");

            GUILayout.Space(6f);

            GUILayout.BeginHorizontal();

            SerializedProperty sp = DrawProperty("Key", serializedObject, "Key");

            string myKey = sp.stringValue;
            Dictionary<string, string> localizations = GetLocalizations(myKey);

            bool ok = localizations != null && localizations.Count != 0;

            GUI.color = ok ? Color.green : Color.red;
            GUILayout.BeginVertical(GUILayout.Width(22f));
            GUILayout.Space(2f);

            GUILayout.Label(ok ? "\u2714" : "\u2718", "TL SelectionButtonNew", GUILayout.Height(20f));

            GUILayout.EndVertical();
            GUI.color = Color.white;
            GUILayout.EndHorizontal();

            if (ok) {
                if (DrawHeader("Preview")) {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(4f);
                    EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
                    GUILayout.BeginVertical();
                    GUILayout.Space(2f);

                    string previousLanguage = string.Empty;
                    foreach (string language in LanguageCodes.All) {
                        string value;
                        if (!localizations.TryGetValue(language, out value)) {
                            continue;
                        }

                        try {
                            if (!ThisTarget.Dynamic) {
                                value = LocalizationHelper.FormatText(value, language, true);
                            }
                        } catch {
                            Debug.LogError("Check Dynamic checkbox");
                        }

                        if (!string.IsNullOrEmpty(previousLanguage) && language.Equals(previousLanguage)) {
                            // fix for 3x zh
                            continue;
                        }

                        previousLanguage = language;

                        GUILayout.BeginHorizontal();
                        GUILayout.Label(language, GUILayout.Width(70f));
                        if (string.IsNullOrEmpty(value)) {
                            GUI.color = new Color(1f, 0.2f, 0.2f, 0.9f);
                            GUI.contentColor = new Color(1f, 0.3f, 0.3f, 1f);
                            if (GUILayout.Button("NOT LOCALIZED!", "AS TextArea", GUILayout.MinWidth(80f), GUILayout.MaxWidth(Screen.width - 110f))) {
                                ThisTarget.LocalizeEditor(value);
                                GUIUtility.hotControl = 0;
                                GUIUtility.keyboardControl = 0;
                            }
                            GUI.contentColor = new Color(1f, 1f, 1f, 1f);
                            GUI.color = new Color(1f, 1f, 1f, 1f);
                        } else {
                            if (GUILayout.Button(value, "AS TextArea", GUILayout.MinWidth(80f), GUILayout.MaxWidth(Screen.width - 110f))) {
                                ThisTarget.LocalizeEditor(value);
                                GUIUtility.hotControl = 0;
                                GUIUtility.keyboardControl = 0;
                            }
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.Space(2f);
                    }

                    GUILayout.Space(3f);
                    GUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(3f);
                    GUILayout.EndHorizontal();
                    GUILayout.Space(3f);
                }
            }
            if (!string.IsNullOrEmpty(myKey)) {
                GUILayout.BeginHorizontal();
                GUILayout.Space(80f);
                GUILayout.BeginVertical();
                GUI.backgroundColor = new Color(1f, 1f, 1f, 0.35f);

                int matches = 0;

                for (int i = 0; i < KeysAsset.Values.Length; ++i) {
                    string key = KeysAsset.Values[i];
                    if (myKey == key) {
                        continue;
                    }

                    if (key.StartsWith(myKey, System.StringComparison.OrdinalIgnoreCase) || key.Contains(myKey)) {
                        if (GUILayout.Button(key + " \u25B2", "CN CountBadge")) {
                            sp.stringValue = key;
                            GUIUtility.hotControl = 0;
                            GUIUtility.keyboardControl = 0;
                        }

                        if (++matches == 8) {
                            GUILayout.Label("...and more");
                            break;
                        }
                    }
                }

                GUI.backgroundColor = Color.white;
                GUILayout.EndVertical();
                GUILayout.Space(22f);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            bool currentCaps = ThisTarget.AllCaps;
            bool currentAsianBoldText = ThisTarget.AsianBold;
            ThisTarget.AllCaps = GUILayout.Toggle(ThisTarget.AllCaps, " AllCaps");
            ThisTarget.AsianBold = GUILayout.Toggle(ThisTarget.AsianBold, " AsianBold");

            if (currentCaps != ThisTarget.AllCaps || currentAsianBoldText != ThisTarget.AsianBold) {
                ThisTarget.LocalizeStatic();
            }

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        private SerializedProperty DrawProperty(string label, SerializedObject serializedObject, string property) {
            SerializedProperty sp = serializedObject.FindProperty(property);

            if (sp != null) {
                EditorGUILayout.BeginHorizontal();

                if (label != null)
                    EditorGUILayout.PropertyField(sp, new GUIContent(label));
                else
                    EditorGUILayout.PropertyField(sp);

                GUILayout.Space(18f);
                EditorGUILayout.EndHorizontal();
            }
            return sp;
        }

        private bool DrawHeader(string text) {
            bool state = EditorPrefs.GetBool(text, true);

            GUILayout.Space(3f);
            if (!state)
                GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
            GUILayout.BeginHorizontal();
            GUILayout.Space(3f);

            GUI.changed = false;
            text = "<b><size=11>" + text + "</size></b>";
            if (state) {
                text = "\u25B2 " + text;
            } else {
                text = "\u25BC " + text;
            }

            if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) {
                state = !state;
            }

            if (GUI.changed)
                EditorPrefs.SetBool(text, state);

            GUILayout.Space(2f);
            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
            if (!state) {
                GUILayout.Space(3f);
            }

            return state;
        }

        public static void LoadLocalizationAssets() {
            if (LocalizationAssets == null) {
                LocalizationAssets.Clear();
            }
            if (AllLocalizationKeys == null) {
                AllLocalizationKeys = new List<string>();
            } else {
                AllLocalizationKeys.Clear();
            }
            KeysAsset = Resources.Load<LocalizationAsset>(LocalizationHelper.GetLocalizationAssetPath(LocalizationHelper.KeyLocalizationAssetName));
            foreach (string language in LanguageCodes.All) {
                LocalizationAssets[language] = Resources.Load<LocalizationAsset>(LocalizationHelper.GetLocalizationAssetPath(language));
                AllLocalizationKeys.Add(language);
            }
        }

        public static Dictionary<string, string> GetLocalizations(string localizationKey) {
            if (KeysAsset == null) {
                LoadLocalizationAssets();
            }
            Dictionary<string, string> localizations = new Dictionary<string, string>();
            int localizationIndex = ArrayUtility.FindIndex(KeysAsset.Values, l => l == localizationKey);
            if (localizationIndex == -1) return localizations;

            foreach (KeyValuePair<string, LocalizationAsset> localizationAsset in LocalizationAssets) {
                localizations.Add(localizationAsset.Key, localizationAsset.Value != null ? localizationAsset.Value.Values[localizationIndex] : "");
            }
            return localizations;
        }
    }
}
