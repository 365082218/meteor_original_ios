//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Text;
using Outfit7.Common;
using Outfit7.Common.Messaging.Database;
using Outfit7.Json;
using Outfit7.Threading;
using Outfit7.Threading.Task;
using Outfit7.Util;
using Outfit7.Web;
using SimpleJSON;
using SQLite;

namespace Outfit7.Common.Messaging {

    /// <summary>
    /// Messaging worker.
    /// </summary>
    public class MessagingWorker {

        protected const string Tag = "MessagingWorker";
        protected const double DbCloseAfterSecs = 60;
        protected const string ExecutorName = "MessagingWorker";
        protected const int SendTimeoutMs = 30 * 1000;
        protected const string RestPath = "/rest/messenger/v1/devices/{0}/applications/{1}/messages";
        protected const string SignatureMagic = "6d70f1fc-83f7-4b07-b4b9-a150a1531721";
        protected ThreadExecutor Executor;
        protected MessagingDatabase Database;
        protected Action DbCloser;
        protected string AppId;
        protected string LanguageCode;
        protected string UserAgentName;

        public MainExecutor MainExecutor { get; set; }

        public virtual void Init() {
            O7Log.DebugT(Tag, "Init");

            AppId = AppPlugin.AppId;
            LanguageCode = AppPlugin.LanguageCode;
            UserAgentName = AppPlugin.UserAgentName;

            Executor = new ThreadExecutor(ExecutorName);
            Executor.SleepMillis = 1000;

            Database = new MessagingDatabase();

            DbCloser = delegate {
                Database.Close();
                O7Log.VerboseT(Tag, "Database closed");
            };

            O7Log.DebugT(Tag, "Done");
        }

        #region Incoming Message
        public virtual void NewMessageIncoming(InboxMessage message)
        {
            Executor.RemoveAllSchedules(DbCloser);
            Executor.Post(delegate
            {
                bool saved;
                try
                {
                    saved = Database.ExecuteInTransaction<bool>(delegate (SQLiteConnection db) {
                        Database.InboxDao.Save(db, message);
                        return true;
                    }, false);
                }
                catch (SQLiteException e)
                {
                    O7Log.ErrorT(Tag, e, "Error saving deleted message to DB");
                    DbCloser();
                    return;
                }
                Executor.RemoveAllSchedules(DbCloser);
                Executor.PostDelayed(DbCloser, DbCloseAfterSecs);
            });
        }

        #endregion
        #region Load

        public virtual void PostLoadMessages(TaskFeedback<IList<InboxMessage>> feedback) {
            ExecutorTaskFeedbackWrapper<IList<InboxMessage>> feedbackWrapper = new ExecutorTaskFeedbackWrapper<IList<InboxMessage>>(feedback);

            Executor.RemoveAllSchedules(DbCloser);
            Executor.Post(delegate {
                O7Log.DebugT(Tag, "Loading inbox...");
                feedbackWrapper.OnStart();

                List<InboxMessage> inbox;
                try {
                    inbox = Database.ExecuteInTransaction<List<InboxMessage>>(Database.InboxDao.LoadAll, false);

                } catch (SQLiteException e) {
                    O7Log.ErrorT(Tag, e, "Error loading inbox from DB");
                    DbCloser();
                    feedbackWrapper.OnError(e);
                    return;
                }

                if (O7Log.DebugEnabled) {
                    O7Log.DebugT(Tag, "Loaded {0} inbox messages: {1}", inbox.Count,
                        StringUtils.CollectionToCommaDelimitedString(inbox));
                }
                feedbackWrapper.OnFinish(inbox);

                Executor.RemoveAllSchedules(DbCloser);
                Executor.PostDelayed(DbCloser, DbCloseAfterSecs);
            });
        }

#endregion

#region Save

