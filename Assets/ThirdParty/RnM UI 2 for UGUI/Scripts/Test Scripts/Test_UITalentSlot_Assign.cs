using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
	public class Test_UITalentSlot_Assign : MonoBehaviour {
		
		public UITalentSlot slot;
		public UITalentDatabase talentDatabase;
		public UISpellDatabase spellDatabase;
		public int assignTalent = 0;
		public int addPoints = 0;
		
		void Start()
		{
			if (this.slot == null)
				this.slot = this.GetComponent<UITalentSlot>();
			
			if (this.slot == null || this.talentDatabase == null || this.spellDatabase == null)
			{
				this.Destruct();
				return;
			}
			
			UITalentInfo info = this.talentDatabase.GetByID(this.assignTalent);
			
			if (info != null)
			{
				this.slot.Assign(info, this.spellDatabase.GetByID(info.spellEntry));
				this.slot.AddPoints(this.addPoints);
			}
			
			this.Destruct();
		}
		
		private void Destruct()
		{
			DestroyImmediate(this);
		}
	}
}