using Outfit7.Common;
using Outfit7.Util;
using UnityEngine;

namespace Outfit7.Devel.O7Debug {

    public class HomeDebugSettings : DebugSettings {
#pragma warning disable 0414
        [SerializeField]
        private GameObject DBGButton = null;
        [SerializeField]
        private GameObject DBGPanel = null;
        [SerializeField]
        private GameObject ProdDBGPanel = null;
#pragma warning restore 0414
#if !STRIP_DBG_SETTINGS

        public static string Tag = "HomeDebugSettings";

        public static HomeDebugSettings Instance { get; private set; }

        private static bool WasDebugOpened = false;
        private bool IsProdDBGOpen = false;
        private bool IsDevel = false;
        private bool IsProd = false;

        public class EmailSettings {
            public string ProjectShortName = "<PROJ>";
            public string ReceiverAddress = "unity-dev@outfit7.com";
        }

        // set in Main.cs
        public EmailSettings EmailPrefs;

        protected override void Awake() {
            base.Awake();

            Instance = this;

#if DEVEL_BUILD || UNITY_EDITOR
            IsDevel = true;
#elif PROD_BUILD
            IsProd = true;
#endif
        }

        public void Init() {
            gameObject.SetActive(IsProd || IsDevel);
        }

        public void OpenHomeDebug() {
            OpenGUIDebug();

            if (IsProd && !IsProdDBGOpen && !WasDebugOpened) {
                SetActiveProdDebugPanel(true);
                SetActiveDebugPanel(false);
            } else {
                SetActiveProdDebugPanel(false);
                SetActiveDebugPanel(true);
            }
        }

        public override void CloseGUIDebug() {
            base.CloseGUIDebug();
            SetActiveDebugPanel(false);
            SetActiveProdDebugPanel(false);
        }

        public void SendBugReport() {
            SendLogs();
        }

        public void SetActiveDebugButton(bool active) {
            DBGButton.SetActive(active);
        }

        private void SetActiveDebugPanel(bool active) {
            DBGPanel.SetActive(active);
            if (active) {
                WasDebugOpened = true;
            }
        }

        private void SetActiveProdDebugPanel(bool active) {
            ProdDBGPanel.SetActive(active);
            IsProdDBGOpen = active;
        }

        private static string[] GetLogPaths() {
            string[] logs;
            if (FileLogger.LogPaths != null && FileLogger.LogPaths.Count > 1) {
                logs = new string[] { FileLogger.LogPath, FileLogger.LogPaths[FileLogger.LogPaths.Count - 2] };
            } else {
                logs = new string[] { FileLogger.LogPath };
            }
            return logs;
        }

        public void SendLogs() {
            if (!DebugSettingsController.Instance.EmailPlugin.CanSendEmail()) return;

            O7Log.DebugT(Tag, "Sending logs - bug report");

            FileLogger.Stop();

            string title = string.Format("{0} ({1}) - {2}", EmailPrefs.ProjectShortName, AppPlugin.AppVersion, AppPlugin.Platform);
            string body = string.Format("Platform: {0}\n Version: {1}\n\n <Description>", AppPlugin.Platform, AppPlugin.AppVersion);
            string[] logs = GetLogPaths();

            DebugSettingsController.Instance.EmailPlugin.StartSendingLogsViaEmail(EmailPrefs.ReceiverAddress, title, body, logs);
        }

        public void SendFeedbackReport() {
            if (!DebugSettingsController.Instance.EmailPlugin.CanSendEmail()) return;

            O7Log.DebugT(Tag, "Sending feedback report");

            FileLogger.Stop();

            string title = string.Format("({0}) - Platform: {1}\n Version: {2}\n\n - <Title>", EmailPrefs.ProjectShortName, AppPlugin.Platform, AppPlugin.AppVersion);
            const string body = "<Description>";
            string[] logs = GetLogPaths();
            DebugSettingsController.Instance.EmailPlugin.StartSendingLogsViaEmail(EmailPrefs.ReceiverAddress, title, body, logs);
        }
#endif
    }
}
