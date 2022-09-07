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
        private void Start()
        {
            StartCoroutine(InitializationCoroutine());
        }

        IEnumerator InitializationCoroutine()
        {
            yield return NPCEngineManager.Instance.GetAPI<SpeechToText>().InitializeMicrophoneInput();
            CallSpeechToText();
        }

        public void CallSpeechToText()
        {
            StartCoroutine(NPCEngineManager.Instance.GetAPI<SpeechToText>().Listen(Context));
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
