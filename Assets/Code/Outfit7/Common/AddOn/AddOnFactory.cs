//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using Outfit7.Util;
using SimpleJSON;

namespace Outfit7.AddOn {

    /// <summary>
    /// Add-on factory interface.
    /// </summary>
    public interface AddOnFactory {

        AddOnItem CreateDevelAddOn(string id, int position);

        IDictionary<string, AddOnItem> UnmarshalBundledAddOns();

        IEnumerator<Null> UnmarshalCachedAddOns(JSONNode dataJ, Action<IDictionary<string, AddOnItem>> callback);

        void UpdateAddOn(AddOnItem bundledItem, AddOnItem gridItem);
    }
}
