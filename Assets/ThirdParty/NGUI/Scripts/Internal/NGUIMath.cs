//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Helper class containing generic functions used throughout the UI library.
/// </summary>

static public class NGUIMath
{
	/// <summary>
	/// Lerp function that doesn't clamp the 'factor' in 0-1 range.
	/// </summary>

	static public float Lerp (float from, float to, float factor) { return from * (1f - factor) + to * factor; }

	/// <summary>
	/// Clamp the specified integer to be between 0 and below 'max'.
	/// </summary>

	static public int ClampIndex (int val, int max) { return (val < 0) ? 0 : (val < max ? val : max - 1); }

	/// <summary>
	/// Wrap the index using repeating logic, so that for example +1 past the end means index of '1'.
	/// </summary>

	static public int RepeatIndex (int val, int max)
	{
		if (max < 1) return 0;
		while (val < 0) val += max;
		while (val >= max) val -= max;
		return val;
	}

	/// <summary>
	/// Ensure that the angle is within -180 to 180 range.
	/// </summary>

	static public float WrapAngle (float angle)
	{
		while (angle > 180f) angle -= 360f;
		while (angle < -180f) angle += 360f;
		return angle;
	}

	/// <summary>
	/// In the shader, equivalent function would be 'fract'
	/// </summary>

	static public float Wrap01 (float val) { return val - Mathf.FloorToInt(val); }

	/// <summary>
	/// Convert a hexadecimal character to its decimal value.
	/// </summary>

	static public int HexToDecimal (char ch)
	{
		switch (ch)
		{
			case '0': return 0x0;
			case '1': return 0x1;
			case '2': return 0x2;
			case '3': return 0x3;
			case '4': return 0x4;
			case '5': return 0x5;
			case '6': return 0x6;
			case '7': return 0x7;
			case '8': return 0x8;
			case '9': return 0x9;
			case 'a':
			case 'A': return 0xA;
			case 'b':
			case 'B': return 0xB;
			case 'c':
			case 'C': return 0xC;
			case 'd':
			case 'D': return 0xD;
			case 'e':
			case 'E': return 0xE;
			case 'f':
			case 'F': return 0xF;
		}
		return 0xF;
	}

	/// <summary>
	/// Convert a single 0-15 value into its hex representation.
	/// It's coded because int.ToString(format) syntax doesn't seem to be supported by Unity's Flash. It just silently crashes.
	/// </summary>

	static public char DecimalToHexChar (int num)
	{
		if (num > 15) return 'F';
		if (num < 10) return (char)('0' + num);
		return (char)('A' + num - 10);
	}

	/// <summary>
	/// Convert a decimal value to its hex representation.
	/// It's coded because num.ToString("X6") syntax doesn't seem to be supported by Unity's Flash. It just silently crashes.
	/// string.Format("{0,6:X}", num).Replace(' ', '0') doesn't work either. It returns the format string, not the formatted value.
	/// </summary>

	static public string DecimalToHex (int num)
	{
		num &= 0xFFFFFF;
#if UNITY_FLASH
		StringBuilder sb = new StringBuilder();
		sb.Append(DecimalToHexChar((num >> 20) & 0xF));
		sb.Append(DecimalToHexChar((num >> 16) & 0xF));
		sb.Append(DecimalToHexChar((num >> 12) & 0xF));
		sb.Append(DecimalToHexChar((num >> 8) & 0xF));
		sb.Append(DecimalToHexChar((num >> 4) & 0xF));
		sb.Append(DecimalToHexChar(num & 0xF));
		return sb.ToString();
#else
		return num.ToString("X6");
#endif
	}

	/// <summary>
	/// Convert the specified color to RGBA32 integer format.
	/// </summary>

	static public int ColorToInt (Color c)
	{
		int retVal = 0;
		retVal |= Mathf.RoundToInt(c.r * 255f) << 24;
		retVal |= Mathf.RoundToInt(c.g * 255f) << 16;
		retVal |= Mathf.RoundToInt(c.b * 255f) << 8;
		retVal |= Mathf.RoundToInt(c.a * 255f);
		return retVal;
	}

	/// <summary>
	/// Convert the specified RGBA32 integer to Color.
	/// </summary>

