using UnityEngine;

[ExecuteInEditMode]
public class NameAppendIndex : MonoBehaviour
{
	void Start()
	{
		int index = 0;
		
		foreach (Transform child in this.transform.parent)
		{
			if (child.name.Contains(this.gameObject.name))
				index++;
		}
		
		this.gameObject.name = this.gameObject.name + " " + index.ToString();
		DestroyImmediate(this); // Remove this script
	}
}