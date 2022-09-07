using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using NPCEngine.RPC;

namespace NPCEngine.API
{

    /// <summary>
    /// <c>Chatbot</c> provides remote procedure calls 
    /// to inference engine's TextGeneration services.
    ///</summary>
    public class TextGeneration<ContextType> : RPCBase
    where ContextType : new()
    {


        void Awake()
        {
            if (this.serviceId == "")
            {
                this.serviceId = "TextGenerationAPI";
            }
        }

        [Serializable()]
        class ChatbotMessage
        {
            public ContextType context;
            public float temperature;
            public int topk;
            public int num_sampled;
        }

        // GenerateReply in a coroutine
        public IEnumerator GenerateReply(
            ContextType context, 
            float temperature = 0.8f, 
            int topk = 5, 
            int numSampled = 3,
            Action<string> outputCallback = null, 
            Action<NPCEngineException> errorCallback = null
        )
        {
            var msg = new ChatbotMessage
            {
                context = context,
                temperature = temperature,
                topk = topk,
                num_sampled = numSampled
            };
            yield return this.Run<ChatbotMessage, string>("generate_reply", msg, outputCallback, errorCallback);
        }

        // GetSpecialTokens in a coroutine
        public IEnumerator GetSpecialTokens(Action<Dictionary<string, string>> outputCallback = null, Action<NPCEngineException> errorCallback = null)
        {
            yield return this.Run<List<string>, Dictionary<string, string>>("get_special_tokens", new List<string>(), outputCallback, errorCallback);
        }

        //GetPromptTemplate in a coroutine
        public IEnumerator GetPromptTemplate(Action<string> outputCallback = null, Action<NPCEngineException> errorCallback = null)
        {
            yield return this.Run<List<string>, string>("get_prompt_template", new List<string>(), outputCallback, errorCallback);
        }
    }
}