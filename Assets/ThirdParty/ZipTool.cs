/******************************************************
* DESCRIPTION: Zip包的压缩与解压
*
*     Copyright (c) 2017, 谭伟俊 （TanWeijun）
*     All rights reserved
*
* CREATED: 2017.03.11, 08:37, CST
******************************************************/

using System.IO;
using System.Collections;
using UnityEngine;
using ICSharpCode.SharpZipLib.Zip;

public static class ZipUtility
{
    #region ZipCallback
    public abstract class ZipCallback
    {
        /// <summary>
        /// 压缩单个文件或文件夹前执行的回调
        /// </summary>
        /// <param name="_entry"></param>
        /// <returns>如果返回true，则压缩文件或文件夹，反之则不压缩文件或文件夹</returns>
        public virtual bool OnPreZip(ZipEntry _entry)
        {
            return true;
        }

        /// <summary>
        /// 压缩单个文件或文件夹后执行的回调
        /// </summary>
        /// <param name="_entry"></param>
        public virtual void OnPostZip(ZipEntry _entry) { }

        /// <summary>
        /// 压缩执行完毕后的回调
        /// </summary>
        /// <param name="_result">true表示压缩成功，false表示压缩失败</param>
        public virtual void OnFinished(bool _result) { }
    }
    #endregion

    #region UnzipCallback
    public abstract class UnzipCallback
    {
        /// <summary>
        /// 解压单个文件或文件夹前执行的回调
        /// </summary>
        /// <param name="_entry"></param>
        /// <returns>如果返回true，则压缩文件或文件夹，反之则不压缩文件或文件夹</returns>
        public virtual bool OnPreUnzip(ZipEntry _entry)
        {
            return true;
        }

        /// <summary>
        /// 解压单个文件或文件夹后执行的回调
        /// </summary>
        /// <param name="_entry"></param>
        public virtual void OnPostUnzip(ZipEntry _entry) { }

        /// <summary>
        /// 解压执行完毕后的回调
        /// </summary>
        /// <param name="_result">true表示解压成功，false表示解压失败</param>
        public virtual void OnFinished(bool _result) { }
    }
    #endregion

    /// <summary>
    /// 压缩文件和文件夹
    /// </summary>
    /// <param name="_fileOrDirectoryArray">文件夹路径和文件名</param>
    /// <param name="_outputPathName">压缩后的输出路径文件名</param>
    /// <param name="_password">压缩密码</param>
    /// <param name="_zipCallback">ZipCallback对象，负责回调</param>
    /// <returns></returns>
    public static bool Zip(string[] _fileOrDirectoryArray, string _outputPathName, string _password = null, ZipCallback _zipCallback = null)
    {
        if ((null == _fileOrDirectoryArray) || string.IsNullOrEmpty(_outputPathName))
        {
            if (null != _zipCallback)
                _zipCallback.OnFinished(false);

            return false;
        }

        ZipOutputStream zipOutputStream = new ZipOutputStream(File.Create(_outputPathName));
        zipOutputStream.SetLevel(6);    // 压缩质量和压缩速度的平衡点
        if (!string.IsNullOrEmpty(_password))
            zipOutputStream.Password = _password;

        for (int index = 0; index < _fileOrDirectoryArray.Length; ++index)
        {
            bool result = false;
            string fileOrDirectory = _fileOrDirectoryArray[index];
            if (Directory.Exists(fileOrDirectory))
                result = ZipDirectory(fileOrDirectory, string.Empty, zipOutputStream, _zipCallback);
            else if (File.Exists(fileOrDirectory))
                result = ZipFile(fileOrDirectory, string.Empty, zipOutputStream, _zipCallback);

            if (!result)
            {
                if (null != _zipCallback)
                    _zipCallback.OnFinished(false);

                return false;
            }
        }

        zipOutputStream.Finish();
        zipOutputStream.Close();

        if (null != _zipCallback)
            _zipCallback.OnFinished(true);

        return true;
    }

