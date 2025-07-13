using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class CSVConverter : EditorWindow
{
    private string csvPath = "Assets/13.CSV";
    private string outputPath = "Assets/02.Scripts";

    [MenuItem("Tools/GunPowder/CSVConverter")]
    private static void ShowWindow()
    {
        var window = GetWindow<CSVConverter>();
        window.titleContent = new GUIContent("CSVConverter");
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("CSV to C# 변환기", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        csvPath = EditorGUILayout.TextField("CSV 파일 경로", csvPath);
        if (GUILayout.Button("browse...", GUILayout.Width(80)))
        {
            string path = EditorUtility.OpenFilePanel("CSV 파일 선택", Application.dataPath, "csv");
            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith(Application.dataPath))
                {
                    csvPath = "Assets" + path.Substring(Application.dataPath.Length);
                }
                else
                {
                    Debug.LogWarning("프로젝트 외부의 파일은 선택할 수 없습니다. Asset 폴더 내에 파일을 넣어주세요.");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        outputPath = EditorGUILayout.TextField("출력 경로", outputPath);

        if (GUILayout.Button("도메인 생성"))
        {
            GenerateClass();
        }
    }

    private void GenerateClass()
    {
        if (string.IsNullOrEmpty(csvPath) || !File.Exists(csvPath))
        {
            EditorUtility.DisplayDialog("Error", "CSV 파일 경로가 잘못되었거나 존재하지 않습니다.", "OK");
            return;
        }

        string headerLine;
        try
        {
            headerLine = File.ReadAllLines(csvPath)[0];
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"CSV 파일을 읽는 중 에러 발생: {e.Message}", "OK");
            return;
        }

        string[] headers = headerLine.Split(',');
        string className = Path.GetFileNameWithoutExtension(csvPath);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine();
        sb.AppendLine($"public class {className}");
        sb.AppendLine("{");

        foreach (string header in headers)
        {
            string[] parts = header.Trim().Split(':');
            if (parts.Length != 2)
            {
                Debug.LogError($"헤더 형식이 잘못되었습니다(변수명:자료형). || {header}");
                continue;
            }

            string name = parts[0].Trim();
            string type = parts[1].Trim();

            sb.AppendLine($"    public {type} {name};");
        }
        sb.AppendLine("}");

        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        string pathWithName = Path.Combine(outputPath, $"{className}.cs");
        File.WriteAllText(pathWithName, sb.ToString());

        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Success", $"'{className}.cs' 생성 완료", "OK");

        Object obj = AssetDatabase.LoadAssetAtPath<Object>(pathWithName);
        EditorGUIUtility.PingObject(obj);
    }
}
