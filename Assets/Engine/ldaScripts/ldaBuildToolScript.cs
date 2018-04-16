using UnityEngine;
//using UnityEditor;
using System.IO;
using System.Collections;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class ldaBuildToolScript {
//public class ldaBuildToolScript : ScriptableWizard {
//
//	//一键通过右键 Player_ActionType.xml 生成 Action_{0}.bytes 文件
//	[MenuItem("Assets/__1__ldaBuildActionTypeData #4")]
//	static void ldaBuildActionTypeData()
//	{
//		string ActionTypeBin = Application.dataPath + "/Resources/Action_{0}.bytes";
//		ActionTypeBin = Application.dataPath + "/Resources/Action_{0}.bytes";
//		
//		foreach (UnityEngine.Object obj in Selection.objects) {
//			TextAsset ta = (TextAsset)obj;
//			if (ta != null) {
//				using (MemoryStream stream = new MemoryStream(ta.bytes)) {
//					XmlSerializer SerializerObj = new XmlSerializer (typeof(Data.ActionData));
//					Data.ActionData actionData = (Data.ActionData)SerializerObj.Deserialize (stream);
//					
//					// build action action.
//					actionData.BuildActionCache ();
//					
//					BinaryFormatter bf = new BinaryFormatter ();
//					foreach (Data.UnitActionInfo unitInfo in actionData.ObjectTypes) {
//						
//						string path = string.Format (ActionTypeBin, unitInfo.ID);
//						using (FileStream fs = new FileStream(path, FileMode.Create)) {
//							bf.Serialize (fs, unitInfo);
//							fs.Close ();
//						}
//					}
//					AssetDatabase.Refresh ();
//				}
//			}
//		}
//		Debug.Log ("ldaBuildActionTypeData done!");
//	}
}
