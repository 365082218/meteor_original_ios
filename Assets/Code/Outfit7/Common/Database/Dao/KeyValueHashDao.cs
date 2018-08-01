//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using Outfit7.Database.Model;
using Outfit7.Util;
using SQLite;

namespace Outfit7.Database.Dao {

    /// <summary>
    /// Key-value-hash DAO.
    /// </summary>
    /// <typeparam name='E'>
    /// The type of key-value-hash entity (database transfer object - DTO).
    /// </typeparam>
    public class KeyValueHashDao<E> : DaoSupport<E, string> where E : KeyValueHashDto, new() {

        protected readonly string HashSuffix;

        public KeyValueHashDao(string hashSuffix) {
            HashSuffix = hashSuffix;
        }

        public virtual void Put(SQLiteConnection db, string key, string value) {
            E dto = new E();
            dto.Key = key;
            dto.Value = value;
            dto.Hash = CreateHash(value);
            Save(db, dto);
        }

        public virtual Pair<E, bool?> Load(SQLiteConnection db, string key, bool checkHash) {
            E dto = base.Load(db, key);
            if (dto == null)
                return null;

            bool? hashOk = null;
            if (checkHash) {
                if (!StringUtils.HasText(dto.Value)) {
                    // If value is empty or null, hash is null and irrelevant
                    hashOk = true;

                } else {
                    // Check hash
                    string hash = dto.Hash;
                    string valueH = CreateHash(dto.Value);
                    hashOk = (valueH == hash);
                }
            }

            return new Pair<E, bool?>(dto, hashOk);
        }

        protected virtual string CreateHash(string value) {
            return StringUtils.HasText(value) ? CryptoUtils.Sha1(value + HashSuffix) : null;
        }
    }
}
