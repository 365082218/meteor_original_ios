//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using Outfit7.Common;
using Outfit7.Database;
using Outfit7.Database.Dao;
using Outfit7.Util;
using Outfit7.Util.Io;
using SQLite;

namespace Outfit7.Analytics.Tracking.Database {

    /// <summary>
    /// Event tracking database.
    /// </summary>
    public class TrackingDatabase : DatabaseSupport {

        private const string Tag = "TrackingDatabase";
        private const string DB_NAME = "tracking";
        private const int DB_VERSION = 1;

        private static string CreateDatabasePath() {
            return AppPlugin.InternalStoragePath + "/" + DB_NAME + ".db";
        }

        public static void DeleteDatabase() {
            O7File.Delete(CreateDatabasePath());
        }

        public DaoSupport<TrackingEvent, int?> TrackingEventDao { get; private set; }

        public TrackingDatabase() : base(CreateDatabasePath(), DB_VERSION) {
            TrackingEventDao = new DaoSupport<TrackingEvent, int?>();
        }

        public override T ExecuteInTransaction<T>(Func<SQLiteConnection, T> function, bool closeDatabaseOnFinish) {
            // Database has been abandoned. It is used ready-only if exists only
            if (!O7File.Exists(DatabasePath)) {
                O7Log.DebugT(Tag, "Skipping transaction executing. No database file");
                return default(T);
            }
            return base.ExecuteInTransaction(function, closeDatabaseOnFinish);
        }

        public override void OnCreate(SQLiteConnection db) {
            // Database has been abandoned. It is used ready-only if exists only
            throw new InvalidProgramException("Must not create new database");
        }

        public override void OnUpgrade(SQLiteConnection db, int oldVersion, int newVersion) {
        }
    }
}
