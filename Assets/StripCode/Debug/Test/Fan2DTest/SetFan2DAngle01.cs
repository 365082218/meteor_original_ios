using UnityEngine;
using System.Collections;

public class SetFan2DAngle01 : MonoBehaviour
{
    public int mRotation = 0;
    public int mAngle = 45;
    public float mRadius = 15;
    //public float mfSpeed = 10.0f;
    float mfSpeed = 10.0f;
    public Vector2 mRotationCenter = new Vector2(0.5f, 0.5f);


    void Update()
    {
        if (mRadius < 5)
            mRadius = 5;

        this.transform.localScale = (new Vector3(mRadius, mRadius, 1));

        this.transform.localRotation = Quaternion.Euler(new Vector3(270, mRotation,0));

        if (mAngle > 360)
            mAngle = 360;
        if (mAngle < 0)
            mAngle = 0;

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