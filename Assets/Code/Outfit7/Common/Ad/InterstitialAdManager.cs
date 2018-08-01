//
//   Copyright (c) 2017 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Outfit7.Analytics.Tracking;
using Outfit7.Event;
using Outfit7.Grid;
using Outfit7.Json;
using Outfit7.Util;
using SimpleJSON;
using UnityEngine;
namespace Outfit7.Ad {

    /// <summary>
    /// Interstitial ad manager.
    /// </summary>
    public class InterstitialAdManager {

        protected const string Tag = "InterstitialAdManager";
        public const int ShowInterstitialLevel = 6;
        protected const string CanStartShowingInterstitialsKey = "AdManager.CanStartShowingInterstitials";
        protected const string TicksSpentInAppKey = "AdManager.TimeSpentInApp";

        protected const int DefaultFirstInterstitialPrepareSkippedSessions = 2;
        // 5 minutes
        protected const int DefaultFirstInterstitialPrepareTimeout = 5 * 60;

        protected static readonly int[] DefaultInterstitialPrepareTimeouts = { 20, 120 };
        protected static readonly int[] DefaultInterstitialShowTimeouts = { 20, 120 };

        protected bool interstitialReady;
        protected bool preferPostitialOverInterstitial;

        // After this is set interstitial can be shown / defaults to true after 5 min in the app or 2 app sessions done
        protected bool CanStartPreparingInterstitials;

        // Num of initial sessions before interstitials start to show
        protected int FirstInterstitialPrepareSkippedSessions;
        // First interstitial timeout in seconds
        protected int FirstInterstitialPrepareTimeout;
        protected List<int> InterstitialPrepareTimeouts;
        protected float NextInterstitialPrepareTime;
        protected int InterstitialPrepareTimeoutIndex;

        protected List<int> InterstitialShowTimeouts;
        protected float NextInterstitialShowTime;
        protected int InterstitialShowTimeoutIndex;
        protected long TicksSpentInAppInPreviousLiveStates;

        protected string LastInterstitialOppurtunityMissReason;

        protected Stopwatch InitStopWatch;
        protected Stopwatch ResumeStopWatch;

        protected bool InterstitialShown;
        protected bool InterstitialPreloading;

#if DEVEL_BUILD || UNITY_EDITOR || PROD_BUILD
        protected float DebugNextAdTimeout;
#endif

        protected Dictionary<string, AdO7Interstitial> O7Interstitials;

        public virtual int InterstitialAdAppSessionId { get; protected set; }

        public virtual bool IgnoreAdsTimeouts { get; protected internal set; }

        public virtual bool PostitialReady { protected get; set; }

        public List<KeyValuePair<string, string>> AllowedTransitions { get; protected set; }

        public AdPlugin AdPlugin { get; set; }

        public EventBus EventBus { get; set; }

        public AppSession AppSession { get; set; }

        public GridManager GridManager { get; set; }

        public AdManager AdManager { get; set; }

        public TrackingManager TrackingManager { get; set; }

        protected virtual float TimeFromAppStart {
            get { return InitStopWatch.ElapsedMilliseconds / 1000f; }
        }

        protected virtual float TimeFromAppResume {
            get { return ResumeStopWatch.ElapsedMilliseconds / 1000f; }
        }

        public virtual bool InterstitialReady {
            get {
                if (!AdManager.AdsEnabled) return false;
                return interstitialReady;
            }
            internal set {
                if (value) {
                    InterstitialPreloading = false;
                }
                interstitialReady = value;
            }
        }

        public virtual bool PreferPostitialOverInterstitial {
            get {
                return preferPostitialOverInterstitial && PostitialReady;
            }
            protected set {
                if (value == preferPostitialOverInterstitial) return;
                preferPostitialOverInterstitial = value;
                O7Log.VerboseT(Tag, "PreferPostitialOverInterstitial({0})", preferPostitialOverInterstitial);
            }
        }

