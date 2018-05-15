using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public class VersionWizard : ScriptableWizard {
	public BuildTarget Target;
	public string Major = "0";
	public string Minor = "0";
	public string Build = "0";
	public string Revision = "0";
    public string Notices = "";//更新提示.
	public string []strHistoryVer;

	[MenuItem("VersionManager/1.VersionWizard", false, 0)]
	static void CreateWizard()
	{
		ScriptableWizard.DisplayWizard<VersionWizard>("CreateVersion", "New Version");
	}

	void OnWizardCreate()
	{
		string strTarget = System.Enum.GetName(typeof(BuildTarget), Target);
		if (strTarget == null || strTarget == "")
			return;

		if (strHistoryVer != null && strHistoryVer.Length >= 1)
		{
			if (!CompareVersion(Major + "." + Minor + "." + Build + "." + Revision, strHistoryVer[strHistoryVer.Length - 1]))
			{
				EditorUtility.DisplayDialog("Error", "Target Version Invalid", "Quit");
				CreateWizard();
				return;
			}
		}

		VersionManager.CreateNewVersion(Target, Major, Minor, Build, Revision, Notices);
	}

	void OnWizardOtherButton()
	{

	}

	void OnWizardUpdate()
	{
		string strTarget = System.Enum.GetName(typeof(BuildTarget), Target);
		if (strTarget == null || strTarget == "")
			return;
		List<string> list = VersionManager.GetAllVersion(Target);
		strHistoryVer = new string[list.Count];
		int index = 0;
		foreach (string str in list)
		{
			strHistoryVer[index++] = str;
		} 
	}

	bool CompareVersion(string strNew, string strOld)
	{
		string []strNewGroup = strNew.Split(new char[]{'.','.','.'}, 4);
		string []strOldGroup = strOld.Split(new char[]{'.','.','.'}, 4);
		if (strNewGroup.Length < 4 || strOldGroup.Length < 4)
			return false;

		for (int i = 0; i < 4; ++i)
		{
			if (int.Parse(strNewGroup[i]) != int.Parse(strOldGroup[i]))
			{
				if (int.Parse(strNewGroup[i]) < int.Parse(strOldGroup[i]))
				    return false;
				else
					return true;
			}
		}
		return true;
	}

	public static List<string>ReadBlackFile(string strFileName, BuildTarget Target)
	{
		List<string>strRet = new List<string>();
		string strPath = PlatformMap.GetPlatformPath(Target) + "/" + strFileName;
		
		XmlDocument xmlIgnore = new XmlDocument();
		try
		{
			xmlIgnore.Load(strPath);
			XmlElement files = xmlIgnore.DocumentElement;
			foreach (XmlElement file in files)
			{
				string strfile = file.GetAttribute("name");
				if (strfile != null && strfile.Length != 0)
					strRet.Add(strfile);
			}
		}
		catch
		{
			return null;
		}
		return strRet;
	}
}

public class PackageWizard : ScriptableWizard {
	public BuildTarget Target;
	public string []IgnorePrefab;
	public Object PrefabDirectory = null;
 	public string strTip = "IgnoreFile: IgnorePrefab.xml";
	[MenuItem("VersionManager/2.Prefab PackageWizard", false, 1)]
	static void CreateWizard()
	{
		ScriptableWizard.DisplayWizard<PackageWizard>("Prefab PackageWizard", "Package Prefab");
	}

	void OnWizardCreate()
	{
		string strTarget = System.Enum.GetName(typeof(BuildTarget), Target);
		if (strTarget == null || strTarget == "")
		{
			CreateWizard();
			return;
		}

		try
		{
			List<string> filelist = null;
			if (IgnorePrefab != null && IgnorePrefab.Length != 0)
			{
				filelist = new List<string>();
				foreach (string str in IgnorePrefab)
				{
					filelist.Add(str);
				}
			}

            if (!CreateAssetBundle.PackageAllPrefab(Target, filelist, PrefabDirectory))
			{
				CreateWizard();
				return;
			}
		}
		catch (System.Exception exp)
		{
			Debug.Log(exp.Message);
			CreateWizard();
			return;
		}
	}

