﻿using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI.Tweens;
using System.Collections;

namespace UnityEngine.UI
{
	[ExecuteInEditMode, AddComponentMenu("UI/Highlight Transition")]
	public class UIHighlightTransition : MonoBehaviour, IEventSystemHandler, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler {
		
		public enum VisualState
		{
			Normal,
			Highlighted,
			Selected
		}
		
		public enum Transition
		{
			None,
			ColorTint,
			SpriteSwap,
			Animation
		}
		
		[SerializeField] private Transition m_Transition = Transition.None;
		
		[SerializeField] private Color m_NormalColor = ColorBlock.defaultColorBlock.normalColor;
		[SerializeField] private Color m_HighlightedColor = ColorBlock.defaultColorBlock.highlightedColor;
		[SerializeField] private Color m_SelectedColor = ColorBlock.defaultColorBlock.highlightedColor;
		[SerializeField] private float m_Duration = 0.1f;
		
		[SerializeField, Range(1f, 6f)] 
		private float m_ColorMultiplier = 1f;
		
		[SerializeField] private Sprite m_HighlightedSprite;
		[SerializeField] private Sprite m_SelectedSprite;
		
		[SerializeField] private string m_NormalTrigger = "Normal";
		[SerializeField] private string m_HighlightedTrigger = "Highlighted";
		[SerializeField] private string m_SelectedTrigger = "Selected";
		
		[SerializeField, Tooltip("Graphic that will have the selected transtion applied.")]
		private Graphic m_TargetGraphic;
		
		[SerializeField, Tooltip("GameObject that will have the selected transtion applied.")]
		private GameObject m_TargetGameObject;
		
		private bool m_Highlighted = false;
		private bool m_Selected = false;

		/// <summary>
		/// Gets or sets the transition type.
		/// </summary>
		/// <value>The transition.</value>
		public Transition transition
		{
			get
			{
				return this.m_Transition;
			}
			set
			{
				this.m_Transition = value;
			}
		}
		
		/// <summary>
		/// Gets or sets the target graphic.
		/// </summary>
		/// <value>The target graphic.</value>
		public Graphic targetGraphic
		{
			get
			{
				return this.m_TargetGraphic;
			}
			set
			{
				this.m_TargetGraphic = value;
			}
		}
		
		/// <summary>
		/// Gets or sets the target game object.
		/// </summary>
		/// <value>The target game object.</value>
		public GameObject targetGameObject
		{
			get
			{
				return this.m_TargetGameObject;
			}
			set
			{
				this.m_TargetGameObject = value;
			}
		}
		
		/// <summary>
		/// Gets the animator.
		/// </summary>
		/// <value>The animator.</value>
		public Animator animator
		{
			get
			{
				if (this.m_TargetGameObject != null)
					return this.m_TargetGameObject.GetComponent<Animator>();
				
				// Default
				return null;
			}
		}
		
		protected void OnEnable()
		{
			this.InternalEvaluateAndTransitionToNormalState(true);
		}
		
		protected void OnDisable()
		{
			this.InstantClearState();
		}
		
#if UNITY_EDITOR
		protected void OnValidate()
		{
			this.m_Duration = Mathf.Max(this.m_Duration, 0f);
			
			if (this.isActiveAndEnabled)
			{
				this.DoSpriteSwap(null);
				this.InternalEvaluateAndTransitionToNormalState(true);
			}
		}
#endif
		
		/// <summary>
		/// Instantly clears the visual state.
		/// </summary>
		protected void InstantClearState()
		{
			switch (this.m_Transition)
			{
				case Transition.ColorTint:
					this.StartColorTween(Color.white, true);
					break;
				case Transition.SpriteSwap:
					this.DoSpriteSwap(null);
					break;
				case Transition.Animation:
					this.TriggerAnimation(this.m_NormalTrigger);
					break;
			}
		}
		
		/// <summary>
		/// Internally evaluates and transitions to normal state.
		/// </summary>
		/// <param name="instant">If set to <c>true</c> instant.</param>
		private void InternalEvaluateAndTransitionToNormalState(bool instant)
		{
			this.DoStateTransition(VisualState.Normal, instant);
		}
		
		public void OnSelect(BaseEventData eventData)
		{
			this.m_Selected = true;
			this.DoStateTransition(VisualState.Selected, false);
		}
		
		public void OnDeselect(BaseEventData eventData)
		{
			this.m_Selected = false;
			this.DoStateTransition((this.m_Highlighted ? VisualState.Highlighted : VisualState.Normal), false);
		}
		
		public void OnPointerEnter(PointerEventData eventData)
		{
			this.m_Highlighted = true;
			
			if (!this.m_Selected)
				this.DoStateTransition(VisualState.Highlighted, false);
		}
		
		public void OnPointerExit(PointerEventData eventData)
		{
			this.m_Highlighted = false;
			
			if (!this.m_Selected)
				this.DoStateTransition(VisualState.Normal, false);
		}
		
		/// <summary>
		/// Does the state transition.
		/// </summary>
		/// <param name="state">State.</param>
		/// <param name="instant">If set to <c>true</c> instant.</param>
		protected virtual void DoStateTransition(VisualState state, bool instant)
		{
			// Check if the script is enabled
			if (!this.enabled || !this.gameObject.activeInHierarchy)
				return;
			
			Color color = this.m_NormalColor;
			Sprite newSprite = null;
			string triggername = this.m_NormalTrigger;
			
			// Prepare the transition values
			switch (state)
			{
				case VisualState.Normal:
					color = this.m_NormalColor;
					newSprite = null;
					triggername = this.m_NormalTrigger;
					break;
				case VisualState.Highlighted:
					color = this.m_HighlightedColor;
					newSprite = this.m_HighlightedSprite;
					triggername = this.m_HighlightedTrigger;
					break;
				case VisualState.Selected:
					color = this.m_SelectedColor;
					newSprite = this.m_SelectedSprite;
					triggername = this.m_SelectedTrigger;
					break;
			}
			
			// Do the transition
			switch (this.m_Transition)
			{
				case Transition.ColorTint:
					this.StartColorTween(color * this.m_ColorMultiplier, instant);
					break;
				case Transition.SpriteSwap:
					this.DoSpriteSwap(newSprite);
					break;
				case Transition.Animation:
					this.TriggerAnimation(triggername);
					break;
			}
		}
		
		/// <summary>
		/// Starts the color tween.
		/// </summary>
		/// <param name="targetColor">Target color.</param>
		/// <param name="instant">If set to <c>true</c> instant.</param>
		private void StartColorTween(Color targetColor, bool instant)
		{
			if (this.m_TargetGraphic == null)
				return;
			
			if (instant || this.m_Duration == 0f || !Application.isPlaying)
			{
				this.m_TargetGraphic.canvasRenderer.SetColor(targetColor);
			}
			else
			{
				this.m_TargetGraphic.CrossFadeColor(targetColor, this.m_Duration, true, true);
			}
		}
		
		private void DoSpriteSwap(Sprite newSprite)
		{
			Image image = this.m_TargetGraphic as Image;
			
			if (image == null)
				return;
			
			image.overrideSprite = newSprite;
		}
		
		private void TriggerAnimation(string triggername)
		{
			if (this.targetGameObject == null)
				return;
			
			if (this.animator == null || !this.animator.enabled || !this.animator.isActiveAndEnabled || this.animator.runtimeAnimatorController == null || string.IsNullOrEmpty(triggername))
				return;
			
			this.animator.ResetTrigger(this.m_HighlightedTrigger);
			this.animator.ResetTrigger(this.m_SelectedTrigger);
			this.animator.SetTrigger(triggername);
		}
	}
}