	static public Color IntToColor (int val)
	{
		float inv = 1f / 255f;
		Color c = Color.black;
		c.r = inv * ((val >> 24) & 0xFF);
		c.g = inv * ((val >> 16) & 0xFF);
		c.b = inv * ((val >> 8) & 0xFF);
		c.a = inv * (val & 0xFF);
		return c;
	}

	/// <summary>
	/// Convert the specified integer to a human-readable string representing the binary value. Useful for debugging bytes.
	/// </summary>

	static public string IntToBinary (int val, int bits)
	{
		string final = "";

		for (int i = bits; i > 0; )
		{
			if (i == 8 || i == 16 || i == 24) final += " ";
			final += ((val & (1 << --i)) != 0) ? '1' : '0';
		}
		return final;
	}

	/// <summary>
	/// Convenience conversion function, allowing hex format (0xRrGgBbAa).
	/// </summary>

	static public Color HexToColor (uint val)
	{
		return IntToColor((int)val);
	}

	/// <summary>
	/// Convert from top-left based pixel coordinates to bottom-left based UV coordinates.
	/// </summary>

	static public Rect ConvertToTexCoords (Rect rect, int width, int height)
	{
		Rect final = rect;

		if (width != 0f && height != 0f)
		{
			final.xMin = rect.xMin / width;
			final.xMax = rect.xMax / width;
			final.yMin = 1f - rect.yMax / height;
			final.yMax = 1f - rect.yMin / height;
		}
		return final;
	}

	/// <summary>
	/// Convert from bottom-left based UV coordinates to top-left based pixel coordinates.
	/// </summary>

	static public Rect ConvertToPixels (Rect rect, int width, int height, bool round)
	{
		Rect final = rect;

		if (round)
		{
			final.xMin = Mathf.RoundToInt(rect.xMin * width);
			final.xMax = Mathf.RoundToInt(rect.xMax * width);
			final.yMin = Mathf.RoundToInt((1f - rect.yMax) * height);
			final.yMax = Mathf.RoundToInt((1f - rect.yMin) * height);
		}
		else
		{
			final.xMin = rect.xMin * width;
			final.xMax = rect.xMax * width;
			final.yMin = (1f - rect.yMax) * height;
			final.yMax = (1f - rect.yMin) * height;
		}
		return final;
	}

	/// <summary>
	/// Round the pixel rectangle's dimensions.
	/// </summary>

	static public Rect MakePixelPerfect (Rect rect)
	{
		rect.xMin = Mathf.RoundToInt(rect.xMin);
		rect.yMin = Mathf.RoundToInt(rect.yMin);
		rect.xMax = Mathf.RoundToInt(rect.xMax);
		rect.yMax = Mathf.RoundToInt(rect.yMax);
		return rect;
	}

	/// <summary>
	/// Round the texture coordinate rectangle's dimensions.
	/// </summary>

	static public Rect MakePixelPerfect (Rect rect, int width, int height)
	{
		rect = ConvertToPixels(rect, width, height, true);
		rect.xMin = Mathf.RoundToInt(rect.xMin);
		rect.yMin = Mathf.RoundToInt(rect.yMin);
		rect.xMax = Mathf.RoundToInt(rect.xMax);
		rect.yMax = Mathf.RoundToInt(rect.yMax);
		return ConvertToTexCoords(rect, width, height);
	}

	/// <summary>
	/// The much-dreaded half-pixel offset of DirectX9:
	/// http://drilian.com/2008/11/25/understanding-half-pixel-and-half-texel-offsets/
	/// </summary>

	static public Vector3 ApplyHalfPixelOffset (Vector3 pos)
	{
		RuntimePlatform platform = Application.platform;

		if (platform == RuntimePlatform.WindowsPlayer ||
			platform == RuntimePlatform.WindowsEditor)
		{
			pos.x = pos.x - 0.5f;
			pos.y = pos.y + 0.5f;
		}
		return pos;
	}

	/// <summary>
	/// Per-pixel offset taking scale into consideration.
	/// If the scale dimension is an odd number, it won't apply the offset.
	/// This is useful for centered sprites.
	/// </summary>

	static public Vector3 ApplyHalfPixelOffset (Vector3 pos, Vector3 scale)
	{
		RuntimePlatform platform = Application.platform;

		if (platform == RuntimePlatform.WindowsPlayer ||
			platform == RuntimePlatform.WindowsEditor)
		{
			if (Mathf.RoundToInt(scale.x) == (Mathf.RoundToInt(scale.x * 0.5f) * 2)) pos.x = pos.x - 0.5f;
			if (Mathf.RoundToInt(scale.y) == (Mathf.RoundToInt(scale.y * 0.5f) * 2)) pos.y = pos.y + 0.5f;
		}
		return pos;
	}

