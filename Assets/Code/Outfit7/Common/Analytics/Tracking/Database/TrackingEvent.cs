//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using Outfit7.Util;
using SQLite;

namespace Outfit7.Analytics.Tracking.Database {

    /// <summary>
    /// Event tracking event.
    /// </summary>
    [Table("event")]
    public class TrackingEvent {

        /// <summary>
        /// Application specific event sequence number; for each event sequence number must be incremented by client (this does auto-increment automatically).
        /// </summary>
        [Column("seqnum"), PrimaryKey, AutoIncrement]
        public int? SequenceNumber { get; set; }

        /// <summary>
        /// [0 | 1 | 2] - 0 - not know, 1 - 3G or other mobile network, 2 - wi-fi.
        /// </summary>
        [Column("wifi")]
        public int Network { get; set; }

        /// <summary>
        /// Is device rooted or jail-broken?
        /// </summary>
        [Column("jb")]
        public bool Rooted { get; set; }

        /// <summary>
        /// Is last GRID push?
        /// </summary>
        [Column("rp")]
        public bool PushedGrid { get; set; }

        /// <summary>
        /// Time in UTC, ms since epoch, when event occurred on client.
        /// </summary>
        [Column("rts")]
        public long Timestamp { get; set; }

        /// <summary>
        /// Elapsed time in ms, null if not sent by client.
        /// </summary>
        [Column("res")]
        public int? ElapsedTime { get; set; }

        /// <summary>
        ///  ID of the event group.
        /// </summary>
        [Column("gid")]
        public string GroupId { get; set; }

        /// <summary>
        /// ID of the event.
        /// </summary>
        [Column("eid")]
        public string EventId { get; set; }

        [Column("p1")]
        public string Parameter1 { get; set; }

        [Column("p2")]
        public string Parameter2 { get; set; }

        [Column("p3")]
        public long? Parameter3 { get; set; }

        [Column("p4")]
        public long? Parameter4 { get; set; }

        [Column("p5")]
        public string Parameter5 { get; set; }

        /// <summary>
        /// Custom payload data.
        /// </summary>
        [Column("data")]
        public string CustomData { get; set; }

        public TrackingEvent() {
        }

        public TrackingEvent(int network, bool rooted, bool pushedGrid, int? elapsedTime, string groupId,
            string eventId, string param1, string param2, long? param3, long? param4, string param5, string customData) : this(network, rooted,
            pushedGrid, TimeUtils.CurrentTimeMillis, elapsedTime, groupId, eventId, param1, param2, param3, param4, param5, customData) {
        }

        public TrackingEvent(int network, bool rooted, bool pushedGrid, long timestamp, int? elapsedTime, string groupId,
            string eventId, string param1, string param2, long? param3, long? param4, string param5, string customData) {
            Assert.HasText(groupId, "groupdId");
            Assert.HasText(eventId, "eventId");

            Network = network;
            Rooted = rooted;
            PushedGrid = pushedGrid;
            Timestamp = timestamp;
            ElapsedTime = elapsedTime;
            GroupId = groupId;
            EventId = eventId;
            Parameter1 = param1;
            Parameter2 = param2;
            Parameter3 = param3;
            Parameter4 = param4;
            Parameter5 = param5;
            CustomData = customData;
        }

        public override string ToString() {
            return string.Format("[TrackingEvent: SequenceNumber={0}, Network={1}, Rooted={2}, PushedGrid={3}, Timestamp={4}, ElapsedTime={5}, GroupId={6}, EventId={7}, Parameter1={8}, Parameter2={9}, Parameter3={10}, Parameter4={11}, Parameter5={12}, CustomData={13}]", SequenceNumber, Network, Rooted, PushedGrid, Timestamp, ElapsedTime, GroupId, EventId, Parameter1, Parameter2, Parameter3, Parameter4, Parameter5, CustomData);
        }
    }
}
