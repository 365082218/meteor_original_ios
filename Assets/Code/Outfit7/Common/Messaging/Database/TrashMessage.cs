//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using SQLite;

namespace Outfit7.Common.Messaging.Database {

    /// <summary>
    /// Trash message.
    /// </summary>
    [Table("trash")]
    public class TrashMessage : Message {

        public TrashMessage() {
        }

        public TrashMessage(Message message) : base(message) {
        }

        public TrashMessage(Guid id, DateTime dateTime, string addressee, string addresseeName,
            string addresseeLanguageCode, string text, string payload, bool read)
            : base(id, dateTime, addressee, addresseeName, addresseeLanguageCode, text, payload, read) {
        }
    }
}
