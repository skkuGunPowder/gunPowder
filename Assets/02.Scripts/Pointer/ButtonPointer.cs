using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
public class ButtonPointer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject SelectedMask;
    public TextMeshProUGUI ButtonText;
    
    [Header("Tweening")]
    public float Speed = 0.2f;
    public Ease EaseType = Ease.OutCubic;
    public bool NoText = false;

    private void Awake()
    {
        if (ButtonText == null)
        {
            NoText = true;
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        SelectedMask.SetActive(true);
        if (NoText)
        {
            return;
        }
        ButtonText.fontStyle = FontStyles.Bold;
        ButtonText.color = ColorPalette.ColorDictionary[EColorType.SelectedButton];
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        SelectedMask.SetActive(false);
        if (NoText)
        {
            return;
        }
        ButtonText.fontStyle = FontStyles.Normal;
        ButtonText.color = ColorPalette.ColorDictionary[EColorType.UnSelectedButton];
    }
    
    public void OnDestroy()
    {
    }
}
