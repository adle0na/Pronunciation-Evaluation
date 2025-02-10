using System;
using System.Collections;
using System.Collections.Generic;
using OpenAI;
using UnityEngine;
using UnityEngine.UI;

public class MainUIPage : MonoBehaviour
{
    [SerializeField] private List<ThemaToggle> themaToggles;

    private void Start()
    {
        foreach (var toggle in themaToggles)
        {
            if (toggle.theme == ChatGPTManager.Instance.userSelectedTheme)
            {
                toggle.SetOn();
            }
        }
    }

    public void OnClickChangeTheme()
    {
        foreach (var toggle in themaToggles)
        {
            if (toggle.GetComponent<Toggle>().isOn)
            {
                ChatGPTManager.Instance.userSelectedTheme = toggle.theme;
            }
        }
    }
    
    public void GotoSetting()
    {
        UIManager.Instance.PageChange(1);
    }

    public void GotoStudy()
    {
        ChatGPTManager.Instance.InitStudy();
        
        UIManager.Instance.PageChange(3);
    }
}