	void OnWizardUpdate()
	{
		string strTarget = System.Enum.GetName(typeof(BuildTarget), Target);
		if (strTarget == null || strTarget == "")
		{
			return;
		}

		if (System.IO.File.Exists(PlatformMap.GetPlatformPath(Target) + "/IgnorePrefab.xml"))
		{
			strTip = "Load IGnoreFile : " + PlatformMap.GetPlatformPath(Target) + "/IgnorePrefab.xml" + "  Successed";

			List<string>filelist = VersionWizard.ReadBlackFile("IgnorePrefab.xml", Target);
			if (filelist != null && filelist.Count != 0)
			{
				IgnorePrefab = new string[filelist.Count];
				int nIndex = 0;
				foreach (string str in filelist)
				{
					IgnorePrefab[nIndex++] = str;
				}
			}
		}
		else
		{
			IgnorePrefab = null;
			strTip = "Load IGnoreFile Failed: file not found";
		}
	}
}



public class PackageSceneWizard : ScriptableWizard {
	public BuildTarget Target;
	public string []IgnorePrefab;
	public Object SceneDirectory;
	public string strTip = "IgnoreFile: IgnoreScene.xml";
	[MenuItem("VersionManager/3.Scene PackageWizard", false, 2)]
	static void CreateWizard()
	{
		ScriptableWizard.DisplayWizard<PackageSceneWizard>("Scene PackageWizard", "Package Scene");
	}
	
	void OnWizardCreate()
	{
		string strTarget = System.Enum.GetName(typeof(BuildTarget), Target);
		if (strTarget == null || strTarget == "")
		{
			CreateWizard();
			return;
		}
		
		if (SceneDirectory == null)
		{
			EditorUtility.DisplayDialog("Error", "SceneDirectory Is Empty", "QUIT");
			CreateWizard();
			return;
		}
		
		try
		{
			List<string> filelist = null;
			if (IgnorePrefab != null && IgnorePrefab.Length != 0)
			{
				filelist = new List<string>();
				foreach (string str in IgnorePrefab)
				{
					filelist.Add(str);
				}
			}
			
			if (!CreateSceneBundles.Execute(Target, filelist, SceneDirectory))
			{
				CreateWizard();
				return;
			}
		}
		catch (System.Exception exp)
		{
			Debug.Log(exp.Message);
			CreateWizard();
			return;
		}
	}

	void OnWizardUpdate()
	{
		string strTarget = System.Enum.GetName(typeof(BuildTarget), Target);
		if (strTarget == null || strTarget == "")
		{
			return;
		}
		
		if (System.IO.File.Exists(PlatformMap.GetPlatformPath(Target) + "/IgnoreScene.xml"))
		{
			strTip = "Load IGnoreFile : " + PlatformMap.GetPlatformPath(Target) + "/IgnoreScene.xml" + "  Successed";
			
			List<string>filelist = VersionWizard.ReadBlackFile("IgnoreScene.xml", Target);
			if (filelist != null && filelist.Count != 0)
			{
				IgnorePrefab = new string[filelist.Count];
				int nIndex = 0;
				foreach (string str in filelist)
				{
					IgnorePrefab[nIndex++] = str;
				}
			}
		}
		else
		{
			IgnorePrefab = null;
			strTip = "Load IGnoreFile Failed: file not found";
		}
	}
}

public class PackageFileWizard : ScriptableWizard {
	public string[] strExt = new string[]{"*.txt"};
	public BuildTarget Target;
	public bool bEntrypt = true;
	public string []IgnoreFile;
	public Object FileDirectory;
	public string strTip = "IgnoreFile: IgnoreFile.xml";
	[MenuItem("VersionManager/4.CommonFile PackageWizard", false, 3)]
	static void CreateWizard()
	{
		ScriptableWizard.DisplayWizard<PackageFileWizard>("PackageFileWizard", "Package CommonFile");
	}
	
