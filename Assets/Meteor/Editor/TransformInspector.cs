using UnityEngine;
using UnityEditor;

namespace Outfit7.UI {
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Transform), true)]
    public class TransformInspector : UnityEditor.Editor {
        private SerializedProperty LocalPosition;
        private SerializedProperty LocalRotation;
        private SerializedProperty LocalScale;

        private void OnEnable() {
            LocalPosition = serializedObject.FindProperty("m_LocalPosition");
            LocalRotation = serializedObject.FindProperty("m_LocalRotation");
            LocalScale = serializedObject.FindProperty("m_LocalScale");
        }

        /// <summary>
        /// Draw the inspector widget.
        /// </summary>
    
        public override void OnInspectorGUI() {
            EditorGUIUtility.labelWidth = 15f;
        
            serializedObject.Update();
        
            DrawPosition();
            DrawRotation();
            DrawScale();
        
            GUILayout.BeginHorizontal();
            GUI.color = new Color(0.7f, 0.7f, 0.7f);
            EditorGUILayout.Vector3Field(new GUIContent("World Pos"), ((Transform) serializedObject.targetObject).position);
            GUI.color = Color.white;
            GUILayout.EndHorizontal();

            Quaternion q = ((Transform)serializedObject.targetObject).rotation;
            Vector3 vec = new Vector4(q.eulerAngles.x, q.eulerAngles.y, q.eulerAngles.z);
            GUILayout.BeginHorizontal();
            EditorGUILayout.Vector3Field(new GUIContent("World Rot"), vec);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                string info = LocalPosition.FindPropertyRelative("x").floatValue + ";" +
                              LocalPosition.FindPropertyRelative("y").floatValue + ";" +
                              LocalPosition.FindPropertyRelative("z").floatValue + ";" +
                              LocalRotation.FindPropertyRelative("x").floatValue + ";" +
                              LocalRotation.FindPropertyRelative("y").floatValue + ";" +
                              LocalRotation.FindPropertyRelative("z").floatValue + ";" +
                              LocalRotation.FindPropertyRelative("w").floatValue + ";" +
                              LocalScale.FindPropertyRelative("x").floatValue + ";" +
                              LocalScale.FindPropertyRelative("y").floatValue + ";" +
                              LocalScale.FindPropertyRelative("z").floatValue;
            
                bool copy = GUILayout.Button("Copy", GUILayout.Width(50f));
                bool paste = GUILayout.Button("Paste", GUILayout.Width(50f));
                bool reset = GUILayout.Button("Identity", GUILayout.Width(70f));
                bool uniformScale = GUILayout.Button("Z - Uniform Scale", GUILayout.Width(110f));
            
                if (copy)
                    EditorGUIUtility.systemCopyBuffer = info;
                if (paste) {
                    string line = EditorGUIUtility.systemCopyBuffer;
                    if (info != line) {
                        bool success = false;
                        string[] atts = line.Split(';');
                        if (atts.Length == 10) {
                            float[] values = new float[10];
                            for (int i = 0; i < atts.Length; i++) {
                                float.TryParse(atts[i], out values[i]);
                            }
                            LocalPosition.vector3Value = new Vector3(values[0], values[1], values[2]);
                            LocalRotation.quaternionValue = new Quaternion(values[3], values[4], values[5], values[6]);
                            LocalScale.vector3Value = new Vector3(values[7], values[8], values[9]);
                            success = true;
                        }
                        if (!success)
                            Debug.LogError("Failed pasting data");
                    
                    }
                }
                if (uniformScale) {
                    float v = LocalScale.FindPropertyRelative("z").floatValue;
                    LocalScale.FindPropertyRelative("x").floatValue = v;
                    LocalScale.FindPropertyRelative("y").floatValue = v;
                }
            
                if (reset) {
                    LocalPosition.vector3Value = Vector3.zero;
                    LocalRotation.quaternionValue = Quaternion.identity;
                    LocalScale.vector3Value = Vector3.one;
                }
            }
            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawPosition() {
            GUILayout.BeginHorizontal();
            {
                bool reset = GUILayout.Button("P", GUILayout.Width(20f));
            
                EditorGUILayout.PropertyField(LocalPosition.FindPropertyRelative("x"));
                EditorGUILayout.PropertyField(LocalPosition.FindPropertyRelative("y"));
                EditorGUILayout.PropertyField(LocalPosition.FindPropertyRelative("z"));
            
                if (reset)
                    LocalPosition.vector3Value = Vector3.zero;
            }
            GUILayout.EndHorizontal();
        }

        void DrawScale() {
            GUILayout.BeginHorizontal();
            {
                bool reset = GUILayout.Button("S", GUILayout.Width(20f));
            
                EditorGUILayout.PropertyField(LocalScale.FindPropertyRelative("x"));
                EditorGUILayout.PropertyField(LocalScale.FindPropertyRelative("y"));
                EditorGUILayout.PropertyField(LocalScale.FindPropertyRelative("z"));

                if (reset)
                    LocalScale.vector3Value = Vector3.one;
            }
            GUILayout.EndHorizontal();
        }

        enum Axes : int {
            None = 0,
            X = 1,
            Y = 2,
            Z = 4,
            All = 7,
        }

        Axes CheckDifference(Transform t, Vector3 original) {
            Vector3 next = t.localEulerAngles;
        
            Axes axes = Axes.None;
        
            if (Differs(next.x, original.x))
                axes |= Axes.X;
            if (Differs(next.y, original.y))
                axes |= Axes.Y;
            if (Differs(next.z, original.z))
                axes |= Axes.Z;
        
            return axes;
        }

        Axes CheckDifference(SerializedProperty property) {
            Axes axes = Axes.None;
        
            if (property.hasMultipleDifferentValues) {
                Vector3 original = property.quaternionValue.eulerAngles;
            
                foreach (Object obj in serializedObject.targetObjects) {
                    axes |= CheckDifference(obj as Transform, original);
                    if (axes == Axes.All)
                        break;
                }
            }
            return axes;
        }

        /// <summary>
        /// Draw an editable float field.
        /// </summary>
        /// <param name="hidden">Whether to replace the value with a dash</param>
        /// <param name="greyedOut">Whether the value should be greyed out or not</param>
    
        static bool FloatField(string name, ref float value, bool hidden, bool greyedOut, GUILayoutOption opt) {
            float newValue = value;
            GUI.changed = false;
        
            if (!hidden) {
                if (greyedOut) {
                    GUI.color = new Color(0.7f, 0.7f, 0.7f);
                    newValue = EditorGUILayout.FloatField(name, newValue, opt);
                    GUI.color = Color.white;
                } else {
                    newValue = EditorGUILayout.FloatField(name, newValue, opt);
                }
            } else if (greyedOut) {
                GUI.color = new Color(0.7f, 0.7f, 0.7f);
                float.TryParse(EditorGUILayout.TextField(name, "--", opt), out newValue);
                GUI.color = Color.white;
            } else {
                float.TryParse(EditorGUILayout.TextField(name, "--", opt), out newValue);
            }
        
            if (GUI.changed && Differs(newValue, value)) {
                value = newValue;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Because Mathf.Approximately is too sensitive.
        /// </summary>
    
        static bool Differs(float a, float b) {
            return Mathf.Abs(a - b) > 0.0001f;
        }

        void DrawRotation() {
            GUILayout.BeginHorizontal();
            {
                bool reset = GUILayout.Button("R", GUILayout.Width(20f));
            
                Vector3 visible = (serializedObject.targetObject as Transform).localEulerAngles;
            
                visible.x = WrapAngle(visible.x);
                visible.y = WrapAngle(visible.y);
                visible.z = WrapAngle(visible.z);
            
                Axes changed = CheckDifference(LocalRotation);
                Axes altered = Axes.None;
            
                GUILayoutOption opt = GUILayout.MinWidth(30f);
            
                if (FloatField("X", ref visible.x, (changed & Axes.X) != 0, false, opt))
                    altered |= Axes.X;
                if (FloatField("Y", ref visible.y, (changed & Axes.Y) != 0, false, opt))
                    altered |= Axes.Y;
                if (FloatField("Z", ref visible.z, (changed & Axes.Z) != 0, false, opt))
                    altered |= Axes.Z;
            
                if (reset) {
                    LocalRotation.quaternionValue = Quaternion.identity;
                } else if (altered != Axes.None) {
                    RegisterUndo("Change Rotation", serializedObject.targetObjects);
                
                    foreach (Object obj in serializedObject.targetObjects) {
                        Transform t = obj as Transform;
                        Vector3 v = t.localEulerAngles;
                    
                        if ((altered & Axes.X) != 0)
                            v.x = visible.x;
                        if ((altered & Axes.Y) != 0)
                            v.y = visible.y;
                        if ((altered & Axes.Z) != 0)
                            v.z = visible.z;
                    
                        t.localEulerAngles = v;
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        private float WrapAngle(float angle) {
            while (angle > 180f)
                angle -= 360f;
            while (angle < -180f)
                angle += 360f;
            return angle;
        }

        private void RegisterUndo(string name, params Object[] objects) {
            if (objects != null && objects.Length > 0) {
                Undo.RecordObjects(objects, name);
                foreach (Object obj in objects) {
                    if (obj == null)
                        continue;
                    EditorUtility.SetDirty(obj);
                }
            }
        }
    }
}