        public virtual void PostSendMessage(Message message, TaskFeedback<Null> feedback) {
            ExecutorTaskFeedbackWrapper<Null> feedbackWrapper = (feedback != null) ? new ExecutorTaskFeedbackWrapper<Null>(feedback) : null;

            Executor.RemoveAllSchedules(DbCloser);
            Executor.Post(delegate {
                O7Log.DebugT(Tag, "Preparing outgoing message: {0}...", message);
                if (feedbackWrapper != null) {
                    feedbackWrapper.OnStart();
                }

                try {
                    Database.ExecuteInTransaction<Null>(delegate(SQLiteConnection db) {
                        Database.OutboxDao.Save(db, new OutboxMessage(message));
                        return null;
                    }, false);

                } catch (SQLiteException e) {
                    O7Log.ErrorT(Tag, e, "Error saving outgoing message to DB");
                    DbCloser();
                    if (feedbackWrapper != null) {
                        feedbackWrapper.OnError(e);
                    }
                    return;
                }

                O7Log.DebugT(Tag, "Prepared outgoing message");
                if (feedbackWrapper != null) {
                    feedbackWrapper.OnFinish(null);
                }

                Executor.RemoveAllSchedules(DbCloser);
                Executor.PostDelayed(DbCloser, DbCloseAfterSecs);
            });
        }

        public virtual void PostDeleteMessage(Guid id, TaskFeedback<bool> feedback) {
            ExecutorTaskFeedbackWrapper<bool> feedbackWrapper = (feedback != null) ? new ExecutorTaskFeedbackWrapper<bool>(feedback) : null;

            Executor.RemoveAllSchedules(DbCloser);
            Executor.Post(delegate {
                O7Log.DebugT(Tag, "Deleting message {0}...", id);
                if (feedbackWrapper != null) {
                    feedbackWrapper.OnStart();
                }

                bool deleted;
                try {
                    deleted = Database.ExecuteInTransaction<bool>(delegate(SQLiteConnection db) {
                        InboxMessage message = Database.InboxDao.Load(db, id);
                        if (message == null) return false;
                        Database.InboxDao.Delete(db, id);
                        Database.TrashDao.Save(db, new TrashMessage(message));
                        return true;
                    }, false);

                } catch (SQLiteException e) {
                    O7Log.ErrorT(Tag, e, "Error saving deleted message to DB");
                    DbCloser();
                    if (feedbackWrapper != null) {
                        feedbackWrapper.OnError(e);
                    }
                    return;
                }

                O7Log.DebugT(Tag, "Deleted message? {0}", deleted);
                if (feedbackWrapper != null) {
                    feedbackWrapper.OnFinish(deleted);
                }

                Executor.RemoveAllSchedules(DbCloser);
                Executor.PostDelayed(DbCloser, DbCloseAfterSecs);
            });
        }

        public virtual void PostSetMessageAsRead(Guid id, bool read, TaskFeedback<bool> feedback) {
            ExecutorTaskFeedbackWrapper<bool> feedbackWrapper = (feedback != null) ? new ExecutorTaskFeedbackWrapper<bool>(feedback) : null;

            Executor.RemoveAllSchedules(DbCloser);
            Executor.Post(delegate {
                O7Log.DebugT(Tag, "Setting message {0} as read={1}...", id, read);
                if (feedbackWrapper != null) {
                    feedbackWrapper.OnStart();
                }

                bool changed;
                try {
                    changed = Database.ExecuteInTransaction<bool>(delegate(SQLiteConnection db) {
                        InboxMessage message = Database.InboxDao.Load(db, id);
                        if (message == null) return false;
                        if (message.Read == read) return false;
                        message.Read = read;
                        Database.InboxDao.Save(db, message);
                        return true;
                    }, false);

                } catch (SQLiteException e) {
                    O7Log.ErrorT(Tag, e, "Error saving message to DB");
                    DbCloser();
                    if (feedbackWrapper != null) {
                        feedbackWrapper.OnError(e);
                    }
                    return;
                }

                O7Log.DebugT(Tag, "Set message {0} as read={1}? {2}", id, read, changed);
                if (feedbackWrapper != null) {
                    feedbackWrapper.OnFinish(changed);
                }

                Executor.RemoveAllSchedules(DbCloser);
                Executor.PostDelayed(DbCloser, DbCloseAfterSecs);
            });
        }

