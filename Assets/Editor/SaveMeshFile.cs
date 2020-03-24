//using UnityEngine;
//using System.Collections;
//using UnityEditor;
//using System.IO;
//using System.Collections.Generic;
//using System;
//using System.Text;

//public class SaveMeshFile:ScriptableWizard 
//{
//	[MenuItem("Meteor/Mesh/SaveMeshToMSXFile", false, 0)]
//	static void SaveMesh()
//	{
//		if (Selection.activeObject != null)
//		{
//			GameObject obj = Selection.activeObject as GameObject;
//			if (obj == null)
//			{
//				EditorUtility.DisplayDialog("SaveMesh", "SelectObject is not gameobject", "OK");
//				return;
//			}

//			SkinnedMeshRenderer [] mr = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
//			MeshFilter[] mf = obj.GetComponentsInChildren<MeshFilter>();
//			if (mr.Length == 0 && mf.Length == 0)
//			{
//				EditorUtility.DisplayDialog("SaveMesh", "SelectObject have none mesh", "OK");
//				return;
//			}

//			string strMsg = "Export " + Selection.activeObject.name + " to Mesh File";
//			if (EditorUtility.DisplayDialog("SaveMesh", strMsg, "OK"))
//			{
//				string strPath = EditorUtility.SaveFilePanel("SelectPath", "Assets/LevelEditor/Resources/", Selection.activeObject.name, "x");
//				if (string.IsNullOrEmpty(strPath))
//					return;
//                string strSaveDir = strPath.Substring(0, strPath.LastIndexOf('/'));
//				FileStream fs = File.Open(strPath, FileMode.OpenOrCreate);
//				fs.Seek(0, SeekOrigin.Begin);
//				foreach (var each in mr)
//				{
//					string strVertex = "Vertex{";
//					for (int i = 0; i < each.sharedMesh.vertexCount; i++)
//					{
//						strVertex += each.sharedMesh.vertices[i].x.ToString() + "," + each.sharedMesh.vertices[i].y.ToString() + "," + each.sharedMesh.vertices[i].z.ToString();
//						if (i != each.sharedMesh.vertexCount - 1)
//							strVertex += ",";
//						else
//							strVertex += "}";

//					}
//					if (each.sharedMesh.vertexCount == 0)
//						strVertex += "}";
//					byte[] buff = System.Text.Encoding.UTF8.GetBytes(strVertex);
//					fs.Write(buff, 0, buff.Length);

//					string strTriangle = "Triangle{";
//					for (int i = 0; i < each.sharedMesh.triangles.Length; i++)
//					{
//						strTriangle += each.sharedMesh.triangles[i].ToString();
//						if (i != each.sharedMesh.triangles.Length - 1)
//							strTriangle += ",";
//						else
//							strTriangle += "}";
//					}
//					if (each.sharedMesh.triangles.Length == 0)
//						strTriangle += "}";

//					byte[] bufftriangle = System.Text.Encoding.UTF8.GetBytes(strTriangle);
//					fs.Write(bufftriangle, 0, bufftriangle.Length);

//					string strUV = "UV{";
//					for (int i = 0; i < each.sharedMesh.uv.Length; i++)
//					{
//						strUV += each.sharedMesh.uv[i].x.ToString() + "," + each.sharedMesh.uv[i].y.ToString();
//						if (i != each.sharedMesh.uv.Length - 1)
//							strUV += ",";
//						else
//							strUV += "}";
//					}
//					if (each.sharedMesh.uv.Length == 0)
//						strUV += "}";

//					byte[] buffUV = System.Text.Encoding.UTF8.GetBytes(strUV);
//					fs.Write(buffUV, 0, buffUV.Length);

//					string strUV1 = "UV1{";
//					for (int i = 0; i < each.sharedMesh.uv2.Length; i++)
//					{
//						strUV1 += each.sharedMesh.uv2[i].x.ToString() + "," + each.sharedMesh.uv2[i].y.ToString();
//						if (i != each.sharedMesh.uv2.Length - 1)
//							strUV1 += ",";
//						else
//							strUV1 += "}";
//					}
//					if (each.sharedMesh.uv2.Length == 0)
//						strUV1 += "}";
//					byte[] buffUV1 = System.Text.Encoding.UTF8.GetBytes(strUV1);
//					fs.Write(buffUV1, 0, buffUV1.Length);

