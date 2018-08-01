//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using Outfit7.Threading;
using Outfit7.Util;
using UnityEngine;

namespace Outfit7.Grid {

    /// <summary>
    /// Grid data plugin for native calls.
    /// </summary>
    public class GridPlugin : MonoBehaviour {

        private const string Tag = "GridPlugin";
#if UNITY_EDITOR || NATIVE_SIM
        private const string FirstRunPref = "GridPlugin.FirstRun";
#endif

#if UNITY_EDITOR || NATIVE_SIM || UNITY_WP8
        private Executor MainExecutor;
#endif

        public GridManager GridManager { get; set; }

#if UNITY_IPHONE && !NATIVE_SIM
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string _LoadGridData();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _UpdateNewsTimestamp(long ts);
#endif

#if UNITY_EDITOR || NATIVE_SIM || UNITY_WP8
        private void Awake() {
            MainExecutor = new Executor();

#if !(UNITY_EDITOR || NATIVE_SIM)
            O7.Plugins.Wp8.UnityCommon.GridNativeProvider.OnGridDataDownloaded += __OnFreshGridDataDownload;
#endif
        }
#endif

#if UNITY_WP8
        private void __OnFreshGridDataDownload(string jsonData) {
            // NOT on main thread!
            MainExecutor.Post(delegate {
                _OnFreshGridDataDownload(jsonData);
            });
        }
#endif

        public string LoadGridData() {
            O7Log.DebugT(Tag, "LoadGridData()");

#if UNITY_EDITOR
            string resourcePath = Application.dataPath + "/EditorTestFiles/";
            WWW www = new WWW("file:///" + resourcePath + "grid_sample.json.txt");
            while (!www.isDone) {
            }
            string json = www.text;
#elif NATIVE_SIM
            TextAsset jsonFile = ResourceManager.Load("grid_sample.json") as TextAsset;
            string json = (jsonFile == null) ? null : jsonFile.text;
#elif UNITY_IPHONE
            return _LoadGridData();
#elif UNITY_ANDROID
            return Outfit7.Util.AndroidPluginManager.Instance.ActivityCall<string>("loadGridData");
#elif UNITY_WP8
            return O7.Plugins.Wp8.UnityCommon.GridNativeProvider.LoadGridData();
#endif

#if UNITY_EDITOR || NATIVE_SIM
            if (UserPrefs.GetBool(FirstRunPref, true)) {
                UserPrefs.SetBool(FirstRunPref, false);
                // Simulate GRID data download on first start after some delay
                MainExecutor.PostDelayed(delegate {
                    _OnFreshGridDataDownload(json);
                }, 10);
                return null;
            }
            return json;
#endif
        }

        public void _OnFreshGridDataDownload(string jsonData) {
            O7Log.DebugT(Tag, "_OnFreshGridDataDownload({0})", jsonData);
            GridManager.OnFreshDataDownload(jsonData);
        }

        public void UpdateNewsTimestamp(DateTime time) {
            O7Log.DebugT(Tag, "UpdateNewsTimestamp({0})", time);

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            long timeMs = Outfit7.Util.TimeUtils.ToTimeMillis(time);
            _UpdateNewsTimestamp(timeMs);
#elif UNITY_ANDROID
            long timeMs = Outfit7.Util.TimeUtils.ToTimeMillis(time);
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("updateNewsTimestamp", timeMs);
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.NewsNativeProvider.UpdateNewsTimestamp(time);
#endif
        }
    }
}
