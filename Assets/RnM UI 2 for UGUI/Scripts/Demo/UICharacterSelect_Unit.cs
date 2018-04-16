using UnityEngine;
using UnityEngine.UI.Tweens;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI
{
	public class UICharacterSelect_Unit : Toggle {
		
		public Text nameTextComponent;
		public Text raceTextComponent;
		public Text classTextComponent;
		public Text levelTextComponent;
		public Text levelLabelTextComponent;
		
		public ColorBlockExtended nameColors = ColorBlockExtended.defaultColorBlock;
		public ColorBlockExtended raceColors = ColorBlockExtended.defaultColorBlock;
		public ColorBlockExtended classColors = ColorBlockExtended.defaultColorBlock;
		public ColorBlockExtended levelColors = ColorBlockExtended.defaultColorBlock;
		
		public Button deleteButton;
		public bool deleteButtonAlwaysVisible = false;
		public float deleteButtonFadeDuration = 0.1f;
		
		// Tween controls
		[System.NonSerialized] private readonly TweenRunner<FloatTween> m_FloatTweenRunner;
		
		// Called by Unity prior to deserialization, 
		// should not be called by users
		protected UICharacterSelect_Unit()
		{
			if (this.m_FloatTweenRunner == null)
				this.m_FloatTweenRunner = new TweenRunner<FloatTween>();
			
			this.m_FloatTweenRunner.Init(this);
		}
		
		protected override void Start()
		{
			base.Start();
			this.DoStateTransition(SelectionState.Normal, true);
			this.onValueChanged.AddListener(OnActiveStateChange);
		}
		
#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate();
			this.ResetColors();
			
			// Prepare the delete button behaviour
			CanvasGroup cg = this.GetDeleteButtonCavnasGroup();
			
			if (cg != null)
			{
				if (this.deleteButtonAlwaysVisible)
					cg.alpha = 1f;
				else
					cg.alpha = 0f;
			}
		}
#endif

		protected override void OnRectTransformDimensionsChange()
		{
			base.OnRectTransformDimensionsChange();
			
			// Fix layout groups
			if (Application.isPlaying)
			{
				foreach (LayoutGroup lg in this.gameObject.GetComponentsInChildren<LayoutGroup>())
				{
					lg.SetLayoutHorizontal();
					lg.SetLayoutVertical();
				}
			}
		}
		
		protected override void InstantClearState()
		{
			base.InstantClearState();
			this.ResetColors();
			
			// Reset the alpha of the delete button to zero
			if (this.GetDeleteButtonCavnasGroup() != null)
				this.GetDeleteButtonCavnasGroup().alpha = (this.deleteButtonAlwaysVisible ? 1f : 0f);
		}
		
		private void ResetColors()
		{
			if (this.nameTextComponent != null) this.nameTextComponent.canvasRenderer.SetColor((this.isOn ? this.nameColors.activeColor : this.nameColors.normalColor) * this.nameColors.colorMultiplier);
			if (this.levelLabelTextComponent != null) this.levelLabelTextComponent.canvasRenderer.SetColor((this.isOn ? this.levelColors.activeColor : this.levelColors.normalColor) * this.levelColors.colorMultiplier);
			if (this.levelTextComponent != null) this.levelTextComponent.canvasRenderer.SetColor((this.isOn ? this.levelColors.activeColor : this.levelColors.normalColor) * this.levelColors.colorMultiplier);
			if (this.raceTextComponent != null) this.raceTextComponent.canvasRenderer.SetColor((this.isOn ? this.raceColors.activeColor : this.raceColors.normalColor) * this.raceColors.colorMultiplier);
			if (this.classTextComponent != null) this.classTextComponent.canvasRenderer.SetColor((this.isOn ? this.classColors.activeColor : this.classColors.normalColor) * this.classColors.colorMultiplier);
		}
	
		protected virtual void OnActiveStateChange(bool isOn)
		{
			this.DoStateTransition(this.currentSelectionState, false);
		}
		
		protected override void DoStateTransition(SelectionState state, bool instant)
		{
			base.DoStateTransition(state, instant);
			
			// Check if the script is enabled
			if (!this.enabled || !this.gameObject.activeInHierarchy || !Application.isPlaying)
				return;
			
			Color nameColor = this.nameColors.normalColor;
			Color raceColor = this.raceColors.normalColor;
			Color classColor = this.classColors.normalColor;
			Color levelColor = this.levelColors.normalColor;
			
			// Prepare the transition values
			if (state == SelectionState.Disabled)
			{
				nameColor = this.nameColors.disabledColor;
				raceColor = this.raceColors.disabledColor;
				classColor = this.classColors.disabledColor;
				levelColor = this.levelColors.disabledColor;
			}
			else if (this.isOn)
			{
				switch (state)
				{
				case SelectionState.Normal:
					nameColor = this.nameColors.activeColor;
					raceColor = this.raceColors.activeColor;
					classColor = this.classColors.activeColor;
					levelColor = this.levelColors.activeColor;
					break;
				case SelectionState.Highlighted:
					nameColor = this.nameColors.activeHighlightedColor;
					raceColor = this.raceColors.activeHighlightedColor;
					classColor = this.classColors.activeHighlightedColor;
					levelColor = this.levelColors.activeHighlightedColor;
					break;
				case SelectionState.Pressed:
					nameColor = this.nameColors.activePressedColor;
					raceColor = this.raceColors.activePressedColor;
					classColor = this.classColors.activePressedColor;
					levelColor = this.levelColors.activePressedColor;
					break;
				}
			}
			else
			{
				switch (state)
				{
				case SelectionState.Normal:
					nameColor = this.nameColors.normalColor;
					raceColor = this.raceColors.normalColor;
					classColor = this.classColors.normalColor;
					levelColor = this.levelColors.normalColor;
					break;
				case SelectionState.Highlighted:
					nameColor = this.nameColors.highlightedColor;
					raceColor = this.raceColors.highlightedColor;
					classColor = this.classColors.highlightedColor;
					levelColor = this.levelColors.highlightedColor;
					break;
				case SelectionState.Pressed:
					nameColor = this.nameColors.pressedColor;
					raceColor = this.raceColors.pressedColor;
					classColor = this.classColors.pressedColor;
					levelColor = this.levelColors.pressedColor;
					break;
				}
			}
			
			// Do the transition
			if (this.nameTextComponent != null)
				this.StartColorTween(this.nameTextComponent, nameColor * this.nameColors.colorMultiplier, (instant ? 0f : this.nameColors.fadeDuration));
			
			if (this.raceTextComponent != null)
				this.StartColorTween(this.raceTextComponent, raceColor * this.raceColors.colorMultiplier, (instant ? 0f : this.raceColors.fadeDuration));
			
			if (this.classTextComponent != null)
				this.StartColorTween(this.classTextComponent, classColor * this.classColors.colorMultiplier, (instant ? 0f : this.classColors.fadeDuration));
			
			if (this.levelTextComponent != null)
				this.StartColorTween(this.levelTextComponent, levelColor * this.levelColors.colorMultiplier, (instant ? 0f : this.levelColors.fadeDuration));
			
			if (this.levelLabelTextComponent != null)
				this.StartColorTween(this.levelLabelTextComponent, levelColor * this.levelColors.colorMultiplier, (instant ? 0f : this.levelColors.fadeDuration));
				
			// Handle the delete button visibility
			if (!this.deleteButtonAlwaysVisible)
			{
				CanvasGroup cg = this.GetDeleteButtonCavnasGroup();
				
				// Check if we have a canvas group
				if (cg != null)
				{
					bool showDelete = (state == SelectionState.Normal || state == SelectionState.Disabled) ? false : true;
					
					if (instant || this.deleteButtonFadeDuration == 0f)
					{
						cg.alpha = (showDelete ? 1f : 0f);
					}
					else
					{
						this.TweenDeleteButtonAlpha((showDelete ? 1f : 0f), this.deleteButtonFadeDuration, true);
					}
					
					// Disable the canvas group interaction
					cg.blocksRaycasts = (showDelete ? true : false);
					cg.interactable = (showDelete ? true : false);
				}
			}
		}
		
		/// <summary>
		/// Starts the color tween.
		/// </summary>
		/// <param name="targetColor">Target color.</param>
		/// <param name="instant">If set to <c>true</c> instant.</param>
		private void StartColorTween(Text target, Color targetColor, float duration)
		{
			if (target == null)
				return;
			
			if (duration == 0f)
			{
				target.canvasRenderer.SetColor(targetColor);
			}
			else
			{
				target.CrossFadeColor(targetColor, duration, true, true);
			}
		}
		
		/// <summary>
		/// Gets the delete button cavnas group.
		/// </summary>
		/// <returns>The delete button cavnas group.</returns>
		protected CanvasGroup GetDeleteButtonCavnasGroup()
		{
			if (this.deleteButton != null)
			{
				CanvasGroup cg = this.deleteButton.gameObject.GetComponent<CanvasGroup>();
				return (cg == null) ? this.deleteButton.gameObject.AddComponent<CanvasGroup>() : cg;
			}
			
			return null;
		}
		
		/// <summary>
		/// Sets the delete button alpha.
		/// </summary>
		/// <param name="alpha">Alpha.</param>
		protected void SetDeleteButtonAlpha(float alpha)
		{
			if (this.GetDeleteButtonCavnasGroup() != null)
				this.GetDeleteButtonCavnasGroup().alpha = alpha;
		}
		
		/// <summary>
		/// Tweens the delete button alpha.
		/// </summary>
		/// <param name="targetAlpha">Target alpha.</param>
		/// <param name="duration">Duration.</param>
		/// <param name="ignoreTimeScale">If set to <c>true</c> ignore time scale.</param>
		public void TweenDeleteButtonAlpha(float targetAlpha, float duration, bool ignoreTimeScale)
		{
			if (this.GetDeleteButtonCavnasGroup() == null)
				return;
			
			float currentAlpha = this.GetDeleteButtonCavnasGroup().alpha;
			
			if (currentAlpha.Equals(targetAlpha))
				return;
			
			var floatTween = new FloatTween { duration = duration, startFloat = currentAlpha, targetFloat = targetAlpha };
			floatTween.AddOnChangedCallback(SetDeleteButtonAlpha);
			floatTween.ignoreTimeScale = ignoreTimeScale;
			this.m_FloatTweenRunner.StartTween(floatTween);
		}
		
		/// <summary>
		/// Sets the name of the unit.
		/// </summary>
		/// <param name="name">Name.</param>
		public void SetName(string name)
		{
			if (this.nameTextComponent != null)
				this.nameTextComponent.text = name;
		}
		
		/// <summary>
		/// Sets the level of the unit.
		/// </summary>
		/// <param name="level">Level.</param>
		public void SetLevel(int level)
		{
			if (this.levelTextComponent != null)
				this.levelTextComponent.text = level.ToString();
		}
		
		/// <summary>
		/// Sets the class of the unit.
		/// </summary>
		public void SetClass(string mClass)
		{
			if (this.classTextComponent != null)
				this.classTextComponent.text = mClass;
		}
		
		/// <summary>
		/// Sets the race of the unit.
		/// </summary>
		public void SetRace(string mRace)
		{
			if (this.raceTextComponent != null)
				this.raceTextComponent.text = mRace;
		}
	}
}