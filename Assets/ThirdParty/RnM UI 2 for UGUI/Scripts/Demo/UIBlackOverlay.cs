using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI
{
	[ExecuteInEditMode, RequireComponent(typeof(Image)), RequireComponent(typeof(CanvasGroup))]
	public class UIBlackOverlay : MonoBehaviour {
		
		private Image m_Image;
		private CanvasGroup m_CanvasGroup;
		private int m_WindowsCount = 0;
		
		protected void Awake()
		{
			this.m_Image = this.gameObject.GetComponent<Image>();
			this.m_CanvasGroup = this.gameObject.GetComponent<CanvasGroup>();
		}
		
		protected void Start()
		{
			// Non interactable
			this.m_CanvasGroup.interactable = false;
			
			// Hide the overlay
			this.Hide();
		}
		
		protected void OnEnable()
		{
			// Hide the overlay
			if (!Application.isPlaying)
				this.Hide();
		}
		
		public void Show()
		{
			// Show the overlay
			this.SetAlpha(1f);
			
			// Toggle block raycast on
			this.m_CanvasGroup.blocksRaycasts = true;
		}
		
		public void Hide()
		{
			// Hide the overlay
			this.SetAlpha(0f);
			
			// Toggle block raycast off
			this.m_CanvasGroup.blocksRaycasts = false;
		}
		
		public bool IsActive()
		{
			return (this.enabled && this.gameObject.activeInHierarchy);
		}
		
		public bool IsVisible()
		{
			return (this.m_Image.canvasRenderer.GetAlpha() > 0f);
		}
		
		public void OnTransitionBegin(UIWindow window, UIWindow.VisualState state, bool instant)
		{
			if (!this.IsActive() || window == null)
				return;
			
			// Check if we are receiving hide event and we are not showing the overlay to begin with, return
			if (state == UIWindow.VisualState.Hidden && !this.IsVisible())
				return;
			
			// Prepare transition duration
			float duration = (instant) ? 0f : window.transitionDuration;
			
			// Showing a window
			if (state == UIWindow.VisualState.Shown)
			{
				// Increase the window count so we know when to hide the overlay
				this.m_WindowsCount += 1;
				
				// Check if the overlay is already visible
				if (this.IsVisible())
				{
					// Bring the window forward
					UIUtility.BringToFront(window.gameObject);
					
					// Break
					return;
				}
				
				// Bring the overlay forward
				UIUtility.BringToFront(this.gameObject);
				
				// Bring the window forward
				UIUtility.BringToFront(window.gameObject);
				
				// Transition
				this.m_Image.CrossFadeAlpha(1f, duration, true);
				
				// Toggle block raycast on
				this.m_CanvasGroup.blocksRaycasts = true;
			}
			// Hiding a window
			else
			{
				// Decrease the window count
				this.m_WindowsCount -= 1;
				
				// Never go below 0
				if (this.m_WindowsCount < 0)
					this.m_WindowsCount = 0;
				
				// Check if we still have windows using the overlay
				if (this.m_WindowsCount > 0)
					return;
				
				// Transition
				this.m_Image.CrossFadeAlpha(0f, duration, true);
				
				// Toggle block raycast on
				this.m_CanvasGroup.blocksRaycasts = false;
			}
		}
		
		public void SetAlpha(float alpha)
		{
			this.m_Image.canvasRenderer.SetAlpha(alpha);
		}
	}
}