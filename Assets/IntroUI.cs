using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IntroUI : MonoBehaviour
{
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private TMP_Text titleText;

    [SerializeField] private GameObject LogoUIPage;
    [SerializeField] private GameObject IntroVideo;

    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField]
    
    private void Start()
    {
        AnimateIntro();
    }

    private void AnimateIntro()
    {
        subtitleText.maxVisibleCharacters = 0;
        int totalCharacters = subtitleText.text.Length;

        LeanTween.value(0, totalCharacters, 1.5f)
            .setOnUpdate((float val) =>
            {
                subtitleText.maxVisibleCharacters = Mathf.RoundToInt(val);
            })
            .setEase(LeanTweenType.linear)
            .setOnComplete(() =>
            {
                titleText.gameObject.SetActive(true);
                
                titleText.transform.localScale = Vector3.zero;
                LeanTween.scale(titleText.gameObject, Vector3.one, 0.5f).setEase(LeanTweenType.easeOutBack);
            });

        StartCoroutine(FadeTransition());
    }

    IEnumerator FadeTransition()
    {
        yield return new WaitForSeconds(2);
        
        LeanTween.alphaCanvas(fadeCanvas, 0, 1f).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() =>
        {
            LogoUIPage.SetActive(false);
            IntroVideo.SetActive(true);
            
            LeanTween.alphaCanvas(fadeCanvas, 1, 1f).setEase(LeanTweenType.easeInOutQuad);
        });
    }
}
