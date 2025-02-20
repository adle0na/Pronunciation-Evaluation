using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.PronunciationAssessment;
using OpenAI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class StudyUI : MonoBehaviour
{
	private const string speechKey = "2yR76y3Nzuo6DMt9pynMc9YvxyyslQCe7djluvqXIlokyKrZtuLJJQQJ99BBACNns7RXJ3w3AAAYACOGx7ny";
	private const string speechRegion = "koreacentral";
	
    [SerializeField] private TMP_Text currentPageText;
    
    [SerializeField] private GameObject scoreGroup;
    
    [SerializeField] private TMP_Text totalScore;

    [SerializeField] private Image circleFillImage;

    [SerializeField] private TMP_Text accuracyText;
    [SerializeField] private Slider accuracySlider;
    
    [SerializeField] private TMP_Text fluencyText;
    [SerializeField] private Slider fluencySlider;
    
    [SerializeField] private TMP_Text completionText;
    [SerializeField] private Slider completionSlider;
    
    [SerializeField] private TMP_Text meterText;
    [SerializeField] private Slider meterSlider;

    [SerializeField] private TMP_Text targetText;
    [SerializeField] private TMP_Text localText;

    [SerializeField] private GameObject infoPage;
    [SerializeField] private Button startStudyButton;
    [SerializeField] private Button goNextButton;
    [SerializeField] private Button recordButton;

    [SerializeField] private GameObject lastResultPage;

    private PronunciationAssessmentResult getScore;
    private SpeechConfig speechConfig;
    private SpeechSynthesizer speechSynthesizer;
    private List<ScoreValue> scoreList;

    private string _microphoneID = null;
    private AudioClip _recording = null;
    private int _recordingLengthSec = 15;
    private int _recordingHZ = 22050;
    const int BlockSize_16Bit = 2;
    private bool enableMiscue = true;
    private bool isRecognizing = false;
    private bool isRecordAble = true;

    private string recognizedText;

    private bool isSamplePlaying;

    public int currentIndex = 0;
    void Start()
    {
	    StartCoroutine(WaitForReadyCor());
	    
        if (Microphone.devices.Length <= 0)
        {
            Debug.LogError("기기에 연결된 마이크가 없습니다.");
            isRecordAble = false;
            return;
        }
    }

    IEnumerator WaitForReadyCor()
    {
	    yield return new WaitUntil(() => ChatGPTManager.Instance.isReady);
	    
	    startStudyButton.interactable = true;
    }

    public void InitPage()
    {
	    currentIndex = 0;
	    
	    SetPage(currentIndex);
	    
	    infoPage.SetActive(false);
    }
    
    public void SetPage(int index)
    {
	    scoreGroup.SetActive(false);
	    
	    currentPageText.text = $"{index + 1}/10";

	    targetText.text = ChatGPTManager.Instance.createdSentences[index];

	    localText.text = ChatGPTManager.Instance.translatedSentences[index];

	    goNextButton.interactable = false;
    }

    public void NextPage()
    {
	    if (currentIndex == 9)
	    {
		    lastResultPage.SetActive(true);

		    ScoreValue averageScore = new ScoreValue();

		    double calPronunciationScore = 0;
		    double calCompleteness = 0;
		    double calAccuracy = 0;
		    double calProsody = 0;
		    double calFluency = 0;
		    
		    foreach (var score in scoreList)
		    {
			    calPronunciationScore += score.PronunciationScore;
			    calCompleteness += score.CompletenessScore;
			    calAccuracy += score.AccuracyScore;
			    calProsody += score.ProsodyScore;
			    calFluency += score.FluencyScore;
		    }

		    averageScore.PronunciationScore = calPronunciationScore / 10;
		    averageScore.CompletenessScore = calCompleteness / 10;
		    averageScore.AccuracyScore = calCompleteness / 10;
		    averageScore.ProsodyScore = calProsody / 10;
		    averageScore.FluencyScore = calFluency / 10;
		    
		    lastResultPage.GetComponent<LastPage>().SetResultToUI(averageScore);
		    
		    return;
	    }
	    
	    currentIndex++;
	    
	    SetPage(currentIndex);
    }
    
    public void StartRecording()
    {
        if (!isRecordAble)
        {
            Debug.LogError("연결된 녹음 기기가 없습니다.");
            return;
        }
	    
        Debug.Log("start recording");
        recordButton.interactable = false;
        _recording = Microphone.Start(_microphoneID, false, _recordingLengthSec, _recordingHZ);

        StartCoroutine(FiveSecondsWaitCor(targetText.text));
    }

    IEnumerator FiveSecondsWaitCor(string targetText)
    {
	    yield return new WaitForSeconds(5);

	    StopRecording(targetText);
    }
    
    private void StopRecording(string targetText)
    {
	    if (!isRecordAble) return;

	    if (Microphone.IsRecording(_microphoneID))
	    {
		    Microphone.End(_microphoneID);
		    Debug.Log("stop recording");

		    if (_recording == null)
		    {
			    Debug.LogError("nothing recorded");
			    return;
		    }

		    // 오디오 데이터를 파일로 저장
		    string filePath = Path.Combine(Application.persistentDataPath, "recorded_audio.wav");
		    File.WriteAllBytes(filePath, getByteFromAudioClip(_recording));

		    Debug.Log("녹음된 파일 저장 완료: " + filePath);

		    // 저장된 오디오 파일을 사용하여 발음 평가 실행
		    StartRealTimePronunciationAssessment(filePath, targetText);
	    }
    }
    
    private byte[] getByteFromAudioClip(AudioClip audioClip)
    {
        MemoryStream stream = new MemoryStream();
        const int headerSize = 44;
        ushort bitDepth = 16;

        int fileSize = audioClip.samples * BlockSize_16Bit + headerSize;

        // audio clip의 정보들을 file stream에 추가(링크 참고 함수 선언)
        WriteFileHeader(ref stream, fileSize);
        WriteFileFormat(ref stream, audioClip.channels, audioClip.frequency, bitDepth);
        WriteFileData(ref stream, audioClip, bitDepth);
    
        // stream을 array형태로 바꿈
        byte[] bytes = stream.ToArray();

        return bytes;
    }
    
	private static int WriteFileHeader (ref MemoryStream stream, int fileSize)
	{
		int count = 0;
		int total = 12;

		// riff chunk id
		byte[] riff = Encoding.ASCII.GetBytes ("RIFF");
		count += WriteBytesToMemoryStream (ref stream, riff, "ID");

		// riff chunk size
		int chunkSize = fileSize - 8; // total size - 8 for the other two fields in the header
		count += WriteBytesToMemoryStream (ref stream, BitConverter.GetBytes (chunkSize), "CHUNK_SIZE");

		byte[] wave = Encoding.ASCII.GetBytes ("WAVE");
		count += WriteBytesToMemoryStream (ref stream, wave, "FORMAT");

		// Validate header
		Debug.AssertFormat (count == total, "Unexpected wav descriptor byte count: {0} == {1}", count, total);

		return count;
	}

	private static int WriteFileFormat (ref MemoryStream stream, int channels, int sampleRate, UInt16 bitDepth)
	{
		int count = 0;
		int total = 24;

		byte[] id = Encoding.ASCII.GetBytes ("fmt ");
		count += WriteBytesToMemoryStream (ref stream, id, "FMT_ID");

		int subchunk1Size = 16; // 24 - 8
		count += WriteBytesToMemoryStream (ref stream, BitConverter.GetBytes (subchunk1Size), "SUBCHUNK_SIZE");

		UInt16 audioFormat = 1;
		count += WriteBytesToMemoryStream (ref stream, BitConverter.GetBytes (audioFormat), "AUDIO_FORMAT");

		UInt16 numChannels = Convert.ToUInt16 (channels);
		count += WriteBytesToMemoryStream (ref stream, BitConverter.GetBytes (numChannels), "CHANNELS");

		count += WriteBytesToMemoryStream (ref stream, BitConverter.GetBytes (sampleRate), "SAMPLE_RATE");

		int byteRate = sampleRate * channels * BytesPerSample (bitDepth);
		count += WriteBytesToMemoryStream (ref stream, BitConverter.GetBytes (byteRate), "BYTE_RATE");

		UInt16 blockAlign = Convert.ToUInt16 (channels * BytesPerSample (bitDepth));
		count += WriteBytesToMemoryStream (ref stream, BitConverter.GetBytes (blockAlign), "BLOCK_ALIGN");

		count += WriteBytesToMemoryStream (ref stream, BitConverter.GetBytes (bitDepth), "BITS_PER_SAMPLE");

		// Validate format
		Debug.AssertFormat (count == total, "Unexpected wav fmt byte count: {0} == {1}", count, total);

		return count;
	}

	private static int WriteFileData (ref MemoryStream stream, AudioClip audioClip, UInt16 bitDepth)
	{
		int count = 0;
		int total = 8;

		// Copy float[] data from AudioClip
		float[] data = new float[audioClip.samples * audioClip.channels];
		audioClip.GetData (data, 0);

		byte[] bytes = ConvertAudioClipDataToInt16ByteArray (data);

		byte[] id = Encoding.ASCII.GetBytes ("data");
		count += WriteBytesToMemoryStream (ref stream, id, "DATA_ID");

		int subchunk2Size = Convert.ToInt32 (audioClip.samples * BlockSize_16Bit); // BlockSize (bitDepth)
		count += WriteBytesToMemoryStream (ref stream, BitConverter.GetBytes (subchunk2Size), "SAMPLES");

		// Validate header
		Debug.AssertFormat (count == total, "Unexpected wav data id byte count: {0} == {1}", count, total);

		// Write bytes to stream
		count += WriteBytesToMemoryStream (ref stream, bytes, "DATA");

		// Validate audio data
		Debug.AssertFormat (bytes.Length == subchunk2Size, "Unexpected AudioClip to wav subchunk2 size: {0} == {1}", bytes.Length, subchunk2Size);

		return count;
	}
    
	private static byte[] ConvertAudioClipDataToInt16ByteArray (float[] data)
	{
		MemoryStream dataStream = new MemoryStream ();

		int x = sizeof(Int16);

		Int16 maxValue = Int16.MaxValue;

		int i = 0;
		while (i < data.Length) {
			dataStream.Write (BitConverter.GetBytes (Convert.ToInt16 (data [i] * maxValue)), 0, x);
			++i;
		}
		byte[] bytes = dataStream.ToArray ();

		// Validate converted bytes
		Debug.AssertFormat (data.Length * x == bytes.Length, "Unexpected float[] to Int16 to byte[] size: {0} == {1}", data.Length * x, bytes.Length);

		dataStream.Dispose ();

		return bytes;
	}
	
	public async void StartRealTimePronunciationAssessment(string filePath, string targetText)
	{
	    if (isRecognizing) return;
	    if (!File.Exists(filePath))
	    {
	        Debug.LogError("오디오 파일을 찾을 수 없습니다: " + filePath);
	        return;
	    }
	
	    isRecognizing = true;
	    getScore = null;
	    
	    var config = SpeechConfig.FromSubscription(speechKey, speechRegion);
	    Debug.Log("발음 평가 시작");
	
	    string languageCode = GetLanguageCode();
	    
	    using (var audioInput = AudioConfig.FromWavFileInput(filePath))
	    {
	        using (var recognizer = new SpeechRecognizer(config, languageCode, audioInput))
	        {
	            var referenceText = targetText;
	            var pronConfig = new PronunciationAssessmentConfig(referenceText, GradingSystem.HundredMark, Granularity.Phoneme, enableMiscue);
	            pronConfig.EnableProsodyAssessment();
	            pronConfig.ApplyTo(recognizer);
	
	            recognizer.Recognized += (s, e) =>
	            {
	                if (e.Result != null)
	                {
	                    recognizedText = e.Result.Text;
	                    Debug.Log($"Recognized Text: {e.Result.Text}");
	                    
	                    var pronResult = PronunciationAssessmentResult.FromResult(e.Result);
	                    Debug.Log($"정확성: {pronResult.AccuracyScore}, 발음: {pronResult.PronunciationScore}, 완전성: {pronResult.CompletenessScore}, 유창함: {pronResult.FluencyScore}, 운율감: {pronResult.ProsodyScore}");
	
	                    getScore = pronResult;
	                }
	                else
	                {
	                    Debug.LogError("Recognized 이벤트 호출되었지만 e.Result가 null입니다.");
	                }
	            };
	
	            recognizer.Canceled += (s, e) =>
	            {
	                Debug.LogError($"Recognition canceled. Reason: {e.Reason}");
	            };
	
	            recognizer.SessionStopped += (s, e) =>
	            {
	                Debug.Log("Session stopped.");
	            };
	
	            // 발음 평가 실행
	            await recognizer.StartContinuousRecognitionAsync();
	
	            // 5초 동안 실행 후 종료
	            await Task.Delay(5000);
	            await recognizer.StopContinuousRecognitionAsync();
	        }
	    }
	    
	    isRecognizing = false;
	    recordButton.interactable = true;
	
	    if (string.IsNullOrEmpty(recognizedText))
	    {
	        Debug.LogError("음성 인식 실패. 다시 시도해주세요.");
	        goNextButton.interactable = true; 
	    }
	    
	    if (getScore != null)
	    {
	        SetResultToUI();
	    }
	    else
	    {
	        Debug.LogError("스코어 데이터를 가져오지 못했습니다.");
	    }
	}

	public void SetResultToUI()
	{
		scoreGroup.SetActive(true);

		totalScore.text = ((int)getScore.PronunciationScore).ToString();
		circleFillImage.fillAmount = (float)getScore.PronunciationScore * 0.01f;
		
		accuracyText.text = $"{getScore.AccuracyScore}/100";
		accuracySlider.value = (float)getScore.AccuracyScore * 0.01f;
		GetScoreColor(getScore.AccuracyScore, accuracySlider.fillRect.GetComponent<Image>());

		fluencyText.text = $"{getScore.FluencyScore}/100";
		fluencySlider.value = (float)getScore.FluencyScore * 0.01f;
		GetScoreColor(getScore.FluencyScore, fluencySlider.fillRect.GetComponent<Image>());
		
		completionText.text = $"{getScore.CompletenessScore}/100";
		completionSlider.value = (float)getScore.CompletenessScore * 0.01f;
		GetScoreColor(getScore.CompletenessScore, completionSlider.fillRect.GetComponent<Image>());
		
		meterText.text = $"{getScore.ProsodyScore}/100";
		meterSlider.value = (float)getScore.ProsodyScore * 0.01f;
		GetScoreColor(getScore.ProsodyScore, meterSlider.fillRect.GetComponent<Image>());
		
		//goNextButton.interactable = getScore.PronunciationScore >= 60;

		goNextButton.interactable = true;
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
	
	private static int WriteBytesToMemoryStream (ref MemoryStream stream, byte[] bytes, string tag = "")
    {
	    int count = bytes.Length;
	    stream.Write (bytes, 0, count);
	    //Debug.LogFormat ("WAV:{0} wrote {1} bytes.", tag, count);
	    return count;
    }
    
    private static int BytesPerSample (UInt16 bitDepth)
    {
	    return bitDepth / 8;
    }
    
    private string GetLanguageCode()
    {
	    switch (ChatGPTManager.Instance.learningLanguage)
	    {
		    case Language.Korean: return "ko-KR";
		    case Language.Japanese: return "ja-JP";
		    case Language.Chinese: return "zh-CN";
		    case Language.English: return "en-US";
		    case Language.German: return "de-DE";
		    case Language.French: return "fr-FR";
		    default: return "en-US";
	    }
    }

    public void RestartStudy()
    {
	    lastResultPage.SetActive(false);
	    
	    SetPage(0);
    }

    public void BackToMain()
    {
	    UIManager.Instance.PageChange(2);
    }

    public void SpeakText()
    {
	    if (isSamplePlaying) return;
	    
	    speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
	    speechConfig.SpeechSynthesisVoiceName = "en-US-AvaMultilingualNeural";
	    speechSynthesizer = new SpeechSynthesizer(speechConfig);
	    
	    StartCoroutine(SpeakTextCor());
    }
    
    private IEnumerator SpeakTextCor()
    {
	    isSamplePlaying = true;
	    
	    string text = targetText.text;                                                         
	    
	    Task<SpeechSynthesisResult> task = speechSynthesizer.SpeakTextAsync(text);
	    yield return new WaitUntil(() => task.IsCompleted);
        
	    HandleSpeechSynthesisResult(task.Result, text);
    }
    
    private void HandleSpeechSynthesisResult(SpeechSynthesisResult result, string text)
    {
	    switch (result.Reason)
	    {
		    case ResultReason.SynthesizingAudioCompleted:
			    Debug.Log($"Speech synthesized: {text}");
			    break;
		    case ResultReason.Canceled:
			    var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
			    Debug.Log($"Error: {cancellation.Reason}");
			    if (cancellation.Reason == CancellationReason.Error)
			    {
				    Debug.LogError($"Error Code: {cancellation.ErrorCode}, Details: {cancellation.ErrorDetails}");
			    }
			    break;
	    }

	    isSamplePlaying = false;
    }
    
    private void OnDestroy()
    {
	    speechSynthesizer?.Dispose();
    }
}
