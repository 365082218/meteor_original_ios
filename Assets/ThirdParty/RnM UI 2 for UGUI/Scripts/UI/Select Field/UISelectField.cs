﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI.Tweens;

namespace UnityEngine.UI
{
	[ExecuteInEditMode, DisallowMultipleComponent, AddComponentMenu("UI/Select Field", 58), RequireComponent(typeof(Image))]
	public class UISelectField : Toggle {
		
		public enum Direction
		{
			Auto,
			Down,
			Up
		}
		
		public enum VisualState
		{
			Normal,
			Highlighted,
			Pressed,
			Active,
			ActiveHighlighted,
			ActivePressed,
			Disabled
		}
		
		public enum ListAnimationType
		{
			None,
			Fade,
			Animation
		}
		
		public enum OptionTextTransitionType
		{
			None,
			CrossFade
		}
		
		public enum OptionTextEffectType
		{
			None,
			Shadow,
			Outline
		}
		
		// Currently selected item
		[HideInInspector][SerializeField]
		private string m_SelectedItem;
		
		[SerializeField]
		private Direction m_Direction = Direction.Auto;
		
		private List<UISelectField_Option> m_OptionObjects = new List<UISelectField_Option>();
		private VisualState m_CurrentVisualState = VisualState.Normal;
		private bool m_PointerWasUsedOnOption = false;
		
		private GameObject m_ListObject;
		private CanvasGroup m_ListCanvasGroup;
		private Vector2 m_LastListSize = Vector2.zero;
		
		/// <summary>
		/// The arrow component.
		/// </summary>
		public UISelectField_Arrow arrow;
		
		/// <summary>
		/// The label component.
		/// </summary>
		public UISelectField_Label label;
		
		/// <summary>
		/// The direction in which the list should pop.
		/// </summary>
		public Direction direction
		{
			get { return this.m_Direction; }
			set { this.m_Direction = value; }
		}
		
		/// <summary>
		/// New line-delimited list of items.
		/// </summary>
		public List<string> options = new List<string>();
		
		/// <summary>
		/// Currently selected option.
		/// </summary>
		public string value {
			get {
				return this.m_SelectedItem;
			}
			set {
				this.SelectOption(value);
			}
		}
		
		/// <summary>
		/// Gets the index of the selected option.
		/// </summary>
		/// <value>The index of the selected option.</value>
		public int selectedOptionIndex {
			get {
				return this.GetOptionIndex(this.m_SelectedItem);
			}
		}
		
		// Select Field layout properties
		public new ColorBlockExtended colors = ColorBlockExtended.defaultColorBlock;
		public new SpriteStateExtended spriteState;
		public new AnimationTriggersExtended animationTriggers = new AnimationTriggersExtended();
		
		// List layout properties
		public Sprite listBackgroundSprite;
		public Image.Type listBackgroundSpriteType = Image.Type.Sliced;
		public Color listBackgroundColor = Color.white;
		public RectOffset listMargins;
		public RectOffset listPadding;
		public float listSpacing = 0f;
		public ListAnimationType listAnimationType = ListAnimationType.Fade;
		public float listAnimationDuration = 0.1f;
		public RuntimeAnimatorController listAnimatorController;
		public string listAnimationOpenTrigger = "Open";
		public string listAnimationCloseTrigger = "Close";
		
		// Option text layout properties
		public Font optionFont = FontData.defaultFontData.font;
		public int optionFontSize = FontData.defaultFontData.fontSize;
		public FontStyle optionFontStyle = FontData.defaultFontData.fontStyle;
		public Color optionColor = Color.white;
		public OptionTextTransitionType optionTextTransitionType = OptionTextTransitionType.CrossFade;
		public ColorBlockExtended optionTextTransitionColors = ColorBlockExtended.defaultColorBlock;
		public RectOffset optionPadding;
		
		// Option text effect properties
		public OptionTextEffectType optionTextEffectType = OptionTextEffectType.None;
		public Color optionTextEffectColor = new Color(0f, 0f, 0f, 128f);
		public Vector2 optionTextEffectDistance = new Vector2(1f, -1f);
		public bool optionTextEffectUseGraphicAlpha = true;
		
