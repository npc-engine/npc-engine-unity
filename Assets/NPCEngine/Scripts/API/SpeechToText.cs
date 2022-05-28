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
    public class SpeechToText : RPCBase
    {

        public override string ServiceId { get { return "SpeechToTextAPI"; } }
        public ResultFuture<string> Listen(string context)
        {
            return this.Run<List<string>, string>("listen", new List<string> { context });
        }

        public ResultFuture<string> Transcribe(List<float> audio)
        {
            return this.Run<List<float>, string>("stt", audio);
        }

        public ResultFuture<List<string>> GetDevices()
        {
            return this.Run<List<string>, List<string>>("get_devices", new List<string>());
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
        }

        public void SetDevice(int deviceId)
        {
            this.Run<int, List<string>>("select_device", deviceId);
        }
    }
}