        /// <summary>
        /// Has to be called after AppSession init.
        /// </summary>
        public virtual void Init() {
            InterstitialAdAppSessionId = -1;
            O7Interstitials = new Dictionary<string, AdO7Interstitial>();
            InitStopWatch = Stopwatch.StartNew();
            ResumeStopWatch = Stopwatch.StartNew();

            CanStartPreparingInterstitials = UserPrefs.GetBool(CanStartShowingInterstitialsKey, false);

            FirstInterstitialPrepareSkippedSessions = DefaultFirstInterstitialPrepareSkippedSessions;
            FirstInterstitialPrepareTimeout = DefaultFirstInterstitialPrepareTimeout;
            InterstitialPrepareTimeouts = new List<int>(DefaultInterstitialPrepareTimeouts);
            InterstitialShowTimeouts = new List<int>(DefaultInterstitialShowTimeouts);
            AllowedTransitions = PrepareDefaultInterstitialTransitions();

            if (GridManager.Ready) {
                UpdateGridData(GridManager.JsonData);
            }

            InterstitialPrepareTimeoutIndex = 0;
            NextInterstitialPrepareTime = TimeFromAppStart + InterstitialPrepareTimeouts[InterstitialPrepareTimeoutIndex];

            InterstitialShowTimeoutIndex = 0;
            NextInterstitialShowTime = TimeFromAppStart + InterstitialShowTimeouts[InterstitialShowTimeoutIndex];

            TicksSpentInAppInPreviousLiveStates = UserPrefs.GetLong(TicksSpentInAppKey, 0);

            EventBus.AddListener(CommonEvents.FRESH_GRID_DOWNLOAD, delegate(object eventData) {
                UpdateGridData((JSONNode) eventData);
            });

            EventBus.AddListener(CommonEvents.NEW_SESSION, OnNewSession);

            O7Log.DebugT(Tag, "Inited; adsEnabled={0}, preferPostitialOverInterstitial={1}, canStartPreparingInterstitials={2}, firstInterstitialPrepareSkippedSessions={3}, firstInterstitialPrepareTimeout={4}, interstitialPrepareTimeouts={5}, nextInterstitialPrepareTime={6}, interstitialShowTimeouts={7}, nextInterstitialShowTime={8}, timeSpentInAppInPreviousLiveStates={9}",
                AdManager.AdsEnabled, PreferPostitialOverInterstitial, CanStartPreparingInterstitials,
                FirstInterstitialPrepareSkippedSessions, FirstInterstitialPrepareTimeout,
                StringUtils.CollectionToCommaDelimitedString(InterstitialPrepareTimeouts),
                NextInterstitialPrepareTime, StringUtils.CollectionToCommaDelimitedString(InterstitialShowTimeouts),
                NextInterstitialShowTime, TimeSpan.FromTicks(TicksSpentInAppInPreviousLiveStates));
        }

        public virtual bool QuitWithPostitial() {
            return AdPlugin.QuitWithPostitial();
        }

        public virtual void PrepareInterstitial() {
            O7Log.DebugT(Tag, "PrepareInterstitial: adsEnabled={0} interstitialReady={1}", AdManager.AdsEnabled,
                InterstitialReady);

            if (InterstitialReady)
            {
#if !STRIP_LOGS
                UnityEngine.Debug.Log("当前插屏广告已经准备好，不再取");
#endif
                return;
            }

            AdPlugin.PrepareInterstitialAd();
            InterstitialPreloading = true;
        }

        protected virtual void UpdateGridData(JSONNode gridJ) {
            UpdatePreferPostitialOverInterstitial(gridJ);
            UpdateInterstitialShowTimeouts(gridJ);
            UpdateInterstitialPreloadTimeouts(gridJ);
            UpdateFirstInterstitialPrepareConditions(gridJ);
            UpdateTransitionsAllowingInterstitials(gridJ);
            UpdateO7Interstitials(gridJ);
        }

        public virtual void ShowInterstitial() {
            O7Log.VerboseT(Tag, "ShowInterstitial()");
            AdPlugin.StartShowingInterstitialAd();
            InterstitialAdAppSessionId = AppSession.SessionId;
            AdvanceInterstitialTimeout();
            ResetInterstitialOpportunity();
            InterstitialShown = true;
            InterstitialReady = false;
        }