		// Option background properties
		public Sprite optionBackgroundSprite;
		public Color optionBackgroundSpriteColor =  Color.white;
		public Image.Type optionBackgroundSpriteType = Image.Type.Sliced;
		public Selectable.Transition optionBackgroundTransitionType = Selectable.Transition.None;
		public ColorBlockExtended optionBackgroundTransColors = ColorBlockExtended.defaultColorBlock;
		public SpriteStateExtended optionBackgroundSpriteStates;
		public AnimationTriggersExtended optionBackgroundAnimationTriggers = new AnimationTriggersExtended();
		public RuntimeAnimatorController optionBackgroundAnimatorController;
		
		// List separator properties
		public Sprite listSeparatorSprite;
		public Image.Type listSeparatorType = Image.Type.Simple;
		public Color listSeparatorColor = Color.white;
		public float listSeparatorHeight = 0f;
		
		[Serializable] public class ChangeEvent : UnityEvent<int, string> { }
		
		/// <summary>
		/// Event delegates triggered when the selected option changes.
		/// </summary>
		public ChangeEvent onChange = new ChangeEvent();
		
		// Tween controls
		[NonSerialized] private readonly TweenRunner<FloatTween> m_FloatTweenRunner;
		
		// Called by Unity prior to deserialization, 
		// should not be called by users
		protected UISelectField()
		{
			if (this.m_FloatTweenRunner == null)
				this.m_FloatTweenRunner = new TweenRunner<FloatTween>();
			
			this.m_FloatTweenRunner.Init(this);
		}
		
		protected override void Awake()
		{
			base.Awake();
			
			// Get the background image
			if (this.targetGraphic == null)
				this.targetGraphic = this.GetComponent<Image>();
		}
		
		protected override void Start()
		{
			base.Start();
			
			// Prepare the toggle
			this.toggleTransition = ToggleTransition.None;
		}
		
#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate();
			
			// Make sure we always have a font
			if (this.optionFont == null)
				this.optionFont = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
		}
#endif
		
		protected override void OnEnable()
		{
			base.OnEnable();
			
			// Hook the on change event
			this.onValueChanged.AddListener(OnToggleValueChanged);
		}
		
		protected override void OnDisable()
		{
			base.OnDisable();
			
			// Unhook the on change event
			this.onValueChanged.RemoveListener(OnToggleValueChanged);
			
			// Close if open
			this.isOn = false;
			
			// Transition to the current state
			this.DoStateTransition(SelectionState.Disabled, true);
		}
		
		/// <summary>
		/// Open the select field list.
		/// </summary>
		public void Open() { this.isOn = true; }
		
		/// <summary>
		/// Closes the select field list.
		/// </summary>
		public void Close() { this.isOn = false; }
		
		/// <summary>
		/// Gets a value indicating whether the list is open.
		/// </summary>
		/// <value><c>true</c> if the list is open; otherwise, <c>false</c>.</value>
		public bool IsOpen {
			get
			{
				return this.isOn;
			}
		}
		
		/// <summary>
		/// Gets the index of the given option.
		/// </summary>
		/// <returns>The option index. (-1 if the option was not found)</returns>
		/// <param name="optionValue">Option value.</param>
		public int GetOptionIndex(string optionValue)
		{
			// Find the option index in the options list
			if (this.options != null && this.options.Count > 0 && !string.IsNullOrEmpty(optionValue))
				for (int i = 0; i < this.options.Count; i++)
					if (optionValue.Equals(this.options[i], System.StringComparison.OrdinalIgnoreCase))
						return i;
			
			// Default
			return -1;
		}
		
		/// <summary>
		/// Selects the option by index.
		/// </summary>
		/// <param name="optionIndex">Option index.</param>
		public void SelectOptionByIndex(int index)
		{
			// If the list is open, use the toggle to select the option
			if (this.IsOpen)
			{
				UISelectField_Option option = this.m_OptionObjects[index];
				
				if (option != null)
					option.isOn = true;
			}
			else // otherwise set as selected
			{
				// Set as selected
				this.m_SelectedItem = this.options[index];
				
				// Trigger change
				this.TriggerChangeEvent();
			}
		}
		
