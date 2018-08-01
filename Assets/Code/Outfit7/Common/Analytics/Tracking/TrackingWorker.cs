//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Text;
using Outfit7.Analytics.Tracking.Database;
using Outfit7.Common;
using Outfit7.Threading;
using Outfit7.Util;
using Outfit7.Web;
using SimpleJSON;
using SQLite;

namespace Outfit7.Analytics.Tracking {

    /// <summary>
    /// Event tracking worker.
    /// </summary>
    public class TrackingWorker {

        private const string Tag = "TrackingWorker";
        private const double DbCloseAfterSecs = 60;
        private const int SendTimeoutMs = 30 * 1000;
        private const string ExecutorName = "TrackingWorker";
        private const string RestPath = "/rest/data/1/events/";
        private const string SignatureMagic = "efDelc5820ckdf-249-4c3fj3iofwFEadEvded";
        private ThreadExecutor Executor;
        private TrackingDatabase Database;
        private Action DbCloser;
        private string Platform;
        private string AppId;
        private string AppVersion;
        private string UserAgentName;
        private string Uid;
        private string ServerBaseUrl;
        private Action Sender;

        // NOT on main thread!
        public int PendingEventCount { get; private set; }

        public MainExecutor MainExecutor { get; set; }

        public void Init() {
            O7Log.DebugT(Tag, "Init with executorName={0}", ExecutorName);

            Platform = AppPlugin.Platform;
            AppId = AppPlugin.AppId;
            AppVersion = AppPlugin.AppVersion;
            UserAgentName = AppPlugin.UserAgentName;

            Executor = new ThreadExecutor(ExecutorName);
            Executor.SleepMillis = 200;

            Database = new TrackingDatabase();

            DbCloser = delegate {
                Database.Close();
                O7Log.VerboseT(Tag, "Database closed");
            };

            // Count all events with some delay to not interfere with start of app
            Action counter = delegate {
                Database.ExecuteInTransaction<Null>(delegate(SQLiteConnection db) {
                    PendingEventCount = Database.TrackingEventDao.CountAll(db);
                    O7Log.DebugT(Tag, "Got {0} pending events on start", PendingEventCount);
                    return null;
                }, false);
                Executor.RemoveAllSchedules(DbCloser);
                Executor.PostDelayed(DbCloser, DbCloseAfterSecs);
            };
            Executor.PostDelayed(counter, 5f);

            O7Log.DebugT(Tag, "Done");
        }

        // ##############
        // ADDING EVENTS
        // ##############

        public virtual void PostAddEvent(TrackingEvent te) {
            Assert.NotNull(te, "te");
            PostAddEvents(new TrackingEvent[]{ te });
        }

        public virtual void PostAddEvents(ICollection<TrackingEvent> events) {
            Assert.HasLength(events, "events");
            Executor.RemoveAllSchedules(DbCloser);
            Action saver = delegate {
                try {
                    AddEvents(events);

                } catch (SQLiteException e) {
                    O7Log.ErrorT(Tag, e, "Error saving events");
                    DbCloser();
                    return;
                }

                Executor.RemoveAllSchedules(DbCloser);
                Executor.PostDelayed(DbCloser, DbCloseAfterSecs);
            };
            Executor.Post(saver);
        }

        private void AddEvents(ICollection<TrackingEvent> events) {
            Database.ExecuteInTransaction<Null>(delegate(SQLiteConnection db) {
                foreach (TrackingEvent te in events) {
                    Database.TrackingEventDao.Save(db, te);
                    PendingEventCount++;
                }
                return null;
            }, false);
        }

        // #########################
        // SENDING EVENTS TO BACKEND
        // #########################

        public virtual void PostSendEvents(string reportingId) {
            Uid = AppPlugin.Uid; // Must call native from main thread
            ServerBaseUrl = AppPlugin.ServerBaseUrl; // Must call native from main thread
            string thisReportingId = reportingId; // Create local var because of delegate problems
            Executor.RemoveAllSchedules(DbCloser);
            if (Sender != null) {
                Executor.RemoveAllSchedules(Sender);
            }
            Sender = delegate {
                try {
                    SendEvents(thisReportingId);

                } catch (SQLiteException e) {
                    O7Log.ErrorT(Tag, e, "Error loading or deleting events");
                    DbCloser();
                    return;

                } catch (TimeoutException) {
                } catch (RestCallException) {
                }

                Executor.RemoveAllSchedules(DbCloser);
                Executor.PostDelayed(DbCloser, DbCloseAfterSecs);
            };
            Executor.Post(Sender);
        }

