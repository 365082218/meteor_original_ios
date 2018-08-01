using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Outfit7.Logic.StateMachineInternal;


namespace Outfit7.Sequencer {
    public class SequencerBookmarkEvent : SequencerTriggerEvent {
        public enum BookmarkType {
            IGNORE,
            STOP,
            LOOP
        }

        public string BookmarkName = "NoName";
        public SequencerSequence AffectingSequence;
        public BookmarkType Type;

        public override bool IgnoreObjects() {
            return true;
        }

        public override void OnTrigger(List<Component> components, float currentTime) {
            if (AffectingSequence == null)
                return;
            switch (Type) {
                case BookmarkType.IGNORE:
                    break;
                case BookmarkType.STOP:
                    AffectingSequence.Stop(StartTime, this);
                    break;
                case BookmarkType.LOOP:
                    LoopToPreviousBookmark();
                    break;
            }
        }

        private void LoopToPreviousBookmark() {
            float time = 0;
            for (int i = 0; i < AffectingSequence.Bookmarks.Count; i++) {
                SequencerBookmarkEvent bookmark = AffectingSequence.Bookmarks[i];
                if (bookmark == this)
                    continue;
                if (bookmark.StartTime < AffectingSequence.GetCurrentTime() && bookmark.StartTime < StartTime && bookmark.StartTime >= time)
                    time = bookmark.StartTime;
            }

            // handle infinite loop rare case when setting loop on bookmark at time 0 kek
            // or some genius sequence design
            if (time == StartTime) {
                return;
            }

            AffectingSequence.MoveToTime(time, AffectingSequence.GetCurrentTime() - StartTime);
        }
    }
}   