		/// <summary>
		/// Selects the option by value.
		/// </summary>
		/// <param name="optionValue">The option value.</param>
		public void SelectOption(string optionValue)
		{
			if (string.IsNullOrEmpty(optionValue))
				return;
			
			// Get the option
			int index = this.GetOptionIndex(optionValue);
			
			// Check if the option index is valid
			if (index < 0 || index >= this.options.Count)
				return;
			
			// Select the option
			this.SelectOptionByIndex(index);
		}
		
		/// <summary>
		/// Adds an option.
		/// </summary>
		/// <param name="optionValue">Option value.</param>
		public void AddOption(string optionValue)
		{
			if (this.options != null)
				this.options.Add(optionValue);
		}
		
		/// <summary>
		/// Adds an option at given index.
		/// </summary>
		/// <param name="optionValue">Option value.</param>
		/// <param name="index">Index.</param>
		public void AddOptionAtIndex(string optionValue, int index)
		{
			if (this.options == null)
				return;
			
			// Check if the index is outside the list
			if (index >= this.options.Count)
			{
				this.options.Add(optionValue);
			}
			else
			{
				this.options.Insert(index, optionValue);
			}
		}
		
		/// <summary>
		/// Removes the option.
		/// </summary>
		/// <param name="optionValue">Option value.</param>
		public void RemoveOption(string optionValue)
		{
			if (this.options == null)
				return;
			
			// Remove the option if exists
			if (this.options.Contains(optionValue))
			{
				this.options.Remove(optionValue);
				this.ValidateSelectedOption();
			}
		}
		
		/// <summary>
		/// Removes the option at the given index.
		/// </summary>
		/// <param name="index">Index.</param>
		public void RemoveOptionAtIndex(int index)
		{
			if (this.options == null)
				return;
			
			// Remove the option if the index is valid
			if (index >= 0 && index < this.options.Count)
			{
				this.options.RemoveAt(index);
				this.ValidateSelectedOption();
			}
		}
		
		/// <summary>
		/// Validates the selected option and makes corrections if it's missing.
		/// </summary>
		public void ValidateSelectedOption()
		{
			if (this.options == null)
				return;
			
			// Fix the selected option if it no longer exists
			if (!this.options.Contains(this.m_SelectedItem))
			{
				// Select the first option
				this.SelectOptionByIndex(0);
			}
		}
		
		/// <summary>
		/// Raises the option select event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		/// <param name="option">Option.</param>
		public void OnOptionSelect(string option)
		{
			if (string.IsNullOrEmpty(option))
				return;
			
			// Save the current string to compare later
			string current = this.m_SelectedItem;
			
			// Save the string
			this.m_SelectedItem = option;
			
			// Trigger change event
			if (!current.Equals(this.m_SelectedItem))
				this.TriggerChangeEvent();
			
			// Close the list if it's opened and the pointer was used to select the option
			if (this.IsOpen && this.m_PointerWasUsedOnOption)
			{
				// Reset the value
				this.m_PointerWasUsedOnOption = false;
				
				// Close the list
				this.Close();
				
				// Deselect the toggle
				base.OnDeselect(new BaseEventData(EventSystem.current));
			}
		}
		
		/// <summary>
		/// Raises the option pointer up event (Used to close the list).
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public void OnOptionPointerUp(BaseEventData eventData)
		{
			// Flag to close the list on selection
			this.m_PointerWasUsedOnOption = true;
		}
		
		/// <summary>
		/// Tiggers the change event.
		/// </summary>
		protected virtual void TriggerChangeEvent()
		{
			// Apply the string to the label componenet
			if (this.label != null && this.label.textComponent != null)
				this.label.textComponent.text = this.m_SelectedItem;
			
			// Invoke the on change event
			if (onChange != null)
				onChange.Invoke(this.selectedOptionIndex, this.m_SelectedItem);
		}
		
		/// <summary>
		/// Raises the toggle value changed event (used to toggle the list).
		/// </summary>
		/// <param name="state">If set to <c>true</c> state.</param>
		private void OnToggleValueChanged(bool state)
		{
			if (!Application.isPlaying)
				return;
			
			// Transition to the current state
			this.DoStateTransition(this.currentSelectionState, false);
			
			// Open / Close the list
			this.ToggleList(this.isOn);
		}
		
