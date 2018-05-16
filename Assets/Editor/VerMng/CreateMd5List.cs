using UnityEngine;
using UnityEditor;
using System.IO;
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

    
	public static void Execute(string baseDir, string newVersion)
	{
        GenFileListXml(baseDir, newVersion);
	}
	
    public static void GenFileListXml(string strBaseDirectory, string version)
    {
        MD5CryptoServiceProvider md5Generator = new MD5CryptoServiceProvider();
        List<UpdateItem> fileInfo = new List<UpdateItem>();
        string strPath = strBaseDirectory + "/" + version + "/";
        string[] allfiles = Directory.GetFiles(strPath, "*.*", SearchOption.AllDirectories);
        foreach (string filePath in allfiles)
        {
            if (filePath.EndsWith(".meta") ||
                filePath.EndsWith(".DS_Store") ||
                filePath.EndsWith("entries"))
                continue;
            FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] hash = md5Generator.ComputeHash(file);
            string strMD5 = System.BitConverter.ToString(hash);
            string localPath = filePath.Substring(strPath.Length, filePath.Length - strPath.Length);
            UpdateItem Item = new UpdateItem();
            Item.strLocalPath = localPath.Replace('\\', '/');
            Item.md5 = strMD5;
            Item.size = file.Length;
            file.Close();
            fileInfo.Add(Item);
        }
        string json = LitJson.JsonMapper.ToJson(fileInfo);
        File.WriteAllText(strBaseDirectory + "/" + version + ".json", json);
    }
}