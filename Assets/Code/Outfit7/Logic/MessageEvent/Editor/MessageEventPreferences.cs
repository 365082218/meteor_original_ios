using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;

namespace Outfit7.Logic {

    public static class MessageEventPreferences {

        private class MessageEventEntry {
            private string FullNameInternal = string.Empty;
            public string FullName {
                get {
                    return FullNameInternal;
                }
                set {
                    FullNameInternal = value;
                    if (FullName.Contains("/")) {
                        string[] splits = FullName.Split('/');
                        if (splits.Length >= 2) {
                            bool allgood = true;
                            for (int a = 0; a < splits.Length; a++) {
                                if (splits[a].Length <= 0) {
                                    allgood = false;
                                }
                            }
                            if (allgood) {
                                Namespace = splits[0];
                                Name = FullName.Substring(Namespace.Length + 1, FullName.Length - (Namespace.Length + 1));
                                Hash = FullName.GetHashCode();
                            }
                        }

                    }
                }
            }

            public string Namespace = string.Empty;
            public string Name = string.Empty;
            public int Hash = 0;
            public string FilePath = string.Empty;

            public MessageEventEntry() {
            }

            public MessageEventEntry(string fullName) {
                FullName = fullName;
            }

            public void Validate() {
            }

            public bool IsValid() {
                return Namespace.Length > 0 && Name.Length > 0;
            }
        }

        private static string[] messageEventNames = null;
        private static List<MessageEventEntry> MessageEventEntries = new List<MessageEventEntry>();
        private static Vector2 ScrollViewPosition = Vector2.zero;
        private static List<MessageEventEntry> ToDelete = new List<MessageEventEntry>();

        private static ReorderableList List = null;

        private static GUIStyle StyleError = null;
        private static GUIStyle StyleOk = null;

        public static string[] MessageEventNames {
            get {
                // If not read yet
                if (messageEventNames == null) {
                    LoadMessageEventSettings();
                }
                return messageEventNames;
            }
        }

        public static void Refresh() {
            messageEventNames = null;
            LoadMessageEventSettings();
        }

        private static void FillMessageEventNames() {
            messageEventNames = new string[MessageEventEntries.Count + 1];
            messageEventNames[0] = "None";
            for (int i = 0; i < MessageEventEntries.Count; ++i) {
                messageEventNames[i + 1] = MessageEventEntries[i].FullName;
            }
        }

        private static void LoadMessageEventSettings() {
            if (messageEventNames != null) {
                return;
            }

            MessageEventEntries.Clear();
            string[] guids = AssetDatabase.FindAssets("MessageEvents");
            foreach (string guid in guids) {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string filename = Path.GetFileName(assetPath);

                string namesp = string.Empty;
                string fn = string.Empty;

                if (filename.StartsWith("MessageEvents") && filename.EndsWith(".cs")) {
                    string[] lines = File.ReadAllLines(assetPath);
                    for (int a = 0; a < lines.Length; a++) {
                        string line = lines[a];
                        if (line.Contains("public class")) {
                            fn = assetPath;
                            string s = line;
                            s = s.Replace("public class", "");
                            s = s.Replace("{", "");
                            s = s.Trim();
                            namesp = s.Trim();
                        }
                        if (!string.IsNullOrEmpty(namesp)) {
                            string[] splits = line.Split('=');
                            if (splits.Length == 2) {
                                string en = splits[0].Trim();
                                en = en.Replace("public const int", "");
                                en = en.Trim();
                                //string val = splits[1].Trim();
                                MessageEventEntry entry = new MessageEventEntry();
                                MessageEventEntries.Add(entry);
                                entry.FullName = namesp + "/" + en;
                                entry.FilePath = fn;
                            }
                        }
                    }
                }
            }

            MessageEventEntries = MessageEventEntries.OrderBy(x => x.Namespace).ThenBy(x => x.Name).ToList();

            FillMessageEventNames();
        }

