using UnityEditor;
using UnityEngine;

namespace Starlite.Raven {

    public static class RavenPreferences {
        private const string c_QuickSearchShowAllProperties = "Outfit7_Raven_QuickSearch_ShowAllProperties";
        private const string c_QuickSearchRemapAllProperties = "Outfit7_Raven_QuickSearch_RemapAllProperties";
        private const string c_TimelineShowFunctionParameters = "Outfit7_Raven_Timeline_ShowFunctionParameters";
        private const string c_TimelineShowFunctionParametersCount = "Outfit7_Raven_Timeline_ShowFunctionParametersCount";
        private const string c_TimelineNewEventDuration = "Outfit7_Raven_Timeline_NewEventDuration";
        private const string c_ShowSelectedEventRect = "Outfit7_Raven_ShowEventSelectedRect";
        private const string c_QuickSearchMaximumSize = "Outfit7_Raven_QuickSearch_MaximumSize";
        private const string c_QuickSearchMaximumElements = "Outfit7_Raven_QuickSearch_MaximumElements";
        private const string c_ValidatePropertiesKey = "Outfit7_Raven_ValidateProperties";
        private const string c_DumpFunctionInfoKey = "Outfit7_Raven_DumpFunctionInfo";
        private const string c_ExclusionFilterKey = "Outfit7_Raven_ExclusionFilter";
        private const string c_SkinFileKey = "Outfit7_Raven_Skin";

        private static bool s_ValidateProperties = EditorPrefs.GetBool(c_ValidatePropertiesKey, true);

        private static bool s_QuickSearchShowAllProperties = EditorPrefs.GetBool(c_QuickSearchShowAllProperties, true);
        private static bool s_QuickSearchRemapAllProperties = EditorPrefs.GetBool(c_QuickSearchRemapAllProperties, true);
        private static bool s_TimelineShowFunctionParameters = EditorPrefs.GetBool(c_TimelineShowFunctionParameters, true);
        private static int s_TimelineShowFunctionParametersCount = EditorPrefs.GetInt(c_TimelineShowFunctionParametersCount, 3);
        private static float s_TimelineNewEventDuration = EditorPrefs.GetFloat(c_TimelineNewEventDuration, 1f);
        private static bool s_ShowSelectedEventRect = EditorPrefs.GetBool(c_ShowSelectedEventRect, true);
        private static int s_QuickSearchMaximumSize = EditorPrefs.GetInt(c_QuickSearchMaximumSize, 0);
        private static int s_QuickSearchMaximumElements = EditorPrefs.GetInt(c_QuickSearchMaximumElements, 5);
        private static bool s_DumpFunctionInfo = EditorPrefs.GetBool(c_DumpFunctionInfoKey, false);
        private static string s_ExclusionFilter = EditorPrefs.GetString(c_ExclusionFilterKey, "External/;Plugins/");
        private static readonly string s_SkinFile = EditorPrefs.GetString(c_SkinFileKey, "");

        private static bool s_FoldoutQuickSearch = false;
        private static bool s_FoldoutTimeline = false;
        private static bool s_FoldoutAdvancedOptions = false;

        private static GUISkin s_CachedSkinFileObject = null;
        private static bool s_SearchedForSkin = false;

