/*
	SetRenderQueue.cs
 
	Sets the RenderQueue of an object's materials on Awake. This will instance
	the materials, so the script won't interfere with other renderers that
	reference the same materials.
*/

using UnityEngine;

[AddComponentMenu("Rendering/ldaSetRenderQueue")]

public class ldaSetRenderQueue : MonoBehaviour
{

    [SerializeField]
    protected int[] m_queues = new int[] { 3000 };

    protected void Awake()
    {
        Material[] materials = GetComponent<Renderer>().materials;
        for (int i = 0; i < materials.Length && i < m_queues.Length; ++i)
        {
            materials[i].renderQueue = m_queues[i];
            //materials[i].shader.renderQueue = m_queues[i];
            GetComponent<Renderer>().sharedMaterial.renderQueue = m_queues[i];
        }
    }

    void Update ()
    {
        Material[] materials = GetComponent<Renderer>().materials;
        for (int i = 0; i < materials.Length && i < m_queues.Length; ++i)
        {
            materials[i].renderQueue = m_queues[i];
            //materials[i].shader.renderQueue = m_queues[i];
            GetComponent<Renderer>().sharedMaterial.renderQueue = m_queues[i];
        }
    }
}