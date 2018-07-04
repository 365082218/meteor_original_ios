using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
	public class UICharacterSelect_List : MonoBehaviour {
		
		public ToggleGroup toggleGroup;
		public GameObject characterPrefab;
		
		protected void Awake()
		{
			if (this.toggleGroup == null) this.toggleGroup = this.gameObject.GetComponent<ToggleGroup>();
		}
		
		/// <summary>
		/// Add a character to the list.
		/// </summary>
		/// <param name="mName">Name.</param>
		/// <param name="mRace">Race.</param>
		/// <param name="mClass">Class.</param>
		/// <param name="mLevel">Level.</param>
		public void AddCharacter(string mName, string mRace, string mClass, int mLevel)
		{
			this.AddCharacter(mName, mRace, mClass, mLevel, false);
		}
	
		/// <summary>
		/// Add a character to the list.
		/// </summary>
		/// <param name="mName">Name.</param>
		/// <param name="mRace">Race.</param>
		/// <param name="mClass">Class.</param>
		/// <param name="mLevel">Level.</param>
		/// <param name="mSelected">If set to <c>true</c> the character will be selected.</param>
		public void AddCharacter(string mName, string mRace, string mClass, int mLevel, bool mSelected)
		{
			if (this.characterPrefab == null)
				return;
			
			// Instantiate the prefab	
			GameObject obj = (GameObject)Instantiate(this.characterPrefab);
			
			// Change parent
			obj.transform.SetParent(this.transform, false);
			
			// Get the unit component
			UICharacterSelect_Unit unit = obj.GetComponent<UICharacterSelect_Unit>();
			
			// Apply the toggle group
			if (this.toggleGroup != null)
				unit.group = this.toggleGroup;
			
			// Set the character details
			unit.SetName(mName);
			unit.SetRace(mRace);
			unit.SetClass(mClass);
			unit.SetLevel(mLevel);
	
			// Apply the selected flag
			unit.isOn = mSelected;
		}
	}
}