        public virtual void HideInterstitial() {
            O7Log.VerboseT(Tag, "HideInterstitial()");
            InterstitialShown = false;
            // TODO has to be united on native side
            InterstitialReady = false; // On Android this is currently never set to false automatically, on iOS it gets set to false when an interstitial doesn't show (error) and after it's shown

            InterstitialPrepareTimeoutIndex = Mathf.Min(InterstitialPrepareTimeouts.Count - 1, InterstitialPrepareTimeoutIndex + 1);
            NextInterstitialPrepareTime = TimeFromAppStart + InterstitialPrepareTimeouts[InterstitialPrepareTimeoutIndex];
        }

        public virtual void ReportInterstitialOpportunityMissReason(string source, string reason) {
            Assert.HasText(reason, "reason");
            Assert.HasText(source, "source");

            if (reason == LastInterstitialOppurtunityMissReason) return;

            LastInterstitialOppurtunityMissReason = reason;

            TrackingManager.AddEvent(CommonTrackingEventParams.GroupId.Ads,
                CommonTrackingEventParams.EventId.InterstitialShowOpportunityFail, source, reason, null, null, null, null);
        }

        protected virtual void ResetInterstitialOpportunity() {
            LastInterstitialOppurtunityMissReason = null;
        }

        public virtual bool CanShowTimeoutedInterstitialAd(string currentStateName, string previousStateName = null) {
            if (!AdManager.AdsEnabled) return false;
            if (IgnoreAdsTimeouts) return InterstitialReady;

            bool canShow = TimeFromAppStart > NextInterstitialShowTime;
            O7Log.DebugT(Tag, "CanShowTimeoutedInterstitialAd={0}, TimeFromAppStart={1} > NextInterstitialShowTime={2}, interstitial ready on native side? {3}",
                canShow, InitStopWatch.Elapsed, NextInterstitialShowTime, InterstitialReady);

            if (!canShow) {
                ReportInterstitialOpportunityMissReason(currentStateName, "time-out");
                return false;
            }

            if (!InterstitialReady) {
                ReportInterstitialOpportunityMissReason(currentStateName, "not-ready");
                return false;
            }

            bool transitionAllowed = false;
            for (int i = 0; i < AllowedTransitions.Count; i++) {
                KeyValuePair<string, string> transition = AllowedTransitions[i];
                if (DoStateNamesMatch(transition.Key, previousStateName) && DoStateNamesMatch(transition.Value, currentStateName)) {
                    transitionAllowed = true;
                }
            }
            O7Log.DebugT(Tag, "CanShowTimeoutedInterstitialAd={0}, Transition: {1} > {2}", transitionAllowed, previousStateName, currentStateName);

            return transitionAllowed;
        }

        protected virtual void UpdatePreferPostitialOverInterstitial(JSONNode json) {
            PreferPostitialOverInterstitial = json["ad"]["preferPostitialOverInterstitial"].AsBool;
        }

        protected virtual void UpdateInterstitialShowTimeouts(JSONNode json) {
            JSONArray timeoutsJ = SimpleJsonUtils.EnsureJsonArray(json["ad"]["aC"]["iTs"]);
            if (timeoutsJ == null) return;

            O7Log.VerboseT(Tag, "UpdateInterstitialShowTimeouts: {0}", timeoutsJ.ToString());

            List<int> newTimeouts = new List<int>(timeoutsJ.Count);
            bool validConfig = true;
            foreach (JSONNode timeoutJ in timeoutsJ) {
                int timeout = timeoutJ.AsInt;
                if (timeout >= 0) {
                    newTimeouts.Add(timeout);
                } else {
                    validConfig = false;
                    break;
                }
            }

            validConfig = validConfig && newTimeouts.Count > 0;

            if (validConfig) {
                InterstitialShowTimeouts = newTimeouts;
            } else {
                O7Log.WarnT(Tag, "UpdateInterstitialShowTimeouts - invalid config: {0}", timeoutsJ.ToString());
            }
        }

