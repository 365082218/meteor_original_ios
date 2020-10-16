using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CollisionCheck {
    private static Vector3 _AB = new Vector3(); // Direction A to B
    private static float[] _R = new float[9]; // 3x3 Rotation
    private static float[] _AbsR = new float[9]; // 3x3 Rotation
    private static Vector3 _AX = new Vector3(); // A Axis
    private static Vector3 _BX = new Vector3(); // B Axis

    private static Vector3 _v1 = new Vector3();
    private static Vector3 _v2 = new Vector3();
    private static Vector3 _v3 = new Vector3();

    private static float[] _aRot = new float[9];
    private static float[] _bRot = new float[9];

    private static float ar = 0.0f, br = 0.0f;

    private static float[] _identityMatrix = new float[9] { 1.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 1.0f };

    private static Collider[] sphere_cast_collider_storage = new Collider[128];

    public static Vector3 HalfExtentsFromBoxCollider(BoxCollider boxCollider1) {
        return new Vector3(boxCollider1.size.x * boxCollider1.transform.localScale.x * 0.5f, boxCollider1.size.y * boxCollider1.transform.localScale.y * 0.5f, boxCollider1.size.z * boxCollider1.transform.localScale.z * 0.5f);
    }
    public static Vector3 PositionFromCollider(BoxCollider collider) {
        return collider.transform.TransformPoint(collider.center);
    }

    public static int OverlapBox(Vector3 center, Vector3 halfExtents, Quaternion rotation, Collider[] colliderStorage, LayerMask killLayer) {
        var radius = new Vector3(halfExtents.x * 2.0f, halfExtents.y * 2.0f, halfExtents.z * 2.0f).magnitude;
        int numOverlaps = Physics.OverlapSphereNonAlloc(center, radius, sphere_cast_collider_storage, killLayer.value);
        int numBoxOverlaps = 0;
        for (int i = 0; i < numOverlaps; i++) {
            var nearbyCollider = sphere_cast_collider_storage[i];
            //Debug.Log("Testing against " + nearbyCollider.gameObject.name);
            var nearbyBoxCollider = nearbyCollider as BoxCollider;
            if (nearbyBoxCollider != null) {
                var nearbyColliderHalfExtents = nearbyBoxCollider != null ? HalfExtentsFromBoxCollider(nearbyBoxCollider) : nearbyCollider.transform.localScale * 0.5f;
                var nearbyColliderPosition = nearbyBoxCollider != null ? PositionFromCollider(nearbyBoxCollider) : nearbyCollider.transform.position;
                //Debug.Log("Position: " + nearbyCollider.transform.position);
                //Debug.Log("Rotation: " + nearbyCollider.transform.rotation);
                //Debug.Log("Half Extents: " + nearbyColliderHalfExtents);
                //Debug.Log("Has box: " + (nearbyBoxCollider != null).ToString());
                if (CollisionCheck.Box_Box(center, halfExtents, rotation, nearbyColliderPosition, nearbyColliderHalfExtents, nearbyCollider.transform.rotation)) {
                    colliderStorage[numBoxOverlaps++] = sphere_cast_collider_storage[i];
                }
            } else {
                var nearbyMeshCollider = nearbyCollider as MeshCollider;
                if (nearbyMeshCollider != null) {
                    //计算这个盒子的obb，转换为obb-obb的相交测试
                    Vector3 centerMesh = nearbyMeshCollider.transform.TransformPoint(nearbyMeshCollider.sharedMesh.bounds.center);
                    Vector3 halfMesh = Vector3.Scale(nearbyMeshCollider.sharedMesh.bounds.size, nearbyMeshCollider.transform.localScale) / 2;
                    if (CollisionCheck.Box_Box(center, halfExtents, rotation, centerMesh, halfMesh, nearbyCollider.transform.rotation)) {
                        colliderStorage[numBoxOverlaps++] = sphere_cast_collider_storage[i];
                    }
                }
            }
        }
        return numBoxOverlaps;
    }

    // Adapted from: [URL]http://www.gamasutra.com/view/feature/131790/simple_intersection_tests_for_games.php?print=1[/URL]
    public static bool Box_Box(Vector3 aPos, Vector3 aSize, Quaternion aQuat, Vector3 bPos, Vector3 bSize, Quaternion bQuat) {
        ar = 0.0f;
        br = 0.0f;

        QuaternionToFloatArray(aQuat, _aRot);
        QuaternionToFloatArray(bQuat, _bRot);


        for (int i = 0, i1 = 0; i < 3; i++, i1 += 3) {
            _AX.Set(_aRot[i1], _aRot[i1 + 1], _aRot[i1 + 2]);
            for (int j = 0, j1 = 0; j < 3; j++, j1 += 3) {
                _BX.Set(_bRot[j1], _bRot[j1 + 1], _bRot[j1 + 2]);
                _R[i1 + j] = Vector3.Dot(_AX, _BX);
            }
        }

        _AB = bPos - aPos;

        _v1.Set(_aRot[0], _aRot[1], _aRot[2]);
        _v2.Set(_aRot[3], _aRot[4], _aRot[5]);
        _v3.Set(_aRot[6], _aRot[7], _aRot[8]);

        _AB.Set(Vector3.Dot(_AB, _v1), Vector3.Dot(_AB, _v2), Vector3.Dot(_AB, _v3));

        for (int i = 0; i < 9; i++) {
            _AbsR[i] = Mathf.Abs(_R[i]) + 0.001f;
        }
        // Test axes L = A0, L = A1, L = A2
        for (int i = 0, i1 = 0; i < 3; i++, i1 += 3) {
            ar = aSize[i];
            br = (bSize[0] * _AbsR[i1]) + (bSize[1] * _AbsR[i1 + 1]) + (bSize[2] * _AbsR[i1 + 2]);
            if (Mathf.Abs(_AB[i]) > (ar + br)) { return false; }
        }
        // Test axes L = B0, L = B1, L = B2
        for (int i = 0; i < 3; i++) {
            ar = (aSize[0] * _AbsR[i]) + (aSize[1] * _AbsR[i + 3]) + (aSize[2] * _AbsR[i + 6]);
            br = bSize[i];
            if (Mathf.Abs((_AB[0] * _R[i]) + (_AB[1] * _R[i + 3]) + (_AB[2] * _R[i + 6])) > (ar + br)) { return false; }
        }
        // Test axis L = A0 x B0
        ar = (aSize[1] * _AbsR[6]) + (aSize[2] * _AbsR[3]);
        br = (bSize[1] * _AbsR[2]) + (bSize[2] * _AbsR[1]);
        if (Mathf.Abs(_AB[2] * _R[3] - _AB[1] * _R[6]) > (ar + br)) { return false; }
        // Test axis L = A0 x B1
        ar = (aSize[1] * _AbsR[7]) + (aSize[2] * _AbsR[4]);
        br = (bSize[0] * _AbsR[2]) + (bSize[2] * _AbsR[0]);
        if (Mathf.Abs((_AB[2] * _R[4]) - (_AB[1] * _R[7])) > (ar + br)) { return false; }
        // Test axis L = A0 x B2
        ar = aSize[1] * _AbsR[8] + aSize[2] * _AbsR[5];
        br = bSize[0] * _AbsR[1] + bSize[1] * _AbsR[0];
        if (Mathf.Abs((_AB[2] * _R[5]) - (_AB[1] * _R[8])) > (ar + br)) { return false; }
        // Test axis L = A1 x B0
        ar = (aSize[0] * _AbsR[6]) + (aSize[2] * _AbsR[0]);
        br = (bSize[1] * _AbsR[5]) + (bSize[2] * _AbsR[4]);
        if (Mathf.Abs((_AB[0] * _R[6]) - (_AB[2] * _R[0])) > (ar + br)) { return false; }
        // Test axis L = A1 x B1
        ar = (aSize[0] * _AbsR[7]) + (aSize[2] * _AbsR[1]);
        br = (bSize[0] * _AbsR[5]) + (bSize[2] * _AbsR[3]);
        if (Mathf.Abs((_AB[0] * _R[7]) - (_AB[2] * _R[1])) > (ar + br)) { return false; }
        // Test axis L = A1 x B2
        ar = (aSize[0] * _AbsR[8]) + (aSize[2] * _AbsR[2]);
        br = (bSize[0] * _AbsR[4]) + (bSize[1] * _AbsR[3]);
        if (Mathf.Abs((_AB[0] * _R[8]) - (_AB[2] * _R[2])) > (ar + br)) { return false; }
        // Test axis L = A2 x B0
        ar = (aSize[0] * _AbsR[3]) + (aSize[1] * _AbsR[0]);
        br = (bSize[1] * _AbsR[8]) + (bSize[2] * _AbsR[7]);
        if (Mathf.Abs((_AB[1] * _R[0]) - (_AB[0] * _R[3])) > (ar + br)) { return false; }
        // Test axis L = A2 x B1
        ar = (aSize[0] * _AbsR[4]) + (aSize[1] * _AbsR[1]);
        br = (bSize[0] * _AbsR[8]) + (bSize[2] * _AbsR[6]);
        if (Mathf.Abs((_AB[1] * _R[1]) - (_AB[0] * _R[4])) > (ar + br)) { return false; }
        // Test axis L = A2 x B2
        ar = (aSize[0] * _AbsR[4]) + (aSize[1] * _AbsR[2]);
        br = (bSize[0] * _AbsR[7]) + (bSize[1] * _AbsR[6]);
        if (Mathf.Abs((_AB[1] * _R[2]) - (_AB[0] * _R[5])) > (ar + br)) { return false; }
        return true;
    }

    // Adapted from: [URL]http://www.mrelusive.com/publications/papers/SIMD-From-Quaternion-to-Matrix-and-Back.pdf[/URL]
    private static float[] QuaternionToFloatArray(Quaternion aQuat, float[] aFlatMatrix3x3) {
        if (aFlatMatrix3x3 == null) {
            aFlatMatrix3x3 = new float[9];
        }

        aFlatMatrix3x3[0] = 1 - (2.0f * (aQuat.y * aQuat.y)) - (2.0f * (aQuat.z * aQuat.z));
        aFlatMatrix3x3[1] = (2.0f * (aQuat.x * aQuat.y)) + (2.0f * (aQuat.w * aQuat.z));
        aFlatMatrix3x3[2] = (2.0f * (aQuat.x * aQuat.z)) - (2.0f * (aQuat.w * aQuat.y));

        aFlatMatrix3x3[3] = (2.0f * (aQuat.x * aQuat.y)) - (2.0f * (aQuat.w * aQuat.z));
        aFlatMatrix3x3[4] = 1.0f - (2.0f * (aQuat.x * aQuat.x)) - (2.0f * (aQuat.z * aQuat.z));
        aFlatMatrix3x3[5] = (2.0f * (aQuat.y * aQuat.z)) + (2.0f * (aQuat.w * aQuat.x));

        aFlatMatrix3x3[6] = (2.0f * (aQuat.x * aQuat.z)) + (2.0f * (aQuat.w * aQuat.y));
        aFlatMatrix3x3[7] = (2.0f * (aQuat.y * aQuat.z)) - (2.0f * (aQuat.w * aQuat.x));
        aFlatMatrix3x3[8] = 1.0f - (2.0f * (aQuat.x * aQuat.x)) - (2.0f * (aQuat.y * aQuat.y));

        return aFlatMatrix3x3;
    }
}