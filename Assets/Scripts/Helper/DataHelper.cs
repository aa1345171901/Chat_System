﻿using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

/// <summary>
/// 用于数据处理
/// </summary>
public class DataHelper
{
    /// <summary>
    /// 将DataSet转byte数组
    /// </summary>
    /// <returns>返回转换后的Byte数组</returns>
    public static byte[] GetBinaryFormatDataSet(DataSet ds)
    {
        // 创建内存流
        MemoryStream memStream = new MemoryStream();

        // 产生二进制序列化格式
        IFormatter formatter = new BinaryFormatter();

        // 指定DataSet串行化格式是二进制
        ds.RemotingFormat = SerializationFormat.Binary;

        // 串行化到内存中
        formatter.Serialize(memStream, ds);

        // 将DataSet转化成byte[]
        byte[] binaryResult = memStream.ToArray();

        // 清空和释放内存流
        memStream.Close();
        memStream.Dispose();
        return binaryResult;
    }

    /// <summary>
    /// 将byte数组转换成DataSet
    /// </summary>
    /// <returns>转换后的DataSet</returns>
    public static DataSet RetrieveDataSet(byte[] binaryData)
    {
        // 创建内存流
        MemoryStream memStream = new MemoryStream(binaryData);

        memStream.Seek(0, SeekOrigin.Begin);

        // 产生二进制序列化格式
        IFormatter formatter = new BinaryFormatter();

        // 反串行化到内存中
        object obj = formatter.Deserialize(memStream);

        // 类型检验
        if (obj is DataSet)
        {
            DataSet dataSetResult = (DataSet)obj;
            return dataSetResult;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 将字典转换成字符串转换
    /// </summary>
    /// <returns>返回字符串,用逗号分割</returns>
    public static string DicToString(Dictionary<int, (string, int)> dics)
    {
        string returnValue = "";
        foreach (var item in dics)
        {
            returnValue += item.Key + ",";
            returnValue += item.Value.Item1 + ",";
            returnValue += item.Value.Item2 + ",";
        }

        return returnValue;
    }

    /// <summary>
    /// 将字符串转字典
    /// </summary>
    /// <returns>返回转换好的字典</returns>
    public static Dictionary<int, (string, int)> StringToDic(string data)
    {
        Dictionary<int, (string, int)> returnDic = new Dictionary<int, (string, int)>();
        int index = 0;
        string[] strs = data.Split(',');
        while (index < strs.Length - 1)
        {
            returnDic.Add(int.Parse(strs[index++]), (strs[index++], int.Parse(strs[index++])));
        }

        return returnDic;
    }

    /// <summary>
    /// string 转dataset
    /// </summary>
    /// <param name="s">dataset字符串</param>
    /// <returns>转换后的dataset</returns>
    public static DataSet DataSetFromString(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return null;
        }

        byte[] bytes = Encoding.UTF8.GetBytes(s);

        Stream inStream = null;
        DataSet ds = new DataSet();
        try
        {
            inStream = new MemoryStream(bytes);
            ds.ReadXml(inStream);
        }
        finally
        {
            inStream.Close();
        }

        return ds;
    }

    /// <summary>
    /// 将dataset转string
    /// </summary>
    /// <param name="ds">dataset数据</param>
    /// <returns>转换的string</returns>
    public static string GetStringFromTable(DataSet ds)
    {
        return ds.GetXml();
    }

    /// <summary>
    /// 将dataSet的值取出转换为List
    /// </summary>
    /// <returns></returns>
    public static List<List<string>> DataSetValueToList(DataSet ds)
    {
        List<List<string>> lists = new List<List<string>>();
        foreach (DataRow item in ds.Tables[0].Rows)
        {
            List<string> list = new List<string>();
            for (int i = 0; i < item.ItemArray.Length; i++)
            {
                list.Add(item.ItemArray[i].ToString());
            }
            lists.Add(list);
        }

        return lists;
    }
}
