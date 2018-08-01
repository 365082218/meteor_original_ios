using System;
using System.Collections.Generic;
using Outfit7.Util;
using Outfit7.Common;
using UnityEngine;

namespace Outfit7.Devel.O7Debug {
    public class DebugSettingsController {

        private const string Tag = "DebugSettingsController";

        public EmailPlugin EmailPlugin { get; set; }

        public  string LogsRecipientEmail;

        public  bool ShowDebugSetting = false;

        private List<object> GlobalDebuggableObjects = new List<object>();

        private List<DebugSettings> ActiveDebugSettings = new List<DebugSettings>();

        private static DebugSettingsController instance;

        public static DebugSettingsController Instance {
            get {
                if (instance == null) {
                    instance = new DebugSettingsController();
                }
                return instance;
            }
        }

#if STRIP_DBG_SETTINGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public void AddDebuggableObject(object obj) {
            InsertDebuggableObject(GlobalDebuggableObjects.Count, obj);
        }

        public void InsertDebuggableObject(int i, object obj) {
            if (obj == null)
                return;

            if (!GlobalDebuggableObjects.Contains(obj)) {
                GlobalDebuggableObjects.Insert(i, obj);
            }

            for (int j = 0; j < ActiveDebugSettings.Count; j++) {
                ActiveDebugSettings[j].Repopulate(GlobalDebuggableObjects, RemoveFromActiveDebugSettings);
            }
        }


#if STRIP_DBG_SETTINGS
        [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
        public void RemoveDebuggableObject(object obj) {
            GlobalDebuggableObjects.Remove(obj);
        }

        public T CreateDebugMenu<T>(string prefabPath, Transform root, bool addToActiveDebgSettings = true) where T : DebugSettings {
            GameObject DBGRoot = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>(prefabPath));
            DBGRoot.SetActive(true);
            T settings = DBGRoot.transform.GetComponent<T>();
            settings.Repopulate(GlobalDebuggableObjects, RemoveFromActiveDebugSettings);
            DBGRoot.transform.SetParent(root, false);

            if (addToActiveDebgSettings) {
                ActiveDebugSettings.Add(settings);
            }
            return settings;
        }

        public void RemoveFromActiveDebugSettings(DebugSettings dbgSettings) {
            ActiveDebugSettings.Remove(dbgSettings);
        }

        public void SendLogs() {
            if (string.IsNullOrEmpty(LogsRecipientEmail)) {
                return;
            }

            O7Log.DebugT(Tag, "Send logs");

            if (EmailPlugin.CanSendEmail()) {
                var logPaths = new List<string>();

                logPaths.Add(FileLogger.LogPath);
                // Logs on iOS 9.3 are failing since we dont have o7logger file (they added some assertion checks).
                //                logs.Add(Application.persistentDataPath + "/o7logger.txt");

                FileLogger.Stop();
                FileLogger.Start();

                string title = string.Format("Log: {0}\n AppBuild: {1}\n\n", AppPlugin.Platform, AppPlugin.AppVersion);
                EmailPlugin.StartSendingLogsViaEmail(LogsRecipientEmail, title, string.Empty, logPaths.ToArray());
            }
        }
    }
}

