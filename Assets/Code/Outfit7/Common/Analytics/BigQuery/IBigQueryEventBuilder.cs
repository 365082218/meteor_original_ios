//
//   Copyright (c) 2016 Outfit7. All rights reserved.
//

namespace Outfit7.Analytics.BigQuery {

    /// <summary>
    /// BigQuery event builder interface.
    /// </summary>
    public interface IBigQueryEventBuilder {

        bool Add();

        bool AddAndSend();

        IBigQueryEventBuilder SetNetwork(int network);

        IBigQueryEventBuilder SetRooted(bool rooted);

        IBigQueryEventBuilder SetPushedGrid(bool pushedGrid);

        IBigQueryEventBuilder SetTime(long timestamp);

        IBigQueryEventBuilder SetTimeZoneOffset(int offsetMs);

        IBigQueryEventBuilder SetSessionId(int sessionId);

        IBigQueryEventBuilder SetGroupId(string groupId);

        IBigQueryEventBuilder SetEventId(string eventId);

        IBigQueryEventBuilder SetElapsedTime(long timestamp);

        IBigQueryEventBuilder SetP1(string p1);

        IBigQueryEventBuilder SetP2(string p2);

        IBigQueryEventBuilder SetP3(long p3);

        IBigQueryEventBuilder SetP4(long p4);

        IBigQueryEventBuilder SetP5(string p5);

        IBigQueryEventBuilder SetCustomData(string data);
    }
}
