using System.Security.Cryptography;


public class Encrypt
{
    private static string key = "AAAABBBBCCCCDDDDEEEEFFFFGGGGHHHH";//32
    public static byte[] EncryptFile(string encryptFile)
    {
        byte[] keyArray = System.Text.Encoding.UTF8.GetBytes(key);
        byte[] toEncryptArray = System.IO.File.ReadAllBytes(encryptFile);
        RijndaelManaged rDel = new RijndaelManaged();
        rDel.Key = keyArray;
        rDel.Mode = CipherMode.ECB;
        rDel.Padding = PaddingMode.PKCS7;
        ICryptoTransform cTransform = rDel.CreateEncryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
        return resultArray;
    }

    public static byte[] DecryptFile(string decryptFile)
    {
        byte[] keyArray = System.Text.Encoding.UTF8.GetBytes(key);
        byte[] toEncryptArray = System.IO.File.ReadAllBytes(decryptFile);
        RijndaelManaged rDel = new RijndaelManaged();
        rDel.Key = keyArray;
        rDel.Mode = CipherMode.ECB;
        rDel.Padding = PaddingMode.PKCS7;
        ICryptoTransform cTransform = rDel.CreateDecryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
        return resultArray;
    }

    public static byte[] EncryptArray(byte[] buff)
    {
        byte[] keyArray = System.Text.Encoding.UTF8.GetBytes(key);
        RijndaelManaged rDel = new RijndaelManaged();
        rDel.Key = keyArray;
        rDel.Mode = CipherMode.ECB;
        rDel.Padding = PaddingMode.PKCS7;
        ICryptoTransform cTransform = rDel.CreateEncryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(buff, 0, buff.Length);
        return resultArray;
    }

    public static byte[] DecryptArray(byte[] buff)
    {
        byte[] keyArray = System.Text.Encoding.UTF8.GetBytes(key);
        RijndaelManaged rDel = new RijndaelManaged();
        rDel.Key = keyArray;
        rDel.Mode = CipherMode.ECB;
        rDel.Padding = PaddingMode.PKCS7;
        ICryptoTransform cTransform = rDel.CreateDecryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(buff, 0, buff.Length);
        return resultArray;
    }
}

