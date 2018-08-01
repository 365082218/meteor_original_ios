
using System;
using System.Text;
using Outfit7.Util;

namespace Outfit7.Social.Network {

    /// <summary>
    /// Social friend.
    /// </summary>
    public class SocialFriend : IComparable<SocialFriend> {

        private string CachedDisplayName;

        public string FirstName { get; private set; }

        public string MiddleName { get; private set; }

        public string LastName { get; private set; }

        public string Id { get; private set; }

        public string ImageUrl { get; private set; }

        public bool Installed { get; private set; }

        public SocialFriend(string id, string firstName, string middleName, string lastName, string imageUrl, bool installed) {
            Assert.HasText(id, "id");
            if (!StringUtils.HasText(lastName) && !StringUtils.HasText(firstName) && !StringUtils.HasText(middleName)) {
                throw new ArgumentException("At least one of firstName, middleName or lastName must not be empty");
            }

            FirstName = firstName;
            MiddleName = middleName;
            LastName = lastName;
            Id = id;
            ImageUrl = imageUrl;
            Installed = installed;
        }

        public string DisplayName {
            get {
                if (CachedDisplayName == null) {
                    StringBuilder sb = new StringBuilder(100);

                    // Any part of name can be empty or null
                    sb.Append(FirstName);
                    if (StringUtils.HasText(MiddleName)) {
                        sb.Append(" ");
                        sb.Append(MiddleName);
                    }
                    sb.Append(" ");
                    sb.Append(LastName);

                    CachedDisplayName = sb.ToString().Trim();
                }
                return CachedDisplayName;
            }
        }

        public int CompareTo(SocialFriend b) {
            return FirstName.CompareTo(b.FirstName);
        }

        public override string ToString() {
            return string.Format("[id={0}, FirstName={1}, MiddleName={2}, LastName={3}, Installed={4}, ImageUrl={5}]",
                Id, FirstName, MiddleName, LastName, Installed, ImageUrl);
        }
    }
}
