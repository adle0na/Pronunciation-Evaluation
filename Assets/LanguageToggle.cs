using System;
using System.Collections;
using System.Collections.Generic;
using OpenAI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LanguageToggle : MonoBehaviour
{
    public TMP_Text languageText;

    public Language language;

    public void SetTouchable(bool value)
    {
        GetComponent<Toggle>().interactable = value;
    }
}
