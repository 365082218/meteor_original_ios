//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using UnityEngine;
using Outfit7.Util;

namespace Outfit7.GameCenter {

    /// <summary>
    /// Game center plugin for native calls.
    /// </summary>
    public class GameCenterPlugin : MonoBehaviour {

        private const string Tag = "GameCenterPlugin";
        private const string ActivityGetter = "getGameCenter";

        public GameCenterManager GameCenterManager { get; set; }

#if UNITY_IPHONE && !NATIVE_SIM
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern bool _GameCenterIsSignedIn();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _GameCenterSignIn();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _GameCenterSubmitGameScore(int score, string gameId);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _GameCenterOpenLeaderboard(string gameId);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _GameCenterOpenAchievements();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _GameCenterUnlockAchievementWithIndex(int id);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string _GameCenterPlayerId();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern bool _GameCenterOpenApp();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _GameCenterReportIncrementalAchievementWithIndex(int achIndex, float percentCompleted);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _GameCenterResetAchievements();
#endif

#if UNITY_EDITOR || NATIVE_SIM
        bool EditorLoggedIn;
#endif

        public bool Available {
            get {
                bool available;
#if UNITY_EDITOR
                WWW www = new WWW("file:///" + Application.dataPath + "/EditorTestFiles/gameHiScores.json.txt");
                while (!www.isDone) {
                }

                _OnHiScoreUpdate(www.text);

                available = true;
#elif NATIVE_SIM
                TextAsset jsonFile = ResourceManager.Load("gameHiScores.json") as TextAsset;
                if (jsonFile != null) _OnHiScoreUpdate(jsonFile.text);
                available = true;
#elif UNITY_IPHONE
                available = true;
#elif UNITY_ANDROID
                available = Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef<bool>(ActivityGetter, "isAvailable");
#elif UNITY_WP8
                available = false;
#endif
                O7Log.VerboseT(Tag, "Available {0}", available);
                return available;
            }
        }

        public bool IsSignedIn {
            get {
                bool signedIn;
#if UNITY_EDITOR || NATIVE_SIM
                signedIn = EditorLoggedIn;
#elif UNITY_IPHONE
                signedIn = _GameCenterIsSignedIn();
#elif UNITY_ANDROID
                signedIn = Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef<bool>(ActivityGetter, "isSignedIn");
#elif UNITY_WP8
                signedIn = false;
#endif
                O7Log.VerboseT(Tag, "IsSignedIn {0}", signedIn);
                return signedIn;
            }
        }

        public void SignIn() {
            O7Log.VerboseT(Tag, "SignIn");

#if UNITY_EDITOR || NATIVE_SIM
            EditorLoggedIn = true;
            _OnSignIn(null);
#elif UNITY_IPHONE
            _GameCenterSignIn();
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(ActivityGetter, "signIn");
#elif UNITY_WP8

#endif
        }

        public void SignOut() {
            O7Log.VerboseT(Tag, "SignOut");

#if UNITY_EDITOR || NATIVE_SIM
            EditorLoggedIn = false;
            _OnSignOut(null);
#elif UNITY_IPHONE

#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(ActivityGetter, "signOut");
#elif UNITY_WP8

#endif
        }

        public string GamingPlatformName {
            get {
                string platformName;
#if UNITY_EDITOR || NATIVE_SIM
                platformName = "UnityEditor";
#elif UNITY_IPHONE
                platformName = "GameCenter";
#elif UNITY_ANDROID
                platformName = "GPlay";
#elif UNITY_WP8
                platformName = "XBoxLive";
#endif
                O7Log.VerboseT(Tag, "GamingPlatformName {0}", platformName);
                return platformName;
            }
        }

        public string PlayerId {
            get {
                string playerId;
#if UNITY_EDITOR || NATIVE_SIM
                playerId = "UnityEditor";
#elif UNITY_IPHONE
                playerId =  _GameCenterPlayerId();
#elif UNITY_ANDROID
                playerId =  Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef<string>(ActivityGetter, "getPlayerId");
#elif UNITY_WP8
                playerId =  null;
#endif
                O7Log.VerboseT(Tag, "PlayerId {0}", playerId);
                return playerId;
            }
        }

