//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using SQLite;

namespace Outfit7.Database.Model {

    /// <summary>
    /// Identity database transfer object.
    /// </summary>
    public abstract class IdentityDto {

        [Column("id"), PrimaryKey]
        public int? Id { get; set; }
    }
}
