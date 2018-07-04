using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

namespace UnityEngine.UI
{
	[DisallowMultipleComponent, AddComponentMenu("UI/Tab", 58)]
	public class UITab : Toggle {
		
		public enum TextTransition
		{
			None,
			ColorTint
		}
		
		[SerializeField] private GameObject m_TargetContent;
		
		[SerializeField] private Image m_ImageTarget;
		[SerializeField] private Transition m_ImageTransition = Transition.None;
		[SerializeField] private ColorBlockExtended m_ImageColors = ColorBlockExtended.defaultColorBlock;
		[SerializeField] private SpriteStateExtended m_ImageSpriteState;
		[SerializeField] private AnimationTriggersExtended m_ImageAnimationTriggers = new AnimationTriggersExtended();
		
		[SerializeField] private Text m_TextTarget;
		[SerializeField] private TextTransition m_TextTransition = TextTransition.None;
		[SerializeField] private ColorBlockExtended m_TextColors = ColorBlockExtended.defaultColorBlock;
		
		private Selectable.SelectionState m_CurrentState = Selectable.SelectionState.Normal;
		
		protected override void OnEnable()
		{
			base.OnEnable();
			
			// Hook an event listener
			this.onValueChanged.AddListener(OnToggleStateChanged);
			
			// Apply initial state
			this.InternalEvaluateAndTransitionState(true);
		}
		
#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate();
			
			this.m_ImageColors.fadeDuration = Mathf.Max(this.m_ImageColors.fadeDuration, 0f);
			this.m_TextColors.fadeDuration = Mathf.Max(this.m_TextColors.fadeDuration, 0f);
			
			if (this.isActiveAndEnabled)
			{
				this.DoSpriteSwap(this.m_ImageTarget, null);
				this.InternalEvaluateAndTransitionState(true);
			}
		}
#endif
		
		/// <summary>
		/// Raises the toggle state changed event.
		/// </summary>
		/// <param name="state">If set to <c>true</c> state.</param>
		protected void OnToggleStateChanged(bool state)
		{
			if (!this.IsActive() || !Application.isPlaying)
				return;
			
			this.InternalEvaluateAndTransitionState(false);
		}
		
		/// <summary>
		/// Evaluates and toggles the content visibility.
		/// </summary>
		private void EvaluateAndToggleContent()
		{
			if (this.m_TargetContent != null)
				m_TargetContent.SetActive(this.isOn);
		}
		
		/// <summary>
		/// Internaly evaluates and transitions to the current state.
		/// </summary>
		/// <param name="instant">If set to <c>true</c> instant.</param>
		private void InternalEvaluateAndTransitionState(bool instant)
		{
			// Toggle the content
			this.EvaluateAndToggleContent();
			
			// Transition the active graphic children
			if (this.graphic != null && this.graphic.transform.childCount > 0)
			{
				// Loop through the children
				foreach (Transform child in this.graphic.transform)
				{
					// Try getting a graphic component
					Graphic g = child.GetComponent<Graphic>();
					
					if (g != null)
					{
						if (instant) g.canvasRenderer.SetAlpha((!this.isOn) ? 0f : 1f);
						else g.CrossFadeAlpha((!this.isOn) ? 0f : 1f, 0.1f, true);
					}
				}
			}
			
			// Do a state transition
			this.DoStateTransition(this.m_CurrentState, instant);
		}
		
