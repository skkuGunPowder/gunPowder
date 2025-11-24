using UnityEngine;
using UnityEngine.UI;

public class TextTruncateByWidth : MonoBehaviour
{
    public Text textComponent;
    
    public void SetText(string fullText)
    {
        textComponent.text = fullText;
        
        // TextGenerator를 사용해 실제 렌더링 크기 확인
        TextGenerator textGen = new TextGenerator();
        TextGenerationSettings settings = textComponent.GetGenerationSettings(textComponent.rectTransform.rect.size);
        
        float width = textGen.GetPreferredWidth(fullText, settings);
        float maxWidth = textComponent.rectTransform.rect.width;
        
        if (width > maxWidth)
        {
            // 한 글자씩 줄여가며 맞는 길이 찾기
            for (int i = fullText.Length - 1; i > 0; i--)
            {
                string truncated = fullText.Substring(0, i) + "...";
                width = textGen.GetPreferredWidth(truncated, settings);
                
                if (width <= maxWidth)
                {
                    textComponent.text = truncated;
                    return;
                }
            }
        }
    }
}