using System.Collections.Generic;
using UnityEngine;
using Starlite.Raven.Internal;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Starlite.Raven {

    public abstract partial class RavenEvent {

        public bool IsEnabled {
            get {
                return (m_Flags & RavenEventFlags.Enabled) != 0;
            }
            set {
#if UNITY_EDITOR
                Undo.RecordObject(this, "IsEnabled");
#endif

                m_Flags = value ? (m_Flags | RavenEventFlags.Enabled) : (m_Flags & ~RavenEventFlags.Enabled);
            }
        }

        public bool IsTrackEnabled {
            get {
                return (m_Flags & RavenEventFlags.TrackEnabled) != 0;
            }
            set {
#if UNITY_EDITOR
                Undo.RecordObject(this, "IsTrackEnabled");
#endif

                m_Flags = value ? (m_Flags | RavenEventFlags.TrackEnabled) : (m_Flags & ~RavenEventFlags.TrackEnabled);
            }
        }

        public int StartFrame {
            get {
                return m_StartFrame;
            }
        }

        public int TrackIndex {
            get {
                return m_TrackIndex;
            }
        }

        public int SubTrackIndex {
            get {
                return m_SubTrackIndex;
            }
        }

        public int DurationInFrames {
            get {
                return EndFrame - m_StartFrame;
            }
        }

        public List<RavenCondition> Conditions {
            get {
                return m_Conditions;
            }
        }

        public abstract int LastFrame {
            get;
        }

        public abstract int EndFrame {
            get;
        }

        public abstract ERavenEventType EventType {
            get;
        }

        public abstract bool IsBarrier {
            get;
        }

        public abstract RavenPropertyComponent PropertyComponent {
            get;
        }

        public abstract void OnEnter(int frame);

        public virtual void Initialize(RavenSequence sequence) {
#if RAVEN_DEBUG
            RavenLog.DebugT(RavenSequence.Tag, "{0} Initialize", this);
#endif

            for (int i = 0; i < m_Conditions.Count; ++i) {
                var condition = m_Conditions[i];
                condition.m_Parameter = sequence.GetParameterAtIndex(condition.m_ParameterIndex);
                condition.m_ValueParameter = sequence.GetParameterAtIndex(condition.m_ValueIndex);
            }
        }

        public virtual bool IsValid() {
            return IsEnabled && IsTrackEnabled;
        }

        public override string ToString() {
            return string.Format("{0} ({1})", base.ToString(), GetInstanceID());
        }

        protected bool ConditionsMet() {
            for (int i = 0; i < Conditions.Count; ++i) {
                if (m_Conditions[i].IsTrue() == false) {
                    return false;
                }
            }
            return true;
        }
    }
}