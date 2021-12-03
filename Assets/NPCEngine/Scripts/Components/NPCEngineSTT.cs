using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NPCEngine;
using NPCEngine.API;
using NPCEngine.Utility;

namespace NPCEngine.Components
{

    public class NPCEngineSTT : AbstractSpeechToText
    {
        ResultFuture<string> result;

        private void Start()
        {
            StartCoroutine(InitializationCoroutine());
        }

        IEnumerator InitializationCoroutine()
        {
            yield return SpeechToText.InitializeMicrophoneInput();
            CallSpeechToText();
        }

        private void Update()
        {
            if (result != null && result.ResultReady)
            {
                if (result.Result == "")
                {
                    CallSpeechToText();
                }
                else
                {
                    SpeechRecognized(result.Result);
                    result = null;
                }
            }
        }

        public void CallSpeechToText()
        {
            result = SpeechToText.Listen(Context);
        }

        public override void StartListening()
        {
            CallSpeechToText();
        }

        public override void StopListening()
        {
        }
    }
}
