using UnityEngine;
using UnityEditor;
using SimpleJSON;
using System.Collections.Generic;
using System.IO;

namespace Outfit7.Devel.SceneLocking {
    public class SceneLockingManager : EditorWindow {
        private List<LockingData> OriginalLockingData = null;
        private List<LockingData> LockingData = null;
        private Vector2 ScrollPos;
        private bool SceneToggle = true;
        private bool PrefabToggle = true;
        private bool OnlyMineToggle = false;
        private string SearchField = string.Empty;

        // Add menu named "My Window" to the Window menu
        [MenuItem("Outfit7/Scene Locking/Manager %l")]
        private static void Init() {
            // Get existing open window or if none, make a new one:
            SceneLockingManager window = (SceneLockingManager) EditorWindow.GetWindow(typeof(SceneLockingManager));
            window.Show();
        }

        private void OnEnable() {
            Refresh();
        }

        private List<LockingData> GetLockingData() {
            string[] scenePaths = SceneLocking.GetAllScenePaths();
            string[] prefabGUIDs = SceneLocking.GetAllPrefabGUIDs();
            List<LockingData> lockingData = new List<LockingData>(scenePaths.Length + prefabGUIDs.Length);

            JSONArray sceneDataArrayJ = new JSONArray();
            JSONArray prefabDataArrayJ = new JSONArray();

            for (int i = 0; i < scenePaths.Length; i++) {
                string sceneName = Path.GetFileName(scenePaths[i]);

                JSONClass sceneDataJ = new JSONClass();
                sceneDataJ["scene_name"] = sceneName;
                sceneDataArrayJ.Add(sceneDataJ);

                //JSONNode sceneJson = SceneLocking.GetSceneInfo(sceneName);
                //if (sceneJson == null) break;
                //lockingData.Add(new SceneLockingData(sceneJson["user_name"].Value, sceneJson["lock_comment"].Value, sceneJson["locked"].AsBool, scenePaths[i], sceneName));
            }

            for (int i = 0; i < prefabGUIDs.Length; i++) {
                JSONClass prefabDataJ = new JSONClass();
                prefabDataJ["prefab_name"] = Path.GetFileName(AssetDatabase.GUIDToAssetPath(prefabGUIDs[i]));
                prefabDataJ["prefab_guid"] = prefabGUIDs[i];
                prefabDataArrayJ.Add(prefabDataJ);
            }

            JSONNode sceneArrayJ = SceneLocking.GetSceneInfoMulti(sceneDataArrayJ);
            JSONNode prefabArrayJ = SceneLocking.GetPrefabInfo(prefabDataArrayJ);
            if (sceneArrayJ != null) {
                for (int i = 0; i < sceneArrayJ.Count; i++) {
                    JSONNode sJ = sceneArrayJ[i];
                    lockingData.Add(new SceneLockingData(sJ["user_name"].Value, sJ["lock_comment"].Value, sJ["locked"].AsBool, scenePaths[i], sJ["scene_name"].Value));
                }
            }
            if (prefabArrayJ != null) {
                for (int i = 0; i < prefabArrayJ.Count; i++) {
                    JSONNode pJ = prefabArrayJ[i];
                    string guid = pJ["prefab_guid"];
                    lockingData.Add(new PrefabLockingData(pJ["user_name"].Value, pJ["lock_comment"].Value, pJ["locked"].AsBool, Path.GetFileName(AssetDatabase.GUIDToAssetPath(guid)), guid));
                }
            }

            return lockingData;
        }

        private void Refresh(bool fetchData = true) {
            if (fetchData) {
                OriginalLockingData = GetLockingData();
            }
            FilterList();
        }

        private Texture2D MakeTex(int width, int height, Color col) {
            Color[] pix = new Color[width * height];

            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();

            return result;
        }

        private void FilterList() {
            LockingData = new List<LockingData>(OriginalLockingData);
            List<LockingData> lockingLockedData = new List<LockingData>();
            string currentUser = SceneLocking.GetUserName();
            for (int i = 0; i < LockingData.Count; i++) {
                LockingData ld = LockingData[i];
                bool isSceneLockingData = ld is SceneLockingData;
                bool isPrefabLockingData = ld is PrefabLockingData;

                if (!(SceneToggle && isSceneLockingData || PrefabToggle && isPrefabLockingData)) {
                    LockingData.RemoveAt(i);
                    i--;
                    continue;
                }
                if (!string.IsNullOrEmpty(SearchField) && !ld.UserName.ToLower().Contains(SearchField.ToLower()) && !ld.GetName().ToLower().Contains(SearchField.ToLower())) {
                    LockingData.RemoveAt(i);
                    i--;
                    continue;
                }
                if (OnlyMineToggle && !(ld.Locked && ld.UserName == currentUser)) {
                    LockingData.RemoveAt(i);
                    i--;
                    continue;
                }
                if (ld.Locked) {
                    lockingLockedData.Add(LockingData[i]);
                    LockingData.RemoveAt(i);
                    i--;
                    continue;
                }
            }
            LockingData.InsertRange(0, lockingLockedData);
        }

