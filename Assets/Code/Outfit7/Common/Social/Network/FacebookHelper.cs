
using Outfit7.Analytics.Tracking;
using Outfit7.Util;

namespace Outfit7.Social.Network {

    /// <summary>
    /// Facebook social helper.
    /// </summary>
    public abstract class FacebookHelper : AbstractSocialHelper {

        public override string Id {
            get {
                return "Facebook";
            }
        }

        protected override string Tag {
            get {
                return "FacebookHelper";
            }
        }

        public override SocialNetworkType SocialType {
            get {
                return SocialNetworkType.Facebook;
            }
        }

        protected override string AutoLoginKey {
            get {
                return "Facebook.AutoLogin";
            }
        }

        protected override string SelectedKey {
            get {
                return "Facebook.Selected";
            }
        }

        protected override string SocialFriendDataFileName {
            get {
                return "O7FBSocialFriends.json";
            }
        }

        protected override string SocialUserDataKey {
            get {
                return "O7FBSocialUserJson";
            }
        }

#region Like

        public virtual bool CanShowLikeDialog {
            get {
                return false;
            }
        }

        public virtual void ShowLikeDialog() {
            (SocialPlugin as FacebookPlugin).ShowLikeDialog();
        }

        public virtual void Liked() {
            O7Log.DebugT(Tag, "Liked");

            BqTracker.CreateBuilder(CommonTrackingEventParams.GroupId.Social, CommonTrackingEventParams.EventId.Like, true)
                .SetP1(Id)
                .SetP2((User != null) ? User.Id : null)
                .Add();
        }

#endregion

        public virtual void ClearPrefs() {
            UserPrefs.Remove(AutoLoginKey);
            UserPrefs.Remove(SelectedKey);
            UserPrefs.Remove(SocialUserDataKey);
            SocialFriendPersister.DeleteFile(SocialFriendDataFileName);
        }
    }
}
