using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using NPCEngine.RPC;

namespace NPCEngine.API
{
    /// <summary>
    /// Static class <c>TextToSpeech</c> provides remote procedure calls 
    /// to inference engine's text to speech module.
    ///</summary>
    public class TextToSpeech : RPCBase
    {
        [Serializable()]
        private class TTSMessage
        {
            public string speaker_id;
            public string text;
            public int n_chunks;
        }

        public void StartTTS(string voiceId, string line, int n_chunks)
        {
            var msg = new TTSMessage { speaker_id = voiceId, text = line, n_chunks = n_chunks };
            this.Run<TTSMessage, bool?>("tts_start", msg);
        }

        public ResultFuture<List<string>> GetSpeakerIds()
        {
            return this.Run<List<string>, List<string>>("get_speaker_ids", new List<string>());
        }

        public ResultFuture<List<float>> GetNextResult()
        {
            return this.Run<List<string>, List<float>>("tts_get_results", new List<string>());
        }

        // GetNextResult in a coroutine
        public IEnumerator GetNextResultCoroutine(Action<List<float>> outputCallback)
        {
            var result = this.Run<List<string>, List<float>>("tts_get_results", new List<string>());
            while (!result.ResultReady)
            {
                yield return null;
            }
            if (result.Error == null)
            {
                outputCallback(result.Result);
            }
        }
    }
}