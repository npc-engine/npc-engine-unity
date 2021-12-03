using System;
using System.Collections.Generic;
using UnityEngine;

namespace NPCEngine.Components
{

    public abstract class AbstractSpeechToText : MonoBehaviour
    {

        // Consumed context to be used in the next STT request
        public string Context
        {
            protected get
            {
                var cpy = context;
                context = "_";
                return cpy;
            }
            set
            {
                context = value;
            }
        }

        public abstract void StartListening();

        public abstract void StopListening();

        // PlayerCharacter subscribes to this event to get the Speech-to-Text result
        public event Action<string> OnSpeechRecognized = new Action<string>(s => { });

        private string context = "_";
        protected void SpeechRecognized(string result)
        {
            OnSpeechRecognized?.Invoke(result);
        }

    }

}