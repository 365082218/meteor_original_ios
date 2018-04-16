using UnityEngine;
using System.Collections;

public class UnitEffect : MonoBehaviour {

    public GameObject PlayUnitEffect(string name)
    {
		GameObject effect = GameObject.Instantiate(Resources.Load(name)) as GameObject;
        effect.transform.parent = gameObject.transform;
		effect.transform.localPosition = Vector3.zero;
		effect.transform.localRotation = Quaternion.identity;
		return effect;
    }
}