        public virtual void PostSetMessagePayload(Guid id, string payload, TaskFeedback<bool> feedback) {
            ExecutorTaskFeedbackWrapper<bool> feedbackWrapper = (feedback != null) ? new ExecutorTaskFeedbackWrapper<bool>(feedback) : null;

            Executor.RemoveAllSchedules(DbCloser);
            Executor.Post(delegate {
                O7Log.DebugT(Tag, "Setting message {0} payload={1}...", id, payload);
                if (feedbackWrapper != null) {
                    feedbackWrapper.OnStart();
                }

                bool changed;
                try {
                    changed = Database.ExecuteInTransaction<bool>(delegate(SQLiteConnection db) {
                        InboxMessage message = Database.InboxDao.Load(db, id);
                        if (message == null) return false;
                        if (message.Payload == payload) return false;
                        message.Payload = payload;
                        Database.InboxDao.Save(db, message);
                        return true;
                    }, false);

                } catch (SQLiteException e) {
                    O7Log.ErrorT(Tag, e, "Error saving message to DB");
                    DbCloser();
                    if (feedbackWrapper != null) {
                        feedbackWrapper.OnError(e);
                    }
                    return;
                }

                O7Log.DebugT(Tag, "Set message {0} payload={1}? {2}", id, payload, changed);
                if (feedbackWrapper != null) {
                    feedbackWrapper.OnFinish(changed);
                }

                Executor.RemoveAllSchedules(DbCloser);
                Executor.PostDelayed(DbCloser, DbCloseAfterSecs);
            });
        }

#endregion

#region Sync

        public virtual void PostSyncMessages(string serverBaseUrl, bool deleteAllIncoming, TaskFeedback<bool> feedback) {
            string uid = AppPlugin.Uid; // Must call native from main thread

            ExecutorTaskFeedbackWrapper<bool> feedbackWrapper = new ExecutorTaskFeedbackWrapper<bool>(feedback);

            Executor.RemoveAllSchedules(DbCloser);
            Executor.Post(delegate {
                O7Log.DebugT(Tag, "Loading outgoing & deleted messages...");
                feedbackWrapper.OnStart();

                Pair<List<OutboxMessage>, List<TrashMessage>> pair;
                try {
                    pair = LoadOutgoingAndDeletedMessages();

                } catch (SQLiteException e) {
                    O7Log.ErrorT(Tag, e, "Error loading outgoing & deleted messages from DB");
                    DbCloser();
                    feedbackWrapper.OnError(e);
                    return;
                }

                List<OutboxMessage> outgoingMessages = pair.First;
                List<TrashMessage> deletedMessages = pair.Second;

                if (O7Log.DebugEnabled) {
                    O7Log.DebugT(Tag, "* {0} outgoing messages: {1}", outgoingMessages.Count,
                        StringUtils.CollectionToCommaDelimitedString(outgoingMessages));
                    O7Log.DebugT(Tag, "* {0} deleted messages: {1}", deletedMessages.Count,
                        StringUtils.CollectionToCommaDelimitedString(deletedMessages));
                }

                SimpleWorker.RunAsync(delegate {
                    O7Log.DebugT(Tag, "Syncing messages ({0} outgoing, {1} deleted)...", outgoingMessages.Count,
                        deletedMessages.Count);

                    List<InboxMessage> incomingMessages;
                    try {
                        incomingMessages = SendAndRequestFromBackend(outgoingMessages, deletedMessages, serverBaseUrl, uid);

                    } catch (TimeoutException e) {
                        O7Log.WarnT(Tag, e, "Timeout syncing messages");
                        feedbackWrapper.OnError(e);
                        return;

                    } catch (RestCallException e) {
                        O7Log.WarnT(Tag, e, "Error syncing messages");
                        feedbackWrapper.OnError(e);
                        return;
                    }

                    O7Log.DebugT(Tag, "Synced messages (got {0} incoming)", incomingMessages.Count);

                    Executor.RemoveAllSchedules(DbCloser);
                    Executor.Post(delegate {
                        if (outgoingMessages.Count > 0 || deletedMessages.Count > 0) {
                            O7Log.DebugT(Tag, "Removing {0} outgoing & {1} deleted messages...",
                                outgoingMessages.Count, deletedMessages.Count);

                            try {
                                RemoveSentOutgoingAndDeletedMessages(outgoingMessages, deletedMessages);

                                O7Log.DebugT(Tag, "Removed outgoing & deleted messages");

                            } catch (SQLiteException e) {
                                O7Log.ErrorT(Tag, e, "Error removing outgoing & deleted messages from DB");
                                DbCloser();
                                // Not so bad if this fails, go on
                            }
                        }

                        int newAddedCount = 0;
                        if (incomingMessages.Count > 0) {
                            try {
                                if (deleteAllIncoming) {
                                    if (O7Log.DebugEnabled) {
                                        O7Log.DebugT(Tag, "Deleting {0} incoming messages: {1}...",
                                            incomingMessages.Count,
                                            StringUtils.CollectionToCommaDelimitedString(incomingMessages));
                                    }

                                    DeleteFetchedIncomingMessages(incomingMessages);

                                    O7Log.DebugT(Tag, "Deleted {0} incoming messages", incomingMessages.Count);

                                } else {
                                    if (O7Log.DebugEnabled) {
                                        O7Log.DebugT(Tag, "Saving new incoming messages from {0}: {1}...",
                                            incomingMessages.Count,
                                            StringUtils.CollectionToCommaDelimitedString(incomingMessages));
                                    }

                                    // Sort by server time before persisting to DB (order stays ensured by DB)
                                    incomingMessages.Sort(delegate(InboxMessage x, InboxMessage y) {
                                        return x.ServerDateTime.CompareTo(y.ServerDateTime);
                                    });
                                    newAddedCount = SaveFetchedIncomingMessages(incomingMessages);

                                    O7Log.DebugT(Tag, "Saved {0} new incoming messages", newAddedCount);
                                }

                            } catch (SQLiteException e) {
                                O7Log.ErrorT(Tag, e, "Error saving incoming messages to DB");
                                DbCloser();
                                feedbackWrapper.OnError(e);
                                return;
                            }
                        }

                        feedbackWrapper.OnFinish(newAddedCount > 0);

                        Executor.RemoveAllSchedules(DbCloser);
                        Executor.PostDelayed(DbCloser, DbCloseAfterSecs);
                    });
                });
            });
        }