        private static void SaveMessageEventSettings() {
            List<string> files1 = new List<string>();
            foreach (MessageEventEntry mee in MessageEventEntries) {
                if (!files1.Contains(mee.FilePath)) {
                    files1.Add(mee.FilePath);
                }
            }

            List<string> files2 = new List<string>();
            foreach (MessageEventEntry mee in ToDelete) {
                if (!files2.Contains(mee.FilePath)) {
                    files2.Add(mee.FilePath);
                }
            }

            foreach (string f in files2) {
                if (!files1.Contains(f)) {
                    AssetDatabase.DeleteAsset(f);
                }
            }


            // merge into groups
            Dictionary<string, List<MessageEventEntry>> groupedEntries = new Dictionary<string, List<MessageEventEntry>>();
            for (int a = 0; a < MessageEventEntries.Count; a++) {
                MessageEventEntry entry = MessageEventEntries[a];
                if (entry.IsValid()) {
                    if (!groupedEntries.ContainsKey(entry.Namespace)) {
                        groupedEntries.Add(entry.Namespace, new List<MessageEventEntry>());
                    }
                    List<MessageEventEntry> entries = groupedEntries[entry.Namespace];
                    entries.Add(entry);
                }
            }

            foreach (KeyValuePair<string, List<MessageEventEntry>> pair in groupedEntries) {
                MessageEventEntry masterEntry = pair.Value[0];
                string filepath = masterEntry.FilePath;
                if (string.IsNullOrEmpty(filepath)) {
                    filepath = "Assets/MessageEvents" + masterEntry.Namespace + ".cs";
                }
                StreamWriter sw = File.CreateText(filepath);
                sw.WriteLine("// AUTOGENERATED FILE, DO NOT MODIFY, M'KAY?\n\n");
                sw.WriteLine("namespace Outfit7.MessageEvents {");
                sw.WriteLine("    public class " + masterEntry.Namespace + " {");
                for (int a = 0; a < pair.Value.Count; a++) {
                    MessageEventEntry e = pair.Value[a];
                    e.FilePath = filepath;
                    sw.WriteLine("        public const int " + e.Name + " = " + e.Hash + ";");
                }

                sw.WriteLine("");

                sw.WriteLine("#if DEVEL_BUILD || PROD_BUILD || UNITY_EDITOR");
                sw.WriteLine("        [UnityEngine.RuntimeInitializeOnLoadMethod]");
                sw.WriteLine("        static void PopulateInfo() {");
                for (int a = 0; a < pair.Value.Count; a++) {
                    MessageEventEntry e = pair.Value[a];
                    string s = string.Format("            Outfit7.Logic.MessageEventManager.MessageEventsInfo.Add({0}, \"{1}.{2}\");", e.Hash, masterEntry.Namespace, e.Name);
                    sw.WriteLine(s);
                }
                sw.WriteLine("        }");
                sw.WriteLine("#endif");

                sw.WriteLine("    }");
                sw.WriteLine("}");
                sw.Close();
            }

            AssetDatabase.Refresh();
        }

        private static ReorderableList DisplayList() {

            if (StyleError == null) {
                StyleError = new GUIStyle(GUI.skin.label);
                StyleError.normal.textColor = Color.red;
            }
            if (StyleOk == null) {
                StyleOk = new GUIStyle(GUI.skin.label);
            }

            if (List != null) {
                return List;
            }
            List = new ReorderableList(MessageEventEntries, typeof(MessageEventEntry), true, true, true, true);
            List.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                if (index >= MessageEventEntries.Count) {
                    return;
                }
                rect.y += 2;
                MessageEventEntry entry = MessageEventEntries[index];
                GUIStyle gs = entry.IsValid() ? StyleOk : StyleError;
                entry.FullName = GUI.TextField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), entry.FullName, gs);
            };
            List.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, string.Format("Message Events"));
            };
            List.onAddCallback = (ReorderableList list) => {
                list.list.Add(new MessageEventEntry());
            };
            List.onChangedCallback = (ReorderableList list) => {
            };
            List.onRemoveCallback = (ReorderableList list) => {
                ToDelete.Add(MessageEventEntries[list.index]);
                MessageEventEntries.RemoveAt(list.index);
            };
            return List;
        }

        [PreferenceItem("Message Events")]
        private static void EnginePreferencesItem() {
            LoadMessageEventSettings();
            ScrollViewPosition = EditorGUILayout.BeginScrollView(ScrollViewPosition);
            EditorGUILayout.BeginVertical();
            EditorGUI.BeginChangeCheck();
            DisplayList().DoLayoutList();

            if (EditorGUI.EndChangeCheck()) {
                FillMessageEventNames();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh")) {
                Refresh();
            }
            if (GUILayout.Button("Apply")) {
                SaveMessageEventSettings();
            }
            EditorGUILayout.EndHorizontal();
        }
    }

}