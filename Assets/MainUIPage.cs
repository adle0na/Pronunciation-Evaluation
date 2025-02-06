using System;
using System.Collections;
using System.Collections.Generic;
using OpenAI;
using UnityEngine;

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

    public void GotoSetting()
    {
        UIManager.Instance.PageChange(1);
    }

    public void GotoStudy()
    {
        UIManager.Instance.PageChange(3);
    }
}
