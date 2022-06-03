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

        public ResultFuture<string> ListenFuture(string context)
        {
            return this.Run<List<string>, string>("listen", new List<string> { context });
        }

        public ResultFuture<string> TranscribeFuture(List<float> audio)
        {
            return this.Run<List<float>, string>("stt", audio);
        }

        public ResultFuture<List<string>> GetDevicesFuture()
        {
            return this.Run<List<string>, List<string>>("get_devices", new List<string>());
        }

        public ResultFuture<List<string>> SetDeviceFuture(int deviceId)
        {
            return this.Run<int, List<string>>("select_device", deviceId);
        }

        public IEnumerator Listen(string context, Action<string> outputCallback)
        {
            var result = this.Run<List<string>, string>("listen", new List<string> { context });

            while (!result.ResultReady)
            {
                yield return null;
            }
            outputCallback(result.Result);
        }

        public IEnumerator Transcribe(List<float> audio, Action<string> outputCallback)
        {
            var result = this.Run<List<float>, string>("stt", audio);

            while (!result.ResultReady)
            {
                yield return null;
            }
            outputCallback(result.Result);
        }

        public IEnumerator GetDevices(Action<List<string>> outputCallback)
        {
            var result = this.Run<List<string>, List<string>>("get_devices", new List<string>());

            while (!result.ResultReady)
            {
                yield return null;
            }
            outputCallback(result.Result);
        }

        public IEnumerator SetDevice(int deviceId, Action<List<string>> outputCallback)
        {
            var result = this.Run<int, List<string>>("select_device", deviceId);

            while (!result.ResultReady)
            {
                yield return null;
            }
            outputCallback(result.Result);
        }

        public IEnumerator InitializeMicrophoneInput()
        {
            var result = this.Run<List<string>, List<string>>("initialize_microphone_input", new List<string>());
            while (!result.ResultReady)
            {
                yield return null;
            }
            // Wait 1 second to allow ASR system to fill silence buffer for better recognition
            yield return new WaitForSeconds(1);
            var test_result = result.Result;
        }

    }
}