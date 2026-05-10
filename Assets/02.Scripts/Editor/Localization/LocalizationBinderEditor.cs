using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;

public static class LocalizationBinderEditor
{
    private const string LocalizationIdPrefix = "TX";
    private static readonly Regex RichTextTagRegex = new Regex("<[^>]+>", RegexOptions.Compiled);

    [MenuItem("Tools/Localization/Binders/Add To Selected Hierarchies")]
    private static void AddToSelectedHierarchies()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            Debug.LogWarning("선택된 오브젝트가 없습니다.");
            return;
        }

        int addedCount = 0;
        int totalTextCount = 0;

        for (int i = 0; i < selectedObjects.Length; i++)
        {
            TextMeshProUGUI[] texts = selectedObjects[i].GetComponentsInChildren<TextMeshProUGUI>(true);
            totalTextCount += texts.Length;
            addedCount += AddBindersToTexts(texts);
        }

        Debug.Log($"LocalizedTextBinder 추가 완료: {addedCount}개 추가 / 대상 텍스트 {totalTextCount}개");
    }

    [MenuItem("Tools/Localization/Binders/Add To Open Scenes")]
    private static void AddToOpenScenes()
    {
        TextMeshProUGUI[] allTexts = UnityEngine.Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int addedCount = AddBindersToTexts(allTexts);

        Debug.Log($"Open Scene LocalizedTextBinder 추가 완료: {addedCount}개 추가 / 대상 텍스트 {allTexts.Length}개");
    }

    [MenuItem("Tools/Localization/Binders/Add To All Prefabs In Project")]
    private static void AddToAllPrefabsInProject()
    {
        bool confirm = EditorUtility.DisplayDialog(
            "Localization Binder",
            "프로젝트의 모든 프리팹을 검사해서 LocalizedTextBinder를 추가합니다. 계속하시겠습니까?",
            "진행",
            "취소");

        if (!confirm)
        {
            return;
        }

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        int addedCount = 0;
        int prefabChangedCount = 0;

        for (int i = 0; i < prefabGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            TextMeshProUGUI[] texts = root.GetComponentsInChildren<TextMeshProUGUI>(true);

            int addedInPrefab = AddBindersToTexts(texts, false);
            if (addedInPrefab > 0)
            {
                PrefabUtility.SaveAsPrefabAsset(root, path);
                prefabChangedCount++;
                addedCount += addedInPrefab;
            }

            PrefabUtility.UnloadPrefabContents(root);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Prefab LocalizedTextBinder 추가 완료: {addedCount}개 추가 / 변경된 프리팹 {prefabChangedCount}개");
    }

    [MenuItem("Tools/Localization/Binders/Remove Non-TX From Selected Hierarchies")]
    private static void RemoveNonTxFromSelectedHierarchies()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            Debug.LogWarning("선택된 오브젝트가 없습니다.");
            return;
        }

        int removedCount = 0;

        for (int i = 0; i < selectedObjects.Length; i++)
        {
            LocalizedTextBinder[] binders = selectedObjects[i].GetComponentsInChildren<LocalizedTextBinder>(true);
            removedCount += RemoveNonTxBinders(binders);
        }

        Debug.Log($"Selected Hierarchies non-TX LocalizedTextBinder 제거 완료: {removedCount}개 제거");
    }

    [MenuItem("Tools/Localization/Binders/Remove Non-TX From Open Scenes")]
    private static void RemoveNonTxFromOpenScenes()
    {
        LocalizedTextBinder[] binders = UnityEngine.Object.FindObjectsByType<LocalizedTextBinder>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int removedCount = RemoveNonTxBinders(binders);

        Debug.Log($"Open Scene non-TX LocalizedTextBinder 제거 완료: {removedCount}개 제거");
    }

    [MenuItem("Tools/Localization/Binders/Remove Non-TX From All Prefabs In Project")]
    private static void RemoveNonTxFromAllPrefabsInProject()
    {
        bool confirm = EditorUtility.DisplayDialog(
            "Localization Binder",
            "프로젝트의 모든 프리팹에서 non-TX 텍스트에 붙은 LocalizedTextBinder를 제거합니다. 계속하시겠습니까?",
            "진행",
            "취소");

        if (!confirm)
        {
            return;
        }

        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        int removedCount = 0;
        int prefabChangedCount = 0;

        for (int i = 0; i < prefabGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            LocalizedTextBinder[] binders = root.GetComponentsInChildren<LocalizedTextBinder>(true);

            int removedInPrefab = RemoveNonTxBinders(binders, false);
            if (removedInPrefab > 0)
            {
                PrefabUtility.SaveAsPrefabAsset(root, path);
                prefabChangedCount++;
                removedCount += removedInPrefab;
            }

            PrefabUtility.UnloadPrefabContents(root);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Prefab non-TX LocalizedTextBinder 제거 완료: {removedCount}개 제거 / 변경된 프리팹 {prefabChangedCount}개");
    }

    private static int AddBindersToTexts(TextMeshProUGUI[] texts, bool registerUndo = true)
    {
        int addedCount = 0;

        for (int i = 0; i < texts.Length; i++)
        {
            TextMeshProUGUI text = texts[i];
            if (text == null)
            {
                continue;
            }

            if (!ShouldAddBinder(text))
            {
                continue;
            }

            if (!TryGetLocalizationIdFromText(text, out string localizationId))
            {
                continue;
            }

            LocalizedTextBinder binder = text.GetComponent<LocalizedTextBinder>();
            if (binder != null)
            {
                continue;
            }

            if (registerUndo)
            {
                binder = Undo.AddComponent<LocalizedTextBinder>(text.gameObject);
            }
            else
            {
                binder = text.gameObject.AddComponent<LocalizedTextBinder>();
            }

            if (binder == null)
            {
                continue;
            }

            binder.SetTextId(localizationId, false);
            EditorUtility.SetDirty(text.gameObject);
            addedCount++;
        }

        return addedCount;
    }

    private static int RemoveNonTxBinders(LocalizedTextBinder[] binders, bool registerUndo = true)
    {
        int removedCount = 0;

        for (int i = 0; i < binders.Length; i++)
        {
            LocalizedTextBinder binder = binders[i];
            if (binder == null)
            {
                continue;
            }

            TextMeshProUGUI text = binder.GetComponent<TextMeshProUGUI>();
            bool shouldKeep = ShouldAddBinder(text);
            if (shouldKeep)
            {
                continue;
            }

            if (registerUndo)
            {
                Undo.DestroyObjectImmediate(binder);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(binder);
            }

            EditorUtility.SetDirty(text != null ? text.gameObject : null);
            removedCount++;
        }

        return removedCount;
    }

    private static bool ShouldAddBinder(TextMeshProUGUI text)
    {
        return TryGetLocalizationIdFromText(text, out _);
    }

    private static bool TryGetLocalizationIdFromText(TextMeshProUGUI text, out string localizationId)
    {
        localizationId = string.Empty;

        if (text == null)
        {
            return false;
        }

        if (string.IsNullOrEmpty(text.text))
        {
            return false;
        }

        string normalized = NormalizeTextForIdCheck(text.text);
        if (string.IsNullOrEmpty(normalized))
        {
            return false;
        }

        if (!normalized.StartsWith(LocalizationIdPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        localizationId = normalized;
        return true;
    }

    private static string NormalizeTextForIdCheck(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        string withoutTags = RichTextTagRegex.Replace(text, string.Empty);
        string withoutZeroWidth = withoutTags.Replace("\u200B", string.Empty).Replace("\uFEFF", string.Empty);
        return withoutZeroWidth.Trim();
    }
}
