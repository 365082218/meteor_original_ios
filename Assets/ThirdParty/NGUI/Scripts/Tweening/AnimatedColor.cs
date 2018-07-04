//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Makes it possible to animate a color of the widget.
/// </summary>

[ExecuteInEditMode]
[RequireComponent(typeof(UIWidget))]
public class AnimatedColor : MonoBehaviour
{
	public Color color = Color.white;
	
	UILabel mLabel;
	
	void Awake () { mLabel = GetComponent<UILabel>(); }
	void Update () { mLabel.color = color; }
}
