#if !STARLITE_EDITOR
using Outfit7.Logic.Util;
#else
using Starlite;
#endif

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Starlite.Raven {

    public sealed class RavenBookmarkEventView : RavenTriggerEventView {
        private const float c_HotzoneWidth = 10f;

        private RavenBookmarkEvent m_Bookmark = null;
        private Color m_BookmarkLineColor = new Color(1f, 1f, 1f);
        private Color m_BookmarkConditionsLineColor = new Color(0.776f, 0.443f, 0.114f, 1f);
        private Color m_BookmarkSelectedLineColor = new Color(0f, 0.5f, 1f, 1f);
        private static GUIStyle s_BookmarkNode = (GUIStyle)"Grad Down Swatch";
        private static GUIStyle s_DefaultStyle = new GUIStyle();

        public override string Name {
            get {
                return m_Bookmark.BookmarkName;
            }
        }

        public override bool AlwaysVisible {
            get {
                return true;
            }
        }

        protected override bool AllowSubTrackChange {
            get {
                return false;
            }
        }

        public override void Initialize(RavenEvent evnt, RavenTrackView parent) {
            base.Initialize(evnt, parent);
            m_Bookmark = evnt as RavenBookmarkEvent;
        }

        protected override void OnDefineEventRect(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, bool foldoutEnabled, float foldoutHeight, bool optimizedView = false) {
            float elementHeight = (foldoutEnabled ? foldoutHeight : 20f);
            startHeight += (optimizedView ? 0 : m_Event.SubTrackIndex) * elementHeight;
            m_EventRect = new Rect(timelineData.GetPositionAtFrame(m_Event.StartFrame),
                startHeight,
                c_HotzoneWidth,
                elementHeight);
            m_HandleTouchRect = m_EventRect;
        }

        protected override void OnDrawExtendedGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, bool foldoutEnabled, float foldoutHeight, List<RavenParameter> parameters) {
            m_Bookmark.BookmarkName = EditorGUI.TextField(new Rect(m_EventRect.x + 3f, m_EventRect.y, Mathf.Max(CalculateTextSize(m_Bookmark.BookmarkName).x, 100f), 20), m_Bookmark.BookmarkName);
            m_Bookmark.BookmarkType = (RavenBookmarkEvent.ERavenBookmarkType)EditorGUI.EnumPopup(new Rect(m_EventRect.x + 3f, m_EventRect.y + 20, 50f, 20), m_Bookmark.BookmarkType);
        }

        protected override void OnDrawGui(Rect windowSize, float startHeight, float splitViewLeft, TimelineData timelineData, bool foldoutEnabled, float foldoutHeight, List<RavenParameter> parameters) {
            if (foldoutEnabled == false) {
                EditorGUI.LabelField(new Rect(m_EventRect.x + 3f, m_EventRect.y, Mathf.Max(CalculateTextSize(m_Bookmark.BookmarkName).x, 100f), 20), m_Bookmark.BookmarkName);
            }
            var lineColor = m_Selected ? m_BookmarkSelectedLineColor : (m_Bookmark.Conditions.Count > 0 ? m_BookmarkConditionsLineColor : m_BookmarkLineColor);
            GUIUtil.DrawLine(new Vector2(m_EventRect.x, m_EventRect.y), new Vector2(m_EventRect.x, 9999f), lineColor);
            GUIUtil.DrawLine(new Vector2(m_EventRect.x + 2, m_EventRect.y), new Vector2(m_EventRect.x + 2, 9999f), lineColor);
            EditorGUI.LabelField(new Rect(m_EventRect.x - 5f, m_EventRect.y, m_EventRect.width, m_EventRect.height), "", s_BookmarkNode);
            EditorGUIUtility.AddCursorRect(m_EventRect, MouseCursor.ResizeHorizontal);
        }

        override protected void DrawFrameLabels(TimelineData timelineData) {
            base.DrawFrameLabels(timelineData);

            GUIStyle centerStyle = new GUIStyle();
            centerStyle.alignment = TextAnchor.MiddleCenter;
            centerStyle.normal.textColor = Color.green;
            EditorGUI.LabelField(new Rect(m_EventRect.x - 35, m_EventRect.y - 10, 30, 30), m_Event.StartFrame.ToString(), centerStyle);
        }

        protected override void OnDrawEventsContextMenu(GenericMenu menu, RavenTrackView parent) {
            menu.AddItem(new GUIContent("Remove All Since Event Before This/Sure?"), false, RemoveDataFromBookmark, false);
            menu.AddItem(new GUIContent("Remove All Until Event After This/Sure?"), false, RemoveDataFromBookmark, true);
        }

        private Vector2 CalculateTextSize(string text) {
            var size = s_DefaultStyle.CalcSize(new GUIContent(text + "  "));   // add 2 extra chars for more space, else it can clip in controls
            return size;
        }

        private void RemoveDataFromBookmark(object forwardObj) {
            var sequence = Event.GetComponent<RavenSequence>();

            Undo.RecordObject(sequence, "RemoveBookmarkData");

            bool forward = (bool)forwardObj;
            var selectedBookmark = m_Bookmark;
            RavenBookmarkEvent otherBookmark = null;
            if (forward) {
                var min = int.MaxValue;
                for (int i = 0; i < sequence.SortedBookmarks.Count; ++i) {
                    var tmpBookmark = sequence.SortedBookmarks[i];
                    if (tmpBookmark.StartFrame > selectedBookmark.StartFrame
                        && tmpBookmark.StartFrame < min) {
                        min = tmpBookmark.StartFrame;
                        otherBookmark = tmpBookmark;
                    }
                }

                var startPoint = selectedBookmark.StartFrame;
                var endPoint = min == int.MaxValue ? sequence.TotalFrameCount : otherBookmark.StartFrame;
                RemoveBookmark(selectedBookmark, startPoint, endPoint);
            } else {
                var max = int.MinValue;
                for (int i = 0; i < sequence.SortedBookmarks.Count; ++i) {
                    var tmpBookmark = sequence.SortedBookmarks[i];
                    if (tmpBookmark.StartFrame < selectedBookmark.StartFrame
                        && tmpBookmark.StartFrame > max) {
                        max = tmpBookmark.StartFrame;
                        otherBookmark = tmpBookmark;
                    }
                }

                var startPoint = max == int.MinValue ? 0 : otherBookmark.StartFrame;
                var endPoint = selectedBookmark.StartFrame;
                RemoveBookmark(selectedBookmark, startPoint, endPoint);
            }
        }

        /// <summary>
        /// Assumes bookmarkFirst.StartFrame < bookmarkSecond.StartFrame
        /// </summary>
        private void RemoveBookmark(RavenBookmarkEvent bookmarkToRemove, int frameStartPoint, int frameEndPoint) {
            var sequencerView = RavenSequenceEditor.Instance.SequenceView;
            sequencerView.RemoveTimeImpl(frameStartPoint, frameEndPoint, m_Event);
        }

        protected override void OnUpdateWhileRecording(double currentTime) {
        }

        protected override void OnRecordingStart() {
        }

        protected override void OnRecordingStop() {
        }
    }
}