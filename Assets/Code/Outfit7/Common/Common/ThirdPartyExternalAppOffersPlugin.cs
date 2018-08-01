//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using UnityEngine;
using Outfit7.Util;

namespace Outfit7.Common {

    /// <summary>
    /// 3rd-party external application offers plugin.
    /// </summary>
    public class ThirdPartyExternalAppOffersPlugin : MonoBehaviour {

        private const string Tag = "ThirdPartyExternalAppOffersPlugin";

        public ThirdPartyExternalAppOffersManager ThirdPartyExternalAppOffersManager { get; set; }

#if UNITY_EDITOR || NATIVE_SIM
        // Show/hide offer in Unity editor only
        private void OnApplicationPause(bool paused) {
            if (paused) return;

            ThirdPartyExternalAppOffersManager.ThirdPartyExternalAppOffersAvailable = !ThirdPartyExternalAppOffersManager.ThirdPartyExternalAppOffersAvailable;
        }
#endif

        public void _SetExternalAppOffersAvailable(string available) {
            O7Log.VerboseT(Tag, "_SetExternalAppOffersAvailable({0})", available);
            ThirdPartyExternalAppOffersManager.ThirdPartyExternalAppOffersAvailable = (available == "true");
        }

        public void StartShowingExternalAppOffers() {
            O7Log.VerboseT(Tag, "StartShowingExternalAppOffers()");

#if UNITY_EDITOR || NATIVE_SIM
            const string data = "{ unityeditor: \"20\" }";
            _OnExternalAppOfferCompletion(data);
#elif UNITY_IPHONE

#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("startShowingExternalAppOffers");
#elif UNITY_WP8

#endif
        }

        public void _OnExternalAppOfferCompletion(string data) {
            O7Log.VerboseT(Tag, "_OnExternalAppOfferCompletion({0})", data);
            ThirdPartyExternalAppOffersManager.OnExternalAppOfferCompletion(data);
        }
    }
}
