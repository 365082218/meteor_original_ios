//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using Outfit7.Util;
using SQLite;

namespace Outfit7.Common.Messaging.Database {

    /// <summary>
    /// Message.
    /// </summary>
    public class Message {

        [Column("id"), PrimaryKey, AutoIncrement]
        public Guid Id { get; set; }

        [Column("datetime")]
        public DateTime DateTime { get; set; }

        [Column("adressee"), NotNull]
        public string Addressee { get; set; }

        [Column("addresseename")]
        public string AddresseeName { get; set; }

        [Column("addresseelc")]
        public string AddresseeLanguageCode { get; set; }

        [Column("text")]
        public string Text { get; set; }

        [Column("payload")]
        public string Payload { get; set; }

        [Column("read")]
        public bool Read { get; set; }

        protected Message() {
        }

        public Message(Message message) : this(message.Id, message.DateTime, message.Addressee, message.AddresseeName,
                message.AddresseeLanguageCode, message.Text, message.Payload, message.Read) {
        }

        public Message(Guid id, DateTime dateTime, string addressee, string addresseeName, string addresseeLanguageCode,
            string text, string payload, bool read) {
            Assert.HasText(addressee, "addressee");
            Id = id;
            DateTime = dateTime;
            Addressee = addressee;
            AddresseeName = addresseeName;
            AddresseeLanguageCode = addresseeLanguageCode;
            Text = text;
            Payload = payload;
            Read = read;
        }

        public override string ToString() {
            return string.Format("[Message: Id={0}, DateTime={1}, Addressee={2}, AddresseeName={3}, AddresseeLanguageCode={4}, Text={5}, Payload={6}, Read={7}]",
                Id, DateTime.ToLocalTime(), Addressee, AddresseeName, AddresseeLanguageCode, Text,
                Payload, Read);
        }
    }
}
