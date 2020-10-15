using System.Collections.Generic;
using UnityEngine;
//网上找的绘制一些辅助线条的，用来调试OBB判定用的
namespace Assets.Scripts.Utils.GizmosHelper {
    public enum CapsuleDirection {
        XAxis,
        YAxis,
        ZAxis,
    }

    public class GizmosHelper : MonoBehaviour {
        #region singleton

        private static GizmosHelper _instance;
        public static GizmosHelper Instance {
            get {
                if (_instance != null) return _instance;
                var go = new GameObject("GizmosHelper");
                _instance = go.AddComponent<GizmosHelper>();
                return _instance;
            }
        }

        #endregion

        #region private members
        private class Point {
            public Vector3 Position;
            public Color Color;
        }
        private static readonly SortedDictionary<string, Point> Points = new SortedDictionary<string, Point>();

        private class Ray {
            public Vector3 Position;
            public Vector3 Direction;
            public Color Color;
        }
        private static readonly SortedDictionary<string, Ray> Rays = new SortedDictionary<string, Ray>();

        private class Line {
            public Vector3 StartPos;
            public Vector3 EndPos;
            public Color Color;
        }
        private static readonly SortedDictionary<string, Line> Lines = new SortedDictionary<string, Line>();

        private class Circle {
            public Vector3 Position;
            public Vector3 Up;
            public float Radius;
            public Color Color;
        }
        private static readonly SortedDictionary<string, Circle> Circles = new SortedDictionary<string, Circle>();

        private class Cube {
            public Vector3 Position;
            public Vector3 Up;
            public Vector3 Forward;
            public Vector3 Center;
            public Vector3 Size;
            public Vector3 Scale;
            public Color Color;
        }
        private static readonly SortedDictionary<string, Cube> Cubes = new SortedDictionary<string, Cube>();

        private class Sphere {
            public Vector3 Position;
            public Vector3 Center;
            public Vector3 Scale;
            public float Radius;
            public Color Color;
        }
        private static readonly SortedDictionary<string, Sphere> Spheres = new SortedDictionary<string, Sphere>();

        private class Capsule {
            public Vector3 Position;
            public Vector3 Center;
            public Vector3 Scale;
            public float Radius;
            public float Height;
            public CapsuleDirection Direction;
            public Color Color;
        }
        private static readonly SortedDictionary<string, Capsule> Capsules = new SortedDictionary<string, Capsule>();
        #endregion private members

        #region public members

        public bool Enable;

        #endregion public members

        #region mono

        private void Awake() {
            DontDestroyOnLoad(gameObject);
        }

        private void Update() {
            if (!Input.GetKey(KeyCode.LeftShift))
                return;
            if (Input.GetKeyDown(KeyCode.D))
                Enable = true;
            else if (Input.GetKeyDown(KeyCode.C)) {
                Enable = false;
                Clear();
            }
        }

        //void OnDrawGizmos() {
        //    if (!Enable)
        //        return;

        //    Color oldColor = Gizmos.color;
        //    DrawPoints();
        //    DrawRays();
        //    DrawLines();
        //    DrawCircles();
        //    DrawCubes();
        //    DrawSpheres();
        //    DrawCapsules();
        //    Gizmos.color = oldColor;
        //}

        #endregion mono

        #region public interfaces

        public void DrawPoint(string pointName, Vector3 pos, Color color) {
            if (!Enable)
                return;

            Point temp;
            if (Points.TryGetValue(pointName, out temp)) {
                temp.Position = pos;
                temp.Color = color;
            } else {
                temp = new Point() { Position = pos, Color = color };
                Points.Add(pointName, temp);
            }
        }

        public void DrawRay(string rayName, Vector3 pos, Vector3 dir, Color color) {
            if (!Enable)
                return;

            Ray temp;
            if (Rays.TryGetValue(rayName, out temp)) {
                temp.Position = pos;
                temp.Direction = dir;
                temp.Color = color;
            } else {
                temp = new Ray() { Position = pos, Direction = dir, Color = color };
                Rays.Add(rayName, temp);
            }
        }

        public void DrawLine(string lineName, Vector3 startPos, Vector3 endPos, Color color) {
            if (!Enable)
                return;

            Line temp;
            if (Lines.TryGetValue(lineName, out temp)) {
                temp.StartPos = startPos;
                temp.EndPos = endPos;
                temp.Color = color;
            } else {
                temp = new Line() { StartPos = startPos, EndPos = endPos, Color = color };
                Lines.Add(lineName, temp);
            }
        }

