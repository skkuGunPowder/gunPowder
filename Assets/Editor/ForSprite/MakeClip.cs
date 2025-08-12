using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class MakeClip
{
    const float defaultFrameRate = 12f;

    [MenuItem("Tools/Sprite → AnimationClips (Per Texture)")]
    static void GenerateClipsFromFolders()
    {
        List<string> folders = new List<string>();
        while (true)
        {
            string folderPath = EditorUtility.OpenFolderPanel("스프라이트 폴더 선택 (취소하면 종료)", Application.dataPath, "");
            if (string.IsNullOrEmpty(folderPath)) break;
            if (!folderPath.StartsWith(Application.dataPath))
            {
                Debug.LogWarning("프로젝트 내부(Assets) 폴더를 선택해 주세요: " + folderPath);
                continue;
            }
            string relative = "Assets" + folderPath.Substring(Application.dataPath.Length);
            folders.Add(relative);
        }

        if (folders.Count == 0)
        {
            Debug.LogWarning("선택된 폴더가 없습니다.");
            return;
        }

        int created = 0;
        foreach (var folder in folders)
        {
            created += ProcessFolder(folder);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"완료: {folders.Count} 폴더 처리, {created}개의 .anim 생성");
    }

    static int ProcessFolder(string relativePath)
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { relativePath });
        if (guids == null || guids.Length == 0)
        {
            Debug.LogWarning($"폴더에 Texture2D가 없습니다: {relativePath}");
            return 0;
        }

        int createdCount = 0;
        foreach (var guid in guids)
        {
            string texPath = AssetDatabase.GUIDToAssetPath(guid);
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(texPath);
            List<Sprite> sprites = new List<Sprite>();
            foreach (var a in assets)
            {
                if (a is Sprite sp) sprites.Add(sp);
            }

            if (sprites.Count == 0) continue;

            // 정렬: 위에서 아래 (y desc), 왼쪽에서 오른쪽 (x asc), 마지막으로 이름
            sprites.Sort((a,b) => {
                int c = -a.rect.y.CompareTo(b.rect.y);
                if (c != 0) return c;
                c = a.rect.x.CompareTo(b.rect.x);
                if (c != 0) return c;
                return a.name.CompareTo(b.name);
            });

            AnimationClip clip = new AnimationClip();
            clip.frameRate = defaultFrameRate;

            var binding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            var keyframes = new ObjectReferenceKeyframe[sprites.Count];
            for (int i = 0; i < sprites.Count; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i / clip.frameRate,
                    value = sprites[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            string dir = Path.GetDirectoryName(texPath).Replace('\\', '/');
            string texName = Path.GetFileNameWithoutExtension(texPath);
            string savePath = $"{dir}/{texName}.anim";

            // 덮어쓰기: 기존에 있으면 삭제
            var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(savePath);
            if (existing != null) AssetDatabase.DeleteAsset(savePath);

            AssetDatabase.CreateAsset(clip, savePath);
            Debug.Log($"생성: {savePath} (원본: {texPath}, frames: {sprites.Count})");
            createdCount++;
        }

        return createdCount;
    }
}

