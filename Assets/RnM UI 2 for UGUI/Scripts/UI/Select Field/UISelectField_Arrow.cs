using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
	[ExecuteInEditMode, AddComponentMenu("UI/Select Field - Arrow", 58), RequireComponent(typeof(Image))]
	public class UISelectField_Arrow : MonoBehaviour {
	
		public Graphic targetGraphic;
		
		public Selectable.Transition transitionType = Selectable.Transition.None;
		public ColorBlockExtended colors = ColorBlockExtended.defaultColorBlock;
		public SpriteStateExtended spriteState;
		public AnimationTriggersExtended animationTriggers = new AnimationTriggersExtended();
		
		protected void Awake()
		{
			// Check if the text component is set
			if (this.targetGraphic == null)
				this.targetGraphic = this.GetComponent<Graphic>();
		}
	
		public void UpdateState(UISelectField.VisualState state)
		{
			this.UpdateState(state, false);
		}
		
		public void UpdateState(UISelectField.VisualState state, bool instant)
		{
			if (this.targetGraphic == null || !this.gameObject.activeInHierarchy || this.transitionType == Selectable.Transition.None)
				return;
	
			Color color = this.colors.normalColor;
			Sprite newSprite = null;
			string triggername = this.animationTriggers.normalTrigger;
		
			// Prepare the state values
			switch (state)
			{
				case UISelectField.VisualState.Normal:
					color = this.colors.normalColor;
					newSprite = null;
					triggername = this.animationTriggers.normalTrigger;
					break;
				case UISelectField.VisualState.Highlighted:
					color = this.colors.highlightedColor;
					newSprite = this.spriteState.highlightedSprite;
					triggername = this.animationTriggers.highlightedTrigger;
					break;
				case UISelectField.VisualState.Pressed:
					color = this.colors.pressedColor;
					newSprite = this.spriteState.pressedSprite;
					triggername = this.animationTriggers.pressedTrigger;
					break;
				case UISelectField.VisualState.Active:
					color = this.colors.activeColor;
					newSprite = this.spriteState.activeSprite;
					triggername = this.animationTriggers.activeTrigger;
					break;
				case UISelectField.VisualState.ActiveHighlighted:
					color = this.colors.activeHighlightedColor;
					newSprite = this.spriteState.activeHighlightedSprite;
					triggername = this.animationTriggers.activeHighlightedTrigger;
					break;
				case UISelectField.VisualState.ActivePressed:
					color = this.colors.activePressedColor;
					newSprite = this.spriteState.activePressedSprite;
					triggername = this.animationTriggers.activePressedTrigger;
					break;
				case UISelectField.VisualState.Disabled:
					color = this.colors.disabledColor;
					newSprite = this.spriteState.disabledSprite;
					triggername = this.animationTriggers.disabledTrigger;
					break;
			}
			
			// Do the transition
			switch (this.transitionType)
			{
				case Selectable.Transition.ColorTint:
					this.StartColorTween(color * this.colors.colorMultiplier, (instant ? true : (this.colors.fadeDuration == 0f)));
					break;
				case Selectable.Transition.SpriteSwap:
					this.DoSpriteSwap(newSprite);
					break;
				case Selectable.Transition.Animation:
					this.TriggerAnimation(triggername);
					break;
			}
		}
		
		private void StartColorTween(Color color, bool instant)
		{
			if (this.targetGraphic == null)
				return;
			
			if (instant)
			{
				this.targetGraphic.canvasRenderer.SetColor(color);
			}
			else
			{
				this.targetGraphic.CrossFadeColor(color, this.colors.fadeDuration, true, true);
			}
		}
		
		private void DoSpriteSwap(Sprite newSprite)
		{
			if (this.targetGraphic == null)
				return;
			
			Image image = this.targetGraphic as Image;
			
			if (image != null)
				image.overrideSprite = newSprite;
		}
		
		private void TriggerAnimation(string trigger)
		{
			Animator animator = this.GetComponent<Animator>();
			
			if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null || string.IsNullOrEmpty(trigger))
				return;
	
			animator.ResetTrigger(this.animationTriggers.normalTrigger);
			animator.ResetTrigger(this.animationTriggers.pressedTrigger);
			animator.ResetTrigger(this.animationTriggers.highlightedTrigger);
			animator.ResetTrigger(this.animationTriggers.activeTrigger);
			animator.ResetTrigger(this.animationTriggers.activeHighlightedTrigger);
			animator.ResetTrigger(this.animationTriggers.activePressedTrigger);
			animator.ResetTrigger(this.animationTriggers.disabledTrigger);
			animator.SetTrigger(trigger);
		}
	}
}