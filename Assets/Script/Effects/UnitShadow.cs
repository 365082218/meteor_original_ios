using UnityEngine;
using System.Collections;

public class UnitShadow : MonoBehaviour
{
    Vector3 mInitOffset = Vector3.zero;
    Transform mCacheTransform;
    Renderer mCacheRenderer;
    bool mLastOnGround = false;
    MeteorUnit mOwner;

    public Transform LockTarget;
    
    const float CheckDistance = 200f;
    const int DefaultLayer = 0;
    const int SceneLayer = 10;
    const int LayerMask = (1 << DefaultLayer) | (1 << SceneLayer);

    // Use this for initialization
    void Start()
    {
        mCacheTransform = transform;
        mCacheRenderer = GetComponent<Renderer>();
        mInitOffset = mCacheTransform.localPosition;
        mOwner = mCacheTransform.parent.GetComponent<MeteorUnit>();
    }

    // Update is called once per frame
    void Update()
    {
        // deactivate the shadow while the owner is dead.
        if (mOwner == null || mOwner.Dead)
        {
            mCacheRenderer.enabled = false;
            return;
        }

        mCacheRenderer.enabled = true;
        // we only do raycast shadow when unit on air.
        if (mOwner.OnGround != mLastOnGround || !mOwner.OnGround)
        {
            RaycastHit hitInfo;
            Vector3 checkPos = mCacheTransform.parent.position;
            if (LockTarget)
            {
                checkPos.x = LockTarget.position.x;
                checkPos.z = LockTarget.position.z;
            }

            if (Physics.Raycast(checkPos + Vector3.up, Vector3.down, out hitInfo, CheckDistance, LayerMask))
                mCacheTransform.position = mInitOffset + hitInfo.point;
            mLastOnGround = mOwner.OnGround;
        }
        else if (LockTarget)
        {
            Vector3 checkPos = LockTarget.position;
            checkPos.y = mCacheTransform.parent.position.y;
            mCacheTransform.position = checkPos + mInitOffset;
        }
    }
}
