using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;

public class UIItemDatabaseEditor
{
	private static string GetSavePath()
	{
		return EditorUtility.SaveFilePanelInProject("New item database", "New item database", "asset", "Create a new item database.");
	}
	
	//[MenuItem("Assets/Create/Databases/Item Database")]
	//public static void CreateDatabase()
	//{
	//	string assetPath = GetSavePath();
	//	UIItemDatabase asset = ScriptableObject.CreateInstance("UIItemDatabase") as UIItemDatabase;  //scriptable object
	//	AssetDatabase.CreateAsset(asset, AssetDatabase.GenerateUniqueAssetPath(assetPath));
	//	AssetDatabase.Refresh();
	//}
}