        private bool SendEvents(string reportingId) {
            return Database.ExecuteInTransaction<bool>(delegate(SQLiteConnection db) {
                List<TrackingEvent> events = Database.TrackingEventDao.LoadAll(db);

                O7Log.DebugT(Tag, "Sending events to backend...");

                if (O7Log.DebugEnabled) {
                    O7Log.DebugT(Tag, "* Got reporting ID: {0}", reportingId);
                    O7Log.DebugT(Tag, "* Got {0} new events", events.Count);
                    foreach (TrackingEvent te in events) {
                        O7Log.DebugT(Tag, "* {0}", te);
                    }
                }

                if (events.Count == 0) {
                    O7Log.DebugT(Tag, "Not sending events; no new events");
                    return false;
                }

                try {
                    SendToGae(reportingId, events);

                } catch (TimeoutException e) {
                    O7Log.WarnT(Tag, e, "Did not send events to backend");
                    throw;

                } catch (RestCallException e) {
                    O7Log.WarnT(Tag, e, "Did not send events to backend");
                    throw;
                }

                Database.TrackingEventDao.DeleteAll(db);
                O7Log.DebugT(Tag, "* New events deleted");

                PendingEventCount = 0;

                O7Log.DebugT(Tag, "Events sent successfully");
                return true;
            }, false);
        }

        //@POST
        //@Path("/data/{versionId}/events")
        //@Produces(MediaType.APPLICATION_JSON)
        //@Consumes(MediaType.APPLICATION_JSON)
        //@PathParam("versionId") String versionId,
        //@QueryParam("ts") String ts,
        //@QueryParam("s") String s,
        //@QueryParam("reportingId") String reportingId,
        //@QueryParam("uid") String uid,
        //@QueryParam("platform") String platform,
        //@QueryParam("appId") String appId,
        //@QueryParam("appVersion") String appVersion,
        //String eventsJson

        private void SendToGae(string reportingId, IEnumerable<TrackingEvent> events) {
            string timestampS = StringUtils.ToUniString(TimeUtils.CurrentTimeMillis);

            JSONArray eventsJ = new JSONArray();
            foreach (TrackingEvent te in events) {
                //{
                //    "seqNum": 0,
                //    "wifi": 0,
                //    "jb": true,
                //    "rp": false,
                //    "rts": 33432323423432,
                //    "res": 12312,
                //    "eid": "a1c23dsf",
                //    "gid": "a1c23dsf",
                //    "p1": "P1",
                //    "p2": "P2",
                //    "p3": 100,
                //    "p4": 100,
                //    "p5": "P3"
                //},
                JSONClass eventJ = new JSONClass();
                // TODO TineL: Checking if this crashes (https://play.google.com/apps/publish/?dev_acc=17968821625316434532#ErrorClusterDetailsPlace:p=com.outfit7.mytalkingangelafree&et=CRASH&sh=false&lr=LAST_7_DAYS&ecn=com.outfit7.unity.exceptions.UnityException&tf=UnparsedFile&tc=++at+System.Nullable%25601%255BSystem.Int32%255D&tm=get_Value+&nid&an&c&s=total_reports_desc)
                Assert.NotNull(te.SequenceNumber, "SequenceNumber cannot be null");
                eventJ["seqNum"].AsInt = te.SequenceNumber.Value; // Must not be null once in DB
                eventJ["wifi"].AsInt = te.Network;
                eventJ["jb"].AsBool = te.Rooted;
                eventJ["rp"].AsBool = te.PushedGrid;
                eventJ["rts"].AsLong = te.Timestamp;
                if (te.ElapsedTime != null) {
                    eventJ["res"].AsInt = te.ElapsedTime.Value;
                }
                eventJ["eid"] = te.EventId;
                eventJ["gid"] = te.GroupId;
                eventJ["p1"] = te.Parameter1;
                eventJ["p2"] = te.Parameter2;
                if (te.Parameter3 != null) {
                    eventJ["p3"].AsLong = te.Parameter3.Value;
                }
                if (te.Parameter4 != null) {
                    eventJ["p4"].AsLong = te.Parameter4.Value;
                }
                eventJ["p5"] = te.Parameter5;
                eventJ["data"] = te.CustomData;
                eventsJ.Add(eventJ);
            }
            string jsonBody = eventsJ.ToString();

            StringBuilder valueToSign = new StringBuilder(100);
            valueToSign.Append(Uid);
            valueToSign.Append(timestampS);
            valueToSign.Append(SignatureMagic);
            string signature = CryptoUtils.Sha1(valueToSign.ToString());

            StringBuilder sb = new StringBuilder();
            sb.Append(ServerBaseUrl);
            sb.Append(RestPath);
            sb.Append("?ts=");
            sb.Append(timestampS);
            sb.Append("&s=");
            sb.Append(signature);
            if (StringUtils.HasText(reportingId)) {
                sb.Append("&reportingId=");
                sb.Append(reportingId);
            }
            sb.Append("&uid=");
            sb.Append(Uid);
            sb.Append("&platform=");
            sb.Append(Platform);
            sb.Append("&appId=");
            sb.Append(AppId);
            sb.Append("&appVersion=");
            sb.Append(AppVersion);
            string url = sb.ToString();

            Dictionary<string, string> headers = new Dictionary<string, string> {
                { RestCall.Header.UserAgent, UserAgentName },
                { RestCall.Header.ContentType, RestCall.ContentType.Json },
            };

            ThreadedRestCall call = new ThreadedRestCall(url, jsonBody, headers);
            call.Start(MainExecutor);
            call.WaitForResponse(SendTimeoutMs);

            call.CheckForError();
        }
    }
}
