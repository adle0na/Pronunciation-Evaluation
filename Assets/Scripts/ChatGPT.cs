using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace OpenAI
{
    public class ChatGPT : MonoBehaviour
    {
        private string gptModel = "gpt-3.5-turbo";

        [SerializeField] private string openai_key = "";
        
        private OpenAIApi openai;
        
        private List<ChatMessage> messages = new List<ChatMessage>();
        private List<ChatMessage> assisMsg = new List<ChatMessage>();
        
        public UnityAction<string> onResponse;
        public UnityAction<string> onResponseExample;

        private IEnumerator Start()
        {
            openai = new OpenAIApi(apiKey: openai_key);

            ExpectedQuestions();
            
            yield return null; // GetPersona();
        }
        
        public async void ExpectedQuestions()
        {
            //if (content.Length > 0)
            var temp = new List<ChatMessage>();// messages.GetRange(messages.Count-2, 2);
            //temp.AddRange(messages);
            foreach (var message in messages)
            {
	            if( message.Role == "user")
                {
					var newMessage = new ChatMessage()
					{
						Role = "assistant",
						Content = message.Content,
					};

					temp.Add(newMessage);
				}
				else if (message.Role == "assistant")
				{
					var newMessage = new ChatMessage()
					{
						Role = "user",
						Content = message.Content,
					};

					temp.Add(newMessage);
				}
                else
                {
					temp.Add(message);
				}

			}
            {
				var newMessage = new OpenAI.ChatMessage()
				{
					Role = "system",
					Content = "All answers are kind and short."
				};
				temp.Add(newMessage);

			}




			// Complete the instruction
			var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
            {
                Model = gptModel,//"gpt -3.5-turbo-0613",
                Messages = temp
            });

            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
            {
                var message = completionResponse.Choices[0].Message;
                //message.Content = message.Content.Trim();
                Debug.Log(message.Content);
                //Debug.Log(message.Role);
                //messages.Add(message);
                var resultMsg = message.Content;
                if (message.Content.Contains(":"))
                {
                    var spilt = message.Content.Split(":");
                    resultMsg = spilt[spilt.Length - 1];

                }
                //chatGPT.text = message.Content;
                onResponseExample?.Invoke(resultMsg);


            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt.");
            }
        }
    }
}