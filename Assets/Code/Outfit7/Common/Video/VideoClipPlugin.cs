//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//
using Outfit7.Util;
using SimpleJSON;
using UnityEngine;

namespace Outfit7.Video {

    /// <summary>
    /// Video clip plugin for native calls.
    /// </summary>
    public class VideoClipPlugin : MonoBehaviour {

        private const string Tag = "VideoClipPlugin";

        public VideoClipManager VideoClipManager { get; set; }
#if UNITY_WP8 && !UNITY_EDITOR
        private Outfit7.Threading.Executor MainExecutor;
#endif

#if UNITY_IPHONE && !NATIVE_SIM

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _StartLoadingVideoClip();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _StartShowingVideoClip();
#endif

#if UNITY_WP8 && !UNITY_EDITOR
        private void Awake() {
            MainExecutor = new Outfit7.Threading.Executor();
            O7.Plugins.Wp8.UnityCommon.VideoClipNativeProvider.OnVideoClipAvailable += __OnVideoClipAvailable;
            O7.Plugins.Wp8.UnityCommon.VideoClipNativeProvider.OnVideoClipCompletion += __OnVideoClipCompletion;
        }

        private void __OnVideoClipAvailable(string amount) {
            MainExecutor.Post(() => {
                _SetVideoClipAvailable(amount);
            });
        }

        private void __OnVideoClipCompletion(string data) {
            MainExecutor.Post(() => {
                _OnVideoClipCompletion(data);
            });
        }
#endif

#if UNITY_EDITOR || NATIVE_SIM
        private bool ShowingVideoClip;

        // Show/hide video clip in Unity editor only
        private void OnApplicationPause(bool paused)
        {
            if (paused) return;

            if (ShowingVideoClip)
            {
                ShowingVideoClip = false;
                const string data = "{ unityeditor: \"10\" }";
                _OnVideoClipCompletion(data);
            }

            VideoClipManager.VideoClipData = (VideoClipManager.VideoClipData == null) ? new VideoClipData("10", false) : null;
        }
#endif

        public void _SetVideoClipAvailable(string jsonString) {
            O7Log.VerboseT(Tag, "_SetVideoClipAvailable({0})", jsonString);

            JSONNode dataJ = JSON.Parse(jsonString);
            if (dataJ == null) {
                VideoClipManager.VideoClipData = null;
                return;
            }

            string amount = dataJ["amount"];
            bool currencyMentioning = dataJ["canGiveOnlyGC"].AsBool;
            VideoClipManager.VideoClipData = StringUtils.HasText(amount) ? new VideoClipData(amount, currencyMentioning) : null;
        }

        public void StartLoadingVideoClip() {
            O7Log.VerboseT(Tag, "StartLoadingVideoClip()");

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _StartLoadingVideoClip();
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef("getAdManager", "startLoadingVideoClip");
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.VideoClipNativeProvider.StartLoadingVideoClip();
#endif
        }

        public void StartShowingVideoClip() {
            O7Log.VerboseT(Tag, "StartShowingVideoClip()");

#if UNITY_EDITOR || NATIVE_SIM
            ShowingVideoClip = true;
            Application.OpenURL("https://www.youtube.com/watch?v=QImasK_hv_k");
#elif UNITY_IPHONE
            _StartShowingVideoClip();
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef("getAdManager", "startShowingVideoClip");      
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.VideoClipNativeProvider.StartShowingVideoClip();
#endif
        }

        public void _OnVideoClipCompletion(string data) {
            O7Log.VerboseT(Tag, "_OnVideoClipCompletion({0})", data);

            JSONNode dataJ = JSON.Parse(data);
            JSONNode rewardJ = dataJ[0]; // Only one node
            string id = rewardJ.Key;
            int amount = rewardJ.AsInt;

            VideoClipManager.OnVideoClipCompletion(id, amount);
        }
    }
}
