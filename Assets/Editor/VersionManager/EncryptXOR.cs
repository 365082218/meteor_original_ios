using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;

public class EncryptXOR: EditorWindow
{
	//256 aes mode
	private static string key = "AAAABBBBCCCCDDDDEEEEFFFFGGGGHHHH";
	public static string Encrypt(string encryptStr)
	{
		byte[] keyArray = System.Text.UTF8Encoding.UTF8.GetBytes(key);
		byte[] toEncryptArray = System.Text.UTF8Encoding.UTF8.GetBytes(encryptStr);
		RijndaelManaged rDel = new RijndaelManaged();
		rDel.Key = keyArray;
		rDel.Mode = CipherMode.ECB;
		rDel.Padding = PaddingMode.PKCS7;
		ICryptoTransform cTransform = rDel.CreateEncryptor();
		byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
		return System.Convert.ToBase64String(resultArray, 0, resultArray.Length);
	}
	
	public static string Decrypt(string decryptStr)
	{
		byte[] keyArray = System.Text.UTF8Encoding.UTF8.GetBytes(key);
		byte[] toEncryptArray = System.Convert.FromBase64String(decryptStr);
		RijndaelManaged rDel = new RijndaelManaged();
		rDel.Key = keyArray;
		rDel.Mode = CipherMode.ECB;
		rDel.Padding = PaddingMode.PKCS7;
		ICryptoTransform cTransform = rDel.CreateDecryptor();
		byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
		return System.Text.UTF8Encoding.UTF8.GetString(resultArray);
	}
	
	//xor mode
	private static byte nKey = 10;
	public static void EncryptXOr(ref byte[] cryptBuffer, bool bEncrypt)
	{
		for (int i = 0; i < cryptBuffer.Length; ++i)
		{
			cryptBuffer[i] ^= nKey;
		}
	}
	
	public static void EncryptXorString(ref byte[] cryptBuffer)
	{
		for (int i = 0; i < cryptBuffer.Length; ++i)
		{
			cryptBuffer[i] ^= (byte)key[i % key.Length];
		}
	}

    //加密一组资源(与被加密资源同一目录,后缀为.xor).返回被加密的资源路径.
    public static bool EncryptResGroup(List<string> strFilelst, ref List<string> output)
    {
        foreach (string file in strFilelst)
        {
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
            FileStream fsWrite = File.Create(perfix + name + ".XOR");
            if (fs != null && fsWrite != null)
            {
                Debug.Log("path:" + path + "fs.length:" + fs.Length + "fs.name" + fs.Name);
                byte[] mem = new byte[fs.Length];
                fs.Read(mem, 0, (int)fs.Length);
                EncryptXorString(ref mem);
                fsWrite.Write(mem, 0, mem.Length);
                fsWrite.Close();
                if (!output.Contains(perfix + name + ".XOR"))
                    output.Add(perfix + name + ".XOR");
            }
        }
        AssetDatabase.Refresh();
        return false;
    }
	public static bool EncryptResource(BuildTarget target, List<string>ignoreFilelist, Object SelectObject, string[] strExt, ref List<string> output)
	{
		string SelectPath = AssetDatabase.GetAssetPath(SelectObject);
		List<string> allEncrypt = new List<string>();
		foreach (string st in strExt)
		{
			string []files = System.IO.Directory.GetFiles(SelectPath, st, System.IO.SearchOption.AllDirectories);
			foreach (string stPush in files)
			{
				if (!allEncrypt.Contains(stPush))
					allEncrypt.Add(stPush);
			}
		}

		int encryptfile = 0;
		int unencryptfile = 0;
		foreach (string file in allEncrypt)
		{
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

			if (ignoreFilelist != null)
			{
				bool bIgnore = false;

				foreach (string strIgnore in ignoreFilelist)
				{
					if (name == strIgnore)
					{
						bIgnore = true;
						break;
					}
				}
				
				if (bIgnore)
				{
					unencryptfile++;
					continue;
				}
			}
			//path = SavePath;
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
			FileStream fsWrite = File.Create(perfix + name + ".XOR");
			if (fs != null && fsWrite != null)
			{
				Debug.Log("path:" + path + "fs.length:" + fs.Length + "fs.name" + fs.Name);
				byte []mem = new byte[fs.Length];
				fs.Read(mem, 0, (int)fs.Length);
				EncryptXorString(ref mem);
				fsWrite.Write(mem, 0, mem.Length);
				fsWrite.Close();
				encryptfile++;
				if (!output.Contains(perfix + name + ".XOR"))
					output.Add(perfix + name + ".XOR");
			}
		}
		AssetDatabase.Refresh();
		return true;
	}
}