	/// <summary>
	/// Constrain 'rect' to be within 'area' as much as possible, returning the Vector2 offset necessary for this to happen.
	/// This function is useful when trying to restrict one area (window) to always be within another (viewport).
	/// </summary>

	static public Vector2 ConstrainRect (Vector2 minRect, Vector2 maxRect, Vector2 minArea, Vector2 maxArea)
	{
		Vector2 offset = Vector2.zero;

		float contentX = maxRect.x - minRect.x;
		float contentY = maxRect.y - minRect.y;

		float areaX = maxArea.x - minArea.x;
		float areaY = maxArea.y - minArea.y;

		if (contentX > areaX)
		{
			float diff = contentX - areaX;
			minArea.x -= diff;
			maxArea.x += diff;
		}

		if (contentY > areaY)
		{
			float diff = contentY - areaY;
			minArea.y -= diff;
			maxArea.y += diff;
		}

		if (minRect.x < minArea.x) offset.x += minArea.x - minRect.x;
		if (maxRect.x > maxArea.x) offset.x -= maxRect.x - maxArea.x;
		if (minRect.y < minArea.y) offset.y += minArea.y - minRect.y;
		if (maxRect.y > maxArea.y) offset.y -= maxRect.y - maxArea.y;
		
		return offset;
	}

	/// <summary>
	/// Calculate the 4 corners of a widget taking padding into consideration.
	/// </summary>

	static public Vector3[] CalculateWidgetCorners (UIWidget w)
	{
		Vector2 size = w.relativeSize;
		Vector2 offset = w.pivotOffset;
		Vector4 padding = w.relativePadding;

		float x0 = offset.x * size.x - padding.x;
		float y0 = offset.y * size.y + padding.y;

		float x1 = x0 + size.x + padding.x + padding.z;
		float y1 = y0 - size.y - padding.y - padding.w;

		Transform wt = w.cachedTransform;

		return new Vector3[]
		{
			wt.TransformPoint(x0, y0, 0f),
			wt.TransformPoint(x0, y1, 0f),
			wt.TransformPoint(x1, y1, 0f),
			wt.TransformPoint(x1, y0, 0f)
		};
	}

	/// <summary>
	/// Calculate the combined bounds of all widgets attached to the specified game object or its children (in world space).
	/// </summary>

	static public Bounds CalculateAbsoluteWidgetBounds (Transform trans)
	{
		UIWidget[] widgets = trans.GetComponentsInChildren<UIWidget>() as UIWidget[];
		if (widgets.Length == 0) return new Bounds(trans.position, Vector3.zero);

		Vector3 vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		Vector3 v;

		for (int i = 0, imax = widgets.Length; i < imax; ++i)
		{
			UIWidget w = widgets[i];
			Vector2 size = w.relativeSize;
			Vector2 offset = w.pivotOffset;
			float x = (offset.x + 0.5f) * size.x;
			float y = (offset.y - 0.5f) * size.y;
			size *= 0.5f;

			Transform wt = w.cachedTransform;

			v = wt.TransformPoint(new Vector3(x - size.x, y - size.y, 0f));
			vMax = Vector3.Max(v, vMax);
			vMin = Vector3.Min(v, vMin);

			v = wt.TransformPoint(new Vector3(x - size.x, y + size.y, 0f));
			vMax = Vector3.Max(v, vMax);
			vMin = Vector3.Min(v, vMin);

			v = wt.TransformPoint(new Vector3(x + size.x, y - size.y, 0f));
			vMax = Vector3.Max(v, vMax);
			vMin = Vector3.Min(v, vMin);

			v = wt.TransformPoint(new Vector3(x + size.x, y + size.y, 0f));
			vMax = Vector3.Max(v, vMax);
			vMin = Vector3.Min(v, vMin);
		}

		Bounds b = new Bounds(vMin, Vector3.zero);
		b.Encapsulate(vMax);
		return b;
	}

	/// <summary>
	/// Calculate the combined bounds of all widgets attached to the specified game object or its children (in relative-to-object space).
	/// </summary>

