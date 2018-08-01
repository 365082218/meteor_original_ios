//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using System.IO;
using Outfit7.Common;
using Outfit7.Database;
using Outfit7.Database.Dao;
using Outfit7.Util;
using Outfit7.Util.Io;
using SQLite;

namespace Outfit7.Common.Messaging.Database {

    /// <summary>
    /// Messages database.
    /// </summary>
    public class MessagingDatabase : DatabaseSupport {

        protected const string Tag = "MessagingDatabase";
        protected const string DatabaseName = "messaging";
        protected const int DatabaseVersion = 1;

        protected static string CreateDatabasePath() {
            return Path.Combine(AppPlugin.InternalStoragePath, DatabaseName + ".db");
        }

        public static void DeleteDatabase() {
            try {
                O7File.Delete(CreateDatabasePath());

            } catch (Exception e) {
                O7Log.WarnT(Tag, e, "Cannot delete database: {0}", CreateDatabasePath());
            }
        }

        public DaoSupport<InboxMessage, Guid> InboxDao { get; private set; }

        public DaoSupport<OutboxMessage, Guid> OutboxDao { get; private set; }

        public DaoSupport<TrashMessage, Guid> TrashDao { get; private set; }

        public MessagingDatabase() : base(CreateDatabasePath(), DatabaseVersion) {
            InboxDao = new DaoSupport<InboxMessage, Guid>();
            OutboxDao = new DaoSupport<OutboxMessage, Guid>();
            TrashDao = new DaoSupport<TrashMessage, Guid>();
        }

        protected override SQLiteConnection CreateConnection(string databasePath) {
            return new SQLiteConnection(databasePath, true); // Store DateTime as ticks for messages, because millis get lost otherwise
        }

        public override void OnCreate(SQLiteConnection db) {
            O7Log.InfoT(Tag, "Creating database '{0}' version={1}", DatabasePath, DatabaseVersion);
            db.CreateTable<InboxMessage>();
            db.CreateTable<OutboxMessage>();
            db.CreateTable<TrashMessage>();
        }

        public override void OnUpgrade(SQLiteConnection db, int oldVersion, int newVersion) {
        }
    }
}
