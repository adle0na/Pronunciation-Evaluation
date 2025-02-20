using System.Collections;
using System.Collections.Generic;
using OpenAI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ThemaToggle : MonoBehaviour
{
    private Toggle toggle;
    public TMP_Text languageText;

    public Theme theme;

    public void OnValueChanged()
    {
        toggle = GetComponent<Toggle>();

        if (toggle.isOn)
        {
            ChatGPTManager.Instance.userSelectedTheme = theme;
        }
    }
    
    public void SetOn()
    {
        toggle = GetComponent<Toggle>();
        
        toggle.isOn = true;
    }
}
