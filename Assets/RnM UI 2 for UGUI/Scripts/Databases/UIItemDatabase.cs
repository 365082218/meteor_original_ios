using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
	public class UIItemDatabase : ScriptableObject {
	
		public UIItemInfo[] items;
		
		/// <summary>
		/// Get the specified ItemInfo by index.
		/// </summary>
		/// <param name="index">Index.</param>
		public UIItemInfo Get(int index)
		{
			return (this.items[index]);
		}
		
		/// <summary>
		/// Gets the specified ItemInfo by ID.
		/// </summary>
		/// <returns>The ItemInfo or NULL if not found.</returns>
		/// <param name="ID">The item ID.</param>
		public UIItemInfo GetByID(int ID)
		{
			for (int i = 0; i < this.items.Length; i++)
			{
				if (this.items[i].ID == ID)
					return this.items[i];
			}
			
			return null;
		}
	}
}