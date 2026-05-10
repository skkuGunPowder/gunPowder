using System;
using System.Collections.Generic;
using UnityEngine;

public enum ELocale
{
    KR,
    EN,
    JP,
}


public class TextManager : DontDestroySingleton<TextManager>
{
    public event Action<ELocale> LocaleChanged;
    public event Action TextDataLoaded;

    private ELocale currentLocale;
    private TextRepository textRepository;
    private Dictionary<string, TextData> textData;

    override protected void Awake()
    {
        base.Awake();
        textRepository = new TextRepository();
        currentLocale = ELocale.KR;
        // LoadTextDataAsync();
    }

    public async void LoadTextDataAsync()
    {
        textData = await textRepository.LoadedTextData();
        TextDataLoaded?.Invoke();
        LocaleChanged?.Invoke(currentLocale);
    }

    public void SetLocale(ELocale locale)
    {
        if (currentLocale == locale)
        {
            return;
        }

        currentLocale = locale;
        LocaleChanged?.Invoke(currentLocale);
    }

    public bool TryGetText(string id, out string localizedText, bool logIfMissing = false)
    {
        localizedText = string.Empty;

        if (string.IsNullOrEmpty(id))
        {
            return false;
        }

        if (textData == null)
        {
            return false;
        }

        if (!textData.TryGetValue(id, out TextData data))
        {
            if (logIfMissing)
            {
                Debug.LogError($"텍스트 데이터가 존재하지 않습니다: {id}");
            }

            return false;
        }

        localizedText = currentLocale switch
        {
            ELocale.KR => data.KR,
            ELocale.EN => data.EN,
            ELocale.JP => data.JP,
            _ => data.KR
        };

        return true;
    }

    public string GetText(string id)
    {
        if (!TryGetText(id, out string localizedText, true))
        {
            return string.Empty;
        }

        return localizedText;
    }


    // 테스트용 메서드
    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.F1))
        {
            SetLocale(ELocale.KR);
        }
        else if(Input.GetKeyDown(KeyCode.F2))
        {
            SetLocale(ELocale.EN);
        }
        else if(Input.GetKeyDown(KeyCode.F3))
        {
            SetLocale(ELocale.JP);
        }
    }
}
