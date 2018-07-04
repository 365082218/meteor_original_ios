using UnityEngine;
using UnityEngine.UI.Tweens;
using System.Collections;

namespace UnityEngine.UI
{
	public class UILoadingBar_OverlayFinish : MonoBehaviour {
		
		/// <summary>
		/// The target image for the finish.
		/// </summary>
		public Image targetImage;
		
		/// <summary>
		/// The loading bar.
		/// </summary>
		public UILoadingBar bar;
		
		/// <summary>
		/// The horizontal offset.
		/// </summary>
		public float offset = 0f;
		
		/// <summary>
		/// Gets or set a value to enable or disable the auto visibility.
		/// </summary>
		public bool autoVisibility = true;
		
		/// <summary>
		/// The percent after which the finish should shown.
		/// </summary>
		public float showAfterPct = 0.05f;
		
		/// <summary>
		/// The percent after which the finish should hidden.
		/// </summary>
		public float hideAfterPct = 0.95f;
		
		/// <summary>
		/// Gets or set a value to enable or dislable fading.
		/// </summary>
		public bool fading = true;
		
		/// <summary>
		/// The duration of the fade.
		/// </summary>
		public float fadeDuration = 0.2f;
		
		private CanvasGroup canvasGroup;
		private float defaultFinishWidth = 0;
		private float defaultFinishAlpha = 1f;
		
		// Tween controls
		[System.NonSerialized] private readonly TweenRunner<FloatTween> m_FloatTweenRunner;
		
		// Called by Unity prior to deserialization, 
		// should not be called by users
		protected UILoadingBar_OverlayFinish()
		{
			if (this.m_FloatTweenRunner == null)
				this.m_FloatTweenRunner = new TweenRunner<FloatTween>();
			
			this.m_FloatTweenRunner.Init(this);
		}
		
		protected void Start()
		{
			if (this.targetImage == null || this.bar == null)
			{
				Debug.LogWarning(this.GetType() + " requires target Image and Test_LoadingBar in order to work.", this);
				this.enabled = false;
				return;
			}
			
			// Make sure our finish has canvas group
			if (this.autoVisibility)
			{
				this.canvasGroup = this.targetImage.gameObject.GetComponent<CanvasGroup>();
				if (this.canvasGroup == null) this.canvasGroup = this.targetImage.gameObject.AddComponent<CanvasGroup>();
				
				// Get the default alpha of the target widget
				this.defaultFinishAlpha = this.canvasGroup.alpha;
				
				// check if we need to hide the finish
				if (this.showAfterPct > 0f)
					this.canvasGroup.alpha = (this.targetImage.fillAmount < this.showAfterPct) ? 0f : 1f;
			}
			
			// Make sure the image anchor is left
			this.targetImage.rectTransform.anchorMin = new Vector2(0f, this.targetImage.rectTransform.anchorMin.y);
			this.targetImage.rectTransform.anchorMax = new Vector2(0f, this.targetImage.rectTransform.anchorMax.y);
			
			// Get the default with of the target widget
			this.defaultFinishWidth = this.targetImage.rectTransform.rect.width;
			
			// Hook on change event
			this.bar.onChange.AddListener(OnBarFillChange);
		}
		
		/// <summary>
		/// Raises the bar fill change event.
		/// </summary>
		/// <param name="amount">Amount.</param>
		public void OnBarFillChange(float amount)
		{
			// Calculate the bar fill based on it's width and value
			float fillWidth = ((float)this.bar.imageComponent.rectTransform.rect.width * this.bar.imageComponent.fillAmount);
			
			// Check if the fill width is too small to bother with the ending
			if (fillWidth <= (1f + (float)this.offset))
			{
				this.targetImage.gameObject.SetActive(false);
				return;
			}
			else if (!this.targetImage.gameObject.activeSelf)
			{
				// Re-enable
				this.targetImage.gameObject.SetActive(true);
			}
			
			// Position the ending at the end of the fill
			this.targetImage.rectTransform.anchoredPosition = new Vector2(
				(this.offset + fillWidth), 
				this.targetImage.rectTransform.anchoredPosition.y
			);
			
			// Check if the fill width is too great to handle the ending width
			if (fillWidth < this.defaultFinishWidth)
			{
				// Change the width to the fill width
				this.targetImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Round(fillWidth));
			}
			else if (this.targetImage.rectTransform.rect.width != this.defaultFinishWidth)
			{
				// Restore default width
				this.targetImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, this.defaultFinishWidth);
			}
			
			// Show / Hide the finish
			if (this.autoVisibility)
			{
				// Check if the finish needs the be shown
				if (this.bar.imageComponent.fillAmount >= this.showAfterPct && this.bar.imageComponent.fillAmount < this.hideAfterPct)
				{
					// Fade in if not 100%
					if (this.fading)
					{
						FloatTween floatTween = new FloatTween { duration = this.fadeDuration, startFloat = this.canvasGroup.alpha, targetFloat = this.defaultFinishAlpha };
						floatTween.AddOnChangedCallback(SetFinishAlpha);
						floatTween.ignoreTimeScale = true;
						this.m_FloatTweenRunner.StartTween(floatTween);
					}
					else
						this.SetFinishAlpha(this.defaultFinishAlpha);
				}
				else if (this.bar.imageComponent.fillAmount >= this.hideAfterPct || this.bar.imageComponent.fillAmount < this.showAfterPct)
				{
					// Fade out at 100%
					if (this.fading)
					{
						FloatTween floatTween = new FloatTween { duration = this.fadeDuration, startFloat = this.canvasGroup.alpha, targetFloat = 0f };
						floatTween.AddOnChangedCallback(SetFinishAlpha);
						floatTween.ignoreTimeScale = true;
						this.m_FloatTweenRunner.StartTween(floatTween);
					}
					else
						this.SetFinishAlpha(0f);
				}
			}
		}
		
		/// <summary>
		/// Sets the finish image alpha.
		/// </summary>
		/// <param name="alpha">Alpha.</param>
		private void SetFinishAlpha(float alpha)
		{
			if (this.canvasGroup == null)
				return;
			
			this.canvasGroup.alpha = alpha;
		}
	}
}