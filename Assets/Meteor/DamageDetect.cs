using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//武器伤害检查
public class DamageDetect : MonoBehaviour {
    private void OnTriggerEnter(Collider other)
    {

        Debug.Log("other.name:" + other.name);
    }
}
