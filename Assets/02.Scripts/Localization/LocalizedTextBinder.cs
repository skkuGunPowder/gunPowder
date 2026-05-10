using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedTextBinder : MonoBehaviour
{
    [SerializeField] private string textId;
    [SerializeField] private string localizationIdPrefix = "TX";
    [SerializeField] private bool autoUseCurrentTextAsId = true;

    private TextMeshProUGUI targetText;

    private void Awake()
    {
        targetText = GetComponent<TextMeshProUGUI>();
    }

    private void Reset()
    {
        targetText = GetComponent<TextMeshProUGUI>();
        InitializeFromCurrentText();
    }

    private void OnEnable()
    {
        if (targetText == null)
        {
            targetText = GetComponent<TextMeshProUGUI>();
        }

        SubscribeToTextManager();
        RefreshText();
    }

    private void OnDisable()
    {
        UnsubscribeFromTextManager();
    }

    private void OnValidate()
    {
        if (targetText == null)
        {
            targetText = GetComponent<TextMeshProUGUI>();
        }

        if (autoUseCurrentTextAsId)
        {
            InitializeFromCurrentText();
        }
    }

    public void InitializeFromCurrentText()
    {
        if (!string.IsNullOrEmpty(textId))
        {
            return;
        }

        if (targetText == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(targetText.text))
        {
            return;
        }

        if (!TryNormalizeLocalizableId(targetText.text, out string normalizedId))
        {
            return;
        }

        textId = normalizedId;
    }

    public void SetTextId(string id, bool refreshImmediately = true)
    {
        textId = id;

        if (refreshImmediately)
        {
            RefreshText();
        }
    }

    public void RefreshText()
    {
        if (targetText == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(textId))
        {
            if (!autoUseCurrentTextAsId)
            {
                return;
            }

            InitializeFromCurrentText();
            if (string.IsNullOrEmpty(textId))
            {
                return;
            }
        }

        if (!TryNormalizeLocalizableId(textId, out string normalizedTextId))
        {
            return;
        }

        textId = normalizedTextId;

        TextManager manager = TextManager.Instance;
        if (manager == null)
        {
            return;
        }

        if (manager.TryGetText(normalizedTextId, out string localizedText))
        {
            targetText.text = localizedText;
        }
    }

    private void SubscribeToTextManager()
    {
        TextManager manager = TextManager.Instance;
        if (manager == null)
        {
            return;
        }

        manager.LocaleChanged -= OnLocaleChanged;
        manager.TextDataLoaded -= OnTextDataLoaded;
        manager.LocaleChanged += OnLocaleChanged;
        manager.TextDataLoaded += OnTextDataLoaded;
    }

    private void UnsubscribeFromTextManager()
    {
        TextManager manager = FindAnyObjectByType<TextManager>();
        if (manager == null)
        {
            return;
        }

        manager.LocaleChanged -= OnLocaleChanged;
        manager.TextDataLoaded -= OnTextDataLoaded;
    }

    private void OnLocaleChanged(ELocale locale)
    {
        RefreshText();
    }

    private void OnTextDataLoaded()
    {
        RefreshText();
    }

    private bool TryNormalizeLocalizableId(string id, out string normalizedId)
    {
        normalizedId = string.Empty;

        if (string.IsNullOrEmpty(id))
        {
            return false;
        }

        string candidate = NormalizeTextForIdCheck(id);
        if (string.IsNullOrEmpty(candidate))
        {
            return false;
        }

        if (string.IsNullOrEmpty(localizationIdPrefix))
        {
            normalizedId = candidate;
            return true;
        }

        if (!candidate.StartsWith(localizationIdPrefix, System.StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        normalizedId = candidate;
        return true;
    }

    private string NormalizeTextForIdCheck(string source)
    {
        if (string.IsNullOrEmpty(source))
        {
            return string.Empty;
        }

        string withoutTags = RemoveRichTextTags(source);
        string withoutZeroWidth = withoutTags.Replace("\u200B", string.Empty).Replace("\uFEFF", string.Empty);
        return withoutZeroWidth.Trim();
    }

    private string RemoveRichTextTags(string source)
    {
        if (string.IsNullOrEmpty(source))
        {
            return string.Empty;
        }

        System.Text.StringBuilder builder = new System.Text.StringBuilder(source.Length);
        bool isInsideTag = false;

        for (int i = 0; i < source.Length; i++)
        {
            char c = source[i];
            if (c == '<')
            {
                isInsideTag = true;
                continue;
            }

            if (c == '>')
            {
                isInsideTag = false;
                continue;
            }

            if (!isInsideTag)
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }
}
