//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using SQLite;

namespace Outfit7.Database.Model {

    /// <summary>
    /// Key-value-hash table.
    /// </summary>
    public abstract class KeyValueHashDto {

        [Column("key"), PrimaryKey]
        public string Key { get; set; }

        [Column("value")]
        public string Value { get; set; }

        [Column("redherring")]
        public string Hash { get; set; }

        public KeyValueHashDto() {
        }

        public override string ToString() {
            return string.Format("[KeyValueHashDto: Key={0}, Value={1}, Hash={2}]", Key, Value, Hash);
        }
    }
}
