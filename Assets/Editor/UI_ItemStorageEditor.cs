using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UI_ItemStorage))]
public class UI_ItemStorageEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("ItemNameText"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("EquipmentSlot"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("SubEquipmentSlot"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("UI_Category"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_itemSlotList"));

        SerializedProperty isFixed = serializedObject.FindProperty("_isFixed");
        EditorGUILayout.PropertyField(isFixed);

        if (isFixed.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_fixedMainCategory"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_fixedItemType"));
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
