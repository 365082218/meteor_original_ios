//
//   Copyright (c) 2016 Outfit7. All rights reserved.
//

using System;

namespace Idevgame.Debugs {

#if STRIP_DBG_SETTINGS
    [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
    [AttributeUsage(AttributeTargets.Method)]
    public class DebugMethod : Attribute {
        public string ButtonName;

        public DebugMethod(string btnName) {
            ButtonName = btnName;
        }
    }

}
