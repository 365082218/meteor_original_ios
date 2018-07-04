using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
	[ExecuteInEditMode]
	public class TalentArrow : MonoBehaviour {
		
		public enum ArrowDirection
		{
			Down,
			Left,
			Right,
			DownLeft,
			DownRight
		}
		
		[SerializeField] private ArrowDirection m_Direction;
		[SerializeField] private GameObject m_Down;
		[SerializeField] private GameObject m_Left;
		[SerializeField] private GameObject m_Right;
		[SerializeField] private GameObject m_DownLeft;
		[SerializeField] private GameObject m_DownRight;
		
		/// <summary>
		/// Gets or sets the direction.
		/// </summary>
		/// <value>The direction.</value>
		public ArrowDirection direction
		{
			get { return this.m_Direction; }
			set {
				this.m_Direction = value;
				this.UpdateArrows();
			}
		}
		
#if UNITY_EDITOR
		protected void OnValidate()
		{
			this.UpdateArrows();
		}
#endif

		private void UpdateArrows()
		{
			// Disable all
			foreach (Transform trans in this.transform)
				trans.gameObject.SetActive(false);
			
			// Enable the right one
			switch (this.m_Direction)
			{
			case ArrowDirection.Down:
				if (this.m_Down != null) this.m_Down.SetActive(true);
				break;
			case ArrowDirection.Left:
				if (this.m_Left != null) this.m_Left.SetActive(true);
				break;
			case ArrowDirection.Right:
				if (this.m_Right != null) this.m_Right.SetActive(true);
				break;
			case ArrowDirection.DownLeft:
				if (this.m_DownLeft != null) this.m_DownLeft.SetActive(true);
				break;
			case ArrowDirection.DownRight:
				if (this.m_DownRight != null) this.m_DownRight.SetActive(true);
				break;
			}
		}
	}
}