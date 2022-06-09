using System;
using UnityEngine;

namespace NPCEngine.Components
{
    [CreateAssetMenu(fileName = "Character", menuName = "NPCEngine/Character")]
    public class Character: ScriptableObject
    {
        public string Name;
        
        public string voiceId;

        [TextArea(3, 10)]
        public string Persona;

        
        // Test properties

        [HideInInspector]
        public Location testLocation;

        [HideInInspector]
        public Character testCharacter;
    }
}