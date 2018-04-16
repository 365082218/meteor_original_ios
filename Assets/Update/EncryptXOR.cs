using UnityEngine;
using System.Collections;
using System.Security.Cryptography;

public class EncryptXOR
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
}
