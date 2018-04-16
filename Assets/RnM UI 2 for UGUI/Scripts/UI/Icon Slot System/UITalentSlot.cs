﻿using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI
{
	[AddComponentMenu("UI/Icon Slots/Talent Slot", 12)]
	public class UITalentSlot : UISlotBase
	{
		[SerializeField] private bool m_IsSpecialTalentSlot = false;
		[SerializeField] private Transform m_SpecialBackground;
		[SerializeField] private Text m_PointsText;
		[SerializeField] private Color m_pointsMinColor = Color.white;
		[SerializeField] private Color m_pointsMaxColor = Color.white;
		[SerializeField] private Color m_pointsActiveColor = Color.white;
		
		private UITalentInfo m_TalentInfo;
		private UISpellInfo m_SpellInfo;
		
		private int m_CurrentPoints = 0;
		
		/// <summary>
		/// Gets or sets a value indicating whether this talent slot is special.
		/// </summary>
		/// <value><c>true</c> if this instance is special; otherwise, <c>false</c>.</value>
		public bool IsSpecial
		{
			get { return this.m_IsSpecialTalentSlot; }
			set {
				this.m_IsSpecialTalentSlot = value;
				this.UpdateSpecialBackground();
			}
		}
		
		protected override void Start()
		{
			base.Start();
			
			// Disable Drag and Drop
			this.dragAndDropEnabled = false;
		}
		
#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate();
			this.UpdateSpecialBackground();
		}
#endif
		
		/// <summary>
		/// Gets the spell info of the spell assigned to this slot.
		/// </summary>
		/// <returns>The spell info.</returns>
		public UISpellInfo GetSpellInfo()
		{
			return this.m_SpellInfo;
		}
		
		/// <summary>
		/// Gets the talent info of the talent assigned to this slot.
		/// </summary>
		/// <returns>The talent info.</returns>
		public UITalentInfo GetTalentInfo()
		{
			return this.m_TalentInfo;
		}
		
		/// <summary>
		/// Determines whether this slot is assigned.
		/// </summary>
		/// <returns><c>true</c> if this instance is assigned; otherwise, <c>false</c>.</returns>
		public override bool IsAssigned()
		{
			return (this.m_TalentInfo != null);
		}
		
		/// <summary>
		/// Updates the special background.
		/// </summary>
		public void UpdateSpecialBackground()
		{
			if (this.m_SpecialBackground != null)
				this.m_SpecialBackground.gameObject.SetActive(this.m_IsSpecialTalentSlot);
		}
		
		/// <summary>
		/// Assign the specified slot by talentInfo and spellInfo.
		/// </summary>
		/// <param name="talentInfo">Talent info.</param>
		/// <param name="spellInfo">Spell info.</param>
		public bool Assign(UITalentInfo talentInfo, UISpellInfo spellInfo)
		{
			if (talentInfo == null || spellInfo == null)
				return false;
			
			// Use the base class to assign the icon
			this.Assign(spellInfo.Icon);
			
			// Set the talent info
			this.m_TalentInfo = talentInfo;
			
			// Set the spell info
			this.m_SpellInfo = spellInfo;
			
			// Update the points label
			this.UpdatePointsLabel();
			
			// Return success
			return true;
		}
		
		/// <summary>
		/// Updates the points label.
		/// </summary>
		public void UpdatePointsLabel()
		{
			if (this.m_PointsText == null)
				return;
			
			// Set the points string on the label
			this.m_PointsText.text = "";
			
			// No points assigned
			if (this.m_CurrentPoints == 0)
			{
				this.m_PointsText.text += "<color=#" + CommonColorBuffer.ColorToString(this.m_pointsMinColor) + ">" + this.m_CurrentPoints.ToString() + "</color>";
				this.m_PointsText.text += "<color=#" + CommonColorBuffer.ColorToString(this.m_pointsMaxColor) + ">/" + this.m_TalentInfo.maxPoints.ToString() + "</color>";
			}
			// Assigned but not maxed
			else if (this.m_CurrentPoints > 0 && this.m_CurrentPoints < this.m_TalentInfo.maxPoints)
			{
				this.m_PointsText.text += "<color=#" + CommonColorBuffer.ColorToString(this.m_pointsActiveColor) + ">" + this.m_CurrentPoints.ToString() + "</color>";
				this.m_PointsText.text += "<color=#" + CommonColorBuffer.ColorToString(this.m_pointsMaxColor) + ">/" + this.m_TalentInfo.maxPoints.ToString() + "</color>";
			}
			// Maxed
			else
			{
				this.m_PointsText.text += 	"<color=#" + CommonColorBuffer.ColorToString(this.m_pointsActiveColor) + ">" + 
											this.m_CurrentPoints.ToString() + "/" + 
											this.m_TalentInfo.maxPoints.ToString() + "</color>";
			}
		}
		
		/// <summary>
		/// Unassign this slot.
		/// </summary>
		public override void Unassign()
		{
			// Remove the icon
			base.Unassign();
			
			// Clear the talent info
			this.m_TalentInfo = null;
			
			// Clear the spell info
			this.m_SpellInfo = null;
		}

		/// <summary>
		/// Raises the pointer click event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public override void OnPointerClick(PointerEventData eventData)
		{
			if (!this.IsAssigned())
				return;
			
			// Redirect right click
			if (eventData.button == PointerEventData.InputButton.Right)
			{
				this.OnRightPointerClick(eventData);
				return;
			}
				
			// Check if the talent is maxed
			if (this.m_CurrentPoints >= this.m_TalentInfo.maxPoints)
				return;
			
			// Increase the points
			this.m_CurrentPoints = this.m_CurrentPoints + 1;
			
			// Update the label string
			this.UpdatePointsLabel();
		}
		
		/// <summary>
		/// Raises the right click event.
		/// </summary>
		public virtual void OnRightPointerClick(PointerEventData eventData)
		{
			// Check if the talent is at it's base
			if (this.m_CurrentPoints == 0)
				return;
			
			// Increase the points
			this.m_CurrentPoints = this.m_CurrentPoints - 1;
			
			// Update the label string
			this.UpdatePointsLabel();
		}
		
		/// <summary>
		/// Adds points.
		/// </summary>
		/// <param name="points">Points.</param>
		public void AddPoints(int points)
		{
			if (!this.IsAssigned() || points == 0)
				return;
			
			// Add the points
			this.m_CurrentPoints = this.m_CurrentPoints + points;
			
			// Make sure we dont exceed the limites
			if (this.m_CurrentPoints < 0)
				this.m_CurrentPoints = 0;
			
			if (this.m_CurrentPoints > this.m_TalentInfo.maxPoints)
				this.m_CurrentPoints = this.m_TalentInfo.maxPoints;
			
			// Update the label string
			this.UpdatePointsLabel();
		}
		
		/// <summary>
		/// Raises the tooltip event.
		/// </summary>
		/// <param name="show">If set to <c>true</c> show.</param>
		public override void OnTooltip(bool show)
		{
			// Make sure we have spell info, otherwise game might crash
			if (this.m_SpellInfo == null)
				return;
			
			// If we are showing the tooltip
			if (show)
			{
				// Prepare the tooltip lines
				UISpellSlot.PrepareTooltip(this.m_SpellInfo);
				
				// Anchor to this slot
				UITooltip.AnchorToRect(this.transform as RectTransform);
				
				// Show the tooltip
				UITooltip.Show();
			}
			else
			{
				// Hide the tooltip
				UITooltip.Hide();
			}
		}
	}	
}