	void OnWizardCreate()
	{
		string strTarget = System.Enum.GetName(typeof(BuildTarget), Target);
		if (strTarget == null || strTarget == "")
		{
			CreateWizard();
			return;
		}
		
		if (FileDirectory == null)
		{
			EditorUtility.DisplayDialog("Error", "SceneDirectory Is Empty", "QUIT");
			CreateWizard();
			return;
		}
		
		try
		{
			List<string> filelist = null;
			if (IgnoreFile != null && IgnoreFile.Length != 0)
			{
				filelist = new List<string>();
				foreach (string str in IgnoreFile)
				{
					filelist.Add(str);
				}
			}
			List<string> strCompress = new List<string>();

			if (bEntrypt)
			{
				if (!EncryptXOR.EncryptResource(Target, filelist, FileDirectory, strExt, ref strCompress))
				{
					CreateWizard();
					return;
				}
			}
			else
			{
				string SavePath = PlatformMap.GetPlatformPath(Target) + "/" + VersionManager.GetCurVersion(Target)+ "/";
				string SelectPath = AssetDatabase.GetAssetPath(FileDirectory);
				foreach (string st in strExt)
				{
					string []files = System.IO.Directory.GetFiles(SelectPath, st, System.IO.SearchOption.AllDirectories);
					foreach (string stPush in files)
					{
						if (!strCompress.Contains(stPush))
							strCompress.Add(stPush);
					}
				}
			}
			//if encrypted delete original file
			CompressForFile.CompressScript(Target, filelist, strCompress, bEntrypt);

			//write force updated
			string ForceTxtSavePath = "";
			try
			{
				ForceTxtSavePath = PlatformMap.GetPlatformPath(Target) + "/" + VersionManager.GetCurVersion(Target)+ "/";
			}
			catch(IOException exp)
			{
				EditorUtility.DisplayDialog("Error", exp.Message, "OK");
				return;
			}

			FileStream fsForce = File.Open(ForceTxtSavePath + "ForceUpdate.txt", FileMode.OpenOrCreate);
			fsForce.Seek(0, SeekOrigin.End);
			int i = 0;
			foreach (string file in strCompress)
			{
				string path = file;
				if (path.EndsWith(".XOR"))
				{
					path = path.Substring(0, path.Length - 4);
				}

				path = path.Replace("\\", "/");
				path += ".zip";
				byte[] buffer = System.Text.Encoding.UTF8.GetBytes(path);
				fsForce.Write(buffer, 0, buffer.Length);
				fsForce.Write(new byte[]{(byte)'\r', (byte)'\n'}, 0, 2);
			}
			fsForce.Flush();
			fsForce.Close();
		}
		catch (System.Exception exp)
		{
			Debug.Log(exp.Message);
			CreateWizard();
			return;
		}
	}
	
	void OnWizardUpdate()
	{
		string strTarget = System.Enum.GetName(typeof(BuildTarget), Target);
		if (strTarget == null || strTarget == "")
			return;
		
		if (System.IO.File.Exists(PlatformMap.GetPlatformPath(Target) + "/IgnoreFile.xml"))
		{
			strTip = "Load IGnoreFile : " + PlatformMap.GetPlatformPath(Target) + "/IgnoreFile.xml" + "  Successed";
			
			List<string>filelist = VersionWizard.ReadBlackFile("IgnoreFile.xml", Target);
			if (filelist != null && filelist.Count != 0)
			{
				IgnoreFile = new string[filelist.Count];
				int nIndex = 0;
				foreach (string str in filelist)
				{
					IgnoreFile[nIndex++] = str;
				}
			}
		}
		else
		{
			IgnoreFile = null;
			strTip = "Load IGnoreFile Failed: file not found";
		}
	}
}

public class EncryptFileWizard : ScriptableWizard 
{
	public bool bEncrypt = true;
	public Object source;
	[MenuItem("VersionManager/8.EncryptTest", false, 7)]
	static void CreateWizard()
	{
		ScriptableWizard.DisplayWizard<EncryptFileWizard>("EncryptTest", "Encrypt CommonFile");
	}
	
