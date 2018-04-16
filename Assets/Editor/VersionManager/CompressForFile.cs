using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;


public class CompressForFile
{

	private static void CompressFileLZMA(string inFile, string outFile)
	{
		SevenZip.Compression.LZMA.Encoder coder = new SevenZip.Compression.LZMA.Encoder();
		FileStream input = new FileStream(inFile, FileMode.Open);
		FileStream output = new FileStream(outFile, FileMode.Create);
		
		// Write the encoder properties
		coder.WriteCoderProperties(output);
		
		// Write the decompressed file size.
		output.Write(System.BitConverter.GetBytes(input.Length), 0, 8);
		
		// Encode the file.
		coder.Code(input, output, input.Length, -1, null);
		output.Flush();
		output.Close();
		input.Close();
	}
	
	private static void DecompressFileLZMA(string inFile, string outFile)
	{
		SevenZip.Compression.LZMA.Decoder coder = new SevenZip.Compression.LZMA.Decoder();
		FileStream input = new FileStream(inFile, FileMode.Open);
		FileStream output = new FileStream(outFile, FileMode.Create);
		
		// Read the decoder properties
		byte[] properties = new byte[5];
		input.Read(properties, 0, 5);
		
		// Read in the decompress file size.
		byte [] fileLengthBytes = new byte[8];
		input.Read(fileLengthBytes, 0, 8);
		long fileLength = System.BitConverter.ToInt64(fileLengthBytes, 0);
		
		// Decompress the file.
		coder.SetDecoderProperties(properties);
		coder.Code(input, output, input.Length, fileLength, null);
		output.Flush();
		output.Close();
		input.Close();
	}

	public static bool CompressFile(string strFileSource, string strFileTarget)
	{
		CompressFileLZMA(strFileSource, strFileTarget);
		AssetDatabase.Refresh();
		return true;
	}

	public static void CompressVersionXml(UnityEditor.BuildTarget target)
	{
		string SavePath = PlatformMap.GetPlatformPath(target);
		CompressFileLZMA(SavePath + "/" + GenAllUpdateVersionXml.strVFile + ".xml", SavePath + "/" + GenAllUpdateVersionXml.strVFile + ".zip");
		AssetDatabase.Refresh();
		return;
	}

    //对加密文件压缩并删除.
    //对非加密文件只压缩.
    //输出压缩后的文件列表.
    public static void Compress(string strBasePath, List<string> CompressTarget, bool bDeleted, ref List<string> CompressResult)
    {
        foreach (string file in CompressTarget)
        {
            string path = file;
            if (path.EndsWith(".XOR"))
                path = path.Substring(0, path.Length - 4);
            string name = "";
            int nNameBegin = path.LastIndexOf('/');
            if (nNameBegin == -1)
                Debug.Log("file named error" + path);
            name = path.Substring(nNameBegin + 1);
            path = strBasePath + path;
            path = path.Replace("\\", "/");
            int nDirectoryIndex = path.LastIndexOf('/');
            Directory.CreateDirectory(path.Substring(0, nDirectoryIndex));
            path += ".zip";
            CompressFileLZMA(file, path);
            if (bDeleted)
                File.Delete(file); 
            CompressResult.Add(path);
        }
        AssetDatabase.Refresh();
    }

	public static void CompressScript(UnityEditor.BuildTarget target, List<string> ignoreFilelist, List<string> CompressTarget, bool bDeleted)
	{
		string SavePath = "";
		try
		{
			SavePath = PlatformMap.GetPlatformPath(target) + "/" + VersionManager.GetCurVersion(target)+ "/";
		}
		catch(IOException exp)
		{
			EditorUtility.DisplayDialog("Error", exp.Message, "OK");
			return;
		}

		int compressfile = 0;
		int uncompressfile = 0;
	
		foreach (string file in CompressTarget)
		{
			string path = file;
			if (path.EndsWith(".XOR"))
			{

				path = path.Substring(0, path.Length - 4);
			}

			if (ignoreFilelist != null)
			{
				bool bIgnore = false;
				string name = "";
				int nNameBegin = path.LastIndexOf('/');
				if (nNameBegin == -1)
					Debug.Log("file named error" + path);
				name = path.Substring(nNameBegin + 1);
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
					uncompressfile++;
					continue;
				}
			}
			path = SavePath + path;
			path = path.Replace("\\", "/");
			int nDirectoryIndex = path.LastIndexOf('/');
			Directory.CreateDirectory(path.Substring(0, nDirectoryIndex));
			path += ".zip";
			CompressFileLZMA(file, path);
			if (bDeleted)
				File.Delete(file);
			compressfile++;
		}
		AssetDatabase.Refresh();
	}
	

	public static void Extract(UnityEditor.BuildTarget target)
	{
		string SavePath = PlatformMap.GetPlatformPath(target);
		DecompressFileLZMA(SavePath + "/v.zip", SavePath + "/" + "v_bak.xml");
		AssetDatabase.Refresh();
//		FileStream fsWrite = new FileStream(SavePath + "/" + "v_bak.xml", FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
//		FileStream fsRead = new FileStream(SavePath + "/v.zip", FileMode.Open, FileAccess.Read, FileShare.Read);
//		GZipStream zs = new GZipStream(fsRead, CompressionMode.Decompress, true);
//		int n = 0;
//		byte []buff = new byte[100];
//		while ((n = zs.Read(buff, 0, 100)) != 0)
//		{
//			fsWrite.Write(buff, 0, n);
//		}
//		zs.Close();
//		fsWrite.Flush();
//		fsWrite.Close();
//		AssetDatabase.Refresh();
	}
	
	public static string ConvertToAssetBundleName(string ResName)
	{
		return ResName.Replace('/', '.');
	}
}