//
//   Copyright (c) 2016 Outfit7. All rights reserved.
//

using System;
using Outfit7.Common;
using Outfit7.Event;
using Outfit7.Util;
using SimpleJSON;

namespace Outfit7.Common {

    /// <summary>
    /// App shortcut manager.
    /// </summary>
    public class AppShortcutManager {

        protected const string Tag = "AppShortcutManager";

        public EventBus EventBus { get; set; }

        public AppShortcutPlugin AppShortcutPlugin { get; set; }

        public virtual void SetAppShortcuts(JSONArray shortcuts) {
            // Wrap shortcuts into a root property that SDK understands.
            JSONClass shortcutsJson = new JSONClass();
            shortcutsJson["shortcuts"] = shortcuts;
            AppShortcutPlugin.SetAppShortcuts(shortcutsJson.ToString());
        }

        public virtual void OnAppShortcut(string shortcutId) {
            EventBus.FireEvent(CommonEvents.APP_SHORTCUT, shortcutId);
        }
    }
}
