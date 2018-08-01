using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Outfit7.Sequencer {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SequencerQuickSearchAttribute : System.Attribute {
        protected string search;

        public SequencerQuickSearchAttribute(string search) {
            this.search = search;
        }

        public string GetSearchString() {
            return search;
        }
    }

    public class SequencerQuickSearchDisplayAttribute : System.Attribute {
        protected string displayName;

        public SequencerQuickSearchDisplayAttribute(string displayName) {
            this.displayName = displayName;
        }

        public string GetDisplayName() {
            return displayName;
        }
    }


    public class SequencerQuickSearch {
        Vector2 size = new Vector2(150, 30);
        Rect position;
        public string searchLine = "";
        IEnumerable<Type> Types;
        GUIStyle DropDownStyle = (GUIStyle) "MiniToolbarButton";
        GUIStyle SelectedStyle = (GUIStyle) "PreButton";
        Type SelectedType;
        Type LastEventType;

        public SequencerQuickSearch(Vector2 mousePos, IEnumerable<Type> types, Type lastEventType = null) {
            position = new Rect(mousePos - new Vector2(75, 20), size);
            Types = types;
            LastEventType = lastEventType;
        }

        public bool OnGui(out Type selectedType) {
            GUILayout.BeginArea(position);
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            //GUILayout.Label("QuickSearch");
            GUI.SetNextControlName("SearchField");
            searchLine = GUILayout.TextArea(searchLine);
            EditorGUI.FocusTextInControl("SearchField");
            if (GUILayout.Button("X", SelectedStyle, GUILayout.MaxWidth(20))) {
                selectedType = null;
                return true;
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
                
            if (DrawMenu() || searchLine.Contains("\n")) {
                selectedType = SelectedType;
                return true;
            }
            selectedType = null;
            return false;
        }

        private bool DrawMenu() {
            string filterLine = searchLine.Replace("\n", "");
            //GenericMenu menu = new GenericMenu();

            //IEnumerable<Type> FilteredTypes = Types.Where(t => t.Name.ToLower().Contains(filterLine.ToLower()));
            List<Type> FilteredTypes = new List<Type>();
            foreach (System.Type t in Types) {
                SequencerQuickSearchDisplayAttribute displayAttr = Attribute.GetCustomAttribute(t, typeof(SequencerQuickSearchDisplayAttribute)) as SequencerQuickSearchDisplayAttribute;
                bool foundMatch = false;
                if (displayAttr == null)
                    continue;
                else {
                    if (displayAttr.GetDisplayName().ToLower().Contains(filterLine.ToLower()))
                        foundMatch = true;
                }

                Attribute[] attrList = Attribute.GetCustomAttributes(t, typeof(SequencerQuickSearchAttribute));
                foreach (SequencerQuickSearchAttribute attr in attrList) {
                    if (attr.GetSearchString().ToLower().Contains(filterLine.ToLower())) {
                        foundMatch = true;
                        break;
                    }
                }
                if (foundMatch)
                    FilteredTypes.Add(t);
            }

            if (LastEventType != null) {
                if (FilteredTypes.IndexOf(LastEventType) != -1) {
                    FilteredTypes.Remove(LastEventType);
                    FilteredTypes.Insert(0, LastEventType);
                }
            }
                

            if (FilteredTypes.Count > 0) {
                SelectedType = FilteredTypes[0];
            } else
                SelectedType = null;

            int count = FilteredTypes.Count;
            if (count > 6)
                count = 6;
            GUILayout.BeginArea(new Rect(position.position + new Vector2(0, 15), new Vector2(size.x, count * 18)));
            GUILayout.BeginVertical(EditorStyles.toolbar);
            int counter = 0;
            foreach (System.Type t in FilteredTypes) {
                counter++;
                if (counter > count)
                    break;

                SequencerQuickSearchDisplayAttribute displayAttr = Attribute.GetCustomAttribute(t, typeof(SequencerQuickSearchDisplayAttribute)) as SequencerQuickSearchDisplayAttribute;
                if (GUILayout.Button(displayAttr.GetDisplayName(), counter == 1 ? SelectedStyle : DropDownStyle)) {
                    SelectedType = t;
                    return true;
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndArea();
            return false;
        }
    }
}