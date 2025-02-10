using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenAI;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
    [SerializeField] private List<LanguageToggle> targetLanguageToggles;

    [SerializeField] private List<LanguageToggle> systemLanguageToggles;

    private void Start()
    {
        foreach (var toggle in targetLanguageToggles)
        {
            if (toggle.language == ChatGPTManager.Instance.learningLanguage)
            {
                toggle.GetComponent<Toggle>().isOn = true;
            }
        }

        foreach (var toggle in systemLanguageToggles)
        {
            if (toggle.language == ChatGPTManager.Instance.systemLanguage)
            {
                toggle.GetComponent<Toggle>().isOn = true;
            }
        }
        
        ChangeTargetLanguage();

        ChangeSystemLanguage();
    }

    public void GotoMain()
    {
        UIManager.Instance.PageChange(2);
    }

    public void SaveAndGotoMain()
    {
        UIManager.Instance.PageChange(2);
    }

    public void ChangeTargetLanguage()
    {
        foreach (var toggle in targetLanguageToggles)
        {
            if (toggle.GetComponent<Toggle>().isOn)
            {
                ChatGPTManager.Instance.learningLanguage = toggle.language;
            }

            toggle.languageText.color = toggle.GetComponent<Toggle>().isOn
                ? Color.white
                : UIManager.Instance.darkGrayTextColor;
        }

        foreach (var toggle in systemLanguageToggles)
        {
            toggle.SetTouchable(toggle.language != ChatGPTManager.Instance.learningLanguage);
        }
    }
    
    public void ChangeSystemLanguage()
    {
        StartCoroutine(ChangeSysmtemLanguageCor());
    }

    IEnumerator ChangeSysmtemLanguageCor()
    {
        foreach (var toggle in systemLanguageToggles)
        {
            toggle.GetComponent<Toggle>().interactable = false;
        }
        
        foreach (var toggle in systemLanguageToggles)
        {
            if (toggle.GetComponent<Toggle>().isOn)
            {
                ChatGPTManager.Instance.systemLanguage = toggle.language;
            }

            toggle.languageText.color = toggle.GetComponent<Toggle>().isOn
                ? Color.white
                : UIManager.Instance.darkGrayTextColor;
        }
        
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

        foreach (var toggle in systemLanguageToggles)
        {
            toggle.SetTouchable(toggle.language != ChatGPTManager.Instance.learningLanguage);
        }
        
        foreach (var toggle in targetLanguageToggles)
        {
            toggle.SetTouchable(toggle.language != ChatGPTManager.Instance.systemLanguage);
        }
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
