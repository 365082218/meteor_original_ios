using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
	[ExecuteInEditMode]
	public class UISpellSlot_BetterHover : MonoBehaviour {
		
		public enum State
		{
			Assigned,
			Unassigned
		}
		
		[SerializeField] private UISpellSlot m_TargetSlot;
		[SerializeField] private Image m_TargetGraphic;
#if UNITY_EDITOR
		[SerializeField] private State m_DefaultState = State.Unassigned;
#endif
		[SerializeField] private Vector2 m_AssignedSize = Vector2.zero;
		[SerializeField] private Vector2 m_UnassignedSize = Vector2.zero;
		
		protected void Start()
		{
			if (Application.isPlaying && this.m_TargetSlot != null)
			{
				this.m_TargetSlot.onAssign.AddListener(OnAssignSpellSlot);
				this.m_TargetSlot.onUnassign.AddListener(OnUnassignSpellSlot);
			}
			
			// Call the events just in case the event get's called before this script is initialized
			if (this.m_TargetSlot != null)
			{
				if (this.m_TargetSlot.IsAssigned())
					this.OnAssignSpellSlot(this.m_TargetSlot);
				else
					this.OnUnassignSpellSlot(this.m_TargetSlot);
			}
		}
		
#if UNITY_EDITOR
		protected void OnValidate()
		{
			if (this.enabled && this.m_TargetGraphic != null)
			{
				this.m_TargetGraphic.rectTransform.SetSizeWithCurrentAnchors(
					RectTransform.Axis.Horizontal, 
					(this.m_DefaultState == State.Unassigned ? this.m_UnassignedSize.x : this.m_AssignedSize.x)
				);
				
				this.m_TargetGraphic.rectTransform.SetSizeWithCurrentAnchors(
					RectTransform.Axis.Vertical, 
					(this.m_DefaultState == State.Unassigned ? this.m_UnassignedSize.y : this.m_AssignedSize.y)
				);
			}
		}
#endif
		
		public void OnAssignSpellSlot(UISpellSlot slot)
		{
			if (this.enabled && this.m_TargetGraphic != null)
			{
				this.m_TargetGraphic.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, this.m_AssignedSize.x);
				this.m_TargetGraphic.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, this.m_AssignedSize.y);
			}
		}
		
		public void OnUnassignSpellSlot(UISpellSlot slot)
		{
			if (this.enabled && this.m_TargetGraphic != null)
			{
				this.m_TargetGraphic.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, this.m_UnassignedSize.x);
				this.m_TargetGraphic.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, this.m_UnassignedSize.y);
			}
		}
	}
}