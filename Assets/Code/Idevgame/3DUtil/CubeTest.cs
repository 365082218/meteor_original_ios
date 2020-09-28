using UnityEngine;
using Assets.Scripts.Utils.GizmosHelper;

public class CubeTest : MonoBehaviour {
    public Vector3 Center;
    public Vector3 Size;

    // Update is called once per frame
    void Update() {
        GizmosHelper.Instance.DrawCube("myCube", transform.position, Center, transform.up, transform.forward, Size,
            transform.localScale, Color.red);
    }
}