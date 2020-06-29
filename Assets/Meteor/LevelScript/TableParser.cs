using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using UnityEngine;

public static class TableParser
{
    static void ParsePropertyValue<T>(T obj, FieldInfo fieldInfo, string valueStr)
    {
        System.Object value = valueStr;
        if (fieldInfo.FieldType.IsEnum)
            value = Enum.Parse(fieldInfo.FieldType, valueStr);
        else
        {
            if (fieldInfo.FieldType == typeof(int))
                value = int.Parse(valueStr);
            else if (fieldInfo.FieldType == typeof(byte))
                value = byte.Parse(valueStr);
            else if (fieldInfo.FieldType == typeof(float))
                value = float.Parse(valueStr);
            else if (fieldInfo.FieldType == typeof(double))
                value = double.Parse(valueStr);
            else
            {
                if (valueStr.Contains("\"\""))
                    valueStr = valueStr.Replace("\"\"", "\"");

                // process the excel string.
                if (valueStr.Length > 2 && valueStr[0] == '\"' && valueStr[valueStr.Length - 1] == '\"')
                    valueStr = valueStr.Substring(1, valueStr.Length - 2);
                
                value = valueStr;
            }
        }

        if (value == null)
            return;

        fieldInfo.SetValue(obj, value);
    }

    static T ParseObject<T>(string[] lines, int idx, Dictionary<int, FieldInfo> propertyInfos)
    {
        T obj = Activator.CreateInstance<T>();
        string line = lines[idx];
        string[] values = line.Split('\t');
        foreach (KeyValuePair<int, FieldInfo> pair in propertyInfos)
        {
            if (pair.Key >= values.Length)
                continue;

            string value = values[pair.Key];
            if (string.IsNullOrEmpty(value))
                continue;
            Type tp = typeof(T);
            try
            {
                ParsePropertyValue(obj, pair.Value, value);
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("ParseError: Row={0} Column={1} Name={2} Want={3} Get={4} Table = {5}",
                    idx + 1,
                    pair.Key + 1,
                    pair.Value.Name,
                    pair.Value.FieldType.Name,
                    value,
                    tp.Name));
                throw ex;
            }
        }
        return obj;
    }

    static Dictionary<int, FieldInfo> GetPropertyInfos<T>(string memberLine)
    {
        Type objType = typeof(T);

        string[] members = memberLine.Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        Dictionary<int, FieldInfo> propertyInfos = new Dictionary<int, FieldInfo>();
        for (int i = 0; i < members.Length; i++)
        {
            FieldInfo fieldInfo = objType.GetField(members[i]);
            if (fieldInfo == null)
                continue;
            propertyInfos[i] = fieldInfo;
        }

        return propertyInfos;
    }

    // parse a data array from the table data.
//    static T[] LoadTable<T>(string name)
//    {
//        TextAsset textAsset = UpdateHelper.Instance.GetAsset("Tables.unity3d", name);
//        MemoryStream stream = new MemoryStream(textAsset.bytes);
//        return ProtoBuf.Serializer.Deserialize<T[]>(stream);
//    }


    public static T[] Parse<T>(string name, bool asFile = false)
    {
		string[] lines;
        string strText = "";
        if (asFile)
        {
            if (File.Exists(name))
                strText = File.ReadAllText(name);
            else
            {
                Debug.LogError("文件不存在");
                return new T[0];
            }
        }
        else
        {
            TextAsset asset = Resources.Load<TextAsset>(name);
            if (asset != null)
                strText = asset.text;
        }

		if (strText == null)
		{
			Debug.LogError("无法加载表格文件：" + name);
            return new T[0];
        }

		if (strText.Length == 0)
		{
			Debug.LogError("表格数据为空：" + name);
			return new T[0];
		}
		// try parse the table lines.
		lines = strText.Trim().Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length < 3)
        {
            Debug.LogError("表格文件行数错误，【1】属性名称【2】变量名称【3-...】值：" + name);
            return new T[0];
        }

        // fetch all of the field infos.
        Dictionary<int, FieldInfo> propertyInfos = GetPropertyInfos<T>(lines[1]);

        // parse it one by one.
        T[] array = new T[lines.Length - 2];
        for (int i = 0; i < lines.Length - 2; i++)
            array[i] = ParseObject<T>(lines, i + 2, propertyInfos);

        return array;
    }

}
