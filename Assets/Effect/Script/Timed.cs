using UnityEngine;
using System.Collections;

public class Timed : MonoBehaviour
{
    public float m_fDestruktionSpeed = 0.1f;
    public Material m_Mat;
    public float m_fTime;

	void Start () {
        m_Mat = GetComponent<Renderer>().material;
	}

	void Update () {
        m_fTime += Time.deltaTime * m_fDestruktionSpeed;
        if (m_fTime >= 1.5f)
            m_fTime = 0;
        m_Mat.SetFloat("_Amount", m_fTime);
	}
}