		/// <summary>
		/// Raises the deselect event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public override void OnDeselect(BaseEventData eventData)
		{
			// Check if the mouse is over our options list
			if (this.m_ListObject != null)
			{
				UISelectField_List list = this.m_ListObject.GetComponent<UISelectField_List>();
				
				if (list.IsHighlighted(eventData))
					return;
			}
			
			// Check if the mouse is over one of our options
			foreach (UISelectField_Option option in this.m_OptionObjects)
			{
				if (option.IsHighlighted(eventData))
					return;
			}
			
			// When the select field loses focus
			// close the list by deactivating the toggle
			this.Close();
			
			// Pass to base
			base.OnDeselect(eventData);
		}
		
		/// <summary>
		/// Raises the move event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public override void OnMove(AxisEventData eventData)
		{
			// Handle navigation for opened list
			if (this.IsOpen)
			{
				int prevIndex = (this.selectedOptionIndex - 1);
				int nextIndex = (this.selectedOptionIndex + 1);
				
				// Highlight the new option
				switch (eventData.moveDir)
				{
				case MoveDirection.Up:
				{
					if (prevIndex >= 0)
					{
						this.SelectOptionByIndex(prevIndex);
					}
					break;
				}
				case MoveDirection.Down:
				{
					if (nextIndex < this.options.Count)
					{
						this.SelectOptionByIndex(nextIndex);
					}
					break;
				}
				}
				
				// Use the event
				eventData.Use();
			}
			else
			{
				// Pass to base
				base.OnMove(eventData);
			}
		}
		
		/// <summary>
		/// Dos the state transition of the select field.
		/// </summary>
		/// <param name="state">State.</param>
		/// <param name="instant">If set to <c>true</c> instant.</param>
		protected override void DoStateTransition(Selectable.SelectionState state, bool instant)
		{
			if (!this.gameObject.activeInHierarchy)
				return;
			
			Color color = this.colors.normalColor;
			Sprite newSprite = null;
			string triggername = this.animationTriggers.normalTrigger;
			
			// Check if this is the disabled state before any others
			if (state == Selectable.SelectionState.Disabled)
			{
				this.m_CurrentVisualState = VisualState.Disabled;
				color = this.colors.disabledColor;
				newSprite = this.spriteState.disabledSprite;
				triggername = this.animationTriggers.disabledTrigger;
			}
			else
			{
				// Prepare the state values
				switch (state)
				{
				case Selectable.SelectionState.Normal:
					this.m_CurrentVisualState = (this.isOn) ? VisualState.Active : VisualState.Normal;
					color = 					(this.isOn) ? this.colors.activeColor : this.colors.normalColor;
					newSprite = 				(this.isOn) ? this.spriteState.activeSprite : null;
					triggername = 				(this.isOn) ? this.animationTriggers.activeTrigger : this.animationTriggers.normalTrigger;
					break;
				case Selectable.SelectionState.Highlighted:
					this.m_CurrentVisualState = (this.isOn) ? VisualState.ActiveHighlighted : VisualState.Highlighted;
					color = 					(this.isOn) ? this.colors.activeHighlightedColor : this.colors.highlightedColor;
					newSprite = 				(this.isOn) ? this.spriteState.activeHighlightedSprite : this.spriteState.highlightedSprite;
					triggername = 				(this.isOn) ? this.animationTriggers.activeHighlightedTrigger : this.animationTriggers.highlightedTrigger;
					break;
				case Selectable.SelectionState.Pressed:
					this.m_CurrentVisualState = (this.isOn) ? VisualState.ActivePressed : VisualState.Pressed;
					color = 					(this.isOn) ? this.colors.activePressedColor : this.colors.pressedColor;
					newSprite = 				(this.isOn) ? this.spriteState.activePressedSprite : this.spriteState.pressedSprite;
					triggername = 				(this.isOn) ? this.animationTriggers.activePressedTrigger : this.animationTriggers.pressedTrigger;
					break;
				}
			}
			
			// Do the transition
			switch (this.transition)
			{
			case Selectable.Transition.ColorTint:
				this.StartColorTween(color * this.colors.colorMultiplier, (instant ? 0f : this.colors.fadeDuration));
				break;
			case Selectable.Transition.SpriteSwap:
				this.DoSpriteSwap(newSprite);
				break;
			case Selectable.Transition.Animation:
				this.TriggerAnimation(triggername);
				break;
			}
			
			// Propagate to the child elements
			if (this.arrow != null)
				this.arrow.UpdateState(this.m_CurrentVisualState, instant);
			
			if (this.label != null)
				this.label.UpdateState(this.m_CurrentVisualState, instant);
		}
		