        protected virtual void UpdateInterstitialPreloadTimeouts(JSONNode json) {
            JSONArray timeoutsJ = SimpleJsonUtils.EnsureJsonArray(json["ad"]["aC"]["iPTs"]);
            if (timeoutsJ == null) return;

            O7Log.VerboseT(Tag, "UpdateInterstitialPreloadTimeouts: {0}", timeoutsJ.ToString());

            List<int> newTimeouts = new List<int>(timeoutsJ.Count);
            bool validConfig = true;
            foreach (JSONNode timeoutJ in timeoutsJ) {
                int timeout = timeoutJ.AsInt;
                if (timeout >= 0) {
                    newTimeouts.Add(timeout);
                } else {
                    validConfig = false;
                    break;
                }
            }

            validConfig = validConfig && newTimeouts.Count > 0;

            if (validConfig) {
                InterstitialPrepareTimeouts = newTimeouts;
            } else {
                O7Log.WarnT(Tag, "UpdateInterstitialPreloadTimeouts - invalid config: {0}", timeoutsJ.ToString());
            }
        }

        protected virtual void UpdateFirstInterstitialPrepareConditions(JSONNode json) {
            FirstInterstitialPrepareTimeout = DefaultFirstInterstitialPrepareTimeout;
            FirstInterstitialPrepareSkippedSessions = DefaultFirstInterstitialPrepareSkippedSessions;

            JSONNode timeoutJ = json["ad"]["aC"]["fIPT"];
            if (timeoutJ == null) return;
            JSONNode sessionsJ = json["ad"]["aC"]["fIPSS"];
            if (sessionsJ == null) return;

            FirstInterstitialPrepareTimeout = timeoutJ.AsInt;
            FirstInterstitialPrepareSkippedSessions = sessionsJ.AsInt;

            O7Log.DebugT(Tag, "UpdateFirstInterstitialPrepareConditions FirstInterstitialPrepareTimeout={0}, FirstInterstitialPrepareSkippedSessions={1}",
                FirstInterstitialPrepareTimeout, FirstInterstitialPrepareSkippedSessions);
        }

        protected virtual void UpdateTransitionsAllowingInterstitials(JSONNode json) {
            JSONArray transitions = SimpleJsonUtils.EnsureJsonArray(json["ad"]["aC"]["iSTs"]);
            if (transitions == null) return;

            AllowedTransitions.Clear();
            for (int i = 0; i < transitions.Count; i++) {
                JSONNode transitionJ = transitions[i];
                string fromStateName = transitionJ["f"].Value;
                string toStateName = transitionJ["t"].Value;
                AllowedTransitions.Add(new KeyValuePair<string, string>(fromStateName, toStateName));
            }
        }

        public virtual void OnUpdate() {
            if (!AdManager.AdsEnabled) return;

            // Don't do anything if interstitial is shown / native can't preload an interstitial if it's being shown at this time
            if (InterstitialShown) return;
#if DEVEL_BUILD || UNITY_EDITOR || PROD_BUILD
            // Spam prepare interstitial for testing purposes
            if (IgnoreAdsTimeouts && DebugNextAdTimeout < Time.time) {
                PrepareInterstitial();
                DebugNextAdTimeout = Time.time + 5.0f;
                return;
            }
#endif

            if (!CanStartPreparingInterstitials) {
                double timeSpentInPreviousLiveStates = TimeSpan.FromTicks(TicksSpentInAppInPreviousLiveStates).TotalSeconds;
                double timeSpentInApp = timeSpentInPreviousLiveStates + TimeFromAppResume;
                if (AppSession.SessionId >= FirstInterstitialPrepareSkippedSessions || timeSpentInApp >= FirstInterstitialPrepareTimeout) {
                    O7Log.DebugT(Tag, "Preparing interstitial for the first time - previous live states[{0}s] + timeFromAppResume[{1}s] = timeSpentInApp[{2}s] / Session ID = {3}",
                        timeSpentInPreviousLiveStates, TimeFromAppResume, timeSpentInApp, AppSession.SessionId);
                    PrepareInterstitial();
                    NextInterstitialPrepareTime = float.MaxValue;
                    CanStartPreparingInterstitials = true;
                }
                return;
            }

            if (TimeFromAppStart >= NextInterstitialPrepareTime) {
                PrepareInterstitial();
                NextInterstitialPrepareTime = float.MaxValue;
            }
        }

