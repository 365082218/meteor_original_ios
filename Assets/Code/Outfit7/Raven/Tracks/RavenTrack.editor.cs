using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Starlite.Raven {

    public abstract partial class RavenTrack {

        [Serializable]
        public sealed class RavenFoldoutData {
            public bool m_Enabled = true;
            public float m_Height = 60f;
        }

#pragma warning disable 0414

        [SerializeField]
        private RavenFoldoutData m_FoldoutData = new RavenFoldoutData();

#pragma warning restore 0414

#if UNITY_EDITOR

        public bool IsEnabled {
            get {
                return m_IsEnabled;
            }
            set {
                if (m_IsEnabled == value) {
                    return;
                }

                Undo.RecordObject(this, "SetEnabled");
                m_IsEnabled = value;

                for (int i = 0; i < m_Events.Count; ++i) {
                    m_Events[i].IsTrackEnabled = m_IsEnabled;
                }
            }
        }

        public float FoldoutHeight {
            get {
                return m_FoldoutData.m_Height;
            }
            set {
                m_FoldoutData.m_Height = value;
            }
        }

        public bool FoldoutEnabled {
            get {
                return m_FoldoutData.m_Enabled;
            }
            set {
                m_FoldoutData.m_Enabled = value;
            }
        }

        public bool RemoveEvent(RavenEvent evnt) {
            Undo.RecordObject(this, "RemoveEvent");
            if (m_Events.Remove(evnt)) {
                EditorUtility.SetDirty(this);
                return true;
            }

            return false;
        }

        public abstract ERavenTrackType TrackType {
            get;
        }

        public abstract void InitializeEditor(RavenSequence sequence);

#endif
    }
}