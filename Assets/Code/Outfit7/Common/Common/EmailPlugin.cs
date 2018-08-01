//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using UnityEngine;
using Outfit7.Event;
using Outfit7.Util;

namespace Outfit7.Common {

    /// <summary>
    /// Native interface plugin for sending e-mails.
    /// </summary>
    public class EmailPlugin : MonoBehaviour {

        private const string Tag = "EmailPlugin";

        public static class CompletionId {
            public const string Sent = "SENT";
            public const string SavedAsDraft = "DRAFT";
            public const string Canceled = "CANCELED";
            public const string Failed = "FAILED";
        }

        public EventBus EventBus { get; set; }

#if UNITY_IPHONE && !NATIVE_SIM
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern bool _CanSendEmail();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _StartSendingEmail(string recipient, string title, string body, string[] paths, int pathsCount);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _StartSendingLogsViaEmail(string recipient, string title, string body, string[] paths, int pathsCount);
#endif


#if UNITY_WP8 && !(UNITY_EDITOR || NATIVE_SIM)
        private Outfit7.Threading.Executor MainExecutor;

        private void Awake() {
            MainExecutor = new Outfit7.Threading.Executor();
            O7.Plugins.Wp8.UnityCommon.MailNativeProvider.OnEmailSent += __OnEmailSentEvent;
        }

        private void __OnEmailSentEvent(string completionId) {
            MainExecutor.Post(delegate {
                _OnEmailSent(completionId);
            });
        }
#endif

        public bool CanSendEmail() {
            O7Log.VerboseT(Tag, "CanSendEmail()");

#if UNITY_EDITOR || NATIVE_SIM
            return true; // Fake
#elif UNITY_IPHONE
            return _CanSendEmail();
#elif UNITY_ANDROID
            return Outfit7.Util.AndroidPluginManager.Instance.ActivityCall<bool>("canSendEmail");
#elif UNITY_WP8
            return O7.Plugins.Wp8.UnityCommon.MailNativeProvider.CanSendMail();
#endif
        }

        public void StartSendingEmail(string recipientEmail, string subject, string body, string[] attachmentPaths) {
            O7Log.VerboseT(Tag, "StartSendingEmail(to='{0}', subject='{1}', body='{2}', attachmentPaths='{3}')", recipientEmail, subject, body, attachmentPaths);

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            if (attachmentPaths == null) {
                _StartSendingEmail(recipientEmail, subject, body, null, 0);
            }else{
                _StartSendingEmail(recipientEmail, subject, body, attachmentPaths, attachmentPaths.Length);
            }
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("startSendingEmail", recipientEmail, subject, body, attachmentPaths);
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.MailNativeProvider.SendMail(recipientEmail, subject, body, attachmentPaths);
#endif
        }

        public void StartSendingLogsViaEmail(string recipientEmail, string subject, string body, string[] attachmentPaths) {
            O7Log.VerboseT(Tag, "StartSendingLogsViaEmail(to='{0}', subject='{1}', body='{2}', attachmentPaths='{3}')", recipientEmail, subject, body, attachmentPaths);

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            if (attachmentPaths == null) {
                _StartSendingLogsViaEmail(recipientEmail, subject, body, null, 0);
            }else{
                _StartSendingLogsViaEmail(recipientEmail, subject, body, attachmentPaths, attachmentPaths.Length);
            }
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("startSendingLogsViaEmail", recipientEmail, subject, body, attachmentPaths);
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.MailNativeProvider.SendLogsViaMail(recipientEmail, subject, body, attachmentPaths);
#endif
        }

        public void _OnEmailSent(string completionId) {
            O7Log.VerboseT(Tag, "_OnEmailSent({0})", completionId);
            EventBus.FireEvent(CommonEvents.EMAIL_COMPLETION, completionId);
        }
    }
}
