//
//   Copyright (c) 2016 Outfit7. All rights reserved.
//

using UnityEngine;
using Outfit7.Event;
using Outfit7.Util;

namespace Outfit7.Common {

    /// <summary>
    /// Plugin for Application shortcuts (3D touch on iOS, App Shortcuts on Android 7+).
    /// </summary>
    public class AppShortcutPlugin : MonoBehaviour {

        protected const string Tag = "AppShortcutPlugin";

        public AppShortcutManager AppShortcutManager { get; set; }

#if UNITY_IPHONE && !NATIVE_SIM

        [System.Runtime.InteropServices.DllImport("__Internal")]
        protected static extern void _SetAppShortcuts(string shortcuts);

#endif

        public virtual void SetAppShortcuts(string shortcuts) {
            O7Log.VerboseT(Tag, "SetAppShortcuts({0})", shortcuts);
#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
                _SetAppShortcuts(shortcuts);
#elif UNITY_ANDROID
                Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("setAppShortcuts", shortcuts);
#endif
        }

        public virtual void _OnAppShortcut(string shortcutId) {
            O7Log.VerboseT(Tag, "OnAppShortcut({0})", shortcutId);
            Assert.HasText(shortcutId, "shortcutId");
            AppShortcutManager.OnAppShortcut(shortcutId);
        }

    }
}
