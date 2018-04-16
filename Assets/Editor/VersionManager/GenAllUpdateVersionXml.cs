using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

public class GenAllUpdateVersionXml
{
	#region UpdateXml Format
	/* update Xml Sample
	 * Filename = {oldv}_{newv}.xml
	 * <UpdateInfo Filenum = xx>
	 * <File name = xxx MD5 = xxx>
	 * ...
	 * </UpdateInfo>
	 * */
	#endregion

	#region vFile Format
	/* v File Sample
	 * <Version ServerV = xxx>
	 * <VersionFile ClientV = xxx File = xxx>
	 * ...
	 * </Version>
	 * */
	#endregion
	//update Xml Format
	internal static string strUpdateXml = "{0}_{1}";
	internal static string strUpdateRoot = "UpdateInfo";
	internal static string strUpdateFileNumAttr = "Filenum";
	internal static string strUpdateElement = "File";
	internal static string strUpdateElementNameAttr = "name";
	internal static string strUpdateElementHashAttr = "MD5";

	//v File Format
	public static string strVFile = "v";
	internal static string strVXmlRoot = "Version";
	internal static string strServerVAttr = "ServerV";
	internal static string strClientVAttr = "ClientV";
	internal static string strElement = "VersionFile";
	internal static string strElementFileAttr = "File";
	internal static string strClientVNumAttr = "FileNum";

    public static void GenUpdateXmlByVersion(BuildTarget target, string strNewVersion, List<string> strOldVersion)
    {
        string platformPath = PlatformMap.GetPlatformPath(target) + "/";
        Dictionary<string, Dictionary<string, string>> strOldVersionInfo = new Dictionary<string, Dictionary<string, string>>();
        foreach (string str in strOldVersion)
        {
            string strFilePath = platformPath + str + "/" + CreateMD5List.strFileName;
            Dictionary<string, string> dicOldMD5Info = ReadMD5File(strFilePath);
            if (!File.Exists(strFilePath))
            {
                EditorUtility.DisplayDialog("Error", "Version File lost", "OK");
                return;
            }

            strOldVersionInfo.Add(str, dicOldMD5Info);
        }

        string newVerXmlPath = platformPath + strNewVersion + "/" + CreateMD5List.strFileName;
        Dictionary<string, string> dicNewMD5Info = ReadMD5File(newVerXmlPath);

        // v File Info DataStruct
        Dictionary<string, string> vFileInfoDict = new Dictionary<string, string>();
        string strVersionXml = platformPath + strVFile;

        // Compare MD5 Generate UpdateXml
        foreach (KeyValuePair<string, Dictionary<string, string>> oldVer in strOldVersionInfo)
        {
            string strUpdateXmlTmp = string.Format(strUpdateXml, oldVer.Key, strNewVersion) + ".xml";
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement xmlRoot = xmlDoc.CreateElement(strUpdateRoot);
            xmlDoc.AppendChild(xmlRoot);

            foreach (KeyValuePair<string, string> NewFileList in dicNewMD5Info)
            {
                //old File Find
                string strMD5 = "";
                XmlElement item = null;
                if (oldVer.Value.TryGetValue(NewFileList.Key, out strMD5))
                {
                    if (strMD5 != NewFileList.Value)
                    {
                        item = xmlDoc.CreateElement(strUpdateElement);
                        item.SetAttribute(strUpdateElementNameAttr, NewFileList.Key.Replace('\\', '/'));
                        item.SetAttribute(strUpdateElementHashAttr, NewFileList.Value);
                    }
                }
                else
                {
                    item = xmlDoc.CreateElement(strUpdateElement);
                    item.SetAttribute(strUpdateElementNameAttr, NewFileList.Key.Replace('\\', '/'));
                    item.SetAttribute(strUpdateElementHashAttr, NewFileList.Value);
                }
                if (item == null)
                    continue;
                xmlRoot.AppendChild(item);
            }
            xmlRoot.SetAttribute(strUpdateFileNumAttr, xmlRoot.ChildNodes.Count.ToString());
            xmlDoc.Save(platformPath + strUpdateXmlTmp);
            string strZip = string.Format(strUpdateXml, oldVer.Key, strNewVersion) + ".zip";
            CompressForFile.CompressFile(platformPath + strUpdateXmlTmp, platformPath + strZip);
            vFileInfoDict.Add(oldVer.Key, strZip);
            xmlDoc = null;
        }

        //update v.xml
        SaveVFile(strNewVersion, vFileInfoDict, strOldVersionInfo.Count, strVersionXml + ".xml");
        CompressForFile.CompressFile(strVersionXml + ".xml", strVersionXml + ".zip");
    }

	public static void Execute(UnityEditor.BuildTarget target)
	{
		ExecuteEx(target);
		AssetDatabase.Refresh();
	}


