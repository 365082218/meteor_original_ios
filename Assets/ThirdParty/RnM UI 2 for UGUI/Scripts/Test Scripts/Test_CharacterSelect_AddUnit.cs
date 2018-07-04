using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
	public class Test_CharacterSelect_AddUnit : MonoBehaviour {
		
		public UICharacterSelect_List listComponent;
		public Transform createButton;
		public int HowMany = 5;
		public bool EmptyOutList = true;
		
		protected void OnEnable()
		{
			string[] races = new string[6] { 	"Human", "Orc", "Troll", "Dwarf", "Elf", "Fish" };
			string[] classes = new string[6] { 	"Warlord", "Blade Warrior", "Arcane Mage", "Boomstick Hunter", "Drunk Assassin", "Arch Bishop" };
			
			if (this.listComponent != null)
			{
				// Empty out the list
				if (this.EmptyOutList)
				{
					foreach (Transform trans in this.listComponent.transform)
					{
						// Ignore the create button
						if (trans.Equals(this.createButton) == false)
							Destroy(trans.gameObject);
					}
				}
				
				// Add the test units
				for (int i = 1; i <= this.HowMany; i++)
				{
					this.listComponent.AddCharacter("Test Character " + i, races[Random.Range(0, 5)], classes[Random.Range(0, 5)], Random.Range(1, 60), (i == 1));
				}
				
				// Make the create button last child
				if (this.createButton != null)
					this.createButton.SetAsLastSibling();
			}
		}
	}
}