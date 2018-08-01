using UnityEditor;
using UnityEngine;
using System.Collections;
using SimpleJSON;

namespace Outfit7.Devel.SceneLocking {
    //[InitializeOnLoad]
    public static class SceneLockingChecker {

        private static string CurrentScene;

        static SceneLockingChecker() {
            CurrentScene = SceneLocking.GetCurrentSceneName();
            EditorApplication.hierarchyWindowChanged += OnHierarchyWindowChanged;
            Selection.selectionChanged += OnSelectionChanged;
        }

        private static void OnHierarchyWindowChanged() {
            if (CurrentScene != SceneLocking.GetCurrentSceneName()) {
                if (Application.isEditor && !EditorApplication.isPlayingOrWillChangePlaymode) {
                    CurrentScene = SceneLocking.GetCurrentSceneName();
                    OnSceneChange();
                }
            }
        }

        private static void OnSceneChange() {
            if (Application.isPlaying) {
                return;
            }

            return;//新版本使用SVN维护，不再通过git协作 -Winson
            JSONNode unityLockJson = SceneLocking.GetSceneInfo(CurrentScene);
            if (unityLockJson == null) {
                EditorUtility.DisplayDialog("Connection to server lost", "The server can't be reached! Please check your connection or open the VPN client.", "Ok");
                return;
            }

            //告知用户该变动的场景被其他git用户锁定，这里做的修改都会起冲突.
            if (unityLockJson["locked"].AsBool && SceneLocking.GetUserName() != unityLockJson["user_name"].Value) {
                EditorUtility.DisplayDialog("Scene locked!", "Current scene (" + CurrentScene + ") is locked by " + unityLockJson["user_name"].Value, "Ok");
            }
        }

        private static void OnSelectionChanged() {
            GameObject[] gameobjects = Selection.gameObjects;
            for (int i = 0; i < gameobjects.Length; i++) {
                if (PrefabUtility.GetPrefabObject(gameobjects[i]) != null) {
                    if (Application.isPlaying && PrefabUtility.GetPrefabParent(gameobjects[i]) == null) {
                        return;
                    }
                    Object prefab = PrefabUtility.GetPrefabParent(gameobjects[i]) ?? gameobjects[i];
                    string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetOrScenePath(prefab));
                    JSONNode unityLockJson = SceneLocking.GetPrefabInfo(prefab.name, guid);
                    if (unityLockJson != null && unityLockJson["locked"] != null)
                    {
                        if (unityLockJson["locked"].AsBool && SceneLocking.GetUserName() != unityLockJson["user_name"].Value)
                        {
                            EditorUtility.DisplayDialog("Prefab locked!", "Prefab is locked by " + unityLockJson["user_name"].Value, "Ok");
                        }
                    }
                }
            }
        }
    }
}