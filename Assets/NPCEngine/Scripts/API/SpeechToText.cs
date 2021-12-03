using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using NPCEngine.Server;

namespace NPCEngine.API
{
    /// <summary>
    /// Static class <c>SpeechToText</c> provides remote procedure calls 
    /// to inference engine's speech to text module.
    ///</summary>
    public static class SpeechToText
    {

        public static ResultFuture<string> Listen(string context)
        {
            return NPCEngineServer.Instance.Run<List<string>, string>("listen", new List<string> { context });
        }

        public static ResultFuture<string> Transcribe(List<float> audio)
        {
            return NPCEngineServer.Instance.Run<List<float>, string>("stt", audio);
        }

        public static ResultFuture<List<string>> GetDevices()
        {
            return NPCEngineServer.Instance.Run<List<string>, List<string>>("get_devices", new List<string>());
        }

        public static IEnumerator InitializeMicrophoneInput()
        {
            var result = NPCEngineServer.Instance.Run<List<string>, List<string>>("initialize_microphone_input", new List<string>());
            while (!result.ResultReady)
            {
                yield return null;
            }
            // Wait 1 second to allow ASR system to fill silence buffer for better recognition
            yield return new WaitForSeconds(1);
        }

        public static void SetDevice(int deviceId)
        {
            NPCEngineServer.Instance.Run<int, List<string>>("select_device", deviceId);
        }
    }
}