        protected virtual Pair<List<OutboxMessage>, List<TrashMessage>> LoadOutgoingAndDeletedMessages() {
            return Database.ExecuteInTransaction<Pair<List<OutboxMessage>, List<TrashMessage>>>(delegate(SQLiteConnection db) {
                List<OutboxMessage> outbox = Database.OutboxDao.LoadAll(db);
                List<TrashMessage> trash = Database.TrashDao.LoadAll(db);
                return new Pair<List<OutboxMessage>, List<TrashMessage>>(outbox, trash);
            }, false);
        }

        protected virtual void RemoveSentOutgoingAndDeletedMessages(List<OutboxMessage> outgoingMessages, List<TrashMessage> deletedMessages) {
            // Delete only messages from outbox & trash that backend got (don't delete all, because user can create new messages or delete some in the meantime)
            Database.ExecuteInTransaction<Null>(delegate(SQLiteConnection db) {
                if (outgoingMessages.Count > 0) {
                    foreach (OutboxMessage m in outgoingMessages) {
                        Database.OutboxDao.Delete(db, m.Id);
                    }
                }
                if (deletedMessages.Count > 0) {
                    foreach (TrashMessage m in deletedMessages) {
                        Database.TrashDao.Delete(db, m.Id);
                    }
                }
                return null;
            }, false);
        }

        protected virtual int SaveFetchedIncomingMessages(List<InboxMessage> messages) {
            // Accept new messages if not already in inbox or trash
            return Database.ExecuteInTransaction<int>(delegate(SQLiteConnection db) {
                int addedCount = 0;
                foreach (InboxMessage m in messages) {
                    TrashMessage tm = Database.TrashDao.Load(db, m.Id);
                    if (tm != null) continue;
                    InboxMessage im = Database.InboxDao.Load(db, m.Id);
                    if (im != null) continue;
                    bool added = Database.InboxDao.Save(db, m);
                    if (added) {
                        addedCount++;
                    }
                }
                return addedCount;
            }, false);
        }