        public void DrawCircle(string circleName, Vector3 pos, Vector3 up, Color color, float radius = 1.0f) {
            if (!Enable)
                return;

            Circle temp;
            if (Circles.TryGetValue(circleName, out temp)) {
                temp.Position = pos;
                temp.Up = up;
                temp.Radius = radius;
                temp.Color = color;
            } else {
                temp = new Circle() { Position = pos, Up = up, Radius = radius, Color = color };
                Circles.Add(circleName, temp);
            }
        }

        public void DrawCircle(string circleName, Vector3 pos, Color color, float radius = 1.0f) {
            DrawCircle(circleName, pos, Vector3.up, color, radius);
        }

        public void DrawCircle(string circleName, Vector3 pos, float radius = 1.0f) {
            DrawCircle(circleName, pos, Vector3.up, Color.white, radius);
        }

        public void DrawCube(string cubeName, Vector3 pos, Vector3 center, Vector3 up, Vector3 forward, Vector3 size, Vector3 scale, Color color) {
            if (!Enable)
                return;

            Cube temp;
            if (Cubes.TryGetValue(cubeName, out temp)) {
                temp.Position = pos;
                temp.Center = center;
                temp.Up = up;
                temp.Forward = forward;
                temp.Size = size;
                temp.Scale = scale;
                temp.Color = color;
            } else {
                temp = new Cube() { Position = pos, Center = center, Up = up, Forward = forward, Size = size, Scale = scale, Color = color };
                Cubes.Add(cubeName, temp);
            }
        }

        public void DrawSphere(string sphereName, Vector3 pos, Vector3 center, Vector3 scale, float radius, Color color) {
            if (!Enable)
                return;

            Sphere temp;
            if (Spheres.TryGetValue(sphereName, out temp)) {
                temp.Position = pos;
                temp.Center = center;
                temp.Scale = scale;
                temp.Radius = radius;
                temp.Color = color;
            } else {
                temp = new Sphere() { Center = center, Radius = radius, Color = color };
                Spheres.Add(sphereName, temp);
            }
        }

        public void DrawCapsule(string capsuleName, Vector3 pos, Vector3 center, Vector3 scale, float radius, float height, CapsuleDirection direction, Color color) {
            if (!Enable)
                return;

            Capsule temp;
            if (Capsules.TryGetValue(capsuleName, out temp)) {
                temp.Position = pos;
                temp.Center = center;
                temp.Scale = scale;
                temp.Radius = radius;
                temp.Height = height;
                temp.Direction = direction;
                temp.Color = color;
            } else {
                temp = new Capsule() { Position = pos, Center = center, Scale = scale, Radius = radius, Height = height, Direction = direction, Color = color };
                Capsules.Add(capsuleName, temp);
            }

        }

        public void Clear() {
            Points.Clear();
            Rays.Clear();
            Lines.Clear();
            Circles.Clear();
            Cubes.Clear();
            Capsules.Clear();
        }

        #endregion public interfaces

        #region private implements

        private void DrawPoints() {
            foreach (var item in Points) {
                Color oldColor = Gizmos.color;
                Gizmos.color = item.Value.Color;

                Gizmos.DrawLine(item.Value.Position + (Vector3.up * 0.5f), item.Value.Position - Vector3.up * 0.5f);
                Gizmos.DrawLine(item.Value.Position + (Vector3.right * 0.5f), item.Value.Position - Vector3.right * 0.5f);
                Gizmos.DrawLine(item.Value.Position + (Vector3.forward * 0.5f), item.Value.Position - Vector3.forward * 0.5f);

                Gizmos.color = oldColor;
            }
        }

        private void DrawRays() {
            foreach (var item in Rays) {
                Color oldColor = Gizmos.color;
                Gizmos.color = item.Value.Color;

                Gizmos.DrawRay(item.Value.Position, item.Value.Direction);

                Gizmos.color = oldColor;

            }
        }

        private static void DrawLines() {
            foreach (var item in Lines) {
                var oldColor = Gizmos.color;
                Gizmos.color = item.Value.Color;

                Gizmos.DrawLine(item.Value.StartPos, item.Value.EndPos);

                Gizmos.color = oldColor;
            }
        }

        private void DrawCircles() {
            foreach (var item in Circles) {
                DrawCircleImp(item.Value.Position, item.Value.Up, item.Value.Color, item.Value.Radius);
            }
        }