    /// <summary>
    /// 解压Zip包
    /// </summary>
    /// <param name="_filePathName">Zip包的文件路径名</param>
    /// <param name="_outputPath">解压输出路径</param>
    /// <param name="_password">解压密码</param>
    /// <param name="_unzipCallback">UnzipCallback对象，负责回调</param>
    /// <returns></returns>
    public static bool UnzipFile(string _filePathName, string _outputPath, string _password = null, UnzipCallback _unzipCallback = null)
    {
        if (string.IsNullOrEmpty(_filePathName) || string.IsNullOrEmpty(_outputPath))
        {
            if (null != _unzipCallback)
                _unzipCallback.OnFinished(false);

            return false;
        }

        try
        {
            return UnzipFile(File.OpenRead(_filePathName), _outputPath, _password, _unzipCallback);
        }
        catch (System.Exception _e)
        {
            Debug.LogError("[ZipUtility.UnzipFile]: " + _e.ToString());

            if (null != _unzipCallback)
                _unzipCallback.OnFinished(false);

            return false;
        }
    }

    /// <summary>
    /// 解压Zip包
    /// </summary>
    /// <param name="_fileBytes">Zip包字节数组</param>
    /// <param name="_outputPath">解压输出路径</param>
    /// <param name="_password">解压密码</param>
    /// <param name="_unzipCallback">UnzipCallback对象，负责回调</param>
    /// <returns></returns>
    public static bool UnzipFile(byte[] _fileBytes, string _outputPath, string _password = null, UnzipCallback _unzipCallback = null)
    {
        if ((null == _fileBytes) || string.IsNullOrEmpty(_outputPath))
        {
            if (null != _unzipCallback)
                _unzipCallback.OnFinished(false);

            return false;
        }

        bool result = UnzipFile(new MemoryStream(_fileBytes), _outputPath, _password, _unzipCallback);
        if (!result)
        {
            if (null != _unzipCallback)
                _unzipCallback.OnFinished(false);
        }

        return result;
    }

    /// <summary>
    /// 解压Zip包
    /// </summary>
    /// <param name="_inputStream">Zip包输入流</param>
    /// <param name="_outputPath">解压输出路径</param>
    /// <param name="_password">解压密码</param>
    /// <param name="_unzipCallback">UnzipCallback对象，负责回调</param>
    /// <returns></returns>
    public static bool UnzipFile(Stream _inputStream, string _outputPath, string _password = null, UnzipCallback _unzipCallback = null)
    {
        if ((null == _inputStream) || string.IsNullOrEmpty(_outputPath))
        {
            if (null != _unzipCallback)
                _unzipCallback.OnFinished(false);

            return false;
        }

        // 创建文件目录
        if (!Directory.Exists(_outputPath))
            Directory.CreateDirectory(_outputPath);

        // 解压Zip包
        ZipEntry entry = null;
        using (ZipInputStream zipInputStream = new ZipInputStream(_inputStream))
        {
            if (!string.IsNullOrEmpty(_password))
                zipInputStream.Password = _password;

            while (null != (entry = zipInputStream.GetNextEntry()))
            {
                if (string.IsNullOrEmpty(entry.Name))
                    continue;

                if ((null != _unzipCallback) && !_unzipCallback.OnPreUnzip(entry))
                    continue;   // 过滤

                string filePathName = Path.Combine(_outputPath, entry.Name);

                // 创建文件目录
                if (entry.IsDirectory)
                {
                    Directory.CreateDirectory(filePathName);
                    continue;
                }

                // 写入文件
                try
                {
                    using (FileStream fileStream = File.Create(filePathName))
                    {
                        byte[] bytes = new byte[1024];
                        while (true)
                        {
                            int count = zipInputStream.Read(bytes, 0, bytes.Length);
                            if (count > 0)
                                fileStream.Write(bytes, 0, count);
                            else
                            {
                                if (null != _unzipCallback)
                                    _unzipCallback.OnPostUnzip(entry);

                                break;
                            }
                        }
                    }
                }
                catch (System.Exception _e)
                {
                    Debug.LogError("[ZipUtility.UnzipFile]: " + _e.ToString());

                    if (null != _unzipCallback)
                        _unzipCallback.OnFinished(false);

                    return false;
                }
            }
        }

        if (null != _unzipCallback)
            _unzipCallback.OnFinished(true);

        return true;
    }