	public static void ExecuteEx(UnityEditor.BuildTarget target)
	{
		string platformPath = PlatformMap.GetPlatformPath(target) + "/";

		Dictionary<string, Dictionary<string, string>> strOldVersionInfo = new Dictionary<string, Dictionary<string, string>>();

		List<string> strOldVersion = VersionManager.GetHistoryVersion(target);
		foreach (string str in strOldVersion)
		{
			string strFilePath = platformPath + str + "/" + CreateMD5List.strFileName;
			Dictionary<string, string> dicOldMD5Info = ReadMD5File(strFilePath);
			if (!File.Exists(strFilePath))
			{
				EditorUtility.DisplayDialog("Error", "Version File lost", "OK");
				return;
			}

			strOldVersionInfo.Add(str, dicOldMD5Info);
		}

		// new version Filelist XML
		string strNewVersion = "";
		try
		{
			strNewVersion = VersionManager.GetCurVersion(target);
		}
		catch(IOException exp)
		{
			EditorUtility.DisplayDialog("Error", exp.Message, "OK");
			return;
		}

		string newVerXmlPath = platformPath + strNewVersion + "/" + CreateMD5List.strFileName;
		Dictionary<string, string> dicNewMD5Info = ReadMD5File(newVerXmlPath);

		// v File Info DataStruct
		Dictionary<string, string> vFileInfoDict = new Dictionary<string, string>();
		string strVersionXml = platformPath + strVFile;

		// Compare MD5 Generate UpdateXml
		foreach (KeyValuePair<string, Dictionary<string, string>> oldVer in strOldVersionInfo)
		{
			string strUpdateXmlTmp = string.Format(strUpdateXml, oldVer.Key, strNewVersion) + ".xml";
			XmlDocument xmlDoc = new XmlDocument();
			XmlElement xmlRoot = xmlDoc.CreateElement(strUpdateRoot);
			xmlDoc.AppendChild(xmlRoot);

			foreach (KeyValuePair<string, string> NewFileList in dicNewMD5Info)
			{
				//old File Find
				string strMD5 = "";
				XmlElement item = null;
				if (oldVer.Value.TryGetValue(NewFileList.Key, out strMD5))
				{
					if (strMD5 != NewFileList.Value)
					{
						item = xmlDoc.CreateElement(strUpdateElement);
						item.SetAttribute(strUpdateElementNameAttr, NewFileList.Key.Replace('\\', '/'));
						item.SetAttribute(strUpdateElementHashAttr, NewFileList.Value);
					}
				}
				else
				{
					item = xmlDoc.CreateElement(strUpdateElement);
					item.SetAttribute(strUpdateElementNameAttr, NewFileList.Key.Replace('\\', '/'));
					item.SetAttribute(strUpdateElementHashAttr, NewFileList.Value);
				}
				if (item == null)
					continue;

				xmlRoot.AppendChild(item);
			}
			xmlRoot.SetAttribute(strUpdateFileNumAttr, xmlRoot.ChildNodes.Count.ToString());
			xmlDoc.Save(platformPath + strUpdateXmlTmp);
			string strZip = string.Format(strUpdateXml, oldVer.Key, strNewVersion) + ".zip";
			CompressForFile.CompressFile(platformPath + strUpdateXmlTmp, platformPath + strZip);
			vFileInfoDict.Add(oldVer.Key, strZip);
			xmlDoc = null;
		}

		//update v.xml
		SaveVFile(strNewVersion, vFileInfoDict, strOldVersionInfo.Count, strVersionXml + ".xml");
		CompressForFile.CompressFile(strVersionXml + ".xml", strVersionXml + ".zip");
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
			
			string file = (node as XmlElement).GetAttribute(CreateMD5List.strFileAttrName);
			string md5 = (node as XmlElement).GetAttribute(CreateMD5List.strFileAttrMd5);
			
			if (DicMD5.ContainsKey(file) == false)
			{
				DicMD5.Add(file, md5);
			}
		}
		
		XmlRoot = null;
		XmlDoc = null;
		
		return DicMD5;
	}


	static void SaveVFile(string strServerv, Dictionary<string, string> vFileInfoDict, int oldVNum, string strSavePath)
	{
		XmlDocument XmlDoc = new XmlDocument();
		XmlElement XmlRoot = XmlDoc.CreateElement(strVXmlRoot);
		XmlDoc.AppendChild(XmlRoot);
		XmlRoot.SetAttribute(strServerVAttr, strServerv);
		XmlRoot.SetAttribute(strClientVNumAttr, oldVNum.ToString());
		foreach (KeyValuePair<string, string> each in vFileInfoDict)
		{
			XmlElement xmlElem = XmlDoc.CreateElement(strElement);
			XmlRoot.AppendChild(xmlElem);
			xmlElem.SetAttribute(strClientVAttr, each.Key);
			xmlElem.SetAttribute(strElementFileAttr, each.Value);
		}
		
		XmlDoc.Save(strSavePath);
		XmlRoot = null;
		XmlDoc = null;
	}
}