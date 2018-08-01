//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System.Collections.Generic;
using Outfit7.Social.Network;
using Outfit7.Util;

namespace Outfit7.Social {

    /// <summary>
    /// Social data.
    /// </summary>
    public class BasicSocialData {

        public string Id { get; private set; }

        public string FirstName { get; private set; }

        public string MiddleName { get; private set; }

        public string LastName { get; private set; }

        public SocialNetworkType? NetworkType { get; private set; }

        public List<string> FriendIds { get; private set; }

        public BasicSocialData(string id, string firstName, string middleName, string lastName,
            SocialNetworkType? networkType, ICollection<string> friendIds) {
            Id = id;
            FirstName = firstName;
            MiddleName = middleName;
            LastName = lastName;
            NetworkType = networkType;
            if (friendIds != null) {
                FriendIds = new List<string>(friendIds); // Make safe copy
            }
        }

        public override string ToString() {
            return string.Format("[BasicSocialData: Id={0}, FirstName={1}, MiddleName={2}, LastName={3}, NetworkType={4}, FriendIds={5}]",
                Id, FirstName, MiddleName, LastName, NetworkType, StringUtils.CollectionToCommaDelimitedString(FriendIds));
        }
    }
}
