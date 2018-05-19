using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;

public class OneFileInfor
{
    public int m_id = 0;
    public int m_StartPos = 0;
    public int m_Size = 0;
    public int m_PathLength = 0;
    public string m_Path = "";
    public byte[] m_data = null;
};

public class UPKPacker
{
    private static int m_id = 0;
    private static int m_totalSize = 0;
    private static Dictionary<int, OneFileInfor> m_allFileInfoDic = new Dictionary<int, OneFileInfor>();
    /** 遍历文件夹获取所有文件信息 **/
    private static void ScanFolder(string folderpath)
    {
        DirectoryInfo tmpDirectoryInfo = new DirectoryInfo(folderpath);
        folderpath = tmpDirectoryInfo.FullName.Replace("\\", "/");
        folderpath = folderpath + "/";

        Debug.Log("遍历文件夹 " + folderpath);

        string sourceDirpath = folderpath.Substring(0, folderpath.LastIndexOf('/'));

        /** 读取文件夹下面所有文件的信息 **/
        DirectoryInfo dirInfo = new DirectoryInfo(folderpath);

        foreach (FileInfo fileinfo in dirInfo.GetFiles("*.*", SearchOption.AllDirectories))
        {
            if (fileinfo.Extension == ".meta")
            {
                continue;
            }

            string filename = fileinfo.FullName.Replace("\\", "/");
            filename = filename.Replace(sourceDirpath + "/", "");

            int filesize = (int)fileinfo.Length;

            Debug.Log(m_id + " : " + filename + " 文件大小: " + filesize);

            OneFileInfor info = new OneFileInfor();
            info.m_id = m_id;
            info.m_Size = filesize;
            info.m_Path = filename;
            info.m_PathLength = new UTF8Encoding().GetBytes(filename).Length;

            /**  读取这个文件  **/
            FileStream fileStreamRead = new FileStream(fileinfo.FullName, FileMode.Open, FileAccess.Read);
            if (fileStreamRead == null)
            {
                Debug.Log("读取文件失败 ： " + fileinfo.FullName);
                return;
            }
            else
            {
                byte[] filedata = new byte[filesize];
                fileStreamRead.Read(filedata, 0, filesize);
                info.m_data = filedata;
            }
            fileStreamRead.Close();


            m_allFileInfoDic.Add(m_id, info);

            m_id++;
            m_totalSize += filesize;
        }
    }


    /**  打包一个文件夹  **/
    public static void PackFolder(string folderpath, string upkfilepath)
    {
        ScanFolder(folderpath);

        Debug.Log("文件数量 : " + m_id);
        Debug.Log("文件总大小 : " + m_totalSize);

        /**  更新文件在UPK中的起始点  **/
        int firstfilestartpos = 0 + 4;
        for (int index = 0; index < m_allFileInfoDic.Count; index++)
        {
            firstfilestartpos += 4 + 4 + 4 + 4 + m_allFileInfoDic[index].m_PathLength;
        }

        int startpos = 0;
        for (int index = 0; index < m_allFileInfoDic.Count; index++)
        {
            if (index == 0)
            {
                startpos = firstfilestartpos;
            }
            else
            {
                startpos = m_allFileInfoDic[index - 1].m_StartPos + m_allFileInfoDic[index - 1].m_Size;//上一个文件的开始+文件大小;
            }

            m_allFileInfoDic[index].m_StartPos = startpos;
        }

        /**  写文件  **/
        FileStream fileStream = new FileStream(upkfilepath, FileMode.Create);

        /**  文件总数量  **/
        byte[] totaliddata = System.BitConverter.GetBytes(m_id);
        fileStream.Write(totaliddata, 0, totaliddata.Length);

        for (int index = 0; index < m_allFileInfoDic.Count; index++)
        {
            /** 写入ID **/
            byte[] iddata = System.BitConverter.GetBytes(m_allFileInfoDic[index].m_id);
            fileStream.Write(iddata, 0, iddata.Length);

            /**  写入StartPos  **/
            byte[] startposdata = System.BitConverter.GetBytes(m_allFileInfoDic[index].m_StartPos);
            fileStream.Write(startposdata, 0, startposdata.Length);

            /**  写入size  **/
            byte[] sizedata = System.BitConverter.GetBytes(m_allFileInfoDic[index].m_Size);
            fileStream.Write(sizedata, 0, sizedata.Length);

            /**  写入pathLength  **/
            byte[] pathLengthdata = System.BitConverter.GetBytes(m_allFileInfoDic[index].m_PathLength);
            fileStream.Write(pathLengthdata, 0, pathLengthdata.Length);

            /**  写入path  **/
            byte[] mypathdata = new UTF8Encoding().GetBytes(m_allFileInfoDic[index].m_Path);

            fileStream.Write(mypathdata, 0, mypathdata.Length);
        }

        /**  写入文件数据  **/
        int totalprocessSize = 0;
        foreach (var infopair in m_allFileInfoDic)
        {
            OneFileInfor info = infopair.Value;
            int size = info.m_Size;
            byte[] tmpdata = null;
            int processSize = 0;
            while (processSize < size)
            {
                if (size - processSize < 1024)
                {
                    tmpdata = new byte[size - processSize];
                }
                else
                {
                    tmpdata = new byte[1024];
                }
                fileStream.Write(info.m_data, processSize, tmpdata.Length);

                processSize += tmpdata.Length;
                totalprocessSize += tmpdata.Length;

                //progress.SetProgressPercent(m_totalSize, totalprocessSize);
            }
        }
        fileStream.Flush();
        fileStream.Close();
        /** 重置数据 **/
        m_id = 0;
        m_totalSize = 0;
        m_allFileInfoDic.Clear();
    }
}
