using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

namespace UnityEngine.UI
{
	[ExecuteInEditMode, DisallowMultipleComponent]
	public class UIRealmSelect_Realm : Toggle {
		
		public enum Status
		{
			Online,
			Offline,
		}
		
		public enum Population
		{
			Low,
			Medium,
			High,
		}
		
		[SerializeField] private bool m_IsClosed = false;
		[SerializeField] private string m_TitleString = "";
		[SerializeField] private Status m_CurrentStatus = Status.Offline;
		[SerializeField] private Population m_CurrentPopulation = Population.Low;
		[SerializeField] private int m_CharactersCount = 0;
		
		[SerializeField] private Image m_Image;
		[SerializeField] private Sprite m_HoverSprite;
		[SerializeField] private Sprite m_ActiveSprite;
		
		[SerializeField] private Text m_TitleText;
		[SerializeField] private Color m_TitleNormalColor = Color.white;
		[SerializeField] private Color m_TitleHoverColor = Color.white;
		[SerializeField] private Color m_TitleClosedColor = Color.gray;
		
		[SerializeField] private Text m_ClosedText;
		[SerializeField] private Color m_ClosedNormalColor = Color.white;
		[SerializeField] private Color m_ClosedHoverColor = Color.white;
		
		[SerializeField] private Text m_StatusText;
		[SerializeField] private string m_StatusOnlineString = "Online";
		[SerializeField] private Color m_StatusOnlineColor = Color.green;
		[SerializeField] private string m_StatusOfflineString = "Offline";
		[SerializeField] private Color m_StatusOfflineColor = Color.red;
		
		[SerializeField] private Text m_PopulationText;
		[SerializeField] private Color m_PopulationLowColor = Color.green;
		[SerializeField] private Color m_PopulationMediumColor = Color.yellow;
		[SerializeField] private Color m_PopulationHighColor = Color.red;
				
		[SerializeField] private Text m_CharactersText;
		
		/// <summary>
		/// Gets or sets the realm title.
		/// </summary>
		/// <value>The title.</value>
		public string title
		{
			get { return this.m_TitleString; }
			set {
				this.m_TitleString = value;
				this.ApplyTitle();
			}
		}
		
		/// <summary>
		/// Gets or sets a value indicating whether this realm is closed.
		/// </summary>
		/// <value><c>true</c> if this instance is closed; otherwise, <c>false</c>.</value>
		public bool IsClosed
		{
			get { return this.m_IsClosed; }
			set {
				this.m_IsClosed = value;
				this.ApplyClosedState();
			}
		}
		
		/// <summary>
		/// Gets or sets the current realm status.
		/// </summary>
		/// <value>The current status.</value>
		public Status status
		{
			get { return this.m_CurrentStatus; }
			set {
				this.m_CurrentStatus = value;
				this.ApplyStatusState();
			}
		}
		
		/// <summary>
		/// Gets or sets the current realm population.
		/// </summary>
		/// <value>The current population.</value>
		public Population population
		{
			get { return this.m_CurrentPopulation; }
			set {
				this.m_CurrentPopulation = value;
				this.ApplyPopulationState();
			}
		}
		
		/// <summary>
		/// Gets or sets the realm characters count.
		/// </summary>
		/// <value>The characters.</value>
		public int characters
		{
			get { return this.m_CharactersCount; }
			set {
				this.m_CharactersCount = value;
				this.ApplyCharactersCount();
			}
		}
		
		protected override void Awake()
		{
			base.Awake();
			
			this.transition = Selectable.Transition.None;
			this.toggleTransition = Toggle.ToggleTransition.None;
			this.graphic = null;
			this.targetGraphic = null;
		}
		
		protected override void Start()
		{
			base.Start();
			this.DoStateTransition(SelectionState.Normal, true);
			this.ResetToStartingState();
			this.onValueChanged.AddListener(OnActiveStateChange);
			
			// Allow the toggles to be switch off
			if (this.group != null)
				this.group.allowSwitchOff = true;
		}
		
#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate();
			this.ResetToStartingState();
		}
#endif
		
		/// <summary>
		/// Raises the active state change event.
		/// </summary>
		/// <param name="isOn">If set to <c>true</c> is on.</param>
		protected virtual void OnActiveStateChange(bool isOn)
		{
			this.DoStateTransition(this.currentSelectionState, false);
		}
		
		/// <summary>
		/// Instantly clears the current state.
		/// </summary>
		protected override void InstantClearState()
		{
			base.InstantClearState();
			this.ResetToStartingState();
		}
		
		/// <summary>
		/// Resets to starting state.
		/// </summary>
		protected void ResetToStartingState()
		{
			// Reset the image sprite
			if (this.m_Image != null)
				this.m_Image.overrideSprite = (this.isOn ? this.m_ActiveSprite : null);
				
			// Reset the title text color
			if (this.m_TitleText != null)
				this.m_TitleText.canvasRenderer.SetColor((this.m_IsClosed) ? this.m_TitleClosedColor : this.m_TitleNormalColor);
			
			// Apply the title
			this.ApplyTitle();
			
			// Reset the closed state
			this.ApplyClosedState();
			
			// Reset the status text
			this.ApplyStatusState();
			
			// Reset the population text
			this.ApplyPopulationState();
		}
		
