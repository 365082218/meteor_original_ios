using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
	[AddComponentMenu("Layout/Vertical Layout Group Extended", 151)]
	public class VerticalLayoutGroupExtended : HorizontalOrVerticalLayoutGroupExtended
	{
		public override void CalculateLayoutInputHorizontal()
		{
			base.CalculateLayoutInputHorizontal();
			base.CalcAlongAxisExtended(0, true);
		}
		
		public override void CalculateLayoutInputVertical()
		{
			base.CalcAlongAxisExtended(1, true);
		}
		
		public override void SetLayoutHorizontal()
		{
			base.SetChildrenAlongAxisExtended(0, true);
		}
		
		public override void SetLayoutVertical()
		{
			base.SetChildrenAlongAxisExtended(1, true);
		}
	}
}