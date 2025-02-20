using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace OpenAI
{
    public class ChatGPTManager : GenericSingleton<ChatGPTManager>
    {
        private string gptModel = "gpt-4o";

        [SerializeField] private string openai_key = "";
        
        private OpenAIApi openai;
        
        private List<ChatMessage> messages = new List<ChatMessage>();

        public List<string> createdSentences = new List<string>();
        public List<string> translatedSentences = new List<string>();

        // 학습 언어
        public Language learningLanguage = Language.English;
        
        // 시스템 언어
        public Language systemLanguage = Language.Korean;
        
        // 학습 테마
        public Theme userSelectedTheme;

        public bool isReady;

        public int difficulty;
        public void InitStudy()
        {
            difficulty = PlayerPrefs.GetInt("difficulty");
            
            openai = new OpenAIApi(apiKey: openai_key);
            
            isReady = false;            
            // 문장 생성 요청
            // 테마, 언어에 따라 10가지 문장 생성
            ExpectedSentences();
        }
        
        public async void ExpectedSentences()
        {
          var temp = new List<ChatMessage>();
            
          string currentTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

          string prompt = $"Generate exactly 10 sentences commonly spoken by a tourist in a {userSelectedTheme.ToString().ToLower()} setting, in {learningLanguage.ToString().ToLower()}. " +
                          "Each sentence should be around 5 seconds long. " +
                          $"Translate each sentence into {systemLanguage.ToString().ToLower()} and provide both. " +
                          "These sentences should be useful for a traveler communicating in common situations, such as asking for information, ordering food, or navigating unfamiliar places. " +
                          $"Adjust the complexity of the sentences based on a difficulty level of {difficulty} (1 = very simple, 5 = very complex). " +
                          "For lower difficulty, use shorter, basic phrases. For higher difficulty, include more complex grammar and vocabulary. " +
                          $"Timestamp: {currentTime}. " +
                          "Format: Original Sentence: [Original sentence] Translated Sentence: [Translated sentence] " +
                          "Your response must contain only these pairs, nothing else. " +
                          "Do not add introductions, explanations, or extra text.";
          
            temp.Add(new ChatMessage { Role = "system", Content = prompt });
            
            // Complete the instruction
            var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
            {
                Model = gptModel,
                Messages = temp
            });

            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
            {
                var message = completionResponse.Choices[0].Message;
                Debug.Log(message.Content);
                
                createdSentences.Clear();
                translatedSentences.Clear();
                
                string resultMsg = message.Content.Trim();
                
                string[] splitPairs = resultMsg.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (string pair in splitPairs)
                {
                    if (pair.StartsWith("Original Sentence:"))
                    {
                        string original = pair.Replace("Original Sentence:", "").Trim();
                        createdSentences.Add(original);
                    }
                    else if (pair.StartsWith("Translated Sentence:"))
                    {
                        string translated = pair.Replace("Translated Sentence:", "").Trim();
                        translatedSentences.Add(translated);
                    }
                }

                isReady = true;
            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt.");
            }
        }
    }
}