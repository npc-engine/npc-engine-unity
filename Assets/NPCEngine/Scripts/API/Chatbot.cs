using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using NPCEngine.Server;

namespace NPCEngine.API
{

    /// <summary>
    /// Static class <c>Chatbot</c> provides remote procedure calls 
    /// to inference engine's chatbot model.
    ///</summary>
    public static class Chatbot<ContextType>
    where ContextType : new()
    {

        [Serializable()]
        class ChatbotMessage
        {
            public ContextType context;
            public float temperature;
            public int topk;
        }


        public static ResultFuture<string> GenerateReply(ContextType context, float temperature = 0.8f, int topk = 5)
        {
            var msg = new ChatbotMessage
            {
                context = context,
                temperature = temperature,
                topk = topk
            };
            return NPCEngineServer.Instance.Run<ChatbotMessage, string>("generate_reply", msg);
        }

        public static ResultFuture<Dictionary<string, string>> GetSpecialTokens()
        {
            return NPCEngineServer.Instance.Run<List<string>, Dictionary<string, string>>("get_special_tokens", new List<string>());
        }

        public static ResultFuture<string> GetPromptTemplate()
        {
            return NPCEngineServer.Instance.Run<List<string>, string>("get_prompt_template", new List<string>());
        }

        // GenerateReply in a coroutine
        public static IEnumerator<string> GenerateReplyCoroutine(ContextType context, Action<string> outputCallback, float temperature = 0.8f, int topk = 5)
        {
            var msg = new ChatbotMessage
            {
                context = context,
                temperature = temperature,
                topk = topk
            };
            var result = NPCEngineServer.Instance.Run<ChatbotMessage, string>("generate_reply", msg);
            while (!result.ResultReady)
            {
                yield return null;
            }
            outputCallback(result.Result);
        }

        // GetSpecialTokens in a coroutine
        public static IEnumerator<Dictionary<string, string>> GetSpecialTokensCoroutine(Action<Dictionary<string, string>> outputCallback)
        {
            var result = NPCEngineServer.Instance.Run<List<string>, Dictionary<string, string>>("get_special_tokens", new List<string>());
            while (!result.ResultReady)
            {
                yield return null;
            }
            outputCallback(result.Result);
        }

        //GetPromptTemplate in a coroutine
        public static IEnumerator GetPromptTemplateCoroutine(Action<string> outputCallback)
        {
            var result = NPCEngineServer.Instance.Run<List<string>, string>("get_prompt_template", new List<string>());
            while (!result.ResultReady)
            {
                yield return null;
            }
            outputCallback(result.Result);
        }
    }
}