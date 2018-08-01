//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using Outfit7.Analytics.Tracking;
using Outfit7.Util;

namespace Outfit7.Common {

    /// <summary>
    /// Age-gate manager.
    /// </summary>
    public class AgeGateManager {

        // Asked == false, Passed == false -> not shown yet or canceled
        // Asked == false, Passed == true -> not possible
        // Asked == true, Passed == false -> not passed
        // Asked == true, Passed == true -> passed

        private const string Tag = "AgeGateManager";
        private const string NotInitializedString = "Not initialized";
        private const string AgeGateAskedKey = "O7.AgeGate.Asked";
        private const string AgeGatePassedKey = "O7.AgeGate.Passed";
        private const string AgeGateBirthYearKey = "O7.AgeGate.BirthYear";
        private const int UndeterminedBirthYear = -1;
        public const int AgePassLimit = 13;

        private bool Asked;
        private bool Passed;
        private bool Initialized;
        private int BirthYear;

        public TrackingManager TrackingManager { get; set; }

        public void Init() {
            Assert.NotNull(TrackingManager, "TrackingManager");

            Asked = UserPrefs.GetBool(AgeGateAskedKey, false);
            Passed = UserPrefs.GetBool(AgeGatePassedKey, false);
            BirthYear = UserPrefs.GetInt(AgeGateBirthYearKey, UndeterminedBirthYear);

            if (Passed) { // just pass it forward to native (due to MTA-4226)
                AppPlugin.SetUserPassedAgeGate(Passed, BirthYear);
            }

            Initialized = true;

            O7Log.DebugT(Tag, "Inited; asked={0}, passed={1}, birthYear={2}", Asked, Passed, BirthYear);
        }

        public bool MustAskOrDidPass {
            get {
                return DidPass || MustAsk;
            }
        }

        public bool MustAsk {
            get {
                Assert.IsTrue(Initialized, NotInitializedString);
                return !Asked;
            }
        }

        public bool DidPass {
            get {
                Assert.IsTrue(Initialized, NotInitializedString);
                return Passed;
            }
        }

        public bool ApplyBirthYear(int birthYear) {
            Assert.IsTrue(birthYear > 0, "birthYear must be > 0");

            // Determine minimum guaranteed year.
            // If it is Jan 2017 and she was born Nov 2004, diff is 13 years but she is not 13 years old yet.
            int currentYear = DateTime.Now.Year;
            int minGuaranteedAge = currentYear - birthYear - 1;

            Asked = true;
            Passed = minGuaranteedAge >= AgePassLimit;
            BirthYear = Passed ? birthYear : UndeterminedBirthYear;

            UserPrefs.SetBool(AgeGateAskedKey, Asked);
            UserPrefs.SetBool(AgeGatePassedKey, Passed);
            UserPrefs.SetInt(AgeGateBirthYearKey, BirthYear);
            UserPrefs.SaveDelayed();

            AppPlugin.SetUserPassedAgeGate(Passed, BirthYear);

            TrackingManager.AddEvent(CommonTrackingEventParams.GroupId.AppFeatures,
                CommonTrackingEventParams.EventId.AgeGatePassed, null, Passed ? "yes" : "no", null,
                Passed ? (int?) BirthYear : null, null, null, null, true);

            return Passed;
        }

        public void OnCancel() {
        }

        public void OnDidShow() {
            TrackingManager.AddEvent(CommonTrackingEventParams.GroupId.AppFeatures,
                CommonTrackingEventParams.EventId.AgeGateShown, null, null, null, null, null, null, null, true);
        }

        public void OnPrivacyPolicyClick() {
            TrackingManager.AddEvent(CommonTrackingEventParams.GroupId.AppFeatures,
                CommonTrackingEventParams.EventId.AgeGatePrivacyPolicyClick, null, null, null, null, null, null,
                null, true);
        }
    }
}
