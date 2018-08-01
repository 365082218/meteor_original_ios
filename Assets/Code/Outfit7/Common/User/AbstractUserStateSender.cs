//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Text;
using Outfit7.Common;
using Outfit7.Threading;
using Outfit7.Threading.Task;
using Outfit7.Util;
using Outfit7.Web;
using SimpleJSON;

namespace Outfit7.User {

    /// <summary>
    /// Abstract user state sender.
    /// </summary>
    public abstract class AbstractUserStateSender<T> {

        protected const string Tag = "UserStateSender";
        protected const string ExecutorName = "UserStateSender";
        protected const int SendTimeoutMs = 120 * 1000;

        public enum SendingMode {
            Normal,
            NormalWithRemote,
            ForcingOverwriteAsNew,
            ForcingOverwriteAsNewWithRemote,
            ForcingOverwriteAsRestored,
            ForcingOverwriteAsRestoredWithRemote
        }

        protected string RestPath;
        protected string SignatureMagic;
        protected ThreadExecutor Executor;
        protected Action Sender;
        protected Action SenderPrematureUnpostCallback;
        protected string AppId;
        protected string AppVersion;
        protected string LibVersion;
        protected string Platform;
        protected string LanguageCode;
        protected string UserAgentName;
        protected bool LastSendingRejected;

        public MainExecutor MainExecutor { get; set; }

        protected virtual void Init(string restPath, string signatureMagic) {
            O7Log.DebugT(Tag, "Init");

            RestPath = restPath;
            SignatureMagic = signatureMagic;

            AppId = AppPlugin.AppId;
            AppVersion = AppPlugin.AppVersion;
            LibVersion = AppPlugin.LibraryVersion;
            Platform = AppPlugin.Platform;
            LanguageCode = AppPlugin.LanguageCode;
            UserAgentName = AppPlugin.UserAgentName;

            Executor = new ThreadExecutor(ExecutorName);
            Executor.SleepMillis = 100;

            O7Log.DebugT(Tag, "Done");
        }

        public virtual void PostSendState(T data, SendingMode sendingMode, string uid, string serverBaseUrl,
            TaskFeedback<JSONNode> feedback, Action prematureUnpostCallback) {
            O7Log.VerboseT(Tag, "PostSendState(sendingMode={0})", sendingMode);
            Assert.NotNull(data, "data");

            // data is immutable, no need to copy
            TaskFeedback<JSONNode> executorFeedback = new ExecutorTaskFeedbackWrapper<JSONNode>(feedback);

            // Clear last sender if not already processing
            if (Sender != null) {
                int prevSenderCount = Executor.RemoveAllSchedules(Sender);
                if (prevSenderCount > 0) {
                    O7Log.VerboseT(Tag, "Removed {0} queued senders", prevSenderCount);
                    if (SenderPrematureUnpostCallback != null) {
                        SenderPrematureUnpostCallback();
                    }
                }
            }

            SenderPrematureUnpostCallback = prematureUnpostCallback;
            Sender = delegate {
                O7Log.VerboseT(Tag, "Starting to send state...");

                executorFeedback.OnStart();

                // Never reset it to Normal until 'overwrite' sending succeeds
                if (sendingMode == SendingMode.ForcingOverwriteAsNew
                    || sendingMode == SendingMode.ForcingOverwriteAsNewWithRemote
                    || sendingMode == SendingMode.ForcingOverwriteAsRestored
                    || sendingMode == SendingMode.ForcingOverwriteAsRestoredWithRemote) {
                    LastSendingRejected = false;
                }

                if (LastSendingRejected) {
                    O7Log.DebugT(Tag, "Cannot send user state because previous sending was rejected");
                    executorFeedback.OnCancel();
                    return;
                }

                // Send data
                JSONNode responseJ;
                try {
                    responseJ = SendState(data, sendingMode, uid, serverBaseUrl);

                } catch (TimeoutException e) {
                    executorFeedback.OnError(e);
                    return;

                } catch (NotAcceptableRestCallException e) {
                    LastSendingRejected = true;
                    executorFeedback.OnError(e);
                    return;

                } catch (RestCallException e) {
                    executorFeedback.OnError(e);
                    return;
                }

                O7Log.VerboseT(Tag, "Done sending state");
                executorFeedback.OnFinish(responseJ);
            };

            Executor.Post(Sender);
        }

