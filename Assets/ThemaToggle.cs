using System.Collections;
using System.Collections.Generic;
using OpenAI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ThemaToggle : MonoBehaviour
{
    private Toggle toggle;
    [SerializeField] private TMP_Text languageText;

    public Theme theme;
    
    private void Start()
    {
        toggle = GetComponent<Toggle>();
    }

    public void OnValueChanged()
    {
        languageText.color = toggle.isOn ? Color.white : UIManager.Instance.darkGrayTextColor;

        if (toggle.isOn)
        {
            ChatGPTManager.Instance.userSelectedTheme = theme;
        }
    }
    
    public void SetOn()
    {
        toggle.isOn = true;
    }
}
