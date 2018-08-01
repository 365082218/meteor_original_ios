using System;
using System.Collections;
using System.Collections.Generic;
using Outfit7.Analytics.Tracking;
using Outfit7.Common;
using Outfit7.Common.Promo;
using Outfit7.Event;
using Outfit7.Grid;
using Outfit7.Json;
using Outfit7.Threading;
using Outfit7.Util;
using Outfit7.Util.Io;
using Outfit7.Web;
using SimpleJSON;
using UnityEngine;

namespace Outfit7.Common.Promo {
    public abstract class PromoManager<PC> where PC : PromoCampaign {

        protected static string CreateCachePath(string dirName) {
            return Application.persistentDataPath + "/" + dirName + "/";
        }

        protected static void DeleteCache(string dirName) {
            try {
                O7Directory.Delete(CreateCachePath(dirName), true);

            } catch (Exception e) {
                // TODO TineL: Use Tag once not static
                O7Log.WarnT("PromoManager", e, "Cannot delete cache: {0}", CreateCachePath(dirName));
            }
        }

        private readonly int CampaignHistoryLength = BuildConfig.IsDevel ? 3 : 20;
        private readonly Dictionary<string, string> RestHeaders = new Dictionary<string, string> {
            { RestCall.Header.UserAgent, AppPlugin.UserAgentName },
        };
        private readonly TimeSpan MaxDurationOfNoLanConnectivity = BuildConfig.IsDevel ? new TimeSpan(0, 5, 0) : new TimeSpan(12, 0, 0);

        protected abstract string Tag { get; }

        protected abstract string LastLanConnectivityTimePrefKey { get; }

        protected abstract string CampaignHistoryPrefKey { get; }

        protected abstract string GridCampaignJsonKey { get; }

        protected abstract string CachePath { get; }

        protected abstract string AnalyticsName { get; }

        public EventBus EventBus { get; set; }

        public GridManager GridManager { get; set; }

        public int LastSleepingSessionIndex { get; set; }

        public PC Campaign { get; private set; }

        public MainExecutor MainExecutor { get; set; }

        public TrackingManager TrackingManager { get; set; }

        protected PromoCampaignPersister PromoCampaignPersister { get; set; }

        protected PromoUnmarshaller<PC> PromoUnmarshaller { get; set; }

        private List<PC> Campaigns { get; set; }

        private List<string> CampaignHistory { get; set; }

        private DateTime LastWifiConnectivityTime;

        private bool IsCreativeRetrieving;

        public virtual void Init() {

            if (UserPrefs.HasKey(LastLanConnectivityTimePrefKey)) {
                LastWifiConnectivityTime = UserPrefs.GetDateTime(LastLanConnectivityTimePrefKey, DateTime.UtcNow); // load last date
            } else {
                MarkWiFiConnectivity(); // setup
            }

            // load currently active
            Campaign = PromoUnmarshaller.Unmarshal(PromoCampaignPersister.LoadCampaign(), CachePath);
            O7Log.DebugT(Tag, "Loaded last campaign {0}", Campaign);

            // load campaign history
            CampaignHistory = UserPrefs.GetCollectionAsList(CampaignHistoryPrefKey, new List<string>());
            O7Log.DebugT(Tag, "Seen campaign history {0}", StringUtils.CollectionToCommaDelimitedString(CampaignHistory));

            if (GridManager.Ready) {
                OnGridChange(GridManager.JsonData);
            }
            EventBus.AddListener(CommonEvents.FRESH_GRID_DOWNLOAD, OnGridChange);
        }

        public virtual void OnAppResume() {

            if (AppPlugin.IsLanOrWiFiNetworkAvailable) { // check only every app resume
                O7Log.DebugT(Tag, "IsLanOrWiFiNetworkAvailable");
                MarkWiFiConnectivity();
            }

            if (Campaign == null) {
                return;
            }

            if (!Campaign.IsPreviewImageCached) {
                RetrieveContent(Campaign);
            }

            Campaign.OnAppResume();
        }