        private static readonly GUIContent s_ExclusionFilterGUIContent = new GUIContent("Exclusion Filter", "Separate entries with ;. It will ignore scenes/prefabs if they have any of the strings in their path/name. Ignores case!");
        private static readonly GUIContent s_QuickSearchShowAllPropertiesGUIContent = new GUIContent("Show All Properties in Quick Search", "If checked, quick search will show all available properties.");
        private static readonly GUIContent s_QuickSearchRemapAllPropertiesGUIContent = new GUIContent("Remap All Properties in Quick Search", "If checked, quick search will remap names to user defined names even when showing all properties.");
        private static readonly GUIContent s_TimelineShowFunctionParametersGUIContent = new GUIContent("Show Function Parameters in Timeline", "If checked, function events will show up to x user defined parameters.");
        private static readonly GUIContent s_TimelineShowFunctionParametersCountGUIContent = new GUIContent("Number of Function Parameters");
        private static readonly GUIContent s_TimelineNewEventDurationGUIContent = new GUIContent("Default duration for newly created events.");
        private static readonly GUIContent s_ShowSelectedEventRectGUIContent = new GUIContent("Show Event Selected Rectangle");
        private static readonly GUIContent s_QuickSearchMaximumSizeGUIContent = new GUIContent("Quick Search Maximum Size", "Maximum horizontal size for quick search bar. 0 for infinite.");
        private static readonly GUIContent s_QuickSearchMaximumElementsGUIContent = new GUIContent("Quick Search Maximum Elements", "Maximum vertical elements displayed.");

        public static GUISkin Skin {
            get {
                return s_CachedSkinFileObject ?? (s_CachedSkinFileObject = FindSkin());
            }
        }

        public static bool QuickSearchShowAllProperties {
            get {
                return s_QuickSearchShowAllProperties;
            }
        }

        public static bool QuickSearchRemapAllProperties {
            get {
                return s_QuickSearchRemapAllProperties;
            }
        }

        public static int QuickSearchMaximumSize {
            get {
                return s_QuickSearchMaximumSize;
            }
        }

        public static int QuickSearchMaximumElements {
            get {
                return s_QuickSearchMaximumElements;
            }
        }

        public static bool TimelineShowFunctionParameters {
            get {
                return s_TimelineShowFunctionParameters;
            }
        }

        public static int TimelineShowFunctionParametersCount {
            get {
                return s_TimelineShowFunctionParametersCount;
            }
        }

        public static float TimelineNewEventDuration {
            get {
                return s_TimelineNewEventDuration;
            }
        }

        public static bool ShowSelectedEventRect {
            get {
                return s_ShowSelectedEventRect;
            }
        }

        public static bool ValidateProperties {
            get {
                return s_ValidateProperties;
            }
        }

        public static bool DumpFunctionInfo {
            get {
                return s_DumpFunctionInfo;
            }
        }

        /// <summary>
        /// Returns an array of strings in lower case.
        /// </summary>
        public static string[] GetExclusionFilters() {
            return s_ExclusionFilter.ToLowerInvariant().Split(';');
        }

        private static void DrawSkinFileGUI() {
            if (s_CachedSkinFileObject == null) {
                s_CachedSkinFileObject = FindSkin();
            }
            s_CachedSkinFileObject = EditorGUILayout.ObjectField("Skin", s_CachedSkinFileObject, typeof(GUISkin), false) as GUISkin;
        }

        private static GUISkin FindSkin() {
            if (!string.IsNullOrEmpty(s_SkinFile)) {
                var skin = AssetDatabase.LoadAssetAtPath<GUISkin>(s_SkinFile);
                if (skin != null) {
                    return skin;
                }
            }

            if (!s_SearchedForSkin) {
                s_SearchedForSkin = true;
                var skinGuids = AssetDatabase.FindAssets("RavenDefaultSkin t:guiskin");
                if (skinGuids.Length > 0) {
                    return AssetDatabase.LoadAssetAtPath<GUISkin>(AssetDatabase.GUIDToAssetPath(skinGuids[0]));
                }
            }
            return null;
        }

