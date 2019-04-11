using UnityEngine;
using System.Collections;

public class LookAtCamara : MonoBehaviour 
{
    // Update is called once per frame
    //   ;
    void LateUpdate()
    {
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            transform.Rotate(new Vector3(-90, 0, 0));
        }
    }
}