//					string strUV2 = "UV2{";
//					for (int i = 0; i < each.sharedMesh.uv2.Length; i++)
//					{
//						strUV2 += each.sharedMesh.uv2[i].x.ToString() + "," + each.sharedMesh.uv2[i].y.ToString();
//						if (i != each.sharedMesh.uv2.Length - 1)
//							strUV2 += ",";
//						else
//							strUV2 += "}";
//					}
//					if (each.sharedMesh.uv2.Length == 0)
//						strUV2 += "}";
//					byte[] buffUV2 = System.Text.Encoding.UTF8.GetBytes(strUV2);
//					fs.Write(buffUV2, 0, buffUV2.Length);

//					string strNormal = "Normal{";
//					for (int i = 0; i < each.sharedMesh.normals.Length; i++)
//					{
//						strNormal += each.sharedMesh.normals[i].x.ToString() + "," + each.sharedMesh.normals[i].y.ToString() + "," + each.sharedMesh.normals[i].z.ToString();
//						if (i != each.sharedMesh.normals.Length - 1)
//							strNormal += ",";
//						else
//							strNormal += "}";
//					}
//					if (each.sharedMesh.normals.Length == 0)
//						strNormal += "}";

//					byte[] buffNormal = System.Text.Encoding.UTF8.GetBytes(strNormal);
//					fs.Write(buffNormal, 0, buffNormal.Length);

//					string strMaterial = "Material{";
//					for (int i = 0; i < each.sharedMaterials.Length; i++)
//					{
//						string strTexture = AssetDatabase.GetAssetPath(each.sharedMaterials[i].mainTexture);
//						string strSourceName = strTexture.Substring(strTexture.LastIndexOf('/') + 1);
//						File.Copy(strTexture, strSaveDir + "/" +strSourceName);
//						strMaterial += strSourceName;
//						if (i != each.sharedMaterials.Length - 1)
//							strMaterial += ",";
//						else
//							strMaterial += "}";
//					}
//					if (each.sharedMaterials.Length == 0)
//						strMaterial += "}";
//					byte[] buffMat = System.Text.Encoding.UTF8.GetBytes(strMaterial);
//					fs.Write(buffMat, 0, buffMat.Length);
//					fs.Flush();
//					fs.Close();
//					break;
//				}

//				foreach (var each in mf)
//				{
//					string strVertex = "Vertex{";
//					for (int i = 0; i < each.sharedMesh.vertexCount; i++)
//					{
//						strVertex += each.sharedMesh.vertices[i].x.ToString() + "," + each.sharedMesh.vertices[i].y.ToString() + "," + each.sharedMesh.vertices[i].z.ToString();
//						if (i != each.sharedMesh.vertexCount - 1)
//							strVertex += ",";
//						else
//							strVertex += "}";
						
//					}
//					if (each.sharedMesh.vertexCount == 0)
//						strVertex += "}";

//					byte[] buff = System.Text.Encoding.UTF8.GetBytes(strVertex);
//					fs.Write(buff, 0, buff.Length);
					
//					string strTriangle = "Triangle{";
//					for (int i = 0; i < each.sharedMesh.triangles.Length; i++)
//					{
//						strTriangle += each.sharedMesh.triangles[i].ToString();
//						if (i != each.sharedMesh.triangles.Length - 1)
//							strTriangle += ",";
//						else
//							strTriangle += "}";
//					}
//					if (each.sharedMesh.triangles.Length == 0)
//						strTriangle += "}";

//					byte[] bufftriangle = System.Text.Encoding.UTF8.GetBytes(strTriangle);
//					fs.Write(bufftriangle, 0, bufftriangle.Length);
					
