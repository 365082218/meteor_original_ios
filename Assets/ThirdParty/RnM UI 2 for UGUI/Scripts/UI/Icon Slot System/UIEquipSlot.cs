using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI
{
	[AddComponentMenu("UI/Icon Slots/Equip Slot", 12)]
	public class UIEquipSlot : UISlotBase {
		
		[Serializable] public class OnAssignEvent : UnityEvent<UIEquipSlot> { }
		[Serializable] public class OnUnassignEvent : UnityEvent<UIEquipSlot> { }
		
		[SerializeField] private UIEquipmentType m_EquipType = UIEquipmentType.None;
		
		/// <summary>
		/// Gets or sets the equip type of the slot.
		/// </summary>
		/// <value>The type of the equip.</value>
		public UIEquipmentType equipType
		{
			get { return this.m_EquipType; }
			set { this.m_EquipType = value; }
		}
		
		/// <summary>
		/// The assigned item info.
		/// </summary>
		private UIItemInfo m_ItemInfo;
		
		/// <summary>
		/// The assign event delegate.
		/// </summary>
		public OnAssignEvent onAssign = new OnAssignEvent();
		
		/// <summary>
		/// The unassign event delegate.
		/// </summary>
		public OnUnassignEvent onUnassign = new OnUnassignEvent();
		
		/// <summary>
		/// Gets the item info of the item assigned to this slot.
		/// </summary>
		/// <returns>The spell info.</returns>
		public UIItemInfo GetItemInfo()
		{
			return this.m_ItemInfo;
		}
		
		/// <summary>
		/// Determines whether this slot is assigned.
		/// </summary>
		/// <returns><c>true</c> if this instance is assigned; otherwise, <c>false</c>.</returns>
		public override bool IsAssigned()
		{
			return (this.m_ItemInfo != null);
		}
		
		/// <summary>
		/// Assign the slot by item info.
		/// </summary>
		/// <param name="itemInfo">The item info.</param>
		public bool Assign(UIItemInfo itemInfo)
		{
			if (itemInfo == null)
				return false;
			
			// Check if the equipment type matches the target slot
			if (!this.CheckEquipType(itemInfo))
				return false;
			
			// Make sure we unassign first, so the event is called before new assignment
			this.Unassign();
			
			// Use the base class assign to set the icon
			this.Assign(itemInfo.Icon);
			
			// Set the spell info
			this.m_ItemInfo = itemInfo;
			
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
			if (source is UIItemSlot)
			{
				UIItemSlot sourceSlot = source as UIItemSlot;
				
				// Check if the equipment type matches the target slot
				if (!this.CheckEquipType(sourceSlot.GetItemInfo()))
					return false;
				
				return this.Assign(sourceSlot.GetItemInfo());
			}
			else if (source is UIEquipSlot)
			{
				UIEquipSlot sourceSlot = source as UIEquipSlot;
				
				// Check if the equipment type matches the target slot
				if (!this.CheckEquipType(sourceSlot.GetItemInfo()))
					return false;
				
				return this.Assign(sourceSlot.GetItemInfo());
			}
			
			// Default
			return false;
		}
		
		/// <summary>
		/// Checks if the given item can assigned to this slot.
		/// </summary>
		/// <returns><c>true</c>, if equip type was checked, <c>false</c> otherwise.</returns>
		/// <param name="info">Info.</param>
		public virtual bool CheckEquipType(UIItemInfo info)
		{
			if (info.EquipType != this.equipType)
				return false;
			
			return true;
		}
		
		/// <summary>
		/// Unassign this slot.
		/// </summary>
		public override void Unassign()
		{
			// Remove the icon
			base.Unassign();
			
			// Clear the spell info
			this.m_ItemInfo = null;
			
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
			if ((target is UIItemSlot) || (target is UIEquipSlot))
			{
				// Check if the equip slot accpets this item
				if (target is UIEquipSlot)
				{
					return (target as UIEquipSlot).CheckEquipType(this.GetItemInfo());
				}
				
				// It's an item slot
				return true;
			}
			
			// Default
			return false;
		}
		
		// <summary>
		/// Performs a slot swap.
		/// </summary>
		/// <returns><c>true</c>, if slot swap was performed, <c>false</c> otherwise.</returns>
		/// <param name="sourceSlot">Source slot.</param>
		public override bool PerformSlotSwap(Object sourceObject)
		{
			// Get the source item info
			UIItemInfo sourceItemInfo = null;
			bool assign1 = false;
			
			// Check the type of the source slot
			if (sourceObject is UIItemSlot)
			{
				sourceItemInfo = (sourceObject as UIItemSlot).GetItemInfo();
				
				// Assign the source slot by this one
				assign1 = (sourceObject as UIItemSlot).Assign(this.GetItemInfo());
			}
			else if (sourceObject is UIEquipSlot)
			{
				sourceItemInfo = (sourceObject as UIEquipSlot).GetItemInfo();
				
				// Assign the source slot by this one
				assign1 = (sourceObject as UIEquipSlot).Assign(this.GetItemInfo());
			}
			else
			{
				return false;
			}
			
			// Assign this slot by the source slot
			bool assign2 = this.Assign(sourceItemInfo);
			
			// Return the status
			return (assign1 && assign2);
		}
		
		/// <summary>
		/// Raises the tooltip event.
		/// </summary>
		/// <param name="show">If set to <c>true</c> show.</param>
		public override void OnTooltip(bool show)
		{
			// Handle unassigned
			if (!this.IsAssigned())
			{
				// If we are showing the tooltip
				if (show)
				{
					UITooltip.AddTitle(UIEquipSlot.EquipTypeToString(this.m_EquipType));
					UITooltip.SetHorizontalFitMode(ContentSizeFitter.FitMode.PreferredSize);
					UITooltip.AnchorToRect(this.transform as RectTransform);
					UITooltip.Show();
				}
				else UITooltip.Hide();
			}
			else
			{
				// Make sure we have spell info, otherwise game might crash
				if (this.m_ItemInfo == null)
					return;
				
				// If we are showing the tooltip
				if (show)
				{
					UIItemSlot.PrepareTooltip(this.m_ItemInfo);
					UITooltip.AnchorToRect(this.transform as RectTransform);
					UITooltip.Show();
				}
				else UITooltip.Hide();
			}
		}
		
		#region Static Methods
		/// <summary>
		/// Equip type to string convertion.
		/// </summary>
		/// <returns>The string.</returns>
		public static string EquipTypeToString(UIEquipmentType type)
		{
			string str = "Undefined";
			
			switch (type)
			{
				case UIEquipmentType.Head: 				str = "Head"; 		break;
				case UIEquipmentType.Necklace:			str = "Necklace"; 	break;
				case UIEquipmentType.Shoulders: 		str = "Shoulders"; 	break;
				case UIEquipmentType.Chest: 			str = "Chest"; 		break;
				case UIEquipmentType.Bracers: 			str = "Bracers"; 	break;
				case UIEquipmentType.Gloves: 			str = "Gloves"; 	break;
				case UIEquipmentType.Belt: 				str = "Belt"; 		break;
				case UIEquipmentType.Pants: 			str = "Pants"; 		break;
				case UIEquipmentType.Boots: 			str = "Boots"; 		break;
				case UIEquipmentType.Trinket: 			str = "Trinket"; 	break;
				case UIEquipmentType.Weapon_MainHand: 	str = "Main Hand"; 	break;
				case UIEquipmentType.Weapon_OffHand: 	str = "Off Hand"; 	break;
			}
			
			return str;
		}
		
		/// <summary>
		/// Gets all the equip slots.
		/// </summary>
		/// <returns>The slots.</returns>
		public static List<UIEquipSlot> GetSlots()
		{
			List<UIEquipSlot> slots = new List<UIEquipSlot>();
			UIEquipSlot[] sl = Resources.FindObjectsOfTypeAll<UIEquipSlot>();
			
			foreach (UIEquipSlot s in sl)
			{
				// Check if the slow is active in the hierarchy
				if (s.gameObject.activeInHierarchy)
					slots.Add(s);
			}
			
			return slots;
		}
		
		/// <summary>
		/// Gets the first equip slot found with the specified Equipment Type.
		/// </summary>
		/// <returns>The slot.</returns>
		/// <param name="type">The slot Equipment Type.</param>
		public static UIEquipSlot GetSlotWithType(UIEquipmentType type)
		{
			UIEquipSlot[] sl = Resources.FindObjectsOfTypeAll<UIEquipSlot>();
			
			foreach (UIEquipSlot s in sl)
			{
				// Check if the slow is active in the hierarchy
				if (s.gameObject.activeInHierarchy && s.equipType == type)
					return s;
			}
			
			// Default
			return null;
		}
		
		/// <summary>
		/// Gets all the equip slots with the specified Equipment Type.
		/// </summary>
		/// <returns>The slots.</returns>
		/// <param name="type">The slot Equipment Type.</param>
		public static List<UIEquipSlot> GetSlotsWithType(UIEquipmentType type)
		{
			List<UIEquipSlot> slots = new List<UIEquipSlot>();
			UIEquipSlot[] sl = Resources.FindObjectsOfTypeAll<UIEquipSlot>();
			
			foreach (UIEquipSlot s in sl)
			{
				// Check if the slow is active in the hierarchy
				if (s.gameObject.activeInHierarchy && s.equipType == type)
					slots.Add(s);
			}
			
			return slots;
		}
		#endregion
	}
}