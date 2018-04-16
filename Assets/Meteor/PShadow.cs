using UnityEngine;
using System.Collections;

  
public class PShadow : MonoBehaviour
{
    [SerializeField]
    protected Camera m_Cam; //产生阴影的照相机  
    [SerializeField]
    protected Shader m_ShadowShader;
    [SerializeField]
    protected Projector m_ShadowProjector;    //阴影接受物  

    private RenderTexture m_RT;
    Material mat;
    // Use this for initialization  
    void Start()
    {
        m_RT = new RenderTexture(512, 512, 8);
        m_Cam.targetTexture = m_RT;
        mat = new Material(m_ShadowShader);
        mat.SetTexture("_MainTex", m_RT);
        mat.SetMatrix("_WorldToCameraMatrix", m_Cam.worldToCameraMatrix);
        mat.SetMatrix("_ProjectionMatrix", m_Cam.projectionMatrix);
        m_ShadowProjector.material = mat;
    }

    private void Update()
    {
    }
}