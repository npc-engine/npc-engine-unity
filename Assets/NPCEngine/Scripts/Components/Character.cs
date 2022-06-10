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
    }
}