using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using NPCEngine.RPC;

namespace NPCEngine.API
{
    /// <summary>
    /// Static class <c>SpeechToText</c> provides remote procedure calls 
    /// to inference engine's speech to text module.
    ///</summary>
    public  class SpeechToText : RPCBase
    {

        void Awake()
        {
            if(serviceId == "")
            {
                serviceId = "SpeechToTextAPI";
            }
        }

        public IEnumerator Listen(string context, Action<string> outputCallback = null, Action<NPCEngineException> errorCallback = null)
        {
            yield return this.Run<List<string>, string>("listen", new List<string> { context }, outputCallback, errorCallback);
        }

        public IEnumerator Transcribe(List<float> audio, Action<string> outputCallback = null, Action<NPCEngineException> errorCallback = null)
        {
            yield return this.Run<List<float>, string>("stt", audio, outputCallback, errorCallback);
        }

        public IEnumerator GetDevices(Action<List<string>> outputCallback = null, Action<NPCEngineException> errorCallback = null)
        {
            yield return this.Run<List<string>, List<string>>("get_devices", new List<string>(), outputCallback, errorCallback);
        }

        public IEnumerator SetDevice(int deviceId, Action<List<string>> outputCallback = null, Action<NPCEngineException> errorCallback = null)
        {
            yield return this.Run<int, List<string>>("select_device", deviceId, outputCallback, errorCallback);
        }

        public IEnumerator InitializeMicrophoneInput(Action<List<string>> outputCallback = null, Action<NPCEngineException> errorCallback = null)
        {
            yield return this.Run<List<string>, List<string>>("initialize_microphone_input", new List<string>(), outputCallback, errorCallback);
        }
    }
}