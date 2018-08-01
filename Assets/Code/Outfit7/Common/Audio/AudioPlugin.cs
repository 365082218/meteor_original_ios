//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

namespace Outfit7.Audio {

    /// <summary>
    /// Audio plugin.
    /// </summary>
    public static class AudioPlugin {

#if UNITY_IPHONE && !NATIVE_SIM
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _forceToSpeaker();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _SetCorrectAudioSessionType();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern float _GetDeviceVolume();
#endif

        public static void ForceToSpeaker() {
#if UNITY_IPHONE && !(UNITY_EDITOR || NATIVE_SIM)
            Outfit7.Util.O7Log.Debug("ForceToSpeaker - fixing audio route on iOS");
            _forceToSpeaker();
#endif
        }

        public static void SetCorrectAudioSessionType() {
#if UNITY_IPHONE && !(UNITY_EDITOR || NATIVE_SIM)
            _SetCorrectAudioSessionType();
#endif
        }

        /// <summary>
        /// Get device volume
        /// </summary>
        /// <returns>
        /// The volume of the device
        /// </returns>
        public static float DeviceVolume {
            get {
#if UNITY_EDITOR || NATIVE_SIM
                return 0.5f;
#elif UNITY_IPHONE
                return _GetDeviceVolume();
#elif UNITY_ANDROID
                return Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef<float>("getAudioManager", "getDeviceVolume");
#elif UNITY_WP8
                return O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.GetDeviceVolume();
#endif
            }
        }
    }
}