        protected virtual JSONNode SendState(T data, SendingMode sendingMode, string uid, string serverBaseUrl) {
            O7Log.DebugT(Tag, "Sending user state to backend...");

            string url = PrepareUrl(sendingMode, uid, serverBaseUrl).ToString();
            string jsonBody = PrepareBody(data).ToString();

            JSONNode responseJ;
            try {
                responseJ = SendToBackend(url, jsonBody);

            } catch (TimeoutException e) {
                O7Log.WarnT(Tag, e, "Did not send user state to backend");
                throw;

            } catch (NotAcceptableRestCallException e) {
                O7Log.WarnT(Tag, e, "User state was not accepted by backend");
                throw;

            } catch (RestCallException e) {
                O7Log.WarnT(Tag, e, "Did not send user state to backend");
                throw;
            }

            O7Log.DebugT(Tag, "User state sent successfully, got response: {0}", responseJ);
            return responseJ;
        }

        protected virtual StringBuilder PrepareUrl(SendingMode sendingMode, string uid, string serverBaseUrl) {
            string timestampS = StringUtils.ToUniString(TimeUtils.CurrentTimeMillis);
            string timeZoneOffsetS = StringUtils.ToUniString(TimeUtils.CurrentTimeZoneOffsetSeconds);
            string signature = string.Format("{0}{1}{2}", uid, timestampS, SignatureMagic);
            string signatureE = CryptoUtils.Sha1(signature);

            StringBuilder sb = new StringBuilder(300);
            sb.Append(serverBaseUrl).Append(RestPath);
            sb.Append("?aId=").Append(AppId);
            sb.Append("&u=").Append(uid);
            sb.Append("&s=").Append(signatureE);
            sb.Append("&t=").Append(timestampS);
            sb.Append("&lC=").Append(LanguageCode);
            sb.Append("&aV=").Append(AppVersion);
            sb.Append("&lV=").Append(LibVersion);
            sb.Append("&p=").Append(Platform);
            sb.Append("&tZO=").Append(timeZoneOffsetS);

            switch (sendingMode) {
                case SendingMode.ForcingOverwriteAsNew:
                    sb.Append("&fOM=NEW");
                    break;
                case SendingMode.ForcingOverwriteAsNewWithRemote:
                    sb.Append("&fOM=MASTER");
                    break;
                case SendingMode.ForcingOverwriteAsRestored:
                case SendingMode.ForcingOverwriteAsRestoredWithRemote:
                    sb.Append("&fOM=RESTORED");
                    break;
            }

            return sb;
        }

        protected abstract JSONNode PrepareBody(T data);

        protected virtual JSONNode SendToBackend(string url, string jsonBody) {
            Dictionary<string, string> headers = new Dictionary<string, string> {
                { RestCall.Header.UserAgent, UserAgentName },
                { RestCall.Header.ContentType, RestCall.ContentType.Json }
            };

            ThreadedRestCall call = new ThreadedRestCall(url, jsonBody, headers);
            call.Start(MainExecutor);
            call.WaitForResponse(SendTimeoutMs);

            call.CheckForError();

            // Check response but prevent crash in any circumstances, because response is much less important
            JSONNode responseJ;
            try {
                responseJ = JSON.Parse(call.ResponseBody);

            } catch {
                throw new RestCallException("Invalid response body");
            }

            if (responseJ == null) {
                throw new RestCallException("Empty response body");
            }

            // Check if backend has deliberately rejected us
            // Should be HTTP status 406, 409 or 412 but fucking Unity does that very differently on every platform,
            // so backend has to do workaround and send 406/409/412 in response body with '200 OK' status.
            string responseCodeS = responseJ["responseCode"].Value.Trim();
            if (responseCodeS == "406") {
                throw new NotAcceptableRestCallException(false, "406 Not Acceptable", responseJ);
            }
            if (responseCodeS == "409") {
                throw new NotAcceptableRestCallException(true, "409 Not Acceptable", responseJ);
            }
            if (responseCodeS == "412") {
                throw new RestCallException("412 Conflict: Version mismatch");
            }

            return responseJ;
        }
    }
}
