using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
	public class UITalentDatabase : ScriptableObject {
		
		public UITalentInfo[] talents;
		
		/// <summary>
		/// Get the specified TalentInfo by index.
		/// </summary>
		/// <param name="index">Index.</param>
		public UITalentInfo Get(int index)
		{
			return (talents[index]);
		}
		
		/// <summary>
		/// Gets the specified TalentInfo by ID.
		/// </summary>
		/// <returns>The TalentInfo or NULL if not found.</returns>
		/// <param name="ID">The talent ID.</param>
		public UITalentInfo GetByID(int ID)
		{
			for (int i = 0; i < this.talents.Length; i++)
			{
				if (this.talents[i].ID == ID)
					return this.talents[i];
			}
			
			return null;
		}
	}
}