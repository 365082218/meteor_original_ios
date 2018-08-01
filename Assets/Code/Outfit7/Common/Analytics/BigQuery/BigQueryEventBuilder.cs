//
//   Copyright (c) 2016 Outfit7. All rights reserved.
//

using SimpleJSON;

namespace Outfit7.Analytics.BigQuery {

    /// <summary>
    /// Default BigQuery event builder.
    /// </summary>
    public class BigQueryEventBuilder : IBigQueryEventBuilder {

        protected readonly BigQueryTracker Tracker;

        public JSONClass Data { get; protected set; }

        public BigQueryEventBuilder(BigQueryTracker tracker) {
            Tracker = tracker;
            Data = new JSONClass();
        }

        // Must be run on Unity thread!
        public virtual bool Add() {
            Tracker.ConfigureBuilder(this);
            Tracker.AddEvent(Data.ToString());
            return true;
        }

        // Must be run on Unity thread!
        public virtual bool AddAndSend() {
            bool added = Add();
            if (added) {
                Tracker.SendEventsToBackend(true);
            }
            return added;
        }

        public virtual IBigQueryEventBuilder SetNetwork(int network) {
            Data["wifi"].AsInt = network;
            return this;
        }

        public virtual IBigQueryEventBuilder SetRooted(bool rooted) {
            Data["jb"].AsBool = rooted;
            return this;
        }

        public virtual IBigQueryEventBuilder SetPushedGrid(bool pushedGrid) {
            Data["rp"].AsBool = pushedGrid;
            return this;
        }

        public virtual IBigQueryEventBuilder SetTime(long timestamp) {
            Data["rts"].AsLong = timestamp;
            return this;
        }

        public IBigQueryEventBuilder SetTimeZoneOffset(int offsetMs) {
            Data["rtzo"].AsInt = offsetMs;
            return this;
        }

        public virtual IBigQueryEventBuilder SetSessionId(int sessionId) {
            Data["usid"].AsInt = sessionId;
            return this;
        }

        public virtual IBigQueryEventBuilder SetGroupId(string groupId) {
            Data["gid"] = groupId;
            return this;
        }

        public virtual IBigQueryEventBuilder SetEventId(string eventId) {
            Data["eid"] = eventId;
            return this;
        }

        public virtual IBigQueryEventBuilder SetElapsedTime(long timestamp) {
            Data["res"].AsLong = timestamp;
            return this;
        }

        public virtual IBigQueryEventBuilder SetP1(string p1) {
            Data["p1"] = p1;
            return this;
        }

        public virtual IBigQueryEventBuilder SetP2(string p2) {
            Data["p2"] = p2;
            return this;
        }

        public virtual IBigQueryEventBuilder SetP3(long p3) {
            Data["p3"].AsLong = p3;
            return this;
        }

        public virtual IBigQueryEventBuilder SetP4(long p4) {
            Data["p4"].AsLong = p4;
            return this;
        }

        public virtual IBigQueryEventBuilder SetP5(string p5) {
            Data["p5"] = p5;
            return this;
        }

        public virtual IBigQueryEventBuilder SetCustomData(string data) {
            Data["data"] = data;
            return this;
        }
    }
}