		/// <summary>
		/// Does the state transition.
		/// </summary>
		/// <param name="state">State.</param>
		/// <param name="instant">If set to <c>true</c> instant.</param>
		protected override void DoStateTransition(SelectionState state, bool instant)
		{
			base.DoStateTransition(state, instant);
			
			// Check the state which we are transitioning to
			if (state == SelectionState.Normal || state == SelectionState.Highlighted)
			{
				// Transition the image
				if (this.m_Image != null)
				{
					switch (state)
					{
					case SelectionState.Normal:
						this.m_Image.overrideSprite = (this.isOn ? this.m_ActiveSprite : null);
						break;
					case SelectionState.Highlighted:
						this.m_Image.overrideSprite = (this.isOn ? this.m_ActiveSprite : this.m_HoverSprite);
						break;
					}
				}
				
				// Transition the title text
				if (this.m_TitleText != null)
				{
					// Other states
					switch (state)
					{
					case SelectionState.Normal:
						this.m_TitleText.canvasRenderer.SetColor((!this.m_IsClosed) ? this.m_TitleNormalColor : this.m_TitleClosedColor);
						break;
					case SelectionState.Highlighted:
						this.m_TitleText.canvasRenderer.SetColor(this.m_TitleHoverColor);
						break;
					}
				}
				
				// Transition the closed text
				if (this.m_ClosedText != null)
				{
					// Other states
					switch (state)
					{
					case SelectionState.Normal:
						this.m_ClosedText.canvasRenderer.SetColor(this.m_ClosedNormalColor);
						break;
					case SelectionState.Highlighted:
						this.m_ClosedText.canvasRenderer.SetColor(this.m_ClosedHoverColor);
						break;
					}
				}
			}
		}
		
		/// <summary>
		/// Raises the pointer click event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public override void OnPointerClick(PointerEventData eventData)
		{
			// Prevent toggle activation when the realm is close
			if (this.IsClosed)
				return;
			
			base.OnPointerClick(eventData);
		}
		
		/// <summary>
		/// Raises the submit event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public override void OnSubmit(BaseEventData eventData)
		{
			// Prevent toggle activation when the realm is close
			if (this.IsClosed)
				return;
			
			base.OnSubmit(eventData);
		}
		
		/// <summary>
		/// Applies the realm title string.
		/// </summary>
		protected void ApplyTitle()
		{
			if (this.m_TitleText != null)
				this.m_TitleText.text = this.m_TitleString;
		}
		
		/// <summary>
		/// Applies the current closed state.
		/// </summary>
		protected void ApplyClosedState()
		{
			if (this.m_IsClosed)
			{
				// Apply title text color
				if (this.m_TitleText != null)
					this.m_TitleText.canvasRenderer.SetColor(this.m_TitleClosedColor);
					
				// Activate the closed text game object
				if (this.m_ClosedText != null)
					this.m_ClosedText.gameObject.SetActive(true);
				
				// Deselect
				this.isOn = false;
			}
			else
			{
				// Transition to the current state
				this.DoStateTransition(this.currentSelectionState, !Application.isPlaying);
				
				// Deactivate the closed text game object
				if (this.m_ClosedText != null)
					this.m_ClosedText.gameObject.SetActive(false);
			}
		}
		
		/// <summary>
		/// Applies the current realm status state.
		/// </summary>
		protected void ApplyStatusState()
		{
			if (this.m_StatusText != null)
			{
				this.m_StatusText.text = ((this.m_CurrentStatus == Status.Online) ? this.m_StatusOnlineString : this.m_StatusOfflineString);
				this.m_StatusText.canvasRenderer.SetColor((this.m_CurrentStatus == Status.Online) ? this.m_StatusOnlineColor : this.m_StatusOfflineColor);
			}
		}
		
		/// <summary>
		/// Applies the current realm population state.
		/// </summary>
		protected void ApplyPopulationState()
		{
			if (this.m_PopulationText != null)
			{
				Color color = Color.white;
				
				switch (this.m_CurrentPopulation)
				{
				case Population.Low:
					color = this.m_PopulationLowColor;
					break;
				case Population.Medium:
					color = this.m_PopulationMediumColor;
					break;
				case Population.High:
					color = this.m_PopulationHighColor;
					break;	
				}
				
				this.m_PopulationText.text = this.m_CurrentPopulation.ToString();
				this.m_PopulationText.canvasRenderer.SetColor(color);
			}
		}
		
		/// <summary>
		/// Applies the realm characters count.
		/// </summary>
		protected void ApplyCharactersCount()
		{
			if (this.m_CharactersText != null)
				this.m_CharactersText.text = this.m_CharactersCount.ToString();
		}
	}
}