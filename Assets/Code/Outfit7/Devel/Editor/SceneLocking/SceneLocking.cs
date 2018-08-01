
using System.Collections.Generic;
using System.IO;
using SimpleJSON;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using Outfit7.Util;

//负责GIT上场景的锁定API接口的调用，避免多个用户同时修改一个场景
namespace Outfit7.Devel.SceneLocking {
    public static class SceneLocking {

        private static string urlSceneInfo = "https://pipelineapi.outfit7.org/manager/unity-locking/scene/info/";
        private static string urlSceneInfoMulti = "https://pipelineapi.outfit7.org/manager/unity-locking/scene/info-multi/";
        private static string urlSceneSubmit = "https://pipelineapi.outfit7.org/manager/unity-locking/scene/";
        private static string urlSceneUnlockAll = "https://pipelineapi.outfit7.org/manager/unity-locking/scene/unlock-all/";

        private static string urlPrefabInfo = "https://pipelineapi.outfit7.org/manager/unity-locking/prefab/info/";
        private static string urlPrefabSubmit = "https://pipelineapi.outfit7.org/manager/unity-locking/prefab/";
        //private static string urlPrefabUnlockAll = "https://pipelineapi.outfit7.org/manager/unity-locking/prefab/unlock-all/";
        private static string urlPrefabInfoMulti = "https://pipelineapi.outfit7.org/manager/unity-locking/prefab/info-multi/";

        //private static string scenesPath = "/Assets/Scenes/";

        [MenuItem("Outfit7/Scene Locking/Lock Scene")]
        private static void LockScene() {
            SetSceneLock(true);
        }

        [MenuItem("Outfit7/Scene Locking/Unlock Scene")]
        private static void UnlockScene() {
            SetSceneLock(false);
        }

        [MenuItem("Outfit7/Scene Locking/Unlock All")]
        private static void UnlockAllScenes() {
            JSONNode statusJson = SetUnityUnlockAll(GetUserName());

            if (statusJson["success"].Value == "true") {
                EditorUtility.DisplayDialog("Scene Locking", "All scenes unlocked!", "Ok");
            } else {
                EditorUtility.DisplayDialog("Scene Locking", "Failed to unlock all scenes!", "Ok");
            }
        }

        //[MenuItem("Assets/Outfit7 Lock Prefab", false, 5000)]
        private static void LockPrefab() {
            GameObject[] selectedObjects = Selection.gameObjects;

            for (int i = 0; i < selectedObjects.Length; i++) {
                if (PrefabUtility.GetPrefabObject(selectedObjects[i]) != null) {
                    string prefabName = Path.GetFileNameWithoutExtension(selectedObjects[i].name);
                    string prefabGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(selectedObjects[i]));

                    SceneLocking.SetPrefabLock(prefabName, prefabGUID, "", true);
                }
            }
        }

        //[MenuItem("Assets/Outfit7 Lock Prefab", true, 5000)]
        private static bool LockPrefabCheck() {
            bool allLockedFlag = true;
            GameObject[] selectedObjects = Selection.gameObjects;

            if (selectedObjects == null || selectedObjects.Length == 0) {
                return false;
            }

            for (int i = 0; i < selectedObjects.Length; i++) {
                if (PrefabUtility.GetPrefabObject(selectedObjects[i]) == null) {
                    return false;
                }

                string path = AssetDatabase.GetAssetPath(selectedObjects[i]);
                string prefabGUID = AssetDatabase.AssetPathToGUID(path);
                JSONNode prefabInfo = SceneLocking.GetPrefabInfo(path, prefabGUID);

                allLockedFlag &= !prefabInfo["locked"].AsBool;
            }

            return allLockedFlag;
        }

