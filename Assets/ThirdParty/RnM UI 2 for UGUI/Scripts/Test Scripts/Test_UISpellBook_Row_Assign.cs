using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Test_UISpellBook_Row_Assign : MonoBehaviour {

	public UISpellBook_Row spellBook_Row;
	public UISpellDatabase spellDatabase;
	public int assignSpell;
	
	void Awake()
	{
		if (this.spellBook_Row == null)
			this.spellBook_Row = this.GetComponent<UISpellBook_Row>();
	}
	
	void Start()
	{
		if (this.spellBook_Row == null || this.spellDatabase == null)
		{
			this.Destruct();
			return;
		}
		
		this.spellBook_Row.Assign(this.spellDatabase.GetByID(this.assignSpell));
		this.Destruct();
	}
	
	private void Destruct()
	{
		DestroyImmediate(this);
	}
}