		/// <summary>
		/// Starts the color tween of the select field.
		/// </summary>
		/// <param name="color">Color.</param>
		/// <param name="instant">If set to <c>true</c> instant.</param>
		private void StartColorTween(Color color, float duration)
		{
			if (this.targetGraphic == null)
				return;
			
			this.targetGraphic.CrossFadeColor(color, duration, true, true);
		}
		
		/// <summary>
		/// Does the sprite swap of the select field.
		/// </summary>
		/// <param name="newSprite">New sprite.</param>
		private void DoSpriteSwap(Sprite newSprite)
		{
			Image image = this.targetGraphic as Image;
			
			if (image == null)
				return;
			
			image.overrideSprite = newSprite;
		}
		
		/// <summary>
		/// Triggers the animation of the select field.
		/// </summary>
		/// <param name="trigger">Trigger.</param>
		private void TriggerAnimation(string trigger)
		{
			if (this.animator == null || !this.animator.isActiveAndEnabled || this.animator.runtimeAnimatorController == null || string.IsNullOrEmpty(trigger))
				return;
			
			this.animator.ResetTrigger(this.animationTriggers.normalTrigger);
			this.animator.ResetTrigger(this.animationTriggers.pressedTrigger);
			this.animator.ResetTrigger(this.animationTriggers.highlightedTrigger);
			this.animator.ResetTrigger(this.animationTriggers.activeTrigger);
			this.animator.ResetTrigger(this.animationTriggers.activeHighlightedTrigger);
			this.animator.ResetTrigger(this.animationTriggers.activePressedTrigger);
			this.animator.ResetTrigger(this.animationTriggers.disabledTrigger);
			this.animator.SetTrigger(trigger);
		}
		
		/// <summary>
		/// Toggles the list.
		/// </summary>
		/// <param name="state">If set to <c>true</c> state.</param>
		protected virtual void ToggleList(bool state)
		{
			if (!this.IsActive())
				return;
			
			// Check if the list is not yet created
			if (this.m_ListObject == null)
				this.CreateList();
			
			// Make sure the creating of the list was successful
			if (this.m_ListObject == null)
				return;
			
			// Make sure we have the canvas group
			if (this.m_ListCanvasGroup != null)
			{
				// Disable or enable list interaction
				this.m_ListCanvasGroup.blocksRaycasts = state;
			}
			
			// Bring to front
			if (state) UIUtility.BringToFront(this.m_ListObject);
			
			// Start the opening/closing animation
			if (this.listAnimationType == ListAnimationType.None || this.listAnimationType == ListAnimationType.Fade)
			{
				float targetAlpha = (state ? 1f : 0f);
				
				// Fade In / Out
				this.TweenListAlpha(targetAlpha, ((this.listAnimationType == ListAnimationType.Fade) ? this.listAnimationDuration : 0f), true);
			}
			else if (this.listAnimationType == ListAnimationType.Animation)
			{
				this.TriggerListAnimation(state ? this.listAnimationOpenTrigger : this.listAnimationCloseTrigger);
			}
		}
		
