using UnityEngine;
using UnityEditor;

public class SpriteAutoSlicer
{
    [MenuItem("Tools/Slice All Sprites In Folder")]
    static void SliceAll()
    {
        string folderPath = "Assets/05.Images/Test"; // 변경
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti != null)
            {
                ti.spriteImportMode = SpriteImportMode.Multiple;
                ti.isReadable = true;

                int cellSizeX = 64; // 셀 크기
                int cellSizeY = 64;

                int textureWidth = 0;
                int textureHeight = 0;

                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                textureWidth = tex.width;
                textureHeight = tex.height;

                int colCount = textureWidth / cellSizeX;
                int rowCount = textureHeight / cellSizeY;

                SpriteMetaData[] smd = new SpriteMetaData[colCount * rowCount];
                int index = 0;

                for (int y = rowCount - 1; y >= 0; y--)
                {
                    for (int x = 0; x < colCount; x++)
                    {
                        SpriteMetaData meta = new SpriteMetaData();
                        meta.rect = new Rect(x * cellSizeX, y * cellSizeY, cellSizeX, cellSizeY);
                        meta.name = tex.name + "_" + index;
                        meta.alignment = (int)SpriteAlignment.Center;
                        smd[index++] = meta;
                    }
                }

                ti.spritesheet = smd;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }

        Debug.Log("모든 스프라이트 자동 Slice 완료!");
    }
}