        protected virtual void DeleteFetchedIncomingMessages(List<InboxMessage> messages) {
            Database.ExecuteInTransaction<Null>(delegate(SQLiteConnection db) {
                foreach (InboxMessage m in messages) {
                    Database.TrashDao.Save(db, new TrashMessage(m));
                }
                return null;
            }, false);
        }

#endregion

#region Backend Communication

        //@POST
        //@Path("/rest/messenger/v1/devices/{uid}/applications/{applicationId}/messages")
        //@PathParam("uid")
        //@PathParam("applicationId")
        //@QueryParam("ts")
        //@QueryParam("s")
        //String jsonBody

        protected virtual List<InboxMessage> SendAndRequestFromBackend(ICollection<OutboxMessage> outgoingMessages,
            ICollection<TrashMessage> deletedMessages, string serverBaseUrl, string uid) {
            string timestampS = StringUtils.ToUniString(TimeUtils.CurrentTimeMillis);

            StringBuilder valueToSign = new StringBuilder(50).Append(uid).Append(AppId).Append(timestampS).Append(SignatureMagic);
            string signature = CryptoUtils.Sha1(valueToSign.ToString());

            StringBuilder sb = new StringBuilder(100);
            sb.Append(serverBaseUrl);
            sb.Append(string.Format(RestPath, uid, AppId));
            sb.Append("/?ts=");
            sb.Append(timestampS);
            sb.Append("&s=");
            sb.Append(signature);
            string url = sb.ToString();

            JSONClass dataJ = new JSONClass();
            if (outgoingMessages.Count > 0) {
                JSONArray messagesJ = new JSONArray();
                foreach (OutboxMessage m in outgoingMessages) {
                    messagesJ.Add(ConvertMessageToJson(m));
                }
                dataJ["messages"] = messagesJ;
            }
            if (deletedMessages.Count > 0) {
                JSONArray messageIdsJ = new JSONArray();
                foreach (TrashMessage m in deletedMessages) {
                    messageIdsJ.Add(m.Id.ToString());
                }
                dataJ["messagesToBeDeleted"] = messageIdsJ;
            }
            string jsonBody = dataJ.ToString();

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

            JSONArray incomingsJ = SimpleJsonUtils.EnsureJsonArray(responseJ);

            List<InboxMessage> incomingMessages = new List<InboxMessage>(incomingsJ.Count);
            foreach (JSONNode j in incomingsJ) {
                InboxMessage m;
                try {
                    m = ConvertJsonToMessage(j);
                    Assert.HasText(m.Id.ToString(), "id", "Fetched message must have ID: {0}", m);

                } catch (Exception e) {
                    O7Log.WarnT(Tag, e, "Error parsing message");
                    continue;
                }
                incomingMessages.Add(m);
            }

            return incomingMessages;
        }

        protected virtual JSONNode ConvertMessageToJson(Message m) {
            JSONClass j = new JSONClass();
            j["id"] = m.Id.ToString();
            j["timestamp"].AsLong = TimeUtils.ToTimeMillis(m.DateTime);
            j["fromName"] = m.AddresseeName;
            j["to"] = m.Addressee;
            j["fromLanguageCode"] = LanguageCode;
            j["toLanguageCode"] = m.AddresseeLanguageCode;
            j["text"] = m.Text;
            if (m.Payload != null) {
                j["payload"] = JSON.Parse(m.Payload);
            }
            return j;
        }

        protected virtual InboxMessage ConvertJsonToMessage(JSONNode j) {
            JSONNode payloadJ = j["payload"];
            string payload = (payloadJ != null) ? payloadJ.ToString() : null;
            return new InboxMessage(new Guid(j["id"]),
                TimeUtils.ToDateTime(j["timestamp"].AsLong),
                TimeUtils.ToDateTime(j["serverTimestamp"].AsLong),
                j["from"],
                j["fromName"],
                j["fromLanguageCode"],
                j["text"],
                payload,
                false);
        }

#endregion
    }
}
