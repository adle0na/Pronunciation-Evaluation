using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenAI;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class SettingUI : MonoBehaviour
{
    [SerializeField] private List<LanguageToggle> languageToggles;

    private bool isChanging;
    
    private void Start()
    {
        foreach (var toggle in languageToggles)
        {
            if (toggle.language == ChatGPTManager.Instance.learningLanguage)
            {
                toggle.SetOn();
            }
        }
    }

    public void GotoMain()
    {
        UIManager.Instance.PageChange(2);
    }

    public void SaveAndGotoMain()
    {
        // 세이브 함수
        ChatGPTManager.Instance.InitStudy();
        
        UIManager.Instance.PageChange(2);
    }

    public void ChangeSystemLanguage()
    {
        if (isChanging) return;

        StartCoroutine(ChangeSysmtemLanguageCor());
    }

    IEnumerator ChangeSysmtemLanguageCor()
    {
        isChanging = true;

        yield return LocalizationSettings.InitializationOperation;

        var locales = LocalizationSettings.AvailableLocales.Locales;

        // ChatGPTManager의 systemLanguage 값을 가져와서 해당하는 Locale 찾기
        string targetLanguageCode = GetLanguageCode(ChatGPTManager.Instance.systemLanguage);
        var targetLocale = locales.FirstOrDefault(locale => locale.Identifier.Code == targetLanguageCode);

        if (targetLocale != null)
        {
            LocalizationSettings.SelectedLocale = targetLocale;
            Debug.Log($"[Localization] Language changed to {targetLocale.Identifier.Code}");
        }
        else
        {
            Debug.LogWarning($"[Localization] Language {targetLanguageCode} not found in available locales!");
        }

        isChanging = false;
    }
    
    private string GetLanguageCode(Language language)
    {
        switch (language)
        {
            case Language.English: return "en";
            case Language.German: return "de";
            case Language.French: return "fr";
            case Language.Korean: return "ko";
            case Language.Japanese: return "ja";
            case Language.Chinese: return "zh";
            case Language.Italian: return "it";
            case Language.Russian: return "ru";
            default: return "en"; // 기본값: 영어
        }
    }
}
