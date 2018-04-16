using UnityEngine;
//using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ldaExportResScript {
//public class ldaExportResScript : ScriptableWizard {
//	/**
//	导出素材，AssetBundle打包Prefab资源，直接右键选取Prefab生成打包软件即可
//	*/
//	/** 导出的场景后缀 */
//	private static string strUnity3d = "unity3d";
//	/** 场景后缀 */
//	private static string strExport = "导出";
//	[MenuItem("Assets/ldaExport/ExportResource #2")]
//	static void ExportResource()
//	{
//		if(Selection.activeObject){
//			string srcName = Selection.activeObject.name;
//			string path = EditorUtility.SaveFilePanel(strExport,"./Assets/ldaActionData/ExAssetBundle/",srcName,strUnity3d);
//			if(path.Length != 0){
//				BuildPipeline.BuildAssetBundle(Selection.activeObject, 
//				                               null, path, 
//				                               BuildAssetBundleOptions.CollectDependencies|
//				                               BuildAssetBundleOptions.UncompressedAssetBundle
//				                               );
//			}
//		}else{
//			Debug.Log("Project中选中要导出的物体.");
//		}
//	}
}
