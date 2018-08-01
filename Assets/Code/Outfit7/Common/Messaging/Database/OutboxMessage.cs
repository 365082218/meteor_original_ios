//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using SQLite;

namespace Outfit7.Common.Messaging.Database {

    /// <summary>
    /// Outbox message.
    /// </summary>
    [Table("outbox")]
    public class OutboxMessage : Message {

        public OutboxMessage() {
        }

        public OutboxMessage(Message message) : base(message) {
        }

        public OutboxMessage(Guid id, DateTime dateTime, string addressee, string addresseeName,
            string addresseeLanguageCode, string text, string payload, bool read)
            : base(id, dateTime, addressee, addresseeName, addresseeLanguageCode, text, payload, read) {
        }
    }
}