    /// <summary>
    /// 压缩文件
    /// </summary>
    /// <param name="_filePathName">文件路径名</param>
    /// <param name="_parentRelPath">要压缩的文件的父相对文件夹</param>
    /// <param name="_zipOutputStream">压缩输出流</param>
    /// <param name="_zipCallback">ZipCallback对象，负责回调</param>
    /// <returns></returns>
    private static bool ZipFile(string _filePathName, string _parentRelPath, ZipOutputStream _zipOutputStream, ZipCallback _zipCallback = null)
    {
        //Crc32 crc32 = new Crc32();
        ZipEntry entry = null;
        FileStream fileStream = null;
        try
        {
            string entryName = _parentRelPath + '/' + Path.GetFileName(_filePathName);
            entry = new ZipEntry(entryName);
            entry.DateTime = System.DateTime.Now;

            if ((null != _zipCallback) && !_zipCallback.OnPreZip(entry))
                return true;    // 过滤

            fileStream = File.OpenRead(_filePathName);
            byte[] buffer = new byte[fileStream.Length];
            fileStream.Read(buffer, 0, buffer.Length);
            fileStream.Close();

            entry.Size = buffer.Length;

            //crc32.Reset();
            //crc32.Update(buffer);
            //entry.Crc = crc32.Value;

            _zipOutputStream.PutNextEntry(entry);
            _zipOutputStream.Write(buffer, 0, buffer.Length);
        }
        catch (System.Exception _e)
        {
            Debug.LogError("[ZipUtility.ZipFile]: " + _e.ToString());
            return false;
        }
        finally
        {
            if (null != fileStream)
            {
                fileStream.Close();
                fileStream.Dispose();
            }
        }

        if (null != _zipCallback)
            _zipCallback.OnPostZip(entry);

        return true;
    }

    /// <summary>
    /// 压缩文件夹
    /// </summary>
    /// <param name="_path">要压缩的文件夹</param>
    /// <param name="_parentRelPath">要压缩的文件夹的父相对文件夹</param>
    /// <param name="_zipOutputStream">压缩输出流</param>
    /// <param name="_zipCallback">ZipCallback对象，负责回调</param>
    /// <returns></returns>
    private static bool ZipDirectory(string _path, string _parentRelPath, ZipOutputStream _zipOutputStream, ZipCallback _zipCallback = null)
    {
        ZipEntry entry = null;
        try
        {
            string entryName = Path.Combine(_parentRelPath, Path.GetFileName(_path) + '/');
            entry = new ZipEntry(entryName);
            entry.DateTime = System.DateTime.Now;
            entry.Size = 0;

            if ((null != _zipCallback) && !_zipCallback.OnPreZip(entry))
                return true;    // 过滤

            _zipOutputStream.PutNextEntry(entry);
            _zipOutputStream.Flush();

            string[] files = Directory.GetFiles(_path);
            for (int index = 0; index < files.Length; ++index)
                ZipFile(files[index], Path.Combine(_parentRelPath, Path.GetFileName(_path)), _zipOutputStream, _zipCallback);
        }
        catch (System.Exception _e)
        {
            Debug.LogError("[ZipUtility.ZipDirectory]: " + _e.ToString());
            return false;
        }

        string[] directories = Directory.GetDirectories(_path);
        for (int index = 0; index < directories.Length; ++index)
        {
            if (!ZipDirectory(directories[index], Path.Combine(_parentRelPath, Path.GetFileName(_path)), _zipOutputStream, _zipCallback))
                return false;
        }

        if (null != _zipCallback)
            _zipCallback.OnPostZip(entry);

        return true;
    }
}