	static public Bounds CalculateRelativeWidgetBounds (Transform root, Transform child)
	{
		UIWidget[] widgets = child.GetComponentsInChildren<UIWidget>() as UIWidget[];
		if (widgets.Length == 0) return new Bounds(Vector3.zero, Vector3.zero);

		Vector3 vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

		Matrix4x4 toLocal = root.worldToLocalMatrix;

		for (int i = 0, imax = widgets.Length; i < imax; ++i)
		{
			UIWidget w = widgets[i];
			Vector2 size = w.relativeSize;
			Vector2 offset = w.pivotOffset;
			Transform toWorld = w.cachedTransform;

			float x = (offset.x + 0.5f) * size.x;
			float y = (offset.y - 0.5f) * size.y;
			size *= 0.5f;
			
			// Start with the corner of the widget
			Vector3 v = new Vector3(x - size.x, y - size.y, 0f);
			
			// Transform the coordinate from relative-to-widget to world space
			v = toWorld.TransformPoint(v);
			
			// Now transform from world space to relative-to-parent space
			v = toLocal.MultiplyPoint3x4(v);

			vMax = Vector3.Max(v, vMax);
			vMin = Vector3.Min(v, vMin);

			// Repeat for the other 3 corners
			v = new Vector3(x - size.x, y + size.y, 0f);
			v = toWorld.TransformPoint(v);
			v = toLocal.MultiplyPoint3x4(v);

			vMax = Vector3.Max(v, vMax);
			vMin = Vector3.Min(v, vMin);

			v = new Vector3(x + size.x, y - size.y, 0f);
			v = toWorld.TransformPoint(v);
			v = toLocal.MultiplyPoint3x4(v);

			vMax = Vector3.Max(v, vMax);
			vMin = Vector3.Min(v, vMin);

			v = new Vector3(x + size.x, y + size.y, 0f);
			v = toWorld.TransformPoint(v);
			v = toLocal.MultiplyPoint3x4(v);

			vMax = Vector3.Max(v, vMax);
			vMin = Vector3.Min(v, vMin);
		}

		Bounds b = new Bounds(vMin, Vector3.zero);
		b.Encapsulate(vMax);
		return b;
	}

	/// <summary>
	/// Convenience function.
	/// </summary>

	static public Bounds CalculateRelativeInnerBounds (Transform root, UISprite sprite)
	{
		if (sprite.type == UISprite.Type.Sliced)
		{
			Matrix4x4 toLocal = root.worldToLocalMatrix;
			Vector2 size = sprite.relativeSize;
			Vector2 offset = sprite.pivotOffset;
			Transform toWorld = sprite.cachedTransform;

			float x = (offset.x + 0.5f) * size.x;
			float y = (offset.y - 0.5f) * size.y;
			size *= 0.5f;

			float sx = toWorld.localScale.x;
			float sy = toWorld.localScale.y;

			// Get the border in pixels
			Vector4 border = sprite.border;

			// Convert pixels to relative coordinates
			if (sx != 0f)
			{
				border.x /= sx;
				border.z /= sx;
			}

			if (sy != 0f)
			{
				border.y /= sy;
				border.w /= sy;
			}

			// Calculate the relative dimensions
			float left = x - size.x + border.x;
			float right = x + size.x - border.z;
			float top = y - size.y + border.y;
			float bottom = y + size.y - border.w;

			// Start with the corner of the widget
			Vector3 v = new Vector3(left, top, 0f);
			v = toWorld.TransformPoint(v);
			v = toLocal.MultiplyPoint3x4(v);
			Bounds b = new Bounds(v, Vector3.zero);

			// Repeat for the other 3 corners
			v = new Vector3(left, bottom, 0f);
			v = toWorld.TransformPoint(v);
			v = toLocal.MultiplyPoint3x4(v);
			b.Encapsulate(v);

			v = new Vector3(right, bottom, 0f);
			v = toWorld.TransformPoint(v);
			v = toLocal.MultiplyPoint3x4(v);
			b.Encapsulate(v);

			v = new Vector3(right, top, 0f);
			v = toWorld.TransformPoint(v);
			v = toLocal.MultiplyPoint3x4(v);
			b.Encapsulate(v);
			return b;
		}
		return CalculateRelativeWidgetBounds(root, sprite.cachedTransform);
	}

	/// <summary>
	/// Calculate the combined bounds of all widgets attached to the specified game object or its children (in relative-to-object space).
	/// </summary>