//					string strUV = "UV{";
//					for (int i = 0; i < each.sharedMesh.uv.Length; i++)
//					{
//						strUV += each.sharedMesh.uv[i].x.ToString() + "," + each.sharedMesh.uv[i].y.ToString();
//						if (i != each.sharedMesh.uv.Length - 1)
//							strUV += ",";
//						else
//							strUV += "}";
//					}
//					if (each.sharedMesh.uv.Length == 0)
//						strUV += "}";
//					byte[] buffUV = System.Text.Encoding.UTF8.GetBytes(strUV);
//					fs.Write(buffUV, 0, buffUV.Length);
					
//					string strUV1 = "UV1{";
//					for (int i = 0; i < each.sharedMesh.uv2.Length; i++)
//					{
//						strUV1 += each.sharedMesh.uv2[i].x.ToString() + "," + each.sharedMesh.uv2[i].y.ToString();
//						if (i != each.sharedMesh.uv2.Length - 1)
//							strUV1 += ",";
//						else
//							strUV1 += "}";
//					}
//					if (each.sharedMesh.uv2.Length == 0)
//						strUV1 += "}";

//					byte[] buffUV1 = System.Text.Encoding.UTF8.GetBytes(strUV1);
//					fs.Write(buffUV1, 0, buffUV1.Length);
					
//					string strUV2 = "UV2{";
//					for (int i = 0; i < each.sharedMesh.uv2.Length; i++)
//					{
//						strUV2 += each.sharedMesh.uv2[i].x.ToString() + "," + each.sharedMesh.uv2[i].y.ToString();
//						if (i != each.sharedMesh.uv2.Length - 1)
//							strUV2 += ",";
//						else
//							strUV2 += "}";
//					}
//					if (each.sharedMesh.uv2.Length == 0)
//						strUV2 += "}";

//					byte[] buffUV2 = System.Text.Encoding.UTF8.GetBytes(strUV2);
//					fs.Write(buffUV2, 0, buffUV2.Length);
					
//					string strNormal = "Normal{";
//					for (int i = 0; i < each.sharedMesh.normals.Length; i++)
//					{
//						strNormal += each.sharedMesh.normals[i].x.ToString() + "," + each.sharedMesh.normals[i].y.ToString() + "," + each.sharedMesh.normals[i].z.ToString();
//						if (i != each.sharedMesh.normals.Length - 1)
//							strNormal += ",";
//						else
//							strNormal += "}";
//					}
//					if (each.sharedMesh.normals.Length == 0)
//						strNormal += "}";

//					byte[] buffNormal = System.Text.Encoding.UTF8.GetBytes(strNormal);
//					fs.Write(buffNormal, 0, buffNormal.Length);

//					string strMaterial = "Material{";

//					for (int i = 0; i < each.gameObject.GetComponent<Renderer>().sharedMaterials.Length; i++)
//					{
//						string strTexture = AssetDatabase.GetAssetPath(each.gameObject.GetComponent<Renderer>().sharedMaterials[i].mainTexture);
//						string strSourceName = strTexture.Substring(strTexture.LastIndexOf('/') + 1);
//						File.Copy(strTexture, strSaveDir + "/" +strSourceName);
//						strMaterial += strSourceName;
//						if (i != each.gameObject.GetComponent<Renderer>().sharedMaterials.Length - 1)
//							strMaterial += ",";
//						else
//							strMaterial += "}";
//					}
//					if (each.gameObject.GetComponent<Renderer>().sharedMaterials.Length == 0)
//						strMaterial += "}";
//					byte[] buffMat = System.Text.Encoding.UTF8.GetBytes(strMaterial);
//					fs.Write(buffMat, 0, buffMat.Length);

//					fs.Flush();
//					fs.Close();
//					break;
//				}
//			}
//		}
//	}
//}


//public class ObjExporter
//{

//    public static string MeshToString(MeshFilter mf)
//    {
//        Mesh m = mf.mesh;
//        Material[] mats = mf.GetComponent<Renderer>().sharedMaterials;

//        StringBuilder sb = new StringBuilder();

//        sb.Append("g ").Append(mf.name).Append("\n");
//        foreach (Vector3 v in m.vertices)
//        {
//            sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
//        }
//        sb.Append("\n");
//        foreach (Vector3 v in m.normals)
//        {
//            sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z));
//        }
//        sb.Append("\n");
//        foreach (Vector3 v in m.uv)
//        {
//            sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
//        }
//        for (int material = 0; material < m.subMeshCount; material++)
//        {
//            sb.Append("\n");
//            sb.Append("usemtl ").Append(mats[material].name).Append("\n");
//            sb.Append("usemap ").Append(mats[material].name).Append("\n");

