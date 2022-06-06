using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NPCEngine.Utility
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(AudioSource))]
    public class AudioSourceQueue : MonoBehaviour
    {
        public AudioSource audioSource;
        Queue<AudioClip> clipQueue = new Queue<AudioClip>();

        private void OnEnable()
        {
            audioSource = GetComponent<AudioSource>();
            CoroutineUtility.StartCoroutine(AudioPlayCoroutine(), this, "AudioPlayCoroutine_" + gameObject.name);
        }

        IEnumerator AudioPlayCoroutine()
        {
            while(true)
            {
                if (audioSource.isPlaying == false && clipQueue.Count > 0)
                {
                    audioSource.clip = clipQueue.Dequeue();
                    audioSource.Play();
                }
                yield return CoroutineUtility.WaitForSeconds(0.1f);
            }
        }

        public void PlaySound(AudioClip clip)
        {
            clipQueue.Enqueue(clip);
        }
    }
}
