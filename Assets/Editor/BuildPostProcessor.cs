using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using UnityEngine;

public class BuildPostProcessor
{
    [PostProcessBuild(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.StandaloneWindows || target == BuildTarget.StandaloneWindows64)
        {
            // .env 파일 경로
            string envSourcePath = Application.dataPath + "/.env";
            string buildDirectory = Path.GetDirectoryName(pathToBuiltProject);
            string envDestPath = Path.Combine(buildDirectory, ".env");

            // .env 파일이 존재하는지 확인
            if (File.Exists(envSourcePath))
            {
                // 빌드 폴더에 .env 파일 복사
                File.Copy(envSourcePath, envDestPath, true);
                Debug.Log($"Copied .env file to: {envDestPath}");
            }
            else
            {
                Debug.LogWarning(".env file not found at: " + envSourcePath);
            }
        }
    }
}