//            int[] triangles = m.GetTriangles(material);
//            for (int i = 0; i < triangles.Length; i += 3)
//            {
//                sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
//                    triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1));
//            }
//        }
//        return sb.ToString();
//    }

//    public static void MeshToFile(MeshFilter mf, string filename)
//    {
//        using (StreamWriter sw = new StreamWriter(filename))
//        {
//            sw.Write(MeshToString(mf));
//        }
//    }
//}
///*
//Based on ObjExporter.cs, this "wrapper" lets you export to .OBJ directly from the editor menu.

//This should be put in your "Editor"-folder. Use by selecting the objects you want to export, and select
//the appropriate menu item from "Custom->Export". Exported models are put in a folder called
//"ExportedObj" in the root of your Unity-project. Textures should also be copied and placed in the
//same folder. */

//internal struct ObjMaterial
//{
//    public string name;
//    public string textureName;
//}

//internal class WorldtoLocalParam
//{
//    public Vector3 vec;
//    public Quaternion rotation;
//}

//public class EditorObjExporter : ScriptableObject
//{
//    private static int vertexOffset = 0;
//    private static int normalOffset = 0;
//    private static int uvOffset = 0;


//    //User should probably be able to change this. It is currently left as an excercise for
//    //the reader.
//    private static string targetFolder = "ExportedObj";


//    private static string MeshToString(MeshFilter mf, Dictionary<string, ObjMaterial> materialList, WorldtoLocalParam param = null)
//    {
//        Mesh m = mf.sharedMesh;
//        Material[] mats = mf.GetComponent<Renderer>().sharedMaterials;

//        StringBuilder sb = new StringBuilder();

//        sb.Append("g ").Append(mf.name).Append("\n");
//        foreach (Vector3 lv in m.vertices)
//        {
//            Vector3 vec = lv;
//            //通过参数，可以还原原流星里的任意的 模型。
//            if (param != null)
//            {
//                vec = vec + param.vec;
//                vec = param.rotation * vec;
//            }
//            //Vector3 wv = mf.transform.TransformPoint(lv);
//            Vector3 wv = vec;
//            //This is sort of ugly - inverting x-component since we're in
//            //a different coordinate system than "everyone" is "used to".
//            sb.Append(string.Format("v {0} {1} {2}\n", -wv.x, wv.y, wv.z));
//        }
//        sb.Append("\n");

//        foreach (Vector3 lv in m.normals)
//        {
//            Vector3 wv = mf.transform.TransformDirection(lv);
//            //Vector3 wv = lv;
//            sb.Append(string.Format("vn {0} {1} {2}\n", -wv.x, wv.y, wv.z));
//        }
//        sb.Append("\n");

//        foreach (Vector3 v in m.uv)
//        {
//            sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
//        }

//        for (int material = 0; material < m.subMeshCount; material++)
//        {
//            sb.Append("\n");


//            //See if this material is already in the materiallist.
//            try
//            {
//                if (material >= mats.Length)
//                    continue;
//                if (mats[material] == null)
//                    continue;
//                ObjMaterial objMaterial = new ObjMaterial();
//                if (mats[material].mainTexture)
//                    objMaterial.textureName = AssetDatabase.GetAssetPath(mats[material].mainTexture);
//                else
//                    objMaterial.textureName = null;
//                string[] s = null;
//                string ss = null;
//                if (!string.IsNullOrEmpty(objMaterial.textureName))
//                {
//                    s = objMaterial.textureName.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);
//                    if (s != null && s.Length != 0)
//                        ss = s[s.Length - 1];
//                }
//                objMaterial.name = mats[material].name + "_" + ss;
//                if (!materialList.ContainsKey(objMaterial.name))
//                    materialList.Add(objMaterial.name, objMaterial);
//                sb.Append("usemtl ").Append(objMaterial.name).Append("\n");
//                sb.Append("usemap ").Append(objMaterial.name).Append("\n");
//            }
//            catch (ArgumentException)
//            {
//                //Already in the dictionary
//            }