        private void OnGUI() {
            GUILayout.Label("Scene Locking Manager", EditorStyles.boldLabel);
            if (GUILayout.Button("Refresh")) {
                Refresh();
            }

            if (LockingData == null) {
                return;
            }

            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            SceneToggle = EditorGUILayout.ToggleLeft("Scenes", SceneToggle, GUILayout.Width(55));
            PrefabToggle = EditorGUILayout.ToggleLeft("Prefabs", PrefabToggle, GUILayout.Width(55));
            OnlyMineToggle = EditorGUILayout.ToggleLeft("My locks", OnlyMineToggle, GUILayout.Width(65));
            EditorGUIUtility.labelWidth = 45;
            SearchField = EditorGUILayout.TextField("Search", SearchField);
            if (EditorGUI.EndChangeCheck()) {
                FilterList();
            }
            GUILayout.EndHorizontal();

            if (!SceneToggle && !PrefabToggle) {
                return;
            }

            GUIStyle style1 = new GUIStyle();
            style1.normal.background = MakeTex(1, 1, new Color(1.0f, 1.0f, 1.0f, 0.08f));
            GUIStyle style2 = new GUIStyle();
            style2.normal.background = MakeTex(1, 1, new Color(1.0f, 1.0f, 1.0f, 0.12f));

            ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos);

            int firstIndex = (int) (ScrollPos.y / EditorGUIUtility.singleLineHeight);
            int visibleCount = Mathf.CeilToInt((position.height - EditorGUIUtility.singleLineHeight * 4) / (EditorGUIUtility.singleLineHeight + 5f));
            firstIndex = Mathf.Clamp(firstIndex, 0, Mathf.Max(0, LockingData.Count - visibleCount));
            GUILayout.Space(firstIndex * EditorGUIUtility.singleLineHeight);

            bool repaint = false;

            for (int i = firstIndex; i < Mathf.Min(LockingData.Count, firstIndex + visibleCount); i++) {
                LockingData ld = LockingData[i];
                bool isSceneLockingData = ld is SceneLockingData;
                bool isPrefabLockingData = ld is PrefabLockingData;

                // Scene name label
                GUI.backgroundColor = ld.Locked ? Color.red : Color.green;
                GUILayout.BeginHorizontal(i % 2 == 0 ? style1 : style2);

                // Scene lock/unlock button
                GUI.backgroundColor = ld.Locked ? Color.red : Color.green;
                string buttonState = ld.Locked ? "Unlock" : "Lock";
                if (GUILayout.Button(buttonState, GUILayout.Width(75))) {
                    if (ld.Locked && ld.UserName != SceneLocking.GetUserName()) {
                        if (!EditorUtility.DisplayDialog(string.Format("{0} is already locked!", isSceneLockingData ? "Scene" : "Prefab"), string.Format("{0} {1} is locked by {2}", isSceneLockingData ? "Scene" : "Prefab", ld.GetName(), ld.UserName), "Steal it", "Cancel")) {
                            GUILayout.EndHorizontal();
                            continue;
                        }
                        buttonState = "Lock";
                        ld.LockComment = "";
                        int idx = OriginalLockingData.IndexOf(ld);
                        OriginalLockingData[idx].UserName = SceneLocking.GetUserName();
                        repaint = true;
                    }
                    JSONNode statusJson = null;
                    if (isSceneLockingData) {
                        statusJson = SceneLocking.SetSceneLock(ld.GetName(), ld.LockComment, buttonState == "Lock", showDialogInfo: false);
                    } else if (isPrefabLockingData) {
                        PrefabLockingData pld = ld as PrefabLockingData;
                        statusJson = SceneLocking.SetPrefabLock(ld.GetName(), pld.PrefabGUID, ld.LockComment, buttonState == "Lock", showDialogInfo: false);
                    }
                    if (statusJson == null) {
                        return;
                    }
                    if (statusJson["success"].AsBool) {
                        int idx = OriginalLockingData.IndexOf(ld);
                        OriginalLockingData[idx].Locked = statusJson["locked"].AsBool;
                        OriginalLockingData[idx].UserName = statusJson["user_name"].Value;
                        Refresh(false);
                        break;
                    } else {
                        EditorUtility.DisplayDialog("Scene Locking", "Something went wrong. Go figure...", "Ok");
                    }
                }

                GUIContent firstColumn = new GUIContent(ld.GetName(), ld.GetToolTip());

                GUILayout.Label(firstColumn, GUILayout.Width(200));

                GUI.backgroundColor = Color.white;
                string lockedByName = ld.Locked ? "Locked by: " + ld.UserName : "Last locked: " + ld.UserName;
                // Scene locked by name label
                GUILayout.Label(lockedByName, GUILayout.Width(200));

                GUI.enabled = !ld.Locked;
                // Scene comment box
                ld.LockComment = EditorGUILayout.TextField(ld.LockComment);
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }
            GUILayout.Space(Mathf.Max(0, (LockingData.Count - firstIndex - visibleCount) * EditorGUIUtility.singleLineHeight));
            EditorGUILayout.EndScrollView();
            if (repaint) {
                Repaint();
            }
        }
    }
}