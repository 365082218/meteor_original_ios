using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

public class CreateMD5List
{
	internal static string strXmlRoot = "Files";
	internal static string strXmlElement = "File";
	public static string strFileAttrName = "FileName";
	public static string strFileAttrMd5 = "MD5";
	internal static string strFileName = "FileList.xml";

	public static void Execute(UnityEditor.BuildTarget target)
	{
		ExecuteEx(target);
		AssetDatabase.Refresh();
	}
	
    public static void GenFileListXml(string strBaseDirectory)
    {
        Dictionary<string, string> DicFileMD5 = new Dictionary<string, string>();
        MD5CryptoServiceProvider md5Generator = new MD5CryptoServiceProvider();

        string[] allfiles = Directory.GetFiles(strBaseDirectory, "*.*", SearchOption.AllDirectories);
        foreach (string filePath in allfiles)
        {
            if (filePath.EndsWith(".meta") ||
                filePath.EndsWith("FileList.xml") ||
                filePath.EndsWith(".xml") ||
                filePath.EndsWith(".DS_Store") ||
                filePath.EndsWith("entries") ||
                filePath.EndsWith("ForceUpdate.txt") ||
                filePath.EndsWith("referenceTable.txt") ||
                filePath.EndsWith("ext.txt") ||
                filePath.EndsWith("MergeLog.txt"))
                continue;
            if (!(filePath.EndsWith(".zip") || filePath.EndsWith(".assetbundle")))
                Debug.LogError("未知类型的文件: " + filePath);
            FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] hash = md5Generator.ComputeHash(file);
            string strMD5 = System.BitConverter.ToString(hash);
            file.Close();

            string key = filePath.Substring(strBaseDirectory.Length, filePath.Length - strBaseDirectory.Length);

            if (DicFileMD5.ContainsKey(key) == false)
                DicFileMD5.Add(key, strMD5);
            else
                Debug.LogWarning("<Two File has the same name> name = " + filePath);
        }

        XmlDocument XmlDoc = new XmlDocument();
        XmlElement XmlRoot = XmlDoc.CreateElement(strXmlRoot);
        XmlDoc.AppendChild(XmlRoot);
        foreach (KeyValuePair<string, string> pair in DicFileMD5)
        {
            XmlElement xmlElem = XmlDoc.CreateElement(strXmlElement);
            XmlRoot.AppendChild(xmlElem);
            xmlElem.SetAttribute(strFileAttrName, pair.Key.Replace('\\', '/'));
            xmlElem.SetAttribute(strFileAttrMd5, pair.Value);
        }
        try
        {
            XmlDoc.Save(strBaseDirectory + "/" + strFileName);
        }
        catch (System.Exception exp)
        {
            Debug.LogError(exp.Message);
        }
        XmlDoc = null;
    }
	public static void ExecuteEx(UnityEditor.BuildTarget target)
	{
		Dictionary<string, string> DicFileMD5 = new Dictionary<string, string>();
		MD5CryptoServiceProvider md5Generator = new MD5CryptoServiceProvider();

		string dir = "";
		try
		{
			dir = PlatformMap.GetPlatformPath(target) + "/" + VersionManager.GetCurVersion(target)+ "/";
		}
		catch(IOException exp)
		{
			EditorUtility.DisplayDialog("Error", exp.Message, "OK");
			return;
		}

		string [] allfiles = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);
		foreach (string filePath in allfiles)
		{
			if (filePath.EndsWith(".meta") || 
			    filePath.EndsWith("FileList.xml") || 
			    filePath.EndsWith(".xml") || 
			    filePath.EndsWith(".DS_Store") || 
			    filePath.EndsWith("entries") ||
				filePath.EndsWith("ForceUpdate.txt") ||
			    filePath.EndsWith("referenceTable.txt") ||
			    filePath.EndsWith("ext.txt") ||
                filePath.EndsWith("MergeLog.txt"))
				continue;
			
			FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			byte[] hash = md5Generator.ComputeHash(file);
			string strMD5 = System.BitConverter.ToString(hash);
			file.Close();
			
			string key = filePath.Substring(dir.Length, filePath.Length - dir.Length);
			
			if (DicFileMD5.ContainsKey(key) == false)
				DicFileMD5.Add(key, strMD5);
			else
				Debug.LogWarning("<Two File has the same name> name = " + filePath);
		}

		XmlDocument XmlDoc = new XmlDocument();
		XmlElement XmlRoot = XmlDoc.CreateElement(strXmlRoot);
		XmlDoc.AppendChild(XmlRoot);
		foreach (KeyValuePair<string, string> pair in DicFileMD5)
		{
			XmlElement xmlElem = XmlDoc.CreateElement(strXmlElement);
			XmlRoot.AppendChild(xmlElem);
			xmlElem.SetAttribute(strFileAttrName, pair.Key.Replace('\\','/'));
			xmlElem.SetAttribute(strFileAttrMd5, pair.Value);
		}
		try
		{
			XmlDoc.Save(dir + "/" + strFileName);
		}
		catch (System.Exception exp)
		{
			Debug.LogError(exp.Message);
		}
		XmlDoc = null;
	}
	
	static Dictionary<string, string> ReadMD5File(string fileName)
	{
		Dictionary<string, string> DicMD5 = new Dictionary<string, string>();
		
		// 如果文件不存在，则直接返回
		if (System.IO.File.Exists(fileName) == false)
			return DicMD5;
		
		XmlDocument XmlDoc = new XmlDocument();
		XmlDoc.Load(fileName);
		XmlElement XmlRoot = XmlDoc.DocumentElement;
		
		foreach (XmlNode node in XmlRoot.ChildNodes)
		{
			if ((node is XmlElement) == false)
				continue;
			
			string file = (node as XmlElement).GetAttribute("FileName");
			string md5 = (node as XmlElement).GetAttribute("MD5");
			
			if (DicMD5.ContainsKey(file) == false)
			{
				DicMD5.Add(file, md5);
			}
		}
		
		XmlRoot = null;
		XmlDoc = null;
		
		return DicMD5;
	}
	
}