		/// <summary>
		/// Creates the list and it's options.
		/// </summary>
		protected void CreateList()
		{
			// Reset the last list size
			this.m_LastListSize = Vector2.zero;
			
			// Clear the option texts list
			this.m_OptionObjects.Clear();
			
			// Create the list game object with the necessary components
			this.m_ListObject = new GameObject("UISelectField - List", typeof(RectTransform));
			
			// Change the parent of the list
			this.m_ListObject.transform.SetParent(this.transform, false);
			
			// Get the select field list component
			UISelectField_List listComp = this.m_ListObject.AddComponent<UISelectField_List>();
			
			// Get the list canvas group component
			this.m_ListCanvasGroup = this.m_ListObject.AddComponent<CanvasGroup>();
			
			// Change the anchor and pivot of the list
			RectTransform rect = (this.m_ListObject.transform as RectTransform);
			rect.anchorMin = Vector2.zero;
			rect.anchorMax = Vector2.zero;
			rect.pivot = new Vector2(0f, 1f);
			
			// Prepare the position of the list
			rect.anchoredPosition = new Vector3((float)this.listMargins.left, ((float)this.listMargins.top * -1f), 0f);
			
			// Prepare the width of the list
			rect.sizeDelta = new Vector2((this.targetGraphic.rectTransform.sizeDelta.x - (this.listMargins.left + this.listMargins.right)), 0f);
			
			// Hook the Dimensions Change event
			listComp.onDimensionsChange.AddListener(ListDimensionsChanged);
			
			// Apply the background sprite
			Image image = this.m_ListObject.AddComponent<Image>();
			if (this.listBackgroundSprite != null)
				image.sprite = this.listBackgroundSprite;
			image.type = this.listBackgroundSpriteType;
			image.color = this.listBackgroundColor;
			
			// Prepare the vertical layout group
			VerticalLayoutGroup layoutGroup = this.m_ListObject.AddComponent<VerticalLayoutGroup>();
			layoutGroup.padding = this.listPadding;
			layoutGroup.spacing = this.listSpacing;
			
			// Prepare the content size fitter
			ContentSizeFitter fitter = this.m_ListObject.AddComponent<ContentSizeFitter>();
			fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			
			// Get the list toggle group
			ToggleGroup toggleGroup = this.m_ListObject.AddComponent<ToggleGroup>();
			
			// Create the options
			for (int i = 0; i < this.options.Count; i++)
			{
				// Create the option
				this.CreateOption(i, toggleGroup);
				
				// Create a separator if this is not the last option
				if (i < (this.options.Count - 1))
					this.CreateSeparator(i);
			}
			
			// Prepare the list for the animation
			if (this.listAnimationType == ListAnimationType.None || this.listAnimationType == ListAnimationType.Fade)
			{
				// Starting alpha should be zero
				this.m_ListCanvasGroup.alpha = 0f;
			}
			else if (this.listAnimationType == ListAnimationType.Animation)
			{
				// Attach animator component
				Animator animator = this.m_ListObject.AddComponent<Animator>();
				
				// Set the animator controller
				animator.runtimeAnimatorController = this.listAnimatorController;
				
				// Set the animation triggers so we can use them to detect when animations finish
				listComp.SetTriggers(this.listAnimationOpenTrigger, this.listAnimationCloseTrigger);
				
				// Hook a callback on the finish event
				listComp.onAnimationFinish.AddListener(OnListAnimationFinish);
			}
		}
		