        [PreferenceItem("Raven")]
        private static void OnPreferences() {
            EditorGUILayout.LabelField("Raven");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("UI", EditorStyles.boldLabel);
            s_FoldoutQuickSearch = EditorGUILayout.Foldout(s_FoldoutQuickSearch, "Quick Search");
            if (s_FoldoutQuickSearch) {
                ++EditorGUI.indentLevel;
                s_QuickSearchShowAllProperties = EditorGUILayout.Toggle(s_QuickSearchShowAllPropertiesGUIContent, s_QuickSearchShowAllProperties);
                if (s_QuickSearchShowAllProperties) {
                    ++EditorGUI.indentLevel;
                    s_QuickSearchRemapAllProperties = EditorGUILayout.Toggle(s_QuickSearchRemapAllPropertiesGUIContent, s_QuickSearchRemapAllProperties);
                    --EditorGUI.indentLevel;
                }
                s_QuickSearchMaximumSize = EditorGUILayout.IntSlider(s_QuickSearchMaximumSizeGUIContent, s_QuickSearchMaximumSize, 0, 1000);
                s_QuickSearchMaximumElements = EditorGUILayout.IntSlider(s_QuickSearchMaximumElementsGUIContent, s_QuickSearchMaximumElements, 1, 10);
                --EditorGUI.indentLevel;
            }

            s_FoldoutTimeline = EditorGUILayout.Foldout(s_FoldoutTimeline, "Timeline");
            if (s_FoldoutTimeline) {
                ++EditorGUI.indentLevel;
                s_TimelineShowFunctionParameters = EditorGUILayout.Toggle(s_TimelineShowFunctionParametersGUIContent, s_TimelineShowFunctionParameters);
                if (s_TimelineShowFunctionParameters) {
                    ++EditorGUI.indentLevel;
                    s_TimelineShowFunctionParametersCount = EditorGUILayout.IntSlider(s_TimelineShowFunctionParametersCountGUIContent, s_TimelineShowFunctionParametersCount, 1, 10);
                    --EditorGUI.indentLevel;
                }
                s_TimelineNewEventDuration = EditorGUILayout.Slider(s_TimelineNewEventDurationGUIContent, s_TimelineNewEventDuration, 0.1f, 2f);
                s_ShowSelectedEventRect = EditorGUILayout.Toggle(s_ShowSelectedEventRectGUIContent, s_ShowSelectedEventRect);
                --EditorGUI.indentLevel;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Compiler", EditorStyles.boldLabel);
            s_ValidateProperties = EditorGUILayout.Toggle("Validate Properties", s_ValidateProperties);
            s_DumpFunctionInfo = EditorGUILayout.Toggle("Dump Function Info", s_DumpFunctionInfo);
            DrawSkinFileGUI();

            s_FoldoutAdvancedOptions = EditorGUILayout.Foldout(s_FoldoutAdvancedOptions, "Advanced Settings");
            if (s_FoldoutAdvancedOptions) {
                s_ExclusionFilter = EditorGUILayout.TextField(s_ExclusionFilterGUIContent, s_ExclusionFilter);
            }

            if (GUI.changed) {
                EditorPrefs.SetBool(c_QuickSearchShowAllProperties, s_QuickSearchShowAllProperties);
                EditorPrefs.SetBool(c_QuickSearchRemapAllProperties, s_QuickSearchRemapAllProperties);
                EditorPrefs.SetInt(c_QuickSearchMaximumSize, s_QuickSearchMaximumSize);
                EditorPrefs.SetInt(c_QuickSearchMaximumElements, s_QuickSearchMaximumElements);
                EditorPrefs.SetBool(c_TimelineShowFunctionParameters, s_TimelineShowFunctionParameters);
                EditorPrefs.SetInt(c_TimelineShowFunctionParametersCount, s_TimelineShowFunctionParametersCount);
                EditorPrefs.SetBool(c_ShowSelectedEventRect, s_ShowSelectedEventRect);
                EditorPrefs.SetBool(c_ValidatePropertiesKey, s_ValidateProperties);
                EditorPrefs.SetBool(c_DumpFunctionInfoKey, s_DumpFunctionInfo);
                EditorPrefs.SetString(c_ExclusionFilterKey, s_ExclusionFilter);
                EditorPrefs.SetString(c_SkinFileKey, AssetDatabase.GetAssetPath(s_CachedSkinFileObject));
            }
        }
    }
}