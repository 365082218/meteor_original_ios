using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Outfit7.Logic.Util {

    public static class GUIUtil {

        public static void BeginGroup(Rect position) {
            GUI.BeginClip(position);
        }

        public static void EndGroup() {
            GUI.EndClip();
        }

        public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color) {
            Handles.BeginGUI();
            Handles.color = color;
            Handles.DrawLine(new Vector3(pointA.x, pointA.y), new Vector3(pointB.x, pointB.y));
            Handles.EndGUI();

        }

        public static void DrawFatLine(Vector2 pointA, Vector2 pointB, float fatness, Color color) {
            Handles.BeginGUI();
            Handles.color = color;
            Handles.DrawAAPolyLine(fatness, 2, new Vector3(pointA.x, pointA.y), new Vector3(pointB.x, pointB.y));
            Handles.EndGUI();
        }

        public static void DrawTriangle(Vector2[] points, Color color) {
            Handles.BeginGUI();
            Handles.color = color;
            Handles.DrawAAConvexPolygon(points[0], points[1], points[2]);
            Handles.EndGUI();
        }

        public static void DrawQuads(Vector2[] points, Color color) {
            Handles.BeginGUI();
            Handles.color = color;
            for (int i = 0; i < points.Length; i += 4) {
                Handles.DrawAAConvexPolygon(points[i], points[i + 1], points[i + 2]);
                Handles.DrawAAConvexPolygon(points[i], points[i + 2], points[i + 3]);
            }
            Handles.EndGUI();
        }

    }

}