		/// <summary>
		/// Creates a option.
		/// </summary>
		/// <param name="index">Index.</param>
		protected void CreateOption(int index, ToggleGroup toggleGroup)
		{
			if (this.m_ListObject == null)
				return;
			
			// Create the option game object with it's components
			GameObject optionObject = new GameObject("Option " + index.ToString(), typeof(RectTransform));
			
			// Change parents
			optionObject.transform.SetParent(this.m_ListObject.transform, false);
			
			// Get the option component
			UISelectField_Option optionComp = optionObject.AddComponent<UISelectField_Option>();
			
			// Prepare the option background
			if (this.optionBackgroundSprite != null)
			{
				Image image = optionObject.AddComponent<Image>();
				image.sprite = this.optionBackgroundSprite;
				image.type = this.optionBackgroundSpriteType;
				image.color = this.optionBackgroundSpriteColor;
				
				// Add the graphic as the option transition target
				optionComp.targetGraphic = image;
			}
			
			// Prepare the option for animation
			if (this.optionBackgroundTransitionType == Transition.Animation)
			{
				// Attach animator component
				Animator animator = optionObject.AddComponent<Animator>();
				
				// Set the animator controller
				animator.runtimeAnimatorController = this.optionBackgroundAnimatorController;
			}
			
			// Apply the option padding
			VerticalLayoutGroup vlg = optionObject.AddComponent<VerticalLayoutGroup>();
			vlg.padding = this.optionPadding;
			
			// Create the option text
			GameObject textObject = new GameObject("Label", typeof(RectTransform));
			
			// Change parents
			textObject.transform.SetParent(optionObject.transform, false);
			
			// Apply pivot
			(textObject.transform as RectTransform).pivot = new Vector2(0f, 1f);
			
			// Prepare the text
			Text text = textObject.AddComponent<Text>();
			text.font = this.optionFont;
			text.fontSize = this.optionFontSize;
			text.fontStyle = this.optionFontStyle;
			text.color = this.optionColor;
			
			if (this.options != null)
				text.text = this.options[index];
			
			// Apply normal state transition color
			if (this.optionTextTransitionType == OptionTextTransitionType.CrossFade)
				text.canvasRenderer.SetColor(this.optionTextTransitionColors.normalColor);
			
			// Add and prepare the text effect
			if (this.optionTextEffectType != OptionTextEffectType.None)
			{
				if (this.optionTextEffectType == OptionTextEffectType.Shadow)
				{
					Shadow effect = textObject.AddComponent<Shadow>();
					effect.effectColor = this.optionTextEffectColor;
					effect.effectDistance = this.optionTextEffectDistance;
					effect.useGraphicAlpha = this.optionTextEffectUseGraphicAlpha;
				}
				else if (this.optionTextEffectType == OptionTextEffectType.Outline)
				{
					Outline effect = textObject.AddComponent<Outline>();
					effect.effectColor = this.optionTextEffectColor;
					effect.effectDistance = this.optionTextEffectDistance; 
					effect.useGraphicAlpha = this.optionTextEffectUseGraphicAlpha;
				}
			}
			
			// Initialize the option component
			optionComp.Initialize(this, text);
			
			// Set active if it's the selected one
			if (index == this.selectedOptionIndex)
				optionComp.isOn = true;
			
			// Register to the toggle group
			if (toggleGroup != null)
				optionComp.group = toggleGroup;
			
			// Hook some events
			optionComp.onSelectOption.AddListener(OnOptionSelect);
			optionComp.onPointerUp.AddListener(OnOptionPointerUp);
			
			// Add it to the list
			if (this.m_OptionObjects != null)
				this.m_OptionObjects.Add(optionComp);
		}
		
		/// <summary>
		/// Creates a separator.
		/// </summary>
		/// <param name="index">Index.</param>
		protected void CreateSeparator(int index)
		{
			if (this.m_ListObject == null || this.listSeparatorSprite == null)
				return;
			
			GameObject separatorObject = new GameObject("Separator " + index.ToString(), typeof(RectTransform));
			
			// Change parent
			separatorObject.transform.SetParent(this.m_ListObject.transform, false);
			
			// Apply the sprite
			Image image = separatorObject.AddComponent<Image>();
			image.sprite = this.listSeparatorSprite;
			image.type = this.listSeparatorType;
			image.color = this.listSeparatorColor;
			
			// Apply preferred height
			LayoutElement le = separatorObject.AddComponent<LayoutElement>();
			le.preferredHeight = (this.listSeparatorHeight > 0f) ? this.listSeparatorHeight : this.listSeparatorSprite.rect.height;
		}
		
		/// <summary>
		/// Does a list cleanup (Destroys the list and clears the option objects list).
		/// </summary>
		protected virtual void ListCleanup()
		{
			if (this.m_ListObject != null)
				Destroy(this.m_ListObject);
			
			this.m_OptionObjects.Clear();
		}
		