	void OnWizardCreate()
	{
		if (source == null)
			CreateWizard();

		string strSource = AssetDatabase.GetAssetPath(source);
		string strTarget = strSource + ".XOR";
		try
		{
			FileStream fs = File.Open(strSource, FileMode.Open);
			byte []mem = new byte[fs.Length];
			fs.Read(mem, 0, (int)fs.Length);
			fs.Close();
			EncryptXOR.EncryptXorString(ref mem);
			FileStream fsWrite = File.Create(strTarget);
			fsWrite.Write(mem, 0, mem.Length);
			fsWrite.Close();
		}
		catch (System.Exception exp)
		{
			Debug.Log(exp.Message);
		}
		AssetDatabase.Refresh();
		EditorUtility.DisplayDialog("Tip", "file : " + strTarget + "Created", "OK");
	}
}

public class FileListWizard : ScriptableWizard 
{
	public BuildTarget target;
	[MenuItem("VersionManager/5.FileListWizard", false, 4)]
	static void CreateWizard()
	{
		ScriptableWizard.DisplayWizard<FileListWizard>("GenFileList", "Gen Filelist");
	}
	
	void OnWizardCreate()
	{
		string strTarget = System.Enum.GetName(typeof(BuildTarget), target);
		if (strTarget == null || strTarget == "")
			return;
		CreateMD5List.Execute(target);
	}
}

public class VersionDiffWizard : ScriptableWizard 
{
	public BuildTarget target;
	public string strLastestVer;
	public string []historyVer;
	[MenuItem("VersionManager/6.VersionDiffWizard", false, 5)]
	static void CreateWizard()
	{
		ScriptableWizard.DisplayWizard<VersionDiffWizard>("Gen UpdateXml By Diff All History Version", "Gen UpdateXml");
	}
	
	void OnWizardCreate()
	{
		string strTarget = System.Enum.GetName(typeof(BuildTarget), target);
		if (strTarget == null || strTarget == "")
			return;
		GenAllUpdateVersionXml.Execute(target);
	}

	void OnWizardUpdate()
	{
		strLastestVer = VersionManager.GetCurVersion(target);
		List<string> str = VersionManager.GetHistoryVersion(target);
		if (str != null && str.Count != 0)
		{
			historyVer = new string[str.Count];
			for (int i = 0; i < str.Count; ++i)
				historyVer[i] = str[i];
		}
	}
}


public class IGnoreFileWizard : ScriptableWizard {
	
	public Object[] IgnoreObjects;
	public BuildTarget Target;
	public bool bGenIgnorePrefabxml;
	public bool bGenIgnoreScenexml;
	public bool bGenIgnoreFilexml;
	[MenuItem("VersionManager/7.IGnoreWizard", false, 6)]
	static void CreateWizard()
	{
		ScriptableWizard.DisplayWizard<IGnoreFileWizard>("IGnoreWizard", "GenIGnoreXml");
	}
	
	void OnWizardCreate()
	{
		string strTarget = System.Enum.GetName(typeof(BuildTarget), Target);
		if (strTarget == null || strTarget == "")
			return;
		
		if (!(bGenIgnorePrefabxml || bGenIgnoreScenexml || bGenIgnoreFilexml))
			return;
		
		XmlDocument xml = new XmlDocument();
		XmlElement root = xml.CreateElement("files");
		xml.AppendChild(root);
		List<Object> ObjectList = new List<Object>();
		foreach (Object obj in IgnoreObjects)
		{
			if (ObjectList.Contains(obj))
				continue;
			XmlElement node = xml.CreateElement("file");
			node.SetAttribute("name", obj.name);
			root.AppendChild(node);
			ObjectList.Add(obj);
		}
		if (bGenIgnorePrefabxml)
			xml.Save(PlatformMap.GetPlatformPath(Target) + "/IgnorePrefab.xml");
		if (bGenIgnoreScenexml)
			xml.Save(PlatformMap.GetPlatformPath(Target) + "/IgnoreScene.xml");
		if (bGenIgnoreFilexml)
			xml.Save(PlatformMap.GetPlatformPath(Target) + "/IgnoreFile.xml");
	}
}

