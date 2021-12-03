using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NPCEngine.Utility
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioSourceQueue : MonoBehaviour
    {
        public AudioSource audioSource;
        Queue<AudioClip> clipQueue;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            clipQueue = new Queue<AudioClip>();
        }

        void Update()
        {
            if (audioSource.isPlaying == false && clipQueue.Count > 0)
            {
                audioSource.clip = clipQueue.Dequeue();
                audioSource.Play();
            }
        }
        public void PlaySound(AudioClip clip)
        {
            clipQueue.Enqueue(clip);
        }
    }
}