//            int[] triangles = m.GetTriangles(material);
//            for (int i = 0; i < triangles.Length; i += 3)
//            {
//                //Because we inverted the x-component, we also needed to alter the triangle winding.
//                sb.Append(string.Format("f {1}/{1}/{1} {0}/{0}/{0} {2}/{2}/{2}\n",
//                    triangles[i] + 1 + vertexOffset, triangles[i + 1] + 1 + normalOffset, triangles[i + 2] + 1 + uvOffset));
//            }
//        }

//        vertexOffset += m.vertices.Length;
//        normalOffset += m.normals.Length;
//        uvOffset += m.uv.Length;

//        return sb.ToString();
//    }

//    private static void Clear()
//    {
//        vertexOffset = 0;
//        normalOffset = 0;
//        uvOffset = 0;
//    }

//    private static Dictionary<string, ObjMaterial> PrepareFileWrite()
//    {
//        Clear();

//        return new Dictionary<string, ObjMaterial>();
//    }

//    private static void MaterialsToFile(Dictionary<string, ObjMaterial> materialList, string folder, string filename)
//    {
//        using (StreamWriter sw = new StreamWriter(folder + "/" + filename + ".mtl"))
//        {
//            foreach (KeyValuePair<string, ObjMaterial> kvp in materialList)
//            {
//                sw.Write("\n");
//                sw.Write("newmtl {0}\n", kvp.Key);
//                sw.Write("Ka  0.6 0.6 0.6\n");
//                sw.Write("Kd  0.6 0.6 0.6\n");
//                sw.Write("Ks  0.9 0.9 0.9\n");
//                sw.Write("d  1.0\n");
//                sw.Write("Ns  0.0\n");
//                sw.Write("illum 2\n");

//                if (kvp.Value.textureName != null)
//                {
//                    string destinationFile = kvp.Value.textureName;


//                    int stripIndex = destinationFile.LastIndexOf('/');//FIXME: Should be Path.PathSeparator;

//                    if (stripIndex >= 0)
//                        destinationFile = destinationFile.Substring(stripIndex + 1).Trim();


//                    string relativeFile = destinationFile;

//                    destinationFile = folder + "/" + destinationFile;

//                    Debug.Log("Copying texture from " + kvp.Value.textureName + " to " + destinationFile);

//                    try
//                    {
//                        //Copy the source file
//                        File.Copy(kvp.Value.textureName, destinationFile);
//                    }
//                    catch
//                    {

//                    }


//                    sw.Write("map_Kd {0}", relativeFile);
//                }

//                sw.Write("\n\n\n");
//            }
//        }
//    }

//    public static void MeshToFile(MeshFilter mf, string folder, string filename)
//    {
//        Dictionary<string, ObjMaterial> materialList = PrepareFileWrite();

//        using (StreamWriter sw = new StreamWriter(folder + "/" + filename + ".obj"))
//        {
//            sw.Write("mtllib ./" + filename + ".mtl\n");

//            sw.Write(MeshToString(mf, materialList));
//        }

//        MaterialsToFile(materialList, folder, filename);
//    }

//    private static void MeshesToFile(MeshFilter[] mf, string folder, string filename, WorldtoLocalParam param = null)
//    {
//        Dictionary<string, ObjMaterial> materialList = PrepareFileWrite();

//        using (StreamWriter sw = new StreamWriter(folder + "/" + filename + ".obj"))
//        {
//            sw.Write("mtllib ./" + filename + ".mtl\n");

//            for (int i = 0; i < mf.Length; i++)
//            {
//                sw.Write(MeshToString(mf[i], materialList, param));
//            }
//        }

//        MaterialsToFile(materialList, folder, filename);
//    }

//    private static bool CreateTargetFolder()
//    {
//        try
//        {
//            System.IO.Directory.CreateDirectory(targetFolder);
//        }
//        catch
//        {
//            EditorUtility.DisplayDialog("Error!", "Failed to create target folder!", "");
//            return false;
//        }

//        return true;
//    }

