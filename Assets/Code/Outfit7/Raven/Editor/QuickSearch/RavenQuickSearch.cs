using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Starlite.Raven {

    public class RavenQuickSearch {
        private Vector2 m_Size = new Vector2(150, 30);
        private Rect m_Position;
        private string m_SearchLine = "";
        private IEnumerable<RavenQuickSearchWrapper> m_PossibleItems;
        private Action<object> m_Callback;

        private RavenQuickSearchWrapper m_SelectedType;
        private int m_SelectedIndex = 0;

        private int m_OnGuiCount = 0;

        private static GUIStyle s_DropDownStyle = (GUIStyle)"MiniToolbarButton";
        private static GUIStyle s_SelectedStyle = (GUIStyle)"PreButton";

        public RavenQuickSearch(Vector2 mousePos, IEnumerable<RavenQuickSearchWrapper> possibleItems, Action<object> callback) {
            m_Position = new Rect(mousePos - new Vector2(75, 20), m_Size);
            m_PossibleItems = possibleItems;
            m_Callback = callback;
            var max = m_PossibleItems.OrderByDescending(x => x.m_PrettyName.Length).FirstOrDefault();
            if (max != null) {
                var maxSize = RavenPreferences.QuickSearchMaximumSize == 0 ? float.MaxValue : RavenPreferences.QuickSearchMaximumSize;
                m_Position.width = m_Size.x = Math.Min(s_SelectedStyle.CalcSize(new GUIContent(max.m_PrettyName)).x, maxSize);
            }
        }

        public bool OnGui() {
            GUILayout.BeginArea(m_Position);
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUI.SetNextControlName("SearchField");
            m_SearchLine = GUILayout.TextArea(m_SearchLine);
            EditorGUI.FocusTextInControl("SearchField");

            if (m_OnGuiCount < 2) {
                // hack to prevent text control stealing chars
                m_SearchLine = string.Empty;
            }

            if (GUILayout.Button("X", s_SelectedStyle, GUILayout.MaxWidth(20))) {
                return true;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            if (DrawMenu() || m_SearchLine.Contains("\n")) {
                if (m_SelectedType != null) {
                    m_Callback(m_SelectedType.m_Data);
                }
                return true;
            }
            ++m_OnGuiCount;
            return false;
        }

        public bool HandleInput(out bool closeQuickSearch) {
            closeQuickSearch = false;

            // hack because we're getting used type here most of the time so just ignore everything we don't care about
            if (UnityEngine.Event.current.type == EventType.Layout ||
                UnityEngine.Event.current.type == EventType.KeyUp) {
                return false;
            }

            if (UnityEngine.Event.current.type == EventType.MouseDown ||
                UnityEngine.Event.current.keyCode == KeyCode.Escape) {
                closeQuickSearch = true;
                return true;
            }

            if (UnityEngine.Event.current.keyCode == KeyCode.DownArrow) {
                ++m_SelectedIndex;
                return true;
            } else if (UnityEngine.Event.current.keyCode == KeyCode.UpArrow) {
                --m_SelectedIndex;
                return true;
            } else if (UnityEngine.Event.current.keyCode == KeyCode.KeypadEnter) {
                closeQuickSearch = true;

                if (m_SelectedType != null) {
                    m_Callback(m_SelectedType.m_Data);
                }
                return true;
            }

            return false;
        }

        private bool DrawMenu() {
            var filterLine = m_SearchLine.Replace("\n", "").ToLowerInvariant();
            var filteredItems = m_PossibleItems.Where(x => x.m_PrettyName.ToLowerInvariant().Contains(filterLine)).ToList();

            int count = filteredItems.Count;
            if (count > RavenPreferences.QuickSearchMaximumElements) {
                count = RavenPreferences.QuickSearchMaximumElements;
            }

            if (count > 0) {
                m_SelectedIndex = Math.Max(Math.Min(count - 1, m_SelectedIndex), 0);
                m_SelectedType = filteredItems[m_SelectedIndex];
            } else {
                m_SelectedType = null;
            }

            GUILayout.BeginArea(new Rect(m_Position.position + new Vector2(0, 16), new Vector2(m_Size.x, count * 18)));
            GUILayout.BeginVertical();
            for (int i = 0; i < count; ++i) {
                if (GUILayout.Button(filteredItems[i].m_PrettyName, i == m_SelectedIndex ? s_SelectedStyle : s_DropDownStyle)) {
                    m_SelectedType = filteredItems[i];
                    return true;
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();
            return false;
        }
    }
}