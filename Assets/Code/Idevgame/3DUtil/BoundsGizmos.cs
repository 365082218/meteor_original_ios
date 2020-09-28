using System.Collections.Generic;
using UnityEngine;
using ShortcutExtension;

//AABB盒绘制
//调用AddCollider即可绘制指定的碰撞盒.
public class BoundsGizmos : MonoBehaviour
{
    private static BoundsGizmos _instance;
    public static BoundsGizmos Instance {
        get {
            if (_instance != null) return _instance;
            if (Main.Ins.MainCamera == null)
                return null;
            _instance = Main.Ins.MainCamera.GetComponent<BoundsGizmos>();
            if (_instance == null) {
                _instance = Main.Ins.MainCamera.gameObject.AddComponent<BoundsGizmos>();
            }
            return _instance;
        }
    }

    private Bounds bounds;
    private Vector3 center;
    private Vector3 ext;
    private Vector3[] points;
    private List<BoxCollider> hitcolliders = new List<BoxCollider>();
    private List<Collider> colliders = new List<Collider>();
    private List<Transform> transforms = new List<Transform>();
    float deltaX;
    float deltaY;
    float deltaZ;
    private void Start()
    {
        points = new Vector3[8];
        lineMatAABB = new Material(ShaderMng.Find("Unlit-Color"));
        lineMatAABB.color = new Color(0.0f, 0.0f, 1.0f, 1.0f);
        lineMatOBB = new Material(ShaderMng.Find("Unlit-Color"));
        lineMatOBB.color = new Color(1.0f, 1.0f, 0.0f, 1.0f);
    }

    public void AddTransform(Transform t) {
        if (!transforms.Contains(t))
            transforms.Add(t);
    }

    public void AddColliders(Collider[] colliderlist) {
        for (int i = 0; i < colliderlist.Length; i++) {
            if (!colliders.Contains(colliderlist[i])) {
                colliders.Add(colliderlist[i]);
            }
        }
    }

    public void AddColliders(List<Collider> colliderlist) {
        for (int i = 0; i < colliderlist.Count; i++) {
            if (!colliders.Contains(colliderlist[i])) {
                colliders.Add(colliderlist[i]);
            }
        }
    }

    public void AddCollider(Collider co) {
        if (!colliders.Contains(co)) {
            colliders.Add(co);
        }
    }

    public void Clear() {
        hitcolliders.Clear();
        colliders.Clear();
        transforms.Clear();
    }

