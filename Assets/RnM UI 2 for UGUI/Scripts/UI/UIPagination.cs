using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
	[AddComponentMenu("UI/Pagination", 82)]
	public class UIPagination : MonoBehaviour {
		
		public Button buttonPrev;
		public Button buttonNext;
		
		public Transform pagesContainer;
		
		private int activePage = 0;
		
		void Start()
		{
			if (this.buttonPrev != null)
				this.buttonPrev.onClick.AddListener(OnPrevClick);
			
			if (this.buttonNext != null)
				this.buttonNext.onClick.AddListener(OnNextClick);
			
			// Detect active page
			if (this.pagesContainer != null && this.pagesContainer.childCount > 0)
			{
				for (int i = 0; i < this.pagesContainer.childCount; i++)
				{
					if (this.pagesContainer.GetChild(i).gameObject.activeSelf)
					{
						this.activePage = i;
						break;
					}
				}
			}
			
			// Prepare the pages visibility
			this.UpdatePagesVisibility();
		}
		
		private void UpdatePagesVisibility()
		{
			if (this.pagesContainer != null && this.pagesContainer.childCount > 0)
			{
				for (int i = 0; i < this.pagesContainer.childCount; i++)
					this.pagesContainer.GetChild(i).gameObject.SetActive((i == activePage) ? true : false);
			}
		}
		
		private void OnPrevClick()
		{
			if (!this.enabled || this.pagesContainer == null)
				return;
			
			// If we are on the first page, jump to the last one
			if (this.activePage == 0)
				this.activePage = this.pagesContainer.childCount - 1;
			else
				this.activePage -= 1;
			
			// Activate
			this.UpdatePagesVisibility();
		}
		
		private void OnNextClick()
		{
			if (!this.enabled || this.pagesContainer == null)
				return;
			
			// If we are on the last page, jump to the first one
			if (this.activePage == (this.pagesContainer.childCount - 1))
				this.activePage = 0;
			else
				this.activePage += 1;
			
			// Activate
			this.UpdatePagesVisibility();
		}
	}
}