        public virtual void OnAppPause() {
            TicksSpentInAppInPreviousLiveStates += ResumeStopWatch.ElapsedTicks;

            O7Log.DebugT(Tag, "OnAppPause InitStopWatch={0}, ResumeStopWatch={1}, TimeSpentInPreviousLiveStates={2}",
                InitStopWatch.Elapsed, ResumeStopWatch.Elapsed, TimeSpan.FromTicks(TicksSpentInAppInPreviousLiveStates));

            InitStopWatch.Stop();
            ResumeStopWatch.Reset();

            UserPrefs.SetBool(CanStartShowingInterstitialsKey, CanStartPreparingInterstitials);
            UserPrefs.SetLong(TicksSpentInAppKey, TicksSpentInAppInPreviousLiveStates);
            UserPrefs.SaveDelayed();
        }

        public virtual void OnAppResume() {
            O7Log.DebugT(Tag, "OnAppResume InitStopWatch={0}", InitStopWatch.Elapsed);

            // We need to prefetch again after Unity is paused since native stops prefetching on every app pause. Don't worry if native was not really paused; MTT-12734
            if (InterstitialPreloading) {
                PrepareInterstitial();
                return;
            }

            InitStopWatch.Start();
            ResumeStopWatch.Start();
        }

        protected virtual void OnNewSession(object data) {
            InterstitialShowTimeoutIndex = 0;
            NextInterstitialShowTime = TimeFromAppStart + InterstitialShowTimeouts[InterstitialShowTimeoutIndex];

            InterstitialPrepareTimeoutIndex = 0;
            NextInterstitialPrepareTime = TimeFromAppStart + InterstitialPrepareTimeouts[InterstitialPrepareTimeoutIndex];

            O7Log.DebugT(Tag, "OnNewSession time from start: {0}, show timeout index: {1}, prepare timeout index: {2}",
                InitStopWatch.Elapsed, InterstitialShowTimeouts[InterstitialShowTimeoutIndex],
                InterstitialPrepareTimeouts[InterstitialPrepareTimeoutIndex]);
        }

        protected virtual void AdvanceInterstitialTimeout() {
            InterstitialShowTimeoutIndex = Math.Min(InterstitialShowTimeoutIndex + 1, InterstitialShowTimeouts.Count - 1);
            NextInterstitialShowTime = TimeFromAppStart + InterstitialShowTimeouts[InterstitialShowTimeoutIndex];
        }

        protected virtual bool DoStateNamesMatch(string configStateName, string stateName) {
            if (!StringUtils.HasText(configStateName) && !StringUtils.HasText(stateName)) {
                return true;
            }
            return configStateName == stateName;
        }

        protected virtual List<KeyValuePair<string, string>> PrepareDefaultInterstitialTransitions() {
            return new List<KeyValuePair<string, string>>();
        }

#region O7Interstitials

        protected virtual void UpdateO7Interstitials(JSONNode json) {
            Dictionary<string,AdO7Interstitial> o7Interstitials = AdO7InterstitialUnmarshaller.Unmarshall(json["ad"]["o7Interstitials"]);

            if (o7Interstitials.Count == 0) { // no interstitials clear list
                O7Interstitials.Clear();
                return;
            }

            foreach (KeyValuePair<string,AdO7Interstitial> o7InterstitialPair in o7Interstitials) {
                if (O7Interstitials.ContainsKey(o7InterstitialPair.Key)) { // update new ones with some props of old ones
                    o7Interstitials[o7InterstitialPair.Key].UpdateWithOld(O7Interstitials[o7InterstitialPair.Key]);
                }
            }

            O7Interstitials = o7Interstitials;
        }