	static public Bounds CalculateRelativeWidgetBounds (Transform trans)
	{
		return CalculateRelativeWidgetBounds(trans, trans);
	}

	/// <summary>
	/// This code is not framerate-independent:
	/// 
	/// target.position += velocity;
	/// velocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime * 9f);
	/// 
	/// But this code is:
	/// 
	/// target.position += NGUIMath.SpringDampen(ref velocity, 9f, Time.deltaTime);
	/// </summary>

	static public Vector3 SpringDampen (ref Vector3 velocity, float strength, float deltaTime)
	{
		// Dampening factor applied each millisecond
		if (deltaTime > 1f) deltaTime = 1f;
		float dampeningFactor = 1f - strength * 0.001f;
		int ms = Mathf.RoundToInt(deltaTime * 1000f);
		Vector3 offset = Vector3.zero;

		// Apply the offset for each millisecond
		for (int i = 0; i < ms; ++i)
		{
			// Mimic 60 FPS the editor runs at
			offset += velocity * 0.06f;
			velocity *= dampeningFactor;
		}
		return offset;
	}

	/// <summary>
	/// Same as the Vector3 version, it's a framerate-independent Lerp.
	/// </summary>

	static public Vector2 SpringDampen (ref Vector2 velocity, float strength, float deltaTime)
	{
		// Dampening factor applied each millisecond
		if (deltaTime > 1f) deltaTime = 1f;
		float dampeningFactor = 1f - strength * 0.001f;
		int ms = Mathf.RoundToInt(deltaTime * 1000f);
		Vector2 offset = Vector2.zero;

		// Apply the offset for each millisecond
		for (int i = 0; i < ms; ++i)
		{
			// Mimic 60 FPS the editor runs at
			offset += velocity * 0.06f;
			velocity *= dampeningFactor;
		}
		return offset;
	}

	/// <summary>
	/// Calculate how much to interpolate by.
	/// </summary>

	static public float SpringLerp (float strength, float deltaTime)
	{
		if (deltaTime > 1f) deltaTime = 1f;
		int ms = Mathf.RoundToInt(deltaTime * 1000f);
		deltaTime = 0.001f * strength;
		float cumulative = 0f;
		for (int i = 0; i < ms; ++i) cumulative = Mathf.Lerp(cumulative, 1f, deltaTime);
		return cumulative;
	}

	/// <summary>
	/// Mathf.Lerp(from, to, Time.deltaTime * strength) is not framerate-independent. This function is.
	/// </summary>

	static public float SpringLerp (float from, float to, float strength, float deltaTime)
	{
		if (deltaTime > 1f) deltaTime = 1f;
		int ms = Mathf.RoundToInt(deltaTime * 1000f);
		deltaTime = 0.001f * strength;
		for (int i = 0; i < ms; ++i) from = Mathf.Lerp(from, to, deltaTime);
		return from;
	}

	/// <summary>
	/// Vector2.Lerp(from, to, Time.deltaTime * strength) is not framerate-independent. This function is.
	/// </summary>

	static public Vector2 SpringLerp (Vector2 from, Vector2 to, float strength, float deltaTime)
	{
		return Vector2.Lerp(from, to, SpringLerp(strength, deltaTime));
	}

	/// <summary>
	/// Vector3.Lerp(from, to, Time.deltaTime * strength) is not framerate-independent. This function is.
	/// </summary>

	static public Vector3 SpringLerp (Vector3 from, Vector3 to, float strength, float deltaTime)
	{
		return Vector3.Lerp(from, to, SpringLerp(strength, deltaTime));
	}

	/// <summary>
	/// Quaternion.Slerp(from, to, Time.deltaTime * strength) is not framerate-independent. This function is.
	/// </summary>

	static public Quaternion SpringLerp (Quaternion from, Quaternion to, float strength, float deltaTime)
	{
		return Quaternion.Slerp(from, to, SpringLerp(strength, deltaTime));
	}

	/// <summary>
	/// Since there is no Mathf.RotateTowards...
	/// </summary>

	static public float RotateTowards (float from, float to, float maxAngle)
	{
		float diff = WrapAngle(to - from);
		if (Mathf.Abs(diff) > maxAngle) diff = maxAngle * Mathf.Sign(diff);
		return from + diff;
	}
}
