using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Test_UIEquipSlot_Assign : MonoBehaviour {
	
	public UIEquipSlot slot;
	public UIItemDatabase itemDatabase;
	public int assignItem;
	
	void Awake()
	{
		if (this.slot == null)
			this.slot = this.GetComponent<UIEquipSlot>();
	}
	
	void Start()
	{
		if (this.slot == null || this.itemDatabase == null)
		{
			this.Destruct();
			return;
		}
		
		this.slot.Assign(this.itemDatabase.GetByID(this.assignItem));
		this.Destruct();
	}
	
	private void Destruct()
	{
		DestroyImmediate(this);
	}
}
