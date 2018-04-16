//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright ㄟ2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Sample script showing how easy it is to implement a standard button that swaps sprites.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Image Button")]
public class UIImageButton : MonoBehaviour
{
	public UISprite target;
	public string normalSprite;
	public string hoverSprite;
	public string pressedSprite;
	public string disabledSprite;
	
	public bool isEnabled
	{
		get
		{
			Collider col = GetComponent<Collider>();
			return col && col.enabled;
		}
		set
		{
			Collider col = GetComponent<Collider>();
			if (!col) return;

			if (col.enabled != value)
			{
				col.enabled = value;				
			}
			UpdateImage();
		}
	}

	void OnEnable ()
	{
		UpdateImage();		
	}

	void Start ()
	{
		if (target == null) target = GetComponentInChildren<UISprite>();
	}

	void OnHover (bool isOver)
	{
		if (enabled)
		{
			UpdateImage();
		}
	}

	void OnPress (bool pressed)
	{
		if (enabled)
		{
			if (target != null)
			{
				target.spriteName = pressed ? pressedSprite : normalSprite;
				//target.MakePixelPerfect();
			}
			//UpdateImage();
		}
	}
	
	void UpdateImage()
	{
		if (target != null)
		{		
			if (isEnabled)
				target.spriteName = UICamera.IsHighlighted(gameObject) ? hoverSprite : normalSprite;
			else
				target.spriteName = disabledSprite;
				
			target.MakePixelPerfect();
		}
	}
}