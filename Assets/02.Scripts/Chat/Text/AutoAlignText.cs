using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class AutoAlignText : MonoBehaviour
{
    private TextMeshProUGUI _textMeshPro;

    private void Awake()
    {
        _textMeshPro = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        // 켜질 때마다 정렬 갱신
        RefreshAlignment();
    }

    // 외부에서 텍스트를 바꿀 때 이 함수를 호출해줘도 됩니다.
    public void RefreshAlignment()
    {
        if (_textMeshPro == null) return;

        // 1. 렌더링 전 강제 업데이트 (줄 수 계산을 위해 필수)
        _textMeshPro.ForceMeshUpdate();

        // 2. 줄 수에 따른 분기
        if (_textMeshPro.textInfo.lineCount <= 1)
        {
            _textMeshPro.alignment = TextAlignmentOptions.Midline; // 중앙
        }
        else
        {
            _textMeshPro.alignment = TextAlignmentOptions.TopLeft; // 좌상단
        }
    }
    
    // 텍스트가 바뀔 때 자동으로 감지하고 싶다면 Update나 LateUpdate를 쓸 수도 있지만, 
    // 성능을 위해 SetText 직후에 RefreshAlignment()를 호출하는 것을 권장합니다.
}