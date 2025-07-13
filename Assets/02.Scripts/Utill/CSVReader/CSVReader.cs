using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

public static class CSVReader
{
    public static List<T> Read<T>(string filePath) where T : new()
    {
        List<T> dataList = new List<T>();
        string fullPath = Path.Combine(Application.dataPath, filePath);

        if (!File.Exists(fullPath))
        {
            throw new Exception($"[CSV Reader] 파일을 찾을 수 없습니다!! || {fullPath}");
        }

        string[] lines = File.ReadAllLines(fullPath);
        if (lines.Length <= 1)
        {
            throw new Exception($"[CSV Reader] 파일에 데이터가 없습니다!! || {filePath}");
        }

        string[] headers = GetHeaders(lines[0]);

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = GetValues(lines[i]);

            if (values.Length != headers.Length)
            {
                throw new Exception($"[CSV Reader] 헤더와 데이터의 개수가 일치하지 않습니다!! {i}번 째 줄|| {filePath}");
            }

            T dataEntry = new T();
            Type type = typeof(T);

            for (int j = 0; j < headers.Length; j++)
            {
                FieldInfo fieldInfo = type.GetField(headers[j]);

                if (fieldInfo == null)
                {
                    throw new Exception($"[CSV Reader] '{typeof(T).Name}'에 '{headers[j]}' 필드가 없습니다.|| {filePath}");
                }

                try
                {
                    var convertedValue = Convert.ChangeType(values[j], fieldInfo.FieldType);
                    fieldInfo.SetValue(dataEntry, convertedValue);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[CSV Reader] {i}번 째 줄,'{headers[j]}' 필드의 값 '{values[j]}'을(를) 변환하는 중 오류 발생: {e.Message}");
                }
            }
            dataList.Add(dataEntry);
        }
        return dataList;
    }

    public static string[] GetHeaders(string headerLine)
    {
        string[] headers = headerLine.Split(',');
        for (int i = 0; i < headers.Length; i++)
        {
            headers[i] = headers[i].Split(':')[0].Trim();
        }

        return headers;
    }

    public static string[] GetValues(string valueLine)
    {
        string[] values = valueLine.Split(',');
        return values;
    }
}
