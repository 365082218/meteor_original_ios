using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;

namespace UnityEditor.UI
{
	[CanEditMultipleObjects, CustomEditor(typeof(UISelectField_List), true)]
	public class UISelectField_ListEditor : Editor {

		public override void OnInspectorGUI()
		{
		}
	}
}