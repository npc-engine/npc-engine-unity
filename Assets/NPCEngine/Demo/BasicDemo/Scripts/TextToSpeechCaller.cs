using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NPCEngine;
using NPCEngine.API;
using NPCEngine.Utility;


public class TextToSpeechCaller : MonoBehaviour
{
    public AudioSourceQueue audioQueue;

    public InputField text;
    public InputField speakerId;
    public Text availableSpeakerIds;
    public InputField nChunks;

    private ResultFuture<List<string>> speakerIdsResult;
    private ResultFuture<List<float>> textToSpeechResult;

    // Start is called before the first frame update
    void OnEnable()
    {
        speakerIdsResult = TextToSpeech.GetSpeakerIds();
    }

    // Update is called once per frame
    void Update()
    {

        if (speakerIdsResult != null && speakerIdsResult.ResultReady)
        {
            var speakerIds = String.Join(", ", speakerIdsResult.Result.ToArray());
            availableSpeakerIds.text += " " + speakerIds;
            speakerIdsResult = null;
        }

        if (textToSpeechResult != null && textToSpeechResult.ResultReady)
        {
            try
            {
                var result = textToSpeechResult.Result;
                var clip = AudioClip.Create("tmp", result.Count, 1, 22050, false);
                clip.SetData(result.ToArray(), 0);
                audioQueue.PlaySound(clip);
                textToSpeechResult = TextToSpeech.GetNextResult();
            }
            catch (NPCEngineException)
            {
                textToSpeechResult = null;
            }
        }
    }

    public void RunTextToSpeech()
    {
        TextToSpeech.StartTTS(speakerId.text, text.text, Int32.Parse(nChunks.text));
        textToSpeechResult = TextToSpeech.GetNextResult();
    }
}
