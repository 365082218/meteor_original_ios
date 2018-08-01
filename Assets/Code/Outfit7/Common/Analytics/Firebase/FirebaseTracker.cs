//
//   Copyright (c) 2016 Outfit7. All rights reserved.
//

using Outfit7.Util;
using System.Collections.Generic;

namespace Outfit7.Analytics.Firebase {

    public class FirebaseTracker {

        public FirebasePlugin FirebasePlugin { get; set; }

        public AnalyticsPlugin AnalyticsPlugin { get; set; }

        public bool IsDisabled { get; set; }

        public virtual void LogEvent(string eventName, IDictionary<string, string> parameters) {
            if (IsDisabled) return;
            if (!AnalyticsPlugin.IsEnabled) return;
            FirebasePlugin.LogEvent(eventName, parameters);
        }

        public virtual void LogSelectContentEvent(string contentType, string itemId) {
            if (IsDisabled) return;
            if (!AnalyticsPlugin.IsEnabled) return;
            Dictionary<string, string> parameters = new Dictionary<string, string>(2);
            parameters.Add("content_type", contentType);
            parameters.Add("item_id", itemId);
            FirebasePlugin.LogEvent("select_content", parameters);
        }

        public virtual void LogSpendCurrencyEvent(string itemName, string currencyName, int amount) {
            if (IsDisabled) return;
            if (!AnalyticsPlugin.IsEnabled) return;
            Dictionary<string, string> parameters = new Dictionary<string, string>(3);
            parameters.Add("item_name", itemName);
            parameters.Add("virtual_currency_name", currencyName);
            parameters.Add("value", StringUtils.ToUniString(amount));
            FirebasePlugin.LogEvent("spend_virtual_currency", parameters);
        }

        public virtual void LogEarnCurrencyEvent(string itemName, string currencyName, int amount) {
            if (IsDisabled) return;
            if (!AnalyticsPlugin.IsEnabled) return;
            Dictionary<string, string> parameters = new Dictionary<string, string>(3);
            parameters.Add("item_name", itemName);
            parameters.Add("virtual_currency_name", currencyName);
            parameters.Add("value", StringUtils.ToUniString(amount));
            FirebasePlugin.LogEvent("earn_virtual_currency", parameters);
        }
    }
}