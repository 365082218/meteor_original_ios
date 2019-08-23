using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI.Tweens;
using System.Collections;

namespace UnityEngine.UI
{
	public class UILoadingBar : MonoBehaviour {
		
		public class OnChangeEvent : UnityEvent<float> {}
		
		public Image imageComponent;
		public Text textComponent;
		public float Duration = 0.1f;
		public OnChangeEvent onChange = new OnChangeEvent();
        //public UnityAction onfinished;
		// Tween controls
		[NonSerialized] private readonly TweenRunner<FloatTween> m_FloatTweenRunner;
		
		// Called by Unity prior to deserialization, 
		// should not be called by users
		protected UILoadingBar()
		{
			if (this.m_FloatTweenRunner == null)
				this.m_FloatTweenRunner = new TweenRunner<FloatTween>();
			
			this.m_FloatTweenRunner.Init(this);
		}
		
		protected void Start()
		{
			if (this.imageComponent != null)
			{
				this.imageComponent.type = Image.Type.Filled;
				this.imageComponent.fillMethod = Image.FillMethod.Horizontal;
				this.imageComponent.fillAmount = 0f;
			}
			
			if (this.textComponent != null)
				this.textComponent.text = "0%";
				
			//this.StartDemoTween();
		}
		
		protected void SetFillAmount(float amount)
		{
			if (this.imageComponent != null)
				this.imageComponent.fillAmount = amount;
				
			if (this.textComponent != null)
				this.textComponent.text = (amount * 100).ToString("0") + "%";
				
			if (this.onChange != null)
				this.onChange.Invoke(amount);
		}
		
		//public void StartDemoTween()
		//{
		//	if (this.imageComponent == null)
		//		return;
			
		//	float targetAmount = (this.imageComponent.fillAmount > 0.5f) ? 0f : 1f;
		
		//	FloatTween floatTween = new FloatTween { duration = this.Duration, startFloat = this.imageComponent.fillAmount, targetFloat = targetAmount };
		//	floatTween.AddOnChangedCallback(SetFillAmount);
		//	floatTween.AddOnFinishCallback(OnTweenFinished);
		//	floatTween.ignoreTimeScale = true;
		//	this.m_FloatTweenRunner.StartTween(floatTween);
		//}
		
		protected void OnTweenFinished()
		{
		}

        public void SetProgress(float percent)
        {
            float targetAmount = percent;
            FloatTween floatTween = new FloatTween { duration = this.Duration, startFloat = this.imageComponent.fillAmount, targetFloat = targetAmount };
            floatTween.AddOnChangedCallback(SetFillAmount);
            floatTween.AddOnFinishCallback(OnTweenFinished);
            floatTween.ignoreTimeScale = true;
            this.m_FloatTweenRunner.StartTween(floatTween);
        }
	}
}