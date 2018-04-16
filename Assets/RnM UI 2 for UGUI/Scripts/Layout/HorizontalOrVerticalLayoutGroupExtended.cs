using System;

namespace UnityEngine.UI
{
	public abstract class HorizontalOrVerticalLayoutGroupExtended : HorizontalOrVerticalLayoutGroup {
		
		[SerializeField] private bool m_SubtractMarginHorizontal = false;
		[SerializeField] private bool m_SubtractMarginVertical = false;
		
		protected void CalcAlongAxisExtended(int axis, bool isVertical)
		{
			float padding = (float)((axis != 0) ? base.padding.vertical : base.padding.horizontal);
			float totalMin = padding;
			float totalPreffered = padding;
			float totalFlexible = 0f;
			
			bool notOrderingAxis = isVertical ^ axis == 1;
			
			for (int i = 0; i < base.rectChildren.Count; i++)
			{
				RectTransform rect = base.rectChildren[i];
				float minSize = LayoutUtility.GetMinSize(rect, axis);
				float preferredSize = LayoutUtility.GetPreferredSize(rect, axis);
				float flexibleSize = LayoutUtility.GetFlexibleSize(rect, axis);
				RectOffset margins = LayoutUtilityExtended.GetMargin(rect);
				float margin = (float)((axis != 0) ? margins.vertical : margins.horizontal);
				
				if ((axis != 0) ? this.childForceExpandHeight : this.childForceExpandWidth)
				{
					flexibleSize = Mathf.Max(flexibleSize, 1f);
				}
				
				if (notOrderingAxis)
				{
					totalMin = Mathf.Max(minSize + padding + margin, totalMin);
					totalPreffered = Mathf.Max(preferredSize + padding + margin, totalPreffered);
					totalFlexible = Mathf.Max(flexibleSize, totalFlexible);
				}
				else
				{
					totalMin += minSize + this.spacing + margin;
					totalPreffered += preferredSize + this.spacing + margin;
					totalFlexible += flexibleSize;
				}
			}
			
			if (!notOrderingAxis && base.rectChildren.Count > 0)
			{
				totalMin -= this.spacing;
				totalPreffered -= this.spacing;
			}
			
			totalPreffered = Mathf.Max(totalMin, totalPreffered);
			
			base.SetLayoutInputForAxis(totalMin, totalPreffered, totalFlexible, axis);
		}
		
		protected void SetChildrenAlongAxisExtended(int axis, bool isVertical)
		{
			float axisSize = base.rectTransform.rect.size[axis];
			bool notOrderingAxis = isVertical ^ axis == 1;
			
			if (notOrderingAxis)
			{
				float axisSize2 = axisSize - (float)((axis != 0) ? base.padding.vertical : base.padding.horizontal);
				
				for (int i = 0; i < base.rectChildren.Count; i++)
				{
					RectTransform rect = base.rectChildren[i];
					float minSize = LayoutUtility.GetMinSize(rect, axis);
					float preferredSize = LayoutUtility.GetPreferredSize(rect, axis);
					float flexibleSize = LayoutUtility.GetFlexibleSize(rect, axis);
					RectOffset margin = LayoutUtilityExtended.GetMargin(rect);
					
					if ((axis != 0) ? this.childForceExpandHeight : this.childForceExpandWidth)
					{
						flexibleSize = Mathf.Max(flexibleSize, 1f);
					}
					
					float elementSize = Mathf.Clamp(axisSize2, minSize, (flexibleSize <= 0f) ? preferredSize : axisSize);
					
					// Subtract margins
					if (axis == 0 && this.m_SubtractMarginHorizontal)
					{
						elementSize -= margin.horizontal;
						elementSize = Mathf.Clamp(elementSize, minSize, elementSize);
					}
					else if (axis != 0 && this.m_SubtractMarginVertical)
					{
						elementSize -= margin.vertical;
						elementSize = Mathf.Clamp(elementSize, minSize, elementSize);
					}
					
					float startOffset = ((axis != 0) ? margin.top : margin.left) + base.GetStartOffset(axis, elementSize);
					
					base.SetChildAlongAxis(rect, axis, startOffset, elementSize);
				}
			}
			else
			{
				float offset = (float)((axis != 0) ? base.padding.top : base.padding.left);
				
				if (base.GetTotalFlexibleSize(axis) == 0f && base.GetTotalPreferredSize(axis) < axisSize)
				{
					offset = base.GetStartOffset(axis, base.GetTotalPreferredSize(axis) - (float)((axis != 0) ? base.padding.vertical : base.padding.horizontal));
				}
				
				float axisSize6 = 0f;
				
				if (base.GetTotalMinSize(axis) != base.GetTotalPreferredSize(axis))
				{
					axisSize6 = Mathf.Clamp01((axisSize - base.GetTotalMinSize(axis)) / (base.GetTotalPreferredSize(axis) - base.GetTotalMinSize(axis)));
				}
				
				float axisSize7 = 0f;
				
				if (axisSize > base.GetTotalPreferredSize(axis) && base.GetTotalFlexibleSize(axis) > 0f)
				{
					axisSize7 = (axisSize - base.GetTotalPreferredSize(axis)) / base.GetTotalFlexibleSize(axis);
				}
				
				for (int j = 0; j < base.rectChildren.Count; j++)
				{
					RectTransform rect2 = base.rectChildren[j];
					float minSize2 = LayoutUtility.GetMinSize(rect2, axis);
					float preferredSize2 = LayoutUtility.GetPreferredSize(rect2, axis);
					float flexibleSize = LayoutUtility.GetFlexibleSize(rect2, axis);
					RectOffset margin = LayoutUtilityExtended.GetMargin(rect2);
					
					if ((axis != 0) ? this.childForceExpandHeight : this.childForceExpandWidth)
					{
						flexibleSize = Mathf.Max(flexibleSize, 1f);
					}
					
					float elementSize = Mathf.Lerp(minSize2, preferredSize2, axisSize6);
					elementSize += flexibleSize * axisSize7;
					
					offset += ((axis != 0) ? margin.top : margin.left);
					base.SetChildAlongAxis(rect2, axis, offset, elementSize);
					
					offset += elementSize + this.spacing + ((axis != 0) ? margin.bottom : margin.right);
				}
			}
		}
	}
}