using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Test_UISlotBase_Assign : MonoBehaviour {
	
	public UISlotBase slot;
	public Texture texture;
	public Sprite sprite;
	
	void Start()
	{
		if (this.slot != null)
		{
			if (this.texture != null)
			{
				this.slot.Assign(this.texture);
			}
			else if (this.sprite != null)
			{
				this.slot.Assign(this.sprite);
			}
		}
	}
}