public class CleanUPRes:ScriptableWizard
{
	//which is in DeleteSceneList
	public static Dictionary<string, bool> RemoveScene = new Dictionary<string, bool>();
	//which is not in ResourceDir
	static Dictionary<string, bool> ReferenceRes = new Dictionary<string, bool>();
	static Dictionary<string, string> LeftReason = new Dictionary<string, string>();
	//which is in ResourceDir
	static List<string> ResourceFile = new List<string>();
	//delete all res exclude resource and Other Scene which Was Added in ProjectSetting
	//[MenuItem("VersionManager/11.CleanupProject", false, 11)]
	static void CreateWizard()
	{
		RemoveScene.Clear();
		ScriptableWizard.DisplayWizard<CleanUPRes>("CleanUPRes", "Clean");
		string []all = Directory.GetFiles("Assets/", "*.unity", SearchOption.AllDirectories);
		foreach (var s in all)
		{
			RemoveScene.Add(s, false);
		}
	}


	void OnWizardCreate()
	{
		string [] allres = Directory.GetFiles("Assets/", "*.*", SearchOption.AllDirectories);
		foreach (var each in allres)
		{
			if (each.StartsWith("Assets/Atlas/Atla/") || each.StartsWith("Assets/StreamingAssets/"))
				continue;
			string [] dir = each.Split('/');
			bool bIgnore = false;
			foreach (var eachDir in dir)
			{
				string strDir = eachDir.ToLower();
				//icon
				if (strDir == ".svn" 
				    || strDir == ".ds_store" 
				    || strDir.EndsWith(".meta") 
				    || strDir.EndsWith(".js") 
				    || strDir.EndsWith(".cs") 
				    || strDir.EndsWith(".h") 
				    || strDir.EndsWith(".dll")
				    || strDir.EndsWith(".mm")
				    || strDir.EndsWith(".cpp")
				    || eachDir == "BuildReport"
				    || eachDir == "Plugins"
				    || strDir.EndsWith(".shader"))
				{
					bIgnore = true;
					break;
				}
			}
			if (bIgnore)
				continue;
			bool bResource = false;
			foreach (var eachDir in dir)
			{
				string strDir = eachDir.ToLower();
				if (strDir == "resources")
				{
					bResource = true;
					break;
				}
			}
			if (bResource)
			{
				if (!ResourceFile.Contains(each))
					ResourceFile.Add(each);
			}
			else
			{
				if (!ReferenceRes.ContainsKey(each))
					ReferenceRes.Add(each, true);
			}
		}

		foreach (var each in ResourceFile)
		{
			string[] sRef = AssetDatabase.GetDependencies(new string[]{each});
			foreach (var eachRef in sRef)
			{
				if (ReferenceRes.ContainsKey(eachRef))
				{
					if (!LeftReason.ContainsKey(eachRef))
						LeftReason.Add(eachRef, " used by " + each + "\r\n");
					else
					{
						string str = LeftReason[eachRef];
						str += (" also used by " + each + "\r\n");
						LeftReason[eachRef] = str;
					}
					ReferenceRes[eachRef] = false;
				}
			}
		}

		List<string> dontremove = new List<string>();
		string strTip = "";
		foreach (var s in RemoveScene)
		{
			if (s.Value)
			{
				strTip += s.Key + "\r\n";
			}
			else
			{
				dontremove.Add(s.Key);
			}
		}

		if (0 != EditorUtility.DisplayDialogComplex("RemoveTip", strTip, "Ok", "Cancel", ""))
		{
			return;
		}

		string strLogPath = EditorUtility.SaveFilePanel("SaveFileList", "/Data", "ClientBak", "txt");
		while (string.IsNullOrEmpty(strLogPath))
		{
			strLogPath = EditorUtility.SaveFilePanel("SaveFileList", "/Data", "ClientBak", "txt");
		}
		FileStream fsLog = File.Open(strLogPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
		foreach (var s in dontremove)
		{
			if (ReferenceRes.ContainsKey(s))
				ReferenceRes[s] = false;
			string[] sRef = AssetDatabase.GetDependencies(new string[]{s});
			foreach (var sItem in sRef)
			{
				if (!LeftReason.ContainsKey(sItem))
					LeftReason.Add(sItem, ":used by " + s + "\r\n");
				else
				{
					string str = LeftReason[sItem];
					str += (" also used by " + s + "\r\n");
					LeftReason[sItem] = str;
				}

				if (ReferenceRes.ContainsKey(sItem))
					ReferenceRes[sItem] = false;
			}
		}

		foreach (var s in ReferenceRes)
		{
			if (s.Value)
			{
				byte[] content = System.Text.Encoding.UTF8.GetBytes(s.Key + " " + "Was Deleted\r\n");
				fsLog.Write(content, 0, content.Length);
				File.Delete(s.Key);
			}
			else
			{
				System.IO.FileInfo fInfo = new System.IO.FileInfo(s.Key);
				byte[] content = System.Text.Encoding.UTF8.GetBytes(s.Key + " " + "Left" + fInfo.Length.ToString() + " byte" + "\r\n");
                fsLog.Write(content, 0, content.Length);
			}
		}

        fsLog.Flush();
        fsLog.Close();

		fsLog = File.Open(strLogPath + "_NotDelete.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
		foreach (var s in LeftReason)
		{
			byte[] content = System.Text.Encoding.UTF8.GetBytes(s.Key + ":" + s.Value + "\r\n");
            fsLog.Write(content, 0, content.Length);
		}
        fsLog.Flush();
        fsLog.Close();
	}

	Vector2 vecPos = new Vector2(0, 0);
	void OnGUI()
	{
		Dictionary<string, bool> tmp = new Dictionary<string, bool>();
		vecPos = GUILayout.BeginScrollView(vecPos,  GUILayout.Width(600), GUILayout.Height(500));
		//GUILayout.BeginVertical(GUILayout.Width(800), GUILayout.Height(800));
		foreach (var each in RemoveScene)
		{
			tmp.Add(each.Key, each.Value);
			tmp[each.Key] = GUILayout.Toggle(tmp[each.Key], each.Key, GUILayout.Width(600));
		}
		//GUILayout.EndVertical();
		GUILayout.EndScrollView();
		foreach (var each in tmp)
		{
			RemoveScene[each.Key] = each.Value;
		}

		if (GUILayout.Button("Delete", GUILayout.Width(100)))
			OnWizardCreate();
	}

    [MenuItem("VersionManager/11.GenScriptListFile", false, 12)]
    public static void GenScriptListFile()
    {
        string path = EditorUtility.OpenFolderPanel("SelectCslPanel", "Assets/StreamingAssets", "StreamingAssets");
        if (path == "")
            return;
        string strScriptList = EditorUtility.SaveFilePanel("Save ScriptList.txt", "", "", "txt");
        FileStream fs = null;
        if (!string.IsNullOrEmpty(strScriptList))
            fs = File.Open(strScriptList, FileMode.OpenOrCreate);
        if (fs == null)
            return;
        string[] SelectPath = Directory.GetFiles(path, "*.csl");
        byte[] buff = null;
        for (int i = 0; i < SelectPath.Length; i++)
        {
            int iIndex = SelectPath[i].LastIndexOf('\\');
            if (iIndex == -1)
                iIndex = SelectPath[i].LastIndexOf('/');
            string str = SelectPath[i].Substring(iIndex + 1);
            buff = System.Text.UTF8Encoding.UTF8.GetBytes(str + ";");
            fs.Write(buff, 0, buff.Length);
        }
        fs.Flush();
        fs.Close();
    }

}