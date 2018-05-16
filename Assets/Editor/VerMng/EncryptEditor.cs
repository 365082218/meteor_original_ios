using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;

public class EncryptEditor: EditorWindow
{
    //批量加密
	public static bool EncryptRes(List<string> Filelist, ref List<string> output)
	{
		int EncryptFileNum = 0;
		foreach (string file in Filelist)
		{
            //过滤Mac下的
			if (file.EndsWith(".DS_Store") || file.EndsWith(".meta") || file.EndsWith("entries"))
				continue;
			string path = file;
			string name = "";
			string perfix = "";
			int nNameBegin = path.LastIndexOf('/');
			if (nNameBegin == -1)
				Debug.Log("file:" + file);
			perfix = path.Substring(0, nNameBegin + 1);
			name = path.Substring(nNameBegin + 1);
			FileStream fs = null;
			try
			{
				fs = File.Open(path, FileMode.Open, FileAccess.Read);
			}

			catch (System.Exception exp)
			{
				Debug.LogError(exp.Message);
				return false;
			}

			FileStream fsWrite = File.Create(perfix + name + ".dat");
			if (fs != null && fsWrite != null)
			{
				WSLog.Log("path:" + path + "fs.length:" + fs.Length + "fs.name" + fs.Name);
				byte [] fileArray = new byte[fs.Length];
				fs.Read(fileArray, 0, (int)fs.Length);
                byte[] encryptedMem = Encrypt.EncryptArray(fileArray);
				fsWrite.Write(encryptedMem, 0, encryptedMem.Length);
				fsWrite.Close();
                EncryptFileNum++;
				if (!output.Contains(perfix + name + ".XOR"))
					output.Add(perfix + name + ".XOR");
			}
		}
		AssetDatabase.Refresh();
		return true;
	}
}
