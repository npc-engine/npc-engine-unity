using System;
using UnityEngine;

namespace NPCEngine.Components
{
    [CreateAssetMenu(fileName = "Character", menuName = "NPCEngine/Character")]
    public class Character: ScriptableObject
    {
        public string Name;

        [TextArea(3, 10)]
        public string Persona;
    }
}