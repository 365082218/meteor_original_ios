using UnityEngine;
using System.Collections;

public class SetFan2DAngle_OnlyDrawEdge : MonoBehaviour
{
    Vector3 OrgScale;
    Vector3 OrgRotation;
    public int mRotation = 0;
    public int mAngle = 45;
    public int mRadius = 1;
    //public float mfSpeed = 10.0f;
    float mfSpeed = 10.0f;
    public Vector2 mRotationCenter = new Vector2(0.5f, 0.5f);

    public UISprite uiss;

    public Color cNormalColor;

    void Awake()
    {
        OrgScale = this.transform.localScale;
        OrgRotation = this.transform.localRotation.eulerAngles;

        //GetComponent<UIPanel>().renderer.sharedMaterial.renderQueue = 300;
        //gameObject.GetComponentInChildren<UISprite>().renderer.sharedMaterial.renderQueue = 2999;

        //cNormalColor = Color.white;
    }


    void Update()
    {
        if (mRadius < 1)
            mRadius = 1;


        //this.transform.localScale = (new Vector3(OrgScale.x * mRadius, OrgScale.y * mRadius, 1));

        //this.transform.localRotation = Quaternion.Euler(new Vector3(OrgRotation.x, OrgRotation.y + mRotation, OrgRotation.z + mAngle / 2));

        if (mAngle > 360)
            mAngle = 360;
        if (mAngle < 0)
            mAngle = 0;

        //cNormalColor = Color.white;

        if (mAngle == 360)
            cNormalColor.a = 0;
        else
            cNormalColor.a = 255;

        GetComponent<Renderer>().sharedMaterial.SetColor("_Color", cNormalColor);

        uiss.fillAmount = (float)mAngle / 360;

        //Debug.Log("Time.time * mfSpeed " + Time.time * mfSpeed);

        //Quaternion rot = Quaternion.Euler(0.0f, 0.0f, Time.time * mfSpeed);
        Quaternion rot = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        Matrix4x4 m = Matrix4x4.TRS(Vector3.zero, rot, Vector3.one);
        Matrix4x4 t = Matrix4x4.TRS(-mRotationCenter, Quaternion.identity, Vector3.one);
        Matrix4x4 t_inverse = Matrix4x4.TRS(mRotationCenter, Quaternion.identity, Vector3.one);
        GetComponent<Renderer>().sharedMaterial.SetMatrix("_Rotation01", t_inverse * m * t);

        rot = Quaternion.Euler(0.0f, 0.0f, 360-mAngle);
        m = Matrix4x4.TRS(Vector3.zero, rot, Vector3.one);
        t = Matrix4x4.TRS(-mRotationCenter, Quaternion.identity, Vector3.one);
        t_inverse = Matrix4x4.TRS(mRotationCenter, Quaternion.identity, Vector3.one);
        GetComponent<Renderer>().sharedMaterial.SetMatrix("_Rotation02", t_inverse * m * t);

        if (GetComponent<Renderer>().sharedMaterial.HasProperty("_Progress"))
            GetComponent<Renderer>().sharedMaterial.SetFloat("_Progress", (float)mAngle / 360);
    }
}