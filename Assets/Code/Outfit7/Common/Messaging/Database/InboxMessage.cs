//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using SQLite;

namespace Outfit7.Common.Messaging.Database {

    /// <summary>
    /// Inbox message.
    /// </summary>
    [Table("inbox")]
    public class InboxMessage : Message {

        // Used only temporarily before incoming message is saved to DB
        [Ignore]
        public DateTime ServerDateTime { get; set; }

        public InboxMessage() {
        }

        public InboxMessage(Message message) : base(message) {
        }

        public InboxMessage(Guid id, DateTime dateTime, DateTime serverDateTime, string addressee, string addresseeName,
            string addresseeLanguageCode, string text, string payload, bool read)
            : base(id, dateTime, addressee, addresseeName, addresseeLanguageCode, text, payload, read) {
            ServerDateTime = serverDateTime;
        }

        public override string ToString() {
            return string.Format("[InboxMessage: ServerDateTime={0}, {1}]", ServerDateTime, base.ToString());
        }
    }
}
