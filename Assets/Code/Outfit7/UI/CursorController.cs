using UnityEngine;
using System.Collections;

namespace Outfit7.UI {
    public class CursorController : MonoBehaviour {

#if UNITY_WP8
    public Texture2D DefaultMouseCursor = null;

	void Start () {
        Cursor.SetCursor(DefaultMouseCursor, Vector2.zero, CursorMode.Auto);
    }
#endif
    }
}