        protected void SaveCampaign(PC campaign) {

            if (Campaign != campaign) {
                return;
            }

            if (Campaign != null) { // save currently active
                O7Log.DebugT(Tag, "Saving active campaign {0}", Campaign);
                PromoCampaignPersister.SaveCampaign(PromoUnmarshaller.MarshalCampaign(Campaign));
            } else {
                O7Log.DebugT(Tag, "Deleting campaign data - campaign deactivated");
                PromoCampaignPersister.DeleteCampaign();
            }
        }

        private void OnGridChange(object eventData) {

            if (!GridManager.Ready) {
                return;
            }

            JSONNode gridJ = GridManager.JsonData;
            JSONArray offerJ = SimpleJsonUtils.EnsureJsonArray(gridJ["ad"][GridCampaignJsonKey]);

            Campaigns = PromoUnmarshaller.UnmarshalCampaigns(offerJ, CachePath);

            RefreshCampaigns();
        }

        private bool CampaignInGrid(PC campaign) {

            if (CollectionUtils.IsEmpty(Campaigns)) {
                return false;
            }

            for (int i = 0; i < Campaigns.Count; i++) {
                if (campaign.Id.Equals(Campaigns[i].Id)) {
                    return true;
                }
            }

            return false;
        }

        private void RefreshCampaigns() {
            O7Log.DebugT(Tag, "RefreshCampaigns");

            if (Campaign != null && Campaign.AppInstalled) { // invalidate campaign if app installed
                CampaignHistory.Add(Campaign.Id);
                Campaign = SelectNextCampaign();
            }

            if (Campaign == null || !Campaign.Valid || !CampaignInGrid(Campaign)) {
                Campaign = SelectNextCampaign();
            }

            if (Campaign != null) {
                RetrieveContent(Campaign);
            }

            O7Log.DebugT(Tag, "Active campaign {0}", Campaign);
            SaveCampaign(Campaign);
        }

        private PC SelectNextCampaign() {

            if (CollectionUtils.IsEmpty(Campaigns)) { // no campaigns
                return null;
            }

            for (int i = 0; i < Campaigns.Count; i++) {

                if (!CampaignHistory.Contains(Campaigns[i].Id)) { // was not already shown to the user

                    PC campaign = Campaigns[i];
                    campaign.Update();
                    return campaign;
                }
            }

            return null;
        }

        private void MarkWiFiConnectivity() {
            O7Log.DebugT(Tag, "MarkWiFiConnectivity");
            LastWifiConnectivityTime = DateTime.UtcNow;
            UserPrefs.SetDateTime(LastLanConnectivityTimePrefKey, LastWifiConnectivityTime);
            UserPrefs.SaveDelayed();
        }

        public virtual bool CanShow {
            get {
                if (Campaign == null) {
                    return false;
                }

                if (!AppPlugin.IsNetworkAvailable) {
                    return false;
                }

                if (!Campaign.IsPreviewImageCached) {
                    return false;
                }

                if (CampaignExpired) {
                    RefreshCampaigns();
                    return false;
                }

                if (!AppPlugin.IsLanOrWiFiNetworkAvailable) { // if no wifi check last wifi connectivity time
                    O7Log.DebugT(Tag, "No wifi available");

                    if (LastWifiConnectivityTime.Add(MaxDurationOfNoLanConnectivity) > DateTime.UtcNow) {
                        O7Log.DebugT(Tag, "Not available at the moment {0} now {1}", LastWifiConnectivityTime, DateTime.UtcNow);
                        return false; // wait for another turn
                    }
                }

                return true;
            }
        }

        private bool CampaignExpired {
            get {

                if (Campaign.WasPresentedToUser && !Campaign.Valid) { // campaign expired
                    return true;
                }

                if (Campaign.AppInstalled) {
                    return true;
                }

                return false;
            }
        }

        public virtual void WasPresented(PC campaign) {

            if (!campaign.WasPresentedToUser) { // add to history only on first impression
                O7Log.DebugT(Tag, "Adding campaign {0} to history list", campaign.Id);
                CampaignHistory.Add(campaign.Id);
                if (CampaignHistory.Count > CampaignHistoryLength) { // save only last 20 campaigns
                    CampaignHistory.RemoveAt(CampaignHistory.Count - 1);
                }
                UserPrefs.SetCollection(CampaignHistoryPrefKey, CampaignHistory);
            }

            campaign.WasPresented();
            SaveCampaign(campaign);

            if (CampaignExpired) {
                RefreshCampaigns();
            }

            TrackingManager.AddEvent(CommonTrackingEventParams.GroupId.PromoAd,
                CommonTrackingEventParams.EventId.AdShow, AnalyticsName, campaign.Id, null, null, null, null);

            MakeImpression(campaign.ImpressionUrl);
        }

