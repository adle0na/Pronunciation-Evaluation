using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadingText : MonoBehaviour
{
    private TMP_Text target;
    private string originText = "";
    private int dotCount = 0;
    private void Start()
    {
        target = GetComponent<TMP_Text>();
        
        originText = GetComponent<TMP_Text>().text;
        
        StartCoroutine(AnimateLoadingText());
    }

    private IEnumerator AnimateLoadingText()
    {
        while (true)
        {
            target.text = originText + new string('.', dotCount);
            dotCount = (dotCount + 1) % 4;
            yield return new WaitForSeconds(0.5f);
        }
    }
}
