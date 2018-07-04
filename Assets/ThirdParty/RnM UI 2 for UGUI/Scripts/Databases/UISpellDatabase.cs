using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
	public class UISpellDatabase : ScriptableObject {
	
		public UISpellInfo[] spells;
	
		/// <summary>
		/// Get the specified SpellInfo by index.
		/// </summary>
		/// <param name="index">Index.</param>
		public UISpellInfo Get(int index)
		{
			return (spells[index]);
		}
	
		/// <summary>
		/// Gets the specified SpellInfo by ID.
		/// </summary>
		/// <returns>The SpellInfo or NULL if not found.</returns>
		/// <param name="ID">The spell ID.</param>
		public UISpellInfo GetByID(int ID)
		{
			for (int i = 0; i < this.spells.Length; i++)
			{
				if (this.spells[i].ID == ID)
					return this.spells[i];
			}
	
			return null;
		}
	}
}