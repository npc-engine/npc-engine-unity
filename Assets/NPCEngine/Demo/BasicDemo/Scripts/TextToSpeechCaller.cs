using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NPCEngine;
using NPCEngine.API;
using NPCEngine.Utility;
using NPCEngine.Components;


public class TextToSpeechCaller : MonoBehaviour
{
    public AudioSourceQueue audioQueue;

    public InputField text;
    public InputField speakerId;
    public Text availableSpeakerIds;
    public InputField nChunks;

    // Start is called before the first frame update
    void OnEnable()
    {
        StartCoroutine(NPCEngineManager.Instance.GetAPI<TextToSpeech>().GetSpeakerIds((result) =>
        {
            var speakerIds = String.Join(", ", result.ToArray());
            availableSpeakerIds.text += " " + speakerIds;
        }));
    }

    public void RunTextToSpeech()
    {
        StartCoroutine(NPCEngineManager.Instance.GetAPI<TextToSpeech>().StartTTS(speakerId.text, text.text, Int32.Parse(nChunks.text), () => { 
            PlaySoundAndStartNext();
        }));
    }

    public void PlaySoundAndStartNext()
    {
        StartCoroutine(NPCEngineManager.Instance.GetAPI<TextToSpeech>().GetNextResult((result) =>
        {
            var clip = AudioClip.Create("tmp", result.Count, 1, 22050, false);
            clip.SetData(result.ToArray(), 0);
            audioQueue.PlaySound(clip);
            PlaySoundAndStartNext();
        }));
    }
}