        private void MakeImpression(string url) {

            if (StringUtils.IsNullOrEmpty(url)) {
                return;
            }

            RestCall call = new RestCall(url, RestHeaders);
            MainExecutor.StartCoroutine(call.Execute());
        }

        public Texture Texture(PC campaign) {
            Assert.IsTrue(campaign.IsPreviewImageCached, "creative not yet ready");

            Texture2D texture = null;
            try {
                texture = new Texture2D(4, 4, TextureFormat.RGB24, false);
                byte[] bytes = O7File.ReadAllBytes(campaign.PreviewPath);
                texture.LoadImage(bytes);
            } catch (Exception e) {
                O7Log.WarnT(Tag, "cannot read preloaded image, exception: {0}", e);
            }
            return texture;
        }

        protected virtual TimeSpan PreviewImageDownloadRetryTimeout {
            get {
                return TimeSpan.FromMinutes(1);
            }
        }

        private bool CanRetryToDownloadPreviewImage(PC campaign) {
            DateTime nextRetryTime = campaign.LastPreviewImageDownloadTry.Add(PreviewImageDownloadRetryTimeout);
            return nextRetryTime < DateTime.UtcNow;
        }

        public void RetrieveContent(PC campaign) {
            if (IsCreativeRetrieving) return;
            if (campaign.IsPreviewImageCached) return;
            if (!CanRetryToDownloadPreviewImage(campaign)) return;

            MainExecutor.StartCoroutine(RetrieveImage(campaign, delegate(bool sucess) {
                campaign.OnPreviewImageDownload(sucess);
            }));
        }

        private IEnumerator RetrieveImage(PC campaign, Action<bool> retrieveSucess) {
            IsCreativeRetrieving = true;
            O7Log.DebugT(Tag, "Image loading from {0}", campaign.PreviewUrl);

            string url = RestCall.FixUrl(campaign.PreviewUrl);
            WWW www = new WWW(url);
            yield return www;

            IsCreativeRetrieving = false;
            retrieveSucess(IsValidImageAndSaved(www, campaign.PreviewPath));
        }

        private bool IsValidImageAndSaved(WWW www, string fullPath) {

            if (!string.IsNullOrEmpty(www.error)) {
                O7Log.WarnT(Tag, "Image loading failed {0} download failed with error  {1}", www.url, www.error);
                return false;
            }

            Texture texture = www.texture;

            if (texture.height == 8 && texture.width == 8) {
                O7Log.DebugT(Tag, "Image loading failed...got dummy image {0}", www.url);
                return false;
            }

            try {
                O7Directory.CreateDirectory(CachePath);
                O7File.WriteAllBytes(fullPath, www.bytes);
                O7Log.DebugT(Tag, "Image saved to: {0}", fullPath);
            } catch (Exception e) {
                O7Log.WarnT(Tag, "Cannot save preloaded image, exception: {0}", e);
                return false;
            }

            if (texture != null) {
                O7Log.DebugT(Tag, "Image loading succeeded {0}", www.url);
                return true;
            } else {
                O7Log.DebugT(Tag, "Image loading failed {0}", www.url);
                return false;
            }
        }

        public void Open(PC campaign) {

            O7Log.DebugT(Tag, "Open campaign {0}", campaign);

            OpenOverride(campaign);

            TrackingManager.AddEvent(CommonTrackingEventParams.GroupId.PromoAd,
                CommonTrackingEventParams.EventId.AdClick, AnalyticsName, campaign.Id, null, null, null, null);
        }

        protected virtual void OpenOverride(PC campaign) {
            O7Log.DebugT(Tag, "Opening url {0}", campaign.ActionUrl);
            AppPlugin.ResolveAndOpenUrl(campaign.ActionUrl);
        }
    }
}