        private static void DrawCircleImp(Vector3 center, Vector3 up, Color color, float radius) {
            var oldColor = Gizmos.color;
            Gizmos.color = color;

            up = (up == Vector3.zero ? Vector3.up : up).normalized * radius;
            var forward = Vector3.Slerp(up, -up, 0.5f);
            var right = Vector3.Cross(up, forward).normalized * radius;
            for (var i = 1; i < 26; i++) {
                Gizmos.DrawLine(center + Vector3.Slerp(forward, right, (i - 1) / 25f), center + Vector3.Slerp(forward, right, i / 25f));
                Gizmos.DrawLine(center + Vector3.Slerp(forward, -right, (i - 1) / 25f), center + Vector3.Slerp(forward, -right, i / 25f));
                Gizmos.DrawLine(center + Vector3.Slerp(right, -forward, (i - 1) / 25f), center + Vector3.Slerp(right, -forward, i / 25f));
                Gizmos.DrawLine(center + Vector3.Slerp(-right, -forward, (i - 1) / 25f), center + Vector3.Slerp(-right, -forward, i / 25f));
            }

            Gizmos.color = oldColor;
        }

        private void DrawCubes() {
            foreach (var item in Cubes) {
                DrawCubeImp(item.Value.Position, item.Value.Center, item.Value.Size, item.Value.Scale, item.Value.Up, item.Value.Forward, item.Value.Color);
            }
        }

        private static void DrawCubeImp(Vector3 pos, Vector3 center, Vector3 size, Vector3 scale, Vector3 up, Vector3 forward, Color color) {
            var oldColor = Gizmos.color;
            Gizmos.color = color;

            Vector3 realCenter = pos + new Vector3(center.x * scale.x, center.y * scale.y, center.z * scale.z);
            Vector3 realSize = new Vector3(size.x * scale.x / 2, size.y * scale.y / 2, size.z * scale.z / 2);
            Vector3 right = Vector3.Cross(up, forward);
            // 八顶点
            Vector3 a = realCenter + realSize.x * right + realSize.y * up + realSize.z * forward;
            Vector3 b = realCenter + realSize.x * right + realSize.y * up - realSize.z * forward;
            Vector3 c = realCenter + realSize.x * right - realSize.y * up + realSize.z * forward;
            Vector3 d = realCenter + realSize.x * right - realSize.y * up - realSize.z * forward;
            Vector3 e = realCenter - realSize.x * right + realSize.y * up + realSize.z * forward;
            Vector3 f = realCenter - realSize.x * right + realSize.y * up - realSize.z * forward;
            Vector3 g = realCenter - realSize.x * right - realSize.y * up + realSize.z * forward;
            Vector3 h = realCenter - realSize.x * right - realSize.y * up - realSize.z * forward;
            // 十二条边
            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(c, d);
            Gizmos.DrawLine(e, f);
            Gizmos.DrawLine(g, h);
            Gizmos.DrawLine(a, e);
            Gizmos.DrawLine(b, f);
            Gizmos.DrawLine(d, h);
            Gizmos.DrawLine(c, g);
            Gizmos.DrawLine(a, c);
            Gizmos.DrawLine(e, g);
            Gizmos.DrawLine(f, h);
            Gizmos.DrawLine(b, d);

            Gizmos.color = oldColor;
        }

        private void DrawSpheres() {
            foreach (var item in Spheres) {
                Sphere sphere = item.Value;
                Color oldColor = Gizmos.color;
                Gizmos.color = item.Value.Color;

                float scale = Mathf.Max(Mathf.Max(Mathf.Abs(sphere.Scale.x), Mathf.Abs(sphere.Scale.y)), Mathf.Abs(sphere.Scale.z));
                float realRadius = sphere.Radius * scale;
                Vector3 realCenter = sphere.Position + new Vector3(sphere.Center.x * sphere.Scale.x, sphere.Center.y * sphere.Scale.y, sphere.Center.z * sphere.Scale.z);
                //Gizmos.DrawSphere(sphere.Position + sphere.Center, sphere.Radius * scale);
                //DrawCapsuleImp(sphere.Position, sphere.Center, sphere.Scale, CapsuleDirection.YAxis, sphere.Radius, sphere.Radius, sphere.Color);
                DrawCircleImp(realCenter, Vector3.up, sphere.Color, realRadius);
                DrawCircleImp(realCenter, Vector3.forward, sphere.Color, realRadius);
                DrawCircleImp(realCenter, Vector3.right, sphere.Color, realRadius);

                Gizmos.color = oldColor;
            }
        }

