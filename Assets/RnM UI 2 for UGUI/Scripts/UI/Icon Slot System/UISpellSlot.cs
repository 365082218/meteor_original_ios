﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI
{
	[AddComponentMenu("UI/Icon Slots/Spell Slot", 12)]
	public class UISpellSlot : UISlotBase {
		
		[Serializable] public class OnAssignEvent : UnityEvent<UISpellSlot> { }
		[Serializable] public class OnUnassignEvent : UnityEvent<UISpellSlot> { }
		[Serializable] public class OnClickEvent : UnityEvent<UISpellSlot> { }
		
		[SerializeField, Tooltip("Placing the slot in a group will make the slot accessible via the static method GetSlot.")]
		private UISpellSlot_Group m_SlotGroup = UISpellSlot_Group.None;
		
		[SerializeField]
		private int m_ID = 0;
		
		/// <summary>
		/// Gets or sets the slot group.
		/// </summary>
		/// <value>The slot group.</value>
		public UISpellSlot_Group slotGroup
		{
			get { return this.m_SlotGroup; }
			set { this.m_SlotGroup = value; }
		}
		
		/// <summary>
		/// Gets or sets the slot ID.
		/// </summary>
		/// <value>The I.</value>
		public int ID
		{
			get { return this.m_ID; }
			set { this.m_ID = value; }
		}
		
		/// <summary>
		/// The assign event delegate.
		/// </summary>
		public OnAssignEvent onAssign = new OnAssignEvent();
		
		/// <summary>
		/// The unassign event delegate.
		/// </summary>
		public OnUnassignEvent onUnassign = new OnUnassignEvent();
		
		/// <summary>
		/// The click event delegate.
		/// </summary>
		public OnClickEvent onClick = new OnClickEvent();
		
		/// <summary>
		/// The assigned spell info.
		/// </summary>
		private UISpellInfo m_SpellInfo;
		
		/// <summary>
		/// Gets the spell info of the spell assigned to this slot.
		/// </summary>
		/// <returns>The spell info.</returns>
		public UISpellInfo GetSpellInfo()
		{
			return this.m_SpellInfo;
		}
		
		/// <summary>
		/// The slot cooldown component if any.
		/// </summary>
		private UISlotCooldown m_Cooldown;
		
		/// <summary>
		/// Determines whether this slot is assigned.
		/// </summary>
		/// <returns><c>true</c> if this instance is assigned; otherwise, <c>false</c>.</returns>
		public override bool IsAssigned()
		{
			return (this.m_SpellInfo != null);
		}
		
		/// <summary>
		/// Assign the slot by spell info.
		/// </summary>
		/// <param name="spellInfo">Spell info.</param>
		public bool Assign(UISpellInfo spellInfo)
		{
			if (spellInfo == null)
				return false;
			
			// Make sure we unassign first, so the event is called before new assignment
			this.Unassign();
			
			// Use the base class assign to set the icon
			this.Assign(spellInfo.Icon);
				
			// Set the spell info
			this.m_SpellInfo = spellInfo;

			// Invoke the on assign event
			if (this.onAssign != null)
				this.onAssign.Invoke(this);
			
			// Success
			return true;
		}
		
		/// <summary>
		/// Assign the slot by the passed source slot.
		/// </summary>
		/// <param name="source">Source.</param>
		public override bool Assign(Object source)
		{
			if (source is UISpellSlot)
			{
				UISpellSlot sourceSlot = source as UISpellSlot;
				
				if (sourceSlot != null)
					return this.Assign(sourceSlot.GetSpellInfo());
			}
			
			// Default
			return false;
		}
		
		/// <summary>
		/// Unassign this slot.
		/// </summary>
		public override void Unassign()
		{
			// Remove the icon
			base.Unassign();
			
			// Clear the spell info
			this.m_SpellInfo = null;
			
			// Invoke the on unassign event
			if (this.onUnassign != null)
				this.onUnassign.Invoke(this);
		}
		
		/// <summary>
		/// Determines whether this slot can swap with the specified target slot.
		/// </summary>
		/// <returns><c>true</c> if this instance can swap with the specified target; otherwise, <c>false</c>.</returns>
		/// <param name="target">Target.</param>
		public override bool CanSwapWith(Object target)
		{
			return (target is UISpellSlot);
		}
		
		// <summary>
		/// Performs a slot swap.
		/// </summary>
		/// <returns><c>true</c>, if slot swap was performed, <c>false</c> otherwise.</returns>
		/// <param name="sourceSlot">Source slot.</param>
		public override bool PerformSlotSwap(Object sourceObject)
		{
			// Get the source slot
			UISpellSlot sourceSlot = (sourceObject as UISpellSlot);
			
			// Get the source spell info
			UISpellInfo sourceSpellInfo = sourceSlot.GetSpellInfo();
			
			// Assign the source slot by this one
			bool assign1 = sourceSlot.Assign(this.GetSpellInfo());
			
			// Assign this slot by the source slot
			bool assign2 = this.Assign(sourceSpellInfo);
			
			// Return the status
			return (assign1 && assign2);
		}
		
		/// <summary>
		/// Raises the pointer click event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public override void OnPointerClick(PointerEventData eventData)
		{
			if (!this.IsAssigned())
				return;
			
			// Run the event on the base
			base.OnPointerClick(eventData);
			
			// Invoke the click event
			if (this.onClick != null)
				this.onClick.Invoke(this);
			
			if (this.m_SpellInfo == null)
				return;
			
			// Handle cooldown just for the demonstration
			if (this.m_Cooldown != null)
			{
				// If the spell is not on cooldown
				if (!this.m_Cooldown.IsOnCooldown)
				{
					// Start the cooldown
					this.m_Cooldown.StartCooldown(this.m_SpellInfo.ID, this.m_SpellInfo.Cooldown);
				}
			}
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
		
		/// <summary>
		/// Assigns the cooldown component.
		/// </summary>
		/// <param name="cooldown">Cooldown.</param>
		public void AssignCooldownComponent(UISlotCooldown cooldown)
		{
			this.m_Cooldown = cooldown;
		}
		
		#region Static Methods
		public static void PrepareTooltip(UISpellInfo spellInfo)
		{
			// Make sure we have spell info, otherwise game might crash
			if (spellInfo == null)
				return;
			
			// Set the spell name as title
			UITooltip.AddTitle(spellInfo.Name);
			
			// Prepare some attributes
			if (spellInfo.Flags.Has(UISpellInfo_Flags.Passive))
			{
				UITooltip.AddLine("Passive");
			}
			else
			{
				// Power consumption
				if (spellInfo.PowerCost > 0f)
				{
					if (spellInfo.Flags.Has(UISpellInfo_Flags.PowerCostInPct))
						UITooltip.AddLineColumn(spellInfo.PowerCost.ToString("0") + "% Energy");
					else
						UITooltip.AddLineColumn(spellInfo.PowerCost.ToString("0") + " Energy");
				}
				
				// Range
				if (spellInfo.Range > 0f)
				{
					if (spellInfo.Range == 1f)
						UITooltip.AddLineColumn("Melee range");
					else
						UITooltip.AddLineColumn(spellInfo.Range.ToString("0") + " yd range");
				}
				
				// Cast time
				if (spellInfo.CastTime == 0f)
					UITooltip.AddLineColumn("Instant");
				else
					UITooltip.AddLineColumn(spellInfo.CastTime.ToString("0.0") + " sec cast");
				
				// Cooldown
				if (spellInfo.Cooldown > 0f)
					UITooltip.AddLineColumn(spellInfo.Cooldown.ToString("0.0") + " sec cooldown");
			}
			
			// Set the spell description if not empty
			if (!string.IsNullOrEmpty(spellInfo.Description))
				UITooltip.AddDescription(spellInfo.Description);
		}
		
		/// <summary>
		/// Gets all the spell slots.
		/// </summary>
		/// <returns>The slots.</returns>
		public static List<UISpellSlot> GetSlots()
		{
			List<UISpellSlot> slots = new List<UISpellSlot>();
			UISpellSlot[] sl = Resources.FindObjectsOfTypeAll<UISpellSlot>();
			
			foreach (UISpellSlot s in sl)
			{
				// Check if the slow is active in the hierarchy
				if (s.gameObject.activeInHierarchy)
					slots.Add(s);
			}
			
			return slots;
		}
		
		/// <summary>
		/// Gets all the slots with the specified ID.
		/// </summary>
		/// <returns>The slots.</returns>
		/// <param name="ID">The slot ID.</param>
		public static List<UISpellSlot> GetSlotsWithID(int ID)
		{
			List<UISpellSlot> slots = new List<UISpellSlot>();
			UISpellSlot[] sl = Resources.FindObjectsOfTypeAll<UISpellSlot>();
			
			foreach (UISpellSlot s in sl)
			{
				// Check if the slow is active in the hierarchy
				if (s.gameObject.activeInHierarchy && s.ID == ID)
					slots.Add(s);
			}
			
			return slots;
		}
		
		/// <summary>
		/// Gets all the slots in the specified group.
		/// </summary>
		/// <returns>The slots.</returns>
		/// <param name="group">The spell slot group.</param>
		public static List<UISpellSlot> GetSlotsInGroup(UISpellSlot_Group group)
		{
			List<UISpellSlot> slots = new List<UISpellSlot>();
			UISpellSlot[] sl = Resources.FindObjectsOfTypeAll<UISpellSlot>();
			
			foreach (UISpellSlot s in sl)
			{
				// Check if the slow is active in the hierarchy
				if (s.gameObject.activeInHierarchy && s.slotGroup == group)
					slots.Add(s);
			}
			
			return slots;
		}
		
		/// <summary>
		/// Gets the slot with the specified ID and Group.
		/// </summary>
		/// <returns>The slot.</returns>
		/// <param name="ID">The slot ID.</param>
		/// <param name="group">The slot Group.</param>
		public static UISpellSlot GetSlot(int ID, UISpellSlot_Group group)
		{
			UISpellSlot[] sl = Resources.FindObjectsOfTypeAll<UISpellSlot>();
			
			foreach (UISpellSlot s in sl)
			{
				// Check if the slow is active in the hierarchy
				if (s.gameObject.activeInHierarchy && s.ID == ID && s.slotGroup == group)
					return s;
			}
			
			return null;
		}
		#endregion
	}
}