// Assets/Editor/SpritePPUBatchSetter.cs
#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SpritePPUBatchSetter : EditorWindow
{
    private DefaultAsset targetFolder;
    private float pixelsPerUnit = 100f;
    private bool includeSubfolders = true;

    [MenuItem("Tools/Sprites/Set PPU for Selected Folder...")]
    private static void Open()
    {
        var w = GetWindow<SpritePPUBatchSetter>("Sprite PPU Batch Setter");
        w.minSize = new Vector2(420, 160);
        w.TryAutoPickSelectedFolder();
        w.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(6);

        EditorGUILayout.LabelField("Target", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            targetFolder = (DefaultAsset)EditorGUILayout.ObjectField(
                "Folder", targetFolder, typeof(DefaultAsset), false);

            if (GUILayout.Button("Use Selection", GUILayout.Width(120)))
                TryAutoPickSelectedFolder();
        }

        pixelsPerUnit = EditorGUILayout.FloatField("Pixels Per Unit", pixelsPerUnit);
        includeSubfolders = EditorGUILayout.Toggle("Include Subfolders", includeSubfolders);

        EditorGUILayout.Space(10);

        using (new EditorGUI.DisabledScope(!IsValidFolder(targetFolder) || pixelsPerUnit <= 0f))
        {
            if (GUILayout.Button("Apply PPU to Sprites", GUILayout.Height(34)))
                Apply();
        }

        EditorGUILayout.Space(6);
        EditorGUILayout.HelpBox(
            "This will change TextureImporter.spritePixelsPerUnit for all Sprite textures under the folder.",
            MessageType.Info);
    }

    private void TryAutoPickSelectedFolder()
    {
        var obj = Selection.activeObject;
        if (obj == null) return;

        // If a folder is selected, Selection.activeObject is often DefaultAsset.
        if (obj is DefaultAsset da && IsValidFolder(da))
        {
            targetFolder = da;
            return;
        }

        // If a file is selected, try to use its directory.
        var path = AssetDatabase.GetAssetPath(obj);
        if (!string.IsNullOrEmpty(path))
        {
            var dir = Directory.Exists(path) ? path : Path.GetDirectoryName(path)?.Replace("\\", "/");
            if (!string.IsNullOrEmpty(dir))
            {
                var folderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(dir);
                if (IsValidFolder(folderAsset))
                    targetFolder = folderAsset;
            }
        }
    }

    private static bool IsValidFolder(DefaultAsset folderAsset)
    {
        if (folderAsset == null) return false;
        var path = AssetDatabase.GetAssetPath(folderAsset);
        return !string.IsNullOrEmpty(path) && AssetDatabase.IsValidFolder(path);
    }

    private void Apply()
    {
        var folderPath = AssetDatabase.GetAssetPath(targetFolder);
        var searchFolders = new[] { folderPath };

        // Get all Texture assets under folder
        var guids = AssetDatabase.FindAssets("t:Texture2D", searchFolders);

        int changed = 0;
        int skippedNotSprite = 0;

        try
        {
            AssetDatabase.StartAssetEditing();

            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);

                // If not including subfolders, skip deeper paths.
                if (!includeSubfolders)
                {
                    var parent = Path.GetDirectoryName(assetPath)?.Replace("\\", "/");
                    if (!string.Equals(parent, folderPath))
                        continue;
                }

                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null) continue;

                // Only sprites
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    skippedNotSprite++;
                    continue;
                }

                // If same value, skip
                if (Mathf.Approximately(importer.spritePixelsPerUnit, pixelsPerUnit))
                    continue;

                importer.spritePixelsPerUnit = pixelsPerUnit;

                // Keep current settings, just apply changes
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
                changed++;
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }

        Debug.Log($"[SpritePPU] Done. Changed: {changed}, Skipped (not Sprite): {skippedNotSprite}");
        EditorUtility.DisplayDialog("Sprite PPU Batch Setter",
            $"Done.\nChanged: {changed}\nSkipped (not Sprite): {skippedNotSprite}", "OK");
    }
}
#endif