		/// <summary>
		/// Does the state transitioning.
		/// </summary>
		/// <param name="state">State.</param>
		/// <param name="instant">If set to <c>true</c> instant.</param>
		protected override void DoStateTransition(Selectable.SelectionState state, bool instant)
		{
			if (!this.isActiveAndEnabled)
				return;
			
			// Save the state as current state
			this.m_CurrentState = state;
			
			Color newImageColor = this.m_ImageColors.normalColor;
			Color newTextColor = this.m_TextColors.normalColor;
			Sprite newSprite = null;
			string imageTrigger = this.m_ImageAnimationTriggers.normalTrigger;
			
			// Prepare state values
			switch (state)
			{
				case Selectable.SelectionState.Normal:
					newImageColor = (!this.isOn) ? this.m_ImageColors.normalColor : this.m_ImageColors.activeColor;
					newTextColor = (!this.isOn) ? this.m_TextColors.normalColor : this.m_TextColors.activeColor;
					newSprite = (!this.isOn) ? null : this.m_ImageSpriteState.activeSprite;
					imageTrigger = (!this.isOn) ? this.m_ImageAnimationTriggers.normalTrigger : this.m_ImageAnimationTriggers.activeTrigger;
					break;
				case Selectable.SelectionState.Highlighted:
					newImageColor = (!this.isOn) ? this.m_ImageColors.highlightedColor : this.m_ImageColors.activeHighlightedColor;
					newTextColor = (!this.isOn) ? this.m_TextColors.highlightedColor : this.m_TextColors.activeHighlightedColor;
					newSprite = (!this.isOn) ? this.m_ImageSpriteState.highlightedSprite : this.m_ImageSpriteState.activeHighlightedSprite;
					imageTrigger = (!this.isOn) ? this.m_ImageAnimationTriggers.highlightedTrigger : this.m_ImageAnimationTriggers.activeHighlightedTrigger;
					break;
				case Selectable.SelectionState.Pressed:
					newImageColor = (!this.isOn) ? this.m_ImageColors.pressedColor : this.m_ImageColors.activePressedColor;
					newTextColor = (!this.isOn) ? this.m_TextColors.pressedColor : this.m_TextColors.activePressedColor;
					newSprite = (!this.isOn) ? this.m_ImageSpriteState.pressedSprite : this.m_ImageSpriteState.activePressedSprite;
					imageTrigger = (!this.isOn) ? this.m_ImageAnimationTriggers.pressedTrigger : this.m_ImageAnimationTriggers.activePressedTrigger;
					break;
				case Selectable.SelectionState.Disabled:
					newImageColor = this.m_ImageColors.disabledColor;
					newTextColor = this.m_TextColors.disabledColor;
					newSprite = this.m_ImageSpriteState.disabledSprite;
					imageTrigger = this.m_ImageAnimationTriggers.disabledTrigger;
					break;
			}
			
			// Check if the tab is active in the scene
			if (this.gameObject.activeInHierarchy)
			{
				// Do the image transition
				switch (this.m_ImageTransition)
				{
					case Selectable.Transition.ColorTint:
						this.StartColorTween((this.m_ImageTarget as Graphic), newImageColor * this.m_ImageColors.colorMultiplier, (instant ? 0f : this.m_ImageColors.fadeDuration));
						break;
					case Selectable.Transition.SpriteSwap:
						this.DoSpriteSwap(this.m_ImageTarget, newSprite);
						break;
					case Selectable.Transition.Animation:
						this.TriggerAnimation(this.m_ImageTarget.gameObject, imageTrigger);
						break;
				}
				
				// Do the text transition
				switch (this.m_TextTransition)
				{
					case TextTransition.ColorTint:
						this.StartColorTween((this.m_TextTarget as Graphic), newTextColor * this.m_TextColors.colorMultiplier, (instant ? 0f : this.m_TextColors.fadeDuration));
						break;
				}
			}
		}
		
		/// <summary>
		/// Starts a color tween.
		/// </summary>
		/// <param name="target">Target.</param>
		/// <param name="targetColor">Target color.</param>
		/// <param name="duration">Duration.</param>
		private void StartColorTween(Graphic target, Color targetColor, float duration)
		{
			if (target == null)
				return;
			
			if (!Application.isPlaying || duration == 0f)
			{
				target.canvasRenderer.SetColor(targetColor);
			}
			else
			{
				target.CrossFadeColor(targetColor, duration, true, true);
			}
		}
		
		/// <summary>
		/// Does a sprite swap.
		/// </summary>
		/// <param name="target">Target.</param>
		/// <param name="newSprite">New sprite.</param>
		private void DoSpriteSwap(Image target, Sprite newSprite)
		{
			if (target == null)
				return;

			target.overrideSprite = newSprite;
		}
		
		/// <summary>
		/// Triggers the animation.
		/// </summary>
		/// <param name="target">Target.</param>
		/// <param name="triggername">Triggername.</param>
		private void TriggerAnimation(GameObject target, string triggername)
		{
			if (target == null)
				return;
				
			Animator animator = target.GetComponent<Animator>();
			
			if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null || string.IsNullOrEmpty(triggername))
				return;

			animator.ResetTrigger(this.m_ImageAnimationTriggers.normalTrigger);
			animator.ResetTrigger(this.m_ImageAnimationTriggers.pressedTrigger);
			animator.ResetTrigger(this.m_ImageAnimationTriggers.highlightedTrigger);
			animator.ResetTrigger(this.m_ImageAnimationTriggers.activeTrigger);
			animator.ResetTrigger(this.m_ImageAnimationTriggers.activeHighlightedTrigger);
			animator.ResetTrigger(this.m_ImageAnimationTriggers.activePressedTrigger);
			animator.ResetTrigger(this.m_ImageAnimationTriggers.disabledTrigger);
			animator.SetTrigger(triggername);
		}
		
		/// <summary>
		/// Activate the tab.
		/// </summary>
		public void Activate()
		{
			if (!this.isOn)
				this.isOn = true;
		}
	}
}