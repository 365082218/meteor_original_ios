//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Text;
using Outfit7.Common;
using Outfit7.Grid;
using Outfit7.Json;
using Outfit7.Threading;
using Outfit7.Threading.Task;
using Outfit7.User.Adjuster;
using Outfit7.Util;
using Outfit7.Web;
using SimpleJSON;

namespace Outfit7.User.Adjuster {

    /// <summary>
    /// Adjuster.
    /// </summary>
    public class Adjuster {

        protected const string Tag = "Adjuster";
        protected const string AdjusterRestPath = "/rest/adjuster/v1/devices/";
        protected const string SignatureMagic = "875E52D8-9F88-4E2F-B8AD-3D42155B335A";
        protected const int FetchTimeoutMs = 30 * 1000;
        protected const int SendTimeoutMs = int.MaxValue;
        protected string AppId;
        protected string UserAgentName;
        protected Dictionary<string, Adjustment> Items;
        protected bool IsFetching;
        protected bool IsSending;

        public MainExecutor MainExecutor { get; set; }

        public virtual void Init() {
            O7Log.DebugT(Tag, "Init");

            AppId = AppPlugin.AppId;
            UserAgentName = AppPlugin.UserAgentName;
        }

        public virtual Adjustment GetAdjustment(string id) {
            if (Items == null)
                return null;
            if (!Items.ContainsKey(id))
                return null;
            return Items[id];
        }

        public virtual bool IsReady {
            get {
                return Items != null && Items.Count > 0;
            }
        }

        protected virtual bool IsInProgress {
            get {
                return IsFetching || IsSending || IsReady;
            }
        }

        public virtual void FetchNewAdjustments(string serverBaseUrl, TaskFeedback<Dictionary<string, Adjustment>> feedback) {
            O7Log.DebugT(Tag, "Fetching adjustments...");

            feedback.OnStart();

            if (IsInProgress) {
                O7Log.DebugT(Tag, "Won't fetch new adjustments: in progress");
                feedback.OnCancel();
                return;
            }

            string uid = AppPlugin.Uid;

            IsFetching = true;
            SimpleWorker.RunAsync(delegate {
                JSONNode jsonData;
                try {
                    jsonData = FetchAdjustmentsFromBackend(uid, serverBaseUrl);

                } catch (Exception e) {
                    O7Log.ErrorT(Tag, e, "Error fetching adjustments");
                    MainExecutor.Post(delegate {
                        IsFetching = false;
                        feedback.OnError(e);
                    });
                    return;
                }

                O7Log.DebugT(Tag, "Parsing adjustments...");
                var adjustments = ParseAdjustments(jsonData);

                O7Log.DebugT(Tag, "Got {0} adjustments: {1}", CollectionUtils.Count(adjustments),
                    StringUtils.CollectionToCommaDelimitedString(adjustments));

                MainExecutor.Post(delegate {
                    Items = adjustments;
                    IsFetching = false;
                    feedback.OnFinish(Items);
                });
            });
        }

        // NOT on main thread!
        protected virtual Dictionary<string, Adjustment> ParseAdjustments(JSONNode jsonData) {
            if (jsonData == null)
                return null;
            JSONArray dataJ = SimpleJsonUtils.EnsureJsonArray(jsonData["data"]);
            if (dataJ == null || dataJ.Count == 0)
                return null;

            var adjustments = new Dictionary<string, Adjustment>(dataJ.Count);
            foreach (JSONNode adjustmentJ in dataJ) {
                Adjustment adj;
                try {
                    adj = new Adjustment(adjustmentJ);

                } catch (Exception e) {
                    O7Log.ErrorT(Tag, e, "Invalid adjustment data: {0}", adjustmentJ);
                    continue;
                }

                adjustments[adj.Id] = adj;
            }

            return adjustments;
        }

