//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using Outfit7.Threading;
using Outfit7.Util;
using SQLite;

namespace Outfit7.Database {

    /// <summary>
    /// Database support. NOT thread safe. Guarded against multi-threading. Only the thread that opens the database,
    /// can use it and close it. Create another instance if you need simultaneous access in another thread.
    /// Using SQLite class (sqlite-net). Although SQLite seems to be thread safe, it is not when using transactions!
    /// Proof: Try to start two transactions in different threads and let the first transaction finish before the second.
    /// </summary>
    public abstract class DatabaseSupport {

        private SQLiteConnection database;
        private int openThreadId = -1;
        private int newVersion;
        private bool initing;

        public string DatabasePath { get; private set; }

        /**
         * Create a helper object to create, open, and/or manage a database.
         * The database is not actually created or opened until OpenWritableDatabase is called. Thus, this can be called
         * in any thread.
         *
         * @param databasePath the path of the database file (empty not permitted)
         * @param version the version number of the database (starting at 1); if the database is older,
         *      OnUpgrade will be used to upgrade the database
         */
        protected DatabaseSupport(string databasePath, int version) {
            Assert.HasText(databasePath, "databasePath");
            Assert.IsTrue(version >= 1, "Version must be >= 1, was {0}", version);

            this.DatabasePath = databasePath;
            this.newVersion = version;
        }

        public virtual T ExecuteInTransaction<T>(Func<SQLiteConnection, T> function, bool closeDatabaseOnFinish) {
            if (database == null) {
                OpenWritableDatabase();
            } else {
                CheckThread();
            }

            try {
                string savePoint = database.SaveTransactionPoint();
                T result = function(database);
                database.Release(savePoint);
                return result;

            } catch {
                try {
                    database.Rollback();
                } catch { // "cannot rollback - no transaction is active" if database is deleted and write is performed
                }
                throw;

            } finally {
                if (closeDatabaseOnFinish) {
                    Close();
                }
            }
        }

        /**
         * Create and/or open a database that will be used for reading and writing.
         * Once opened successfully, the database is cached, so you can call this
         * method every time you need to write to the database. Make sure to call
         * Close when you no longer need it.
         *
         * Errors such as bad permissions or a full disk may cause this operation
         * to fail, but future attempts may succeed if the problem is fixed.
         *
         * @return a read/write database object valid until Close is called
         */
        public virtual SQLiteConnection OpenWritableDatabase() {
            if (database != null) {
                CheckThread();
                return database;  // The database is already open for business
            }

            Assert.State(!initing, "OpenWritableDatabase called recursively");

            try {
                initing = true;
                openThreadId = O7Thread.CurrentThreadId;
                database = CreateConnection(DatabasePath);

#if UNITY_WP8 && !NETFX_CORE && !UNITY_EDITOR
                // Windows Phone 8: Set temporary files directory or database will return CannotOpen error in special cases.
                // The same issue applies to Windows Runtime (WinRT) environment, look for NETFX_CORE #ifs.
                // http://briandunnington.github.io/sqlite-winrt-bug.html
                // http://stackoverflow.com/questions/14461280/error-using-begintransaction-in-csharp-sqlite-net-library
                // The problem arises when you want to update more than one row in the same table in the same transaction.
                // SQLite then tries to create temporary journal files in an inaccessible directory.
                // Note that PRAGMA temp_store_directory is deprecated. The real solution would be a fix inside SQLite engine for WP8
                // or at least calling SQLite3.SetDirectory() or sqlite3_win32_set_directory.
                string storagePath = O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.GetInternalStoragePath();
                database.Execute(string.Format("PRAGMA temp_store_directory = '{0}'", storagePath));
#endif

                int version = Version;
                if (version != newVersion) {
                    database.RunInTransaction(delegate {
                        if (version == 0) {
                            OnCreate(database);
                        } else {
                            OnUpgrade(database, version, newVersion);
                        }
                        Version = newVersion;
                    });
                }

                OnOpen(database);
                return database;

            } catch {
                if (database != null) {
                    try {
                        database.Close();
                    } catch {
                    }
                    database = null;
                }
                openThreadId = -1;
                throw;

            } finally {
                initing = false;
            }
        }

        protected virtual SQLiteConnection CreateConnection(string databasePath) {
            return new SQLiteConnection(databasePath);
        }

        /**
         * Gets & sets the database version. The database must be open.
         */
        private int Version {
            get {
                return database.ExecuteScalar<int>("PRAGMA user_version");
            }
            set {
                database.Execute("PRAGMA user_version = " + value);
            }
        }

        /**
         * Closes the database if open.
         */
        public virtual void Close() {
            Assert.State(!initing, "Closing during initing");

            if (database == null)
                return;

            CheckThread();
            database.Close();
            database = null;
            openThreadId = -1;
        }

        private void CheckThread() {
            if (openThreadId != O7Thread.CurrentThreadId) {
                throw new InvalidOperationException("Trying to use opened database on different thread");
            }
        }

        /**
         * Called when the database is created for the first time. This is where the
         * creation of tables and the initial population of the tables should happen.
         *
         * @param db the database
         */
        public abstract void OnCreate(SQLiteConnection db);

        /**
         * Called when the database needs to be upgraded. The implementation
         * should use this method to drop tables, add tables, or do anything else it
         * needs to upgrade to the new schema version.
         *
         * The SQLite ALTER TABLE documentation can be found
         * <a href="http://sqlite.org/lang_altertable.html">here</a>. If you add new columns
         * you can use ALTER TABLE to insert them into a live table. If you rename or remove columns
         * you can use ALTER TABLE to rename the old table, then create the new table and then
         * populate the new table with the contents of the old table.
         *
         * @param db the database
         * @param oldVersion the old database version
         * @param newVersion the new database version
         */
        public abstract void OnUpgrade(SQLiteConnection db, int oldVersion, int newVersion);

        /**
         * Called when the database has been opened.
         *
         * @param db the database
         */
        public virtual void OnOpen(SQLiteConnection db) {
        }
    }
}
