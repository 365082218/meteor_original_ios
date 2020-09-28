using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OBB3DBehaviour : MonoBehaviour {

    //挂到带有BoxCollider或者MeshCollider的游戏对象上可以显示OBB盒
    //使用的GIZMOS绘制.
    Collider obbCollider;
    Vector3 center;
    Vector3 size;
	void Start () {
        obbCollider = gameObject.GetComponent<Collider>();
        if (obbCollider is BoxCollider) {
            center = (obbCollider as BoxCollider).center;
            size = (obbCollider as BoxCollider).size;
        } else if (obbCollider is MeshCollider){
            center = (obbCollider as MeshCollider).sharedMesh.bounds.center;
            size = (obbCollider as MeshCollider).sharedMesh.bounds.size;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public Color gizmosColor = Color.white;
    public bool Intersects(OBB3DBehaviour other) {
        OBB3D a = new OBB3D(transform, center, size);
        OBB3D b = new OBB3D(other.transform, other.center, other.size);
        return a.Intersects(b);
    }

    public void OnDrawGizmos() {
        if (transform == null)
            return;
        var cacheMatrix = Gizmos.matrix;
        var cacheColor = Gizmos.color;

        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.color = gizmosColor;
        Gizmos.DrawWireCube(center, size);
        Gizmos.matrix = cacheMatrix;
        Gizmos.color = cacheColor;
    }
}
