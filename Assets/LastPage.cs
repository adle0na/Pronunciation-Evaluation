using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class LastPage : MonoBehaviour
{
    [SerializeField] private TMP_Text totalScore;
    [SerializeField] private TMP_Text totalScoreEvaluation;
    
    [SerializeField] private Image circleFillImage;

    [SerializeField] private TMP_Text accuracyText;
    [SerializeField] private Slider accuracySlider;
    
    [SerializeField] private TMP_Text fluencyText;
    [SerializeField] private Slider fluencySlider;
    
    [SerializeField] private TMP_Text completionText;
    [SerializeField] private Slider completionSlider;
    
    [SerializeField] private TMP_Text meterText;
    [SerializeField] private Slider meterSlider;

    public void SetResultToUI(ScoreValue getAverageScore)
    {
	    totalScore.text = ((int)getAverageScore.PronunciationScore).ToString();
        circleFillImage.fillAmount = (float)getAverageScore.PronunciationScore * 0.01f;
		
        accuracyText.text = $"{getAverageScore.AccuracyScore}/100";
        accuracySlider.value = (float)getAverageScore.AccuracyScore * 0.01f;
        GetScoreColor(getAverageScore.AccuracyScore, accuracySlider.fillRect.GetComponent<Image>());

        fluencyText.text = $"{getAverageScore.FluencyScore}/100";
        fluencySlider.value = (float)getAverageScore.FluencyScore * 0.01f;
        GetScoreColor(getAverageScore.FluencyScore, fluencySlider.fillRect.GetComponent<Image>());
		
        completionText.text = $"{getAverageScore.CompletenessScore}/100";
        completionSlider.value = (float)getAverageScore.CompletenessScore * 0.01f;
        GetScoreColor(getAverageScore.CompletenessScore, completionSlider.fillRect.GetComponent<Image>());
		
        meterText.text = $"{getAverageScore.ProsodyScore}/100";
        meterSlider.value = (float)getAverageScore.ProsodyScore * 0.01f;
        GetScoreColor(getAverageScore.ProsodyScore, meterSlider.fillRect.GetComponent<Image>());
    }
    
    private void GetScoreColor(double checkValue, Image target)
    {
	    if (checkValue >= 80)
	    {
		    target.color = UIManager.Instance.perfectScoreColor;
	    }
	    else if (checkValue >= 60)
	    {
		    target.color = UIManager.Instance.normalScoreColor;
	    }
	    else
	    {
		    target.color = UIManager.Instance.badScoreColor;
	    }
    }

    IEnumerator ChangeLocaleCor(double score)
    {
	    var loadingOperation = LocalizationSettings.StringDatabase.GetTableAsync("New Table");
	    yield return loadingOperation;

	    if (loadingOperation.Status == AsyncOperationStatus.Succeeded)
	    {
		    StringTable table = loadingOperation.Result;

		    string text = "";
		    
		    if (score >= 60 && score < 70)
		    {
			    text = table.GetEntry("39")?.GetLocalizedString();
		    }
		    else if (score >= 70 && score < 80)
		    {
			    text = table.GetEntry("40")?.GetLocalizedString();
		    }
		    else if (score >= 80 && score < 90)
		    {
			    text = table.GetEntry("41")?.GetLocalizedString();
		    }
		    else if (score >= 90 && score < 100)
		    {
			    text = table.GetEntry("42")?.GetLocalizedString();
		    }
		    
		    totalScoreEvaluation.text = text;
	    }
    }
}
