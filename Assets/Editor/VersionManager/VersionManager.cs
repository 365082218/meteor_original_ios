using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Xml;
using System.Collections.Generic;
using System;
using System.IO;

public class VersionItem
{
    public string strVersion;//版本
    public string strNotice;//更新内容
}

public class VersionManager : EditorWindow{

	internal static string strVersionRoot = "VersionTotal";
	internal static string strVersionElement = "Version";
	internal static string strVersionAttr = "Ver";
	internal static string strVerFile = "Version.txt";
	internal static string Major = "0";
	internal static string Minor = "0";
	internal static string Build = "0";
	internal static string Revision = "0";
	
	public static string BuildVersion()
	{
		return Major + "." +  Minor + "." + Build + "." + Revision;
	}

	public static void ResetBaseVersion()
	{
		Major = "0";
		Minor = "0";
		Build = "0";
		Revision = "0";
	}
	

	public static string GetCurVersion(UnityEditor.BuildTarget target)
	{
		string strTarget = System.Enum.GetName(typeof(BuildTarget), target);
		if (strTarget == null || strTarget == "")
			return null;

		string strVersion = "";
        string strVersionPath;
        strVersionPath = PlatformMap.GetPlatformPath(target);
        strVersionPath += "/" + strVerFile;

        List<VersionItem> strVersionlst = JsonUtility.FromJson<List<VersionItem>>(File.ReadAllText(strVersionPath));

		if (strVersionlst.Count != 0)
			strVersion = strVersionlst[strVersionlst.Count - 1].strVersion;

		string [] strVerBuild = new string[4];
		if (strVersion.Length != 0)
		{
			strVerBuild = strVersion.Split(new char[]{'.'}, StringSplitOptions.RemoveEmptyEntries);
			if (strVerBuild.Length == 4)
			{
				Major = strVerBuild[0];
				Minor = strVerBuild[1];
				Build = strVerBuild[2];
				Revision = strVerBuild[3];
			}
		}
		return strVersion;
	}

	public static List<string> GetAllVersion(UnityEditor.BuildTarget target)
	{
		string strTarget = System.Enum.GetName(typeof(BuildTarget), target);
		if (strTarget == null || strTarget == "")
			return null;

		List<string> strRet = new List<string>();
		
		string strVersionPath;
		strVersionPath = PlatformMap.GetPlatformPath(target);
		strVersionPath += "/" + strVerFile;
		try
		{
			strRet =  ReadVersion(strVersionPath);
		}
		catch (IOException exp)
		{
		}
		return strRet;
	}

	public static List<string> GetHistoryVersion(UnityEditor.BuildTarget target)
	{
		string strTarget = System.Enum.GetName(typeof(BuildTarget), target);
		if (strTarget == null || strTarget == "")
			return null;

		List<string> strRet = new List<string>();

		string strVersionPath;
		strVersionPath = PlatformMap.GetPlatformPath(target);
		strVersionPath += "/" + strVerFile;
		try
		{
			strRet =  ReadVersion(strVersionPath);
		}
		catch (IOException exp)
		{
		}

		if (strRet.Count != 0)
		{
			string strVersion = strRet[0];
			for (int i = 1; i <= strRet.Count - 1; ++i)
			{
				if (strVersion.CompareTo(strRet[i]) == -1)
					strVersion = strRet[i];
			}

			strRet.Remove(strVersion);
		}
		return strRet;
	}


	static List<string> ReadVersion(string fileName)
	{
		List<string> strCurver = new List<string>();
		if (System.IO.File.Exists(fileName) == false)
		{
			throw new IOException(string.Format("file {0} \r\nlost Create New Version First", fileName));
			return strCurver;
		}
		XmlDocument XmlDoc = new XmlDocument();
		XmlDoc.Load(fileName);
		XmlElement XmlRoot = XmlDoc.DocumentElement;
		string strVer = "";
		foreach (XmlNode node in XmlRoot.ChildNodes)
		{
			if ((node is XmlElement) == false)
				continue;
			strVer = (node as XmlElement).GetAttribute(strVersionAttr);
			if (strVer.Length != 0)
			{
				strCurver.Add(strVer);
			}
		}

		XmlRoot = null;
		XmlDoc = null;
		return strCurver;
	}

	public static bool CreateNewVersion(BuildTarget target, string major = "", string minor = "", string build = "", string rev = "", string notices = "")
	{
		Major = major;
		Minor = minor;
		Build = build;
		Revision = rev;
		string strTip = "";
		string strVer = BuildVersion();

		strTip = "NewVersion:" + strVer + "\r\n";

		List<string> strVersionLst = new List<string>();
		strVersionLst = GetAllVersion(target);

		int nNum = 0;
		foreach (string str in strVersionLst)
		{
			++nNum;
			strTip += string.Format("OldVersion{0} : {1}\r\n", nNum, str);
		}

		if (0 != EditorUtility.DisplayDialogComplex("Tip", strTip, "OK", "Cancel", ""))
		{
			return false;
		}

		try
		{
			System.IO.Directory.CreateDirectory(PlatformMap.GetPlatformPath(target) + "/" + strVer);
		}
		catch(IOException exp)
		{
			EditorUtility.DisplayDialog("Error", "Target Directory Exist", "Quit");
			return false;
		}

		if (!UpdateVersionXml(target, strVersionLst, strVer, notices))
		{
			return false;
		}

		AssetDatabase.Refresh();
		return true;
	}

	public static bool UpdateVersionXml(BuildTarget target, List<string>strOldVersionLst, string strNewVer, string notices)
	{
		string strVersionPath;
		strVersionPath = PlatformMap.GetPlatformPath(target);
		strVersionPath += "/" + strVerFile;

		XmlDocument XmlDoc = new XmlDocument();
		XmlElement XmlRoot = XmlDoc.CreateElement(strVersionRoot);
		XmlDoc.AppendChild(XmlRoot);
		foreach (string str in strOldVersionLst)
		{
			XmlElement xmlElem = XmlDoc.CreateElement(strVersionElement);
			xmlElem.SetAttribute(strVersionAttr, str);
			XmlRoot.AppendChild(xmlElem);
		}
		XmlElement xmlNewver = XmlDoc.CreateElement(strVersionElement);
		xmlNewver.SetAttribute(strVersionAttr, strNewVer);
		XmlRoot.AppendChild(xmlNewver);
		
		XmlDoc.Save(strVersionPath);
		XmlDoc = null;
		return true;
	}

	void OnGUI()
	{
	}
}
