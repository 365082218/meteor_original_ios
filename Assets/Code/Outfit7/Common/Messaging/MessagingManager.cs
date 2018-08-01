//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using Outfit7.Common;
using Outfit7.Common.Messaging.Database;
using Outfit7.Event;
using Outfit7.Grid;
using Outfit7.Json;
using Outfit7.Threading.Task;
using Outfit7.Util;
using SimpleJSON;

namespace Outfit7.Common.Messaging {

    /// <summary>
    /// Messaging manager.
    /// </summary>
    public class MessagingManager {

        protected const string Tag = "MessagingManager";
        protected const string FirstSyncedPref = "MessagingManager.FirstSynced";
        protected bool IsSyncInProgress;

        public virtual bool FirstSynced { get; private set; }

        public EventBus EventBus { get; set; }

        public GridManager GridManager { get; set; }

        public MessagingWorker MessagingWorker { get; set; }

        public virtual void Init() {
            FirstSynced = UserPrefs.GetBool(FirstSyncedPref, false);
            O7Log.VerboseT(Tag, "Init, FirstSynced={0}", FirstSynced);
        }

        public virtual void LoadMessages(TaskFeedback<IList<InboxMessage>> feedback) {
            MessagingWorker.PostLoadMessages(feedback);
        }

        public virtual void SendMessage(string senderName, string recipient, string recipientLanguageCode, string text,
            string payload, TaskFeedback<Null> feedback) {
            Message m = new Message(Guid.Empty, DateTime.UtcNow, recipient, senderName, recipientLanguageCode, text,
                            payload, true);
            SendMessage(m, feedback);
        }

        public virtual void SendMessage(Message message, TaskFeedback<Null> feedback) {
            MessagingWorker.PostSendMessage(message, feedback);
        }

        public virtual void DeleteMessage(Message message, TaskFeedback<bool> feedback) {
            DeleteMessage(message.Id, feedback);
        }

        public virtual void DeleteMessage(Guid id, TaskFeedback<bool> feedback) {
            MessagingWorker.PostDeleteMessage(id, feedback);
        }

        public virtual void SetMessageAsRead(Message message, bool read, TaskFeedback<bool> feedback) {
            message.Read = read;
            SetMessageAsRead(message.Id, read, feedback);
        }

        public virtual void SetMessageAsRead(Guid id, bool read, TaskFeedback<bool> feedback) {
            MessagingWorker.PostSetMessageAsRead(id, read, feedback);
        }

        public virtual void SetMessagePayload(Message message, string payload, TaskFeedback<bool> feedback) {
            message.Payload = payload;
            SetMessagePayload(message.Id, payload, feedback);
        }

        public virtual void SetMessagePayload(Guid id, string payload, TaskFeedback<bool> feedback) {
            MessagingWorker.PostSetMessagePayload(id, payload, feedback);
        }

        public virtual void SyncMessages() {
            O7Log.DebugT(Tag, "Syncing messages (FirstSynced={0})...", FirstSynced);

            if (!GridManager.Ready) {
                // Prevent sending anything to BE before GRID data, because UID might change
                O7Log.WarnT(Tag, "Canceled syncing messages: no GRID data");
                return;
            }

            if (IsSyncInProgress) {
                O7Log.DebugT(Tag, "Canceled syncing messages: already in progress");
                return;
            }

            IsSyncInProgress = true;
            TaskFeedback<bool> syncer = new TaskFeedback<bool>(delegate {
                IsSyncInProgress = false;

            }, delegate(bool newIncomingMessages) {
                IsSyncInProgress = false;
                ChangeFirstSynced(true);
                OnSync(newIncomingMessages);

            }, delegate(Exception e) {
                IsSyncInProgress = false;
            });
            MessagingWorker.PostSyncMessages(ServerBaseUrl, !FirstSynced, syncer);
        }

        protected virtual string ServerBaseUrl {
            get {
                if (!AppPlugin.IsDevelServerEnabled) { // Don't go alternative if DEV backend is in use
                    JSONArray j = SimpleJsonUtils.EnsureJsonArray(GridManager.JsonData["sDL"]["mBUs"]); // GridManager is ready here for sure
                    if (j != null && j.Count > 0) {
                        string altUrl = j[0].Value; // Use first element only
                        if (StringUtils.HasText(altUrl)) return altUrl;
                    }
                }
                return AppPlugin.ServerBaseUrl;
            }
        }

        protected virtual void OnSync(bool newIncomingMessages) {
            O7Log.DebugT(Tag, "Synced messages, got new incoming? {0}", newIncomingMessages);
        }

        protected virtual void ChangeFirstSynced(bool firstSynced) {
            if (firstSynced == FirstSynced) return;
            FirstSynced = firstSynced;
            UserPrefs.SetBool(FirstSyncedPref, FirstSynced);
            UserPrefs.SaveDelayed();
        }
    }
}