		/// <summary>
		/// Positions the list for the given direction (Auto is not handled in this method).
		/// </summary>
		/// <param name="direction">Direction.</param>
		public virtual void PositionListForDirection(Direction direction)
		{
			// Make sure the creating of the list was successful
			if (this.m_ListObject == null)
				return;
			
			// Get the select field and list rect transforms
			RectTransform selectRect = (this.transform as RectTransform);
			RectTransform listRect = (this.m_ListObject.transform as RectTransform);
			
			// Determine the direction of the pop
			if (direction == Direction.Auto)
			{
				// Get the list world corners
				Vector3[] listWorldCorner = new Vector3[4];
				listRect.GetWorldCorners(listWorldCorner);
				
				// Check if the list is going outside to the bottom
				if (listWorldCorner[0].y < 0f)
				{
					direction = Direction.Up;
				}
				else
				{
					direction = Direction.Down;
				}
			}
			
			// Get the select field world corners
			Vector3[] selectWorldCorner = new Vector3[4];
			selectRect.GetWorldCorners(selectWorldCorner);
			
			// Handle up or down direction
			if (direction == Direction.Down)
			{
				listRect.position = new Vector3((selectWorldCorner[0].x + (float)this.listMargins.left), (selectWorldCorner[0].y + (float)this.listMargins.top * -1f), 0f);
			}
			else
			{
				listRect.position = new Vector3((selectWorldCorner[1].x + (float)this.listMargins.left), ((selectWorldCorner[1].y + (float)this.listMargins.bottom) + listRect.rect.height), 0f);
			}
		}
		
		/// <summary>
		/// Event invoked when the list dimensions change.
		/// </summary>
		protected virtual void ListDimensionsChanged()
		{
			if (!this.IsActive() || this.m_ListObject == null)
				return;
			
			// Check if the list size has changed
			if (this.m_LastListSize.Equals((this.m_ListObject.transform as RectTransform).sizeDelta))
				return;
			
			// Update the last list size
			this.m_LastListSize = (this.m_ListObject.transform as RectTransform).sizeDelta;
			
			// Update the list direction
			this.PositionListForDirection(this.m_Direction);
		}
		
		/// <summary>
		/// Tweens the list alpha.
		/// </summary>
		/// <param name="targetAlpha">Target alpha.</param>
		/// <param name="duration">Duration.</param>
		/// <param name="ignoreTimeScale">If set to <c>true</c> ignore time scale.</param>
		private void TweenListAlpha(float targetAlpha, float duration, bool ignoreTimeScale)
		{
			if (this.m_ListCanvasGroup == null)
				return;
			
			float currentAlpha = this.m_ListCanvasGroup.alpha;
			
			if (currentAlpha.Equals(targetAlpha))
				return;
			
			var floatTween = new FloatTween { duration = duration, startFloat = currentAlpha, targetFloat = targetAlpha };
			floatTween.AddOnChangedCallback(SetListAlpha);
			floatTween.AddOnFinishCallback(OnListTweenFinished);
			floatTween.ignoreTimeScale = ignoreTimeScale;
			this.m_FloatTweenRunner.StartTween(floatTween);
		}
		
		/// <summary>
		/// Sets the list alpha.
		/// </summary>
		/// <param name="alpha">Alpha.</param>
		private void SetListAlpha(float alpha)
		{
			if (this.m_ListCanvasGroup == null)
				return;
			
			// Set the alpha
			this.m_ListCanvasGroup.alpha = alpha;
		}
		
		/// <summary>
		/// Triggers the list animation.
		/// </summary>
		/// <param name="trigger">Trigger.</param>
		private void TriggerListAnimation(string trigger)
		{
			if (this.m_ListObject == null || string.IsNullOrEmpty(trigger))
				return;
			
			Animator animator = this.m_ListObject.GetComponent<Animator>();
			
			if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null)
				return;
			
			animator.ResetTrigger(this.listAnimationOpenTrigger);
			animator.ResetTrigger(this.listAnimationCloseTrigger);
			animator.SetTrigger(trigger);
		}
		
		/// <summary>
		/// Raises the list tween finished event.
		/// </summary>
		protected virtual void OnListTweenFinished()
		{
			// If the list is closed do a cleanup
			if (!this.IsOpen)
				this.ListCleanup();
		}
		
		/// <summary>
		/// Raises the list animation finish event.
		/// </summary>
		/// <param name="state">State.</param>
		protected virtual void OnListAnimationFinish(UISelectField_List.State state)
		{
			// If the list is closed do a cleanup
			if (state == UISelectField_List.State.Closed && !this.IsOpen)
				this.ListCleanup();
		}
	}
}