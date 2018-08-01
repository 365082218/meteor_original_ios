using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Starlite.Raven {

    public abstract partial class RavenTrack {

        public List<RavenEvent> Events {
            get {
                return m_Events;
            }
        }
    }
}