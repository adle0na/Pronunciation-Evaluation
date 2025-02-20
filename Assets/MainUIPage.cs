using System;
using System.Collections;
using System.Collections.Generic;
using OpenAI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainUIPage : MonoBehaviour
{
    [SerializeField] private List<ThemaToggle> themaToggles;
    [SerializeField] private TMP_Text difficultyText;
    
    private int difficulty;
    
    private void Start()
    {
        if (PlayerPrefs.HasKey("difficulty"))
        {
            difficulty = PlayerPrefs.GetInt("difficulty");
        }
        else
        {
            difficulty = 1;
            
            PlayerPrefs.SetInt("difficulty", difficulty);
        }
        
        difficultyText.text = difficulty.ToString();

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
                
                toggle.languageText.color = Color.white;
            }
            else
            {
                toggle.languageText.color = UIManager.Instance.darkGrayTextColor;
            }
        }
    }
    
    public void GotoSetting()
    {
        if (Microphone.devices.Length <= 0)
        {
            Debug.LogError("기기에 연결된 마이크가 없습니다.");
            return;
        }
        
        UIManager.Instance.PageChange(1);
    }

    public void GotoStudy()
    {
        ChatGPTManager.Instance.InitStudy();
        
        UIManager.Instance.PageChange(3);
    }

    public void SetDifficulty(bool isPlus)
    {
        difficulty = isPlus ? Mathf.Min(difficulty += 1, 5) : Mathf.Max(difficulty -= 1, 1);

        PlayerPrefs.SetInt("difficulty", difficulty);
        
        difficultyText.text = difficulty.ToString();
    }
}
