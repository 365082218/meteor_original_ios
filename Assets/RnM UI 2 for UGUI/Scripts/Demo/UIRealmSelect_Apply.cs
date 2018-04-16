using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI
{
	public class UIRealmSelect_Apply : MonoBehaviour {
		
		[SerializeField] private ToggleGroup m_RealmsToggleGroup;
		[SerializeField] private Text targetText;
		
		public void ApplySelectedRealm()
		{
			if (this.m_RealmsToggleGroup == null || !this.m_RealmsToggleGroup.AnyTogglesOn())
				return;
			
			Toggle active = this.m_RealmsToggleGroup.ActiveToggles().FirstOrDefault();
			
			if (active == null)
				return;
			
			// Update the text
			if (this.targetText != null)
				this.targetText.text = (active as UIRealmSelect_Realm).title;
			
			// Close the window
			UIWindow window = UIWindow.GetWindow(UIWindowID.RealmSelect);
			if (window != null) window.Hide();
		}
	}
}