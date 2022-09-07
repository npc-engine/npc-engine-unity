using System;
using UnityEngine;

namespace NPCEngine.Components
{
    /// <summary>
    /// Character natural language descriptions.
    /// </summary>
    [CreateAssetMenu(fileName = "Character", menuName = "NPCEngine/Character")]
    public class Character: ScriptableObject
    {
        public string Name;

        /// <summary>
        /// Character's persona natural language description.
        /// </summary>
        [TextArea(3, 10)]
        public string Persona;

        /// <summary>
        /// Sampling parameter for the character's text generation. Higher temperature means more variation in the generated text.
        /// </summary>
        [Tooltip("Sampling parameter for the character's text generation. Higher temperature means more variation in the generated text.")]
        [Range(0, 2)]
        public float temperature=1.0f;

        /// <summary>
        /// Sampling parameter for the character's text generation. TopK is the number of top tokens to be sampled from.
        /// </summary>
        [Tooltip("Sampling parameter for the character's text generation. TopK is the number of top tokens to be sampled from.")]
        [Range(0, 50000)]
        public int topK=1000;

        /// <summary>
        /// Sampling parameter for the character's text generation. Number of sentences generated and selected from.
        /// </summary>
        [Tooltip("Sampling parameter for the character's text generation. Number of sentences generated and selected from.")]
        public int numSampled=3;
    }
}