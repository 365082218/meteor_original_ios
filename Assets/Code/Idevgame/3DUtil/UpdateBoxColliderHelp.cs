using UnityEngine;
public class UpdateBoxColliderHelp {
    private Transform m_Transform;
    private MeshRenderer m_MeshRenderer;
    private BoxCollider m_BoxCollider;
    public UpdateBoxColliderHelp(Transform transform) {
        m_Transform = transform;
        m_MeshRenderer = m_Transform.GetComponent<MeshRenderer>();
        m_BoxCollider = m_Transform.GetComponent<BoxCollider>();
        UpdateBoxCollider();
    }
    public void UpdateBoxCollider() {
        if (m_MeshRenderer == null || m_BoxCollider == null) {
            Debug.Log(string.Format("对象{0}没有指定控件，跳过。", m_Transform.name));
            return;
        }
        Vector3 position = m_Transform.position;
        Vector3 scale = m_Transform.localScale;
        Quaternion rotation = m_Transform.rotation;
        m_Transform.position = Vector3.zero;
        m_Transform.localScale = Vector3.one;
        m_Transform.rotation = new Quaternion(0, 0, 0, 1);
        m_BoxCollider.size = m_MeshRenderer.bounds.size;
        m_BoxCollider.center = m_MeshRenderer.bounds.center;
        m_Transform.position = position;
        m_Transform.localScale = scale;
        m_Transform.rotation = rotation;
        Debug.Log(string.Format("对象{0}的BoxCollider更新完毕。", m_Transform.name));
    }
}