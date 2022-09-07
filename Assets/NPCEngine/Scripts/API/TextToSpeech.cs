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

        void Awake()
        {
            if (this.serviceId == "")
            {
                this.serviceId = "TextToSpeechAPI";
            }
        }

        [Serializable()]
        private class TTSMessage
        {
            public string speaker_id;
            public string text;
            public int n_chunks;
        }

        public IEnumerator StartTTS(string voiceId, string line, int n_chunks, Action outputCallback, Action<NPCEngineException> errorCallback = null)
        {
            var msg = new TTSMessage { speaker_id = voiceId, text = line, n_chunks = n_chunks };
            yield return this.Run<TTSMessage, bool?>("tts_start", msg, (ok) => {outputCallback();}, errorCallback);
        }

        public IEnumerator GetNextResult(Action<List<float>> outputCallback = null, Action<NPCEngineException> errorCallback = null)
        {
            yield return this.Run<List<string>, List<float>>("tts_get_results", new List<string>(), outputCallback, errorCallback);
        }

        public IEnumerator GetSpeakerIds(Action<List<string>> outputCallback = null, Action<NPCEngineException> errorCallback = null)
        {
            yield return this.Run<List<string>, List<string>>("get_speaker_ids", new List<string>(), outputCallback, errorCallback);
        }
    }
}