        private void DrawCapsules() {
            foreach (var item in Capsules) {
                DrawCapsuleImp(item.Value.Position, item.Value.Center, item.Value.Scale, item.Value.Direction, item.Value.Radius, item.Value.Height, item.Value.Color);
            }
        }

        private void DrawCapsuleImp(Vector3 pos, Vector3 center, Vector3 scale, CapsuleDirection direction, float radius, float height, Color color) {
            // 参数保护
            if (height < 0f) {
                Debug.LogWarning("Capsule height can not be negative!");
                return;
            }
            if (radius < 0f) {
                Debug.LogWarning("Capsule radius can not be negative!");
                return;
            }
            // 根据朝向找到up 和 高度缩放值
            Vector3 up = Vector3.up;
            // 半径缩放值
            float radiusScale = 1f;
            // 高度缩放值
            float heightScale = 1f;
            switch (direction) {
                case CapsuleDirection.XAxis:
                    up = Vector3.right;
                    heightScale = Mathf.Abs(scale.x);
                    radiusScale = Mathf.Max(Mathf.Abs(scale.y), Mathf.Abs(scale.z));
                    break;
                case CapsuleDirection.YAxis:
                    up = Vector3.up;
                    heightScale = Mathf.Abs(scale.y);
                    radiusScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
                    break;
                case CapsuleDirection.ZAxis:
                    up = Vector3.forward;
                    heightScale = Mathf.Abs(scale.z);
                    radiusScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y));
                    break;
            }

            float realRadius = radiusScale * radius;
            height = height * heightScale;
            float sideHeight = Mathf.Max(height - 2 * realRadius, 0f);

            center = new Vector3(center.x * scale.x, center.y * scale.y, center.z * scale.z);
            // 为了符合Unity的CapsuleCollider的绘制样式，调整位置
            pos = pos - up.normalized * (sideHeight * 0.5f + realRadius) + center;

            Color oldColor = Gizmos.color;
            Gizmos.color = color;

            up = up.normalized * realRadius;
            Vector3 forward = Vector3.Slerp(up, -up, 0.5f);
            Vector3 right = Vector3.Cross(up, forward).normalized * realRadius;

            Vector3 start = pos + up;
            Vector3 end = pos + up.normalized * (sideHeight + realRadius);

            // 半径圆
            DrawCircleImp(start, up, color, realRadius);
            DrawCircleImp(end, up, color, realRadius);

            // 边线
            Gizmos.DrawLine(start - forward, end - forward);
            Gizmos.DrawLine(start + right, end + right);
            Gizmos.DrawLine(start - right, end - right);
            Gizmos.DrawLine(start + forward, end + forward);
            Gizmos.DrawLine(start - forward, end - forward);

            for (int i = 1; i < 26; i++) {
                // 下部的头
                Gizmos.DrawLine(start + Vector3.Slerp(right, -up, (i - 1) / 25f), start + Vector3.Slerp(right, -up, i / 25f));
                Gizmos.DrawLine(start + Vector3.Slerp(-right, -up, (i - 1) / 25f), start + Vector3.Slerp(-right, -up, i / 25f));
                Gizmos.DrawLine(start + Vector3.Slerp(forward, -up, (i - 1) / 25f), start + Vector3.Slerp(forward, -up, i / 25f));
                Gizmos.DrawLine(start + Vector3.Slerp(-forward, -up, (i - 1) / 25f), start + Vector3.Slerp(-forward, -up, i / 25f));

                // 上部的头
                Gizmos.DrawLine(end + Vector3.Slerp(forward, up, (i - 1) / 25f), end + Vector3.Slerp(forward, up, i / 25f));
                Gizmos.DrawLine(end + Vector3.Slerp(-forward, up, (i - 1) / 25f), end + Vector3.Slerp(-forward, up, i / 25f));
                Gizmos.DrawLine(end + Vector3.Slerp(right, up, (i - 1) / 25f), end + Vector3.Slerp(right, up, i / 25f));
                Gizmos.DrawLine(end + Vector3.Slerp(-right, up, (i - 1) / 25f), end + Vector3.Slerp(-right, up, i / 25f));
            }

            Gizmos.color = oldColor;
        }

        #endregion private implements
    }
}