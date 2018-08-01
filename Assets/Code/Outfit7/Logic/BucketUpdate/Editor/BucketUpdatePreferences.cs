using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SimpleJSON;

namespace Outfit7.Logic {

    public static class BucketUpdatePreferences {

        private const string PreferencesFilename = "ProjectSettings/BucketUpdateSettings.json";

        private static string[] bucketNames = null;
        private static List<string> validBucketNames = null;
        private static List<int> validBucketIndex = null;
        private static Vector2 ScrollViewPosition = Vector2.zero;

        public static string[] BucketNames {
            get {
                // If not read yet
                if (bucketNames == null) {
                    LoadBucketUpdateSettings();
                }
                return validBucketNames.ToArray();
            }
        }

        public static int GetValidBucketIndex(int index) {
            LoadBucketUpdateSettings();
            for (int i = 0; i < validBucketIndex.Count; ++i) {
                if (validBucketIndex[i] == index) {
                    return i;
                }
            }
            return 0;
        }

        public static int GetBucketIndex(int validIndex) {
            LoadBucketUpdateSettings();
            if (validIndex < 0 || validIndex >= validBucketIndex.Count) {
                validIndex = 0;
            }
            return validBucketIndex[validIndex];
        }

        private static void FillValidBucketNames() {
            validBucketNames.Clear();
            validBucketIndex.Clear();
            validBucketNames.Add("None");
            validBucketIndex.Add(-1);
            for (int i = 0; i < bucketNames.Length; ++i) {
                if (bucketNames[i].Length == 0) {
                    continue;
                }
                validBucketNames.Add(bucketNames[i]);
                validBucketIndex.Add(i);
            }
        }

        private static void LoadBucketUpdateSettings() {
            if (bucketNames != null) {
                return;
            }
            validBucketNames = new List<string>(BucketUpdateSystem.MaxBucketCount);
            validBucketIndex = new List<int>(BucketUpdateSystem.MaxBucketCount);
            // Fill defaults
            bucketNames = new string[BucketUpdateSystem.MaxBucketCount];
            bucketNames[0] = "Default";
            for (int i = 1; i < bucketNames.Length; ++i) {
                bucketNames[i] = string.Empty;
            }
            string json = string.Empty;
            if (File.Exists(PreferencesFilename)) {
                json = File.ReadAllText(PreferencesFilename);
                if (json.Length > 0) {
                    JSONArray node = JSON.Parse(json) as JSONArray;
                    if (node != null) {
                        for (int i = 0; i < node.Count; ++i) {
                            JSONClass bucketNode = node[i] as JSONClass;
                            string name = bucketNode["name"];
                            int index = bucketNode["index"].AsInt;
                            bucketNames[index] = name;
                        }
                    }
                }
            }
            FillValidBucketNames();
        }

        private static void SaveBucketUpdateSettings() {
            JSONArray node = new JSONArray();
            for (int i = 0; i < validBucketNames.Count; ++i) {
                if (validBucketIndex[i] < 0) {
                    continue;
                }
                JSONClass bucketNode = new JSONClass();
                bucketNode["name"] = validBucketNames[i];
                bucketNode["index"] = new JSONData(validBucketIndex[i]);
                node.Add(bucketNode);
            }
            File.WriteAllText(PreferencesFilename, node.ToString());
        }

        [PreferenceItem("Bucket Update")]
        private static void EnginePreferencesItem() {
            LoadBucketUpdateSettings();
            ScrollViewPosition = EditorGUILayout.BeginScrollView(ScrollViewPosition);
            EditorGUILayout.BeginVertical();
            bool hasChanged = false;
            for (int i = 0; i < bucketNames.Length; ++i) {
                string name = EditorGUILayout.TextField(string.Format("Bucket #{0}:", i), bucketNames[i]);
                if (name != bucketNames[i]) {
                    hasChanged = true;
                    if (i != 0 || name.Length != 0) {
                        bucketNames[i] = name;
                    }
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            // If changed save
            if (hasChanged) {
                FillValidBucketNames();
                SaveBucketUpdateSettings();
            }
        }
    }

}