        protected virtual bool IsO7InterstitialShowPossible(AdO7Interstitial interstitial, int showCount) {
            if (interstitial.Ready) {
                O7Log.DebugT(Tag, "IsO7InterstitialShowPossible o7interstitial already ready");
                return true;
            }

            // check if can be showed in predefined sequence
            int sequence = 0;
            for (int i = 0; i < interstitial.Sequence.Count; i++) {
                sequence += interstitial.Sequence[i];
                if (sequence == showCount) {
                    O7Log.DebugT(Tag, "IsO7InterstitialShowPossible o7interstitial sequence trigger {0}", sequence);
                    return true;
                }
                if (showCount < sequence) {
                    O7Log.DebugT(Tag, "IsO7InterstitialShowPossible not enough interstitials shown (show count {0}, sequence {1})",
                        showCount, sequence);
                    return false;
                }
            }

            // repeat each last sequence
            showCount -= sequence; // reduce by accumulated sequence

            // repeat last sequence with help of mod
            bool canShow = (showCount % interstitial.Sequence[interstitial.Sequence.Count - 1]) == 0;

            O7Log.DebugT(Tag, "IsO7InterstitialShowPossible {0} via mod (show count {1}, sequence {2})", canShow,
                showCount, sequence);

            return canShow;
        }

        public virtual bool CanShowO7Interstitial(string channel, int showCount) {
            if (!AdManager.AdsEnabled) return false;

            if (!O7Interstitials.ContainsKey(channel)) {
                O7Log.DebugT(Tag, "CanShowO7Interstitial channel {0} not found", channel);
                return false;
            }

            AdO7Interstitial interstitial = O7Interstitials[channel];

            // start precaching - look ahead
            if (!interstitial.Ready && IsO7InterstitialShowPossible(interstitial, showCount + 1)) {
                AdPlugin.PrepareO7InterstitialAd(interstitial.Url, interstitial.Channel);
                return false;
            }

            if (!interstitial.Ready) {
                O7Log.DebugT(Tag, "CanShowO7Interstitial interstitial for channel {0} not ready", channel);
                return false;
            }

            if (interstitial.PerSession > 0) { // limit interstitials per session
                // update session and reset counter if new
                if (interstitial.LastSessionShown != InterstitialAdAppSessionId) {
                    interstitial.LastSessionShownCount = 0;
                    interstitial.LastSessionShown = InterstitialAdAppSessionId;
                }

                if (interstitial.LastSessionShownCount >= interstitial.PerSession) { // over the limit
                    O7Log.DebugT(Tag, "CanShowO7Interstitial over the session display limit: {0}",
                        interstitial.PerSession);
                    return false;
                }
            }

            return IsO7InterstitialShowPossible(interstitial, showCount);
        }

        public virtual void ShowO7InterstitialAd(string channel) {
            if (O7Interstitials.ContainsKey(channel)) {
                AdO7Interstitial interstitial = O7Interstitials[channel];
                interstitial.LastSessionShownCount++;
            }
            AdvanceInterstitialTimeout();
            AdPlugin.OpenO7InterstitialAd(channel);
        }

        public virtual void SetO7InterstitialAdReady(string jsonData) {
            O7Log.VerboseT(Tag, "SetO7InterstitialAdReady({0})", jsonData);
            JSONNode interstitialJ;
            try {
                interstitialJ = JSON.Parse(jsonData);

                bool ready = interstitialJ["ready"].AsBool;
                string channel = interstitialJ["id"];

                if (!StringUtils.IsNullOrEmpty(channel)) {
                    if (O7Interstitials.ContainsKey(channel)) {
                        O7Interstitials[channel].Ready = ready;
                    }
                }

            } catch (Exception e) {
                O7Log.WarnT(Tag, e, "Cannot parse SetO7InterstitialAdReady data");
            }
        }

        #endregion
        public virtual void TryOpenInterstitialAd()
        {
        }
    }
}