        public virtual void ConfirmAdjustments(ICollection<Adjustment> adjustments, string serverBaseUrl,
            TaskFeedback<Dictionary<string, Adjustment>> feedback) {
            O7Log.DebugT(Tag, "Sending adjustments...");

            feedback.OnStart();

            if (IsSending) {
                O7Log.DebugT(Tag, "Won't send new adjustments: already sending");
                feedback.OnCancel();
                return;
            }

            string uid = AppPlugin.Uid;

            if (adjustments.Count == 0) {
                O7Log.DebugT(Tag, "Won't send new adjustments: nothing to send");
                feedback.OnCancel();
                return;
            }

            IsSending = true;
            SimpleWorker.RunAsync(delegate {
                JSONNode jsonData;
                try {
                    jsonData = SendAdjustmentsToBackend(adjustments, uid, serverBaseUrl);

                } catch (Exception e) {
                    O7Log.ErrorT(Tag, e, "Error sending adjustments");
                    MainExecutor.Post(delegate {
                        IsSending = false;
                        feedback.OnError(e);
                    });
                    return;
                }

                O7Log.DebugT(Tag, "Parsing unconfirmed adjustments...");
                var unconfirmedAdjs = ParseAdjustments(jsonData);

                O7Log.DebugT(Tag, "Confirmed {0} adjustments: {1}; Got {2} unconfirmed: {3}",
                    CollectionUtils.Count(adjustments), StringUtils.CollectionToCommaDelimitedString(adjustments),
                    CollectionUtils.Count(unconfirmedAdjs), StringUtils.CollectionToCommaDelimitedString(unconfirmedAdjs));

                MainExecutor.Post(delegate {
                    // Find and remove confirmed adjustments
                    var confirmedAdjs = new Dictionary<string, Adjustment>(adjustments.Count);
                    foreach (Adjustment adj in adjustments) {
                        if (unconfirmedAdjs != null && unconfirmedAdjs.ContainsKey(adj.Id)) {
                            O7Log.WarnT(Tag, "Adjustment was not confirmed: {0}", adj);
                            continue;
                        }
                        bool removed = Items.Remove(adj.Id);
                        if (!removed) {
                            O7Log.WarnT(Tag, "Unknown adjustment: {0}", adj);
                            continue;
                        }
                        confirmedAdjs[adj.Id] = adj;
                    }

                    // Update adjustments with unconfirmed
                    if (unconfirmedAdjs != null) {
                        foreach (Adjustment adj in unconfirmedAdjs.Values) {
                            Items[adj.Id] = adj;
                        }
                    }

                    IsSending = false;
                    feedback.OnFinish(confirmedAdjs);
                });
            });
        }

#region Backend Communication

        //@GET
        //@Path("/rest/adjuster/v1/devices/{uid}/applications/{applicationId}/adjustments")
        //@PathParam("uid")
        //@PathParam("applicationId")
        //@QueryParam("ts")
        //@QueryParam("s")

        // NOT on main thread!
        protected virtual JSONNode FetchAdjustmentsFromBackend(string uid, string serverBaseUrl) {
            string timestampS = StringUtils.ToUniString(TimeUtils.CurrentTimeMillis);

            StringBuilder valueToSign = new StringBuilder(60).Append(uid).Append(AppId).Append(timestampS).Append(SignatureMagic);
            string signature = CryptoUtils.Sha1(valueToSign.ToString());

            StringBuilder sb = new StringBuilder(100);
            sb.Append(serverBaseUrl);
            sb.Append(AdjusterRestPath);
            sb.Append(uid);
            sb.Append("/applications/");
            sb.Append(AppId);
            sb.Append("/adjustments/?ts=");
            sb.Append(timestampS);
            sb.Append("&s=");
            sb.Append(signature);
            string url = sb.ToString();

            Dictionary<string, string> headers = new Dictionary<string, string> {
                { RestCall.Header.UserAgent, UserAgentName },
                { RestCall.Header.ContentType, RestCall.ContentType.Json },
            };

            ThreadedRestCall call = new ThreadedRestCall(url, headers);
            call.Start(MainExecutor);
            call.WaitForResponse(FetchTimeoutMs);

            call.CheckForError();

            try {
                return JSON.Parse(call.ResponseBody);

            } catch {
                throw new RestCallException("Invalid response body");
            }
        }

        //@POST
        //@Path("/rest/adjuster/v1/devices/{uid}/applications/{applicationId}/adjustments")
        //@PathParam("uid")
        //@PathParam("applicationId")
        //@QueryParam("ts")
        //@QueryParam("s")
        //String jsonBody

        // NOT on main thread!
        protected virtual JSONNode SendAdjustmentsToBackend(IEnumerable<Adjustment> adjustments, string uid, string serverBaseUrl) {
            string timestampS = StringUtils.ToUniString(TimeUtils.CurrentTimeMillis);

            JSONArray array = new JSONArray();
            foreach (Adjustment adj in adjustments) {
                array.Add(adj.ToJson());
            }
            JSONClass dataJ = new JSONClass();
            dataJ["data"] = array;
            string jsonBody = dataJ.ToString();

            StringBuilder valueToSign = new StringBuilder(60).Append(uid).Append(AppId).Append(timestampS).Append(SignatureMagic);
            string signature = CryptoUtils.Sha1(valueToSign.ToString());

            StringBuilder sb = new StringBuilder(100);
            sb.Append(serverBaseUrl);
            sb.Append(AdjusterRestPath);
            sb.Append(uid);
            sb.Append("/applications/");
            sb.Append(AppId);
            sb.Append("/adjustments/?ts=");
            sb.Append(timestampS);
            sb.Append("&s=");
            sb.Append(signature);
            string url = sb.ToString();

            Dictionary<string, string> headers = new Dictionary<string, string> {
                { RestCall.Header.UserAgent, UserAgentName },
                { RestCall.Header.ContentType, RestCall.ContentType.Json },
            };

            ThreadedRestCall call = new ThreadedRestCall(url, jsonBody, headers);
            call.Start(MainExecutor);
            call.WaitForResponse(SendTimeoutMs);

            call.CheckForError();

            try {
                return JSON.Parse(call.ResponseBody);

            } catch {
                throw new RestCallException("Invalid response body");
            }
        }
    }

#endregion
}
