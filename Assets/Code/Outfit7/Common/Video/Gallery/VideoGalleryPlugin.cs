//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using UnityEngine;
using Outfit7.Util;

namespace Outfit7.Video.Gallery {

    /// <summary>
    /// Video gallery plugin.
    /// </summary>
    public class VideoGalleryPlugin : MonoBehaviour {

        private const string Tag = "VideoGalleryPlugin";

        public VideoGalleryManager VideoGalleryManager { get; set; }

#if UNITY_IPHONE && !(UNITY_EDITOR || NATIVE_SIM)
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern bool _OpenVideoGalleryView(string videoId);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern bool _OpenVideoGalleryViewWithUrl(string videoUrl);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern bool _OpenVideoGalleryViewWithRoom(string videoId, string room);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern bool _ReportVGButtonImpression();
#endif

#if UNITY_EDITOR || NATIVE_SIM
        // Show/hide video gallery in Unity editor only
        private void Start() {
            VideoGalleryManager.Ready = true;
        }
#endif

        public void ReportVGButtonImpression() {
            O7Log.DebugT(Tag, "ReportVGButtonImpression");

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _ReportVGButtonImpression();
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("reportVGButtonImpression");
#elif UNITY_WP8

#endif
        }

        public void OpenVideoGalleryView(string videoId, string room) {
            O7Log.DebugT(Tag, "OpenVideoGalleryView({0},{1})", videoId, room);

#if UNITY_EDITOR || NATIVE_SIM
            Application.OpenURL("http://www.youtube.com/channel/UCDCNmuaOXOo25Yn4mbMHhhQ"); // Talking Tom and Friends YouTube channel
#elif UNITY_IPHONE
            _OpenVideoGalleryViewWithRoom(videoId, room);
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("openVideoGalleryView", videoId, room);
#elif UNITY_WP8

#endif
        }

        public void OpenVideoGalleryView(string videoId) {
            O7Log.DebugT(Tag, "OpenVideoGalleryView({0})", videoId);

#if UNITY_EDITOR || NATIVE_SIM
            Application.OpenURL("http://www.youtube.com/channel/UCDCNmuaOXOo25Yn4mbMHhhQ"); // Talking Tom and Friends YouTube channel
#elif UNITY_IPHONE
            _OpenVideoGalleryView(videoId);
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("openVideoGalleryView", videoId);
#elif UNITY_WP8

#endif
        }

        public void OpenVideoGalleryViewWithUrl(string videoUrl) {
            O7Log.DebugT(Tag, "OpenVideoGalleryViewWithUrl({0})", videoUrl);

#if UNITY_EDITOR || NATIVE_SIM
            Application.OpenURL(videoUrl); // Talking Tom and Friends YouTube channel
#elif UNITY_IPHONE
            _OpenVideoGalleryViewWithUrl(videoUrl);
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("openVideoGalleryViewWithUrl", videoUrl);
#elif UNITY_WP8

#endif
        }

        private void SetVideoGalleryReady(bool ready) {
            O7Log.DebugT(Tag, "SetVideoGalleryReady({0})", ready);
            VideoGalleryManager.Ready = ready;
        }

        public void _SetVideoGalleryReady(string ready) {
            SetVideoGalleryReady(ready == "true");
        }
    }
}
