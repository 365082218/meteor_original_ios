using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
	[AddComponentMenu("UI/Raycast Filters/UIPolygon Raycast Filter")]
	public class UIPolygonRaycast : MonoBehaviour, ICanvasRaycastFilter 
	{
        private PolygonCollider2D _polygon = null;
        private PolygonCollider2D polygon
        {
            get
            {
                if (_polygon == null)
                    _polygon = GetComponent<PolygonCollider2D>();
                return _polygon;
            }
        }
        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
		{
            return polygon.OverlapPoint(eventCamera.ScreenToWorldPoint(sp));
        }
	}
}
