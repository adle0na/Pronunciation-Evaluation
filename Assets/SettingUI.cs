using System;
using System.Collections;
using System.Collections.Generic;
using OpenAI;
using UnityEngine;

public class SettingUI : MonoBehaviour
{
    [SerializeField] private List<LanguageToggle> languageToggles;
    
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
}
