//
//   Copyright (c) 2016 Outfit7. All rights reserved.
//

namespace Outfit7.Analytics.BigQuery {

    /// <summary>
    /// Dummy BigQuery event builder. Does nothing.
    /// </summary>
    public sealed class DummyBigQueryEventBuilder : IBigQueryEventBuilder {

        public static readonly DummyBigQueryEventBuilder Instance = new DummyBigQueryEventBuilder();

        public bool Add() {
            return false;
        }

        public bool AddAndSend() {
            return false;
        }

        public IBigQueryEventBuilder SetNetwork(int network) {
            return this;
        }

        public IBigQueryEventBuilder SetRooted(bool rooted) {
            return this;
        }

        public IBigQueryEventBuilder SetPushedGrid(bool pushedGrid) {
            return this;
        }

        public IBigQueryEventBuilder SetTime(long timestamp) {
            return this;
        }

        public IBigQueryEventBuilder SetTimeZoneOffset(int offsetMs) {
            return this;
        }

        public IBigQueryEventBuilder SetSessionId(int sessionId) {
            return this;
        }

        public IBigQueryEventBuilder SetGroupId(string groupId) {
            return this;
        }

        public IBigQueryEventBuilder SetEventId(string eventId) {
            return this;
        }

        public IBigQueryEventBuilder SetElapsedTime(long timestamp) {
            return this;
        }

        public IBigQueryEventBuilder SetP1(string p1) {
            return this;
        }

        public IBigQueryEventBuilder SetP2(string p2) {
            return this;
        }

        public IBigQueryEventBuilder SetP3(long p3) {
            return this;
        }

        public IBigQueryEventBuilder SetP4(long p4) {
            return this;
        }

        public IBigQueryEventBuilder SetP5(string p5) {
            return this;
        }

        public IBigQueryEventBuilder SetCustomData(string data) {
            return this;
        }
    }
}
