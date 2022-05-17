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
    public class TextGeneration<ContextType> : RPCBase<TextGeneration<ContextType>>
    where ContextType : new()
    {

        [Serializable()]
        class ChatbotMessage
        {
            public ContextType context;
            public float temperature;
            public int topk;
        }


        public ResultFuture<string> GenerateReply(ContextType context, float temperature = 0.8f, int topk = 5)
        {
            var msg = new ChatbotMessage
            {
                context = context,
                temperature = temperature,
                topk = topk
            };
            return this.Run<ChatbotMessage, string>("generate_reply", msg);
        }

        public ResultFuture<Dictionary<string, string>> GetSpecialTokens()
        {
            return this.Run<List<string>, Dictionary<string, string>>("get_special_tokens", new List<string>());
        }

        public ResultFuture<string> GetPromptTemplate()
        {
            return this.Run<List<string>, string>("get_prompt_template", new List<string>());
        }

        // GenerateReply in a coroutine
        public IEnumerator GenerateReplyCoroutine(ContextType context, Action<string> outputCallback, float temperature = 0.8f, int topk = 5)
        {
            var msg = new ChatbotMessage
            {
                context = context,
                temperature = temperature,
                topk = topk
            };
            var result = this.Run<ChatbotMessage, string>("generate_reply", msg);
            while (!result.ResultReady)
            {
                yield return null;
            }
            outputCallback(result.Result);
        }

        // GetSpecialTokens in a coroutine
        public IEnumerator GetSpecialTokensCoroutine(Action<Dictionary<string, string>> outputCallback)
        {
            var result = this.Run<List<string>, Dictionary<string, string>>("get_special_tokens", new List<string>());
            while (!result.ResultReady)
            {
                yield return null;
            }
            outputCallback(result.Result);
        }

        //GetPromptTemplate in a coroutine
        public IEnumerator GetPromptTemplateCoroutine(Action<string> outputCallback)
        {
            var result = this.Run<List<string>, string>("get_prompt_template", new List<string>());
            while (!result.ResultReady)
            {
                yield return null;
            }
            outputCallback(result.Result);
        }
    }
}