//    [MenuItem("Meteor/Mesh/Export/Export all MeshFilters in selection to separate OBJs")]
//    static void ExportSelectionToSeparate()
//    {
//        if (!CreateTargetFolder())
//            return;

//        Transform[] selection = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);

//        if (selection.Length == 0)
//        {
//            EditorUtility.DisplayDialog("No source object selected!", "Please select one or more target objects", "");
//            return;
//        }

//        int exportedObjects = 0;

//        for (int i = 0; i < selection.Length; i++)
//        {
//            Component[] meshfilter = selection[i].GetComponentsInChildren(typeof(MeshFilter));

//            for (int m = 0; m < meshfilter.Length; m++)
//            {
//                exportedObjects++;
//                MeshToFile((MeshFilter)meshfilter[m], targetFolder, selection[i].name + "_" + i + "_" + m);
//            }
//        }

//        if (exportedObjects > 0)
//            EditorUtility.DisplayDialog("Objects exported", "Exported " + exportedObjects + " objects", "");
//        else
//            EditorUtility.DisplayDialog("Objects not exported", "Make sure at least some of your selected objects have mesh filters!", "");
//    }

//    [MenuItem("Meteor/Mesh/Export/Export whole selection to single OBJ")]
//    static void ExportWholeSelectionToSingle()
//    {
//        if (!CreateTargetFolder())
//            return;


//        Transform[] selection = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);

//        if (selection.Length == 0)
//        {
//            EditorUtility.DisplayDialog("No source object selected!", "Please select one or more target objects", "");
//            return;
//        }

//        int exportedObjects = 0;

//        ArrayList mfList = new ArrayList();

//        for (int i = 0; i < selection.Length; i++)
//        {
//            Component[] meshfilter = selection[i].GetComponentsInChildren(typeof(MeshFilter));

//            for (int m = 0; m < meshfilter.Length; m++)
//            {
//                exportedObjects++;
//                mfList.Add(meshfilter[m]);
//            }
//        }

//        if (exportedObjects > 0)
//        {
//            MeshFilter[] mf = new MeshFilter[mfList.Count];

//            for (int i = 0; i < mfList.Count; i++)
//            {
//                mf[i] = (MeshFilter)mfList[i];
//            }

//            string filename = EditorApplication.currentScene + "_" + exportedObjects;

//            int stripIndex = filename.LastIndexOf('/');//FIXME: Should be Path.PathSeparator

//            if (stripIndex >= 0)
//                filename = filename.Substring(stripIndex + 1).Trim();

            
//            MeshesToFile(mf, targetFolder, filename);


//            EditorUtility.DisplayDialog("Objects exported", "Exported " + exportedObjects + " objects to " + filename, "");
//        }
//        else
//            EditorUtility.DisplayDialog("Objects not exported", "Make sure at least some of your selected objects have mesh filters!", "");
//    }



//    [MenuItem("Meteor/Mesh/Export/Export each selected to single OBJ")]
//    static void ExportEachSelectionToSingle()
//    {
//        if (!CreateTargetFolder())
//            return;

//        Transform[] selection = Selection.GetTransforms(SelectionMode.Editable | SelectionMode.ExcludePrefab);

//        if (selection.Length == 0)
//        {
//            EditorUtility.DisplayDialog("No source object selected!", "Please select one or more target objects", "");
//            return;
//        }

//        int exportedObjects = 0;


//        for (int i = 0; i < selection.Length; i++)
//        {
//            Component[] meshfilter = selection[i].GetComponentsInChildren(typeof(MeshFilter));

//            MeshFilter[] mf = new MeshFilter[meshfilter.Length];

//            for (int m = 0; m < meshfilter.Length; m++)
//            {
//                exportedObjects++;
//                mf[m] = (MeshFilter)meshfilter[m];
//            }

//            MeshesToFile(mf, targetFolder, selection[i].name + "_" + i);
//        }

//        if (exportedObjects > 0)
//        {
//            EditorUtility.DisplayDialog("Objects exported", "Exported " + exportedObjects + " objects", "");
//        }
//        else
//            EditorUtility.DisplayDialog("Objects not exported", "Make sure at least some of your selected objects have mesh filters!", "");
//    }
//}