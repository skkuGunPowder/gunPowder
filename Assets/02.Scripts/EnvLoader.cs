using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class EnvLoader
{
    private static Dictionary<string, string> envValues = new Dictionary<string, string>();

    public static void LoadEnv(string path)
    {
        envValues.Clear();

        if (!File.Exists(path))
        {
            Debug.LogWarning($".env 파일이 존재하지 않습니다: {path}");
            return;
        }

        var lines = File.ReadAllLines(path);
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

            var parts = line.Split('=');
            if (parts.Length != 2) continue;

            var key = parts[0].Trim();
            var value = parts[1].Trim();
            envValues[key] = value;
        }

        Debug.Log(".env 파일 로드 완료");
    }

    public static string Get(string key, string defaultValue = "")
    {
        return envValues.TryGetValue(key, out var value) ? value : defaultValue;
    }
}