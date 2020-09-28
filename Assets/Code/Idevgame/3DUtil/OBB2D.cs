using System;
using UnityEngine;
public class OBB2D {
    public Transform m_Transform;
    public BoxCollider m_BoxCollider;
    private double m_Rotation;
    public Vector2 m_Extents;
    public Vector2[] m_Axiss;
    public OBB2D(Transform transform, BoxCollider boxCollider) {
        m_Transform = transform;
        m_BoxCollider = m_Transform.GetComponent<BoxCollider>();
        m_Axiss = new Vector2[2];
        SetExtents();
    }

    private void SetExtents() {
        Quaternion rotation = m_Transform.rotation;
        m_Transform.rotation = new Quaternion(0, 0, 0, 1);
        Vector3 center = m_BoxCollider.center;
        Vector3 size = m_BoxCollider.size / 2;
        Vector3 Point1 = new Vector3(center.x + size.x, center.y, center.z - size.z);
        Vector3 Point2 = new Vector3(center.x - size.x, center.y, center.z + size.z);
        Point1 = m_Transform.localToWorldMatrix.MultiplyPoint3x4(Point1);
        Point2 = m_Transform.localToWorldMatrix.MultiplyPoint3x4(Point2);
        m_Extents = new Vector2(Mathf.Abs(Point1.x - Point2.x) / 2, Mathf.Abs(Point2.z - Point1.z) / 2);
        m_Transform.rotation = rotation;
    }

    public float dot(Vector2 a, Vector2 b) {
        return Mathf.Abs(a.x * b.x + a.y * b.y);
    }

    public float getProjectionRadius(Vector2 axis) {
        return (m_Extents.x * dot(m_Axiss[0], axis) + m_Extents.y * dot(m_Axiss[1], axis));
    }

    public void Update() {
        m_Rotation = m_Transform.eulerAngles.y * Math.PI / 180;
        m_Axiss[0] = new Vector2((float)Math.Cos(m_Rotation), -(float)Math.Sin(m_Rotation));
        m_Axiss[1] = new Vector2(-m_Axiss[0].y, m_Axiss[0].x);
    }

    public bool intersects(OBB2D other) {
        Update();
        other.Update();
        Vector2 distanceVector = new Vector2(m_Transform.position.x - other.m_Transform.position.x, m_Transform.position.z - other.m_Transform.position.z);

        Vector2[] checkObbVector2s =
        {
            m_Axiss[0],
            m_Axiss[1],
            other.m_Axiss[0],
            other.m_Axiss[1],
        };
        for (int index = 0; index < checkObbVector2s.Length; index++) {
            Vector2 curVector2 = checkObbVector2s[index];
            if ((getProjectionRadius(curVector2) + other.getProjectionRadius(curVector2)) <= dot(distanceVector, curVector2)) {
                return false;
            }
        }
        return true;
    }
}