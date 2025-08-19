using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class SpriteToUIImageAnimationBatchConverter : EditorWindow
{
    [SerializeField] private Object[] sourceClips; // <- Serialized 가능하게 [SerializeField] 붙임

    [MenuItem("Tools/Batch Convert SpriteRenderer Animation to UI Image Animation")]
    static void OpenWindow()
    {
        GetWindow<SpriteToUIImageAnimationBatchConverter>("Batch Sprite→UI Image Converter");
    }

    void OnGUI()
    {
        GUILayout.Label("SpriteRenderer → UI.Image 변환 (여러 개)", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox("여러 AnimationClip을 Project 창에서 드래그해서 넣으세요.", MessageType.Info);

        // 배열을 SerializedProperty로 그리기
        SerializedObject so = new SerializedObject(this);
        SerializedProperty clipsProperty = so.FindProperty("sourceClips");
        EditorGUILayout.PropertyField(clipsProperty, true);
        so.ApplyModifiedProperties();

        if (sourceClips != null && sourceClips.Length > 0)
        {
            if (GUILayout.Button("Convert All"))
            {
                ConvertClips(sourceClips);
            }
        }
    }

    void ConvertClips(Object[] clips)
    {
        int convertedCount = 0;

        foreach (var obj in clips)
        {
            if (obj is AnimationClip clip)
            {
                string path = AssetDatabase.GetAssetPath(clip);
                string newPath = path.Replace(".anim", "_UI.anim");

                AnimationClip newClip = new AnimationClip();
                newClip.frameRate = clip.frameRate;

                var bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
                foreach (var binding in bindings)
                {
                    if (binding.propertyName == "m_Sprite") // SpriteRenderer.sprite
                    {
                        var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);

                        // 새 binding (Image.sprite)
                        EditorCurveBinding newBinding = new EditorCurveBinding
                        {
                            path = binding.path,
                            type = typeof(Image),
                            propertyName = "m_Sprite"
                        };

                        AnimationUtility.SetObjectReferenceCurve(newClip, newBinding, keyframes);
                    }
                }

                AssetDatabase.CreateAsset(newClip, newPath);
                convertedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("완료", $"{convertedCount}개의 애니메이션이 변환되었습니다.", "확인");
    }
}
