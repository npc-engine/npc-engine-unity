using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NPCEngine.Utility;
using NPCEngine.API;


namespace NPCEngine.Components
{

    /// <summary>
    /// Script for generating audio files using text to speech.
    /// </summary>
    public class TTSGenerator : MonoBehaviour
    {
        public string savePath;
        public AudioSource audioSource;
        public string voiceId;

        public AudioClip clip;

        public string text;

        private bool generating = false;


        public bool Generating
        {
            get
            {
                return generating;
            }
        }

        private bool saving = false;

        public bool Saving
        {
            get
            {
                return saving;
            }
        }

        [ContextMenu("Reset Generate")]
        public void ResetGeneration()
        {
            generating = false;
        }

        public IEnumerator GenerateAudioCoroutine(Action generationReady = null)
        {
            generating = true;
            yield return NPCEngineManager.Instance.GetAPI<TextToSpeech>().StartTTS(voiceId, text, 1, ()=>{});
            yield return NPCEngineManager.Instance.GetAPI<TextToSpeech>().GetNextResult((raw_audio)=>{
                try{
                    clip = AudioClip.Create("generated_audio", raw_audio.Count, 1, 22050, false);
                    clip.SetData(raw_audio.ToArray(), 0);
                    audioSource.clip = clip;
                } catch(Exception e)
                {
                    Debug.LogError(e.Message);
                    
                } finally
                {
                    generating = false;
                }
            });
            if (generationReady != null)
            {
                generationReady();
            }
        }

        public IEnumerator SaveAudio()
        {
            if (clip != null)
            {
                var wav_path = Path.Combine(Application.dataPath, savePath, "generated_audio.wav");
                SavWav.Save(wav_path, clip);
                saving = true;
                //Wait until file is created and saved
                while (!File.Exists(wav_path))
                {
                    yield return CoroutineUtility.WaitForSeconds(0.1f);
                }
                saving = false;
            }

        }

    }
}