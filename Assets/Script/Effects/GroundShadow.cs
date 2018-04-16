using UnityEngine;
using System.Collections;

public class GroundShadow : MonoBehaviour {

    public Vector3 Offset = new Vector3(0, 0.2f, 0);
	Transform mUnitTransform;
    Transform mTransform;

	// Use this for initialization
	void Start () {
        mTransform = gameObject.transform;
		mUnitTransform = mTransform.parent;
	}
	
	// Update is called once per frame
	void Update () {
        Ray ray = new Ray(mUnitTransform.position + Offset, Vector3.down);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, 5.0f))
            mTransform.position = hitInfo.point + Offset;
	}
}
