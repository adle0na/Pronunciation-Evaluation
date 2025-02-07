using System;
using System.Collections;
using System.Collections.Generic;
using OpenAI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LanguageToggle : MonoBehaviour
{
    private Toggle toggle;
    [SerializeField] private TMP_Text languageText;

    public Language language;

    public bool isSystemLanguage;

    private void Start()
    {
        toggle = GetComponent<Toggle>();
    }

    public void OnValueChanged()
    {
        languageText.color = toggle.isOn ? Color.white : UIManager.Instance.darkGrayTextColor;

        if (toggle.isOn)
        {
            if (isSystemLanguage)
            {
                ChatGPTManager.Instance.systemLanguage = language;
            }
            else
            {
                ChatGPTManager.Instance.learningLanguage = language;
            }
        }
    }

    public void SetOn()
    {
        toggle.isOn = true;
    }
}
