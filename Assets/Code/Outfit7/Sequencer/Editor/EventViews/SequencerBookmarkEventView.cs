using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Outfit7.Util;
using Outfit7.Logic.StateMachineInternal;
using Outfit7.Logic.Util;
using System.Linq;

namespace Outfit7.Sequencer {
    [SequencerQuickSearchDisplayAttribute("Bookmark")]
    [SequencerNormalTrackAttribute("Sequence/Bookmark")]
    public class SequencerBookmarkEventView : SequencerTriggerEventView {
        private SequencerBookmarkEvent Event = null;
        private Color BookmarkLineColor = new Color(1f, 1f, 1f);
        private GUIStyle Node = (GUIStyle) "Grad Down Swatch";
        private GUIStyle DefaultStyle = new GUIStyle();

        public override void OnInit(object evnt, object parent) {
            Event = evnt as SequencerBookmarkEvent;
            base.OnInit(evnt, parent);
        }

        public override string GetName() {
            return Event.BookmarkName;
        }

        public override void OnDrawExtendedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            Event.BookmarkName = EditorGUI.TextField(new Rect(EventRect.x + 8f, 0, Mathf.Max(CalculateTextSize(Event.BookmarkName).x, 100f), 20), Event.BookmarkName);
            Event.Type = (SequencerBookmarkEvent.BookmarkType) EditorGUI.EnumPopup(new Rect(EventRect.x + 8f, 20, 50f, 20), Event.Type);
        }

        public override void OnDrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, SequencerFoldoutData foldoutData, List<Parameter> parameters) {
            if (foldoutData.Enabled == false)
                EditorGUI.LabelField(new Rect(EventRect.x + 8f, 0, Mathf.Max(CalculateTextSize(Event.BookmarkName).x, 100f), 20), Event.BookmarkName);
            GUIUtil.DrawLine(new Vector2(EventRect.center.x, 0), new Vector2(EventRect.center.x, 9999f), BookmarkLineColor);
            GUIUtil.DrawLine(new Vector2(EventRect.center.x + 2, 0), new Vector2(EventRect.center.x + 2, 9999f), BookmarkLineColor);
            EditorGUI.LabelField(new Rect(EventRect.x, EventRect.y, EventRect.width, EventRect.height), "", Node);
            EditorGUIUtility.AddCursorRect(EventRect, MouseCursor.ResizeHorizontal);
        }

        protected override void OnDrawEventsContextMenu(GenericMenu menu, SequencerTrackView parent, object actor) {
            base.OnDrawEventsContextMenu(menu, parent, actor);
            menu.AddItem(new GUIContent("Remove All Since Event Before This/Sure?"), false, RemoveDataFromBookmark, false);
            menu.AddItem(new GUIContent("Remove All Until Event After This/Sure?"), false, RemoveDataFromBookmark, true);
        }

        public override void OnRefresh(object actor) {
            base.OnRefresh(actor);
            SequencerSequence sequence = Event.GetComponent<SequencerSequence>();
            if (Event.AffectingSequence == null)
                Event.AffectingSequence = sequence;
            if (sequence.Bookmarks.IndexOf(Event) == -1) {
                sequence.Bookmarks.Add(Event);
                EditorUtility.SetDirty(sequence);
            }
        }

        protected override void OnDestroy() {
            SequencerSequence sequence = Event.GetComponent<SequencerSequence>();
            Undo.RecordObject(sequence, "DestroyBookmark");
            if (sequence.Bookmarks.IndexOf(Event) != -1) {
                sequence.Bookmarks.Remove(Event);
                EditorUtility.SetDirty(sequence);
            }
        }

        private Vector2 CalculateTextSize(string text) {
            var size = DefaultStyle.CalcSize(new GUIContent(text + "  "));   // add 2 extra chars for more space, else it can clip in controls
            return size;
        }

        private void RemoveDataFromBookmark(object forwardObj) {
            var sequence = Event.GetComponent<SequencerSequence>();

            Undo.RecordObject(sequence, "RemoveBookmarkData");

            bool forward = (bool) forwardObj;
            var selectedBookmark = Event;
            SequencerBookmarkEvent otherBookmark = null;
            if (forward) {
                var min = float.MaxValue;
                for (int i = 0; i < sequence.Bookmarks.Count; ++i) {
                    var tmpBookmark = sequence.Bookmarks[i];
                    if (tmpBookmark.StartTime > selectedBookmark.StartTime
                        && tmpBookmark.StartTime < min) {
                        min = tmpBookmark.StartTime;
                        otherBookmark = tmpBookmark;
                    }
                }

                var startPoint = selectedBookmark.StartTime;
                var endPoint = min == float.MaxValue ? sequence.Duration : otherBookmark.StartTime;
                RemoveBookmark(selectedBookmark, startPoint, endPoint);
            } else {
                var max = float.MinValue;
                for (int i = 0; i < sequence.Bookmarks.Count; ++i) {
                    var tmpBookmark = sequence.Bookmarks[i];
                    if (tmpBookmark.StartTime < selectedBookmark.StartTime
                        && tmpBookmark.StartTime > max) {
                        max = tmpBookmark.StartTime;
                        otherBookmark = tmpBookmark;
                    }
                }

                var startPoint = max == float.MinValue ? 0f : otherBookmark.StartTime;
                var endPoint = selectedBookmark.StartTime;
                RemoveBookmark(selectedBookmark, startPoint, endPoint);
            }
        }

        /// <summary>
        /// Assumes bookmarkFirst.StarTtime < bookmarkSecond.TtartTime
        /// </summary>
        private void RemoveBookmark(SequencerBookmarkEvent bookmarkToRemove, float timeToRemoveStartPoint, float timeToRemoveEndPoint) {

            var sequencerView = SequencerWindow.SequencerEditor.SequenceView;
            SequencerEventView view = null;
            if (ViewLookup.TryGetValue(bookmarkToRemove, out view)) {
                sequencerView.RemoveTimeImpl(timeToRemoveStartPoint, timeToRemoveEndPoint, view);
            }
        }
    }
}
