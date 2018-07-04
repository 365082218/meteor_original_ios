using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
	public static class LayoutUtilityExtended
	{
		public static RectOffset GetMargin(RectTransform rect)
		{
			return LayoutUtilityExtended.GetMargin(rect, new RectOffset(0, 0, 0, 0));
		}
		
		public static RectOffset GetMargin(RectTransform rect, RectOffset defaultValue)
		{
			if (rect == null)
				return new RectOffset(0, 0, 0, 0);

			RectOffset currentMargin = defaultValue;
			LayoutElementExtended[] list = rect.GetComponents<LayoutElementExtended>();
			
			for (int i = 0; i < list.Length; i++)
			{
				LayoutElementExtended layoutElementExtended = list[i];
				
				if (layoutElementExtended.enabled)
				{
					RectOffset elementMargin = layoutElementExtended.margin;

					currentMargin.top += elementMargin.top;
					currentMargin.bottom += elementMargin.bottom;
					currentMargin.left += elementMargin.left;
					currentMargin.right += elementMargin.right;
				}
			}
			
			return currentMargin;
		}
	}
}
