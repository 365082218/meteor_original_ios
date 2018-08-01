
using System.Collections.Generic;
using Outfit7.Event;
using Outfit7.Util;

namespace Outfit7.Social.Network {

    /// <summary>
    /// Social network.
    /// </summary>
    public class SocialNetwork {

        protected const string Tag = "SocialNetwork";

        protected List<AbstractSocialHelper> SocialHelpers { get; set; }

        public AbstractSocialHelper SocialHelper { get; protected set; }

        public EventBus EventBus { get; set; }

        public SocialNetwork() {
            SocialHelpers = new List<AbstractSocialHelper>();
        }

        public virtual void Init() {
            AbstractSocialHelper prevSelectedSocialHelper = FindSelectedSocialNetwork();
            O7Log.VerboseT(Tag, "Previously selected social network: {0}", (prevSelectedSocialHelper == null) ? "none" : prevSelectedSocialHelper.Id);
            UpdateSelectedSocialNetwork(prevSelectedSocialHelper);
            EventBus.AddListener(CommonEvents.SOCIAL_LOGIN_START, OnSocialLoginStart);
        }

        protected virtual void OnSocialLoginStart(object eventData) {
            UpdateSelectedSocialNetwork(eventData as AbstractSocialHelper);
        }

        public virtual void RegisterSocialHelper(AbstractSocialHelper helper) {
            SocialHelpers.Add(helper);
        }

        protected virtual AbstractSocialHelper FindSelectedSocialNetwork() {
            for (int i = 0; i < SocialHelpers.Count; i++) {
                AbstractSocialHelper helper = SocialHelpers[i];
                if (helper.SelectedNetwork) return helper;
            }
            return null;
        }

        protected virtual void UpdateSelectedSocialNetwork(AbstractSocialHelper selectedSocialHelper) {
            if (SocialHelpers.Count == 0) {
                O7Log.WarnT(Tag, "No social helpers registered");
                return;
            }

            AbstractSocialHelper newSocialHelper = selectedSocialHelper ?? SocialHelpers[0]; // First is default
            if (!SocialHelpers.Contains(newSocialHelper)) {
                O7Log.WarnT(Tag, "Social helper '{0}' not registered", newSocialHelper.Id);
                return;
            }

            AbstractSocialHelper oldSocialHelper = SocialHelper;
            if (newSocialHelper == oldSocialHelper) return;

            // First set new social helper, then log out old one (avoids cyclic dependency)
            SocialHelper = newSocialHelper;

            O7Log.DebugT(Tag, "New social network selected: {0}", (newSocialHelper == null) ? "none" : newSocialHelper.Id);

            if (oldSocialHelper == null) return;

            O7Log.DebugT(Tag, "Logging out from old social network: {0}", oldSocialHelper.Id);
            oldSocialHelper.LogOut();
        }
    }
}