        //[MenuItem("Assets/Outfit7 Unlock Prefab", false, 5000)]
        private static void UnlockPrefab() {
            GameObject[] selectedObjects = Selection.gameObjects;

            for (int i = 0; i < selectedObjects.Length; i++) {
                if (PrefabUtility.GetPrefabObject(selectedObjects[i]) != null) {
                    string prefabName = Path.GetFileNameWithoutExtension(selectedObjects[i].name);
                    string prefabGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(selectedObjects[i]));

                    SceneLocking.SetPrefabLock(prefabName, prefabGUID, "", false);
                }
            }
        }

        //[MenuItem("Assets/Outfit7 Unlock Prefab", true, 5000)]
        private static bool UnlockPrefabCheck() {
            bool allUnlockedFlag = true;
            GameObject[] selectedObjects = Selection.gameObjects;

            if (selectedObjects == null || selectedObjects.Length == 0) {
                return false;
            }

            for (int i = 0; i < selectedObjects.Length; i++) {
                if (PrefabUtility.GetPrefabObject(selectedObjects[i]) == null) {
                    return false;
                }

                string path = AssetDatabase.GetAssetPath(selectedObjects[i]);
                string prefabGUID = AssetDatabase.AssetPathToGUID(path);
                JSONNode prefabInfo = SceneLocking.GetPrefabInfo(path, prefabGUID);

                allUnlockedFlag &= prefabInfo["locked"].AsBool;
            }

            return allUnlockedFlag;
        }

        public static string GetCurrentSceneName() {
            return EditorSceneManager.GetActiveScene().name + ".unity";
        }

        public static string[] GetAllScenePaths() {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            string[] filesTmp = new string[scenes.Length];
            for (int i = 0; i < scenes.Length; i++) {
                filesTmp[i] = scenes[i].path;
            }
            return filesTmp;
        }

        public static string[] GetAllPrefabGUIDs() {
            return AssetDatabase.FindAssets("t:prefab");
        }

        public static string GetProjectGitName() {
            string gitConfigPath = Directory.GetCurrentDirectory() + "/.git/config";

            if (!File.Exists(gitConfigPath)) {
                Debug.LogError("git config not found! (" + gitConfigPath + ")");
                return null;
            }

            string[] lines = File.ReadAllLines(gitConfigPath);
            bool searchNextLineFlag = false;
            foreach (string line in lines) {
                if (searchNextLineFlag) {
                    if (line.Contains("url =")) {
                        string[] split = line.Split(':');
                        if (split.Length >= 2) {
                            if (split[1].Contains("/")) {
                                string[] spl = split[1].Split('/');
                                return spl[spl.Length - 1];
                            }
                            return split[1];
                        }
                    }
                    if (line.Contains("[")) {
                        searchNextLineFlag = false;
                    }
                }
                if (line.Contains("[remote ")) {
                    searchNextLineFlag = true;
                }
            }

            return null;
        }

        public static string GetUserName() {
            return System.Environment.UserName;
        }

        public static JSONNode GetSceneInfo(string sceneName) {
            string projectName = GetProjectGitName();

            if (projectName == null) {
                EditorUtility.DisplayDialog("Unity Locking", "Failed to retrieve project name from git config file!", "Ok");
                return null;
            }

            WWWForm form = new WWWForm();
            form.AddField("project_name", projectName);
            form.AddField("scene_name", sceneName);

            WWW www = new WWW(urlSceneInfo, form);

            while (!www.isDone) {
                //Debug.LogError("www downloading: " + www.bytesDownloaded.ToString());
            }

            return JSON.Parse(www.text);
        }

        public static JSONNode GetSceneInfoMulti(JSONNode sceneInfo) {
            string projectName = GetProjectGitName();
            Debug.Log(projectName);

            if (projectName == null) {
                EditorUtility.DisplayDialog("Unity Locking", "Failed to retrieve project name from git config file!", "Ok");
                return null;
            }

            WWWForm form = new WWWForm();
            form.AddField("project_name", projectName);
            form.AddField("scene_data", sceneInfo.ToString());

            WWW www = new WWW(urlSceneInfoMulti, form);

            while (!www.isDone) {
                //Debug.LogError("www downloading: " + www.bytesDownloaded.ToString());
            }

            return JSON.Parse(www.text);
        }

        public static JSONNode GetPrefabInfo(string prefabName, string prefabGUID) {
            string projectName = GetProjectGitName();

            if (projectName == null) {
                EditorUtility.DisplayDialog("Unity Locking", "Failed to retrieve project name from git config file!", "Ok");
                return null;
            }

            WWWForm form = new WWWForm();
            form.AddField("project_name", projectName);
            form.AddField("prefab_name", prefabName);
            form.AddField("prefab_guid", prefabGUID);

            WWW www = new WWW(urlPrefabInfo, form);

            while (!www.isDone) {
                //Debug.LogError("www downloading: " + www.bytesDownloaded.ToString());
            }

            return JSON.Parse(www.text);
        }

        public static JSONNode GetPrefabInfo(JSONNode prefabInfo) {
            string projectName = GetProjectGitName();

            if (projectName == null) {
                EditorUtility.DisplayDialog("Unity Locking", "Failed to retrieve project name from git config file!", "Ok");
                return null;
            }

            WWWForm form = new WWWForm();
            form.AddField("project_name", projectName);
            form.AddField("prefab_data", prefabInfo.ToString());

            WWW www = new WWW(urlPrefabInfoMulti, form);

            while (!www.isDone) {
                //Debug.LogError("www downloading: " + www.bytesDownloaded.ToString());
            }

            return JSON.Parse(www.text);
        }

        public static JSONNode SetSceneLock(string projectName, string sceneName, string lockComment, string userName, bool locked) {
            if (projectName == null) {
                EditorUtility.DisplayDialog("Scene Locking", "Failed to retrieve project name from git config file!", "Ok");
                return null;
            }

            string locked_string = "";
            if (locked) {
                locked_string = "1";
            } else {
                locked_string = "0";
            }

            WWWForm form = new WWWForm();
            form.AddField("project_name", projectName);
            form.AddField("scene_name", sceneName);
            form.AddField("lock_comment", lockComment);
            form.AddField("user_name", userName);
            form.AddField("locked", locked_string);

            WWW www = new WWW(urlSceneSubmit, form);

            while (!www.isDone) {
                //Debug.LogError("www downloading: " + www.bytesDownloaded.ToString());
            }

            return JSON.Parse(www.text);
        }

        public static JSONNode SetSceneLock(string sceneName, string lockComment, bool locked, bool showDialogInfo = true) {
            string projectName = GetProjectGitName();
            string userName = GetUserName();

            JSONNode statusJson = SetSceneLock(projectName, sceneName, lockComment, userName, locked);

            if (showDialogInfo) {
                if (statusJson["success"].AsBool) {
                    if (locked) {
                        EditorUtility.DisplayDialog("Scene Locking", "Scene is now locked!", "Ok");
                    } else {
                        EditorUtility.DisplayDialog("Scene Locking", "Scene is now unlocked!", "Ok");
                    }
                } else {
                    EditorUtility.DisplayDialog("Scene Locking", "Failed to lock/unlock scene!", "Ok");
                }
            }

            return statusJson;
        }

        public static JSONNode SetSceneLock(bool locked, bool showDialogInfo = true) {
            return SetSceneLock(GetCurrentSceneName(), "", locked, showDialogInfo);
        }

        public static JSONNode SetPrefabLock(string projectName, string prefabName, string prefabGUID, string lockComment, string userName, bool locked) {
            if (projectName == null) {
                EditorUtility.DisplayDialog("Unity Locking", "Failed to retrieve project name from git config file!", "Ok");
                return null;
            }

            string locked_string = "";
            if (locked) {
                locked_string = "1";
            } else {
                locked_string = "0";
            }

            WWWForm form = new WWWForm();
            form.AddField("project_name", projectName);
            form.AddField("prefab_name", prefabName);
            form.AddField("prefab_guid", prefabGUID);
            form.AddField("lock_comment", lockComment);
            form.AddField("user_name", userName);
            form.AddField("locked", locked_string);

            WWW www = new WWW(urlPrefabSubmit, form);

            while (!www.isDone) {
                //Debug.LogError("www downloading: " + www.bytesDownloaded.ToString());
            }

            return JSON.Parse(www.text);
        }

        public static JSONNode SetPrefabLock(string prefabName, string prefabGUID, string lockComment, bool locked, bool showDialogInfo = true) {
            string projectName = GetProjectGitName();
            string userName = GetUserName();

            JSONNode statusJson = SetPrefabLock(projectName, prefabName, prefabGUID, lockComment, userName, locked);

            if (showDialogInfo) {
                if (statusJson["success"].AsBool) {
                    if (locked) {
                        EditorUtility.DisplayDialog("Unity Locking", "Prefab is now locked!", "Ok");
                    } else {
                        EditorUtility.DisplayDialog("Unity Locking", "Prefab is now unlocked!", "Ok");
                    }
                } else {
                    EditorUtility.DisplayDialog("Unity Locking", "Failed to lock/unlock prefab!", "Ok");
                }
            }

            return statusJson;
        }

        public static JSONNode SetUnityUnlockAll(string userName) {
            WWWForm form = new WWWForm();
            form.AddField("user_name", userName);

            WWW www = new WWW(urlSceneUnlockAll, form);

            while (!www.isDone) {
            }

            return JSON.Parse(www.text);
        }
    }
}