        public bool OpenApp() {
            bool canOpenApp;

#if UNITY_EDITOR || NATIVE_SIM
            canOpenApp = true;
#elif UNITY_IPHONE
            canOpenApp = _GameCenterOpenApp();
#elif UNITY_ANDROID
            canOpenApp = true;
#elif UNITY_WP8
            canOpenApp = true;
#endif
            O7Log.VerboseT(Tag, "OpenApp {0}", canOpenApp);
            return canOpenApp;
        }

        public void _OnSignIn(string dummy) {
            O7Log.VerboseT(Tag, "_OnSignIn");
            GameCenterManager.IsSignedIn = true;
        }

        public void _OnSignOut(string dummy) {
            O7Log.VerboseT(Tag, "_OnSignOut");
            GameCenterManager.IsSignedIn = false;
        }

        public void _OnHiScoreUpdate(string jsonString) {
            O7Log.VerboseT(Tag, "_OnHiScoreUpdate {0}", jsonString);
            GameCenterManager.OnHiScoreUpdate(jsonString);
        }

        public void SubmitGameScore(int num, string id, long score) {
            O7Log.VerboseT(Tag, "SubmitGameScore(num={0}, id={1}, score={2})", num, id, score);

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _GameCenterSubmitGameScore((int) score, id);
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(ActivityGetter, "submitGameScore", num, score);
#elif UNITY_WP8

#endif
        }

        public void StartOpeningLeaderboard(int num, string id) {
            O7Log.VerboseT(Tag, "StartOpeningLeaderboard(num={0}, id={1})", num, id);

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _GameCenterOpenLeaderboard(id);
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(ActivityGetter, "openLeaderboard", num);
#elif UNITY_WP8

#endif
        }

        public void StartOpeningAchievements() {
            O7Log.VerboseT(Tag, "StartOpeningAchievements()");

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _GameCenterOpenAchievements();
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(ActivityGetter, "openAchievements");
#elif UNITY_WP8

#endif
        }

        public void UnlockAchievement(int id) {
            O7Log.VerboseT(Tag, "UnlockAchievement({0})", id);

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _GameCenterUnlockAchievementWithIndex(id);
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(ActivityGetter, "unlockAchievement", id);
#elif UNITY_WP8

#endif
        }

        /// <summary>
        /// Increments the achievement to the set amount of steps or percent (percent is defined in [0.0, 100.0] range!).
        /// </summary>
        public void IncrementAchievement(int id, int steps, float percent) {
            O7Log.VerboseT(Tag, "IncrementAchievement(id={0}, steps={1}, percent={2})", id, steps, percent);

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _GameCenterReportIncrementalAchievementWithIndex(id, percent);
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(ActivityGetter, "setAchievementSteps", id, steps, percent);
#elif UNITY_WP8

#endif
        }

        public void ResetAchievements() {
            O7Log.VerboseT(Tag, "ResetAchievements()");

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _GameCenterResetAchievements();
#elif UNITY_ANDROID

#elif UNITY_WP8

#endif
        }

        /// <summary>
        /// Check if GC or GPG is installed on the system. NOT the same as GameCenterPlugin.Available.
        /// </summary>
        /// <returns>true if GPG or GC is installed, false otherwise</returns>
        public bool IsGameCenterAppInstalled() {
            bool isInstalled = false;

#if UNITY_EDITOR || NATIVE_SIM
            isInstalled = true;
#elif UNITY_IPHONE
            isInstalled = true;
#elif UNITY_ANDROID
            isInstalled = Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef<bool>(ActivityGetter, "isGameCenterAppInstalled");
#elif UNITY_WP8

#endif
            O7Log.VerboseT(Tag, "IsGameCenterAppInstalled == {0}", isInstalled);
            return isInstalled;
        }
    }
}