    Material lineMatAABB;//蓝色AABB-包围角色整个
    Material lineMatOBB;//红色OBB
    private void OnPostRender() {
        //绘制角色的大整体AABB包围盒.
        for (int i = 0; i < transforms.Count; i++) {
            if (!transforms[i])
                continue;
            bounds = transforms[i].GetBounds();
            center = bounds.center;
            ext = bounds.extents;

            deltaX = Mathf.Abs(ext.x);
            deltaY = Mathf.Abs(ext.y);
            deltaZ = Mathf.Abs(ext.z);
            points[0] = center + new Vector3(-deltaX, deltaY, -deltaZ);        // 上前左（相对于中心点）
            points[1] = center + new Vector3(deltaX, deltaY, -deltaZ);         // 上前右
            points[2] = center + new Vector3(deltaX, deltaY, deltaZ);          // 上后右
            points[3] = center + new Vector3(-deltaX, deltaY, deltaZ);         // 上后左

            points[4] = center + new Vector3(-deltaX, -deltaY, -deltaZ);       // 下前左
            points[5] = center + new Vector3(deltaX, -deltaY, -deltaZ);        // 下前右
            points[6] = center + new Vector3(deltaX, -deltaY, deltaZ);         // 下后右
            points[7] = center + new Vector3(-deltaX, -deltaY, deltaZ);        // 下后左

            //AABB盒都有。
            GL.Begin(GL.LINES);
            lineMatAABB.SetPass(0);

            GL.Color(new Color(0.0f, 0.0f, 1.0f, 1.0f));
            DrawLine(points[0], points[1]);
            DrawLine(points[0], points[3]);
            DrawLine(points[0], points[4]);
            DrawLine(points[2], points[1]);
            DrawLine(points[2], points[3]);
            DrawLine(points[2], points[6]);
            DrawLine(points[5], points[1]);
            DrawLine(points[5], points[4]);
            DrawLine(points[5], points[6]);
            DrawLine(points[7], points[3]);
            DrawLine(points[7], points[4]);
            DrawLine(points[7], points[6]);
            GL.End();
        }

        for (int i = 0; i < colliders.Count; i++) {
            if (!colliders[i])
                continue;
            bounds = colliders[i].transform.GetBounds();
            center = bounds.center;
            ext = bounds.extents;

            deltaX = Mathf.Abs(ext.x);
            deltaY = Mathf.Abs(ext.y);
            deltaZ = Mathf.Abs(ext.z);
            points[0] = center + new Vector3(-deltaX, deltaY, -deltaZ);        // 上前左（相对于中心点）
            points[1] = center + new Vector3(deltaX, deltaY, -deltaZ);         // 上前右
            points[2] = center + new Vector3(deltaX, deltaY, deltaZ);          // 上后右
            points[3] = center + new Vector3(-deltaX, deltaY, deltaZ);         // 上后左

            points[4] = center + new Vector3(-deltaX, -deltaY, -deltaZ);       // 下前左
            points[5] = center + new Vector3(deltaX, -deltaY, -deltaZ);        // 下前右
            points[6] = center + new Vector3(deltaX, -deltaY, deltaZ);         // 下后右
            points[7] = center + new Vector3(-deltaX, -deltaY, deltaZ);        // 下后左

            //AABB盒都有。
            GL.Begin(GL.LINES);
            lineMatAABB.SetPass(0);

            GL.Color(new Color(1.0f, 0.0f, 0.0f, 1.0f));
            DrawLine(points[0], points[1]);
            DrawLine(points[0], points[3]);
            DrawLine(points[0], points[4]);
            DrawLine(points[2], points[1]);
            DrawLine(points[2], points[3]);
            DrawLine(points[2], points[6]);
            DrawLine(points[5], points[1]);
            DrawLine(points[5], points[4]);
            DrawLine(points[5], points[6]);
            DrawLine(points[7], points[3]);
            DrawLine(points[7], points[4]);
            DrawLine(points[7], points[6]);
            GL.End();
            GL.Begin(GL.LINES);
            lineMatOBB.SetPass(0);
            GL.Color(new Color(0.0f, 1.0f, 0.0f, 1.0f));
            if (colliders[i] is BoxCollider) {
                center = (colliders[i] as BoxCollider).center;
                ext = (colliders[i] as BoxCollider).size;
                deltaX = Mathf.Abs(ext.x / 2);
                deltaY = Mathf.Abs(ext.y / 2);
                deltaZ = Mathf.Abs(ext.z / 2);
                points[0] = colliders[i].transform.TransformPoint(center + new Vector3(-deltaX, deltaY, -deltaZ));        // 上前左（相对于中心点）
                points[1] = colliders[i].transform.TransformPoint(center + new Vector3(deltaX, deltaY, -deltaZ));         // 上前右
                points[2] = colliders[i].transform.TransformPoint(center + new Vector3(deltaX, deltaY, deltaZ));          // 上后右
                points[3] = colliders[i].transform.TransformPoint(center + new Vector3(-deltaX, deltaY, deltaZ));         // 上后左

                points[4] = colliders[i].transform.TransformPoint(center + new Vector3(-deltaX, -deltaY, -deltaZ));       // 下前左
                points[5] = colliders[i].transform.TransformPoint(center + new Vector3(deltaX, -deltaY, -deltaZ));        // 下前右
                points[6] = colliders[i].transform.TransformPoint(center + new Vector3(deltaX, -deltaY, deltaZ));         // 下后右
                points[7] = colliders[i].transform.TransformPoint(center + new Vector3(-deltaX, -deltaY, deltaZ));        // 下后左
                DrawLine( points[0],  points[1]);
                DrawLine( points[0],  points[3]);
                DrawLine( points[0],  points[4]);
                DrawLine( points[2],  points[1]);
                DrawLine( points[2],  points[3]);
                DrawLine( points[2],  points[6]);
                DrawLine( points[5],  points[1]);
                DrawLine( points[5],  points[4]);
                DrawLine( points[5],  points[6]);
                DrawLine( points[7],  points[3]);
                DrawLine( points[7],  points[4]);
                DrawLine( points[7],  points[6]);

            } else if (colliders[i] is MeshCollider) {
                center = (colliders[i] as MeshCollider).sharedMesh.bounds.center;
                ext = (colliders[i] as MeshCollider).sharedMesh.bounds.size;
                deltaX = Mathf.Abs(ext.x / 2);
                deltaY = Mathf.Abs(ext.y / 2);
                deltaZ = Mathf.Abs(ext.z / 2);
                points[0] = colliders[i].transform.TransformPoint(center + new Vector3(-deltaX, deltaY, -deltaZ));        // 上前左（相对于中心点）
                points[1] = colliders[i].transform.TransformPoint(center + new Vector3(deltaX, deltaY, -deltaZ));         // 上前右
                points[2] = colliders[i].transform.TransformPoint(center + new Vector3(deltaX, deltaY, deltaZ));          // 上后右
                points[3] = colliders[i].transform.TransformPoint(center + new Vector3(-deltaX, deltaY, deltaZ));         // 上后左

                points[4] = colliders[i].transform.TransformPoint(center + new Vector3(-deltaX, -deltaY, -deltaZ));       // 下前左
                points[5] = colliders[i].transform.TransformPoint(center + new Vector3(deltaX, -deltaY, -deltaZ));        // 下前右
                points[6] = colliders[i].transform.TransformPoint(center + new Vector3(deltaX, -deltaY, deltaZ));         // 下后右
                points[7] = colliders[i].transform.TransformPoint(center + new Vector3(-deltaX, -deltaY, deltaZ));        // 下后左
                DrawLine(points[0], points[1]);
                DrawLine(points[0], points[3]);
                DrawLine(points[0], points[4]);
                DrawLine(points[2], points[1]);
                DrawLine(points[2], points[3]);
                DrawLine(points[2], points[6]);
                DrawLine(points[5], points[1]);
                DrawLine(points[5], points[4]);
                DrawLine(points[5], points[6]);
                DrawLine(points[7], points[3]);
                DrawLine(points[7], points[4]);
                DrawLine(points[7], points[6]);
            }
            
            GL.End();
        }
    }
    
    void DrawLine(Vector3 pos0, Vector3 pos1) {
        GL.Vertex3(pos0.x, pos0.y, pos0.z);
        GL.Vertex3(pos1.x, pos1.y, pos1.z);
    }
}
