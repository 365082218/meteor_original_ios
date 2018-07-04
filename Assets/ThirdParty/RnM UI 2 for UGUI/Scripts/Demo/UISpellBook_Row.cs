using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace UnityEngine.UI
{
	public class UISpellBook_Row : MonoBehaviour {
	
		[SerializeField] private Text m_RankText;
		[SerializeField] private Text m_NameText;
		[SerializeField] private Text m_DescriptionText;
		[SerializeField] private UISpellSlot m_SpellSlot;
		
		/// <summary>
		/// Assign the the spell row by the specified spellInfo.
		/// </summary>
		/// <param name="spellInfo">Spell info.</param>
		public void Assign(UISpellInfo spellInfo)
		{
			if (spellInfo == null)
				return;
				
			if (this.m_RankText != null)
				this.m_RankText.text = Random.Range(1, 10).ToString();
			
			if (this.m_NameText != null)
				this.m_NameText.text = spellInfo.Name;
			
			if (this.m_DescriptionText != null)
				this.m_DescriptionText.text = spellInfo.Description;
			
			if (this.m_SpellSlot != null)
				this.m_SpellSlot.Assign(spellInfo);
		}
	}
}