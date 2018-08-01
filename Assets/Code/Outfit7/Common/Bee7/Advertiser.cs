//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using Outfit7.Event;
using Outfit7.Util;

namespace Outfit7.Bee7 {

    /// <summary>
    /// Bee7 Advertiser.
    /// </summary>
    public class Advertiser {

        private const string Tag = "Advertiser";

        public Reward VirtualReward { get; private set; }

        /// <value><c>true</c> if Advertiser is enabled &amp; rewarded session has been started once.</value>
        public bool RewardedSessionStarted { get; private set; }

        public EventBus EventBus { get; set; }

        public AdvertiserPlugin AdvertiserPlugin { get; set; }

        public bool Enabled {
            get {
                return VirtualReward != null;
            }
        }

        public virtual void Init() {
            O7Log.DebugT(Tag, "Init");

            AdvertiserPlugin.StartAdvertiser();
        }

        public virtual void StartOrResumeRewardedSession() {
            if (!Enabled)
                return;

            AdvertiserPlugin.StartOrResumeRewardedSession();
            RewardedSessionStarted = true;
        }

        public virtual void PauseRewardedSession() {
            if (!Enabled)
                return;

            AdvertiserPlugin.PauseRewardedSession();
        }

        public virtual void EndRewardedSession(int points) {
            Assert.State(RewardedSessionStarted, "RewardedSessionStarted must be true");

            AdvertiserPlugin.EndRewardedSession(points);
            RewardedSessionStarted = false;
        }

        public virtual void EndRewardedSession(int points, Action<Reward> rewardAction) {
            Assert.State(RewardedSessionStarted, "RewardedSessionStarted must be true");

            AdvertiserPlugin.EndRewardedSession(points);
            RewardedSessionStarted = false;

            GetAccumulatedReward(rewardAction);
        }

        public virtual void ClearReward() {
            if (!Enabled)
                return;

            AdvertiserPlugin.ClearReward();
            RewardedSessionStarted = false;
        }

        private Action<Reward> AccuRewardAction;

        public virtual void GetAccumulatedReward(Action<Reward> rewardAction) {
            Assert.NotNull(rewardAction, "rewardAction");
            AccuRewardAction = rewardAction;
            AdvertiserPlugin.StartGettingAccumulatedReward();
        }

        internal virtual void OnAccumulatedRewardGet(Reward reward) {
            if (AccuRewardAction != null) {
                AccuRewardAction(reward);
            }
            AccuRewardAction = null;
        }

        private Action<Reward> VirtualRewardAction;

        public virtual void GetVirtualReward(int points, Action<Reward> rewardAction) {
            Assert.NotNull(rewardAction, "rewardAction");
            VirtualRewardAction = rewardAction;
            AdvertiserPlugin.StartGettingVirtualReward(points);
        }

        internal virtual void OnVirtualRewardGet(Reward reward) {
            VirtualRewardAction(reward);
            VirtualRewardAction = null;
        }

        public virtual void StartClaimingReward() {
            if (!Enabled)
                return;

            AdvertiserPlugin.ClaimReward();
        }

        internal virtual void OnEnableChange(Reward virtualReward) {
            VirtualReward = virtualReward;
            RewardedSessionStarted = false;
            EventBus.FireEvent(CommonEvents.BEE7_ADVERTISER_ENABLED, Enabled);
        }
    }
}
