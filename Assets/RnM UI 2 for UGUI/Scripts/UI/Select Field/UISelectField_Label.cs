using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
	[ExecuteInEditMode, AddComponentMenu("UI/Select Field - Label", 58)]
	public class UISelectField_Label : MonoBehaviour {
		
		public enum TransitionType
		{
			None,
			CrossFade
		}
	
		public Text textComponent;
		
		public TransitionType transitionType = TransitionType.None;
		public ColorBlockExtended colors = ColorBlockExtended.defaultColorBlock;
		
		protected void Awake()
		{
			// Check if the text component is set
			if (this.textComponent == null)
				this.textComponent = this.GetComponent<Text>();
		}
	
		public void UpdateState(UISelectField.VisualState state)
		{
			this.UpdateState(state, false);
		}
		
		public void UpdateState(UISelectField.VisualState state, bool instant)
		{
			if (this.textComponent == null || !this.gameObject.activeInHierarchy || this.transitionType == TransitionType.None)
				return;
			
			Color color = this.colors.normalColor;
			
			// Prepare the state values
			switch (state)
			{
			case UISelectField.VisualState.Normal:
				color = this.colors.normalColor;
				break;
			case UISelectField.VisualState.Highlighted:
				color = this.colors.highlightedColor;
				break;
			case UISelectField.VisualState.Pressed:
				color = this.colors.pressedColor;
				break;
			case UISelectField.VisualState.Active:
				color = this.colors.activeColor;
				break;
			case UISelectField.VisualState.ActiveHighlighted:
				color = this.colors.activeHighlightedColor;
				break;
			case UISelectField.VisualState.ActivePressed:
				color = this.colors.activePressedColor;
				break;
			case UISelectField.VisualState.Disabled:
				color = this.colors.disabledColor;
				break;
			}
			
			// Do the transition
			if (this.transitionType == TransitionType.CrossFade)
			{
				this.StartColorTween(color * this.colors.colorMultiplier, (instant ? true : (this.colors.fadeDuration == 0f)));
			}
		}
		
		private void StartColorTween(Color color, bool instant)
		{
			if (this.textComponent == null)
				return;
			
			if (instant)
			{
				this.textComponent.canvasRenderer.SetColor(color);
			}
			else
			{
				this.textComponent.CrossFadeColor(color, this.colors.fadeDuration, true, true);
			}
		}
		
		private void TriggerAnimation(string trigger)
		{
			Animator animator = this.GetComponent<Animator>();
			
			if (animator == null || !animator.isActiveAndEnabled)
				return;
			
			animator.SetTrigger(trigger);
		}
	}
}