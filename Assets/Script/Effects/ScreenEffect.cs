using UnityEngine;
using System.Collections;

public class ScreenEffect : MonoBehaviour 
{
    public GameObject PlayScreenEffect(string name)
    {
		GameObject effect = GameObject.Instantiate(Resources.Load(name)) as GameObject;
        effect.transform.parent = gameObject.transform;
		effect.transform.localPosition = Vector3.zero;
		effect.transform.localRotation = Quaternion.identity;
		return effect;
    }
}
