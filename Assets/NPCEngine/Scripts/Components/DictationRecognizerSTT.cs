using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

namespace NPCEngine.Components
{

    public class DictationRecognizerSTT : AbstractSpeechToText
    {
        public bool debugLogs = false;

        public bool PolicyNotAccepted
        {
            get
            {
                return policyNotAccepted;
            }
        }

        public bool RecognizerStarted
        {
            get
            {
                return recognizerStarted;
            }
        }

        private string hypotheses = null;
        private float lastHypothesesTime;

        private bool policyNotAccepted = false;
        private bool recognizerStarted = false;

        private bool isListening = false;


        private bool enable_stt = false;
        private int hypothesisCounter = 0;
        private DictationRecognizer dictationRecognizer;

        // Start is called before the first frame update
        void Start()
        {
            isListening = true;
            if (dictationRecognizer == null)
                dictationRecognizer = new DictationRecognizer();

            dictationRecognizer.AutoSilenceTimeoutSeconds = float.PositiveInfinity;
            dictationRecognizer.InitialSilenceTimeoutSeconds = float.PositiveInfinity;

            dictationRecognizer.DictationResult += (text, confidence) =>
            {
                if (debugLogs)
                    Debug.LogFormat("Dictation result: {0}", text);
                if (isListening)
                    SpeechRecognized(text);
                hypothesisCounter = 0;
                hypotheses = null;
            };

            dictationRecognizer.DictationHypothesis += (text) =>
            {
                if (debugLogs)
                    Debug.LogFormat("Dictation hypothesis: {0}", text);
                hypothesisCounter += 1;
                hypotheses = text;
                lastHypothesesTime = Time.time;
            };

            dictationRecognizer.DictationComplete += (completionCause) =>
            {
                if (completionCause != DictationCompletionCause.Complete)
                    Debug.LogErrorFormat("Dictation completed unsuccessfully: {0}. Reason: {1}", completionCause, completionCause);

            };

            dictationRecognizer.DictationError += (error, hresult) =>
            {

                if (hresult == -2147199735)
                {
                    policyNotAccepted = true;
                }
                Debug.LogErrorFormat("Dictation error: {0}; HResult = {1}.", error, hresult);
            };
        }


        private void OnEnable()
        {
            if (dictationRecognizer == null)
                dictationRecognizer = new DictationRecognizer();
            enable_stt = true;
            StartCoroutine("DictationRecognizerHealthCheck");
        }


        private void OnDisable()
        {
            dictationRecognizer.Stop();
            enable_stt = false;
        }

        IEnumerator DictationRecognizerHealthCheck()
        {
            while (true)
            {
                if ((dictationRecognizer.Status != SpeechSystemStatus.Running) && enable_stt)
                {
                    var st = dictationRecognizer.Status;
                    dictationRecognizer.Start();
                    recognizerStarted = true;
                    Debug.LogErrorFormat("Restarting stt: before restart {0}, after restart: {1}", st, dictationRecognizer.Status);
                    hypothesisCounter = 0;
                    hypotheses = null;
                    yield return new WaitForSeconds(1);
                }
                yield return null;
            }
        }
        private void OnDestroy()
        {
            dictationRecognizer.Stop();
            dictationRecognizer.Dispose();
        }

        public override void StartListening()
        {
            isListening = true;
        }

        public override void StopListening()
        {
            isListening = false;
        }

        private void Update()
        {
            if (hypotheses != null && lastHypothesesTime + 2 < Time.time)
            {
                if (debugLogs)
                    Debug.LogFormat("Dictation result: {0}", hypotheses);
                if (isListening)
                    SpeechRecognized(hypotheses);
                hypothesisCounter = 0;
                hypotheses = null;
            }
        }

    }

}