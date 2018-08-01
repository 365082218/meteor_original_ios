//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System.Collections.Generic;
using SQLite;

namespace Outfit7.Database.Dao {

    /// <summary>
    /// Database access object support.
    /// </summary>
    /// <typeparam name='E'>
    /// The type of entity (database transfer object - DTO).
    /// </typeparam>
    /// <typeparam name='K'>
    /// The type of the primary key of the entity (database transfer object - DTO). Usually int?.
    /// </typeparam>
    public class DaoSupport<E, K> where E : new() {

        /// <summary>
        /// Counts all elements using the 'count' SQL query.
        /// </summary>
        /// <returns>A number of elements (always &gt;= 0).</returns>
        /// <param name="db">The database connection (null not permitted).</param>
        public virtual int CountAll(SQLiteConnection db) {
            return db.Table<E>().Count();
        }

        /// <summary>
        /// Load the element with the specified ID (as primary key).
        /// </summary>
        /// <returns>A found element or null if an element with the specified ID does not exist.</returns>
        /// <param name="db">The database connection (null not permitted).</param>
        /// <param name="id">The element's ID (null not permitted).</param>
        public virtual E Load(SQLiteConnection db, K id) {
            return db.Find<E>(id);
        }

        /// <summary>
        /// Loads all elements into the list.
        /// </summary>
        /// <returns>A list of all elements (null not possible).</returns>
        /// <param name="db">The database connection (null not permitted).</param>
        public virtual List<E> LoadAll(SQLiteConnection db) {
            return new List<E>(db.Table<E>());
        }

        /// <summary>
        /// Saves (puts or updates) the given element (database transfer object) in the table.
        /// </summary>
        /// <returns>True if the element is put or updated in the table or False if the table is not modified.</returns>
        /// <param name="db">The database connection (null not permitted).</param>
        /// <param name="dto">The element to save (null not permitted).</param>
        public virtual bool Save(SQLiteConnection db, E dto) {
            int modifiedRowCount = db.InsertOrReplace(dto);
            return modifiedRowCount > 0;
        }

        /// <summary>
        /// Deletes the element with the specified ID (as primary key).
        /// </summary>
        /// <returns>True if the element is deleted from the table or False if the table was not modified.</returns>
        /// <param name="db">The database connection (null not permitted).</param>
        /// <param name="id">The element's ID (null not permitted).</param>
        public virtual bool Delete(SQLiteConnection db, K id) {
            int deletedRowCount = db.Delete<E>(id);
            return deletedRowCount > 0;
        }

        /// <summary>
        /// Deletes all elements.
        /// </summary>
        /// <returns>A number of deleted elements.</returns>
        /// <param name="db">The database connection (null not permitted).</param>
        public virtual int DeleteAll(SQLiteConnection db) {
            return db.DeleteAll<E>();
        }
    }
}
