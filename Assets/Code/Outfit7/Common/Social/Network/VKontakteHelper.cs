
using Outfit7.Event;
using Outfit7.Purchase;
using Outfit7.Util;

namespace Outfit7.Social.Network {

    /// <summary>
    /// VKontakte social helper.
    /// </summary>
    public abstract class VKontakteHelper : AbstractSocialHelper {

        public override string Id {
            get {
                return "VKontakte";
            }
        }

        protected override string Tag {
            get {
                return "VKontakteHelper";
            }
        }

        public override SocialNetworkType SocialType {
            get {
                return SocialNetworkType.VKontakte;
            }
        }

        protected override string AutoLoginKey {
            get {
                return "VKontakte.AutoLogin";
            }
        }

        protected override string SelectedKey {
            get {
                return "VKontakte.Selected";
            }
        }

        protected override string SocialFriendDataFileName {
            get {
                return  "O7VKSocialFriends.json";
            }
        }

        protected override string SocialUserDataKey {
            get {
                return "O7VKSocialUserJson";
            }
        }

#region Subscribe

        protected virtual IapPack SubscribePurchasePack {
            get {
                return null;
            }
        }

        public virtual bool WasSubscribeRewarded {
            get {
                return true; // To prevent rewarding in SyncSubscribeReward by default, because SubscribePurchasePack is null
            }
        }

        public virtual void SyncSubscribeReward() {
            O7Log.DebugT(Tag, "SyncSubscribeReward");

            if (!WasSubscribeRewarded) {
                PurchaseManager.RewardPurchase(SubscribePurchasePack, true);
            }
        }

        public virtual bool CanSubscribe {
            get {
                return Available;
            }
        }

        public virtual void Subscribe() {
            if (SocialPlugin.LoggedIn) {
                (SocialPlugin as VKontaktePlugin).Subscribe();
                return;
            }

            OnLoginAction = delegate {
                OnLoginAction = null;
                if (SocialPlugin.LoggedIn) {
                    (SocialPlugin as VKontaktePlugin).Subscribe();
                } else {
                    SubscribeFailed();
                }
            };
            LogIn();
        }

        public virtual void SubscribeCompleted() {
            O7Log.DebugT(Tag, "SubscribeCompleted");

            SyncSubscribeReward();
            EventBus.FireEvent(CommonEvents.SOCIAL_SUBSCRIBE);
        }

        public virtual void SubscribeFailed() {
            O7Log.DebugT(Tag, "SubscribeFailed");
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
