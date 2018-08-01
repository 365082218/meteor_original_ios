using System;

namespace Outfit7.Devel.O7Debug {
#if STRIP_DBG_SETTINGS
    [System.Diagnostics.ConditionalAttribute("FALSE")]
#endif
    [AttributeUsage(AttributeTargets.Method)]
    public class DebugInit : Attribute {
       
    }
}

