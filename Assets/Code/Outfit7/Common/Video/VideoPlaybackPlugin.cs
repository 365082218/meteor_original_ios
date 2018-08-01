using UnityEngine;
using Outfit7.Util;

namespace Outfit7.Video {
    public class VideoPlaybackPlugin : MonoBehaviour {

        protected const string Tag = "VideoPlaybackPlugin";

        protected const string AndroidGetter = "getVideoPlayback";

#if UNITY_IPHONE && !NATIVE_SIM
        [System.Runtime.InteropServices.DllImport("__Internal")]
        protected static extern bool _IsVideoPlaybackSupported();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        protected static extern void _PlayVideo(string json);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        protected static extern void _StopVideo(bool pause);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        protected static extern void _HideVideo();

#endif

        public VideoPlaybackManager VideoPlaybackManager { get; set; }

        public virtual bool IsPlaybackSupported {
            get {
                bool supported;
#if UNITY_EDITOR || NATIVE_SIM
                supported = true;
#elif UNITY_IPHONE
                supported = _IsVideoPlaybackSupported();
#elif UNITY_WP8
                supported = false;
#elif UNITY_ANDROID
                supported = Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef<bool>(AndroidGetter, "IsPlaybackSupported");
#endif
                O7Log.DebugT(Tag, "IsPlaybackSupported {0}", supported);
                return supported;
            }
        }

        public virtual void Play(string json) {
            O7Log.DebugT(Tag, "Play {0}", json);
#if UNITY_EDITOR || NATIVE_SIM
            PlaybackStarted("");
#elif UNITY_IPHONE
            _PlayVideo(json);
#elif UNITY_WP8

#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "Play", json);
#endif
        }

        public virtual void Stop(bool pause) {
            O7Log.DebugT(Tag, "Stop({0})", pause);
#if UNITY_EDITOR || NATIVE_SIM
            PlaybackStopped("{\"reason\": \"finished\"}");
#elif UNITY_IPHONE
            _StopVideo(pause);
#elif UNITY_WP8

#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "Stop", pause);
#endif
        }

        public virtual void Hide() {
            O7Log.DebugT(Tag, "Hide");
#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _HideVideo();
#elif UNITY_WP8

#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "Hide");
#endif
        }

#region NativeCallbacks

        public virtual void PlaybackStarted(string json) {
            VideoPlaybackManager.PlaybackStarted(json);
        }

        public virtual void PlaybackStopped(string json) {
            VideoPlaybackManager.PlaybackStopped(json);
        }

#endregion
    }
}