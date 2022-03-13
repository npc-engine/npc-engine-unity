using System;
using System.Collections.Generic;
using UnityEngine;

namespace NPCEngine.Components
{

    /// <summary>
    /// Abstract class for speech to text system.
    /// Concrete implementations are used by PlayerCharacter 
    /// component for receiving speech.
    /// </summary>
    public abstract class AbstractSpeechToText : MonoBehaviour
    {

        /// <summary>
        /// Consumed context to be used in the next STT request
        /// </summary>
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

        /// <summary>
        /// Called when system must start listening for speech.
        /// </summary>
        public abstract void StartListening();

        /// <summary>
        /// Called when system should stop listening for speech.
        /// </summary>
        public abstract void StopListening();

        /// <summary>
        /// PlayerCharacter subscribes to this event to get the Speech-to-Text result
        /// </summary>
        public event Action<string> OnSpeechRecognized = new Action<string>(s => { });
        /// <summary>
        /// Can be used to handle speech recogintion errors
        /// </summary>
        public event Action<string> OnSpeechRecognitionFailed = new Action<string>(s => { });

        private string context = "_";
        protected void SpeechRecognized(string result)
        {
            OnSpeechRecognized?.Invoke(result);
        }

    }

}